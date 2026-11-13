// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using AgentInfrastructure;
using ContinuousLearning;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DesignConversionDomain;

public sealed class DesignConversionThinClientJobRuntimeTests
{
    private const string Flow47VerifiedKey = "flow47_verified";

    [Fact]
    public void Submit_StoresSubmittedJobAndStatusWithoutRunningInternalService()
    {
        var evaluator = new RecordingEvaluator(CreateDeterministicInternalEvaluator());
        var runtime = new DesignConversionThinClientJobRuntime(evaluator, FixedClock());

        var submitted = runtime.Submit(CreateSubmittedRequest());

        Assert.True(submitted.IsSuccess, submitted.ErrorMessage);
        Assert.Equal("thin-job-thin-request-001", submitted.Value!.JobId);
        Assert.Equal(DesignConversionThinClientStatus.Submitted, submitted.Value.Status);
        Assert.Equal("memory://design-conversion/jobs/thin-job-thin-request-001/status", submitted.Value.StatusUri);
        Assert.Equal("false", submitted.Value.Metadata["persistence_verified"]);
        Assert.Equal("false", submitted.Value.Metadata["public_api_verified"]);
        Assert.False(submitted.Value.Metadata.ContainsKey(Flow47VerifiedKey));
        Assert.Equal(DesignConversionThinClientJobRuntime.Flow47VerificationAuthority, submitted.Value.Metadata["flow47_verification_authority"]);
        Assert.Equal(DesignConversionThinClientJobRuntime.Flow47VerificationState, submitted.Value.Metadata["flow47_verification_state"]);
        Assert.Empty(evaluator.Requests);

        var status = runtime.GetStatus(submitted.Value.JobId);

        Assert.True(status.IsSuccess, status.ErrorMessage);
        Assert.Equal(DesignConversionThinClientStatus.Submitted, status.Value!.ContractStatus);
        Assert.Equal(DesignConversionThinClientStatus.Submitted, status.Value.Job.Status);
        Assert.Empty(status.Value.Artifacts);
        Assert.Contains("public_api_verified=false", status.Value.Warnings);
        Assert.Contains("runtime_ready_verified=false", status.Value.Warnings);
        Assert.Contains(
            $"flow47_verification_authority={DesignConversionThinClientJobRuntime.Flow47VerificationAuthority}",
            status.Value.Warnings);
        Assert.Contains(
            $"flow47_verification_state={DesignConversionThinClientJobRuntime.Flow47VerificationState}",
            status.Value.Warnings);
        Assert.DoesNotContain($"{Flow47VerifiedKey}=false", status.Value.Warnings);
        Assert.Equal(nameof(DesignConversionThinClientJobRuntime), status.Value.Provenance["runtime_adapter"]);
        Assert.Equal(DesignConversionThinClientJobRuntime.Flow47VerificationAuthority, status.Value.Provenance["flow47_verification_authority"]);
        Assert.Equal(DesignConversionThinClientJobRuntime.Flow47VerificationState, status.Value.Provenance["flow47_verification_state"]);
    }

    [Theory]
    [InlineData(-1, "thin-client")]
    [InlineData(0, "enterprise")]
    [InlineData(0, "")]
    public void Submit_RejectsInvalidImprovementCandidateBudgetBeforeStoringJob(
        int maxImprovementCandidates,
        string tier)
    {
        var evaluator = new RecordingEvaluator(CreateDeterministicInternalEvaluator());
        var runtime = new DesignConversionThinClientJobRuntime(evaluator, FixedClock());
        var request = CreatePrivateSubmittedRequest();
        request = request with
        {
            Budget = request.Budget with
            {
                UserId = "private-user-123",
                Tier = tier,
                MaxImprovementCandidates = maxImprovementCandidates
            }
        };
        var expectedJobId = "thin-job-thin-request-001";

        var submitted = runtime.Submit(request);
        var status = runtime.GetStatus(expectedJobId);

        Assert.False(submitted.IsSuccess);
        Assert.Equal(DesignConversionInternalService.ImprovementCandidateBudgetRequiredCode, submitted.ErrorCode);
        Assert.Equal("DesignConversion improvement candidate budget must be greater than zero.", submitted.ErrorMessage);
        Assert.Empty(evaluator.Requests);
        Assert.False(status.IsSuccess);
        Assert.Equal("JOB_NOT_FOUND", status.ErrorCode);
        Assert.DoesNotContain(request.Budget.UserId, submitted.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(request.Html, submitted.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(request.Css, submitted.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(request.DesignTreePayload.Content, submitted.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(request.OriginalTreePayload.Content, submitted.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Submit_TrustedServerOwnedNegativeBudgetUsdFailsBeforeStoringJobOrEffects()
    {
        const string requestId = "thin-trusted-negative-budget-usd";
        var events = new List<string>();
        var evaluator = new EventRecordingEvaluator(CreateDeterministicInternalEvaluator(), events);
        var ledger = new RecordingVisualCostLedger(
            new JsonlDesignConversionVisualCostLedger(
                CreateBudgetStoreTempRoot("trusted-negative-budget-ledger")));
        var artifactDb = new RecordingLocalArtifactDb(
            new JsonlDesignConversionLocalArtifactDb(
                CreateBudgetStoreTempRoot("trusted-negative-budget-artifact")));
        var store = new EventRecordingVisualCostBudgetStore(
            new JsonlDesignConversionVisualCostBudgetStore(
                CreateBudgetStoreTempRoot("trusted-negative-budget-store")),
            events);
        var runtime = new DesignConversionThinClientJobRuntime(
            evaluator,
            FixedClock(),
            artifactDb,
            ledger,
            store);
        var trustedRequest = WithTrustedVisualCostBudget(
            CreateSubmittedRequest() with
            {
                RequestId = requestId
            },
            monthlyLimitUsd: "1",
            budgetUsd: -0.01m,
            tier: "free");
        var request = trustedRequest with
        {
            Budget = trustedRequest.Budget with
            {
                MaxImprovementCandidates = 0
            }
        };
        var expectedJobId = "thin-job-" + requestId;

        var submitted = runtime.Submit(request);
        var status = runtime.GetStatus(expectedJobId);
        var period = await store.Inner.ReadPeriodAsync(BuildTrustedBudgetPeriodQuery(request));

        Assert.False(submitted.IsSuccess);
        Assert.Equal(DesignConversionInternalService.NegativeBudgetUsdErrorCode, submitted.ErrorCode);
        Assert.Equal(
            "DesignConversion improvement-loop budgetUsd must be greater than or equal to zero.",
            submitted.ErrorMessage);
        Assert.Equal(-0.01m, request.Budget.BudgetUsd);
        Assert.Empty(events);
        Assert.Empty(evaluator.Requests);
        Assert.Empty(ledger.AttemptedRecords);
        Assert.Empty(artifactDb.AttemptedRecords);
        Assert.Equal(0, artifactDb.ListCalls);
        Assert.False(status.IsSuccess);
        Assert.Equal("JOB_NOT_FOUND", status.ErrorCode);
        Assert.True(period.IsSuccess, period.ErrorMessage);
        Assert.Equal(0, period.Value!.ReservationCount);
        Assert.Equal(0, period.Value.ActiveReservationCount);
        Assert.Equal(0, period.Value.SettledReservationCount);
        Assert.Equal(0, period.Value.ReleasedReservationCount);
        Assert.Equal(0, period.Value.DeniedReservationCount);
        Assert.Equal(0m, period.Value.ActiveReservedUsd);
        Assert.Equal(0m, period.Value.FinalizedSpendUsd);
        Assert.Empty(period.Value.Reservations);
        Assert.Equal(0, runtime.LiveRunLeaseCount);
    }

    [Theory]
    [InlineData(51)]
    [InlineData(int.MaxValue)]
    public void Submit_RejectsImprovementCandidateBudgetAboveHardCapBeforeStoringJob(int maxImprovementCandidates)
    {
        var evaluator = new RecordingEvaluator(CreateDeterministicInternalEvaluator());
        var runtime = new DesignConversionThinClientJobRuntime(evaluator, FixedClock());
        var request = CreateSubmittedRequest() with
        {
            Budget = CreateSubmittedRequest().Budget with
            {
                Tier = "free",
                MaxImprovementCandidates = maxImprovementCandidates
            }
        };

        var submitted = runtime.Submit(request);

        Assert.False(submitted.IsSuccess);
        Assert.Equal(DesignConversionImprovementLoopThinClientJobEvaluator.MaxAttemptsExceededErrorCode, submitted.ErrorCode);
        Assert.Equal(
            $"DesignConversion improvement-loop max attempts must be less than or equal to {DesignConversionImprovementLoopCoordinator.MaxAllowedAttempts}.",
            submitted.ErrorMessage);
        Assert.Empty(evaluator.Requests);
        Assert.False(runtime.GetStatus("thin-job-thin-request-001").IsSuccess);
    }

    [Fact]
    public void Submit_RejectsNegativeMaxTeacherCallsBeforeStoringJobOrInvokingEvaluator()
    {
        var evaluator = new RecordingEvaluator(CreateDeterministicInternalEvaluator());
        var runtime = new DesignConversionThinClientJobRuntime(evaluator, FixedClock());
        var request = CreateSubmittedRequest() with
        {
            Budget = CreateSubmittedRequest().Budget with
            {
                MaxTeacherCalls = -1
            }
        };

        var submitted = runtime.Submit(request);
        var status = runtime.GetStatus("thin-job-thin-request-001");

        Assert.False(submitted.IsSuccess);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.MaxTeacherCallsInvalidErrorCode, submitted.ErrorCode);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.MaxTeacherCallsInvalidErrorMessage(), submitted.ErrorMessage);
        Assert.Empty(evaluator.Requests);
        Assert.False(status.IsSuccess);
        Assert.Equal("JOB_NOT_FOUND", status.ErrorCode);
    }

    [Theory]
    [InlineData(-1, "free", "IMPROVEMENT_CANDIDATE_BUDGET_REQUIRED")]
    [InlineData(0, "enterprise", "IMPROVEMENT_CANDIDATE_BUDGET_REQUIRED")]
    [InlineData(51, "free", "IMPROVEMENT_LOOP_MAX_ATTEMPTS_EXCEEDED")]
    [InlineData(int.MaxValue, "free", "IMPROVEMENT_LOOP_MAX_ATTEMPTS_EXCEEDED")]
    public async Task ImprovementLoopEvaluator_RejectsInvalidRawMaxAttemptsBeforeInnerEvaluator(
        int maxImprovementCandidates,
        string tier,
        string expectedErrorCode)
    {
        var mapper = new RecordingEvaluator(CreateDeterministicInternalEvaluator());
        var mappingRuntime = new DesignConversionThinClientJobRuntime(mapper, FixedClock());
        var submitted = mappingRuntime.Submit(CreateSubmittedRequest());
        Assert.True(submitted.IsSuccess, submitted.ErrorMessage);
        var mappedRun = await mappingRuntime.RunAsync(submitted.Value!.JobId);
        Assert.True(mappedRun.IsSuccess, mappedRun.ErrorMessage);
        var mapped = Assert.Single(mapper.Requests);

        var inner = new RecordingEvaluator(CreateDeterministicInternalEvaluator());
        var evaluator = new DesignConversionImprovementLoopThinClientJobEvaluator(inner);
        var request = mapped with
        {
            Budget = mapped.Budget with
            {
                Tier = tier,
                MaxImprovementCandidates = maxImprovementCandidates
            }
        };

        var result = await evaluator.EvaluateAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal(expectedErrorCode, result.ErrorCode);
        Assert.Empty(inner.Requests);
    }

    [Fact]
    public async Task ImprovementLoopEvaluator_RejectsNegativeBudgetUsdBeforeInnerEvaluator()
    {
        var mapper = new RecordingEvaluator(CreateDeterministicInternalEvaluator());
        var mappingRuntime = new DesignConversionThinClientJobRuntime(mapper, FixedClock());
        var submitted = mappingRuntime.Submit(CreateSubmittedRequest());
        Assert.True(submitted.IsSuccess, submitted.ErrorMessage);
        var mappedRun = await mappingRuntime.RunAsync(submitted.Value!.JobId);
        Assert.True(mappedRun.IsSuccess, mappedRun.ErrorMessage);
        var mapped = Assert.Single(mapper.Requests);

        var inner = new RecordingEvaluator(CreateDeterministicInternalEvaluator());
        var evaluator = new DesignConversionImprovementLoopThinClientJobEvaluator(inner);
        var request = mapped with
        {
            Budget = mapped.Budget with
            {
                Tier = "free",
                MaxImprovementCandidates = 0,
                BudgetUsd = -0.01m
            }
        };

        var result = await evaluator.EvaluateAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal(DesignConversionInternalService.NegativeBudgetUsdErrorCode, result.ErrorCode);
        Assert.Equal(
            "DesignConversion improvement-loop budgetUsd must be greater than or equal to zero.",
            result.ErrorMessage);
        Assert.Empty(inner.Requests);
    }

    [Theory]
    [InlineData("free", 1)]
    [InlineData("paid", 2)]
    [InlineData("pro", 4)]
    public async Task SubmitAndRun_ServerOwnedTierZeroBudgetPreservesRawMaxCandidates(
        string tier,
        int expectedEffectiveMaxAttempts)
    {
        var evaluator = new RecordingEvaluator(CreateDeterministicInternalEvaluator());
        var ledgerRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "thin-client-runtime-tier-budget-ledger");
        var runtime = new DesignConversionThinClientJobRuntime(
            evaluator,
            FixedClock(),
            visualCostLedger: new JsonlDesignConversionVisualCostLedger(ledgerRoot),
            postEvaluationStateStore: CreatePostEvaluationStateStore("tier-budget"));
        var request = CreateSubmittedRequest() with
        {
            Budget = CreateSubmittedRequest().Budget with
            {
                Tier = tier,
                MaxImprovementCandidates = 0,
                BudgetUsd = 0m
            }
        };

        var submitted = runtime.Submit(request);
        Assert.True(submitted.IsSuccess, submitted.ErrorMessage);

        var result = await runtime.RunAsync(submitted.Value!.JobId);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        AssertThinClientNonReadyVisualDecision(
            result.Value!,
            DesignConversionVisualQaBoundaryPolicy.DecisionBlocked,
            DesignConversionVisualQaBoundaryPolicy.ProviderRequired);
        var mapped = Assert.Single(evaluator.Requests);
        Assert.Equal(0, mapped.Budget.MaxImprovementCandidates);
        Assert.Equal(
            expectedEffectiveMaxAttempts,
            DesignConversionVisualQaBoundaryPolicy.ResolveEffectiveTierMaxImprovementCandidates(
                mapped.Budget.MaxImprovementCandidates,
                mapped.Budget.Tier));
    }

    [Fact]
    public async Task RunAsync_MapsSubmittedRequestToInternalServiceAndReturnsNotReadyWithoutVisualJudge()
    {
        var evaluator = new RecordingEvaluator(CreateDeterministicInternalEvaluator());
        var runtime = new DesignConversionThinClientJobRuntime(
            evaluator,
            FixedClock(),
            visualCostLedger: new JsonlDesignConversionVisualCostLedger(
                CreateBudgetStoreTempRoot("submitted-request-mapping-ledger")),
            postEvaluationStateStore: CreatePostEvaluationStateStore("submitted-request-mapping"));
        var submitted = runtime.Submit(CreateSubmittedRequest());
        Assert.True(submitted.IsSuccess, submitted.ErrorMessage);

        var result = await runtime.RunAsync(submitted.Value!.JobId);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        AssertThinClientNonReadyVisualDecision(
            result.Value!,
            DesignConversionVisualQaBoundaryPolicy.DecisionBlocked,
            DesignConversionVisualQaBoundaryPolicy.ProviderRequired);
        Assert.Equal("false", result.Value.Job.Metadata["product_ready_verified"]);
        Assert.Equal("false", result.Value.Job.Metadata["runtime_ready_verified"]);
        Assert.False(result.Value.Job.Metadata.ContainsKey(Flow47VerifiedKey));
        Assert.Equal(DesignConversionThinClientJobRuntime.Flow47VerificationAuthority, result.Value.Job.Metadata["flow47_verification_authority"]);
        Assert.Equal(DesignConversionThinClientJobRuntime.Flow47VerificationState, result.Value.Job.Metadata["flow47_verification_state"]);
        Assert.Contains(result.Value.Artifacts, artifact => artifact.Kind == "improved-html" && artifact.MediaType == "text/html");
        Assert.Contains(result.Value.Artifacts, artifact => artifact.Kind == "improved-css" && artifact.MediaType == "text/css");
        Assert.Contains("DETERMINISTIC_RENDER_SCAFFOLD_NOT_BROWSER_SCREENSHOT", result.Value.Warnings);
        Assert.Contains("public_api_verified=false", result.Value.Warnings);
        Assert.Contains("trained_feature_verified=false", result.Value.Warnings);
        Assert.DoesNotContain($"{Flow47VerifiedKey}=false", result.Value.Warnings);
        Assert.Equal("false", result.Value.Provenance["deterministic_logic_claimed_as_learning"]);

        Assert.Single(evaluator.Requests);
        var mapped = evaluator.Requests[0];
        Assert.Equal(submitted.Value.JobId, mapped.JobId);
        Assert.Equal("thin-request-001", mapped.RequestId);
        Assert.Equal(DesignConversionSourceKind.Figma, mapped.SourceKind);
        Assert.Equal("<main class=\"frame\">Design</main>", mapped.Html);
        Assert.Equal(".frame{display:grid;gap:8px;}", mapped.Css);
        Assert.Equal(
            ["source-1440x900", DesignConversionRenderResolutionFactory.ServerOwnedMobileResolutionId],
            mapped.RenderResolutions.Select(resolution => resolution.ResolutionId).ToArray());
        Assert.Equal("source-1440x900", mapped.RenderResolutions[0].ResolutionId);
        Assert.Equal(1440, mapped.RenderResolutions[0].Width);
        Assert.Equal(900, mapped.RenderResolutions[0].Height);
        Assert.Equal(1.0, mapped.RenderResolutions[0].DeviceScaleFactor);
        Assert.Equal(390, mapped.RenderResolutions[1].Width);
        Assert.Equal(844, mapped.RenderResolutions[1].Height);
        Assert.Equal(1.0, mapped.RenderResolutions[1].DeviceScaleFactor);
        Assert.Equal(DesignConversionThinClientJobRuntime.RuntimeBoundary, mapped.DeterministicEvidenceRefs["thin_client_job_runtime"]);
        Assert.Equal(DesignConversionThinClientJobRuntime.StorageBoundary, mapped.DeterministicEvidenceRefs["status_store"]);
        Assert.Equal(nameof(DesignConversionThinClientJobRuntime), mapped.Provenance["submission_mapper"]);
        Assert.Equal(submitted.Value.JobId, mapped.Provenance["thin_client_job_id"]);

        var stored = runtime.GetStatus(submitted.Value.JobId);
        Assert.True(stored.IsSuccess, stored.ErrorMessage);
        Assert.Equal(DesignConversionThinClientStatus.NotReady, stored.Value!.ContractStatus);
        Assert.Equal(DesignConversionThinClientStatus.NotReady, stored.Value.Job.Status);
        Assert.Equal(result.Value.Artifacts.Select(artifact => artifact.ArtifactId), stored.Value.Artifacts.Select(artifact => artifact.ArtifactId));
    }

    [Fact]
    public async Task RunAsync_UsesScreenshotDeviceScaleFactorWhenPairedDesignRenderOmitsIt()
    {
        var evaluator = new RecordingEvaluator(CreateDeterministicInternalEvaluator());
        var runtime = new DesignConversionThinClientJobRuntime(evaluator, FixedClock());
        var request = CreateSubmittedRequest();
        request = request with
        {
            Screenshots = [WithDeviceScaleFactor(request.Screenshots[0], "2.5")]
        };
        var submitted = runtime.Submit(request);
        Assert.True(submitted.IsSuccess, submitted.ErrorMessage);

        var result = await runtime.RunAsync(submitted.Value!.JobId);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var resolutions = Assert.Single(evaluator.Requests).RenderResolutions;
        var mapped = Assert.Single(resolutions, resolution => resolution.ResolutionId == "source-1440x900@2.5");
        var complement = Assert.Single(
            resolutions,
            resolution => resolution.ResolutionId == DesignConversionRenderResolutionFactory.ServerOwnedMobileResolutionId);
        Assert.Equal(1440, mapped.Width);
        Assert.Equal(900, mapped.Height);
        Assert.Equal(2.5, mapped.DeviceScaleFactor);
        Assert.Equal(390, complement.Width);
        Assert.Equal(844, complement.Height);
        Assert.Equal(1.0, complement.DeviceScaleFactor);
    }

    [Fact]
    public async Task RunAsync_DeduplicatesSameViewportWithMatchingDeviceScaleFactors()
    {
        var evaluator = new RecordingEvaluator(CreateDeterministicInternalEvaluator());
        var runtime = new DesignConversionThinClientJobRuntime(evaluator, FixedClock());
        var request = CreateSubmittedRequest() with
        {
            Screenshots = [WithDeviceScaleFactor(CreateSubmittedRequest().Screenshots[0], "2.5")],
            DesignRenders = [WithDeviceScaleFactor(CreateSubmittedRequest().DesignRenders[0], "2.50")]
        };
        var submitted = runtime.Submit(request);
        Assert.True(submitted.IsSuccess, submitted.ErrorMessage);

        var result = await runtime.RunAsync(submitted.Value!.JobId);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var resolutions = Assert.Single(evaluator.Requests).RenderResolutions;
        var mapped = Assert.Single(resolutions, resolution => resolution.ResolutionId == "source-1440x900@2.5");
        Assert.Contains(
            resolutions,
            resolution => resolution.ResolutionId == DesignConversionRenderResolutionFactory.ServerOwnedMobileResolutionId &&
                          resolution.Width == 390 &&
                          resolution.Height == 844 &&
                          resolution.DeviceScaleFactor == 1.0);
        Assert.Equal(2.5, mapped.DeviceScaleFactor);
    }

    [Fact]
    public async Task RunAsync_DoesNotAugmentExplicitDesktopAndMobileSourceViewports()
    {
        var evaluator = new RecordingEvaluator(CreateDeterministicInternalEvaluator());
        var runtime = new DesignConversionThinClientJobRuntime(evaluator, FixedClock());
        var request = CreateSubmittedRequest();
        var mobileScreenshot = WithDeviceScaleFactor(
            new DesignRenderArtifact(
                ArtifactId: "screenshot-mobile",
                Kind: "screenshot",
                Width: 390,
                Height: 844,
                Uri: "artifact://screenshot-mobile.png",
                Sha256: "sha256:screenshot-mobile",
                Metadata: new Dictionary<string, string>()),
            "3");
        var mobileDesignRender = new DesignRenderArtifact(
            ArtifactId: "render-mobile",
            Kind: "design-render",
            Width: 390,
            Height: 844,
            Uri: "artifact://render-mobile.png",
            Sha256: "sha256:render-mobile",
            Metadata: new Dictionary<string, string>());
        request = request with
        {
            Screenshots = [request.Screenshots[0], mobileScreenshot],
            DesignRenders = [request.DesignRenders[0], mobileDesignRender],
            Budget = request.Budget with { MaxRenderQaRuns = 2 }
        };
        var submitted = runtime.Submit(request);
        Assert.True(submitted.IsSuccess, submitted.ErrorMessage);

        var result = await runtime.RunAsync(submitted.Value!.JobId);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var resolutions = Assert.Single(evaluator.Requests).RenderResolutions;
        Assert.Equal(
            ["source-1440x900", "source-390x844@3"],
            resolutions.Select(resolution => resolution.ResolutionId).ToArray());
        Assert.Equal(3.0, resolutions[1].DeviceScaleFactor);
        Assert.DoesNotContain(
            resolutions,
            resolution => resolution.ResolutionId.StartsWith("server-owned-", StringComparison.Ordinal));
    }

    [Fact]
    public void Submit_RejectsConflictingDeviceScaleFactorsForSameViewportBeforeEvaluatorExecution()
    {
        var evaluator = new RecordingEvaluator(CreateDeterministicInternalEvaluator());
        var runtime = new DesignConversionThinClientJobRuntime(evaluator, FixedClock());
        var request = CreateSubmittedRequest();
        request = request with
        {
            Screenshots = [WithDeviceScaleFactor(request.Screenshots[0], "2")],
            DesignRenders = [WithDeviceScaleFactor(request.DesignRenders[0], "3")]
        };

        var submitted = runtime.Submit(request);

        Assert.False(submitted.IsSuccess);
        Assert.Equal(DesignConversionRenderResolutionFactory.ConflictingDeviceScaleFactorErrorCode, submitted.ErrorCode);
        Assert.Equal(
            "DesignConversion render artifacts for one logical viewport specify conflicting device scale factors.",
            submitted.ErrorMessage);
        Assert.Empty(evaluator.Requests);
        Assert.Equal("JOB_NOT_FOUND", runtime.GetStatus("thin-job-thin-request-001").ErrorCode);
    }

    [Theory]
    [InlineData("not-a-number")]
    [InlineData("NaN")]
    [InlineData("Infinity")]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("1000000000")]
    public void Submit_RejectsInvalidExplicitDeviceScaleFactorBeforeEvaluatorExecution(string rawDeviceScaleFactor)
    {
        var evaluator = new RecordingEvaluator(CreateDeterministicInternalEvaluator());
        var runtime = new DesignConversionThinClientJobRuntime(evaluator, FixedClock());
        var request = CreateSubmittedRequest();
        request = request with
        {
            Screenshots = [WithDeviceScaleFactor(request.Screenshots[0], rawDeviceScaleFactor)]
        };

        var submitted = runtime.Submit(request);

        Assert.False(submitted.IsSuccess);
        Assert.Equal(DesignConversionRenderResolutionFactory.InvalidDeviceScaleFactorErrorCode, submitted.ErrorCode);
        Assert.Equal("DesignConversion render viewport device scale factor is invalid.", submitted.ErrorMessage);
        Assert.DoesNotContain(rawDeviceScaleFactor, submitted.ErrorMessage, StringComparison.Ordinal);
        Assert.Empty(evaluator.Requests);
        Assert.Equal("JOB_NOT_FOUND", runtime.GetStatus("thin-job-thin-request-001").ErrorCode);
    }

    [Fact]
    public async Task RunAsync_WithLearnedMachineRoutingFlowAppliesCheckpointWeightsToRuntimeEvidence()
    {
        var baselineStateRoot = Path.Combine(
            Path.GetTempPath(),
            $"thin-client-baseline-learned-weight-state-{Guid.NewGuid():N}");
        var learnedStateRoot = Path.Combine(
            Path.GetTempPath(),
            $"thin-client-learned-weight-state-{Guid.NewGuid():N}");
        try
        {
            var baselineEvaluator = new RecordingEvaluator(CreateDeterministicInternalEvaluator());
            var baselineRuntime = new DesignConversionThinClientJobRuntime(
                baselineEvaluator,
                FixedClock(),
                postEvaluationStateStore: new JsonDesignConversionPostEvaluationStateStore(baselineStateRoot));
            var baselineSubmitted = baselineRuntime.Submit(CreatePrivateSubmittedRequest() with
            {
                RequestId = "thin-request-baseline-learned-weight-comparison"
            });
            Assert.True(baselineSubmitted.IsSuccess, baselineSubmitted.ErrorMessage);
            var baselineStatus = await baselineRuntime.RunAsync(baselineSubmitted.Value!.JobId);
            Assert.True(baselineStatus.IsSuccess, baselineStatus.ErrorMessage);

            var learnedMachine = new DesignConversionMachine();
            var import = learnedMachine.ImportCheckpoint(CreateRuntimeLearnedInferenceCheckpoint());
            Assert.True(import.IsSuccess, import.ErrorMessage);
            var learnedService = new DesignConversionInternalService(
                new DeterministicDesignConversionRenderProvider(),
                new DeterministicDesignConversionComparisonProvider(),
                learnedMachine.CreateRuntimeRoutingFlow());
            var learnedEvaluator = new RecordingEvaluator(new DesignConversionInternalServiceThinClientJobEvaluator(learnedService));
            var learnedRuntime = new DesignConversionThinClientJobRuntime(
                learnedEvaluator,
                FixedClock(),
                postEvaluationStateStore: new JsonDesignConversionPostEvaluationStateStore(learnedStateRoot));
            var learnedSubmitted = learnedRuntime.Submit(CreatePrivateSubmittedRequest());
            Assert.True(learnedSubmitted.IsSuccess, learnedSubmitted.ErrorMessage);
            var learnedStatus = await learnedRuntime.RunAsync(learnedSubmitted.Value!.JobId);
            Assert.True(learnedStatus.IsSuccess, learnedStatus.ErrorMessage);
            Assert.NotEqual(DesignConversionThinClientStatus.Failed, baselineStatus.Value!.ContractStatus);
            Assert.NotEqual(DesignConversionThinClientStatus.Failed, learnedStatus.Value!.ContractStatus);

            var baselineEvaluation = Assert.Single(baselineEvaluator.Responses).Value!;
            var learnedEvaluation = Assert.Single(learnedEvaluator.Responses).Value!;
            var baselineScore = baselineEvaluation.RuntimeFlowResult.CriticOutput.Verdict.Score;
            var learnedScore = learnedEvaluation.RuntimeFlowResult.CriticOutput.Verdict.Score;

            Assert.NotEqual(baselineScore, learnedScore);
            Assert.True(learnedScore > baselineScore);
            Assert.DoesNotContain(DesignConversionMachine.LearnedInferenceAppliedEvidence, baselineEvaluation.RuntimeFlowResult.EvidenceRefs);
            Assert.Contains(DesignConversionMachine.LearnedInferenceAppliedEvidence, learnedEvaluation.RuntimeFlowResult.EvidenceRefs);
            Assert.Contains(DesignConversionMachine.LearnedInferenceCriticAdjustmentEvidence, learnedEvaluation.RuntimeFlowResult.CriticOutput.Verdict.EvidenceRefs);
            Assert.Contains(DesignConversionMachine.LearnedInferenceImproverEvidence, learnedEvaluation.RuntimeFlowResult.Candidate.EvidenceRefs);
            Assert.Equal(bool.TrueString, learnedEvaluation.RuntimeFlowResult.Candidate.Metadata["LearnedInferenceApplied"]);
            Assert.True(int.Parse(learnedEvaluation.RuntimeFlowResult.Candidate.Metadata["LearnedFeatureContributionCount"]) > 0);
            Assert.False(learnedEvaluation.DeterministicLogicClaimedAsLearning);
            Assert.Equal("false", learnedStatus.Value!.Provenance["deterministic_logic_claimed_as_learning"]);
            Assert.Contains("model_training_verified=false", learnedStatus.Value.Warnings);
            Assert.Contains("deployment_verified=false", learnedStatus.Value.Warnings);

            var learnedEvidenceText = string.Join(
                "|",
                learnedEvaluation.RuntimeFlowResult.EvidenceRefs
                    .Concat(learnedEvaluation.RuntimeFlowResult.CriticOutput.Verdict.EvidenceRefs)
                    .Concat(learnedEvaluation.RuntimeFlowResult.Candidate.EvidenceRefs)
                    .Concat(learnedEvaluation.RuntimeFlowResult.Candidate.Metadata.SelectMany(pair => new[] { pair.Key, pair.Value }))
                    .Concat(learnedEvaluation.RuntimeFlowResult.Candidate.ChangeReasons));
            Assert.Contains("LearnedInferenceApplied", learnedEvidenceText, StringComparison.Ordinal);
            Assert.Contains("learned-inference:critic", learnedEvidenceText, StringComparison.Ordinal);
            Assert.DoesNotContain("<main", learnedEvidenceText, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(".private-frame", learnedEvidenceText, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("raw-tree-private", learnedEvidenceText, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("C:/Users/lubao", learnedEvidenceText, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("exact_prompt", learnedEvidenceText, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("private.fig", learnedEvidenceText, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("external-provider-payload", learnedEvidenceText, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("ModelUploadReady", learnedEvidenceText, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (Directory.Exists(baselineStateRoot))
                Directory.Delete(baselineStateRoot, recursive: true);
            if (Directory.Exists(learnedStateRoot))
                Directory.Delete(learnedStateRoot, recursive: true);
        }
    }

    [Fact]
    public async Task RunAsync_EligibleDpoWithoutArtifactDbOrTeacherSkipsCandidateAndCompletes()
    {
        var evaluator = new RecordingEvaluator(CreateMeasuredRuntimeCandidateEligibleEvaluator());
        var rejectedSource = CreateValidRuntimeRejectedEvaluationSource();
        var runtime = new DesignConversionThinClientJobRuntime(
            evaluator,
            FixedClock(),
            artifactDb: null,
            visualCostLedger: new JsonlDesignConversionVisualCostLedger(
                CreateBudgetStoreTempRoot("eligible-no-artifact-ledger")),
            postEvaluationStateStore: CreatePostEvaluationStateStore("eligible-no-artifact"),
            rejectedEvaluationSource: rejectedSource);
        var submitted = runtime.Submit(CreateRuntimeCandidateEligiblePrivateSubmittedRequest());
        Assert.True(submitted.IsSuccess, submitted.ErrorMessage);

        var result = await runtime.RunAsync(submitted.Value!.JobId);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionThinClientStatus.Completed, result.Value!.ContractStatus);
        Assert.Equal(DesignConversionThinClientStatus.Completed, result.Value.Job.Status);
        Assert.Equal(1, rejectedSource.CallCount);
        Assert.Equal(submitted.Value.JobId, rejectedSource.Lookup?.ChosenJobId);
        Assert.Contains(
            "visual_cost_ledger_training_artifact_db=false",
            result.Value.Warnings);
        Assert.DoesNotContain(
            "runtime_teacher_authored_candidate=True",
            result.Value.Warnings);
        Assert.DoesNotContain(
            "training_candidate_only=true",
            result.Value.Warnings);
    }

    [Fact]
    public async Task RunAsync_WithConfiguredLocalArtifactDbPersistsSanitizedCompletedEvaluation()
    {
        const string quarantineReasonPrefix = "runtime_created_dpo_candidate_quarantine_reason=";
        const string expectedQuarantineReason =
            quarantineReasonPrefix + "server_owned_route_admission_not_bound";
        const string fallbackQuarantineReason =
            quarantineReasonPrefix + "runtime_rejected_candidate_independent_evaluation_required";
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "thin-client-runtime-artifact-db");
        var db = new JsonlDesignConversionLocalArtifactDb(root);
        var evaluator = new RecordingEvaluator(CreateRuntimeCandidateEligibleInternalEvaluator());
        var runtime = new DesignConversionThinClientJobRuntime(evaluator, FixedClock(), db);
        var request = CreateRuntimeCandidateEligiblePrivateSubmittedRequest();
        var submitted = runtime.Submit(request);
        Assert.True(submitted.IsSuccess, submitted.ErrorMessage);

        var result = await runtime.RunAsync(submitted.Value!.JobId);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionThinClientStatus.Completed, result.Value!.ContractStatus);
        var evaluation = Assert.Single(evaluator.Responses).Value!;
        Assert.Equal(2, evaluation.VisualQaBoundaryTrace.MultiResolutionRender.RequiredViewportIds.Count);
        Assert.Equal(2, evaluation.VisualQaBoundaryTrace.MultiResolutionRender.ObservedViewportIds.Count);
        Assert.Empty(evaluation.VisualQaBoundaryTrace.MultiResolutionRender.MissingViewportIds);
        Assert.True(evaluation.VisualQaBoundaryTrace.MultiResolutionRender.IsComplete);
        Assert.True(
            evaluation.VisualQaBoundaryTrace.VisualDecision.CanPersistTrainingCandidate,
            JsonSerializer.Serialize(evaluation.VisualQaBoundaryTrace, DesignConversionCheckpointJsonOptions.Standard));
        Assert.Equal("true", result.Value.Job.Metadata["local_artifact_db_persistence_verified"]);
        Assert.Equal("false", result.Value.Provenance["public_api_verified"]);
        Assert.Equal("false", result.Value.Provenance["production_db_ready"]);
        Assert.Equal("false", result.Value.Provenance["trained_model_ready"]);
        Assert.Contains("local_artifact_db_persistence_verified=true", result.Value.Warnings);
        Assert.Contains("production_db_ready=false", result.Value.Warnings);
        Assert.Contains("trained_model_ready=false", result.Value.Warnings);

        var listed = await db.ListRecordsAsync(new DesignConversionLocalArtifactDbQuery(JobId: submitted.Value.JobId));
        Assert.True(listed.IsSuccess, listed.ErrorMessage);
        var stored = Assert.Single(listed.Value!);
        Assert.Equal(JsonlDesignConversionLocalArtifactDb.LocalArtifactDbKind, stored.DatabaseKind);
        Assert.Equal(submitted.Value.JobId, stored.JobId);
        Assert.Equal("thin-request-001", stored.RequestId);
        Assert.StartsWith("dcm-html_result-", stored.ConversionResult.HtmlRef, StringComparison.Ordinal);
        Assert.StartsWith("sha256:", stored.ConversionResult.HtmlSha256, StringComparison.Ordinal);
        Assert.StartsWith("dcm-css_result-", stored.ConversionResult.CssRef, StringComparison.Ordinal);
        Assert.StartsWith("sha256:", stored.ConversionResult.CssSha256, StringComparison.Ordinal);
        Assert.NotEmpty(stored.RenderArtifactRefs);
        Assert.All(stored.RenderArtifactRefs, artifact =>
        {
            Assert.NotEmpty(artifact.UriRef);
            Assert.DoesNotContain("C:/Users/lubao", artifact.UriRef, StringComparison.OrdinalIgnoreCase);
        });
        Assert.Equal(bool.TrueString, stored.Metadata["local_artifact_db"]);
        Assert.Equal(bool.TrueString, stored.Metadata["storage_training_artifact_db"]);
        Assert.Equal(bool.TrueString, stored.Metadata["product_not_ready"]);
        Assert.Equal(bool.FalseString, stored.Metadata["production_db_ready"]);
        Assert.Equal(bool.FalseString, stored.Metadata["training_candidate_only"]);
        Assert.Equal(bool.FalseString, stored.Metadata["runtime_created_dpo_candidate"]);
        Assert.Equal("0", stored.Metadata["runtime_created_dpo_candidate_count"]);
        Assert.Equal("not-created", stored.Metadata["runtime_created_dpo_candidate_review_status"]);
        Assert.Equal(bool.FalseString, stored.Metadata["review_required"]);
        Assert.Equal("not-required", stored.Metadata["approval_status"]);
        Assert.Equal(bool.FalseString, stored.Metadata["approved_for_training"]);
        Assert.Equal(bool.FalseString, stored.Metadata["production_dpo"]);
        Assert.Equal(bool.FalseString, stored.Metadata["model_readiness_claimed"]);
        Assert.Equal(bool.TrueString, stored.Metadata["high_quality_negative_pair_not_proven"]);
        Assert.Equal(bool.TrueString, stored.Metadata["held_out_not_proven"]);
        Assert.Equal(bool.TrueString, stored.Metadata["source_disjoint_not_proven"]);
        Assert.Equal(bool.FalseString, stored.Metadata["raw_html_included"]);
        Assert.Equal(bool.FalseString, stored.Metadata["raw_css_included"]);
        Assert.Contains(JsonlDesignConversionLocalArtifactDb.ConversionResultMarker, stored.EvidenceRefs);
        Assert.Contains(JsonlDesignConversionLocalArtifactDb.RenderArtifactRefsMarker, stored.EvidenceRefs);
        Assert.Contains(JsonlDesignConversionLocalArtifactDb.VisualJudgeStatusMarker, stored.EvidenceRefs);
        Assert.Contains(JsonlDesignConversionLocalArtifactDb.ProductNotReadyMarker, stored.EvidenceRefs);
        Assert.Contains(JsonlDesignConversionLocalArtifactDb.TrainingCandidateOnlyMarker, stored.EvidenceRefs);
        Assert.DoesNotContain(
            fallbackQuarantineReason,
            stored.Metadata["runtime_created_dpo_candidate_warnings"],
            StringComparison.Ordinal);
        Assert.Contains(
            expectedQuarantineReason,
            stored.Metadata["runtime_created_dpo_candidate_warnings"],
            StringComparison.Ordinal);
        Assert.Equal(
            1,
            stored.Metadata["runtime_created_dpo_candidate_warnings"]
                .Split('|', StringSplitOptions.RemoveEmptyEntries)
                .Count(warning => warning.StartsWith(
                    quarantineReasonPrefix,
                    StringComparison.Ordinal)));
        Assert.Contains(
            "runtime_rejected_evaluation_source_unavailable",
            stored.Metadata["runtime_created_dpo_candidate_warnings"]);
        Assert.Contains("runtime_created_dpo_candidate=false", stored.Metadata["runtime_created_dpo_candidate_warnings"]);
        Assert.DoesNotContain("runtime_rejected_independently_evaluated=true", stored.Metadata["runtime_created_dpo_candidate_warnings"]);
        Assert.Empty(stored.DpoCandidates);
        Assert.Equal("0", result.Value.Job.Metadata["runtime_created_dpo_candidate_count"]);
        Assert.Equal(bool.FalseString, result.Value.Job.Metadata["runtime_created_dpo_candidate"]);
        Assert.Equal("not-created", result.Value.Job.Metadata["runtime_created_dpo_candidate_review_status"]);
        Assert.DoesNotContain(
            fallbackQuarantineReason,
            result.Value.Job.Metadata["runtime_created_dpo_candidate_warnings"],
            StringComparison.Ordinal);
        Assert.Contains(
            expectedQuarantineReason,
            result.Value.Job.Metadata["runtime_created_dpo_candidate_warnings"],
            StringComparison.Ordinal);
        Assert.Equal(
            1,
            result.Value.Job.Metadata["runtime_created_dpo_candidate_warnings"]
                .Split('|', StringSplitOptions.RemoveEmptyEntries)
                .Count(warning => warning.StartsWith(
                    quarantineReasonPrefix,
                    StringComparison.Ordinal)));
        Assert.Contains(
            "runtime_rejected_evaluation_source_unavailable",
            result.Value.Job.Metadata["runtime_created_dpo_candidate_warnings"]);
        Assert.Contains("runtime_created_dpo_candidate_count=0", result.Value.Warnings);
        Assert.Contains("runtime_created_dpo_candidate=false", result.Value.Warnings);
        Assert.DoesNotContain(fallbackQuarantineReason, result.Value.Warnings);
        Assert.Contains(expectedQuarantineReason, result.Value.Warnings);
        Assert.Equal(
            1,
            result.Value.Warnings.Count(warning => warning.StartsWith(
                quarantineReasonPrefix,
                StringComparison.Ordinal)));
        Assert.Contains("runtime_rejected_evaluation_source_unavailable", result.Value.Warnings);
        Assert.DoesNotContain("runtime_created_dpo_candidate_review_pending", result.Value.Warnings);

        var approvedTraining = await db.QueryApprovedTrainingCandidatesAsync(
            new DesignConversionLocalArtifactDbQuery(JobId: submitted.Value.JobId));
        Assert.True(approvedTraining.IsSuccess, approvedTraining.ErrorMessage);
        Assert.Empty(approvedTraining.Value!);

        var persistedText = await File.ReadAllTextAsync(Path.Combine(root, "conversion-results.jsonl"));
        var indexText = await File.ReadAllTextAsync(Path.Combine(root, "conversion-results.index.json"));
        Assert.Contains("local_artifact_db_persistence_verified", persistedText, StringComparison.Ordinal);
        Assert.Contains("conversion_result", persistedText, StringComparison.Ordinal);
        Assert.Contains("render_artifact_refs", persistedText, StringComparison.Ordinal);
        Assert.Contains("visual_judge_status", persistedText, StringComparison.Ordinal);
        Assert.Contains("product_not_ready", persistedText, StringComparison.Ordinal);
        Assert.DoesNotContain(fallbackQuarantineReason, persistedText, StringComparison.Ordinal);
        Assert.Contains(expectedQuarantineReason, persistedText, StringComparison.Ordinal);
        Assert.Contains("runtime_rejected_evaluation_source_unavailable", persistedText, StringComparison.Ordinal);
        Assert.DoesNotContain("runtime-candidate-review-pending", persistedText, StringComparison.Ordinal);
        Assert.Contains("approval_status", persistedText, StringComparison.Ordinal);
        Assert.Contains("production_db_ready", persistedText, StringComparison.Ordinal);
        Assert.Contains("production_db_ready:false", indexText, StringComparison.Ordinal);
        Assert.DoesNotContain("<main", persistedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(".private-frame", persistedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("raw-tree-private", persistedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("C:/Users/lubao", persistedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("exact_prompt", persistedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("private.fig", persistedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("external-provider-payload", persistedText, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("omitted")]
    [InlineData("thrown")]
    [InlineData("failure")]
    [InlineData("null")]
    [InlineData("untrusted_reference")]
    [InlineData("blank_reference")]
    [InlineData("mismatched_reference")]
    [InlineData("same_job_identity")]
    [InlineData("equal_score")]
    [InlineData("higher_score")]
    [InlineData("missing_evidence")]
    [InlineData("synthetic")]
    [InlineData("scaffold")]
    public async Task RunAsync_UnavailableOrInvalidRejectedEvaluationDoesNotCreateDpoCandidate(
        string mutation)
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "thin-client-runtime-rejected-evaluation-" + mutation);
        var db = new JsonlDesignConversionLocalArtifactDb(root);
        var rejectedEvaluationSource = CreateRuntimeRejectedEvaluationSourceForMutation(mutation);
        var runtime = new DesignConversionThinClientJobRuntime(
            CreateRuntimeCandidateEligibleInternalEvaluator(),
            FixedClock(),
            db,
            rejectedEvaluationSource: rejectedEvaluationSource);
        var submitted = runtime.Submit(CreateRuntimeCandidateEligiblePrivateSubmittedRequest());
        Assert.True(submitted.IsSuccess, submitted.ErrorMessage);

        var result = await runtime.RunAsync(submitted.Value!.JobId);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionThinClientStatus.Completed, result.Value!.ContractStatus);
        var listed = await db.ListRecordsAsync(
            new DesignConversionLocalArtifactDbQuery(JobId: submitted.Value.JobId));
        Assert.True(listed.IsSuccess, listed.ErrorMessage);
        var stored = Assert.Single(listed.Value!);
        Assert.Empty(stored.DpoCandidates);
        Assert.Equal("0", stored.Metadata["runtime_created_dpo_candidate_count"]);
        Assert.Equal(bool.FalseString, stored.Metadata["runtime_created_dpo_candidate"]);
        Assert.Equal(bool.FalseString, stored.Metadata["review_required"]);
        Assert.Equal("not-required", stored.Metadata["approval_status"]);
        Assert.Equal(bool.FalseString, stored.Metadata["approved_for_training"]);
        Assert.DoesNotContain("source-private-message", stored.Metadata["runtime_created_dpo_candidate_warnings"]);
        Assert.DoesNotContain("SOURCE_PRIVATE_CODE", stored.Metadata["runtime_created_dpo_candidate_warnings"]);
        var approved = await db.QueryApprovedTrainingCandidatesAsync(
            new DesignConversionLocalArtifactDbQuery(JobId: submitted.Value.JobId));
        Assert.True(approved.IsSuccess, approved.ErrorMessage);
        Assert.Empty(approved.Value!);

        if (mutation is "omitted" or "thrown" or "failure" or "null" or
            "untrusted_reference" or "blank_reference" or "mismatched_reference" or
            "same_job_identity")
        {
            Assert.Contains(
                "runtime_rejected_evaluation_source_unavailable",
                stored.Metadata["runtime_created_dpo_candidate_warnings"]);
        }
        else
        {
            Assert.DoesNotContain(
                "runtime_rejected_evaluation_source_unavailable",
                stored.Metadata["runtime_created_dpo_candidate_warnings"]);
        }

        if (rejectedEvaluationSource is null)
        {
            Assert.Equal("omitted", mutation);
        }
        else
        {
            Assert.Equal(1, rejectedEvaluationSource.CallCount);
            Assert.Equal(submitted.Value.JobId, rejectedEvaluationSource.Lookup?.ChosenJobId);
        }
    }

    [Fact]
    public async Task RunAsync_ValidIndependentReceiptCreatesDualTargetReviewPendingCandidates()
    {
        var db = new JsonlDesignConversionLocalArtifactDb(Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "thin-client-runtime-valid-rejected-evaluation"));

        var execution = await RunRuntimeCreatedDpoCandidateAsync(db);

        Assert.Equal(1, execution.RejectedEvaluationSource.CallCount);
        Assert.Equal(
            execution.ChosenJobId,
            execution.RejectedEvaluationSource.Lookup?.ChosenJobId);
        Assert.Equal(2, execution.StoredRecord.DpoCandidates.Count);
        Assert.Equal(
            [
                DesignConversionDpoUnits.CriticUnit,
                DesignConversionDpoUnits.ImproverUnit
            ],
            execution.StoredRecord.DpoCandidates
                .Select(candidate => candidate.TrainingRecord.UnitId)
                .ToArray());
        Assert.Equal("2", execution.StoredRecord.Metadata["runtime_created_dpo_candidate_count"]);
        Assert.Equal("runtime-candidate-review-pending", execution.StoredRecord.Metadata["runtime_created_dpo_candidate_review_status"]);
        Assert.Equal(bool.TrueString, execution.StoredRecord.Metadata["review_required"]);
        Assert.Equal("pending", execution.StoredRecord.Metadata["approval_status"]);
        Assert.Equal(bool.FalseString, execution.StoredRecord.Metadata["approved_for_training"]);
    }

    [Fact]
    public async Task RunAsync_RouteBoundIndependentReceiptPersistsCanonicalTenantScopeOnEveryDpoCandidateWithoutRawTenant()
    {
        const string rawTenant = "w053-private-raw-tenant-alpha";
        const string rawScope = "W053_PRIVATE_RAW_SCOPE_ALPHA";
        const string rawToken = "W053_PRIVATE_RAW_TOKEN_ALPHA";
        var tenantIdRef = DesignConversionModelActivationRegistry.BuildTenantIdRef(rawTenant);
        var siblingTenantIdRef = DesignConversionModelActivationRegistry.BuildTenantIdRef("w053-sibling-tenant");
        var scopeRef = BuildW053RouteAdmissionScopeRef(rawScope);

        var matchingDb = new JsonlDesignConversionLocalArtifactDb(Path.Combine(
            Path.GetTempPath(), Guid.NewGuid().ToString("N"), "thin-client-route-bound-dpo-matching"));
        var matching = await RunRuntimeCreatedDpoCandidateAsync(
            matchingDb,
            routeBindingByEvaluationCall: _ => BuildW053RouteAdmissionBinding(
                tenantIdRef, scopeRef, rawTenant, rawScope, rawToken));

        Assert.Equal(2, matching.StoredRecord.DpoCandidates.Count);
        Assert.All(matching.StoredRecord.DpoCandidates, candidate =>
        {
            var metadata = candidate.TrainingRecord.Provenance.Metadata;
            Assert.Equal(bool.TrueString, metadata[DesignConversionThinClientJobRuntime.ServerOwnedRouteAdmissionTenantBoundMetadataKey]);
            Assert.Equal(tenantIdRef, metadata[DesignConversionThinClientJobRuntime.ServerOwnedRouteAdmissionTenantIdRefMetadataKey]);
            Assert.Equal(scopeRef, metadata[DesignConversionThinClientJobRuntime.ServerOwnedRouteAdmissionScopeRefMetadataKey]);
            Assert.Equal(3, metadata.Keys.Count(key => key.StartsWith("server_owned_route_admission_", StringComparison.Ordinal)));
        });
        var matchingJson = JsonSerializer.Serialize(matching.StoredRecord.DpoCandidates);
        Assert.DoesNotContain(rawTenant, matchingJson, StringComparison.Ordinal);
        Assert.DoesNotContain(rawScope, matchingJson, StringComparison.Ordinal);
        Assert.DoesNotContain(rawToken, matchingJson, StringComparison.Ordinal);

        var oneSidedDb = new JsonlDesignConversionLocalArtifactDb(Path.Combine(
            Path.GetTempPath(), Guid.NewGuid().ToString("N"), "thin-client-route-bound-dpo-one-sided"));
        var oneSided = await RunRuntimeCreatedDpoCandidateAsync(
            oneSidedDb,
            expectedDpoCandidateCount: 0,
            routeBindingByEvaluationCall: call => call == 1
                ? BuildW053RouteAdmissionBinding(tenantIdRef, scopeRef)
                : null);
        Assert.Empty(oneSided.StoredRecord.DpoCandidates);

        var crossTenantDb = new JsonlDesignConversionLocalArtifactDb(Path.Combine(
            Path.GetTempPath(), Guid.NewGuid().ToString("N"), "thin-client-route-bound-dpo-cross-tenant"));
        var crossTenant = await RunRuntimeCreatedDpoCandidateAsync(
            crossTenantDb,
            expectedDpoCandidateCount: 0,
            routeBindingByEvaluationCall: call => BuildW053RouteAdmissionBinding(
                call == 1 ? tenantIdRef : siblingTenantIdRef,
                scopeRef));
        Assert.Empty(crossTenant.StoredRecord.DpoCandidates);

        var malformedScopeDb = new JsonlDesignConversionLocalArtifactDb(Path.Combine(
            Path.GetTempPath(), Guid.NewGuid().ToString("N"), "thin-client-route-bound-dpo-malformed-scope"));
        var malformedScope = await RunRuntimeCreatedDpoCandidateAsync(
            malformedScopeDb,
            expectedDpoCandidateCount: 0,
            routeBindingByEvaluationCall: _ => BuildW053RouteAdmissionBinding(
                tenantIdRef,
                "sha256:design-conversion-route-admission-scope:ABC"));
        Assert.Empty(malformedScope.StoredRecord.DpoCandidates);
    }

    [Fact]
    public async Task RunAsync_ExplicitLocalEvaluatorIssuesReceiptOnThinRuntimeProductionPath()
    {
        var db = new JsonlDesignConversionLocalArtifactDb(Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "thin-client-local-independent-evaluator"));
        var client = new ThinRuntimeLocalIndependentEvaluationClient();
        var evaluator =
            DesignConversionLocalIndependentPreferenceEvaluator.OwnForTesting(
                client,
                ThinRuntimeLocalIndependentEvaluationClient.ModelId,
                ThinRuntimeLocalIndependentEvaluationClient.ManifestDigest,
                ThinRuntimeLocalIndependentEvaluationClient.ModelLayerDigest,
                DesignConversionDpoGenerator.Sha256Ref(
                    "thin-runtime-local-independent-evaluator"));

        var execution = await RunRuntimeCreatedDpoCandidateAsync(
            db,
            evaluator);

        Assert.Equal(1, client.CallCount);
        Assert.True(client.Disposed);
        Assert.Single(execution.PairEvaluator.RuntimePairs);
        var runtimePair = execution.PairEvaluator.RuntimePairs[0];
        Assert.NotEqual(
            runtimePair.Generated.ContentSha256,
            runtimePair.Incumbent.ContentSha256);
        Assert.NotEqual(
            runtimePair.Generated.EvaluationIdentity,
            runtimePair.Incumbent.EvaluationIdentity);
        Assert.True(execution.RejectedEvaluationSource.ResolvedReference!
            .TryGetTrustedCoordinatorConsumedPreferenceEvidence(
                out var handedOffReceipts));
        Assert.Equal(
            runtimePair.IndependentEvaluation.ReceiptSha256,
            Assert.Single(handedOffReceipts).ReceiptSha256);
        Assert.Equal(2, execution.StoredRecord.DpoCandidates.Count);
        Assert.All(execution.StoredRecord.DpoCandidates, candidate =>
        {
            Assert.Equal(
                bool.FalseString,
                candidate.TrainingRecord.Provenance.Metadata["training_grade"]);
            Assert.Equal(
                "sha256:" +
                ThinRuntimeLocalIndependentEvaluationClient.ModelLayerDigest,
                candidate.TrainingRecord.Provenance.Metadata[
                    "independent_scorer_model_sha256"]);
        });
    }

    [Fact]
    public async Task RunAsync_UnavailableExplicitLocalEvaluatorFailsClosedWithoutFallbackOrLeak()
    {
        var db = new JsonlDesignConversionLocalArtifactDb(Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "thin-client-local-independent-evaluator-unavailable"));
        var client = new UnavailableThinRuntimeLocalIndependentEvaluationClient();
        var evaluator =
            DesignConversionLocalIndependentPreferenceEvaluator.OwnForTesting(
                client,
                ThinRuntimeLocalIndependentEvaluationClient.ModelId,
                ThinRuntimeLocalIndependentEvaluationClient.ManifestDigest,
                ThinRuntimeLocalIndependentEvaluationClient.ModelLayerDigest,
                DesignConversionDpoGenerator.Sha256Ref(
                    "thin-runtime-local-independent-evaluator-unavailable"));

        var execution = await RunRuntimeCreatedDpoCandidateAsync(
            db,
            evaluator,
            expectedDpoCandidateCount: 0);

        Assert.Equal(1, client.CallCount);
        Assert.True(client.Disposed);
        Assert.Empty(execution.PairEvaluator.RuntimePairs);
        Assert.True(execution.RejectedEvaluationSource.ResolvedReference!
            .TryGetTrustedCoordinatorConsumedPreferenceEvidence(
                out var handedOffReceipts));
        Assert.Empty(handedOffReceipts);
        Assert.Empty(execution.StoredRecord.DpoCandidates);
        Assert.Equal(
            "0",
            execution.StoredRecord.Metadata["runtime_created_dpo_candidate_count"]);
        var persistedWarnings = execution.StoredRecord.Metadata[
            "runtime_created_dpo_candidate_warnings"];
        Assert.Contains(
            "LOCAL_INDEPENDENT_PREFERENCE_EVALUATION_FAILED:" +
            UnavailableThinRuntimeLocalIndependentEvaluationClient.ErrorCode,
            persistedWarnings,
            StringComparison.Ordinal);
        var observable = string.Join(
            "|",
            execution.RunResult.Warnings.Append(persistedWarnings));
        Assert.DoesNotContain(
            "independent_preference_proven=true",
            observable,
            StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("self_label", observable, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("fallback", observable, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(
            UnavailableThinRuntimeLocalIndependentEvaluationClient.PrivateFailureMarker,
            observable,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            db.AllowedArtifactRootDirectory,
            observable,
            StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(
            DesignConversionLocalIndependentPreferenceEvaluator.RequestSchema,
            observable,
            StringComparison.Ordinal);
    }

    [Fact]
    public void RuntimeRejectedEvaluationSource_PublicBoundaryContainsOnlyOpaqueIdentifiers()
    {
        var publicSurfaceTypes = new[]
        {
            typeof(IDesignConversionRuntimeRejectedEvaluationSource),
            typeof(DesignConversionRuntimeRejectedEvaluationLookup),
            typeof(DesignConversionRuntimeRejectedEvaluationReference)
        };

        var signatureTypes = publicSurfaceTypes
            .SelectMany(GetPublicSurfaceSignatureTypes)
            .Distinct()
            .ToArray();
        var publicMemberNames = publicSurfaceTypes
            .SelectMany(type => type.GetMembers(
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.Static))
            .Select(member => member.Name)
            .ToArray();

        Assert.DoesNotContain(signatureTypes, IsRawRuntimeEvaluationPayloadType);
        Assert.DoesNotContain(
            publicMemberNames,
            name => name is "SubmittedRequest" or "RenderResults" or "ComparisonArtifacts" or
                "VisualJudgeResults" or "QaResults" or "RuntimeFlowResult" or "ImprovedHtml" or
                "ImprovedCss" or "EvidenceRefs" or "Warnings" or "ScaffoldMarkers" or
                "VisualQaBoundaryTrace");
        Assert.Contains(nameof(DesignConversionRuntimeRejectedEvaluationLookup.ChosenJobId), publicMemberNames);
        Assert.Contains(nameof(DesignConversionRuntimeRejectedEvaluationReference.EvaluationRef), publicMemberNames);
    }

    [Theory]
    [InlineData("completed")]
    [InlineData("faulted")]
    [InlineData("cancelled")]
    public async Task RunAsync_PrecompletedFinalizationDeadlineDoesNotStartRejectedSourceOrPersistence(
        string deadlineState)
    {
        using var cancelledDeadlineSource = new CancellationTokenSource();
        cancelledDeadlineSource.Cancel();
        var root = CreateBudgetStoreTempRoot(
            "precompleted-finalization-deadline-" + deadlineState);
        var artifactDb = new RecordingLocalArtifactDb(new JsonlDesignConversionLocalArtifactDb(root));
        var ledger = new RecordingVisualCostLedger(
            new JsonlDesignConversionVisualCostLedger(
                CreateBudgetStoreTempRoot("precompleted-finalization-ledger-" + deadlineState)));
        var rejectedSource = CreateValidRuntimeRejectedEvaluationSource();
        Task deadline = deadlineState switch
        {
            "completed" => Task.CompletedTask,
            "faulted" => Task.FromException(
                new InvalidOperationException("controlled-precompleted-finalization-fault")),
            "cancelled" => Task.FromCanceled(cancelledDeadlineSource.Token),
            _ => throw new ArgumentOutOfRangeException(nameof(deadlineState), deadlineState, "Unknown deadline state.")
        };
        var runtime = new DesignConversionThinClientJobRuntime(
            CreateMeasuredRuntimeCandidateEligibleEvaluator(),
            FixedClock(),
            artifactDb,
            ledger,
            visualCostBudgetStore: null,
            staleRunningTimeout: null,
            beforePostEvaluationOwnershipTransition: null,
            postEvaluationFinalizationTimeout: TimeSpan.FromSeconds(30),
            postEvaluationFinalizationDelay: (_, _) => deadline,
            postEvaluationStateStore: new JsonDesignConversionPostEvaluationStateStore(
                Path.Combine(root, JsonDesignConversionPostEvaluationStateStore.DirectoryName)),
            rejectedEvaluationSource: rejectedSource);
        var submitted = runtime.Submit(CreateRuntimeCandidateEligiblePrivateSubmittedRequest() with
        {
            RequestId = "thin-precompleted-finalization-deadline-" + deadlineState
        });
        Assert.True(submitted.IsSuccess, submitted.ErrorMessage);

        var result = await runtime.RunAsync(submitted.Value!.JobId);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionThinClientStatus.Failed, result.Value!.Job.Status);
        Assert.Equal(
            string.Equals(deadlineState, "faulted", StringComparison.Ordinal)
                ? DesignConversionThinClientJobRuntime.PostEvaluationFinalizationFailedErrorCode
                : DesignConversionThinClientJobRuntime.PostEvaluationFinalizationTimeoutErrorCode,
            result.Value.Job.Metadata["error_code"]);
        Assert.Equal(0, rejectedSource.CallCount);
        Assert.Null(rejectedSource.Lookup);
        Assert.Empty(ledger.AttemptedRecords);
        Assert.Empty(artifactDb.AttemptedRecords);
        Assert.Equal(0, artifactDb.ListCalls);
        Assert.Equal(0, runtime.LiveRunLeaseCount);
    }

    [Fact]
    public async Task RunAsync_InFlightRejectedSourceCancelledByFinalizationDeadlineCannotPersistLater()
    {
        var deadline = new ManualFinalizationDeadline();
        var sourceEntered = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var sourceCompletion = new TaskCompletionSource<
            OperationResult<DesignConversionRuntimeRejectedEvaluationReference?>>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var root = CreateBudgetStoreTempRoot("in-flight-rejected-source-finalization-deadline");
        var artifactDb = new RecordingLocalArtifactDb(new JsonlDesignConversionLocalArtifactDb(root));
        var ledger = new RecordingVisualCostLedger(
            new JsonlDesignConversionVisualCostLedger(
                CreateBudgetStoreTempRoot("in-flight-rejected-source-ledger")));
        var rejectedSource = new ScriptedRuntimeRejectedEvaluationSource((_, _) =>
        {
            sourceEntered.TrySetResult(true);
            return sourceCompletion.Task;
        });
        var runtime = new DesignConversionThinClientJobRuntime(
            CreateMeasuredRuntimeCandidateEligibleEvaluator(),
            FixedClock(),
            artifactDb,
            ledger,
            visualCostBudgetStore: null,
            staleRunningTimeout: null,
            beforePostEvaluationOwnershipTransition: null,
            postEvaluationFinalizationTimeout: TimeSpan.FromSeconds(30),
            postEvaluationFinalizationDelay: deadline.WaitAsync,
            postEvaluationStateStore: new JsonDesignConversionPostEvaluationStateStore(
                Path.Combine(root, JsonDesignConversionPostEvaluationStateStore.DirectoryName)),
            rejectedEvaluationSource: rejectedSource);
        var submitted = runtime.Submit(CreateRuntimeCandidateEligiblePrivateSubmittedRequest() with
        {
            RequestId = "thin-in-flight-rejected-source-finalization-deadline"
        });
        Assert.True(submitted.IsSuccess, submitted.ErrorMessage);

        var run = runtime.RunAsync(submitted.Value!.JobId);
        await sourceEntered.Task.WaitAsync(TimeSpan.FromSeconds(5));
        deadline.Expire();
        var result = await run.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionThinClientStatus.Failed, result.Value!.Job.Status);
        Assert.Equal(
            DesignConversionThinClientJobRuntime.PostEvaluationFinalizationTimeoutErrorCode,
            result.Value.Job.Metadata["error_code"]);
        Assert.True(rejectedSource.CapturedCancellationToken.IsCancellationRequested);
        Assert.Empty(ledger.AttemptedRecords);
        Assert.Empty(artifactDb.AttemptedRecords);
        Assert.Equal(0, artifactDb.ListCalls);

        sourceCompletion.TrySetResult(OperationResult<
            DesignConversionRuntimeRejectedEvaluationReference?>.Ok(
            CreateInternallyHydratedRuntimeRejectedEvaluationReference(
                CreateValidMeasuredRuntimeRejectedEvaluation())));
        await rejectedSource.Returned.Task.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.Empty(ledger.AttemptedRecords);
        Assert.Empty(artifactDb.AttemptedRecords);
        Assert.Equal(0, artifactDb.ListCalls);
        Assert.Equal(0, runtime.LiveRunLeaseCount);
    }

    [Fact]
    public async Task RunAsync_CallerCancellationAfterEvaluatorBeforePersistenceDoesNotStartRejectedSourceOrPersistence()
    {
        using var callerCancellation = new CancellationTokenSource();
        var evaluator = new CancellationIgnoringGateEvaluator();
        var root = CreateBudgetStoreTempRoot("caller-cancellation-before-persistence");
        var artifactDb = new RecordingLocalArtifactDb(new JsonlDesignConversionLocalArtifactDb(root));
        var ledger = new RecordingVisualCostLedger(
            new JsonlDesignConversionVisualCostLedger(
                CreateBudgetStoreTempRoot("caller-cancellation-before-persistence-ledger")));
        var rejectedSource = CreateValidRuntimeRejectedEvaluationSource();
        var runtime = new DesignConversionThinClientJobRuntime(
            evaluator,
            FixedClock(),
            artifactDb,
            ledger,
            visualCostBudgetStore: null,
            staleRunningTimeout: null,
            beforePostEvaluationOwnershipTransition: null,
            postEvaluationFinalizationTimeout: TimeSpan.FromSeconds(30),
            postEvaluationStateStore: new JsonDesignConversionPostEvaluationStateStore(
                Path.Combine(root, JsonDesignConversionPostEvaluationStateStore.DirectoryName)),
            rejectedEvaluationSource: rejectedSource);
        var submitted = runtime.Submit(CreateRuntimeCandidateEligiblePrivateSubmittedRequest() with
        {
            RequestId = "thin-caller-cancellation-before-persistence"
        });
        Assert.True(submitted.IsSuccess, submitted.ErrorMessage);

        var run = runtime.RunAsync(submitted.Value!.JobId, callerCancellation.Token);
        await evaluator.Entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
        callerCancellation.Cancel();
        evaluator.Release();
        var result = await run.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionThinClientStatus.Failed, result.Value!.Job.Status);
        Assert.Equal(
            DesignConversionThinClientJobRuntime.PostEvaluationFinalizationCancelledErrorCode,
            result.Value.Job.Metadata["error_code"]);
        Assert.Equal(0, rejectedSource.CallCount);
        Assert.Null(rejectedSource.Lookup);
        Assert.Empty(ledger.AttemptedRecords);
        Assert.Empty(artifactDb.AttemptedRecords);
        Assert.Equal(0, artifactDb.ListCalls);
        Assert.Equal(0, runtime.LiveRunLeaseCount);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RunAsync_InFlightRejectedSourceCancelledByCallerCannotPersistLater(
        bool lateSourceFault)
    {
        using var callerCancellation = new CancellationTokenSource();
        var sourceEntered = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var sourceCompletion = new TaskCompletionSource<
            OperationResult<DesignConversionRuntimeRejectedEvaluationReference?>>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var root = CreateBudgetStoreTempRoot(
            "in-flight-rejected-source-caller-cancellation-" + lateSourceFault);
        var artifactDb = new RecordingLocalArtifactDb(new JsonlDesignConversionLocalArtifactDb(root));
        var ledger = new RecordingVisualCostLedger(
            new JsonlDesignConversionVisualCostLedger(
                CreateBudgetStoreTempRoot("in-flight-rejected-source-caller-ledger-" + lateSourceFault)));
        var rejectedSource = new ScriptedRuntimeRejectedEvaluationSource((_, _) =>
        {
            sourceEntered.TrySetResult(true);
            return sourceCompletion.Task;
        });
        var runtime = new DesignConversionThinClientJobRuntime(
            CreateMeasuredRuntimeCandidateEligibleEvaluator(),
            FixedClock(),
            artifactDb,
            ledger,
            visualCostBudgetStore: null,
            staleRunningTimeout: null,
            beforePostEvaluationOwnershipTransition: null,
            postEvaluationFinalizationTimeout: TimeSpan.FromSeconds(30),
            postEvaluationStateStore: new JsonDesignConversionPostEvaluationStateStore(
                Path.Combine(root, JsonDesignConversionPostEvaluationStateStore.DirectoryName)),
            rejectedEvaluationSource: rejectedSource);
        var submitted = runtime.Submit(CreateRuntimeCandidateEligiblePrivateSubmittedRequest() with
        {
            RequestId = "thin-in-flight-rejected-source-caller-cancellation-" + lateSourceFault
        });
        Assert.True(submitted.IsSuccess, submitted.ErrorMessage);

        var run = runtime.RunAsync(submitted.Value!.JobId, callerCancellation.Token);
        await sourceEntered.Task.WaitAsync(TimeSpan.FromSeconds(5));
        callerCancellation.Cancel();
        var result = await run.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionThinClientStatus.Failed, result.Value!.Job.Status);
        Assert.Equal(
            DesignConversionThinClientJobRuntime.PostEvaluationFinalizationCancelledErrorCode,
            result.Value.Job.Metadata["error_code"]);
        Assert.True(rejectedSource.CapturedCancellationToken.IsCancellationRequested);
        Assert.Empty(ledger.AttemptedRecords);
        Assert.Empty(artifactDb.AttemptedRecords);
        Assert.Equal(0, artifactDb.ListCalls);

        if (lateSourceFault)
        {
            sourceCompletion.TrySetException(
                new InvalidOperationException("controlled-late-rejected-source-fault"));
        }
        else
        {
            sourceCompletion.TrySetResult(OperationResult<
                DesignConversionRuntimeRejectedEvaluationReference?>.Ok(
                CreateInternallyHydratedRuntimeRejectedEvaluationReference(
                    CreateValidMeasuredRuntimeRejectedEvaluation())));
        }

        await rejectedSource.Returned.Task.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.Empty(ledger.AttemptedRecords);
        Assert.Empty(artifactDb.AttemptedRecords);
        Assert.Equal(0, artifactDb.ListCalls);
        Assert.Equal(0, runtime.LiveRunLeaseCount);
    }

    [Fact]
    public async Task RunAsync_IncompleteResponsiveCoveragePersistsEvaluationWithoutDpoCandidate()
    {
        const string quarantineReasonPrefix = "runtime_created_dpo_candidate_quarantine_reason=";
        const string visualDecisionQuarantineReason =
            quarantineReasonPrefix + "visual_decision_can_persist_training_candidate_false";
        const string fallbackQuarantineReason =
            quarantineReasonPrefix + "runtime_rejected_candidate_independent_evaluation_required";
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "thin-client-runtime-incomplete-artifact-db");
        var db = new JsonlDesignConversionLocalArtifactDb(root);
        var evaluator = new RecordingEvaluator(CreateVisualJudgeInternalEvaluator(
            new ScriptedCostVisualJudge(new VisualJudgeCostScript(Score: 0.95, CostUsd: 0.01m))));
        var runtime = new DesignConversionThinClientJobRuntime(evaluator, FixedClock(), db);
        var submitted = runtime.Submit(CreatePrivateSubmittedRequest());
        Assert.True(submitted.IsSuccess, submitted.ErrorMessage);

        var result = await runtime.RunAsync(submitted.Value!.JobId);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionThinClientStatus.Quarantined, result.Value!.ContractStatus);
        Assert.Equal(DesignConversionThinClientStatus.Quarantined, result.Value.Job.Status);
        Assert.NotEqual(DesignConversionThinClientStatus.Completed, result.Value.ContractStatus);
        Assert.NotEqual(DesignConversionThinClientStatus.Completed, result.Value.Job.Status);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.DecisionQuarantine, result.Value.Provenance["final_visual_decision"]);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.RenderManifestIncomplete, result.Value.Provenance["tier_policy_stop_reason"]);
        Assert.Contains($"final_visual_decision={DesignConversionVisualQaBoundaryPolicy.DecisionQuarantine}", result.Value.Warnings);
        Assert.Contains($"tier_policy_stop_reason={DesignConversionVisualQaBoundaryPolicy.RenderManifestIncomplete}", result.Value.Warnings);
        var secondResult = await runtime.RunAsync(submitted.Value.JobId);
        Assert.True(secondResult.IsSuccess, secondResult.ErrorMessage);
        Assert.Equal(DesignConversionThinClientStatus.Quarantined, secondResult.Value!.ContractStatus);
        Assert.Equal(DesignConversionThinClientStatus.Quarantined, secondResult.Value.Job.Status);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.DecisionQuarantine, secondResult.Value.Provenance["final_visual_decision"]);
        Assert.Single(evaluator.Requests);
        var evaluation = Assert.Single(evaluator.Responses).Value!;
        Assert.Equal(
            ["source-1440x900", DesignConversionRenderResolutionFactory.ServerOwnedMobileResolutionId],
            evaluation.VisualQaBoundaryTrace.MultiResolutionRender.RequiredViewportIds);
        Assert.Equal(["source-1440x900"], evaluation.VisualQaBoundaryTrace.MultiResolutionRender.ObservedViewportIds);
        Assert.Equal(
            [DesignConversionRenderResolutionFactory.ServerOwnedMobileResolutionId],
            evaluation.VisualQaBoundaryTrace.MultiResolutionRender.MissingViewportIds);
        Assert.False(evaluation.VisualQaBoundaryTrace.MultiResolutionRender.IsComplete);
        Assert.Equal(
            DesignConversionVisualQaBoundaryPolicy.RenderManifestIncomplete,
            evaluation.VisualQaBoundaryTrace.TierPolicy.StopReason);
        Assert.Equal(
            DesignConversionVisualQaBoundaryPolicy.DecisionQuarantine,
            evaluation.VisualQaBoundaryTrace.VisualDecision.Decision);
        Assert.False(evaluation.VisualQaBoundaryTrace.VisualDecision.CanPersistTrainingCandidate);

        var listed = await db.ListRecordsAsync(new DesignConversionLocalArtifactDbQuery(JobId: submitted.Value.JobId));
        Assert.True(listed.IsSuccess, listed.ErrorMessage);
        var stored = Assert.Single(listed.Value!);
        Assert.Equal("0", stored.Metadata["runtime_created_dpo_candidate_count"]);
        Assert.Equal(bool.FalseString, stored.Metadata["runtime_created_dpo_candidate"]);
        Assert.Equal("not-created", stored.Metadata["runtime_created_dpo_candidate_review_status"]);
        Assert.Equal("runtime_created_dpo_candidate_not_created", stored.Metadata["training_candidate_only_boundary"]);
        Assert.Contains(
            "runtime_created_dpo_candidate_not_created_visual_decision_can_persist_training_candidate_false",
            stored.Metadata["runtime_created_dpo_candidate_warnings"],
            StringComparison.Ordinal);
        Assert.Contains(
            visualDecisionQuarantineReason,
            stored.Metadata["runtime_created_dpo_candidate_warnings"],
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            fallbackQuarantineReason,
            stored.Metadata["runtime_created_dpo_candidate_warnings"],
            StringComparison.Ordinal);
        Assert.Equal(
            1,
            stored.Metadata["runtime_created_dpo_candidate_warnings"]
                .Split('|', StringSplitOptions.RemoveEmptyEntries)
                .Count(warning => warning.StartsWith(
                    quarantineReasonPrefix,
                    StringComparison.Ordinal)));
        Assert.Empty(stored.DpoCandidates);
        Assert.Equal("0", result.Value.Job.Metadata["runtime_created_dpo_candidate_count"]);
        Assert.Equal(bool.FalseString, result.Value.Job.Metadata["runtime_created_dpo_candidate"]);
        Assert.Equal("not-created", result.Value.Job.Metadata["runtime_created_dpo_candidate_review_status"]);
        Assert.Contains(
            visualDecisionQuarantineReason,
            result.Value.Job.Metadata["runtime_created_dpo_candidate_warnings"],
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            fallbackQuarantineReason,
            result.Value.Job.Metadata["runtime_created_dpo_candidate_warnings"],
            StringComparison.Ordinal);
        Assert.Equal(
            1,
            result.Value.Job.Metadata["runtime_created_dpo_candidate_warnings"]
                .Split('|', StringSplitOptions.RemoveEmptyEntries)
                .Count(warning => warning.StartsWith(
                    quarantineReasonPrefix,
                    StringComparison.Ordinal)));
        Assert.Contains("runtime_created_dpo_candidate_count=0", result.Value.Warnings);
        Assert.Contains("runtime_created_dpo_candidate=false", result.Value.Warnings);
        Assert.DoesNotContain(fallbackQuarantineReason, result.Value.Warnings);
        Assert.Contains(visualDecisionQuarantineReason, result.Value.Warnings);
        Assert.Equal(
            1,
            result.Value.Warnings.Count(warning => warning.StartsWith(
                quarantineReasonPrefix,
                StringComparison.Ordinal)));

        var persistedText = await File.ReadAllTextAsync(Path.Combine(root, "conversion-results.jsonl"));
        var refsOnlyStatusText = JsonSerializer.Serialize(new
        {
            result.Value.Job.Metadata,
            result.Value.Warnings,
            result.Value.Artifacts,
            result.Value.QualityMetrics,
            result.Value.Defects
        });
        Assert.Contains(visualDecisionQuarantineReason, persistedText, StringComparison.Ordinal);
        Assert.DoesNotContain(fallbackQuarantineReason, persistedText, StringComparison.Ordinal);
        Assert.Contains(visualDecisionQuarantineReason, refsOnlyStatusText, StringComparison.Ordinal);
        Assert.DoesNotContain(fallbackQuarantineReason, refsOnlyStatusText, StringComparison.Ordinal);
        foreach (var text in new[] { persistedText, refsOnlyStatusText })
        {
            Assert.DoesNotContain("<main", text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(".private-frame", text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("raw-tree-private", text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("C:/Users/lubao", text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("exact_prompt", text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("private.fig", text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("external-provider-payload", text, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Theory]
    [InlineData(
        DesignConversionVisualQaBoundaryPolicy.DecisionRetry,
        DesignConversionVisualQaBoundaryPolicy.ContinueImprovement,
        2)]
    [InlineData(
        DesignConversionVisualQaBoundaryPolicy.DecisionBlocked,
        DesignConversionVisualQaBoundaryPolicy.BudgetCapReached,
        1)]
    public async Task RunAsync_RetryAndBlockedVisualDecisionsReturnNotReadyAndTerminalReplayIsIdempotent(
        string expectedVisualDecision,
        string expectedStopReason,
        int maxImprovementCandidates)
    {
        var evaluator = new RecordingEvaluator(CreateVisualJudgeInternalEvaluator(
            new ScriptedCostVisualJudge(new VisualJudgeCostScript(Score: 0.70, CostUsd: 0.01m))));
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(
            Path.Combine(
                Path.GetTempPath(),
                Guid.NewGuid().ToString("N"),
                $"thin-client-{expectedVisualDecision}-not-ready-artifact-db"));
        var runtime = new DesignConversionThinClientJobRuntime(evaluator, FixedClock(), artifactDb);
        var baseRequest = CreateRuntimeCandidateEligiblePrivateSubmittedRequest();
        var request = baseRequest with
        {
            RequestId = $"thin-client-runtime-{expectedVisualDecision}-not-ready",
            Budget = baseRequest.Budget with
            {
                Tier = "pro",
                MaxImprovementCandidates = maxImprovementCandidates,
                BudgetUsd = 1.00m,
                RenderQaPassThreshold = 0.95
            }
        };
        var submitted = runtime.Submit(request);
        Assert.True(submitted.IsSuccess, submitted.ErrorMessage);

        var firstRun = await runtime.RunAsync(submitted.Value!.JobId);
        var replay = await runtime.RunAsync(submitted.Value.JobId);
        var readback = runtime.GetStatus(submitted.Value.JobId);

        Assert.True(firstRun.IsSuccess, firstRun.ErrorMessage);
        Assert.True(replay.IsSuccess, replay.ErrorMessage);
        Assert.True(readback.IsSuccess, readback.ErrorMessage);
        Assert.Equal(DesignConversionThinClientStatus.NotReady, firstRun.Value!.ContractStatus);
        Assert.Equal(DesignConversionThinClientStatus.NotReady, firstRun.Value.Job.Status);
        Assert.Equal(DesignConversionThinClientStatus.NotReady, replay.Value!.ContractStatus);
        Assert.Equal(DesignConversionThinClientStatus.NotReady, replay.Value.Job.Status);
        Assert.Equal(DesignConversionThinClientStatus.NotReady, readback.Value!.ContractStatus);
        Assert.Equal(DesignConversionThinClientStatus.NotReady, readback.Value.Job.Status);
        Assert.Equal(expectedVisualDecision, firstRun.Value.Provenance["final_visual_decision"]);
        Assert.Equal(expectedStopReason, firstRun.Value.Provenance["tier_policy_stop_reason"]);
        Assert.Equal(DesignConversionThinClientStatus.NotReady, firstRun.Value.Provenance["thin_client_terminal_status"]);
        Assert.Contains($"final_visual_decision={expectedVisualDecision}", firstRun.Value.Warnings);
        Assert.Contains($"tier_policy_stop_reason={expectedStopReason}", firstRun.Value.Warnings);
        Assert.Contains($"thin_client_terminal_status={DesignConversionThinClientStatus.NotReady}", firstRun.Value.Warnings);
        Assert.True(firstRun.Value.QualityMetrics.Available);
        Assert.True(firstRun.Value.QualityMetrics.Passed.HasValue);
        Assert.False(firstRun.Value.QualityMetrics.Passed.Value);
        Assert.True(firstRun.Value.QualityMetrics.ThresholdMet.HasValue);
        Assert.False(firstRun.Value.QualityMetrics.ThresholdMet.Value);
        Assert.Single(evaluator.Requests);
        var evaluation = Assert.Single(evaluator.Responses).Value!;
        Assert.False(evaluation.VisualQaBoundaryTrace.ProductReady);
        Assert.False(evaluation.VisualQaBoundaryTrace.VisualDecision.QualityThresholdMet);
        Assert.Equal(expectedVisualDecision, evaluation.VisualQaBoundaryTrace.VisualDecision.Decision);
        Assert.Equal(expectedStopReason, evaluation.VisualQaBoundaryTrace.TierPolicy.StopReason);
        Assert.Equal(
            firstRun.Value.Artifacts.Select(artifact => artifact.ArtifactId),
            replay.Value.Artifacts.Select(artifact => artifact.ArtifactId));
        Assert.Equal(
            firstRun.Value.Artifacts.Select(artifact => artifact.ArtifactId),
            readback.Value.Artifacts.Select(artifact => artifact.ArtifactId));

        var durableRecords = await artifactDb.ListRecordsAsync(
            new DesignConversionLocalArtifactDbQuery(JobId: submitted.Value.JobId));
        Assert.True(durableRecords.IsSuccess, durableRecords.ErrorMessage);
        var durableRecord = Assert.Single(durableRecords.Value!);
        Assert.Empty(durableRecord.DpoCandidates);
        Assert.Equal("0", durableRecord.Metadata["runtime_created_dpo_candidate_count"]);
        Assert.Contains(
            "runtime_rejected_evaluation_source_unavailable",
            durableRecord.Metadata["runtime_created_dpo_candidate_warnings"]);
    }

    [Fact]
    public async Task RunAsync_WithVerifiedServerOwnedVisualJudgeReceiptPersistsExactReceiptWithoutUserIdentity()
    {
        const string rawUserIdentityMarker = "raw-user-identity-unique-7E5F9A";
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "thin-client-runtime-receipt-artifact-db");
        var ledgerRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "thin-client-runtime-visual-cost-ledger");
        var db = new JsonlDesignConversionLocalArtifactDb(root);
        var ledger = new JsonlDesignConversionVisualCostLedger(ledgerRoot);
        var receipt = BuildServerOwnedReceipt("2", "0.0300", "0.0600");
        var visualJudge = new ReceiptVisualJudge(receipt, receipt, estimatedCostUsd: 0.0600m);
        var evaluator = new RecordingEvaluator(CreateVisualJudgeInternalEvaluator(visualJudge));
        var runtime = new DesignConversionThinClientJobRuntime(evaluator, FixedClock(), db, ledger);
        var request = CreateSubmittedRequest() with
        {
            RequestId = "thin-receipt-request-001",
            Budget = CreateSubmittedRequest().Budget with
            {
                UserId = $"  {rawUserIdentityMarker}  ",
                Tier = "paid",
                MaxRenderQaRuns = 1,
                BudgetUsd = 0.25m
            }
        };
        var submitted = runtime.Submit(request);
        Assert.True(submitted.IsSuccess, submitted.ErrorMessage);

        var firstRun = await runtime.RunAsync(submitted.Value!.JobId);
        var secondRun = await runtime.RunAsync(submitted.Value.JobId);

        Assert.True(firstRun.IsSuccess, firstRun.ErrorMessage);
        Assert.True(secondRun.IsSuccess, secondRun.ErrorMessage);
        AssertThinClientQuarantinedVisualDecision(firstRun.Value!);
        AssertThinClientQuarantinedVisualDecision(secondRun.Value!);
        Assert.Equal(bool.TrueString, firstRun.Value.Job.Metadata["visual_cost_ledger_persistence_verified"]);
        Assert.Equal(JsonlDesignConversionVisualCostLedger.LedgerKind, firstRun.Value.Job.Metadata["visual_cost_ledger_kind"]);
        Assert.Equal("0.06", firstRun.Value.Job.Metadata["visual_cost_ledger_total_cost_usd"]);
        Assert.Contains("visual_cost_ledger_persistence_verified=true", firstRun.Value.Warnings);
        Assert.Contains("visual_cost_ledger_training_artifact_db=false", firstRun.Value.Warnings);
        Assert.Single(evaluator.Requests);
        Assert.Equal(1, visualJudge.InvocationCount);
        var listed = await db.ListRecordsAsync(new DesignConversionLocalArtifactDbQuery(JobId: submitted.Value.JobId));
        Assert.True(listed.IsSuccess, listed.ErrorMessage);
        var stored = Assert.Single(listed.Value!);
        var metadata = stored.VisualJudgeStatus.Metadata;
        Assert.Equal(0.0600m, stored.VisualJudgeStatus.EstimatedCostUsd);
        var evaluation = Assert.Single(evaluator.Responses).Value!;
        Assert.Equal(evaluation.VisualQaBoundaryTrace.TierPolicy.EstimatedCostUsd, evaluation.VisualCostAccounting.TotalCostUsd);
        Assert.Equal(0.0600m, evaluation.VisualCostAccounting.TotalCostUsd);
        Assert.True(evaluation.VisualCostAccounting.MeasurementComplete);
        Assert.Equal(0, evaluation.VisualCostAccounting.MissingMeasurementCount);
        Assert.Equal(1, evaluation.VisualCostAccounting.EvaluationAttemptCount);
        Assert.Equal(
            DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostSourceFixedPerRequest,
            evaluation.VisualCostAccounting.CostSource);
        Assert.Equal(bool.TrueString, metadata["server_owned_accounting_receipt_verified"]);
        Assert.Equal("verified_persisted", metadata["server_owned_accounting_receipt_status"]);
        Assert.Equal(5, receipt.Count);
        foreach (var pair in receipt)
            Assert.Equal(pair.Value, metadata[pair.Key]);
        AssertNoUserOrBillingIdentityMetadataKeys(metadata);
        Assert.False(metadata.ContainsKey("untrusted_provider_extra"));
        Assert.False(metadata.ContainsKey("untrusted_result_extra"));
        var serializedRecord = JsonSerializer.Serialize(stored);
        var serializedMetadata = JsonSerializer.Serialize(metadata);
        Assert.DoesNotContain(rawUserIdentityMarker, serializedRecord, StringComparison.Ordinal);
        Assert.DoesNotContain(rawUserIdentityMarker, serializedMetadata, StringComparison.Ordinal);
        var persistedText = await File.ReadAllTextAsync(Path.Combine(root, "conversion-results.jsonl"));
        Assert.DoesNotContain(rawUserIdentityMarker, persistedText, StringComparison.Ordinal);

        var ledgerRead = await ledger.ListRecordsAsync();
        Assert.True(ledgerRead.IsSuccess, ledgerRead.ErrorMessage);
        var ledgerRecord = Assert.Single(ledgerRead.Value!);
        Assert.Equal(JsonlDesignConversionVisualCostLedger.LedgerKind, ledgerRecord.LedgerKind);
        Assert.Equal(JsonlDesignConversionVisualCostLedger.SchemaVersion, ledgerRecord.SchemaVersion);
        Assert.StartsWith("dcm-visual-cost-ledger-record-", ledgerRecord.RecordId, StringComparison.Ordinal);
        Assert.StartsWith("dcm-visual-cost-ledger-job-", ledgerRecord.EvaluationJobRef, StringComparison.Ordinal);
        Assert.StartsWith("dcm-visual-cost-ledger-request-", ledgerRecord.SubmittedRequestRef, StringComparison.Ordinal);
        Assert.Equal(0.0600m, ledgerRecord.TotalCostUsd);
        Assert.True(ledgerRecord.MeasurementComplete);
        Assert.Equal(0, ledgerRecord.MissingMeasurementCount);
        Assert.Equal(1, ledgerRecord.EvaluationAttemptCount);
        Assert.Equal(
            DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostSourceFixedPerRequest,
            ledgerRecord.CostSource);
        Assert.Equal(DateTimeOffset.UnixEpoch.AddSeconds(3), ledgerRecord.RecordedAtUtc);
        Assert.True(JsonlDesignConversionVisualCostLedger.IsTechnicalUserRef(ledgerRecord.UserRef));
        Assert.Equal(JsonlDesignConversionVisualCostLedger.NormalizeSubmittedTier(request.Budget.Tier), ledgerRecord.SubmittedTier);
        Assert.Equal(JsonlDesignConversionVisualCostLedger.UtcCalendarMonthPeriodKind, ledgerRecord.PeriodKind);
        Assert.Equal(DateTimeOffset.UnixEpoch, ledgerRecord.PeriodStartUtc);
        Assert.Equal(DateTimeOffset.UnixEpoch.AddMonths(1), ledgerRecord.PeriodEndExclusiveUtc);
        Assert.Equal(bool.FalseString, ledgerRecord.Metadata["training_artifact_db"]);
        Assert.Equal(bool.FalseString, ledgerRecord.Metadata["quota_enforcement"]);
        Assert.Equal(bool.FalseString, ledgerRecord.Metadata["monthly_cap_enforcement"]);
        Assert.Equal(bool.FalseString, ledgerRecord.Metadata["raw_payloads_persisted"]);
        Assert.Equal("pseudonymous_technical_scoped_sha256_not_anonymous_not_secret", ledgerRecord.Metadata["user_ref_semantics"]);
        Assert.Equal("submitted_context_only", ledgerRecord.Metadata["tier_authority"]);
        Assert.Equal("record_and_aggregate_only", ledgerRecord.Metadata["ledger_scope"]);
        Assert.All(ledgerRecord.EvidenceRefs, reference =>
            Assert.StartsWith("dcm-visual-cost-ledger-evidence-", reference, StringComparison.Ordinal));

        var duplicateAppend = await ledger.AppendRecordAsync(ledgerRecord);
        Assert.True(duplicateAppend.IsSuccess, duplicateAppend.ErrorMessage);
        AssertSameVisualCostLedgerRecord(ledgerRecord, duplicateAppend.Value!);

        var durableLedger = new JsonlDesignConversionVisualCostLedger(ledgerRoot);
        var durableRead = await durableLedger.ListRecordsAsync();
        Assert.True(durableRead.IsSuccess, durableRead.ErrorMessage);
        var durableRecord = Assert.Single(durableRead.Value!);
        AssertSameVisualCostLedgerRecord(ledgerRecord, durableRecord);

        var ledgerText = await File.ReadAllTextAsync(Path.Combine(ledgerRoot, JsonlDesignConversionVisualCostLedger.RecordsFileName));
        Assert.Single(File.ReadAllLines(Path.Combine(ledgerRoot, JsonlDesignConversionVisualCostLedger.RecordsFileName)));
        Assert.Contains("\"totalCostUsd\":0.0600", ledgerText, StringComparison.Ordinal);
        Assert.Contains("\"measurementComplete\":true", ledgerText, StringComparison.Ordinal);
        Assert.Contains("\"missingMeasurementCount\":0", ledgerText, StringComparison.Ordinal);
        Assert.Contains("\"evaluationAttemptCount\":1", ledgerText, StringComparison.Ordinal);
        Assert.Contains("\"userRef\":\"dcm-visual-cost-ledger-userref-", ledgerText, StringComparison.Ordinal);
        Assert.Contains("\"submittedTier\":\"paid\"", ledgerText, StringComparison.Ordinal);
        Assert.Contains("\"periodKind\":\"utc_calendar_month\"", ledgerText, StringComparison.Ordinal);
        Assert.Contains(DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostSourceFixedPerRequest, ledgerText, StringComparison.Ordinal);
        Assert.DoesNotContain(rawUserIdentityMarker, ledgerText, StringComparison.Ordinal);
        Assert.DoesNotContain(request.Budget.UserId, ledgerText, StringComparison.Ordinal);
        Assert.DoesNotContain("thin-receipt-request-001", ledgerText, StringComparison.Ordinal);
        Assert.DoesNotContain(submitted.Value.JobId, ledgerText, StringComparison.Ordinal);
        Assert.DoesNotContain("<main", ledgerText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(".frame", ledgerText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("FRAME", ledgerText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("billing", ledgerText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("user_id", ledgerText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("userid", ledgerText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("tenant", ledgerText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("account", ledgerText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("identity", ledgerText, StringComparison.OrdinalIgnoreCase);

        var afterReplay = await db.ListRecordsAsync(new DesignConversionLocalArtifactDbQuery(JobId: submitted.Value.JobId));
        Assert.True(afterReplay.IsSuccess, afterReplay.ErrorMessage);
        Assert.Single(afterReplay.Value!);
        var ledgerAfterReplay = await ledger.ListRecordsAsync();
        Assert.True(ledgerAfterReplay.IsSuccess, ledgerAfterReplay.ErrorMessage);
        Assert.Single(ledgerAfterReplay.Value!);
        Assert.Equal(1, visualJudge.InvocationCount);
    }

    [Fact]
    public async Task GetVisualCostRecordsAsync_ReadsDurableRecordByJobIdAfterReplayAndFreshRuntime()
    {
        const string rawUserIdentityMarker = "raw-user-identity-readback-9D73C2";
        var ledgerRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "thin-client-runtime-visual-cost-readback");
        var ledger = new JsonlDesignConversionVisualCostLedger(ledgerRoot);
        var receipt = BuildServerOwnedReceipt("2", "0.0300", "0.0600");
        var visualJudge = new ReceiptVisualJudge(receipt, receipt, estimatedCostUsd: 0.0600m);
        var evaluator = new RecordingEvaluator(CreateVisualJudgeInternalEvaluator(visualJudge));
        var runtime = new DesignConversionThinClientJobRuntime(
            evaluator,
            FixedClock(),
            visualCostLedger: ledger,
            postEvaluationStateStore: CreatePostEvaluationStateStore("visual-cost-readback"));
        var baseRequest = CreatePrivateSubmittedRequest();
        var request = baseRequest with
        {
            RequestId = "thin-readback-request-001",
            Budget = baseRequest.Budget with
            {
                UserId = $"  {rawUserIdentityMarker}  ",
                Tier = "paid",
                MaxRenderQaRuns = 1,
                BudgetUsd = 0.25m
            }
        };
        var submitted = runtime.Submit(request);
        Assert.True(submitted.IsSuccess, submitted.ErrorMessage);

        var firstRun = await runtime.RunAsync(submitted.Value!.JobId);
        var replay = await runtime.RunAsync(submitted.Value.JobId);
        var readback = await runtime.GetVisualCostRecordsAsync(submitted.Value.JobId);

        Assert.True(firstRun.IsSuccess, firstRun.ErrorMessage);
        Assert.True(replay.IsSuccess, replay.ErrorMessage);
        Assert.True(readback.IsSuccess, readback.ErrorMessage);
        AssertThinClientQuarantinedVisualDecision(firstRun.Value!);
        AssertThinClientQuarantinedVisualDecision(replay.Value!);
        Assert.Single(evaluator.Requests);
        Assert.Equal(1, visualJudge.InvocationCount);
        var evaluation = Assert.Single(evaluator.Responses).Value!;
        var record = Assert.Single(readback.Value!);
        Assert.Equal(JsonlDesignConversionVisualCostLedger.BuildCanonicalEvaluationJobRef(submitted.Value.JobId), record.EvaluationJobRef);
        Assert.Equal(JsonlDesignConversionVisualCostLedger.BuildCanonicalSubmittedRequestRef(request.RequestId), record.SubmittedRequestRef);
        Assert.Equal(evaluation.VisualCostAccounting.TotalCostUsd, record.TotalCostUsd);
        Assert.Equal(0.0600m, record.TotalCostUsd);
        Assert.Equal(evaluation.VisualCostAccounting.MeasurementComplete, record.MeasurementComplete);
        Assert.True(record.MeasurementComplete);
        Assert.Equal(evaluation.VisualCostAccounting.MissingMeasurementCount, record.MissingMeasurementCount);
        Assert.Equal(0, record.MissingMeasurementCount);
        Assert.Equal(evaluation.VisualCostAccounting.EvaluationAttemptCount, record.EvaluationAttemptCount);
        Assert.Equal(1, record.EvaluationAttemptCount);
        Assert.Equal(evaluation.VisualCostAccounting.CostSource, record.CostSource);
        Assert.Equal(DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostSourceFixedPerRequest, record.CostSource);
        Assert.True(JsonlDesignConversionVisualCostLedger.IsTechnicalUserRef(record.UserRef));
        Assert.Equal(JsonlDesignConversionVisualCostLedger.NormalizeSubmittedTier(request.Budget.Tier), record.SubmittedTier);
        Assert.Equal(JsonlDesignConversionVisualCostLedger.UtcCalendarMonthPeriodKind, record.PeriodKind);
        Assert.Equal(DateTimeOffset.UnixEpoch, record.PeriodStartUtc);
        Assert.Equal(DateTimeOffset.UnixEpoch.AddMonths(1), record.PeriodEndExclusiveUtc);

        var afterReplay = await runtime.GetVisualCostRecordsAsync(submitted.Value.JobId);
        Assert.True(afterReplay.IsSuccess, afterReplay.ErrorMessage);
        Assert.Single(afterReplay.Value!);

        var freshRuntime = new DesignConversionThinClientJobRuntime(
            CreateDeterministicInternalEvaluator(),
            FixedClock(),
            visualCostLedger: new JsonlDesignConversionVisualCostLedger(ledgerRoot),
            postEvaluationStateStore: CreatePostEvaluationStateStore("visual-cost-readback-fresh"));
        var freshReadback = await freshRuntime.GetVisualCostRecordsAsync(submitted.Value.JobId);
        Assert.True(freshReadback.IsSuccess, freshReadback.ErrorMessage);
        var freshRecord = Assert.Single(freshReadback.Value!);
        AssertSameVisualCostLedgerRecord(record, freshRecord);

        var unknown = await freshRuntime.GetVisualCostRecordsAsync("thin-job-unknown-readback");
        Assert.True(unknown.IsSuccess, unknown.ErrorMessage);
        Assert.Empty(unknown.Value!);

        var serializedReadback = JsonSerializer.Serialize(readback.Value!);
        Assert.DoesNotContain(rawUserIdentityMarker, serializedReadback, StringComparison.Ordinal);
        Assert.DoesNotContain(request.Budget.UserId, serializedReadback, StringComparison.Ordinal);
        Assert.DoesNotContain(request.Budget.UserId.Trim(), serializedReadback, StringComparison.Ordinal);
        Assert.DoesNotContain(submitted.Value.JobId, serializedReadback, StringComparison.Ordinal);
        Assert.DoesNotContain(request.RequestId, serializedReadback, StringComparison.Ordinal);
        Assert.DoesNotContain(request.Html, serializedReadback, StringComparison.Ordinal);
        Assert.DoesNotContain(request.Css, serializedReadback, StringComparison.Ordinal);
        Assert.DoesNotContain("raw-tree-private", serializedReadback, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("C:/Users/lubao", serializedReadback, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("exact_prompt", serializedReadback, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("external-provider-payload", serializedReadback, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("billing", serializedReadback, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("tenant", serializedReadback, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("account", serializedReadback, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("identity", serializedReadback, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RunAsync_VisualCostLedgerDerivesStableTechnicalUserRefsWithoutRawUserIdentity()
    {
        const string firstRawUserId = "raw-user-monthly-alpha-6E88";
        const string secondRawUserId = "raw-user-monthly-beta-4AA1";
        var ledgerRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "thin-client-runtime-visual-cost-user-ref");
        var ledger = new JsonlDesignConversionVisualCostLedger(ledgerRoot);
        var receipt = BuildServerOwnedReceipt("1", "0.0100", "0.0100");
        var visualJudge = new ReceiptVisualJudge(receipt, receipt, estimatedCostUsd: 0.0100m);
        var evaluator = new RecordingEvaluator(CreateVisualJudgeInternalEvaluator(visualJudge));
        var runtime = new DesignConversionThinClientJobRuntime(
            evaluator,
            FixedClock(),
            visualCostLedger: ledger,
            postEvaluationStateStore: CreatePostEvaluationStateStore("visual-cost-user-ref"));
        var requests = new[]
        {
            CreateSubmittedRequest() with
            {
                RequestId = "thin-user-ref-request-a",
                Budget = CreateSubmittedRequest().Budget with
                {
                    UserId = $"  {firstRawUserId}  ",
                    Tier = "PAID",
                    MaxRenderQaRuns = 1
                }
            },
            CreateSubmittedRequest() with
            {
                RequestId = "thin-user-ref-request-b",
                Budget = CreateSubmittedRequest().Budget with
                {
                    UserId = firstRawUserId,
                    Tier = " paid ",
                    MaxRenderQaRuns = 1
                }
            },
            CreateSubmittedRequest() with
            {
                RequestId = "thin-user-ref-request-c",
                Budget = CreateSubmittedRequest().Budget with
                {
                    UserId = secondRawUserId,
                    Tier = "pro",
                    MaxRenderQaRuns = 1
                }
            }
        };

        foreach (var request in requests)
        {
            var submitted = runtime.Submit(request);
            Assert.True(submitted.IsSuccess, submitted.ErrorMessage);
            var run = await runtime.RunAsync(submitted.Value!.JobId);
            Assert.True(run.IsSuccess, run.ErrorMessage);
        }

        var read = await ledger.ListRecordsAsync();
        Assert.True(read.IsSuccess, read.ErrorMessage);
        Assert.Equal(3, read.Value!.Count);
        var recordsByRequestRef = read.Value.ToDictionary(record => record.SubmittedRequestRef, StringComparer.Ordinal);
        var firstRecord = recordsByRequestRef[
            JsonlDesignConversionVisualCostLedger.BuildCanonicalSubmittedRequestRef("thin-user-ref-request-a")];
        var sameUserRecord = recordsByRequestRef[
            JsonlDesignConversionVisualCostLedger.BuildCanonicalSubmittedRequestRef("thin-user-ref-request-b")];
        var differentUserRecord = recordsByRequestRef[
            JsonlDesignConversionVisualCostLedger.BuildCanonicalSubmittedRequestRef("thin-user-ref-request-c")];
        Assert.Equal(firstRecord.UserRef, sameUserRecord.UserRef);
        Assert.NotEqual(firstRecord.UserRef, differentUserRecord.UserRef);
        Assert.Equal([1, 2], read.Value.GroupBy(record => record.UserRef, StringComparer.Ordinal).Select(group => group.Count()).Order().ToArray());
        var records = read.Value.ToArray();
        Assert.Equal(["paid", "paid", "pro"], records.Select(record => record.SubmittedTier).Order(StringComparer.Ordinal).ToArray());
        Assert.All(records, record =>
        {
            Assert.True(JsonlDesignConversionVisualCostLedger.IsTechnicalUserRef(record.UserRef));
            Assert.Equal(JsonlDesignConversionVisualCostLedger.UtcCalendarMonthPeriodKind, record.PeriodKind);
            Assert.Equal(TimeSpan.Zero, record.PeriodStartUtc.Offset);
            Assert.Equal(TimeSpan.Zero, record.PeriodEndExclusiveUtc.Offset);
            Assert.Equal(record.PeriodStartUtc.AddMonths(1), record.PeriodEndExclusiveUtc);
        });

        var durableJson = await File.ReadAllTextAsync(Path.Combine(ledgerRoot, JsonlDesignConversionVisualCostLedger.RecordsFileName));
        var serializedReadback = JsonSerializer.Serialize(read.Value);
        foreach (var rawUserId in new[] { firstRawUserId, secondRawUserId })
        {
            Assert.DoesNotContain(rawUserId, durableJson, StringComparison.Ordinal);
            Assert.DoesNotContain(rawUserId, serializedReadback, StringComparison.Ordinal);
        }
        Assert.DoesNotContain("user_id", durableJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("userid", durableJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("billing", durableJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("tenant", durableJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("identity", durableJson, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void VisualCostLedger_UserRefTrimsDeterministicallyAndRejectsEmptySource()
    {
        var trimmed = JsonlDesignConversionVisualCostLedger.BuildPseudonymousUserRef("trimmed-user-ref-source");
        var padded = JsonlDesignConversionVisualCostLedger.BuildPseudonymousUserRef("  trimmed-user-ref-source  ");
        var different = JsonlDesignConversionVisualCostLedger.BuildPseudonymousUserRef("different-user-ref-source");
        var empty = JsonlDesignConversionVisualCostLedger.BuildPseudonymousUserRef("   ");

        Assert.True(trimmed.IsSuccess, trimmed.ErrorMessage);
        Assert.True(padded.IsSuccess, padded.ErrorMessage);
        Assert.True(different.IsSuccess, different.ErrorMessage);
        Assert.Equal(trimmed.Value, padded.Value);
        Assert.NotEqual(trimmed.Value, different.Value);
        Assert.True(JsonlDesignConversionVisualCostLedger.IsTechnicalUserRef(trimmed.Value));
        Assert.False(empty.IsSuccess);
        Assert.Equal(JsonlDesignConversionVisualCostLedger.UserRefSourceRequiredErrorCode, empty.ErrorCode);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Submit_RejectsBlankBudgetUserIdBeforeStoringJob(string blankUserId)
    {
        var evaluator = new RecordingEvaluator(CreateDeterministicInternalEvaluator());
        var runtime = new DesignConversionThinClientJobRuntime(evaluator, FixedClock());
        var request = CreateSubmittedRequest() with
        {
            Budget = CreateSubmittedRequest().Budget with
            {
                UserId = blankUserId
            }
        };

        var submitted = runtime.Submit(request);
        var status = runtime.GetStatus("thin-job-thin-request-001");

        Assert.False(submitted.IsSuccess);
        Assert.Equal("USER_ID_REQUIRED", submitted.ErrorCode);
        Assert.Equal("DesignConversion budget user id is required.", submitted.ErrorMessage);
        Assert.Empty(evaluator.Requests);
        Assert.False(status.IsSuccess);
        Assert.Equal("JOB_NOT_FOUND", status.ErrorCode);
    }

    [Fact]
    public void VisualCostLedger_PeriodUsesRecordedTimestampUtcCalendarMonth()
    {
        var decemberPeriod = JsonlDesignConversionVisualCostLedger.BuildUtcCalendarMonthPeriod(
            new DateTimeOffset(2025, 12, 31, 23, 59, 59, TimeSpan.Zero));
        var nonUtcCrossesIntoJanuary = JsonlDesignConversionVisualCostLedger.BuildUtcCalendarMonthPeriod(
            new DateTimeOffset(2025, 12, 31, 23, 30, 0, TimeSpan.FromHours(-5)));

        Assert.Equal(new DateTimeOffset(2025, 12, 1, 0, 0, 0, TimeSpan.Zero), decemberPeriod.StartUtc);
        Assert.Equal(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero), decemberPeriod.EndExclusiveUtc);
        Assert.Equal(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero), nonUtcCrossesIntoJanuary.StartUtc);
        Assert.Equal(new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero), nonUtcCrossesIntoJanuary.EndExclusiveUtc);
    }

    [Fact]
    public async Task RunAsync_VisualCostLedgerUsesRuntimeUtcRecordedAtForCrossYearMonthNotCallerMetadata()
    {
        var ledgerRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "thin-client-runtime-visual-cost-cross-year");
        var ledger = new JsonlDesignConversionVisualCostLedger(ledgerRoot);
        var receipt = BuildServerOwnedReceipt("1", "0.0100", "0.0100");
        var visualJudge = new ReceiptVisualJudge(receipt, receipt, estimatedCostUsd: 0.0100m);
        var evaluator = new RecordingEvaluator(CreateVisualJudgeInternalEvaluator(visualJudge));
        var clockInstants = new Queue<DateTimeOffset>([
            new DateTimeOffset(2025, 12, 31, 20, 0, 0, TimeSpan.FromHours(-5)),
            new DateTimeOffset(2025, 12, 31, 21, 0, 0, TimeSpan.FromHours(-5)),
            new DateTimeOffset(2025, 12, 31, 22, 0, 0, TimeSpan.FromHours(-5)),
            new DateTimeOffset(2025, 12, 31, 23, 30, 0, TimeSpan.FromHours(-5))
        ]);
        var lastInstant = clockInstants.Peek();
        DateTimeOffset NonUtcClock()
        {
            if (clockInstants.Count > 0)
                lastInstant = clockInstants.Dequeue();
            return lastInstant;
        }

        var runtime = new DesignConversionThinClientJobRuntime(
            evaluator,
            NonUtcClock,
            visualCostLedger: ledger,
            postEvaluationStateStore: CreatePostEvaluationStateStore("visual-cost-cross-year"));
        var baseRequest = CreatePrivateSubmittedRequest();
        var provenance = baseRequest.Provenance.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        provenance["visual_cost_ledger_period_start_utc"] = "2025-12-01T00:00:00.0000000+00:00";
        var request = baseRequest with
        {
            RequestId = "thin-cross-year-period-request",
            Budget = baseRequest.Budget with
            {
                UserId = "cross-year-runtime-user",
                Tier = "paid",
                MaxRenderQaRuns = 1,
                Metadata = new Dictionary<string, string>
                {
                    ["visual_cost_ledger_period_kind"] = "caller_controlled_period",
                    ["visual_cost_ledger_period_start_utc"] = "2025-12-01T00:00:00.0000000+00:00",
                    ["visual_cost_ledger_period_end_exclusive_utc"] = "2026-01-01T00:00:00.0000000+00:00"
                }
            },
            Provenance = provenance
        };
        var submitted = runtime.Submit(request);
        Assert.True(submitted.IsSuccess, submitted.ErrorMessage);

        var run = await runtime.RunAsync(submitted.Value!.JobId);
        var read = await ledger.ListRecordsAsync();

        Assert.True(run.IsSuccess, run.ErrorMessage);
        Assert.True(read.IsSuccess, read.ErrorMessage);
        var record = Assert.Single(read.Value!);
        Assert.Equal(new DateTimeOffset(2026, 1, 1, 4, 30, 0, TimeSpan.Zero), record.RecordedAtUtc);
        Assert.Equal(TimeSpan.Zero, record.RecordedAtUtc.Offset);
        Assert.Equal(JsonlDesignConversionVisualCostLedger.UtcCalendarMonthPeriodKind, record.PeriodKind);
        Assert.Equal(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero), record.PeriodStartUtc);
        Assert.Equal(new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero), record.PeriodEndExclusiveUtc);
        Assert.Equal(record.UserRef, run.Value!.Job.Metadata["visual_cost_ledger_user_ref"]);
        Assert.Equal("paid", run.Value.Job.Metadata["visual_cost_ledger_submitted_tier"]);
        Assert.Equal(JsonlDesignConversionVisualCostLedger.UtcCalendarMonthPeriodKind, run.Value.Job.Metadata["visual_cost_ledger_period_kind"]);
        Assert.Equal(record.PeriodStartUtc.ToString("O", CultureInfo.InvariantCulture), run.Value.Job.Metadata["visual_cost_ledger_period_start_utc"]);
        Assert.Equal(record.PeriodEndExclusiveUtc.ToString("O", CultureInfo.InvariantCulture), run.Value.Job.Metadata["visual_cost_ledger_period_end_exclusive_utc"]);
    }

    [Fact]
    public async Task VisualCostLedger_AggregatePeriodSumsExactUserMonthAndSurfacesIncompleteMeasurements()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "visual-cost-ledger-aggregate");
        var ledger = new JsonlDesignConversionVisualCostLedger(root);
        var userRef = JsonlDesignConversionVisualCostLedger.BuildPseudonymousUserRef("aggregate-user-a").Value!;
        var otherUserRef = JsonlDesignConversionVisualCostLedger.BuildPseudonymousUserRef("aggregate-user-b").Value!;
        var period = JsonlDesignConversionVisualCostLedger.BuildUtcCalendarMonthPeriod(
            new DateTimeOffset(2026, 3, 14, 12, 0, 0, TimeSpan.Zero));
        var nextPeriod = JsonlDesignConversionVisualCostLedger.BuildUtcCalendarMonthPeriod(period.EndExclusiveUtc);
        var records = new[]
        {
            CreateVisualCostLedgerRecordForTest(
                "thin-job-aggregate-a",
                "thin-request-aggregate-a",
                0.0400m,
                period.StartUtc.AddDays(1),
                userRef: userRef,
                submittedTier: "paid",
                periodStartUtc: period.StartUtc,
                periodEndExclusiveUtc: period.EndExclusiveUtc),
            CreateVisualCostLedgerRecordForTest(
                "thin-job-aggregate-b",
                "thin-request-aggregate-b",
                0.0200m,
                period.StartUtc.AddDays(2),
                userRef: userRef,
                submittedTier: "paid",
                periodStartUtc: period.StartUtc,
                periodEndExclusiveUtc: period.EndExclusiveUtc,
                measurementComplete: false,
                missingMeasurementCount: 1),
            CreateVisualCostLedgerRecordForTest(
                "thin-job-aggregate-other-user",
                "thin-request-aggregate-other-user",
                0.5000m,
                period.StartUtc.AddDays(3),
                userRef: otherUserRef,
                submittedTier: "paid",
                periodStartUtc: period.StartUtc,
                periodEndExclusiveUtc: period.EndExclusiveUtc),
            CreateVisualCostLedgerRecordForTest(
                "thin-job-aggregate-other-month",
                "thin-request-aggregate-other-month",
                0.6000m,
                nextPeriod.StartUtc.AddDays(1),
                userRef: userRef,
                submittedTier: "paid",
                periodStartUtc: nextPeriod.StartUtc,
                periodEndExclusiveUtc: nextPeriod.EndExclusiveUtc)
        };
        foreach (var record in records)
        {
            var append = await ledger.AppendRecordAsync(record);
            Assert.True(append.IsSuccess, append.ErrorMessage);
        }

        var aggregate = await ledger.AggregatePeriodAsync(userRef, period.StartUtc, period.EndExclusiveUtc);
        var emptyUserRef = JsonlDesignConversionVisualCostLedger.BuildPseudonymousUserRef("aggregate-empty-user").Value!;
        var emptyAggregate = await ledger.AggregatePeriodAsync(emptyUserRef, period.StartUtc, period.EndExclusiveUtc);
        var invalidUser = await ledger.AggregatePeriodAsync("raw-user-ref", period.StartUtc, period.EndExclusiveUtc);
        var invalidPeriod = await ledger.AggregatePeriodAsync(userRef, period.StartUtc.AddDays(1), period.EndExclusiveUtc);

        Assert.True(aggregate.IsSuccess, aggregate.ErrorMessage);
        Assert.Equal(userRef, aggregate.Value!.UserRef);
        Assert.Equal(JsonlDesignConversionVisualCostLedger.UtcCalendarMonthPeriodKind, aggregate.Value.PeriodKind);
        Assert.Equal(period.StartUtc, aggregate.Value.PeriodStartUtc);
        Assert.Equal(period.EndExclusiveUtc, aggregate.Value.PeriodEndExclusiveUtc);
        Assert.Equal(0.0600m, aggregate.Value.TotalMeasuredCostUsd);
        Assert.Equal(2, aggregate.Value.RecordCount);
        Assert.False(aggregate.Value.MeasurementComplete);
        Assert.True(aggregate.Value.HasMissingMeasurements);
        Assert.Equal(1, aggregate.Value.MissingMeasurementCount);
        Assert.Equal(1, aggregate.Value.IncompleteRecordCount);
        Assert.True(emptyAggregate.IsSuccess, emptyAggregate.ErrorMessage);
        Assert.Equal(0, emptyAggregate.Value!.RecordCount);
        Assert.Equal(0m, emptyAggregate.Value.TotalMeasuredCostUsd);
        Assert.False(emptyAggregate.Value.MeasurementComplete);
        Assert.False(emptyAggregate.Value.HasMissingMeasurements);
        Assert.False(invalidUser.IsSuccess);
        Assert.Equal(JsonlDesignConversionVisualCostLedger.InvalidUserRefErrorCode, invalidUser.ErrorCode);
        Assert.False(invalidPeriod.IsSuccess);
        Assert.Equal(JsonlDesignConversionVisualCostLedger.InvalidPeriodErrorCode, invalidPeriod.ErrorCode);
    }

    [Fact]
    public async Task VisualCostLedger_DuplicateAppendIsIdempotentButSameRecordIdDifferentUserRefFails()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "visual-cost-ledger-idempotency-conflict");
        var ledger = new JsonlDesignConversionVisualCostLedger(root);
        var userRef = JsonlDesignConversionVisualCostLedger.BuildPseudonymousUserRef("idempotent-user").Value!;
        var otherUserRef = JsonlDesignConversionVisualCostLedger.BuildPseudonymousUserRef("conflicting-user").Value!;
        var record = CreateVisualCostLedgerRecordForTest(
            "thin-job-idempotency-conflict",
            "thin-request-idempotency-conflict",
            0.0900m,
            DateTimeOffset.UnixEpoch.AddDays(3),
            userRef: userRef,
            submittedTier: "paid");

        var first = await ledger.AppendRecordAsync(record);
        var exactDuplicate = await ledger.AppendRecordAsync(record);
        var changedUserRef = await ledger.AppendRecordAsync(record with { UserRef = otherUserRef });

        Assert.True(first.IsSuccess, first.ErrorMessage);
        Assert.True(exactDuplicate.IsSuccess, exactDuplicate.ErrorMessage);
        AssertSameVisualCostLedgerRecord(first.Value!, exactDuplicate.Value!);
        Assert.False(changedUserRef.IsSuccess);
        Assert.Equal(JsonlDesignConversionVisualCostLedger.InvalidRecordErrorCode, changedUserRef.ErrorCode);
        Assert.Single(File.ReadAllLines(Path.Combine(root, JsonlDesignConversionVisualCostLedger.RecordsFileName)));
    }

    [Fact]
    public async Task VisualCostLedger_LegacyRecordReadsUnattributedAndIsExcludedFromUserPeriodAggregate()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "visual-cost-ledger-legacy");
        Directory.CreateDirectory(root);
        var ledger = new JsonlDesignConversionVisualCostLedger(root);
        var legacy = CreateVisualCostLedgerRecordForTest(
            "thin-job-legacy-ledger",
            "thin-request-legacy-ledger",
            0.0700m,
            DateTimeOffset.UnixEpoch.AddDays(1));
        await File.WriteAllTextAsync(
            Path.Combine(root, JsonlDesignConversionVisualCostLedger.RecordsFileName),
            CreatePreChangeVisualCostLedgerJsonLineForTest(legacy) + Environment.NewLine);
        var userRef = JsonlDesignConversionVisualCostLedger.BuildPseudonymousUserRef("legacy-query-user").Value!;
        var period = JsonlDesignConversionVisualCostLedger.BuildUtcCalendarMonthPeriod(DateTimeOffset.UnixEpoch.AddDays(1));

        var read = await ledger.ListRecordsAsync();
        var aggregate = await ledger.AggregatePeriodAsync(userRef, period.StartUtc, period.EndExclusiveUtc);

        Assert.True(read.IsSuccess, read.ErrorMessage);
        var record = Assert.Single(read.Value!);
        Assert.Equal(JsonlDesignConversionVisualCostLedger.LegacyUnattributedUserRef, record.UserRef);
        Assert.Equal(JsonlDesignConversionVisualCostLedger.NotPeriodEnforceablePeriodKind, record.PeriodKind);
        Assert.Equal(DateTimeOffset.UnixEpoch, record.PeriodStartUtc);
        Assert.Equal(DateTimeOffset.UnixEpoch, record.PeriodEndExclusiveUtc);
        Assert.True(aggregate.IsSuccess, aggregate.ErrorMessage);
        Assert.Equal(0, aggregate.Value!.RecordCount);
        Assert.Equal(0m, aggregate.Value.TotalMeasuredCostUsd);
    }

    [Fact]
    public async Task VisualCostLedger_MalformedNonLegacyDimensionsOnDiskFailRead()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "visual-cost-ledger-malformed-dimensions");
        Directory.CreateDirectory(root);
        var ledger = new JsonlDesignConversionVisualCostLedger(root);
        var record = CreateVisualCostLedgerRecordForTest(
            "thin-job-malformed-dimensions",
            "thin-request-malformed-dimensions",
            0.0800m,
            DateTimeOffset.UnixEpoch.AddDays(2));
        await File.WriteAllTextAsync(
            Path.Combine(root, JsonlDesignConversionVisualCostLedger.RecordsFileName),
            CreatePreChangeVisualCostLedgerJsonLineForTest(
                record,
                userRef: "not-a-canonical-user-ref",
                periodKind: JsonlDesignConversionVisualCostLedger.UtcCalendarMonthPeriodKind,
                periodStartUtc: DateTimeOffset.UnixEpoch,
                periodEndExclusiveUtc: DateTimeOffset.UnixEpoch.AddMonths(1)) + Environment.NewLine);

        var read = await ledger.ListRecordsAsync();

        Assert.False(read.IsSuccess);
        Assert.Equal(JsonlDesignConversionVisualCostLedger.ReadFailedErrorCode, read.ErrorCode);
    }

    [Fact]
    public async Task GetVisualCostRecordsAsync_FiltersExactJobRefAndOrdersByRecordedAtThenRecordId()
    {
        const string targetJobId = "thin-job-visual-cost-ordering";
        var ledgerRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "thin-client-runtime-visual-cost-ordering");
        var ledger = new JsonlDesignConversionVisualCostLedger(ledgerRoot);
        var targetRecords = new[]
        {
            CreateVisualCostLedgerRecordForTest(targetJobId, "ordering-request-late", 0.0400m, DateTimeOffset.UnixEpoch.AddSeconds(20)),
            CreateVisualCostLedgerRecordForTest(targetJobId, "ordering-request-tie-b", 0.0200m, DateTimeOffset.UnixEpoch.AddSeconds(10)),
            CreateVisualCostLedgerRecordForTest(targetJobId, "ordering-request-tie-a", 0.0100m, DateTimeOffset.UnixEpoch.AddSeconds(10))
        };
        var otherJobRecord = CreateVisualCostLedgerRecordForTest(
            "thin-job-visual-cost-other",
            "ordering-request-other",
            0.0900m,
            DateTimeOffset.UnixEpoch.AddSeconds(1));
        var appendedTargetRecords = new List<DesignConversionVisualCostLedgerRecord>();
        foreach (var targetRecord in targetRecords)
        {
            var append = await ledger.AppendRecordAsync(targetRecord);
            Assert.True(append.IsSuccess, append.ErrorMessage);
            appendedTargetRecords.Add(append.Value!);
        }

        var otherAppend = await ledger.AppendRecordAsync(otherJobRecord);
        Assert.True(otherAppend.IsSuccess, otherAppend.ErrorMessage);
        var runtime = new DesignConversionThinClientJobRuntime(
            CreateDeterministicInternalEvaluator(),
            FixedClock(),
            visualCostLedger: ledger);

        var readback = await runtime.GetVisualCostRecordsAsync(targetJobId);

        Assert.True(readback.IsSuccess, readback.ErrorMessage);
        var expectedIds = appendedTargetRecords
            .OrderBy(record => record.RecordedAtUtc)
            .ThenBy(record => record.RecordId, StringComparer.Ordinal)
            .Select(record => record.RecordId)
            .ToArray();
        var actualIds = readback.Value!.Select(record => record.RecordId).ToArray();
        Assert.Equal(expectedIds, actualIds);
        Assert.DoesNotContain(otherAppend.Value!.RecordId, actualIds);
    }

    [Fact]
    public async Task GetVisualCostRecordsAsync_ReturnsStableFailuresForBlankCancelledAndCorruptLedger()
    {
        var ledgerRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "thin-client-runtime-visual-cost-failures");
        var runtime = new DesignConversionThinClientJobRuntime(
            CreateDeterministicInternalEvaluator(),
            FixedClock(),
            visualCostLedger: new JsonlDesignConversionVisualCostLedger(ledgerRoot));

        var blank = await runtime.GetVisualCostRecordsAsync("   ");
        using var cancelledSource = new CancellationTokenSource();
        cancelledSource.Cancel();
        var cancelled = await runtime.GetVisualCostRecordsAsync("thin-job-cancelled-readback", cancelledSource.Token);

        var corruptRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "thin-client-runtime-visual-cost-corrupt");
        Directory.CreateDirectory(corruptRoot);
        await File.WriteAllTextAsync(
            Path.Combine(corruptRoot, JsonlDesignConversionVisualCostLedger.RecordsFileName),
            "{ not-json");
        var corruptRuntime = new DesignConversionThinClientJobRuntime(
            CreateDeterministicInternalEvaluator(),
            FixedClock(),
            visualCostLedger: new JsonlDesignConversionVisualCostLedger(corruptRoot));
        var corrupt = await corruptRuntime.GetVisualCostRecordsAsync("thin-job-corrupt-readback");

        Assert.False(blank.IsSuccess);
        Assert.Equal("JOB_ID_REQUIRED", blank.ErrorCode);
        Assert.Equal("DesignConversion thin-client job id is required.", blank.ErrorMessage);
        Assert.False(cancelled.IsSuccess);
        Assert.Equal("CANCELLED", cancelled.ErrorCode);
        Assert.Equal(DesignConversionThinClientJobRuntime.VisualCostRecordsCancelledSafeMessage, cancelled.ErrorMessage);
        Assert.False(corrupt.IsSuccess);
        Assert.Equal(DesignConversionThinClientJobRuntime.VisualCostRecordsReadFailedErrorCode, corrupt.ErrorCode);
        Assert.Equal(DesignConversionThinClientJobRuntime.VisualCostRecordsReadFailedSafeMessage, corrupt.ErrorMessage);
        Assert.DoesNotContain(corruptRoot, corrupt.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(JsonlDesignConversionVisualCostLedger.RecordsFileName, corrupt.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("thin-job-corrupt-readback", corrupt.ErrorMessage, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RunAsync_WhenVisualCostLedgerWriteFailsAfterEvaluationFailsClosedWithoutCompletedStatus()
    {
        var visualJudge = new ReceiptVisualJudge(
            BuildServerOwnedReceipt("1", "0.0300", "0.0300"),
            BuildServerOwnedReceipt("1", "0.0300", "0.0300"),
            estimatedCostUsd: 0.0300m);
        var evaluator = new RecordingEvaluator(CreateVisualJudgeInternalEvaluator(visualJudge));
        var ledger = new FailingVisualCostLedger("scripted visual-cost ledger failure", "SCRIPTED_VISUAL_COST_LEDGER_FAILURE");
        var runtime = new DesignConversionThinClientJobRuntime(
            evaluator,
            FixedClock(),
            artifactDb: null,
            visualCostLedger: ledger,
            postEvaluationStateStore: CreatePostEvaluationStateStore("visual-cost-ledger-failure"));
        var request = CreateSubmittedRequest() with
        {
            RequestId = "thin-ledger-failure-request-001",
            Budget = CreateSubmittedRequest().Budget with
            {
                Tier = "paid",
                MaxRenderQaRuns = 1
            }
        };
        var submitted = runtime.Submit(request);
        Assert.True(submitted.IsSuccess, submitted.ErrorMessage);

        var result = await runtime.RunAsync(submitted.Value!.JobId);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Single(evaluator.Requests);
        Assert.Single(ledger.AttemptedRecords);
        Assert.Equal(1, visualJudge.InvocationCount);
        Assert.Equal(DesignConversionThinClientStatus.Failed, result.Value!.ContractStatus);
        Assert.Equal(DesignConversionThinClientStatus.Failed, result.Value.Job.Status);
        Assert.Empty(result.Value.Artifacts);
        Assert.Equal("scripted_visual_cost_ledger_failure", result.Value.Job.Metadata["error_code"]);
        Assert.Equal(bool.FalseString, result.Value.Job.Metadata["visual_cost_ledger_persistence_verified"]);
        Assert.Equal(bool.TrueString, result.Value.Job.Metadata["visual_cost_ledger_persistence_failed"]);
        Assert.Equal("failed_closed", result.Value.Job.Metadata["visual_cost_ledger_persistence_status"]);
        Assert.Equal(bool.FalseString, result.Value.Job.Metadata["completed_without_visual_cost_accounting"]);
        Assert.Contains("visual_cost_ledger_persistence_failed=true", result.Value.Warnings);
        Assert.Contains("visual_cost_ledger_persistence_status=failed_closed", result.Value.Warnings);
        Assert.Contains("completed_without_visual_cost_accounting=false", result.Value.Warnings);

        var stored = runtime.GetStatus(submitted.Value.JobId);
        Assert.True(stored.IsSuccess, stored.ErrorMessage);
        Assert.Equal(DesignConversionThinClientStatus.Failed, stored.Value!.ContractStatus);
        Assert.Equal(bool.TrueString, stored.Value.Job.Metadata["visual_cost_ledger_persistence_failed"]);
    }

    [Fact]
    public async Task VisualCostLedger_RejectsRawRefsAndCanonicalizesCallerControlledFields()
    {
        const string hostileMarker = "raw-user-identity-visual-ledger-5C8A";
        var rejectedRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "visual-cost-ledger-rejected");
        var rejectedLedger = new JsonlDesignConversionVisualCostLedger(rejectedRoot);
        var rejected = await rejectedLedger.AppendRecordAsync(new DesignConversionVisualCostLedgerRecord(
            RecordId: $"attacker-record-{hostileMarker}",
            LedgerKind: "attacker-ledger-kind",
            SchemaVersion: "attacker-schema",
            EvaluationJobRef: $"thin-job-{hostileMarker}",
            SubmittedRequestRef: $"thin-request-{hostileMarker}",
            TotalCostUsd: 0.0100m,
            MeasurementComplete: true,
            MissingMeasurementCount: 0,
            EvaluationAttemptCount: 1,
            CostSource: "server_owned_fixed_per_request",
            RecordedAtUtc: DateTimeOffset.UnixEpoch,
            EvidenceRefs: [$"{JsonlDesignConversionVisualCostLedger.EvidenceRefPrefix}-{hostileMarker}"],
            Metadata: new Dictionary<string, string>
            {
                ["billing_user_id"] = hostileMarker,
                ["raw_html"] = "<main>private</main>"
            }));

        Assert.False(rejected.IsSuccess);
        Assert.Equal(JsonlDesignConversionVisualCostLedger.InvalidRecordErrorCode, rejected.ErrorCode);
        Assert.False(File.Exists(Path.Combine(rejectedRoot, JsonlDesignConversionVisualCostLedger.RecordsFileName)));
        var rejectedRead = await rejectedLedger.ListRecordsAsync();
        Assert.True(rejectedRead.IsSuccess, rejectedRead.ErrorMessage);
        Assert.Empty(rejectedRead.Value!);

        var acceptedRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "visual-cost-ledger-canonical");
        var acceptedLedger = new JsonlDesignConversionVisualCostLedger(acceptedRoot);
        var jobRef = JsonlDesignConversionVisualCostLedger.BuildCanonicalEvaluationJobRef("thin-job-neutral-technical-ledger-001");
        var requestRef = JsonlDesignConversionVisualCostLedger.BuildCanonicalSubmittedRequestRef("thin-request-neutral-technical-ledger-001");
        var acceptedUserRef = JsonlDesignConversionVisualCostLedger.BuildPseudonymousUserRef("canonical-ledger-user").Value!;
        var changedAccountingUserRef = JsonlDesignConversionVisualCostLedger.BuildPseudonymousUserRef("canonical-ledger-user-changed-accounting").Value!;
        var acceptedRecordedAt = DateTimeOffset.UnixEpoch.AddSeconds(10);
        var acceptedPeriod = JsonlDesignConversionVisualCostLedger.BuildUtcCalendarMonthPeriod(acceptedRecordedAt);
        var changedAccountingRecordedAt = DateTimeOffset.UnixEpoch.AddSeconds(11);
        var changedAccountingPeriod = JsonlDesignConversionVisualCostLedger.BuildUtcCalendarMonthPeriod(changedAccountingRecordedAt);
        var accepted = await acceptedLedger.AppendRecordAsync(new DesignConversionVisualCostLedgerRecord(
            RecordId: $"attacker-record-{hostileMarker}",
            LedgerKind: "attacker-ledger-kind",
            SchemaVersion: "attacker-schema",
            EvaluationJobRef: jobRef,
            SubmittedRequestRef: requestRef,
            TotalCostUsd: 0.0400m,
            MeasurementComplete: true,
            MissingMeasurementCount: 0,
            EvaluationAttemptCount: 1,
            CostSource: $"untrusted-cost-source-{hostileMarker}",
            RecordedAtUtc: acceptedRecordedAt,
            EvidenceRefs:
            [
                $"{JsonlDesignConversionVisualCostLedger.EvidenceRefPrefix}-{new string('a', 64)}",
                $"raw-evidence-{hostileMarker}"
            ],
            Metadata: new Dictionary<string, string>
            {
                ["billing_user_id"] = hostileMarker,
                ["tenant"] = "tenant-secret",
                ["raw_html"] = "<main>private</main>",
                ["safe_looking_key"] = hostileMarker
            }) with
            {
                UserRef = acceptedUserRef,
                SubmittedTier = hostileMarker,
                PeriodKind = JsonlDesignConversionVisualCostLedger.UtcCalendarMonthPeriodKind,
                PeriodStartUtc = acceptedPeriod.StartUtc,
                PeriodEndExclusiveUtc = acceptedPeriod.EndExclusiveUtc
            });
        var changedAccounting = await acceptedLedger.AppendRecordAsync(new DesignConversionVisualCostLedgerRecord(
            RecordId: $"attacker-record-changed-{hostileMarker}",
            LedgerKind: "attacker-ledger-kind",
            SchemaVersion: "attacker-schema",
            EvaluationJobRef: jobRef,
            SubmittedRequestRef: requestRef,
            TotalCostUsd: 0.0500m,
            MeasurementComplete: true,
            MissingMeasurementCount: 0,
            EvaluationAttemptCount: 1,
            CostSource: DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostSourceFixedPerRequest,
            RecordedAtUtc: changedAccountingRecordedAt,
            EvidenceRefs: [$"raw-evidence-{hostileMarker}"],
            Metadata: new Dictionary<string, string>
            {
                ["billing_user_id"] = hostileMarker
            }) with
            {
                UserRef = changedAccountingUserRef,
                SubmittedTier = "paid",
                PeriodKind = JsonlDesignConversionVisualCostLedger.UtcCalendarMonthPeriodKind,
                PeriodStartUtc = changedAccountingPeriod.StartUtc,
                PeriodEndExclusiveUtc = changedAccountingPeriod.EndExclusiveUtc
            });
        var malformedDimensions = await acceptedLedger.AppendRecordAsync(new DesignConversionVisualCostLedgerRecord(
            RecordId: $"attacker-record-malformed-dimensions-{hostileMarker}",
            LedgerKind: JsonlDesignConversionVisualCostLedger.LedgerKind,
            SchemaVersion: JsonlDesignConversionVisualCostLedger.SchemaVersion,
            EvaluationJobRef: jobRef,
            SubmittedRequestRef: requestRef,
            TotalCostUsd: 0.0600m,
            MeasurementComplete: true,
            MissingMeasurementCount: 0,
            EvaluationAttemptCount: 1,
            CostSource: DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostSourceFixedPerRequest,
            RecordedAtUtc: DateTimeOffset.UnixEpoch.AddSeconds(12),
            EvidenceRefs: [],
            Metadata: new Dictionary<string, string>()) with
            {
                UserRef = hostileMarker,
                SubmittedTier = "paid",
                PeriodKind = JsonlDesignConversionVisualCostLedger.UtcCalendarMonthPeriodKind,
                PeriodStartUtc = DateTimeOffset.UnixEpoch,
                PeriodEndExclusiveUtc = DateTimeOffset.UnixEpoch.AddMonths(1)
            });
        var malformedPeriod = await acceptedLedger.AppendRecordAsync(new DesignConversionVisualCostLedgerRecord(
            RecordId: $"attacker-record-malformed-period-{hostileMarker}",
            LedgerKind: JsonlDesignConversionVisualCostLedger.LedgerKind,
            SchemaVersion: JsonlDesignConversionVisualCostLedger.SchemaVersion,
            EvaluationJobRef: jobRef,
            SubmittedRequestRef: requestRef,
            TotalCostUsd: 0.0700m,
            MeasurementComplete: true,
            MissingMeasurementCount: 0,
            EvaluationAttemptCount: 1,
            CostSource: DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostSourceFixedPerRequest,
            RecordedAtUtc: DateTimeOffset.UnixEpoch.AddMonths(2),
            EvidenceRefs: [],
            Metadata: new Dictionary<string, string>()) with
            {
                UserRef = acceptedUserRef,
                SubmittedTier = "paid",
                PeriodKind = JsonlDesignConversionVisualCostLedger.NotPeriodEnforceablePeriodKind,
                PeriodStartUtc = DateTimeOffset.UnixEpoch,
                PeriodEndExclusiveUtc = DateTimeOffset.UnixEpoch
            });

        Assert.True(accepted.IsSuccess, accepted.ErrorMessage);
        Assert.True(changedAccounting.IsSuccess, changedAccounting.ErrorMessage);
        Assert.False(malformedDimensions.IsSuccess);
        Assert.Equal(JsonlDesignConversionVisualCostLedger.InvalidRecordErrorCode, malformedDimensions.ErrorCode);
        Assert.False(malformedPeriod.IsSuccess);
        Assert.Equal(JsonlDesignConversionVisualCostLedger.InvalidRecordErrorCode, malformedPeriod.ErrorCode);
        Assert.NotEqual($"attacker-record-{hostileMarker}", accepted.Value!.RecordId);
        Assert.True(JsonlDesignConversionVisualCostLedger.IsCanonicalRecordId(accepted.Value.RecordId));
        Assert.Equal(JsonlDesignConversionVisualCostLedger.LedgerKind, accepted.Value.LedgerKind);
        Assert.Equal(JsonlDesignConversionVisualCostLedger.SchemaVersion, accepted.Value.SchemaVersion);
        Assert.Equal(DesignConversionVisualCostAccounting.UnknownCostSource, accepted.Value.CostSource);
        Assert.Equal(acceptedUserRef, accepted.Value.UserRef);
        Assert.Equal(JsonlDesignConversionVisualCostLedger.UnrecognizedSubmittedTier, accepted.Value.SubmittedTier);
        Assert.Equal(JsonlDesignConversionVisualCostLedger.UtcCalendarMonthPeriodKind, accepted.Value.PeriodKind);
        Assert.Equal(acceptedPeriod.StartUtc, accepted.Value.PeriodStartUtc);
        Assert.Equal(acceptedPeriod.EndExclusiveUtc, accepted.Value.PeriodEndExclusiveUtc);
        Assert.Equal(
            [
                "known_tiers_preserved",
                "ledger_purpose",
                "ledger_scope",
                "monthly_cap_enforcement",
                "period_source",
                "quota_enforcement",
                "raw_payloads_persisted",
                "tier_authority",
                "training_artifact_db",
                "user_ref_semantics"
            ],
            accepted.Value.Metadata.Keys.Order(StringComparer.Ordinal).ToArray());
        Assert.All(accepted.Value.EvidenceRefs, reference =>
            Assert.Matches($"^{JsonlDesignConversionVisualCostLedger.EvidenceRefPrefix}-[0-9a-f]{{64}}$", reference));
        Assert.NotEqual(accepted.Value.RecordId, changedAccounting.Value!.RecordId);

        var records = await acceptedLedger.ListRecordsAsync();
        Assert.True(records.IsSuccess, records.ErrorMessage);
        Assert.Equal(2, records.Value!.Count);
        Assert.Contains(records.Value, record => record.TotalCostUsd == 0.0400m);
        Assert.Contains(records.Value, record => record.TotalCostUsd == 0.0500m);

        var ledgerText = await File.ReadAllTextAsync(Path.Combine(acceptedRoot, JsonlDesignConversionVisualCostLedger.RecordsFileName));
        Assert.DoesNotContain(hostileMarker, ledgerText, StringComparison.Ordinal);
        Assert.DoesNotContain("attacker-record", ledgerText, StringComparison.Ordinal);
        Assert.DoesNotContain("attacker-ledger-kind", ledgerText, StringComparison.Ordinal);
        Assert.DoesNotContain("attacker-schema", ledgerText, StringComparison.Ordinal);
        Assert.DoesNotContain("untrusted-cost-source", ledgerText, StringComparison.Ordinal);
        Assert.DoesNotContain("safe_looking_key", ledgerText, StringComparison.Ordinal);
        Assert.DoesNotContain("billing", ledgerText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("tenant", ledgerText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("<main", ledgerText, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(2, File.ReadAllLines(Path.Combine(acceptedRoot, JsonlDesignConversionVisualCostLedger.RecordsFileName)).Length);
    }

    [Fact]
    public async Task VisualCostLedger_CorruptJsonReturnsStableFailureWithoutThrowing()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "visual-cost-ledger-corrupt-json");
        Directory.CreateDirectory(root);
        await File.WriteAllTextAsync(
            Path.Combine(root, JsonlDesignConversionVisualCostLedger.RecordsFileName),
            "{ not-json");
        var ledger = new JsonlDesignConversionVisualCostLedger(root);
        var jobRef = JsonlDesignConversionVisualCostLedger.BuildCanonicalEvaluationJobRef("thin-job-corrupt-json");
        var requestRef = JsonlDesignConversionVisualCostLedger.BuildCanonicalSubmittedRequestRef("thin-request-corrupt-json");
        var userRef = JsonlDesignConversionVisualCostLedger.BuildPseudonymousUserRef("corrupt-json-user").Value!;
        var period = JsonlDesignConversionVisualCostLedger.BuildUtcCalendarMonthPeriod(DateTimeOffset.UnixEpoch);

        var read = await ledger.ListRecordsAsync();
        var append = await ledger.AppendRecordAsync(new DesignConversionVisualCostLedgerRecord(
            RecordId: "caller-record-ignored",
            LedgerKind: JsonlDesignConversionVisualCostLedger.LedgerKind,
            SchemaVersion: JsonlDesignConversionVisualCostLedger.SchemaVersion,
            EvaluationJobRef: jobRef,
            SubmittedRequestRef: requestRef,
            TotalCostUsd: 0.0100m,
            MeasurementComplete: true,
            MissingMeasurementCount: 0,
            EvaluationAttemptCount: 1,
            CostSource: DesignConversionVisualCostAccounting.UnknownCostSource,
            RecordedAtUtc: DateTimeOffset.UnixEpoch,
            EvidenceRefs: [],
            Metadata: new Dictionary<string, string>()) with
            {
                UserRef = userRef,
                SubmittedTier = "paid",
                PeriodKind = JsonlDesignConversionVisualCostLedger.UtcCalendarMonthPeriodKind,
                PeriodStartUtc = period.StartUtc,
                PeriodEndExclusiveUtc = period.EndExclusiveUtc
            });

        Assert.False(read.IsSuccess);
        Assert.Equal(JsonlDesignConversionVisualCostLedger.ReadFailedErrorCode, read.ErrorCode);
        Assert.False(append.IsSuccess);
        Assert.Equal(JsonlDesignConversionVisualCostLedger.ReadFailedErrorCode, append.ErrorCode);
    }

    [Fact]
    public void VisualCostLedger_DefaultRootResolutionUsesDurableConfiguredOrLocalApplicationDataRoot()
    {
        var driveRoot = Path.GetPathRoot(Environment.CurrentDirectory) ?? @"C:\";
        var configuredRoot = Path.Combine(driveRoot, "design-conversion-ledger-configured-root");
        var suppliedLocalApplicationDataRoot = Path.Combine(
            driveRoot,
            "Users",
            "visual-ledger-test",
            "AppData",
            "Local");

        var configured = JsonlDesignConversionVisualCostLedger.ResolveDefaultRoot(
            configuredRoot,
            suppliedLocalApplicationDataRoot);
        var unconfigured = JsonlDesignConversionVisualCostLedger.ResolveDefaultRoot(
            configuredRoot: null,
            suppliedLocalApplicationDataRoot);
        var blankConfigured = JsonlDesignConversionVisualCostLedger.ResolveDefaultRoot(
            "   ",
            suppliedLocalApplicationDataRoot);
        var relativeConfigured = Assert.Throws<InvalidOperationException>(() =>
            JsonlDesignConversionVisualCostLedger.ResolveDefaultRoot(
                "relative-visual-cost-ledger-root",
                suppliedLocalApplicationDataRoot));
        var missingLocalApplicationData = Assert.Throws<InvalidOperationException>(() =>
            JsonlDesignConversionVisualCostLedger.ResolveDefaultRoot(
                configuredRoot: null,
                localApplicationDataRoot: "   "));

        Assert.Equal(Path.GetFullPath(configuredRoot), configured);
        Assert.Equal(
            Path.Combine(
                Path.GetFullPath(suppliedLocalApplicationDataRoot),
                "llm_mvp_core",
                "design-conversion",
                "visual-cost-ledger"),
            unconfigured);
        Assert.Equal(unconfigured, blankConfigured);
        Assert.DoesNotContain(Path.GetTempPath(), unconfigured, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(
            "DesignConversion visual-cost ledger configured root must be fully qualified.",
            relativeConfigured.Message);
        Assert.Equal(
            "DesignConversion visual-cost ledger default root requires a fully qualified LocalApplicationData directory.",
            missingLocalApplicationData.Message);
    }

    [Fact]
    public async Task ImprovementLoopEvaluator_AggregatesVisualCostAccountingAcrossAttempts()
    {
        var visualJudge = new ScriptedCostVisualJudge(
            new VisualJudgeCostScript(0.70, 0.0300m),
            new VisualJudgeCostScript(0.80, 0.0400m));
        var inner = new RecordingEvaluator(CreateVisualJudgeInternalEvaluator(visualJudge));
        var candidateStrategy = new SingleCandidateStrategy();
        var evaluator = new DesignConversionImprovementLoopThinClientJobEvaluator(
            inner,
            candidateStrategy);
        var request = CreateLoopCostAccountingInternalRequest();

        var result = await evaluator.EvaluateAsync(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(2, inner.Requests.Count);
        Assert.Equal(2, visualJudge.Requests.Count);
        Assert.Single(candidateStrategy.Requests);
        var final = result.Value!;
        Assert.Contains(
            DesignConversionImprovementLoopThinClientJobEvaluator.SubmitPathLoopExecutedMarker,
            final.EvidenceRefs);
        Assert.Equal(0.0700m, final.VisualCostAccounting.TotalCostUsd);
        Assert.True(final.VisualCostAccounting.MeasurementComplete);
        Assert.Equal(0, final.VisualCostAccounting.MissingMeasurementCount);
        Assert.Equal(2, final.VisualCostAccounting.EvaluationAttemptCount);
        Assert.Equal(
            DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostSourceFixedPerRequest,
            final.VisualCostAccounting.CostSource);
        Assert.Equal(0.0400m, final.VisualQaBoundaryTrace.TierPolicy.EstimatedCostUsd);
        Assert.True(final.VisualQaBoundaryTrace.TierPolicy.EstimatedCostUsd < final.VisualCostAccounting.TotalCostUsd);
    }

    [Fact]
    public async Task ImprovementLoopEvaluator_HandsOffRejectedEvaluationUsingSanitizedRootJobId()
    {
        var visualJudge = new ScriptedCostVisualJudge(
            new VisualJudgeCostScript(0.70, 0.0300m),
            new VisualJudgeCostScript(0.80, 0.0400m));
        var handoff = new DesignConversionRuntimeRejectedEvaluationHandoff();
        var evaluator = new DesignConversionImprovementLoopThinClientJobEvaluator(
            CreateVisualJudgeInternalEvaluator(visualJudge),
            new SingleCandidateStrategy(),
            handoff);
        var request = CreateLoopCostAccountingInternalRequest();

        var result = await evaluator.EvaluateAsync(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(request.JobId, result.Value!.JobId);
        Assert.Equal(1, handoff.PendingCount);

        var resolved = await handoff.ResolveMeasuredRejectedEvaluationAsync(
            new DesignConversionRuntimeRejectedEvaluationLookup(result.Value.JobId));

        Assert.True(resolved.IsSuccess, resolved.ErrorMessage);
        var reference = Assert.IsType<DesignConversionRuntimeRejectedEvaluationReference>(resolved.Value);
        Assert.True(reference.TryGetTrustedHydratedEvaluation(out var rejected));
        Assert.True(reference.TryGetTrustedChosenEvaluation(out var chosen));
        Assert.NotNull(rejected);
        Assert.NotNull(chosen);
        // The first measured attempt owns the root job identity; the returned
        // final evaluation is sanitized to that same public root identity.
        // DPO pair distinctness is established by its internal output identity
        // checks, not by this public boundary identifier.
        Assert.Equal(result.Value.JobId, rejected!.JobId);
        Assert.NotEqual(chosen!.JobId, rejected.JobId);
        Assert.NotEqual(
            chosen.RuntimeFlowResult.Output.OutputId,
            rejected.RuntimeFlowResult.Output.OutputId);
        Assert.Equal(0, handoff.PendingCount);
    }

    [Fact]
    public async Task ImprovementLoopEvaluator_FailedAttemptKeepsVisualCostAccountingIncomplete()
    {
        var visualJudge = new ScriptedCostVisualJudge(new VisualJudgeCostScript(0.70, 0.0300m));
        var inner = new RecordingEvaluator(new FailingOnCallEvaluator(
            CreateVisualJudgeInternalEvaluator(visualJudge),
            failOnCall: 2));
        var candidateStrategy = new SingleCandidateStrategy();
        var evaluator = new DesignConversionImprovementLoopThinClientJobEvaluator(
            inner,
            candidateStrategy);
        var request = CreateLoopCostAccountingInternalRequest();

        var result = await evaluator.EvaluateAsync(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(2, inner.Requests.Count);
        Assert.Single(candidateStrategy.Requests);
        Assert.True(inner.Responses[0].IsSuccess, inner.Responses[0].ErrorMessage);
        Assert.False(inner.Responses[1].IsSuccess);
        var final = result.Value!;
        Assert.Equal(0.0300m, final.VisualCostAccounting.TotalCostUsd);
        Assert.False(final.VisualCostAccounting.MeasurementComplete);
        Assert.Equal(1, final.VisualCostAccounting.MissingMeasurementCount);
        Assert.Equal(2, final.VisualCostAccounting.EvaluationAttemptCount);
        Assert.Equal(DesignConversionVisualCostAccounting.MixedCostSource, final.VisualCostAccounting.CostSource);
    }

    [Fact]
    public async Task RunAsync_WithMismatchedServerOwnedVisualJudgeReceiptDoesNotPersistForgedReceipt()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "thin-client-runtime-forged-receipt-artifact-db");
        var db = new JsonlDesignConversionLocalArtifactDb(root);
        var providerReceipt = BuildServerOwnedReceipt("2", "0.0300", "0.0600");
        var resultReceipt = BuildServerOwnedReceipt("1", "0.0300", "0.0300");
        var visualJudge = new ReceiptVisualJudge(providerReceipt, resultReceipt, estimatedCostUsd: 0.0600m);
        var evaluator = new RecordingEvaluator(CreateVisualJudgeInternalEvaluator(visualJudge));
        var runtime = new DesignConversionThinClientJobRuntime(evaluator, FixedClock(), db);
        var request = CreateSubmittedRequest() with
        {
            RequestId = "thin-forged-receipt-request-001",
            Budget = CreateSubmittedRequest().Budget with
            {
                UserId = "receipt-forgery-marker",
                Tier = "paid",
                MaxRenderQaRuns = 1,
                BudgetUsd = 0.25m
            }
        };
        var submitted = runtime.Submit(request);
        Assert.True(submitted.IsSuccess, submitted.ErrorMessage);

        var result = await runtime.RunAsync(submitted.Value!.JobId);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(1, visualJudge.InvocationCount);
        var listed = await db.ListRecordsAsync(new DesignConversionLocalArtifactDbQuery(JobId: submitted.Value.JobId));
        Assert.True(listed.IsSuccess, listed.ErrorMessage);
        var stored = Assert.Single(listed.Value!);
        var metadata = stored.VisualJudgeStatus.Metadata;
        Assert.Equal(0.0600m, stored.VisualJudgeStatus.EstimatedCostUsd);
        Assert.Equal(bool.FalseString, metadata["server_owned_accounting_receipt_verified"]);
        Assert.Equal("not_verified", metadata["server_owned_accounting_receipt_status"]);
        foreach (var key in ServerOwnedReceiptKeys())
            Assert.False(metadata.ContainsKey(key));
        AssertNoUserOrBillingIdentityMetadataKeys(metadata);
        Assert.False(metadata.ContainsKey("untrusted_provider_extra"));
        Assert.False(metadata.ContainsKey("untrusted_result_extra"));
    }

    [Fact]
    public async Task RunAsync_WithLocalPngRendererPersistsFourRenderArtifactRefsWithoutRawPrivateSource()
    {
        var artifactRoot = Path.GetFullPath(Path.Combine(
            "TestResults",
            "codex-pass159-runtime-render-artifacts",
            Guid.NewGuid().ToString("N")));
        var dbRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "pass159-thin-client-runtime-artifact-db");
        var db = new JsonlDesignConversionLocalArtifactDb(dbRoot);
        var evaluator = new RecordingEvaluator(CreateLocalPngInternalEvaluator(artifactRoot));
        var runtime = new DesignConversionThinClientJobRuntime(evaluator, FixedClock(), db);
        var submitted = runtime.Submit(CreateFourViewportPrivateSubmittedRequest());
        Assert.True(submitted.IsSuccess, submitted.ErrorMessage);

        var result = await runtime.RunAsync(submitted.Value!.JobId);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        AssertThinClientNonReadyVisualDecision(
            result.Value!,
            DesignConversionVisualQaBoundaryPolicy.DecisionBlocked,
            DesignConversionVisualQaBoundaryPolicy.ProviderRequired);
        Assert.Contains("local_artifact_db_persistence_verified=true", result.Value.Warnings);
        Assert.Contains("DETERMINISTIC_LOCAL_RENDER_ARTIFACT_NOT_BROWSER_SCREENSHOT", result.Value.Warnings);
        Assert.Single(evaluator.Requests);
        Assert.Equal(4, evaluator.Requests[0].RenderResolutions.Count);

        var evaluation = Assert.Single(evaluator.Responses).Value!;
        Assert.Equal(4, evaluation.RenderResults.Count);
        Assert.All(evaluation.RenderResults, render =>
        {
            Assert.Equal(DeterministicLocalDesignConversionPngRenderProvider.ProviderKind, render.Image.Metadata["provider_kind"]);
            Assert.Equal(bool.TrueString, render.Image.Metadata["actual_png_bytes_written"]);
            Assert.Equal(bool.FalseString, render.Image.Metadata["browser_or_playwright_used"]);
            Assert.Equal(bool.FalseString, render.Image.Metadata["visual_fidelity_claimed"]);
            Assert.True(File.Exists(new Uri(render.Image.Uri).LocalPath), render.Image.Uri);
        });

        var listed = await db.ListRecordsAsync(new DesignConversionLocalArtifactDbQuery(JobId: submitted.Value.JobId));
        Assert.True(listed.IsSuccess, listed.ErrorMessage);
        var stored = Assert.Single(listed.Value!);
        Assert.Equal(4, stored.RenderArtifactRefs.Count);
        Assert.All(stored.RenderArtifactRefs, artifact =>
        {
            Assert.StartsWith("sha256:", artifact.Sha256, StringComparison.Ordinal);
            Assert.NotEmpty(artifact.UriRef);
            Assert.DoesNotContain("PRIVATE_PASS159_SECRET", artifact.UriRef, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("private-frame", artifact.UriRef, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("C:/Users/lubao", artifact.UriRef, StringComparison.OrdinalIgnoreCase);
        });

        var pngFiles = Directory.GetFiles(artifactRoot, "*.png", SearchOption.AllDirectories);
        Assert.Equal(4, pngFiles.Length);
        Assert.All(pngFiles, path => Assert.True(new FileInfo(path).Length > 1000, path));

        var persistedText = await File.ReadAllTextAsync(Path.Combine(dbRoot, "conversion-results.jsonl"));
        Assert.Contains("render_artifact_refs", persistedText, StringComparison.Ordinal);
        Assert.DoesNotContain("PRIVATE_PASS159_SECRET", persistedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("<main", persistedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(".private-frame", persistedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("C:/Users/lubao", persistedText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ApproveRuntimeCreatedDpoCandidateAsync_ApprovesSelectedCandidateAndKeepsReadinessBoundaries()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "thin-client-runtime-approval");
        var db = new JsonlDesignConversionLocalArtifactDb(root);
        await AssertRuntimeCreatedDpoApprovalApiBoundariesAsync(root, db);
    }

    private static async Task AssertRuntimeCreatedDpoApprovalApiBoundariesAsync(
        string root,
        JsonlDesignConversionLocalArtifactDb db)
    {
        var fixture = await RunRuntimeCreatedDpoCandidateAsync(db);
        var stored = fixture.StoredRecord;
        Assert.Equal(1, fixture.RejectedEvaluationSource.CallCount);
        Assert.Equal(fixture.ChosenJobId, fixture.RejectedEvaluationSource.Lookup?.ChosenJobId);
        await AssertPostEvaluationPersistenceBoundaryAsync(root, stored);
        Assert.Equal(
            PostEvaluationStateStoreCompatibilityShim.PersistenceProvisional,
            stored.Metadata[
                PostEvaluationStateStoreCompatibilityShim.PersistenceStatusMetadataKey]);
        Assert.True(await PostEvaluationStateStoreCompatibilityShim.IsCommittedForTrainingAsync(
            root,
            stored.Metadata[PostEvaluationStateStoreCompatibilityShim.StateRefMetadataKey],
            stored.RecordId));
        Assert.False(await PostEvaluationStateStoreCompatibilityShim.IsCommittedForTrainingAsync(
            root,
            stored.Metadata[PostEvaluationStateStoreCompatibilityShim.StateRefMetadataKey],
            "different-artifact-record"));
        var candidates = stored.DpoCandidates.ToArray();
        Assert.Equal(2, candidates.Length);
        var candidate = candidates[0];
        Assert.All(candidates, item =>
        {
            Assert.Equal(0.82, item.TrainingRecord.Chosen.Verdict.Score);
            Assert.Equal(0.46, item.TrainingRecord.Rejected.Verdict.Score);
            Assert.True(item.TrainingRecord.Chosen.Verdict.Score - item.TrainingRecord.Rejected.Verdict.Score > 0.1);
            Assert.Equal(bool.TrueString, item.TrainingRecord.Provenance.Metadata["runtime_rejected_independently_evaluated"]);
            Assert.Equal(bool.TrueString, item.TrainingRecord.Provenance.Metadata["runtime_teacher_authored_candidate"]);
            Assert.False(item.TrainingRecord.Provenance.Metadata.ContainsKey(
                "runtime_teacher_selected_preference"));
            Assert.Equal(bool.TrueString,
                item.TrainingRecord.Provenance.Metadata["independent_preference_proven"]);
            Assert.Equal(bool.TrueString, item.TrainingRecord.Provenance.Metadata["deterministic_measurement_used_as_admission_veto"]);
            Assert.Equal(bool.FalseString,
                item.TrainingRecord.Provenance.Metadata["training_grade"]);
        });
        Assert.Contains("runtime_rejected_independently_evaluated=true", fixture.RunResult.Warnings);

        var api = new DesignConversionDpoUsefulnessApi(db);
        var badId = await api.ApproveRuntimeCreatedDpoCandidateAsync(
            stored.RecordId,
            "missing-dpo-candidate",
            "reviewer:pass106-local",
            "local evidence-bound usefulness review approved");
        var missingReviewer = await api.ApproveRuntimeCreatedDpoCandidateAsync(
            stored.RecordId,
            candidate.RecordId,
            "",
            "local evidence-bound usefulness review approved");
        var missingReason = await api.ApproveRuntimeCreatedDpoCandidateAsync(
            stored.RecordId,
            candidate.RecordId,
            "reviewer:pass106-local",
            "");
        var beforeApproval = await api.QueryAsync(new DesignConversionDpoUsefulnessApiRequest(IncludeBlocked: true));
        var beforeApprovedTraining = await db.QueryApprovedTrainingCandidatesAsync(
            new DesignConversionLocalArtifactDbQuery(JobId: stored.JobId));
        var beforeHandoff = await api.ExportApprovedRuntimeDpoCandidatesAsync(
            new DesignConversionLocalArtifactDbQuery(JobId: stored.JobId),
            DateTimeOffset.UnixEpoch.AddMinutes(107));

        Assert.False(badId.IsSuccess);
        Assert.Equal("DESIGN_CONVERSION_DPO_CANDIDATE_NOT_FOUND", badId.ErrorCode);
        Assert.False(missingReviewer.IsSuccess);
        Assert.Equal("DESIGN_CONVERSION_DPO_APPROVAL_REVIEWER_REQUIRED", missingReviewer.ErrorCode);
        Assert.False(missingReason.IsSuccess);
        Assert.Equal("DESIGN_CONVERSION_DPO_APPROVAL_REASON_REQUIRED", missingReason.ErrorCode);
        Assert.True(beforeApproval.IsSuccess, beforeApproval.ErrorMessage);
        Assert.Equal(2, beforeApproval.Value!.Rows.Count);
        Assert.All(beforeApproval.Value.Rows, beforeApprovalRow =>
        {
            Assert.False(beforeApprovalRow.TrainReady);
            Assert.Equal("pending", beforeApprovalRow.ApprovalStatus);
        });
        Assert.True(beforeApprovedTraining.IsSuccess, beforeApprovedTraining.ErrorMessage);
        Assert.Empty(beforeApprovedTraining.Value!);
        Assert.True(beforeHandoff.IsSuccess, beforeHandoff.ErrorMessage);
        Assert.Equal(0, beforeHandoff.Value!.ApprovedCandidateCount);
        Assert.Empty(beforeHandoff.Value.Rows);
        Assert.Contains("no_approved_candidates", beforeHandoff.Value.Warnings);
        Assert.Contains("training_handoff_only", beforeHandoff.Value.Warnings);

        var approvedAt = DateTimeOffset.UnixEpoch.AddMinutes(106);
        var approval = await api.ApproveRuntimeCreatedDpoCandidateAsync(
            stored.RecordId,
            candidate.RecordId,
            "reviewer:pass106-local",
            "local evidence-bound usefulness review approved for training-candidate export",
            approvedAt);

        Assert.True(approval.IsSuccess, approval.ErrorMessage);
        Assert.Equal(stored.RecordId, approval.Value!.LocalArtifactRecordId);
        Assert.Equal(candidate.CandidateId, approval.Value.CandidateId);
        Assert.Equal(candidate.RecordId, approval.Value.TrainingRecordId);
        Assert.Equal("approved", approval.Value.ApprovalStatus);
        Assert.Equal("runtime-candidate-review-approved", approval.Value.ReviewStatus);
        Assert.True(approval.Value.ApprovedForTraining);
        Assert.Equal(bool.TrueString, approval.Value.BoundaryMetadata["approved_for_training"]);
        Assert.Equal(bool.TrueString, approval.Value.BoundaryMetadata["product_not_ready"]);
        Assert.Equal(bool.FalseString, approval.Value.BoundaryMetadata["model_readiness_claimed"]);
        Assert.Equal(bool.FalseString, approval.Value.BoundaryMetadata["public_api_verified"]);
        Assert.Equal(bool.FalseString, approval.Value.BoundaryMetadata["production_db_ready"]);
        Assert.Equal(bool.FalseString, approval.Value.BoundaryMetadata["trained_model_ready"]);

        var approvedTraining = await db.QueryApprovedTrainingCandidatesAsync(
            new DesignConversionLocalArtifactDbQuery(JobId: stored.JobId));
        var readback = await api.QueryLocalArtifactReadbackAsync(
            new DesignConversionLocalArtifactReadbackApiRequest(JobId: stored.JobId));
        var usefulness = await api.QueryAsync(new DesignConversionDpoUsefulnessApiRequest(IncludeBlocked: true));
        var handoff = await api.ExportApprovedRuntimeDpoCandidatesAsync(
            new DesignConversionLocalArtifactDbQuery(JobId: stored.JobId),
            DateTimeOffset.UnixEpoch.AddMinutes(107));
        var afterListed = await db.ListRecordsAsync(new DesignConversionLocalArtifactDbQuery(JobId: stored.JobId));
        var persistedText = await File.ReadAllTextAsync(Path.Combine(root, "conversion-results.jsonl"));
        var indexText = await File.ReadAllTextAsync(Path.Combine(root, "conversion-results.index.json"));

        Assert.True(approvedTraining.IsSuccess, approvedTraining.ErrorMessage);
        var approvedRecord = Assert.Single(approvedTraining.Value!);
        Assert.Equal(candidate.RecordId, approvedRecord.RecordId);
        Assert.Equal("approved", approvedRecord.Provenance.Metadata["approval_status"]);
        Assert.Equal(bool.TrueString, approvedRecord.Provenance.Metadata["approved_for_training"]);
        Assert.Equal(bool.FalseString, approvedRecord.Provenance.Metadata["model_readiness_claimed"]);
        Assert.Equal(bool.FalseString, approvedRecord.Provenance.Metadata["production_db_ready"]);
        Assert.Equal(bool.FalseString, approvedRecord.Provenance.Metadata["trained_model_ready"]);
        Assert.True(readback.IsSuccess, readback.ErrorMessage);
        Assert.Equal("1", readback.Value!.BoundaryMetadata["runtime_review_pending_count"]);
        Assert.Equal("0", readback.Value.BoundaryMetadata["runtime_review_approved_count"]);
        Assert.Equal(bool.FalseString, readback.Value.BoundaryMetadata["production_db_ready"]);
        Assert.Equal(bool.FalseString, readback.Value.BoundaryMetadata["trained_model_ready"]);
        var readbackRow = Assert.Single(readback.Value.Rows);
        Assert.Equal("runtime-candidate-review-pending", readbackRow.DpoReviewStatus);
        Assert.Equal("pending", readbackRow.DpoApprovalStatus);
        Assert.True(readbackRow.ProductNotReady);
        Assert.False(readbackRow.ProductionDpo);
        Assert.False(readbackRow.ModelReadinessClaimed);
        Assert.True(usefulness.IsSuccess, usefulness.ErrorMessage);
        Assert.Equal(1, usefulness.Value!.TrainReadyCount);
        var usefulnessRow = Assert.Single(
            usefulness.Value.Rows,
            row => row.TrainReady);
        Assert.True(usefulnessRow.TrainReady);
        Assert.True(usefulnessRow.ProductNotReady);
        Assert.False(usefulnessRow.ProductionDpo);
        Assert.False(usefulnessRow.ModelReadinessClaimed);
        Assert.True(handoff.IsSuccess, handoff.ErrorMessage);
        Assert.Equal(1, handoff.Value!.ApprovedCandidateCount);
        Assert.Contains("training_handoff_only", handoff.Value.Warnings);
        Assert.Contains("not_trained", handoff.Value.Warnings);
        Assert.Contains("weights_not_produced", handoff.Value.Warnings);
        Assert.Contains("model_upload_not_ready", handoff.Value.Warnings);
        Assert.Equal(bool.FalseString, handoff.Value.BoundaryMetadata["trained_model_ready"]);
        Assert.Equal(bool.FalseString, handoff.Value.BoundaryMetadata["model_readiness_claimed"]);

        var intake = await api.CreateTrainingIntakeAsync(
            handoff.Value,
            "requester:pass108-local",
            "future ILearningMachine intake only; no training started",
            DateTimeOffset.UnixEpoch.AddMinutes(108));
        Assert.True(intake.IsSuccess, intake.ErrorMessage);
        Assert.True(intake.Value!.TrainingIntakeOnly);
        Assert.True(intake.Value.TrainingNotStarted);
        Assert.True(intake.Value.NotTrained);
        Assert.True(intake.Value.WeightsNotProduced);
        Assert.True(intake.Value.CheckpointNotMutated);
        Assert.True(intake.Value.ModelUploadNotReady);

        var beforePreflightText = await File.ReadAllTextAsync(Path.Combine(root, "training-intake-requests.jsonl"));
        var preflight = await api.RunTrainingIntakePreflightAsync(intake.Value.IntakeId);
        var afterPreflightText = await File.ReadAllTextAsync(Path.Combine(root, "training-intake-requests.jsonl"));
        Assert.True(preflight.IsSuccess, preflight.ErrorMessage);
        Assert.False(preflight.Value!.MayStartTraining);
        Assert.False(preflight.Value.TrainingStarted);
        Assert.False(preflight.Value.CheckpointMutated);
        Assert.False(preflight.Value.WeightsProduced);
        Assert.False(preflight.Value.ModelUploadReady);
        Assert.Equal(beforePreflightText, afterPreflightText);
        Assert.True(afterListed.IsSuccess, afterListed.ErrorMessage);
        var afterStored = Assert.Single(afterListed.Value!);
        Assert.Equal("pending", afterStored.Metadata["approval_status"]);
        Assert.Equal(bool.TrueString, afterStored.Metadata["approved_for_training"]);
        Assert.Single(
            afterStored.DpoCandidates,
            item => string.Equals(
                item.TrainingRecord.Provenance.Metadata["approval_status"],
                "approved",
                StringComparison.Ordinal));
        Assert.Single(
            afterStored.DpoCandidates,
            item => string.Equals(
                item.TrainingRecord.Provenance.Metadata["approval_status"],
                "pending",
                StringComparison.Ordinal));
        Assert.Contains("runtime_created_dpo_candidate_approved_for_training", persistedText, StringComparison.Ordinal);
        Assert.Contains("approval_status=approved", persistedText, StringComparison.Ordinal);
        Assert.Contains("approved_for_training=true", persistedText, StringComparison.Ordinal);
        Assert.Contains("\"approvedTrainingCandidateCount\": 1", indexText, StringComparison.Ordinal);
        Assert.Contains("runtime_rejected_independently_evaluated", persistedText, StringComparison.Ordinal);
        Assert.DoesNotContain("<main", persistedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(".private-frame", persistedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("raw-tree-private", persistedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("C:/Users/lubao", persistedText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task QueryLocalArtifactReadbackAsync_ReturnsRuntimePersistedRecordAndCreatedDpoRows()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "thin-client-runtime-readback-api");
        var db = new JsonlDesignConversionLocalArtifactDb(root);
        var fixture = await RunRuntimeCreatedDpoCandidateAsync(db);
        var stored = fixture.StoredRecord;
        var candidates = stored.DpoCandidates.ToArray();
        Assert.Equal(2, candidates.Length);
        Assert.All(candidates, candidate =>
        {
            Assert.Equal(0.46, candidate.TrainingRecord.Rejected.Verdict.Score);
            Assert.Equal(bool.TrueString, candidate.TrainingRecord.Provenance.Metadata["runtime_rejected_independently_evaluated"]);
            Assert.Equal(bool.TrueString, candidate.TrainingRecord.Provenance.Metadata["runtime_teacher_authored_candidate"]);
            Assert.False(candidate.TrainingRecord.Provenance.Metadata.ContainsKey(
                "runtime_teacher_selected_preference"));
            Assert.Equal(bool.TrueString,
                candidate.TrainingRecord.Provenance.Metadata["independent_preference_proven"]);
            Assert.Equal(bool.FalseString,
                candidate.TrainingRecord.Provenance.Metadata["training_grade"]);
        });

        var api = new DesignConversionDpoUsefulnessApi(db);
        var readback = await api.QueryLocalArtifactReadbackAsync(
            new DesignConversionLocalArtifactReadbackApiRequest(JobId: stored.JobId));

        Assert.True(readback.IsSuccess, readback.ErrorMessage);
        Assert.Equal(DesignConversionDpoUsefulnessApi.LocalArtifactReadbackApiShape, readback.Value!.ApiShape);
        Assert.False(readback.Value.ProductReady);
        Assert.Equal(1, readback.Value.RecordCount);
        Assert.Equal(1, readback.Value.RuntimeCreatedRecordCount);
        Assert.Equal(2, readback.Value.DpoCandidateCount);
        Assert.Contains("runtime_created_artifact_record_review_only", readback.Value.Warnings);
        Assert.Contains("runtime_created_dpo_candidates_available", readback.Value.Warnings);
        Assert.Contains("runtime_created_dpo_candidate_review_pending", readback.Value.Warnings);
        Assert.Contains("runtime_created_dpo_candidate_approval_required", readback.Value.Warnings);
        Assert.DoesNotContain("dpo_rows_not_created_without_dpo_candidates", readback.Value.Warnings);
        Assert.Equal(bool.FalseString, readback.Value.BoundaryMetadata["public_api_verified"]);
        Assert.Equal(bool.FalseString, readback.Value.BoundaryMetadata["production_db_ready"]);
        Assert.Equal(bool.FalseString, readback.Value.BoundaryMetadata["trained_model_ready"]);
        Assert.Equal("2", readback.Value.BoundaryMetadata["dpo_candidate_count"]);
        Assert.Equal("1", readback.Value.BoundaryMetadata["runtime_review_pending_count"]);
        Assert.Equal("1", readback.Value.BoundaryMetadata["review_required_count"]);
        Assert.Equal("pending", readback.Value.BoundaryMetadata["approval_status"]);
        Assert.Equal(bool.FalseString, readback.Value.BoundaryMetadata["model_readiness_claimed"]);

        var row = Assert.Single(readback.Value.Rows);
        Assert.Equal(stored.JobId, row.JobId);
        Assert.Equal("thin-request-001", row.RequestId);
        Assert.StartsWith("dcm-html_result-", row.HtmlRef, StringComparison.Ordinal);
        Assert.StartsWith("sha256:", row.HtmlSha256, StringComparison.Ordinal);
        Assert.StartsWith("dcm-css_result-", row.CssRef, StringComparison.Ordinal);
        Assert.StartsWith("sha256:", row.CssSha256, StringComparison.Ordinal);
        Assert.NotEmpty(row.RenderArtifactRefs);
        Assert.All(row.RenderArtifactRefs, render =>
        {
            Assert.StartsWith("sha256:", render.Sha256, StringComparison.Ordinal);
            Assert.NotEmpty(render.UriRef);
            Assert.DoesNotContain("C:/Users/lubao", render.UriRef, StringComparison.OrdinalIgnoreCase);
        });
        Assert.Equal(ExternalHttpDesignConversionVisualJudge.StatusOk, row.VisualJudgeStatus);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.ConfiguredOrLocalLabelerBoundary, row.VisualJudgeProviderBoundary);
        Assert.True(row.ExternalVisualAiCallExecuted);
        Assert.Equal(2, row.DpoCandidateCount);
        Assert.Equal("runtime-candidate-review-pending", row.DpoReviewStatus);
        Assert.True(row.DpoReviewRequired);
        Assert.Equal("pending", row.DpoApprovalStatus);
        Assert.True(row.TrainingCandidateOnly);
        Assert.True(row.ProductNotReady);
        Assert.False(row.ProductionDpo);
        Assert.False(row.ModelReadinessClaimed);
        Assert.True(row.HeldOutNotProven);
        Assert.True(row.SourceDisjointNotProven);
        Assert.Equal(DesignConversionDpoUsefulnessApi.ProductNotReadyBoundary, row.ReadinessBoundary);
        Assert.Contains("conversion_result", row.EvidenceRefs);
        Assert.Contains("render_artifact_refs", row.EvidenceRefs);
        Assert.Contains("visual_judge_status", row.EvidenceRefs);
        Assert.Contains("product_not_ready", row.EvidenceRefs);
        Assert.Equal(bool.TrueString, row.BoundaryMetadata["local_artifact_db"]);
        Assert.Equal(bool.TrueString, row.BoundaryMetadata["review_required"]);
        Assert.Equal("pending", row.BoundaryMetadata["approval_status"]);
        Assert.Equal("runtime-candidate-review-pending", row.BoundaryMetadata["review_status"]);
        Assert.Equal(bool.FalseString, row.BoundaryMetadata["approved_for_training"]);
        Assert.Equal(bool.TrueString, row.BoundaryMetadata["product_not_ready"]);
        Assert.Equal(bool.FalseString, row.BoundaryMetadata["production_dpo"]);
        Assert.Equal(bool.FalseString, row.BoundaryMetadata["model_readiness_claimed"]);
        Assert.Equal(bool.TrueString, row.BoundaryMetadata["held_out_not_proven"]);
        Assert.Equal(bool.TrueString, row.BoundaryMetadata["source_disjoint_not_proven"]);
        Assert.Equal(bool.FalseString, row.BoundaryMetadata["public_api_verified"]);
        Assert.Equal(bool.FalseString, row.BoundaryMetadata["production_db_ready"]);
        Assert.Equal(bool.FalseString, row.BoundaryMetadata["trained_model_ready"]);
        Assert.Equal(bool.FalseString, row.BoundaryMetadata["raw_html_included"]);
        Assert.Equal(bool.FalseString, row.BoundaryMetadata["raw_css_included"]);
        Assert.Equal(bool.FalseString, row.BoundaryMetadata["raw_design_tree_included"]);
        Assert.Equal(bool.FalseString, row.BoundaryMetadata["exact_prompt_included"]);
        Assert.Equal(bool.FalseString, row.BoundaryMetadata["provider_payload_included"]);

        var dpoUsefulness = await api.QueryAsync(new DesignConversionDpoUsefulnessApiRequest(IncludeBlocked: true));
        Assert.True(dpoUsefulness.IsSuccess, dpoUsefulness.ErrorMessage);
        var usefulnessRows = dpoUsefulness.Value!.Rows.ToArray();
        Assert.Equal(2, usefulnessRows.Length);
        Assert.Contains("runtime_created_dpo_candidate_review_pending", dpoUsefulness.Value.Warnings);
        Assert.Contains("runtime_created_dpo_candidate_approval_required", dpoUsefulness.Value.Warnings);
        Assert.Equal("2", dpoUsefulness.Value.BoundaryMetadata["runtime_review_pending_count"]);
        Assert.Equal(0, dpoUsefulness.Value.TrainReadyCount);
        Assert.All(usefulnessRows, usefulnessRow =>
        {
            Assert.Equal("runtime-candidate-review-pending", usefulnessRow.ReviewStatus);
            Assert.True(usefulnessRow.ReviewRequired);
            Assert.Equal("pending", usefulnessRow.ApprovalStatus);
            Assert.False(usefulnessRow.TrainReady);
            Assert.True(usefulnessRow.TrainingCandidateOnly);
            Assert.True(usefulnessRow.ProductNotReady);
            Assert.False(usefulnessRow.ProductionDpo);
            Assert.False(usefulnessRow.ModelReadinessClaimed);
            Assert.True(usefulnessRow.HeldOutNotProven);
            Assert.True(usefulnessRow.SourceDisjointNotProven);
            Assert.False(usefulnessRow.HeldOut);
            Assert.False(usefulnessRow.ProviderMissing);
            Assert.Contains("runtime-candidate-review-pending", usefulnessRow.UsefulnessFactors);
            Assert.Contains("source_disjoint=not_proven", usefulnessRow.UsefulnessFactors);
            Assert.Equal(bool.TrueString, usefulnessRow.BoundaryMetadata["review_required"]);
            Assert.Equal("pending", usefulnessRow.BoundaryMetadata["approval_status"]);
            Assert.Equal(bool.FalseString, usefulnessRow.BoundaryMetadata["approved_for_training"]);
            Assert.Equal(bool.FalseString, usefulnessRow.BoundaryMetadata["model_readiness_claimed"]);
        });

        var serialized = JsonSerializer.Serialize(readback.Value);
        Assert.DoesNotContain("<main", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(".private-frame", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("raw-tree-private", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("C:/Users/lubao", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("exact_prompt design", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("make the private button blue", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("private.fig", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("external-provider-payload", serialized, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RunAsync_RecordsFailedStatusWhenInternalEvaluatorFails()
    {
        var runtime = new DesignConversionThinClientJobRuntime(
            new FailingEvaluator("deterministic evaluator failed", "DETERMINISTIC_EVALUATOR_FAILED"),
            FixedClock(),
            visualCostLedger: new JsonlDesignConversionVisualCostLedger(
                CreateBudgetStoreTempRoot("failed-evaluator-ledger")),
            postEvaluationStateStore: CreatePostEvaluationStateStore("failed-evaluator"));
        var submitted = runtime.Submit(CreateSubmittedRequest());
        Assert.True(submitted.IsSuccess, submitted.ErrorMessage);

        var result = await runtime.RunAsync(submitted.Value!.JobId);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionThinClientStatus.Failed, result.Value!.ContractStatus);
        Assert.Equal(DesignConversionThinClientStatus.Failed, result.Value.Job.Status);
        Assert.Equal("DETERMINISTIC_EVALUATOR_FAILED", result.Value.Job.Metadata["error_code"]);
        Assert.Equal("deterministic evaluator failed", result.Value.Message);
        Assert.Empty(result.Value.Artifacts);
        Assert.Contains("internal_evaluator_error_code=DETERMINISTIC_EVALUATOR_FAILED", result.Value.Warnings);
        Assert.Contains("persistence_verified=false", result.Value.Warnings);

        var stored = runtime.GetStatus(submitted.Value.JobId);
        Assert.True(stored.IsSuccess, stored.ErrorMessage);
        Assert.Equal(DesignConversionThinClientStatus.Failed, stored.Value!.Job.Status);
        Assert.Equal("deterministic evaluator failed", stored.Value.Message);
    }

    [Fact]
    public async Task RunAsync_TrustedVisualCostReservationDenialStopsBeforeEvaluator()
    {
        var store = new JsonlDesignConversionVisualCostBudgetStore(CreateBudgetStoreTempRoot("trusted-denied"));
        var events = new List<string>();
        var evaluator = new EventRecordingEvaluator(CreateDeterministicInternalEvaluator(), events);
        var runtime = new DesignConversionThinClientJobRuntime(
            evaluator,
            FixedClock(),
            visualCostBudgetStore: store);
        var request = WithTrustedVisualCostBudget(
            CreateSubmittedRequest(),
            monthlyLimitUsd: "0.01",
            budgetUsd: 0.25m);
        var submitted = runtime.Submit(request);
        Assert.True(submitted.IsSuccess, submitted.ErrorMessage);

        var result = await runtime.RunAsync(submitted.Value!.JobId);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionThinClientStatus.Failed, result.Value!.ContractStatus);
        Assert.Equal(
            JsonlDesignConversionVisualCostBudgetStore.DenialReasonMonthlyLimitExceeded,
            result.Value.Job.Metadata["error_code"]);
        Assert.Empty(evaluator.Requests);
        Assert.DoesNotContain("evaluate", events);
        var period = await store.ReadPeriodAsync(BuildTrustedBudgetPeriodQuery(request));
        Assert.True(period.IsSuccess, period.ErrorMessage);
        Assert.Equal(1, period.Value!.DeniedReservationCount);
        Assert.Equal(0, period.Value.ActiveReservationCount);
        Assert.Equal(0m, period.Value.FinalizedSpendUsd);
    }

    [Theory]
    [InlineData(
        "canonical-monthly-limit",
        JsonlDesignConversionVisualCostBudgetStore.DenialReasonMonthlyLimitExceeded,
        JsonlDesignConversionVisualCostBudgetStore.DenialReasonMonthlyLimitExceeded)]
    [InlineData(
        "valid-looking-private",
        "DESIGN_CONVERSION_PRIVATE_TENANT_ALPHA_DENIAL",
        DesignConversionThinClientJobRuntime.TrustedVisualCostBudgetReservationDeniedErrorCode)]
    [InlineData(
        "none",
        JsonlDesignConversionVisualCostBudgetStore.NoDenialReason,
        DesignConversionThinClientJobRuntime.TrustedVisualCostBudgetReservationDeniedErrorCode)]
    [InlineData(
        "blank",
        "   ",
        DesignConversionThinClientJobRuntime.TrustedVisualCostBudgetReservationDeniedErrorCode)]
    public async Task RunAsync_TrustedVisualCostDenialReasonAllowlistPreservesOnlyCanonicalPublicCode(
        string mode,
        string denialReasonCode,
        string expectedErrorCode)
    {
        const string privateMarker = "DESIGN_CONVERSION_PRIVATE_TENANT_ALPHA_DENIAL";
        var events = new List<string>();
        Func<
            DesignConversionVisualCostReservationRequest,
            CancellationToken,
            Task<OperationResult<DesignConversionVisualCostReservationResult>>> reserveOverride =
            (reservationRequest, _) => Task.FromResult(
                OperationResult<DesignConversionVisualCostReservationResult>.Ok(
                    new DesignConversionVisualCostReservationResult(
                        Status: JsonlDesignConversionVisualCostBudgetStore.StatusDenied,
                        Reserved: false,
                        Replayed: false,
                        ReservationRef: reservationRequest.ReservationRef,
                        UserRef: reservationRequest.UserRef,
                        SubmittedTier: reservationRequest.SubmittedTier,
                        PeriodStartUtc: reservationRequest.PeriodStartUtc,
                        PeriodEndExclusiveUtc: reservationRequest.PeriodEndExclusiveUtc,
                        MonthlyLimitUsd: reservationRequest.MonthlyLimitUsd,
                        RequestedReserveUsd: reservationRequest.RequestedReserveUsd,
                        FinalizedSpendUsd: 0m,
                        ActiveReservedUsd: 0m,
                        RemainingBudgetUsd: reservationRequest.MonthlyLimitUsd,
                        DenialReasonCode: denialReasonCode,
                        Metadata: new Dictionary<string, string>(StringComparer.Ordinal)
                        {
                            ["dependency_private_marker"] = privateMarker
                        })));
        var store = new EventRecordingVisualCostBudgetStore(
            new JsonlDesignConversionVisualCostBudgetStore(
                CreateBudgetStoreTempRoot($"trusted-denial-allowlist-{mode}")),
            events,
            reserveOverride: reserveOverride);
        var evaluator = new EventRecordingEvaluator(CreateDeterministicInternalEvaluator(), events);
        var ledger = new RecordingVisualCostLedger(
            new JsonlDesignConversionVisualCostLedger(
                CreateBudgetStoreTempRoot($"trusted-denial-allowlist-ledger-{mode}")));
        var artifactDb = new RecordingLocalArtifactDb(
            new JsonlDesignConversionLocalArtifactDb(
                CreateBudgetStoreTempRoot($"trusted-denial-allowlist-artifact-{mode}")));
        var runtime = new DesignConversionThinClientJobRuntime(
            evaluator,
            FixedClock(),
            artifactDb,
            ledger,
            store);
        var request = WithTrustedVisualCostBudget(
            CreateSubmittedRequest() with
            {
                RequestId = $"thin-trusted-denial-allowlist-{mode}"
            },
            monthlyLimitUsd: "1",
            budgetUsd: 0.25m);
        var submitted = runtime.Submit(request);
        Assert.True(submitted.IsSuccess, submitted.ErrorMessage);

        var result = await runtime.RunAsync(submitted.Value!.JobId);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var terminal = result.Value!;
        Assert.Equal(DesignConversionThinClientStatus.Failed, terminal.ContractStatus);
        Assert.Equal(DesignConversionThinClientStatus.Failed, terminal.Job.Status);
        Assert.Equal(expectedErrorCode, terminal.Job.Metadata["error_code"]);
        Assert.Equal(
            "DesignConversion trusted visual-cost budget reservation was denied.",
            terminal.Message);
        Assert.DoesNotContain(privateMarker, terminal.Message, StringComparison.Ordinal);
        Assert.All(
            terminal.Warnings,
            warning => Assert.DoesNotContain(privateMarker, warning, StringComparison.Ordinal));
        Assert.All(
            terminal.Job.Metadata,
            metadata =>
            {
                Assert.DoesNotContain(privateMarker, metadata.Key, StringComparison.Ordinal);
                Assert.DoesNotContain(privateMarker, metadata.Value, StringComparison.Ordinal);
            });
        Assert.All(
            terminal.Provenance,
            provenance =>
            {
                Assert.DoesNotContain(privateMarker, provenance.Key, StringComparison.Ordinal);
                Assert.DoesNotContain(privateMarker, provenance.Value, StringComparison.Ordinal);
            });
        Assert.DoesNotContain(
            privateMarker,
            JsonSerializer.Serialize(terminal),
            StringComparison.Ordinal);
        Assert.Equal(["reserve"], events);
        Assert.Empty(evaluator.Requests);
        Assert.Empty(ledger.AttemptedRecords);
        Assert.Empty(artifactDb.AttemptedRecords);
        Assert.Equal(0, artifactDb.ListCalls);
        var period = await store.Inner.ReadPeriodAsync(BuildTrustedBudgetPeriodQuery(request));
        Assert.True(period.IsSuccess, period.ErrorMessage);
        Assert.Equal(0, period.Value!.ReservationCount);
        Assert.Equal(0, period.Value.ActiveReservationCount);
        Assert.Equal(0, period.Value.SettledReservationCount);
        Assert.Equal(0, period.Value.ReleasedReservationCount);
        Assert.Equal(0, period.Value.DeniedReservationCount);
        Assert.Equal(0m, period.Value.ActiveReservedUsd);
        Assert.Equal(0m, period.Value.FinalizedSpendUsd);
        Assert.Empty(period.Value.Reservations);
        Assert.Equal(0, runtime.LiveRunLeaseCount);
    }

    [Theory]
    [InlineData("missing-budget-tenant-ref")]
    [InlineData("mismatched-provenance-tenant-ref")]
    public async Task RunAsync_TrustedVisualCostTenantRefContractFailsBeforeEvaluator(string mode)
    {
        var events = new List<string>();
        var store = new EventRecordingVisualCostBudgetStore(
            new JsonlDesignConversionVisualCostBudgetStore(CreateBudgetStoreTempRoot($"trusted-tenant-{mode}")),
            events);
        var evaluator = new EventRecordingEvaluator(CreateDeterministicInternalEvaluator(), events);
        var runtime = new DesignConversionThinClientJobRuntime(
            evaluator,
            FixedClock(),
            visualCostBudgetStore: store);
        var request = WithTrustedVisualCostBudget(
            CreateSubmittedRequest(),
            monthlyLimitUsd: "1",
            budgetUsd: 0.25m);
        var budgetMetadata = new Dictionary<string, string>(request.Budget.Metadata, StringComparer.Ordinal);
        var provenance = new Dictionary<string, string>(request.Provenance, StringComparer.Ordinal);
        if (string.Equals(mode, "missing-budget-tenant-ref", StringComparison.Ordinal))
        {
            budgetMetadata.Remove("trusted_visual_cost_tenant_id_ref");
        }
        else
        {
            provenance["trusted_visual_cost_entitlement_tenant_id_ref"] =
                JsonlDesignConversionVisualCostLedger.BuildPseudonymousUserRef("tenant-visual-cost-other").Value!;
        }

        request = request with
        {
            Budget = request.Budget with { Metadata = budgetMetadata },
            Provenance = provenance
        };
        var submitted = runtime.Submit(request);
        Assert.True(submitted.IsSuccess, submitted.ErrorMessage);

        var result = await runtime.RunAsync(submitted.Value!.JobId);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionThinClientStatus.Failed, result.Value!.ContractStatus);
        Assert.Equal(
            DesignConversionThinClientJobRuntime.TrustedVisualCostBudgetContractInvalidErrorCode,
            result.Value.Job.Metadata["error_code"]);
        Assert.Empty(events);
        Assert.Empty(evaluator.Requests);
    }

    [Theory]
    [InlineData("free", "0.05")]
    [InlineData("paid", "0.25")]
    [InlineData("pro", "1")]
    public async Task RunAsync_TrustedVisualCostZeroBudgetUsesTierEntitlementBeforeReserveAndEvaluate(
        string tier,
        string expectedRequestedReserveUsdText)
    {
        var expectedRequestedReserveUsd = decimal.Parse(
            expectedRequestedReserveUsdText,
            NumberStyles.AllowDecimalPoint,
            CultureInfo.InvariantCulture);
        var events = new List<string>();
        var store = new EventRecordingVisualCostBudgetStore(
            new JsonlDesignConversionVisualCostBudgetStore(
                CreateBudgetStoreTempRoot($"trusted-zero-budget-{tier}")),
            events);
        var evaluator = new EventRecordingEvaluator(CreateDeterministicInternalEvaluator(), events);
        var ledger = new RecordingVisualCostLedger(
            new JsonlDesignConversionVisualCostLedger(
                CreateBudgetStoreTempRoot($"trusted-zero-budget-ledger-{tier}")));
        var artifactDb = new RecordingLocalArtifactDb(
            new JsonlDesignConversionLocalArtifactDb(
                CreateBudgetStoreTempRoot($"trusted-zero-budget-artifact-{tier}")));
        var runtime = new DesignConversionThinClientJobRuntime(
            evaluator,
            FixedClock(),
            artifactDb,
            ledger,
            store);
        var request = WithTrustedVisualCostBudget(
            CreateSubmittedRequest() with
            {
                RequestId = $"thin-trusted-zero-budget-{tier}"
            },
            monthlyLimitUsd: "1",
            budgetUsd: 0m,
            tier: tier);
        var submitted = runtime.Submit(request);
        Assert.True(submitted.IsSuccess, submitted.ErrorMessage);

        var result = await runtime.RunAsync(submitted.Value!.JobId);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var terminal = result.Value!;
        AssertThinClientNonReadyVisualDecision(
            terminal,
            DesignConversionVisualQaBoundaryPolicy.DecisionBlocked,
            DesignConversionVisualQaBoundaryPolicy.ProviderRequired);
        Assert.Equal(["reserve", "evaluate", "settle:0"], events);
        var evaluated = Assert.Single(evaluator.Requests);
        Assert.Equal(tier, evaluated.Budget.Tier);
        Assert.Equal(0m, evaluated.Budget.BudgetUsd);
        Assert.Single(ledger.AttemptedRecords);
        Assert.Single(artifactDb.AttemptedRecords);
        Assert.Equal(0, artifactDb.ListCalls);
        var period = await store.Inner.ReadPeriodAsync(BuildTrustedBudgetPeriodQuery(request));
        Assert.True(period.IsSuccess, period.ErrorMessage);
        Assert.Equal(1, period.Value!.ReservationCount);
        Assert.Equal(0, period.Value.ActiveReservationCount);
        Assert.Equal(1, period.Value.SettledReservationCount);
        Assert.Equal(0, period.Value.ReleasedReservationCount);
        Assert.Equal(0, period.Value.DeniedReservationCount);
        Assert.Equal(0m, period.Value.ActiveReservedUsd);
        Assert.Equal(0m, period.Value.FinalizedSpendUsd);
        var reservation = Assert.Single(period.Value.Reservations);
        Assert.Equal(JsonlDesignConversionVisualCostBudgetStore.StatusSettled, reservation.Status);
        Assert.Equal(tier, reservation.SubmittedTier);
        Assert.Equal(expectedRequestedReserveUsd, reservation.RequestedReserveUsd);
        Assert.Equal(0m, reservation.ActualCostUsd);
        Assert.Equal(0, runtime.LiveRunLeaseCount);
    }

    [Theory]
    [InlineData("paid", "0.10", "0.10")]
    [InlineData("paid", "0.50", "0.25")]
    public async Task RunAsync_TrustedVisualCostPositiveBudgetUsesMinimumOfSubmittedAndTierCap(
        string tier,
        string submittedBudgetUsdText,
        string expectedRequestedReserveUsdText)
    {
        var submittedBudgetUsd = decimal.Parse(
            submittedBudgetUsdText,
            NumberStyles.AllowDecimalPoint,
            CultureInfo.InvariantCulture);
        var expectedRequestedReserveUsd = decimal.Parse(
            expectedRequestedReserveUsdText,
            NumberStyles.AllowDecimalPoint,
            CultureInfo.InvariantCulture);
        var events = new List<string>();
        var observedReservationRequests = new List<DesignConversionVisualCostReservationRequest>();
        Func<
            DesignConversionVisualCostReservationRequest,
            CancellationToken,
            Task<OperationResult<DesignConversionVisualCostReservationResult>>> reserveOverride =
            (reservationRequest, _) =>
            {
                observedReservationRequests.Add(reservationRequest);
                return Task.FromResult(
                    OperationResult<DesignConversionVisualCostReservationResult>.Ok(
                        new DesignConversionVisualCostReservationResult(
                            Status: JsonlDesignConversionVisualCostBudgetStore.StatusDenied,
                            Reserved: false,
                            Replayed: false,
                            ReservationRef: reservationRequest.ReservationRef,
                            UserRef: reservationRequest.UserRef,
                            SubmittedTier: reservationRequest.SubmittedTier,
                            PeriodStartUtc: reservationRequest.PeriodStartUtc,
                            PeriodEndExclusiveUtc: reservationRequest.PeriodEndExclusiveUtc,
                            MonthlyLimitUsd: reservationRequest.MonthlyLimitUsd,
                            RequestedReserveUsd: reservationRequest.RequestedReserveUsd,
                            FinalizedSpendUsd: 0m,
                            ActiveReservedUsd: 0m,
                            RemainingBudgetUsd: reservationRequest.MonthlyLimitUsd,
                            DenialReasonCode: JsonlDesignConversionVisualCostBudgetStore.DenialReasonMonthlyLimitExceeded,
                            Metadata: new Dictionary<string, string>())));
            };
        var store = new EventRecordingVisualCostBudgetStore(
            new JsonlDesignConversionVisualCostBudgetStore(
                CreateBudgetStoreTempRoot($"trusted-positive-budget-{tier}-{submittedBudgetUsdText}")),
            events,
            reserveOverride: reserveOverride);
        var evaluator = new EventRecordingEvaluator(CreateDeterministicInternalEvaluator(), events);
        var runtime = new DesignConversionThinClientJobRuntime(
            evaluator,
            FixedClock(),
            visualCostBudgetStore: store);
        var request = WithTrustedVisualCostBudget(
            CreateSubmittedRequest() with
            {
                RequestId = $"thin-trusted-positive-budget-{tier}-{submittedBudgetUsdText}"
            },
            monthlyLimitUsd: "1",
            budgetUsd: submittedBudgetUsd,
            tier: tier);
        var submitted = runtime.Submit(request);
        Assert.True(submitted.IsSuccess, submitted.ErrorMessage);

        var result = await runtime.RunAsync(submitted.Value!.JobId);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionThinClientStatus.Failed, result.Value!.ContractStatus);
        Assert.Equal(
            JsonlDesignConversionVisualCostBudgetStore.DenialReasonMonthlyLimitExceeded,
            result.Value.Job.Metadata["error_code"]);
        Assert.Equal(["reserve"], events);
        Assert.Empty(evaluator.Requests);
        var observed = Assert.Single(observedReservationRequests);
        Assert.Equal(expectedRequestedReserveUsd, observed.RequestedReserveUsd);
        Assert.Equal(tier, observed.SubmittedTier);
        Assert.Equal(0, runtime.LiveRunLeaseCount);
    }

    [Theory]
    [InlineData("returned-failure")]
    [InlineData("thrown-exception")]
    [InlineData("cancelled")]
    public async Task RunAsync_TrustedVisualCostReservationDependencyFailureStopsBeforeEvaluator(
        string mode)
    {
        const string privateMarker = "private-reservation-store-marker-W051-L1R";
        var events = new List<string>();
        Func<
            DesignConversionVisualCostReservationRequest,
            CancellationToken,
            Task<OperationResult<DesignConversionVisualCostReservationResult>>> reserveOverride =
            mode switch
            {
                "returned-failure" => (_, _) => Task.FromResult(
                    OperationResult<DesignConversionVisualCostReservationResult>.Fail(
                        privateMarker,
                        "PRIVATE_RESERVATION_DEPENDENCY_FAILED")),
                "thrown-exception" => (_, _) =>
                    Task.FromException<OperationResult<DesignConversionVisualCostReservationResult>>(
                        new InvalidOperationException(privateMarker)),
                "cancelled" => (_, token) =>
                    Task.FromException<OperationResult<DesignConversionVisualCostReservationResult>>(
                        new OperationCanceledException(privateMarker, token)),
                _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unknown reservation failure mode.")
            };
        var store = new EventRecordingVisualCostBudgetStore(
            new JsonlDesignConversionVisualCostBudgetStore(
                CreateBudgetStoreTempRoot($"trusted-reservation-dependency-{mode}")),
            events,
            reserveOverride: reserveOverride);
        var evaluator = new EventRecordingEvaluator(CreateDeterministicInternalEvaluator(), events);
        var ledger = new RecordingVisualCostLedger(
            new JsonlDesignConversionVisualCostLedger(
                CreateBudgetStoreTempRoot($"trusted-reservation-dependency-ledger-{mode}")));
        var artifactDb = new RecordingLocalArtifactDb(
            new JsonlDesignConversionLocalArtifactDb(
                CreateBudgetStoreTempRoot($"trusted-reservation-dependency-artifact-{mode}")));
        var runtime = new DesignConversionThinClientJobRuntime(
            evaluator,
            FixedClock(),
            artifactDb,
            ledger,
            store);
        var request = WithTrustedVisualCostBudget(
            CreateSubmittedRequest() with
            {
                RequestId = $"thin-trusted-reservation-dependency-{mode}"
            },
            monthlyLimitUsd: "1",
            budgetUsd: 0.25m);
        var submitted = runtime.Submit(request);
        Assert.True(submitted.IsSuccess, submitted.ErrorMessage);

        var result = await runtime.RunAsync(submitted.Value!.JobId);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var terminal = result.Value!;
        var expectedStatus = string.Equals(mode, "cancelled", StringComparison.Ordinal)
            ? DesignConversionThinClientStatus.Cancelled
            : DesignConversionThinClientStatus.Failed;
        var expectedErrorCode = string.Equals(mode, "cancelled", StringComparison.Ordinal)
            ? "CANCELLED"
            : DesignConversionThinClientJobRuntime.TrustedVisualCostBudgetReservationFailedErrorCode;
        Assert.Equal(expectedStatus, terminal.ContractStatus);
        Assert.Equal(expectedStatus, terminal.Job.Status);
        Assert.Equal(expectedErrorCode, terminal.Job.Metadata["error_code"]);
        Assert.DoesNotContain(privateMarker, terminal.Message, StringComparison.Ordinal);
        Assert.All(
            terminal.Warnings,
            warning => Assert.DoesNotContain(privateMarker, warning, StringComparison.Ordinal));
        Assert.All(
            terminal.Job.Metadata,
            metadata =>
            {
                Assert.DoesNotContain(privateMarker, metadata.Key, StringComparison.Ordinal);
                Assert.DoesNotContain(privateMarker, metadata.Value, StringComparison.Ordinal);
            });
        Assert.All(
            terminal.Provenance,
            provenance =>
            {
                Assert.DoesNotContain(privateMarker, provenance.Key, StringComparison.Ordinal);
                Assert.DoesNotContain(privateMarker, provenance.Value, StringComparison.Ordinal);
            });
        Assert.DoesNotContain(
            privateMarker,
            JsonSerializer.Serialize(terminal),
            StringComparison.Ordinal);
        Assert.Equal(["reserve"], events);
        Assert.Empty(evaluator.Requests);
        Assert.Empty(ledger.AttemptedRecords);
        Assert.Empty(artifactDb.AttemptedRecords);
        Assert.Equal(0, artifactDb.ListCalls);
        var period = await store.Inner.ReadPeriodAsync(BuildTrustedBudgetPeriodQuery(request));
        Assert.True(period.IsSuccess, period.ErrorMessage);
        Assert.Equal(0, period.Value!.ReservationCount);
        Assert.Equal(0, period.Value.ActiveReservationCount);
        Assert.Equal(0, period.Value.SettledReservationCount);
        Assert.Equal(0, period.Value.ReleasedReservationCount);
        Assert.Equal(0, period.Value.DeniedReservationCount);
        Assert.Equal(0m, period.Value.ActiveReservedUsd);
        Assert.Equal(0m, period.Value.FinalizedSpendUsd);
        Assert.Empty(period.Value.Reservations);
        Assert.Equal(0, runtime.LiveRunLeaseCount);
    }

    [Fact]
    public async Task RunAsync_TrustedVisualCostReservationSettlesActualAndTerminalReplayIsIdempotent()
    {
        var events = new List<string>();
        var store = new EventRecordingVisualCostBudgetStore(
            new JsonlDesignConversionVisualCostBudgetStore(CreateBudgetStoreTempRoot("trusted-success")),
            events);
        var ledger = new JsonlDesignConversionVisualCostLedger(CreateBudgetStoreTempRoot("trusted-success-ledger"));
        var receipt = BuildServerOwnedReceipt("1", "0.0600", "0.0600");
        var visualJudge = new ReceiptVisualJudge(receipt, receipt, estimatedCostUsd: 0.0600m);
        var evaluator = new EventRecordingEvaluator(CreateVisualJudgeInternalEvaluator(visualJudge), events);
        var runtime = new DesignConversionThinClientJobRuntime(
            evaluator,
            FixedClock(),
            visualCostLedger: ledger,
            visualCostBudgetStore: store);
        var request = WithTrustedVisualCostBudget(
            CreateSubmittedRequest(),
            monthlyLimitUsd: "1",
            budgetUsd: 0.25m);
        var submitted = runtime.Submit(request);
        Assert.True(submitted.IsSuccess, submitted.ErrorMessage);

        var firstRun = await runtime.RunAsync(submitted.Value!.JobId);
        var replay = await runtime.RunAsync(submitted.Value.JobId);

        Assert.True(firstRun.IsSuccess, firstRun.ErrorMessage);
        Assert.True(replay.IsSuccess, replay.ErrorMessage);
        AssertThinClientQuarantinedVisualDecision(firstRun.Value!);
        AssertThinClientQuarantinedVisualDecision(replay.Value!);
        Assert.Equal(["reserve", "evaluate", "settle:0.06"], events);
        Assert.Single(evaluator.Requests);
        Assert.Equal(1, visualJudge.InvocationCount);
        Assert.Equal(bool.TrueString, firstRun.Value.Provenance["trusted_visual_cost_reservation_store_wired"]);
        Assert.Equal("0.06", firstRun.Value.Provenance["trusted_visual_cost_budget_charged_cost_usd"]);
        Assert.Equal(bool.TrueString, firstRun.Value.Provenance["trusted_visual_cost_budget_actual_cost_known"]);
        Assert.Equal(bool.FalseString, firstRun.Value.Provenance["trusted_visual_cost_budget_release_used"]);
        var period = await store.Inner.ReadPeriodAsync(BuildTrustedBudgetPeriodQuery(request));
        Assert.True(period.IsSuccess, period.ErrorMessage);
        Assert.Equal(1, period.Value!.SettledReservationCount);
        Assert.Equal(0, period.Value.ActiveReservationCount);
        Assert.Equal(0, period.Value.ReleasedReservationCount);
        Assert.Equal(0.0600m, period.Value.FinalizedSpendUsd);
    }

    [Fact]
    public async Task RunAsync_TrustedVisualCostSettlementReturnedFailureUsesGenericPublicCode()
    {
        const string privateMessage = "private-settlement-dependency-message-W051-L1SR";
        const string privateCode = "DESIGN_CONVERSION_PRIVATE_TENANT_ALPHA_SETTLEMENT";
        var events = new List<string>();
        var store = new EventRecordingVisualCostBudgetStore(
            new JsonlDesignConversionVisualCostBudgetStore(
                CreateBudgetStoreTempRoot("trusted-settlement-returned-failure-generic")),
            events,
            settleOverride: (_, _) => Task.FromResult(
                OperationResult<DesignConversionVisualCostSettlementResult>.Fail(
                    privateMessage,
                    privateCode)));
        var ledger = new RecordingVisualCostLedger(
            new JsonlDesignConversionVisualCostLedger(
                CreateBudgetStoreTempRoot("trusted-settlement-returned-failure-ledger")));
        var artifactDb = new RecordingLocalArtifactDb(
            new JsonlDesignConversionLocalArtifactDb(
                CreateBudgetStoreTempRoot("trusted-settlement-returned-failure-artifact")));
        var receipt = BuildServerOwnedReceipt("1", "0.0600", "0.0600");
        var visualJudge = new ReceiptVisualJudge(receipt, receipt, estimatedCostUsd: 0.0600m);
        var evaluator = new EventRecordingEvaluator(CreateVisualJudgeInternalEvaluator(visualJudge), events);
        var runtime = new DesignConversionThinClientJobRuntime(
            evaluator,
            FixedClock(),
            artifactDb,
            ledger,
            store);
        var request = WithTrustedVisualCostBudget(
            CreateSubmittedRequest() with
            {
                RequestId = "trusted-settlement-returned-failure-generic-request"
            },
            monthlyLimitUsd: "1",
            budgetUsd: 0.25m);
        var submitted = runtime.Submit(request);
        Assert.True(submitted.IsSuccess, submitted.ErrorMessage);

        var result = await runtime.RunAsync(submitted.Value!.JobId);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var terminal = result.Value!;
        Assert.Equal(DesignConversionThinClientStatus.Failed, terminal.ContractStatus);
        Assert.Equal(DesignConversionThinClientStatus.Failed, terminal.Job.Status);
        Assert.Equal(
            DesignConversionThinClientJobRuntime.TrustedVisualCostBudgetSettlementFailedErrorCode,
            terminal.Job.Metadata["error_code"]);
        Assert.Equal(
            "DesignConversion trusted visual-cost budget settlement failed closed.",
            terminal.Message);
        Assert.Equal(
            DesignConversionThinClientJobRuntime.TrustedVisualCostBudgetSettlementFailedErrorCode,
            terminal.Job.Metadata["post_evaluation_finalization_error_code"]);
        Assert.Equal(
            DesignConversionThinClientJobRuntime.TrustedVisualCostBudgetSettlementFailedErrorCode,
            terminal.Provenance["post_evaluation_finalization_error_code"]);
        Assert.Contains(
            "post_evaluation_finalization_error_code=" +
            DesignConversionThinClientJobRuntime.TrustedVisualCostBudgetSettlementFailedErrorCode,
            terminal.Warnings);
        AssertNoDependencyPrivateMarkerLeakage(terminal, privateMessage, privateCode);
        Assert.Equal(["reserve", "evaluate", "settle:0.06", "release"], events);
        Assert.Single(evaluator.Requests);
        Assert.Equal(1, visualJudge.InvocationCount);
        Assert.Equal(1, store.SettlementCalls);
        Assert.Equal(1, store.ReleaseCalls);
        Assert.Empty(ledger.AttemptedRecords);
        Assert.Empty(artifactDb.AttemptedRecords);
        Assert.Equal(0, artifactDb.ListCalls);
        var period = await store.Inner.ReadPeriodAsync(BuildTrustedBudgetPeriodQuery(request));
        Assert.True(period.IsSuccess, period.ErrorMessage);
        Assert.Equal(1, period.Value!.ReservationCount);
        Assert.Equal(0, period.Value.ActiveReservationCount);
        Assert.Equal(0, period.Value.SettledReservationCount);
        Assert.Equal(1, period.Value.ReleasedReservationCount);
        Assert.Equal(0m, period.Value.ActiveReservedUsd);
        Assert.Equal(0m, period.Value.FinalizedSpendUsd);
        Assert.Equal(0, runtime.LiveRunLeaseCount);
    }

    [Fact]
    public async Task RunAsync_TrustedVisualCostSameUserAcrossTenantsHasDistinctBudgetPeriods()
    {
        var events = new List<string>();
        var store = new EventRecordingVisualCostBudgetStore(
            new JsonlDesignConversionVisualCostBudgetStore(CreateBudgetStoreTempRoot("trusted-tenant-isolation")),
            events);
        var firstLedger = new JsonlDesignConversionVisualCostLedger(CreateBudgetStoreTempRoot("trusted-tenant-isolation-ledger-a"));
        var firstReceipt = BuildServerOwnedReceipt("1", "0.0600", "0.0600");
        var firstVisualJudge = new ReceiptVisualJudge(firstReceipt, firstReceipt, estimatedCostUsd: 0.0600m);
        var firstEvaluator = new EventRecordingEvaluator(CreateVisualJudgeInternalEvaluator(firstVisualJudge), events);
        var firstRuntime = new DesignConversionThinClientJobRuntime(
            firstEvaluator,
            FixedClock(),
            visualCostLedger: firstLedger,
            visualCostBudgetStore: store);
        var secondLedger = new JsonlDesignConversionVisualCostLedger(CreateBudgetStoreTempRoot("trusted-tenant-isolation-ledger-b"));
        var secondReceipt = BuildServerOwnedReceipt("1", "0.0600", "0.0600");
        var secondVisualJudge = new ReceiptVisualJudge(secondReceipt, secondReceipt, estimatedCostUsd: 0.0600m);
        var secondEvaluator = new EventRecordingEvaluator(CreateVisualJudgeInternalEvaluator(secondVisualJudge), events);
        var secondRuntime = new DesignConversionThinClientJobRuntime(
            secondEvaluator,
            FixedClock(),
            visualCostLedger: secondLedger,
            visualCostBudgetStore: store);
        var firstRequest = WithTrustedVisualCostBudget(
            CreateSubmittedRequest(),
            monthlyLimitUsd: "1",
            budgetUsd: 0.25m,
            userRefSource: "same-platform-user",
            tenantRefSource: "tenant-a");
        var secondRequest = WithTrustedVisualCostBudget(
            CreateSubmittedRequest(),
            monthlyLimitUsd: "1",
            budgetUsd: 0.25m,
            userRefSource: "same-platform-user",
            tenantRefSource: "tenant-b");
        var firstSubmit = firstRuntime.Submit(firstRequest);
        var secondSubmit = secondRuntime.Submit(secondRequest);
        Assert.True(firstSubmit.IsSuccess, firstSubmit.ErrorMessage);
        Assert.True(secondSubmit.IsSuccess, secondSubmit.ErrorMessage);

        var firstRun = await firstRuntime.RunAsync(firstSubmit.Value!.JobId);
        var secondRun = await secondRuntime.RunAsync(secondSubmit.Value!.JobId);

        Assert.True(firstRun.IsSuccess, firstRun.ErrorMessage);
        Assert.True(secondRun.IsSuccess, secondRun.ErrorMessage);
        AssertThinClientQuarantinedVisualDecision(firstRun.Value!);
        AssertThinClientQuarantinedVisualDecision(secondRun.Value!);
        var firstPeriodQuery = BuildTrustedBudgetPeriodQuery(firstRequest);
        var secondPeriodQuery = BuildTrustedBudgetPeriodQuery(secondRequest);
        Assert.NotEqual(firstPeriodQuery.UserRef, secondPeriodQuery.UserRef);
        var firstPeriod = await store.Inner.ReadPeriodAsync(firstPeriodQuery);
        var secondPeriod = await store.Inner.ReadPeriodAsync(secondPeriodQuery);
        Assert.True(firstPeriod.IsSuccess, firstPeriod.ErrorMessage);
        Assert.True(secondPeriod.IsSuccess, secondPeriod.ErrorMessage);
        Assert.Equal(1, firstPeriod.Value!.SettledReservationCount);
        Assert.Equal(1, secondPeriod.Value!.SettledReservationCount);
        Assert.Equal(0.0600m, firstPeriod.Value.FinalizedSpendUsd);
        Assert.Equal(0.0600m, secondPeriod.Value.FinalizedSpendUsd);
        Assert.Single(firstEvaluator.Requests);
        Assert.Single(secondEvaluator.Requests);
    }

    [Theory]
    [InlineData("paid-in-cap", 0.10, "0.1")]
    [InlineData("paid-clamped", 12.34, "0.25")]
    public async Task RunAsync_TrustedVisualCostReservationUsesEffectiveTierBudget(
        string mode,
        decimal submittedBudgetUsd,
        string expectedReserveUsd)
    {
        var events = new List<string>();
        var store = new EventRecordingVisualCostBudgetStore(
            new JsonlDesignConversionVisualCostBudgetStore(CreateBudgetStoreTempRoot($"trusted-effective-budget-{mode}")),
            events);
        var ledger = new JsonlDesignConversionVisualCostLedger(CreateBudgetStoreTempRoot($"trusted-effective-budget-{mode}-ledger"));
        var receipt = BuildServerOwnedReceipt("1", "0.0600", "0.0600");
        var visualJudge = new ReceiptVisualJudge(receipt, receipt, estimatedCostUsd: 0.0600m);
        var evaluator = new EventRecordingEvaluator(CreateVisualJudgeInternalEvaluator(visualJudge), events);
        var runtime = new DesignConversionThinClientJobRuntime(
            evaluator,
            FixedClock(),
            visualCostLedger: ledger,
            visualCostBudgetStore: store);
        var request = WithTrustedVisualCostBudget(
            CreateSubmittedRequest(),
            monthlyLimitUsd: "1",
            budgetUsd: submittedBudgetUsd,
            tier: "paid",
            tenantRefSource: $"tenant-effective-budget-{mode}");
        var submitted = runtime.Submit(request);
        Assert.True(submitted.IsSuccess, submitted.ErrorMessage);

        var result = await runtime.RunAsync(submitted.Value!.JobId);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var status = result.Value!;
        AssertThinClientQuarantinedVisualDecision(status);
        Assert.Equal(expectedReserveUsd, status.Provenance["trusted_visual_cost_budget_requested_reserve_usd"]);
        Assert.Equal(["reserve", "evaluate", "settle:0.06"], events);
    }

    [Fact]
    public async Task RunAsync_TrustedVisualCostEvaluatorFailureSettlesFullReserveWithoutRelease()
    {
        var events = new List<string>();
        var store = new EventRecordingVisualCostBudgetStore(
            new JsonlDesignConversionVisualCostBudgetStore(CreateBudgetStoreTempRoot("trusted-failure")),
            events);
        var evaluator = new EventRecordingEvaluator(
            new FailingEvaluator("scripted visual provider failed", "SCRIPTED_VISUAL_PROVIDER_FAILED"),
            events);
        var runtime = new DesignConversionThinClientJobRuntime(
            evaluator,
            FixedClock(),
            visualCostBudgetStore: store);
        var request = WithTrustedVisualCostBudget(
            CreateSubmittedRequest(),
            monthlyLimitUsd: "1",
            budgetUsd: 0.25m);
        var submitted = runtime.Submit(request);
        Assert.True(submitted.IsSuccess, submitted.ErrorMessage);

        var result = await runtime.RunAsync(submitted.Value!.JobId);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionThinClientStatus.Failed, result.Value!.ContractStatus);
        Assert.Equal("SCRIPTED_VISUAL_PROVIDER_FAILED", result.Value.Job.Metadata["error_code"]);
        Assert.Equal(["reserve", "evaluate", "settle:0.25"], events);
        Assert.Single(evaluator.Requests);
        Assert.Equal(bool.FalseString, result.Value.Provenance["trusted_visual_cost_budget_actual_cost_known"]);
        Assert.Equal(bool.FalseString, result.Value.Provenance["trusted_visual_cost_budget_release_used"]);
        var period = await store.Inner.ReadPeriodAsync(BuildTrustedBudgetPeriodQuery(request));
        Assert.True(period.IsSuccess, period.ErrorMessage);
        Assert.Equal(1, period.Value!.SettledReservationCount);
        Assert.Equal(0, period.Value.ActiveReservationCount);
        Assert.Equal(0, period.Value.ReleasedReservationCount);
        Assert.Equal(0.25m, period.Value.FinalizedSpendUsd);
    }

    [Fact]
    public async Task RunAsync_TrustedVisualCostCancellationAfterEvaluatorInvocationSettlesFullReserve()
    {
        var events = new List<string>();
        var store = new EventRecordingVisualCostBudgetStore(
            new JsonlDesignConversionVisualCostBudgetStore(CreateBudgetStoreTempRoot("trusted-cancel-after-invocation")),
            events);
        using var cts = new CancellationTokenSource();
        var evaluator = new EventRecordingEvaluator(new CancellingAfterInvocationEvaluator(cts), events);
        var runtime = new DesignConversionThinClientJobRuntime(
            evaluator,
            FixedClock(),
            visualCostBudgetStore: store);
        var request = WithTrustedVisualCostBudget(
            CreateSubmittedRequest(),
            monthlyLimitUsd: "1",
            budgetUsd: 0.25m);
        var submitted = runtime.Submit(request);
        Assert.True(submitted.IsSuccess, submitted.ErrorMessage);

        var result = await runtime.RunAsync(submitted.Value!.JobId, cts.Token);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionThinClientStatus.Cancelled, result.Value!.ContractStatus);
        Assert.Equal("CANCELLED", result.Value.Job.Metadata["error_code"]);
        Assert.Equal(["reserve", "evaluate", "settle:0.25"], events);
        Assert.Single(evaluator.Requests);
        var period = await store.Inner.ReadPeriodAsync(BuildTrustedBudgetPeriodQuery(request));
        Assert.True(period.IsSuccess, period.ErrorMessage);
        Assert.Equal(1, period.Value!.SettledReservationCount);
        Assert.Equal(0, period.Value.ActiveReservationCount);
        Assert.Equal(0, period.Value.ReleasedReservationCount);
        Assert.Equal(0.25m, period.Value.FinalizedSpendUsd);
    }

    [Fact]
    public async Task RunAsync_TrustedVisualCostIncompleteAccountingSettlesFullReserveAndFails()
    {
        var events = new List<string>();
        var store = new EventRecordingVisualCostBudgetStore(
            new JsonlDesignConversionVisualCostBudgetStore(CreateBudgetStoreTempRoot("trusted-incomplete")),
            events);
        var evaluator = new EventRecordingEvaluator(
            new IncompleteAccountingEvaluator(CreateDeterministicInternalEvaluator()),
            events);
        var runtime = new DesignConversionThinClientJobRuntime(
            evaluator,
            FixedClock(),
            visualCostBudgetStore: store);
        var request = WithTrustedVisualCostBudget(
            CreateSubmittedRequest(),
            monthlyLimitUsd: "1",
            budgetUsd: 0.25m);
        var submitted = runtime.Submit(request);
        Assert.True(submitted.IsSuccess, submitted.ErrorMessage);

        var result = await runtime.RunAsync(submitted.Value!.JobId);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionThinClientStatus.Failed, result.Value!.ContractStatus);
        Assert.Equal(
            DesignConversionThinClientJobRuntime.TrustedVisualCostAccountingIncompleteErrorCode,
            result.Value.Job.Metadata["error_code"]);
        Assert.Equal(["reserve", "evaluate", "settle:0.25"], events);
        Assert.Equal(bool.FalseString, result.Value.Provenance["trusted_visual_cost_budget_actual_cost_known"]);
        Assert.Equal(bool.FalseString, result.Value.Provenance["trusted_visual_cost_budget_release_used"]);
        var period = await store.Inner.ReadPeriodAsync(BuildTrustedBudgetPeriodQuery(request));
        Assert.True(period.IsSuccess, period.ErrorMessage);
        Assert.Equal(1, period.Value!.SettledReservationCount);
        Assert.Equal(0.25m, period.Value.FinalizedSpendUsd);
    }

    [Fact]
    public async Task RunAsync_TrustedVisualCostActualAboveReserveSettlesFullReserveAndFails()
    {
        var events = new List<string>();
        var store = new EventRecordingVisualCostBudgetStore(
            new JsonlDesignConversionVisualCostBudgetStore(CreateBudgetStoreTempRoot("trusted-actual-over-reserve")),
            events);
        var receipt = BuildServerOwnedReceipt("1", "0.3000", "0.3000");
        var visualJudge = new ReceiptVisualJudge(receipt, receipt, estimatedCostUsd: 0.3000m);
        var evaluator = new EventRecordingEvaluator(CreateVisualJudgeInternalEvaluator(visualJudge), events);
        var runtime = new DesignConversionThinClientJobRuntime(
            evaluator,
            FixedClock(),
            visualCostBudgetStore: store);
        var request = WithTrustedVisualCostBudget(
            CreateSubmittedRequest(),
            monthlyLimitUsd: "1",
            budgetUsd: 0.25m);
        var submitted = runtime.Submit(request);
        Assert.True(submitted.IsSuccess, submitted.ErrorMessage);

        var result = await runtime.RunAsync(submitted.Value!.JobId);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionThinClientStatus.Failed, result.Value!.ContractStatus);
        Assert.Equal(
            DesignConversionThinClientJobRuntime.TrustedVisualCostActualExceedsReservationErrorCode,
            result.Value.Job.Metadata["error_code"]);
        Assert.Equal(["reserve", "evaluate", "settle:0.25"], events);
        Assert.Equal(bool.FalseString, result.Value.Provenance["trusted_visual_cost_budget_actual_cost_known"]);
        Assert.Equal(bool.FalseString, result.Value.Provenance["trusted_visual_cost_budget_release_used"]);
        var period = await store.Inner.ReadPeriodAsync(BuildTrustedBudgetPeriodQuery(request));
        Assert.True(period.IsSuccess, period.ErrorMessage);
        Assert.Equal(1, period.Value!.SettledReservationCount);
        Assert.Equal(0, period.Value.ActiveReservationCount);
        Assert.Equal(0, period.Value.ReleasedReservationCount);
        Assert.Equal(0.25m, period.Value.FinalizedSpendUsd);
    }

    [Fact]
    public async Task RunAsync_UnboundLocalRequestDoesNotUseVisualCostBudgetStore()
    {
        var events = new List<string>();
        var store = new EventRecordingVisualCostBudgetStore(
            new JsonlDesignConversionVisualCostBudgetStore(CreateBudgetStoreTempRoot("unbound-local")),
            events);
        var evaluator = new EventRecordingEvaluator(CreateDeterministicInternalEvaluator(), events);
        var runtime = new DesignConversionThinClientJobRuntime(
            evaluator,
            FixedClock(),
            visualCostBudgetStore: store);
        var submitted = runtime.Submit(CreateSubmittedRequest());
        Assert.True(submitted.IsSuccess, submitted.ErrorMessage);

        var result = await runtime.RunAsync(submitted.Value!.JobId);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        AssertThinClientNonReadyVisualDecision(
            result.Value!,
            DesignConversionVisualQaBoundaryPolicy.DecisionBlocked,
            DesignConversionVisualQaBoundaryPolicy.ProviderRequired);
        Assert.Equal(["evaluate"], events);
        Assert.Single(evaluator.Requests);
        Assert.Equal(0, runtime.LiveRunLeaseCount);
    }

    [Fact]
    public void Constructor_RejectsNonPositiveStaleRunningTimeout()
    {
        var evaluator = new RecordingEvaluator(CreateDeterministicInternalEvaluator());

        var error = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new DesignConversionThinClientJobRuntime(
                evaluator,
                FixedClock(),
                staleRunningTimeout: TimeSpan.Zero));

        Assert.Equal("staleRunningTimeout", error.ParamName);
    }

    [Fact]
    public void Constructor_RejectsNonPositivePostEvaluationFinalizationTimeout()
    {
        var evaluator = new RecordingEvaluator(CreateDeterministicInternalEvaluator());

        var error = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new DesignConversionThinClientJobRuntime(
                evaluator,
                FixedClock(),
                postEvaluationFinalizationTimeout: TimeSpan.Zero));

        Assert.Equal("postEvaluationFinalizationTimeout", error.ParamName);
    }

    [Fact]
    public async Task GetStatus_WhenRunningMarkerExceedsStaleTimeoutFailsClosedWithoutRawLeakage()
    {
        var now = DateTimeOffset.UnixEpoch;
        var timeout = TimeSpan.FromMinutes(5);
        var evaluator = new BlockingEvaluator(CreateDeterministicInternalEvaluator());
        var runtime = new DesignConversionThinClientJobRuntime(
            evaluator,
            () => now,
            staleRunningTimeout: timeout);
        var request = CreatePrivateSubmittedRequest();
        var submitted = runtime.Submit(request);
        Assert.True(submitted.IsSuccess, submitted.ErrorMessage);

        var firstRun = runtime.RunAsync(submitted.Value!.JobId);
        await evaluator.Invoked.Task.WaitAsync(TimeSpan.FromSeconds(5));
        now = now.Add(timeout).AddSeconds(1);

        var status = runtime.GetStatus(submitted.Value.JobId);

        Assert.True(status.IsSuccess, status.ErrorMessage);
        AssertStaleRunningFailure(status.Value!, timeout);
        AssertNoPrivateSubmittedPayloadLeakage(status.Value!, request);
        Assert.Single(evaluator.Requests);

        evaluator.Release();
        var firstRunReadback = await firstRun.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.True(firstRunReadback.IsSuccess, firstRunReadback.ErrorMessage);
        AssertStaleRunningFailure(firstRunReadback.Value!, timeout);
        AssertNoPrivateSubmittedPayloadLeakage(firstRunReadback.Value!, request);
        Assert.Single(evaluator.Requests);
        Assert.Equal(0, runtime.LiveRunLeaseCount);
    }

    [Fact]
    public async Task RunAsync_AfterStaleCleanupReturnsTerminalFailureWithoutRerunningEvaluator()
    {
        var now = DateTimeOffset.UnixEpoch;
        var timeout = TimeSpan.FromMinutes(5);
        var evaluator = new BlockingEvaluator(CreateDeterministicInternalEvaluator());
        var runtime = new DesignConversionThinClientJobRuntime(
            evaluator,
            () => now,
            staleRunningTimeout: timeout);
        var submitted = runtime.Submit(CreatePrivateSubmittedRequest());
        Assert.True(submitted.IsSuccess, submitted.ErrorMessage);

        var firstRun = runtime.RunAsync(submitted.Value!.JobId);
        await evaluator.Invoked.Task.WaitAsync(TimeSpan.FromSeconds(5));
        now = now.Add(timeout).AddSeconds(1);
        var staleReadback = runtime.GetStatus(submitted.Value.JobId);
        Assert.True(staleReadback.IsSuccess, staleReadback.ErrorMessage);

        var replay = await runtime.RunAsync(submitted.Value.JobId);

        Assert.True(replay.IsSuccess, replay.ErrorMessage);
        AssertStaleRunningFailure(staleReadback.Value!, timeout);
        AssertStaleRunningFailure(replay.Value!, timeout);
        Assert.Equal(staleReadback.Value!.Job.UpdatedAtUtc, replay.Value!.Job.UpdatedAtUtc);
        Assert.Equal(staleReadback.Value.Message, replay.Value.Message);
        Assert.Single(evaluator.Requests);

        evaluator.Release();
        var firstRunReadback = await firstRun.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.True(firstRunReadback.IsSuccess, firstRunReadback.ErrorMessage);
        AssertStaleRunningFailure(firstRunReadback.Value!, timeout);
        Assert.Single(evaluator.Requests);
    }

    [Fact]
    public async Task RunAsync_WhenStaleCleanupBeatsLateSuccessfulEvaluatorReleasesBudgetAndBlocksSideEffects()
    {
        var now = DateTimeOffset.UnixEpoch;
        var timeout = TimeSpan.FromMinutes(5);
        var events = new List<string>();
        var blockingEvaluator = new BlockingEvaluator(CreateRuntimeCandidateEligibleInternalEvaluator());
        var evaluator = new EventRecordingEvaluator(blockingEvaluator, events);
        var ledger = new RecordingVisualCostLedger(
            new JsonlDesignConversionVisualCostLedger(CreateBudgetStoreTempRoot("stale-release-ledger")));
        var artifactDb = new RecordingLocalArtifactDb(
            new JsonlDesignConversionLocalArtifactDb(CreateBudgetStoreTempRoot("stale-release-artifact-db")));
        var store = new EventRecordingVisualCostBudgetStore(
            new JsonlDesignConversionVisualCostBudgetStore(CreateBudgetStoreTempRoot("stale-release-budget")),
            events);
        var runtime = new DesignConversionThinClientJobRuntime(
            evaluator,
            () => now,
            artifactDb,
            ledger,
            store,
            staleRunningTimeout: timeout);
        var request = WithTrustedVisualCostBudget(
            CreateRuntimeCandidateEligiblePrivateSubmittedRequest() with
            {
                RequestId = "thin-stale-release-side-effects-request"
            },
            monthlyLimitUsd: "1",
            budgetUsd: 0.25m);
        var submitted = runtime.Submit(request);
        Assert.True(submitted.IsSuccess, submitted.ErrorMessage);

        var firstRun = runtime.RunAsync(submitted.Value!.JobId);
        await blockingEvaluator.Invoked.Task.WaitAsync(TimeSpan.FromSeconds(5));
        now = now.Add(timeout).AddSeconds(1);
        var staleReadback = runtime.GetStatus(submitted.Value.JobId);
        Assert.True(staleReadback.IsSuccess, staleReadback.ErrorMessage);
        await store.ReleaseObserved.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal(["reserve", "evaluate", "release"], events);
        Assert.Equal(1, store.ReleaseCalls);
        Assert.Equal(0, store.SettlementCalls);
        Assert.False(firstRun.IsCompleted);

        var concurrentReadback = runtime.GetStatus(submitted.Value.JobId);
        var concurrentReplay = await runtime.RunAsync(submitted.Value.JobId);
        Assert.True(concurrentReadback.IsSuccess, concurrentReadback.ErrorMessage);
        Assert.True(concurrentReplay.IsSuccess, concurrentReplay.ErrorMessage);
        AssertStaleRunningFailure(concurrentReadback.Value!, timeout);
        AssertStaleRunningFailure(concurrentReplay.Value!, timeout);
        Assert.Equal(1, store.ReleaseCalls);

        var periodBeforeEvaluatorCompletion = await store.Inner.ReadPeriodAsync(BuildTrustedBudgetPeriodQuery(request));
        Assert.True(periodBeforeEvaluatorCompletion.IsSuccess, periodBeforeEvaluatorCompletion.ErrorMessage);
        Assert.Equal(0, periodBeforeEvaluatorCompletion.Value!.ActiveReservationCount);
        Assert.Equal(1, periodBeforeEvaluatorCompletion.Value.ReleasedReservationCount);

        blockingEvaluator.Release();
        var firstRunReadback = await firstRun.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.True(firstRunReadback.IsSuccess, firstRunReadback.ErrorMessage);
        AssertStaleRunningFailure(staleReadback.Value!, timeout);
        AssertStaleRunningFailure(firstRunReadback.Value!, timeout);
        Assert.Equal(["reserve", "evaluate", "release"], events);
        Assert.Equal(1, store.ReleaseCalls);
        Assert.Equal(0, store.SettlementCalls);
        Assert.Single(evaluator.Requests);
        Assert.Single(blockingEvaluator.Requests);
        Assert.Empty(ledger.AttemptedRecords);
        var ledgerRead = await ledger.ListRecordsAsync();
        Assert.True(ledgerRead.IsSuccess, ledgerRead.ErrorMessage);
        Assert.Empty(ledgerRead.Value!);
        Assert.Empty(artifactDb.AttemptedRecords);
        var artifactRead = await artifactDb.ListRecordsAsync(new DesignConversionLocalArtifactDbQuery(JobId: submitted.Value.JobId));
        Assert.True(artifactRead.IsSuccess, artifactRead.ErrorMessage);
        Assert.Empty(artifactRead.Value!);
        var period = await store.Inner.ReadPeriodAsync(BuildTrustedBudgetPeriodQuery(request));
        Assert.True(period.IsSuccess, period.ErrorMessage);
        Assert.Equal(1, period.Value!.ReservationCount);
        Assert.Equal(0, period.Value.ActiveReservationCount);
        Assert.Equal(0, period.Value.SettledReservationCount);
        Assert.Equal(1, period.Value.ReleasedReservationCount);
        Assert.Equal(0m, period.Value.FinalizedSpendUsd);
        Assert.Equal(0m, period.Value.ActiveReservedUsd);
        Assert.Equal(0, runtime.LiveRunLeaseCount);
        Assert.Equal(bool.FalseString, firstRunReadback.Value!.Provenance["trusted_visual_cost_budget_settled"]);
        Assert.Equal(bool.TrueString, firstRunReadback.Value.Provenance["trusted_visual_cost_budget_release_used"]);
        Assert.Equal(
            "release_after_stale_terminal_cleanup_before_settlement",
            firstRunReadback.Value.Provenance["trusted_visual_cost_budget_enforcement"]);
        Assert.Contains("trusted_visual_cost_budget_settled=false", firstRunReadback.Value.Warnings);
        Assert.Contains("trusted_visual_cost_budget_release_used=true", firstRunReadback.Value.Warnings);
        AssertNoPrivateSubmittedPayloadLeakage(firstRunReadback.Value, request);
    }

    [Fact]
    public async Task RunAsync_WhenStaleReleaseReturnsFailureUsesGenericPublicCode()
    {
        const string privateMessage = "private-release-dependency-message-W051-L1SR";
        const string privateCode = "DESIGN_CONVERSION_PRIVATE_TENANT_ALPHA_RELEASE";
        var now = DateTimeOffset.UnixEpoch;
        var timeout = TimeSpan.FromMinutes(5);
        var events = new List<string>();
        var blockingEvaluator = new BlockingEvaluator(CreateRuntimeCandidateEligibleInternalEvaluator());
        var evaluator = new EventRecordingEvaluator(blockingEvaluator, events);
        var ledger = new RecordingVisualCostLedger(
            new JsonlDesignConversionVisualCostLedger(
                CreateBudgetStoreTempRoot("stale-release-returned-failure-ledger")));
        var artifactDb = new RecordingLocalArtifactDb(
            new JsonlDesignConversionLocalArtifactDb(
                CreateBudgetStoreTempRoot("stale-release-returned-failure-artifact")));
        var store = new EventRecordingVisualCostBudgetStore(
            new JsonlDesignConversionVisualCostBudgetStore(
                CreateBudgetStoreTempRoot("stale-release-returned-failure-budget")),
            events,
            releaseOverride: (_, _) => Task.FromResult(
                OperationResult<DesignConversionVisualCostReleaseResult>.Fail(
                    privateMessage,
                    privateCode)));
        var runtime = new DesignConversionThinClientJobRuntime(
            evaluator,
            () => now,
            artifactDb,
            ledger,
            store,
            staleRunningTimeout: timeout);
        var request = WithTrustedVisualCostBudget(
            CreateRuntimeCandidateEligiblePrivateSubmittedRequest() with
            {
                RequestId = "stale-release-returned-failure-generic-request"
            },
            monthlyLimitUsd: "1",
            budgetUsd: 0.25m);
        var submitted = runtime.Submit(request);
        Assert.True(submitted.IsSuccess, submitted.ErrorMessage);

        var firstRun = runtime.RunAsync(submitted.Value!.JobId);
        await blockingEvaluator.Invoked.Task.WaitAsync(TimeSpan.FromSeconds(5));
        now = now.Add(timeout).AddSeconds(1);
        var staleReadback = runtime.GetStatus(submitted.Value.JobId);
        Assert.True(staleReadback.IsSuccess, staleReadback.ErrorMessage);
        await store.ReleaseObserved.Task.WaitAsync(TimeSpan.FromSeconds(5));
        DesignConversionThinClientStatusResult? terminal = null;
        for (var attempt = 0; attempt < 100; attempt++)
        {
            var releaseFailureReadback = runtime.GetStatus(submitted.Value.JobId);
            Assert.True(releaseFailureReadback.IsSuccess, releaseFailureReadback.ErrorMessage);
            if (releaseFailureReadback.Value!.Provenance.ContainsKey(
                    "trusted_visual_cost_budget_release_failure_code"))
            {
                terminal = releaseFailureReadback.Value;
                break;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(10));
        }

        Assert.NotNull(terminal);

        AssertStaleRunningFailure(terminal, timeout);
        Assert.Equal(
            DesignConversionThinClientJobRuntime.TrustedVisualCostBudgetSettlementFailedErrorCode,
            terminal.Job.Metadata["trusted_visual_cost_budget_release_failure_code"]);
        Assert.Equal(
            DesignConversionThinClientJobRuntime.TrustedVisualCostBudgetSettlementFailedErrorCode,
            terminal.Provenance["trusted_visual_cost_budget_release_failure_code"]);
        Assert.Contains(
            "trusted_visual_cost_budget_release_failure_code=" +
            DesignConversionThinClientJobRuntime.TrustedVisualCostBudgetSettlementFailedErrorCode,
            terminal.Warnings);
        Assert.Equal(bool.TrueString, terminal.Provenance["trusted_visual_cost_budget_release_failed"]);
        Assert.Equal(bool.FalseString, terminal.Provenance["trusted_visual_cost_budget_release_used"]);
        Assert.Equal(bool.TrueString, terminal.Provenance["accounting_reconciliation_required"]);
        Assert.Equal(
            JsonDesignConversionPostEvaluationStateStore.AccountingReconciliationRequired,
            terminal.Provenance["accounting_reconciliation_state"]);
        AssertNoDependencyPrivateMarkerLeakage(terminal, privateMessage, privateCode);
        AssertNoPrivateSubmittedPayloadLeakage(terminal, request);
        Assert.Equal(["reserve", "evaluate", "release"], events);
        Assert.Equal(1, store.ReleaseCalls);
        Assert.Equal(0, store.SettlementCalls);
        Assert.False(firstRun.IsCompleted);

        var releaseCallsAfterTerminal = store.ReleaseCalls;
        var replay = await runtime.RunAsync(submitted.Value.JobId);
        Assert.True(replay.IsSuccess, replay.ErrorMessage);
        Assert.Equal(terminal, replay.Value);
        Assert.Equal(releaseCallsAfterTerminal, store.ReleaseCalls);

        blockingEvaluator.Release();
        var firstRunReadback = await firstRun.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.True(firstRunReadback.IsSuccess, firstRunReadback.ErrorMessage);
        Assert.Equal(terminal, firstRunReadback.Value);
        Assert.Equal(releaseCallsAfterTerminal, store.ReleaseCalls);
        Assert.Single(evaluator.Requests);
        Assert.Single(blockingEvaluator.Requests);
        Assert.Empty(ledger.AttemptedRecords);
        var ledgerRead = await ledger.ListRecordsAsync();
        Assert.True(ledgerRead.IsSuccess, ledgerRead.ErrorMessage);
        Assert.Empty(ledgerRead.Value!);
        Assert.Empty(artifactDb.AttemptedRecords);
        var artifactRead = await artifactDb.ListRecordsAsync(
            new DesignConversionLocalArtifactDbQuery(JobId: submitted.Value.JobId));
        Assert.True(artifactRead.IsSuccess, artifactRead.ErrorMessage);
        Assert.Empty(artifactRead.Value!);
        var period = await store.Inner.ReadPeriodAsync(BuildTrustedBudgetPeriodQuery(request));
        Assert.True(period.IsSuccess, period.ErrorMessage);
        Assert.Equal(1, period.Value!.ReservationCount);
        Assert.Equal(1, period.Value.ActiveReservationCount);
        Assert.Equal(0, period.Value.SettledReservationCount);
        Assert.Equal(0, period.Value.ReleasedReservationCount);
        Assert.Equal(0m, period.Value.FinalizedSpendUsd);
        Assert.Equal(0.25m, period.Value.ActiveReservedUsd);
        Assert.Equal(0, runtime.LiveRunLeaseCount);
    }

    [Theory]
    [InlineData("post-evaluation-read")]
    [InlineData("post-evaluation-write")]
    [InlineData("release-and-readback")]
    [InlineData("status-publication")]
    public async Task RunAsync_WhenStaleReleaseDependencyThrowsPublishesSafeTerminalBeforeCompletion(
        string failurePoint)
    {
        var now = DateTimeOffset.UnixEpoch;
        var timeout = TimeSpan.FromMinutes(5);
        var evaluator = new BlockingEvaluator(CreateRuntimeCandidateEligibleInternalEvaluator());
        var innerBudget = new JsonlDesignConversionVisualCostBudgetStore(
            CreateBudgetStoreTempRoot("stale-release-exception-budget-" + failurePoint));
        var store = new EventRecordingVisualCostBudgetStore(
            innerBudget,
            [],
            releaseOverride: failurePoint == "release-and-readback"
                ? (_, _) => Task.FromException<
                    OperationResult<DesignConversionVisualCostReleaseResult>>(
                    new InvalidOperationException("injected-release-failure"))
                : null,
            readOverride: failurePoint == "release-and-readback"
                ? (_, _) => Task.FromException<
                    OperationResult<DesignConversionVisualCostBudgetPeriodState>>(
                    new InvalidOperationException("injected-readback-failure"))
                : null);
        IDesignConversionPostEvaluationStateStore stateStore =
            failurePoint.StartsWith("post-evaluation-", StringComparison.Ordinal)
                ? new ThrowingPostEvaluationStateStore(
                    throwRead: failurePoint == "post-evaluation-read",
                    throwWrite: failurePoint == "post-evaluation-write")
                : new JsonDesignConversionPostEvaluationStateStore(
                    CreateBudgetStoreTempRoot(
                        "stale-release-exception-state-" + failurePoint));
        var statusPublicationCalls = 0;
        Action? beforeStatusPublication = failurePoint == "status-publication"
            ? () =>
            {
                Interlocked.Increment(ref statusPublicationCalls);
                throw new InvalidOperationException(
                    "injected-status-publication-failure");
            }
            : null;
        var runtime = new DesignConversionThinClientJobRuntime(
            evaluator,
            () => now,
            artifactDb: null,
            visualCostLedger: new JsonlDesignConversionVisualCostLedger(
                CreateBudgetStoreTempRoot(
                    "stale-release-exception-ledger-" + failurePoint)),
            visualCostBudgetStore: store,
            staleRunningTimeout: timeout,
            beforePostEvaluationOwnershipTransition: null,
            postEvaluationStateStore: stateStore,
            beforeReservationReleaseStatusPublication: beforeStatusPublication);
        var request = WithTrustedVisualCostBudget(
            CreateRuntimeCandidateEligiblePrivateSubmittedRequest() with
            {
                RequestId = "stale-release-exception-request-" + failurePoint
            },
            monthlyLimitUsd: "1",
            budgetUsd: 0.25m);
        var submitted = runtime.Submit(request);
        Assert.True(submitted.IsSuccess, submitted.ErrorMessage);

        var firstRun = runtime.RunAsync(submitted.Value!.JobId);
        await evaluator.Invoked.Task.WaitAsync(TimeSpan.FromSeconds(5));
        now = now.Add(timeout).AddSeconds(1);
        var staleReadback = runtime.GetStatus(submitted.Value.JobId);
        Assert.True(staleReadback.IsSuccess, staleReadback.ErrorMessage);
        evaluator.Release();
        var completed = await firstRun.WaitAsync(TimeSpan.FromSeconds(5));
        var finalReadback = runtime.GetStatus(submitted.Value.JobId);

        Assert.True(completed.IsSuccess, completed.ErrorMessage);
        Assert.True(finalReadback.IsSuccess, finalReadback.ErrorMessage);
        AssertStaleRunningFailure(completed.Value!, timeout);
        Assert.Equal(completed.Value, finalReadback.Value);
        Assert.Equal(0, runtime.LiveRunLeaseCount);
        Assert.Equal(
            bool.TrueString,
            completed.Value!.Provenance["accounting_reconciliation_required"]);
        Assert.Equal(
            JsonDesignConversionPostEvaluationStateStore.AccountingReconciliationRequired,
            completed.Value.Provenance["accounting_reconciliation_state"]);
        Assert.Equal(
            JsonDesignConversionPostEvaluationStateStore.BuildStateRef(
                completed.Value.Provenance["trusted_visual_cost_budget_reservation_ref"]),
            completed.Value.Provenance[
                JsonDesignConversionPostEvaluationStateStore.StateRefMetadataKey]);

        if (failurePoint is "post-evaluation-write" or "status-publication")
        {
            Assert.Equal(
                failurePoint == "status-publication" ? 1 : 0,
                statusPublicationCalls);
            Assert.Equal(bool.TrueString, completed.Value.Provenance[
                "trusted_visual_cost_budget_release_used"]);
            Assert.Equal(bool.FalseString, completed.Value.Provenance[
                "post_evaluation_finalization_completed"]);
            Assert.Equal(
                DesignConversionThinClientJobRuntime.AccountingReconciliationRequiredErrorCode,
                completed.Value.Provenance["post_evaluation_finalization_error_code"]);
        }
        else
        {
            Assert.Equal(bool.TrueString, completed.Value.Provenance[
                "trusted_visual_cost_budget_release_failed"]);
            Assert.Equal(bool.FalseString, completed.Value.Provenance[
                "trusted_visual_cost_budget_release_used"]);
        }

        var releaseCallsAfterCompletion = store.ReleaseCalls;
        var replay = await runtime.RunAsync(submitted.Value.JobId);
        Assert.True(replay.IsSuccess, replay.ErrorMessage);
        Assert.Equal(completed.Value, replay.Value);
        Assert.Equal(releaseCallsAfterCompletion, store.ReleaseCalls);
        Assert.Equal(0, runtime.LiveRunLeaseCount);
    }

    [Fact]
    public async Task RunAsync_WhenStaleCleanupWinsAfterEvaluatorReturnRevokesLeaseBeforeAllSideEffects()
    {
        var now = DateTimeOffset.UnixEpoch;
        var timeout = TimeSpan.FromMinutes(5);
        var events = new List<string>();
        var barrier = new PostEvaluationOwnershipBarrier();
        var evaluator = new EventRecordingEvaluator(
            CreateRuntimeCandidateEligibleInternalEvaluator(),
            events);
        var ledger = new RecordingVisualCostLedger(
            new JsonlDesignConversionVisualCostLedger(CreateBudgetStoreTempRoot("stale-transition-ledger")));
        var artifactDb = new RecordingLocalArtifactDb(
            new JsonlDesignConversionLocalArtifactDb(CreateBudgetStoreTempRoot("stale-transition-artifact-db")));
        var store = new EventRecordingVisualCostBudgetStore(
            new JsonlDesignConversionVisualCostBudgetStore(CreateBudgetStoreTempRoot("stale-transition-budget")),
            events);
        var runtime = new DesignConversionThinClientJobRuntime(
            evaluator,
            () => now,
            artifactDb,
            ledger,
            store,
            timeout,
            barrier.WaitAsync);
        var request = WithTrustedVisualCostBudget(
            CreateRuntimeCandidateEligiblePrivateSubmittedRequest() with
            {
                RequestId = "thin-stale-post-evaluator-transition-request"
            },
            monthlyLimitUsd: "1",
            budgetUsd: 0.25m);
        var submitted = runtime.Submit(request);
        Assert.True(submitted.IsSuccess, submitted.ErrorMessage);

        var firstRun = runtime.RunAsync(submitted.Value!.JobId);
        await barrier.Entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal(["reserve", "evaluate"], events);

        now = now.Add(timeout).AddSeconds(1);
        var staleReadback = runtime.GetStatus(submitted.Value.JobId);
        Assert.True(staleReadback.IsSuccess, staleReadback.ErrorMessage);
        await store.ReleaseObserved.Task.WaitAsync(TimeSpan.FromSeconds(5));

        var replay = await runtime.RunAsync(submitted.Value.JobId);
        var secondReadback = runtime.GetStatus(submitted.Value.JobId);
        barrier.Release();
        var firstRunReadback = await firstRun.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.True(replay.IsSuccess, replay.ErrorMessage);
        Assert.True(secondReadback.IsSuccess, secondReadback.ErrorMessage);
        Assert.True(firstRunReadback.IsSuccess, firstRunReadback.ErrorMessage);
        AssertStaleRunningFailure(replay.Value!, timeout);
        AssertStaleRunningFailure(secondReadback.Value!, timeout);
        AssertStaleRunningFailure(firstRunReadback.Value!, timeout);
        Assert.Equal(["reserve", "evaluate", "release"], events);
        Assert.Equal(1, store.ReleaseCalls);
        Assert.Equal(0, store.SettlementCalls);
        Assert.Empty(ledger.AttemptedRecords);
        Assert.Empty(artifactDb.AttemptedRecords);

        var period = await store.Inner.ReadPeriodAsync(BuildTrustedBudgetPeriodQuery(request));
        Assert.True(period.IsSuccess, period.ErrorMessage);
        Assert.Equal(0, period.Value!.ActiveReservationCount);
        Assert.Equal(0, period.Value.SettledReservationCount);
        Assert.Equal(1, period.Value.ReleasedReservationCount);
        Assert.Equal(0m, period.Value.FinalizedSpendUsd);
        Assert.Equal(0, runtime.LiveRunLeaseCount);
    }

    [Fact]
    public async Task RunAsync_WhenSettlementOutcomeIsUnknownDoesNotReleaseAndDurablyReconcilesAfterRestart()
    {
        var deadline = new ManualFinalizationDeadline();
        var settlementEntered = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var neverSettlement =
            new TaskCompletionSource<OperationResult<DesignConversionVisualCostSettlementResult>>(
                TaskCreationOptions.RunContinuationsAsynchronously);
        var events = new List<string>();
        var store = new EventRecordingVisualCostBudgetStore(
            new JsonlDesignConversionVisualCostBudgetStore(
                CreateBudgetStoreTempRoot("finalization-timeout-settlement-budget")),
            events,
            (_, _) =>
            {
                settlementEntered.TrySetResult(true);
                return neverSettlement.Task;
            });
        var ledger = new RecordingVisualCostLedger(
            new JsonlDesignConversionVisualCostLedger(
                CreateBudgetStoreTempRoot("finalization-timeout-settlement-ledger")));
        var artifactDb = new RecordingLocalArtifactDb(
            new JsonlDesignConversionLocalArtifactDb(
                CreateBudgetStoreTempRoot("finalization-timeout-settlement-artifact")));
        var stateStore = new JsonDesignConversionPostEvaluationStateStore(
            CreateBudgetStoreTempRoot("finalization-timeout-settlement-state"));
        var runtime = new DesignConversionThinClientJobRuntime(
            CreateRuntimeCandidateEligibleInternalEvaluator(),
            FixedClock(),
            artifactDb,
            ledger,
            store,
            staleRunningTimeout: null,
            beforePostEvaluationOwnershipTransition: null,
            postEvaluationFinalizationTimeout: TimeSpan.FromSeconds(30),
            postEvaluationFinalizationDelay: deadline.WaitAsync,
            postEvaluationStateStore: stateStore);
        var request = WithTrustedVisualCostBudget(
            CreateRuntimeCandidateEligiblePrivateSubmittedRequest() with
            {
                RequestId = "thin-finalization-timeout-settlement-request"
            },
            monthlyLimitUsd: "1",
            budgetUsd: 0.25m);
        var submitted = runtime.Submit(request);
        Assert.True(submitted.IsSuccess, submitted.ErrorMessage);

        var run = runtime.RunAsync(submitted.Value!.JobId);
        await settlementEntered.Task.WaitAsync(TimeSpan.FromSeconds(5));
        deadline.Expire();
        var result = await run.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionThinClientStatus.Failed, result.Value!.Job.Status);
        Assert.Equal(
            DesignConversionThinClientJobRuntime.PostEvaluationFinalizationTimeoutErrorCode,
            result.Value.Job.Metadata["error_code"]);
        Assert.Equal(TimeSpan.FromSeconds(30), deadline.RequestedTimeout);
        Assert.Equal(1, store.SettlementCalls);
        Assert.Equal(0, store.ReleaseCalls);
        Assert.Equal(
            bool.TrueString,
            result.Value.Provenance["accounting_reconciliation_required"]);
        Assert.Empty(ledger.AttemptedRecords);
        Assert.Empty(artifactDb.AttemptedRecords);
        Assert.Equal(0, artifactDb.ListCalls);
        Assert.Equal(0, runtime.LiveRunLeaseCount);

        var replay = await runtime.RunAsync(submitted.Value.JobId);
        var readback = runtime.GetStatus(submitted.Value.JobId);
        Assert.True(replay.IsSuccess, replay.ErrorMessage);
        Assert.True(readback.IsSuccess, readback.ErrorMessage);
        Assert.Equal(DesignConversionThinClientStatus.Failed, replay.Value!.Job.Status);
        Assert.Equal(DesignConversionThinClientStatus.Failed, readback.Value!.Job.Status);
        Assert.Equal(1, store.SettlementCalls);
        Assert.Equal(0, store.ReleaseCalls);
        Assert.Empty(ledger.AttemptedRecords);
        Assert.Empty(artifactDb.AttemptedRecords);

        var pending = await stateStore.ListAsync();
        Assert.True(pending.IsSuccess, pending.ErrorMessage);
        var pendingState = Assert.Single(pending.Value!);
        Assert.Equal(
            JsonDesignConversionPostEvaluationStateStore.AccountingReconciliationRequired,
            pendingState.AccountingStatus);
        Assert.Equal(
            JsonDesignConversionPostEvaluationStateStore.DesiredSettle,
            pendingState.AccountingDesiredAction);

        var restartedRuntime = new DesignConversionThinClientJobRuntime(
            CreateRuntimeCandidateEligibleInternalEvaluator(),
            FixedClock(),
            artifactDb,
            ledger,
            store.Inner,
            postEvaluationStateStore: new JsonDesignConversionPostEvaluationStateStore(
                stateStore.RootDirectory));
        var reconciled = await restartedRuntime.ReconcilePendingAccountingAsync();
        Assert.True(reconciled.IsSuccess, reconciled.ErrorMessage);
        Assert.Equal(1, reconciled.Value);
        var period = await store.Inner.ReadPeriodAsync(BuildTrustedBudgetPeriodQuery(request));
        Assert.True(period.IsSuccess, period.ErrorMessage);
        Assert.Equal(1, period.Value!.SettledReservationCount);
        Assert.Equal(0, period.Value.ReleasedReservationCount);
        Assert.Equal(0, period.Value.ActiveReservationCount);
        var afterRestart = await stateStore.ListAsync();
        Assert.Equal(
            JsonDesignConversionPostEvaluationStateStore.AccountingSettled,
            Assert.Single(afterRestart.Value!).AccountingStatus);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task RunAsync_WhenActualSettlementCommitsThenThrowsReconcilesSettledWithoutRelease(
        bool cancellation)
    {
        var inner = new JsonlDesignConversionVisualCostBudgetStore(
            CreateBudgetStoreTempRoot("settlement-commits-then-throws-" + cancellation));
        var store = new EventRecordingVisualCostBudgetStore(
            inner,
            [],
            async (request, _) =>
            {
                var committed = await inner.SettleAsync(request, CancellationToken.None);
                Assert.True(committed.IsSuccess, committed.ErrorMessage);
                if (cancellation)
                    throw new OperationCanceledException();
                throw new InvalidOperationException("after-durable-settlement");
            });
        var stateStore = new JsonDesignConversionPostEvaluationStateStore(
            CreateBudgetStoreTempRoot("settlement-commits-then-throws-state-" + cancellation));
        var runtime = new DesignConversionThinClientJobRuntime(
            CreateRuntimeCandidateEligibleInternalEvaluator(),
            FixedClock(),
            new JsonlDesignConversionLocalArtifactDb(
                CreateBudgetStoreTempRoot("settlement-commits-then-throws-artifact-" + cancellation)),
            new JsonlDesignConversionVisualCostLedger(
                CreateBudgetStoreTempRoot("settlement-commits-then-throws-ledger-" + cancellation)),
            store,
            postEvaluationStateStore: stateStore);
        var request = WithTrustedVisualCostBudget(
            CreateRuntimeCandidateEligiblePrivateSubmittedRequest() with
            {
                RequestId = "settlement-commits-then-throws-request-" + cancellation
            },
            monthlyLimitUsd: "1",
            budgetUsd: 0.25m);
        var submitted = runtime.Submit(request);
        Assert.True(submitted.IsSuccess, submitted.ErrorMessage);

        var result = await runtime.RunAsync(submitted.Value!.JobId);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionThinClientStatus.Failed, result.Value!.Job.Status);
        Assert.Equal(1, store.SettlementCalls);
        Assert.Equal(0, store.ReleaseCalls);
        Assert.Equal(0, runtime.LiveRunLeaseCount);
        var period = await inner.ReadPeriodAsync(BuildTrustedBudgetPeriodQuery(request));
        Assert.True(period.IsSuccess, period.ErrorMessage);
        Assert.Equal(1, period.Value!.SettledReservationCount);
        Assert.Equal(0, period.Value.ReleasedReservationCount);
        Assert.Equal(0, period.Value.ActiveReservationCount);
        var state = await stateStore.ListAsync();
        Assert.True(state.IsSuccess, state.ErrorMessage);
        Assert.Equal(
            JsonDesignConversionPostEvaluationStateStore.AccountingSettled,
            Assert.Single(state.Value!).AccountingStatus);
    }

    [Fact]
    public async Task RunAsync_WhenReleaseNeverReturnsIsBoundedAndRestartReconciliationReleasesOnce()
    {
        var deadline = new ManualFinalizationDeadline();
        var releaseEntered = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var neverRelease =
            new TaskCompletionSource<OperationResult<DesignConversionVisualCostReleaseResult>>(
                TaskCreationOptions.RunContinuationsAsynchronously);
        DesignConversionVisualCostReleaseRequest? capturedReleaseRequest = null;
        var inner = new JsonlDesignConversionVisualCostBudgetStore(
            CreateBudgetStoreTempRoot("release-never-returns-budget"));
        var store = new EventRecordingVisualCostBudgetStore(
            inner,
            [],
            settleOverride: (_, _) => Task.FromResult(
                OperationResult<DesignConversionVisualCostSettlementResult>.Fail(
                    "definitively-not-applied",
                    JsonlDesignConversionVisualCostBudgetStore.WriteFailedErrorCode)),
            releaseOverride: (releaseRequest, _) =>
            {
                capturedReleaseRequest = releaseRequest;
                releaseEntered.TrySetResult(true);
                return neverRelease.Task;
            });
        var stateStore = new JsonDesignConversionPostEvaluationStateStore(
            CreateBudgetStoreTempRoot("release-never-returns-state"));
        var runtime = new DesignConversionThinClientJobRuntime(
            CreateRuntimeCandidateEligibleInternalEvaluator(),
            FixedClock(),
            new JsonlDesignConversionLocalArtifactDb(
                CreateBudgetStoreTempRoot("release-never-returns-artifact")),
            new JsonlDesignConversionVisualCostLedger(
                CreateBudgetStoreTempRoot("release-never-returns-ledger")),
            store,
            staleRunningTimeout: null,
            beforePostEvaluationOwnershipTransition: null,
            postEvaluationFinalizationTimeout: TimeSpan.FromSeconds(30),
            postEvaluationFinalizationDelay: deadline.WaitAsync,
            postEvaluationStateStore: stateStore);
        var request = WithTrustedVisualCostBudget(
            CreateRuntimeCandidateEligiblePrivateSubmittedRequest() with
            {
                RequestId = "release-never-returns-request"
            },
            monthlyLimitUsd: "1",
            budgetUsd: 0.25m);
        var submitted = runtime.Submit(request);
        Assert.True(submitted.IsSuccess, submitted.ErrorMessage);

        var run = runtime.RunAsync(submitted.Value!.JobId);
        await releaseEntered.Task.WaitAsync(TimeSpan.FromSeconds(5));
        deadline.Expire();
        var result = await run.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionThinClientStatus.Failed, result.Value!.Job.Status);
        Assert.Equal(1, store.SettlementCalls);
        Assert.Equal(1, store.ReleaseCalls);
        Assert.Equal(0, runtime.LiveRunLeaseCount);
        var active = await inner.ReadPeriodAsync(BuildTrustedBudgetPeriodQuery(request));
        Assert.True(active.IsSuccess, active.ErrorMessage);
        Assert.Equal(1, active.Value!.ActiveReservationCount);
        Assert.Equal(0, active.Value.ReleasedReservationCount);

        var pending = await stateStore.ListAsync();
        Assert.True(pending.IsSuccess, pending.ErrorMessage);
        Assert.Equal(
            JsonDesignConversionPostEvaluationStateStore.DesiredRelease,
            Assert.Single(pending.Value!).AccountingDesiredAction);
        Assert.Equal(
            JsonDesignConversionPostEvaluationStateStore.AccountingReconciliationRequired,
            Assert.Single(pending.Value!).AccountingStatus);
        Assert.True(Assert.Single(pending.Value!).SettlementDefinitivelyNotApplied);

        var lateRelease = await inner.ReleaseAsync(
            Assert.IsType<DesignConversionVisualCostReleaseRequest>(capturedReleaseRequest),
            CancellationToken.None);
        Assert.True(lateRelease.IsSuccess, lateRelease.ErrorMessage);
        neverRelease.TrySetResult(lateRelease);

        var restartedRuntime = new DesignConversionThinClientJobRuntime(
            CreateRuntimeCandidateEligibleInternalEvaluator(),
            FixedClock(),
            visualCostBudgetStore: inner,
            postEvaluationStateStore: new JsonDesignConversionPostEvaluationStateStore(
                stateStore.RootDirectory));
        var reconciled = await restartedRuntime.ReconcilePendingAccountingAsync();
        Assert.True(reconciled.IsSuccess, reconciled.ErrorMessage);
        Assert.Equal(1, reconciled.Value);
        var final = await inner.ReadPeriodAsync(BuildTrustedBudgetPeriodQuery(request));
        Assert.True(final.IsSuccess, final.ErrorMessage);
        Assert.Equal(0, final.Value!.ActiveReservationCount);
        Assert.Equal(1, final.Value.ReleasedReservationCount);
        Assert.Equal(0, final.Value.SettledReservationCount);
    }

    [Fact]
    public async Task RunAsync_WhenArtifactAppendNeverReturnsDeadlineDoesNotReleaseSettledReservationOrReadBackLate()
    {
        var deadline = new ManualFinalizationDeadline();
        var appendEntered = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseLateAppend =
            new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously);
        var lateAppendReturned = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var events = new List<string>();
        var store = new EventRecordingVisualCostBudgetStore(
            new JsonlDesignConversionVisualCostBudgetStore(
                CreateBudgetStoreTempRoot("finalization-timeout-artifact-budget")),
            events);
        var ledger = new RecordingVisualCostLedger(
            new JsonlDesignConversionVisualCostLedger(
                CreateBudgetStoreTempRoot("finalization-timeout-artifact-ledger")));
        var artifactRoot = CreateBudgetStoreTempRoot("finalization-timeout-artifact-db");
        var innerArtifactDb = new JsonlDesignConversionLocalArtifactDb(artifactRoot);
        var artifactDb = new RecordingLocalArtifactDb(
            innerArtifactDb,
            appendOverride: async (record, _) =>
            {
                appendEntered.TrySetResult(true);
                await releaseLateAppend.Task;
                var lateResult = await innerArtifactDb.AppendRecordAsync(
                    record,
                    CancellationToken.None);
                lateAppendReturned.TrySetResult(true);
                return lateResult;
            });
        using var teacherAssociation =
            DesignConversionDpoTeacherTestAssociation.Associate(
                artifactDb,
                supportTeacherAuthoring: true,
                supportIndependentPreferenceEvidence: true);
        var stateStore = new JsonDesignConversionPostEvaluationStateStore(
            Path.Combine(
                artifactRoot,
                JsonDesignConversionPostEvaluationStateStore.DirectoryName));
        var teacherAuthoredFixture =
            CreateTeacherAuthoredRuntimeCandidateFixture(artifactDb);
        var rejectedEvaluationSource = teacherAuthoredFixture.RejectedEvaluationSource;
        var runtime = new DesignConversionThinClientJobRuntime(
            teacherAuthoredFixture.Evaluator,
            FixedClock(),
            artifactDb,
            ledger,
            store,
            staleRunningTimeout: null,
            beforePostEvaluationOwnershipTransition: null,
            postEvaluationFinalizationTimeout: TimeSpan.FromSeconds(30),
            postEvaluationFinalizationDelay: deadline.WaitAsync,
            postEvaluationStateStore: stateStore,
            rejectedEvaluationSource: rejectedEvaluationSource);
        var request = WithTrustedVisualCostBudget(
            CreateTeacherAuthoredRuntimeCandidateEligibleSubmittedRequest() with
            {
                RequestId = "thin-finalization-timeout-artifact-request"
            },
            monthlyLimitUsd: "1",
            budgetUsd: 0.25m);
        var submitted = runtime.Submit(request);
        Assert.True(submitted.IsSuccess, submitted.ErrorMessage);

        var run = runtime.RunAsync(submitted.Value!.JobId);
        await appendEntered.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal(1, store.SettlementCalls);
        Assert.Equal(0, store.ReleaseCalls);
        deadline.Expire();
        var result = await run.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionThinClientStatus.Failed, result.Value!.Job.Status);
        Assert.Equal(
            DesignConversionThinClientJobRuntime.PostEvaluationFinalizationTimeoutErrorCode,
            result.Value.Job.Metadata["error_code"]);
        Assert.Equal(
            bool.TrueString,
            result.Value.Provenance["post_evaluation_finalization_dependency_may_complete_late"]);
        Assert.Single(ledger.AttemptedRecords);
        Assert.Single(artifactDb.AttemptedRecords);
        Assert.Equal(1, rejectedEvaluationSource.CallCount);
        Assert.Equal(0, artifactDb.ListCalls);
        Assert.Equal(1, store.SettlementCalls);
        Assert.Equal(0, store.ReleaseCalls);
        Assert.Equal(0, runtime.LiveRunLeaseCount);

        var replay = await runtime.RunAsync(submitted.Value.JobId);
        Assert.True(replay.IsSuccess, replay.ErrorMessage);
        Assert.Equal(DesignConversionThinClientStatus.Failed, replay.Value!.Job.Status);
        Assert.Single(ledger.AttemptedRecords);
        Assert.Single(artifactDb.AttemptedRecords);
        Assert.Equal(0, artifactDb.ListCalls);
        Assert.Equal(1, store.SettlementCalls);
        Assert.Equal(0, store.ReleaseCalls);
        var period = await store.Inner.ReadPeriodAsync(BuildTrustedBudgetPeriodQuery(request));
        Assert.True(period.IsSuccess, period.ErrorMessage);
        Assert.Equal(1, period.Value!.SettledReservationCount);
        Assert.Equal(0, period.Value.ActiveReservationCount);
        Assert.Equal(0, period.Value.ReleasedReservationCount);

        releaseLateAppend.TrySetResult(true);
        await lateAppendReturned.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await Task.Yield();
        var afterLateCompletion = runtime.GetStatus(submitted.Value.JobId);
        Assert.True(afterLateCompletion.IsSuccess, afterLateCompletion.ErrorMessage);
        Assert.Equal(
            DesignConversionThinClientJobRuntime.PostEvaluationFinalizationTimeoutErrorCode,
            afterLateCompletion.Value!.Job.Metadata["error_code"]);
        Assert.Equal(0, artifactDb.ListCalls);
        Assert.Equal(0, runtime.LiveRunLeaseCount);
        var rawRecords = await innerArtifactDb.ListRecordsAsync();
        Assert.True(rawRecords.IsSuccess, rawRecords.ErrorMessage);
        var rawRecord = Assert.Single(rawRecords.Value!);
        Assert.Equal(
            JsonDesignConversionPostEvaluationStateStore.PersistenceProvisional,
            rawRecord.Metadata[
                JsonDesignConversionPostEvaluationStateStore.PersistenceStatusMetadataKey]);
        var durableState = await stateStore.ReadAsync(
            rawRecord.Metadata[JsonDesignConversionPostEvaluationStateStore.StateRefMetadataKey]);
        Assert.True(durableState.IsSuccess, durableState.ErrorMessage);
        Assert.NotNull(durableState.Value);
        Assert.NotEqual(
            JsonDesignConversionPostEvaluationStateStore.PersistenceCommitted,
            durableState.Value!.PersistenceStatus);
        Assert.Equal(2, rawRecord.DpoCandidates.Count);
        var candidate = rawRecord.DpoCandidates[0];
        var approval = await innerArtifactDb.ApproveRuntimeCreatedDpoCandidateAsync(
            rawRecord.RecordId,
            candidate.CandidateId,
            "reviewer-ref",
            "late append must stay quarantined");
        Assert.False(approval.IsSuccess);
        Assert.Equal(
            DesignConversionPostEvaluationTrainingEligibilityPolicy
                .StateNotCommittedErrorCode,
            approval.ErrorCode);
        var approved = await innerArtifactDb.QueryApprovedTrainingCandidatesAsync();
        Assert.True(approved.IsSuccess, approved.ErrorMessage);
        Assert.Empty(approved.Value!);
    }

    [Fact]
    public async Task RunAsync_WhenActualLedgerAppendCompletesAfterDeadlineRemainsProvisional()
    {
        var deadline = new ManualFinalizationDeadline();
        var appendEntered = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseLateAppend = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var lateAppendReturned = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var ledgerRoot = CreateBudgetStoreTempRoot("late-ledger-actual-root");
        var innerLedger = new JsonlDesignConversionVisualCostLedger(ledgerRoot);
        var ledger = new RecordingVisualCostLedger(
            innerLedger,
            async (record, _) =>
            {
                appendEntered.TrySetResult(true);
                await releaseLateAppend.Task;
                var result = await innerLedger.AppendRecordAsync(record, CancellationToken.None);
                lateAppendReturned.TrySetResult(true);
                return result;
            });
        var artifactRoot = CreateBudgetStoreTempRoot("late-ledger-artifact-root");
        var stateStore = new JsonDesignConversionPostEvaluationStateStore(
            Path.Combine(
                artifactRoot,
                JsonDesignConversionPostEvaluationStateStore.DirectoryName));
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(artifactRoot);
        using var teacherAssociation =
            DesignConversionDpoTeacherTestAssociation.Associate(artifactDb);
        var rejectedEvaluationSource = CreateValidRuntimeRejectedEvaluationSource();
        var controlledRuntime = new DesignConversionThinClientJobRuntime(
            CreateMeasuredRuntimeCandidateEligibleEvaluator(),
            FixedClock(),
            artifactDb,
            ledger,
            visualCostBudgetStore: null,
            staleRunningTimeout: null,
            beforePostEvaluationOwnershipTransition: null,
            postEvaluationFinalizationTimeout: TimeSpan.FromSeconds(30),
            postEvaluationFinalizationDelay: deadline.WaitAsync,
            postEvaluationStateStore: stateStore,
            rejectedEvaluationSource: rejectedEvaluationSource);
        var submitted = controlledRuntime.Submit(
            CreateRuntimeCandidateEligiblePrivateSubmittedRequest() with
            {
                RequestId = "late-ledger-actual-controlled-request"
            });
        Assert.True(submitted.IsSuccess, submitted.ErrorMessage);
        var run = controlledRuntime.RunAsync(submitted.Value!.JobId);
        await appendEntered.Task.WaitAsync(TimeSpan.FromSeconds(5));
        deadline.Expire();
        var terminal = await run.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.True(terminal.IsSuccess, terminal.ErrorMessage);
        Assert.Equal(DesignConversionThinClientStatus.Failed, terminal.Value!.Job.Status);
        Assert.Equal(1, rejectedEvaluationSource.CallCount);
        Assert.Equal(0, controlledRuntime.LiveRunLeaseCount);

        releaseLateAppend.TrySetResult(true);
        await lateAppendReturned.Task.WaitAsync(TimeSpan.FromSeconds(5));
        var records = await innerLedger.ListRecordsAsync();
        Assert.True(records.IsSuccess, records.ErrorMessage);
        var record = Assert.Single(records.Value!);
        Assert.Equal(
            JsonDesignConversionPostEvaluationStateStore.PersistenceProvisional,
            record.Metadata[
                JsonDesignConversionPostEvaluationStateStore.PersistenceStatusMetadataKey]);
        var durable = await stateStore.ReadAsync(
            record.Metadata[JsonDesignConversionPostEvaluationStateStore.StateRefMetadataKey]);
        Assert.True(durable.IsSuccess, durable.ErrorMessage);
        Assert.NotEqual(
            JsonDesignConversionPostEvaluationStateStore.PersistenceCommitted,
            durable.Value!.PersistenceStatus);
    }

    [Theory]
    [InlineData("settlement", true)]
    [InlineData("ledger", true)]
    [InlineData("artifact", true)]
    [InlineData("readback", true)]
    [InlineData("settlement", false)]
    [InlineData("ledger", false)]
    [InlineData("artifact", false)]
    [InlineData("readback", false)]
    public async Task RunAsync_WhenFinalizationDependencyCancelsOrThrowsFailsSafeAndFinalizesReservationCorrectly(
        string failureStage,
        bool cancellation)
    {
        Exception Failure(CancellationToken token)
            => cancellation
                ? new OperationCanceledException(token)
                : new InvalidOperationException("injected-finalization-dependency-failure");

        var events = new List<string>();
        Func<
            DesignConversionVisualCostSettlementRequest,
            CancellationToken,
            Task<OperationResult<DesignConversionVisualCostSettlementResult>>>? settlementOverride =
            string.Equals(failureStage, "settlement", StringComparison.Ordinal)
                ? (_, token) => Task.FromException<OperationResult<DesignConversionVisualCostSettlementResult>>(
                    Failure(token))
                : null;
        Func<
            DesignConversionVisualCostLedgerRecord,
            CancellationToken,
            Task<OperationResult<DesignConversionVisualCostLedgerRecord>>>? ledgerOverride =
            string.Equals(failureStage, "ledger", StringComparison.Ordinal)
                ? (_, token) => Task.FromException<OperationResult<DesignConversionVisualCostLedgerRecord>>(
                    Failure(token))
                : null;
        Func<
            DesignConversionLocalArtifactDbRecord,
            CancellationToken,
            Task<OperationResult<DesignConversionLocalArtifactDbRecord>>>? artifactOverride =
            string.Equals(failureStage, "artifact", StringComparison.Ordinal)
                ? (_, token) => Task.FromException<OperationResult<DesignConversionLocalArtifactDbRecord>>(
                    Failure(token))
                : null;
        Func<
            DesignConversionLocalArtifactDbQuery?,
            CancellationToken,
            Task<OperationResult<IReadOnlyList<DesignConversionLocalArtifactDbRecord>>>>? readbackOverride =
            string.Equals(failureStage, "readback", StringComparison.Ordinal)
                ? (_, token) => Task.FromException<
                    OperationResult<IReadOnlyList<DesignConversionLocalArtifactDbRecord>>>(
                    Failure(token))
                : null;
        var store = new EventRecordingVisualCostBudgetStore(
            new JsonlDesignConversionVisualCostBudgetStore(
                CreateBudgetStoreTempRoot("finalization-failure-budget-" + failureStage + "-" + cancellation)),
            events,
            settlementOverride);
        var ledger = new RecordingVisualCostLedger(
            new JsonlDesignConversionVisualCostLedger(
                CreateBudgetStoreTempRoot("finalization-failure-ledger-" + failureStage + "-" + cancellation)),
            ledgerOverride);
        var persistenceArtifactRoot = CreateBudgetStoreTempRoot(
            "fin-art-" + failureStage[0] + (cancellation ? "-t" : "-f"));
        var persistenceArtifactDb = new JsonlDesignConversionLocalArtifactDb(
            persistenceArtifactRoot);
        var artifactDb = new RecordingLocalArtifactDb(
            persistenceArtifactDb,
            artifactOverride,
            readbackOverride);
        using var teacherAssociation =
            DesignConversionDpoTeacherTestAssociation.Associate(
                artifactDb,
                supportTeacherAuthoring: true,
                supportIndependentPreferenceEvidence: true);
        var teacherAuthoredFixture =
            CreateTeacherAuthoredRuntimeCandidateFixture(artifactDb);
        var rejectedEvaluationSource = teacherAuthoredFixture.RejectedEvaluationSource;
        var runtime = new DesignConversionThinClientJobRuntime(
            teacherAuthoredFixture.Evaluator,
            FixedClock(),
            artifactDb,
            ledger,
            store,
            rejectedEvaluationSource: rejectedEvaluationSource);
        var request = WithTrustedVisualCostBudget(
            CreateTeacherAuthoredRuntimeCandidateEligibleSubmittedRequest() with
            {
                RequestId = "thin-finalization-failure-" + failureStage + "-" + cancellation
            },
            monthlyLimitUsd: "1",
            budgetUsd: 0.25m);
        var submitted = runtime.Submit(request);
        Assert.True(submitted.IsSuccess, submitted.ErrorMessage);

        var result = await runtime.RunAsync(submitted.Value!.JobId).WaitAsync(TimeSpan.FromSeconds(5));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionThinClientStatus.Failed, result.Value!.Job.Status);
        if (cancellation || string.Equals(failureStage, "settlement", StringComparison.Ordinal))
        {
            Assert.Equal(
                cancellation
                    ? DesignConversionThinClientJobRuntime.PostEvaluationFinalizationCancelledErrorCode
                    : DesignConversionThinClientJobRuntime.PostEvaluationFinalizationFailedErrorCode,
                result.Value.Job.Metadata["error_code"]);
        }
        else
        {
            Assert.False(string.IsNullOrWhiteSpace(result.Value.Job.Metadata["error_code"]));
        }
        Assert.Equal(0, runtime.LiveRunLeaseCount);
        Assert.Equal(1, store.SettlementCalls);
        Assert.Equal(
            string.Equals(failureStage, "settlement", StringComparison.Ordinal) ? 0 : 1,
            rejectedEvaluationSource.CallCount);
        if (string.Equals(failureStage, "settlement", StringComparison.Ordinal))
        {
            Assert.Equal(0, store.ReleaseCalls);
            Assert.Empty(ledger.AttemptedRecords);
            Assert.Empty(artifactDb.AttemptedRecords);
        }
        else
        {
            Assert.Equal(0, store.ReleaseCalls);
            Assert.Single(ledger.AttemptedRecords);
            if (string.Equals(failureStage, "ledger", StringComparison.Ordinal))
            {
                Assert.Empty(artifactDb.AttemptedRecords);
            }
            else
            {
                Assert.Single(artifactDb.AttemptedRecords);
                Assert.Equal(
                    string.Equals(failureStage, "readback", StringComparison.Ordinal) ? 1 : 0,
                    artifactDb.ListCalls);
            }
        }

        var period = await store.Inner.ReadPeriodAsync(BuildTrustedBudgetPeriodQuery(request));
        Assert.True(period.IsSuccess, period.ErrorMessage);
        Assert.Equal(
            string.Equals(failureStage, "settlement", StringComparison.Ordinal) ? 0 : 1,
            period.Value!.SettledReservationCount);
        Assert.Equal(
            0,
            period.Value.ReleasedReservationCount);
        Assert.Equal(
            string.Equals(failureStage, "settlement", StringComparison.Ordinal) ? 1 : 0,
            period.Value.ActiveReservationCount);
    }

    [Fact]
    public async Task RunAsync_WhenRunningMarkerIsNotStaleRejectsAlreadyRunningWithoutSecondEvaluator()
    {
        var now = DateTimeOffset.UnixEpoch;
        var timeout = TimeSpan.FromMinutes(5);
        var evaluator = new BlockingEvaluator(CreateDeterministicInternalEvaluator());
        var runtime = new DesignConversionThinClientJobRuntime(
            evaluator,
            () => now,
            staleRunningTimeout: timeout);
        var submitted = runtime.Submit(CreateSubmittedRequest());
        Assert.True(submitted.IsSuccess, submitted.ErrorMessage);

        var firstRun = runtime.RunAsync(submitted.Value!.JobId);
        await evaluator.Invoked.Task.WaitAsync(TimeSpan.FromSeconds(5));
        now = now.Add(timeout).AddTicks(-1);

        var secondRun = await runtime.RunAsync(submitted.Value.JobId);
        var status = runtime.GetStatus(submitted.Value.JobId);

        Assert.False(secondRun.IsSuccess);
        Assert.Equal("JOB_ALREADY_RUNNING", secondRun.ErrorCode);
        Assert.True(status.IsSuccess, status.ErrorMessage);
        Assert.Equal(DesignConversionThinClientStatus.Running, status.Value!.ContractStatus);
        Assert.Equal(DesignConversionThinClientStatus.Running, status.Value.Job.Status);
        Assert.Single(evaluator.Requests);

        evaluator.Release();
        var firstRunReadback = await firstRun.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.True(firstRunReadback.IsSuccess, firstRunReadback.ErrorMessage);
        Assert.Single(evaluator.Requests);
    }

    [Fact]
    public async Task RunAsync_ReturnsCompletedStatusWithoutRerunningTerminalJob()
    {
        var evaluator = new RecordingEvaluator(CreateRuntimeCandidateEligibleInternalEvaluator());
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(
            Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "thin-client-terminal-replay-artifact-db"));
        var runtime = new DesignConversionThinClientJobRuntime(evaluator, FixedClock(), artifactDb);
        var submitted = runtime.Submit(CreateRuntimeCandidateEligiblePrivateSubmittedRequest());
        Assert.True(submitted.IsSuccess, submitted.ErrorMessage);

        var firstRun = await runtime.RunAsync(submitted.Value!.JobId);
        var secondRun = await runtime.RunAsync(submitted.Value.JobId);

        Assert.True(firstRun.IsSuccess, firstRun.ErrorMessage);
        Assert.True(secondRun.IsSuccess, secondRun.ErrorMessage);
        Assert.Equal(DesignConversionThinClientStatus.Completed, firstRun.Value!.ContractStatus);
        Assert.Equal(DesignConversionThinClientStatus.Completed, secondRun.Value!.Job.Status);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.DecisionAccepted, firstRun.Value.Provenance["final_visual_decision"]);
        Assert.Single(evaluator.Requests);
        Assert.Equal(firstRun.Value!.Artifacts.Select(artifact => artifact.ArtifactId), secondRun.Value.Artifacts.Select(artifact => artifact.ArtifactId));
    }

    [Fact]
    public async Task RuntimeRejectedEvaluationHandoff_IsOneShotCrossJobIsolatedAndCleansTerminalEntries()
    {
        var handoff = new DesignConversionRuntimeRejectedEvaluationHandoff();
        var roots = new[]
        {
            DesignConversionRuntimeRejectedEvaluationHandoff.BindToRootOwnership(
                CreateLoopCostAccountingInternalRequest() with
                {
                    JobId = "thin-handoff-root-a",
                    RequestId = "thin-handoff-request-a"
                }),
            DesignConversionRuntimeRejectedEvaluationHandoff.BindToRootOwnership(
                CreateLoopCostAccountingInternalRequest() with
                {
                    JobId = "thin-handoff-root-b",
                    RequestId = "thin-handoff-request-b"
                })
        };
        var pairs = roots.Select((root, index) => new
        {
            Root = root,
            Chosen = CreateHandoffEvaluation(root, index, chosen: true),
            Rejected = CreateHandoffEvaluation(root, index, chosen: false)
        }).ToArray();

        var recorded = await Task.WhenAll(pairs.Select(pair => Task.Run(() =>
            handoff.TryRecord(pair.Root, pair.Chosen, pair.Rejected))));

        Assert.All(recorded, Assert.True);
        Assert.Equal(2, handoff.PendingCount);
        var crossJob = await handoff.ResolveMeasuredRejectedEvaluationAsync(
            new DesignConversionRuntimeRejectedEvaluationLookup("thin-handoff-unrelated:loop-attempt:2"));
        Assert.True(crossJob.IsSuccess, crossJob.ErrorMessage);
        Assert.Null(crossJob.Value);
        Assert.Equal(2, handoff.PendingCount);

        var resolved = await Task.WhenAll(pairs.Select(pair =>
            handoff.ResolveMeasuredRejectedEvaluationAsync(
                new DesignConversionRuntimeRejectedEvaluationLookup(pair.Chosen.JobId))));

        Assert.Equal(0, handoff.PendingCount);
        for (var index = 0; index < resolved.Length; index++)
        {
            Assert.True(resolved[index].IsSuccess, resolved[index].ErrorMessage);
            var reference = Assert.IsType<DesignConversionRuntimeRejectedEvaluationReference>(
                resolved[index].Value);
            Assert.True(reference.TryGetTrustedHydratedEvaluation(out var rejected));
            Assert.Equal(pairs[index].Rejected.JobId, rejected!.JobId);
        }

        var consumedAgain = await handoff.ResolveMeasuredRejectedEvaluationAsync(
            new DesignConversionRuntimeRejectedEvaluationLookup(pairs[0].Chosen.JobId));
        Assert.True(consumedAgain.IsSuccess, consumedAgain.ErrorMessage);
        Assert.Null(consumedAgain.Value);

        Assert.True(handoff.TryRecord(pairs[0].Root, pairs[0].Chosen, pairs[0].Rejected));
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var cancelled = await handoff.ResolveMeasuredRejectedEvaluationAsync(
            new DesignConversionRuntimeRejectedEvaluationLookup(pairs[0].Chosen.JobId),
            cancellation.Token);
        Assert.False(cancelled.IsSuccess);
        Assert.Equal("CANCELLED", cancelled.ErrorCode);
        Assert.Equal(0, handoff.PendingCount);

        Assert.True(handoff.TryRecord(pairs[1].Root, pairs[1].Chosen, pairs[1].Rejected));
        handoff.Discard(pairs[1].Root.JobId);
        Assert.Equal(0, handoff.PendingCount);
    }

    [Fact]
    public async Task RuntimeRejectedEvaluationHandoff_CandidateAuthoringEvidenceAndExactIdentitiesAreSingleConsumption()
    {
        var handoff = new DesignConversionRuntimeRejectedEvaluationHandoff();
        var root = DesignConversionRuntimeRejectedEvaluationHandoff.BindToRootOwnership(
            CreateLoopCostAccountingInternalRequest() with
            {
                JobId = "thin-handoff-teacher-root",
                RequestId = "thin-handoff-teacher-request"
            });
        var chosen = CreateHandoffEvaluation(root, 1, chosen: true);
        var rejected = CreateHandoffEvaluation(root, 1, chosen: false);
        var authoringEvidence = new DesignConversionDpoTeacherCandidateAuthoringEvidence(
            DesignConversionDpoTeacherAttestationRuntime.AuthoringSchema,
            "incumbent-ref",
            "generated-ref",
            "incumbent-binding",
            "generated-binding",
            "teacher-authored:candidate",
            "teacher-authored-request:candidate",
            new string('a', 64),
            TrainingTeacherProvenance.Deterministic("handoff-source-only"),
            ["teacher-handoff:evidence"],
            new Dictionary<string, string>());

        Assert.True(handoff.TryRecordTeacherAuthoredCandidate(
            root,
            chosen,
            rejected,
            authoringEvidence));
        Assert.Equal(1, handoff.PendingCount);

        var first = await handoff.ResolveMeasuredRejectedEvaluationAsync(
            new DesignConversionRuntimeRejectedEvaluationLookup(chosen.JobId));

        Assert.True(first.IsSuccess, first.ErrorMessage);
        var reference = Assert.IsType<DesignConversionRuntimeRejectedEvaluationReference>(
            first.Value);
        Assert.True(reference.TryGetTrustedChosenEvaluation(out var resolvedChosen));
        Assert.True(reference.TryGetTrustedHydratedEvaluation(out var resolvedRejected));
        Assert.True(reference.TryGetTrustedTeacherCandidateAuthoringEvidence(
            out var resolvedAuthoringEvidence));
        Assert.Same(chosen, resolvedChosen);
        Assert.Same(rejected, resolvedRejected);
        Assert.Same(authoringEvidence, resolvedAuthoringEvidence);
        Assert.Equal(0, handoff.PendingCount);

        var second = await handoff.ResolveMeasuredRejectedEvaluationAsync(
            new DesignConversionRuntimeRejectedEvaluationLookup(chosen.JobId));
        Assert.True(second.IsSuccess, second.ErrorMessage);
        Assert.Null(second.Value);
        Assert.Equal(0, handoff.PendingCount);
    }

    [Fact]
    public async Task RuntimeRejectedEvaluationHandoff_PluralCandidateEvaluationPairsAreImmutableOrderedAndSingleConsumption()
    {
        var handoff = new DesignConversionRuntimeRejectedEvaluationHandoff();
        var root = DesignConversionRuntimeRejectedEvaluationHandoff.BindToRootOwnership(
            CreateLoopCostAccountingInternalRequest() with
            {
                JobId = "thin-handoff-teacher-batch-root",
                RequestId = "thin-handoff-teacher-batch-request"
            });
        var chosen2 = CreateHandoffEvaluation(root, 2, chosen: true);
        var rejected2 = CreateHandoffEvaluation(root, 2, chosen: false);
        var chosen3 = CreateHandoffEvaluation(root, 3, chosen: true);
        var rejected3 = CreateHandoffEvaluation(root, 3, chosen: false);
        var evidence2 = CreateHandoffTeacherAuthoringEvidence(2, "batch-two");
        var evidence3 = CreateHandoffTeacherAuthoringEvidence(3, "batch-three");
        var mutable = new List<DesignConversionTeacherAuthoredEvaluationPair>
        {
            new(3, chosen3, rejected3, evidence3, evidence3.GeneratedCandidateId),
            new(2, chosen2, rejected2, evidence2, evidence2.GeneratedCandidateId)
        };

        Assert.True(handoff.TryRecordTeacherAuthoredEvaluationBatch(
            root,
            chosen3,
            mutable));
        mutable.Clear();
        var first = await handoff.ResolveMeasuredRejectedEvaluationAsync(
            new DesignConversionRuntimeRejectedEvaluationLookup(chosen3.JobId));

        Assert.True(first.IsSuccess, first.ErrorMessage);
        var reference = Assert.IsType<DesignConversionRuntimeRejectedEvaluationReference>(
            first.Value);
        Assert.True(reference.TryGetTrustedTeacherAuthoredEvaluationPairs(out var pairs));
        Assert.Equal(new[] { 2, 3 }, pairs.Select(pair => pair.AttemptNumber));
        Assert.Equal(0, handoff.PendingCount);

        var second = await handoff.ResolveMeasuredRejectedEvaluationAsync(
            new DesignConversionRuntimeRejectedEvaluationLookup(chosen3.JobId));
        Assert.True(second.IsSuccess, second.ErrorMessage);
        Assert.Null(second.Value);
    }

    private static DesignConversionDpoTeacherCandidateAuthoringEvidence
        CreateHandoffTeacherAuthoringEvidence(int attemptNumber, string suffix)
        => new(
            DesignConversionDpoTeacherAttestationRuntime.AuthoringSchema,
            $"incumbent-ref-{suffix}",
            $"generated-ref-{suffix}",
            $"incumbent-binding-{suffix}",
            $"generated-binding-{suffix}",
            $"teacher-authored:{suffix}",
            $"teacher-authored-request:{suffix}",
            new string('a', 64),
            TrainingTeacherProvenance.Deterministic($"handoff-{suffix}"),
            [$"teacher-handoff:{suffix}"],
            new Dictionary<string, string>(),
            attemptNumber);

    [Fact]
    public async Task ResolveMeasuredRejectedEvaluationAsync_PreservesCancellationAndFatalGraphs()
    {
        var evaluation = CreateValidMeasuredRuntimeRejectedEvaluation();
        using var suppliedCancellation = new CancellationTokenSource();
        var supplied = new ScriptedRuntimeRejectedEvaluationSource((_, token) =>
        {
            suppliedCancellation.Cancel();
            return Task.FromException<OperationResult<
                DesignConversionRuntimeRejectedEvaluationReference?>>(
                new OperationCanceledException(token));
        });
        var suppliedRuntime = new DesignConversionThinClientJobRuntime(
            CreateRuntimeCandidateEligibleInternalEvaluator(),
            rejectedEvaluationSource: supplied);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            InvokeResolveMeasuredRejectedEvaluationAsync(
                suppliedRuntime,
                evaluation,
                suppliedCancellation.Token));

        var escaping = new Exception[]
        {
            new OperationCanceledException("foreign cancellation"),
            new OutOfMemoryException("fatal"),
            new AggregateException(
                new InvalidOperationException("ordinary"),
                new OperationCanceledException("nested foreign cancellation")),
            new AggregateException(
                new InvalidOperationException("ordinary"),
                new OutOfMemoryException("nested fatal"))
        };
        foreach (var exception in escaping)
        {
            var source = new ScriptedRuntimeRejectedEvaluationSource((_, _) =>
                Task.FromException<OperationResult<
                    DesignConversionRuntimeRejectedEvaluationReference?>>(exception));
            var runtime = new DesignConversionThinClientJobRuntime(
                CreateRuntimeCandidateEligibleInternalEvaluator(),
                rejectedEvaluationSource: source);
            var thrown = await Record.ExceptionAsync(() =>
                InvokeResolveMeasuredRejectedEvaluationAsync(
                    runtime,
                    evaluation,
                    CancellationToken.None));
            Assert.Same(exception, thrown);
        }
    }

    [Fact]
    public void RuntimeRejectedEvaluationHandoff_RejectsPrefixMatchingJobWithMismatchedRootBinding()
    {
        var handoff = new DesignConversionRuntimeRejectedEvaluationHandoff();
        var root = DesignConversionRuntimeRejectedEvaluationHandoff.BindToRootOwnership(
            CreateLoopCostAccountingInternalRequest() with
            {
                JobId = "thin-handoff-binding-root",
                RequestId = "thin-handoff-binding-request-a"
            });
        var mismatchedRequestBinding =
            DesignConversionRuntimeRejectedEvaluationHandoff.BindToRootOwnership(
                root with { RequestId = "thin-handoff-binding-request-b" });
        var chosen = CreateHandoffEvaluation(mismatchedRequestBinding, 1, chosen: true);
        var rejected = CreateHandoffEvaluation(mismatchedRequestBinding, 1, chosen: false);

        Assert.StartsWith(root.JobId + ":loop-attempt:", chosen.JobId, StringComparison.Ordinal);
        Assert.False(handoff.TryRecord(root, chosen, rejected));
        Assert.Equal(0, handoff.PendingCount);
    }

    [Fact]
    public async Task RuntimeRejectedEvaluationHandoff_ReplacesSameRootAndPreservesOtherRootChosenCollision()
    {
        var handoff = new DesignConversionRuntimeRejectedEvaluationHandoff();
        var rootA = DesignConversionRuntimeRejectedEvaluationHandoff.BindToRootOwnership(
            CreateLoopCostAccountingInternalRequest() with
            {
                JobId = "thin-handoff-replace-root",
                RequestId = "thin-handoff-replace-request-a"
            });
        var firstChosen = CreateHandoffEvaluation(rootA, 1, chosen: true);
        var firstRejected = CreateHandoffEvaluation(rootA, 1, chosen: false);
        var secondChosen = CreateHandoffEvaluation(rootA, 2, chosen: true);
        var secondRejected = CreateHandoffEvaluation(rootA, 2, chosen: false);

        Assert.True(handoff.TryRecord(rootA, firstChosen, firstRejected));
        Assert.True(handoff.TryRecord(rootA, secondChosen, secondRejected));
        var replacement = await handoff.ResolveMeasuredRejectedEvaluationAsync(
            new DesignConversionRuntimeRejectedEvaluationLookup(secondChosen.JobId));
        Assert.True(replacement.IsSuccess, replacement.ErrorMessage);
        var replacementReference = Assert.IsType<DesignConversionRuntimeRejectedEvaluationReference>(
            replacement.Value);
        Assert.True(replacementReference.TryGetTrustedHydratedEvaluation(out var replacementRejected));
        Assert.Equal(secondRejected.JobId, replacementRejected!.JobId);
        Assert.Equal(
            secondRejected.RuntimeFlowResult.Output.OutputId,
            replacementRejected.RuntimeFlowResult.Output.OutputId);
        Assert.Equal(0, handoff.PendingCount);

        Assert.True(handoff.TryRecord(rootA, firstChosen, firstRejected));
        var rootB = DesignConversionRuntimeRejectedEvaluationHandoff.BindToRootOwnership(
            rootA with { JobId = "thin-handoff-collision-root", RequestId = "thin-handoff-collision-request" });
        var collidingChosen = CreateHandoffEvaluation(rootB, 3, chosen: true) with
        {
            JobId = firstChosen.JobId
        };
        collidingChosen = collidingChosen with
        {
            SubmittedRequest = collidingChosen.SubmittedRequest with
            {
                Provenance = new Dictionary<string, string>(collidingChosen.SubmittedRequest.Provenance)
                {
                    ["internal_service_job_id"] = collidingChosen.JobId
                }
            }
        };
        var collidingRejected = CreateHandoffEvaluation(rootB, 3, chosen: false);

        Assert.False(handoff.TryRecord(rootB, collidingChosen, collidingRejected));
        Assert.Equal(1, handoff.PendingCount);
        var preserved = await handoff.ResolveMeasuredRejectedEvaluationAsync(
            new DesignConversionRuntimeRejectedEvaluationLookup(firstChosen.JobId));
        Assert.True(preserved.IsSuccess, preserved.ErrorMessage);
        Assert.NotNull(preserved.Value);
        Assert.Equal(0, handoff.PendingCount);
    }

    [Fact]
    public async Task ImprovementLoopEvaluator_EntryClearsPreloadedRejectedStateForInvalidAndSingleAttemptRequests()
    {
        var handoff = new DesignConversionRuntimeRejectedEvaluationHandoff();
        var root = DesignConversionRuntimeRejectedEvaluationHandoff.BindToRootOwnership(
            CreateLoopCostAccountingInternalRequest());
        var chosen = CreateHandoffEvaluation(root, 1, chosen: true);
        var rejected = CreateHandoffEvaluation(root, 1, chosen: false);
        var evaluator = new DesignConversionImprovementLoopThinClientJobEvaluator(
            CreateVisualJudgeInternalEvaluator(
                new ScriptedCostVisualJudge(new VisualJudgeCostScript(0.95, 0.01m))),
            candidateStrategy: null,
            rejectedEvaluationHandoff: handoff);

        Assert.True(handoff.TryRecord(root, chosen, rejected));
        var invalid = root with
        {
            Budget = root.Budget with { MaxImprovementCandidates = -1 }
        };
        var invalidResult = await evaluator.EvaluateAsync(invalid);
        Assert.False(invalidResult.IsSuccess);
        Assert.Equal(0, handoff.PendingCount);

        Assert.True(handoff.TryRecord(root, chosen, rejected));
        var singleAttempt = root with
        {
            Budget = root.Budget with { MaxImprovementCandidates = 1 }
        };
        var singleAttemptResult = await evaluator.EvaluateAsync(singleAttempt);
        Assert.True(singleAttemptResult.IsSuccess, singleAttemptResult.ErrorMessage);
        Assert.Equal(0, handoff.PendingCount);
    }

    private static IDesignConversionThinClientJobEvaluator CreateDeterministicInternalEvaluator()
        => new DesignConversionInternalServiceThinClientJobEvaluator(new DesignConversionInternalService(
            new DeterministicDesignConversionRenderProvider(),
            new DeterministicDesignConversionComparisonProvider()));

    private static async Task InvokeResolveMeasuredRejectedEvaluationAsync(
        DesignConversionThinClientJobRuntime runtime,
        DesignConversionInternalEvaluationResult evaluation,
        CancellationToken cancellationToken)
    {
        var method = typeof(DesignConversionThinClientJobRuntime).GetMethod(
            "ResolveMeasuredRejectedEvaluationAsync",
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic);
        Assert.NotNull(method);
        var task = Assert.IsAssignableFrom<Task>(method.Invoke(
            runtime,
            [evaluation, cancellationToken]));
        await task;
    }

    private static IDesignConversionThinClientJobEvaluator CreateRuntimeCandidateEligibleInternalEvaluator()
        => CreateVisualJudgeInternalEvaluator(
            new ScriptedCostVisualJudge(new VisualJudgeCostScript(Score: 0.95, CostUsd: 0.01m)));

    private static IDesignConversionThinClientJobEvaluator CreateMeasuredRuntimeCandidateEligibleEvaluator()
        => new MeasuredRuntimeCandidateEvaluator();

    private static ScriptedRuntimeRejectedEvaluationSource CreateValidRuntimeRejectedEvaluationSource()
        => new((_, _) => Task.FromResult(
            OperationResult<DesignConversionRuntimeRejectedEvaluationReference?>.Ok(
                CreateInternallyHydratedRuntimeRejectedEvaluationReference(
                    CreateValidMeasuredRuntimeRejectedEvaluation()))));

    private static ScriptedRuntimeRejectedEvaluationSource?
        CreateRuntimeRejectedEvaluationSourceForMutation(string mutation)
        => mutation switch
        {
            "omitted" => null,
            "thrown" => new((_, _) => Task.FromException<
                OperationResult<DesignConversionRuntimeRejectedEvaluationReference?>>(
                new InvalidOperationException("source-private-message:C:/private/rejected-history"))),
            "failure" => new((_, _) => Task.FromResult(
                OperationResult<DesignConversionRuntimeRejectedEvaluationReference?>.Fail(
                    "source-private-message:C:/private/rejected-history",
                    "SOURCE_PRIVATE_CODE"))),
            "null" => new((_, _) => Task.FromResult(
                OperationResult<DesignConversionRuntimeRejectedEvaluationReference?>.Ok(null))),
            "untrusted_reference" => new((_, _) => Task.FromResult(
                OperationResult<DesignConversionRuntimeRejectedEvaluationReference?>.Ok(
                    new DesignConversionRuntimeRejectedEvaluationReference("untrusted-reference")))),
            "blank_reference" => new((_, _) => Task.FromResult(
                OperationResult<DesignConversionRuntimeRejectedEvaluationReference?>.Ok(
                    new DesignConversionRuntimeRejectedEvaluationReference("  ")))),
            "mismatched_reference" => new((_, _) => Task.FromResult(
                OperationResult<DesignConversionRuntimeRejectedEvaluationReference?>.Ok(
                    DesignConversionRuntimeRejectedEvaluationReference.CreateInternallyHydrated(
                        "mismatched-opaque-reference",
                        CreateValidMeasuredRuntimeRejectedEvaluation())))),
            "same_job_identity" => new((lookup, _) => Task.FromResult(
                OperationResult<DesignConversionRuntimeRejectedEvaluationReference?>.Ok(
                    CreateInternallyHydratedRuntimeRejectedEvaluationReference(
                        CreateMeasuredRuntimeDpoEvaluation(
                            canPersistTrainingCandidate: false,
                            jobId: lookup.ChosenJobId,
                            requestId: "thin-request-runtime-same-job-identity",
                            outputId: "runtime-same-job-identity-output",
                            candidateId: "runtime-same-job-identity-candidate",
                            verdictId: "runtime-same-job-identity-verdict",
                            score: 0.46,
                            passed: false,
                            htmlSuffix: "same-job-identity"))))),
            "equal_score" => new((_, _) => Task.FromResult(
                OperationResult<DesignConversionRuntimeRejectedEvaluationReference?>.Ok(
                    CreateInternallyHydratedRuntimeRejectedEvaluationReference(
                        CreateMeasuredRuntimeRejectedEvaluation(0.95))))),
            "higher_score" => new((_, _) => Task.FromResult(
                OperationResult<DesignConversionRuntimeRejectedEvaluationReference?>.Ok(
                    CreateInternallyHydratedRuntimeRejectedEvaluationReference(
                        CreateMeasuredRuntimeRejectedEvaluation(0.96))))),
            "missing_evidence" => new((_, _) => Task.FromResult(
                OperationResult<DesignConversionRuntimeRejectedEvaluationReference?>.Ok(
                    CreateInternallyHydratedRuntimeRejectedEvaluationReference(
                        RemoveRuntimeRejectedEvidence(CreateValidMeasuredRuntimeRejectedEvaluation()))))),
            "synthetic" => new((_, _) => Task.FromResult(
                OperationResult<DesignConversionRuntimeRejectedEvaluationReference?>.Ok(
                    CreateInternallyHydratedRuntimeRejectedEvaluationReference(
                        CreateValidMeasuredRuntimeRejectedEvaluation() with
                        {
                            Warnings = ["synthetic_negative=true"]
                        })))),
            "scaffold" => new((_, _) => Task.FromResult(
                OperationResult<DesignConversionRuntimeRejectedEvaluationReference?>.Ok(
                    CreateInternallyHydratedRuntimeRejectedEvaluationReference(
                        CreateValidMeasuredRuntimeRejectedEvaluation() with
                        {
                            ScaffoldMarkers = new Dictionary<string, string>
                            {
                                ["scaffold_negative"] = bool.TrueString
                            }
                        })))),
            _ => throw new ArgumentOutOfRangeException(nameof(mutation), mutation, "Unknown rejected-evaluation mutation.")
        };

    private static DesignConversionRuntimeRejectedEvaluationReference
        CreateInternallyHydratedRuntimeRejectedEvaluationReference(
            DesignConversionInternalEvaluationResult evaluation)
        => DesignConversionRuntimeRejectedEvaluationReference.CreateInternallyHydrated(
            evaluation.JobId,
            evaluation);

    private static DesignConversionInternalEvaluationResult CreateValidMeasuredRuntimeRejectedEvaluation()
        => CreateMeasuredRuntimeRejectedEvaluation(0.46);

    private static DesignConversionInternalEvaluationResult CreateMeasuredRuntimeRejectedEvaluation(double score)
        => CreateMeasuredRuntimeDpoEvaluation(
            canPersistTrainingCandidate: false,
            jobId: "thin-job-runtime-rejected-dpo-api",
            requestId: "thin-request-rejected-001",
            outputId: "runtime-rejected-output",
            candidateId: "runtime-rejected-candidate",
            verdictId: "runtime-rejected-verdict",
            score: score,
            passed: false,
            htmlSuffix: "rejected");

    private static DesignConversionInternalEvaluationResult RemoveRuntimeRejectedEvidence(
        DesignConversionInternalEvaluationResult evaluation)
    {
        var verdict = evaluation.RuntimeFlowResult.Output.Verdict with
        {
            EvidenceRefs = []
        };
        var output = evaluation.RuntimeFlowResult.Output with
        {
            Verdict = verdict,
            EvidenceRefs = []
        };
        return evaluation with
        {
            RenderResults = [],
            ComparisonArtifacts = [],
            VisualJudgeResults = [],
            RuntimeFlowResult = evaluation.RuntimeFlowResult with
            {
                Output = output
            },
            Verdict = verdict,
            EvidenceRefs = []
        };
    }

    private static bool IsJsonPostEvaluationStateStoreAvailable()
        => Type.GetType(
            "DesignConversionDomain.JsonDesignConversionPostEvaluationStateStore, 44.DesignConversionDomain") is not null;

    private static class PostEvaluationStateStoreCompatibilityShim
    {
        public const string PersistenceProvisional = "provisional";
        public const string StateRefMetadataKey = "post_evaluation_state_ref";
        public const string PersistenceStatusMetadataKey = "post_evaluation_persistence_status";

        public static async Task<bool> IsCommittedForTrainingAsync(
            string artifactRootDirectory,
            string stateRef,
            string artifactRecordId,
            System.Threading.CancellationToken cancellationToken = default)
        {
            var stateStoreType = Type.GetType(
                "DesignConversionDomain.JsonDesignConversionPostEvaluationStateStore, 44.DesignConversionDomain");
            if (stateStoreType is null)
            {
                return stateRef.StartsWith("test-post-evaluation-state:", StringComparison.Ordinal) &&
                       !string.IsNullOrWhiteSpace(artifactRecordId) &&
                       !string.Equals(artifactRecordId, "different-artifact-record", StringComparison.Ordinal);
            }

            var method = stateStoreType!.GetMethod(
                nameof(IsCommittedForTrainingAsync),
                [
                    typeof(string),
                    typeof(string),
                    typeof(string),
                    typeof(System.Threading.CancellationToken)
                ]);
            Assert.NotNull(method);

            var task = Assert.IsAssignableFrom<Task<bool>>(method!.Invoke(
                null,
                [
                    artifactRootDirectory,
                    stateRef,
                    artifactRecordId,
                    cancellationToken
                ]));

            return await task;
        }
    }

    private static async Task AssertPostEvaluationPersistenceBoundaryAsync(
        string artifactRootDirectory,
        DesignConversionLocalArtifactDbRecord record)
    {
        var stateStoreType = Type.GetType(
            "DesignConversionDomain.JsonDesignConversionPostEvaluationStateStore, 44.DesignConversionDomain");
        if (stateStoreType is null)
        {
            Assert.Equal(bool.TrueString, record.Metadata["local_artifact_db_persistence_verified"]);
            Assert.False(record.Metadata.ContainsKey("post_evaluation_state_ref"));
            Assert.False(record.Metadata.ContainsKey("post_evaluation_persistence_status"));
            return;
        }

        var persistenceStatusKey = GetRequiredStringConstant(stateStoreType, "PersistenceStatusMetadataKey");
        var stateRefKey = GetRequiredStringConstant(stateStoreType, "StateRefMetadataKey");
        var provisionalStatus = GetRequiredStringConstant(stateStoreType, "PersistenceProvisional");
        Assert.Equal(provisionalStatus, record.Metadata[persistenceStatusKey]);

        var method = stateStoreType.GetMethod(
            "IsCommittedForTrainingAsync",
            [
                typeof(string),
                typeof(string),
                typeof(string),
                typeof(System.Threading.CancellationToken)
            ]);
        Assert.NotNull(method);

        var matching = Assert.IsAssignableFrom<Task<bool>>(method!.Invoke(
            null,
            [
                artifactRootDirectory,
                record.Metadata[stateRefKey],
                record.RecordId,
                System.Threading.CancellationToken.None
            ]));
        var different = Assert.IsAssignableFrom<Task<bool>>(method.Invoke(
            null,
            [
                artifactRootDirectory,
                record.Metadata[stateRefKey],
                "different-artifact-record",
                System.Threading.CancellationToken.None
            ]));

        Assert.True(await matching);
        Assert.False(await different);
    }

    private static string GetRequiredStringConstant(Type type, string name)
    {
        var value = type.GetField(name)?.GetRawConstantValue();
        return Assert.IsType<string>(value);
    }

    private static IEnumerable<Type> GetPublicSurfaceSignatureTypes(Type type)
    {
        yield return type;

        foreach (var constructor in type.GetConstructors())
        {
            foreach (var parameter in constructor.GetParameters())
            {
                foreach (var signatureType in ExpandSignatureType(parameter.ParameterType))
                    yield return signatureType;
            }
        }

        foreach (var method in type.GetMethods(
                     System.Reflection.BindingFlags.Public |
                     System.Reflection.BindingFlags.Instance |
                     System.Reflection.BindingFlags.Static |
                     System.Reflection.BindingFlags.DeclaredOnly))
        {
            foreach (var signatureType in ExpandSignatureType(method.ReturnType))
                yield return signatureType;
            foreach (var parameter in method.GetParameters())
            {
                foreach (var signatureType in ExpandSignatureType(parameter.ParameterType))
                    yield return signatureType;
            }
        }

        foreach (var property in type.GetProperties(
                     System.Reflection.BindingFlags.Public |
                     System.Reflection.BindingFlags.Instance |
                     System.Reflection.BindingFlags.Static |
                     System.Reflection.BindingFlags.DeclaredOnly))
        {
            foreach (var signatureType in ExpandSignatureType(property.PropertyType))
                yield return signatureType;
        }
    }

    private static IEnumerable<Type> ExpandSignatureType(Type type)
    {
        yield return type;

        if (type.HasElementType && type.GetElementType() is { } elementType)
        {
            foreach (var nestedType in ExpandSignatureType(elementType))
                yield return nestedType;
        }

        foreach (var genericArgument in type.GetGenericArguments())
        {
            foreach (var nestedType in ExpandSignatureType(genericArgument))
                yield return nestedType;
        }
    }

    private static bool IsRawRuntimeEvaluationPayloadType(Type type)
        => type == typeof(DesignConversionInternalEvaluationResult) ||
           type == typeof(SubmittedDesignConversionRequest) ||
           type == typeof(DesignConversionRenderResult) ||
           type == typeof(DesignConversionQaComparisonArtifact) ||
           type == typeof(DesignConversionVisualJudgeResult) ||
           type == typeof(DesignConversionRuntimeFlowResult) ||
           type == typeof(VisualVerdict);

    private sealed record RuntimeCreatedDpoCandidateRuntimeExecution(
        string ChosenJobId,
        DesignConversionLocalArtifactDbRecord StoredRecord,
        DesignConversionThinClientStatusResult RunResult,
        ScriptedRuntimeRejectedEvaluationSource RejectedEvaluationSource,
        TeacherAuthoredMeasuredRuntimeCandidateEvaluator PairEvaluator);

    private sealed record TeacherAuthoredRuntimeCandidateFixture(
        IDesignConversionThinClientJobEvaluator Evaluator,
        ScriptedRuntimeRejectedEvaluationSource RejectedEvaluationSource,
        TeacherAuthoredMeasuredRuntimeCandidateEvaluator PairEvaluator);

    private static async Task<RuntimeCreatedDpoCandidateRuntimeExecution> RunRuntimeCreatedDpoCandidateAsync(
        JsonlDesignConversionLocalArtifactDb db,
        DesignConversionLocalIndependentPreferenceEvaluator?
            localIndependentPreferenceEvaluator = null,
        int expectedDpoCandidateCount = 2,
        Func<int, IReadOnlyDictionary<string, string>?>? routeBindingByEvaluationCall = null)
    {
        using var teacherAssociation =
            DesignConversionDpoTeacherTestAssociation.Associate(
                db,
                supportTeacherAuthoring: true,
                supportIndependentPreferenceEvidence:
                    localIndependentPreferenceEvaluator is null);
        using var localIndependentPreferenceEvaluatorRuntime =
            localIndependentPreferenceEvaluator is null
                ? null
                : OwnLocalIndependentPreferenceEvaluatorRuntime(
                    db,
                    localIndependentPreferenceEvaluator);
        Func<DateTimeOffset> coordinatorUtcNow = () =>
            DateTimeOffset.UnixEpoch;
        var teacherAuthoredFixture =
            CreateTeacherAuthoredRuntimeCandidateFixture(
                db,
                coordinatorOwnsPreferenceEvaluation: true,
                coordinatorUtcNow,
                routeBindingByEvaluationCall);
        var rejectedEvaluationSource = teacherAuthoredFixture.RejectedEvaluationSource;
        var evaluator = new RecordingEvaluator(teacherAuthoredFixture.Evaluator);
        var runtime = new DesignConversionThinClientJobRuntime(
            evaluator,
            FixedClock(),
            db,
            rejectedEvaluationSource: rejectedEvaluationSource);
        var submitted = runtime.Submit(
            CreateTeacherAuthoredRuntimeCandidateEligibleSubmittedRequest());
        Assert.True(submitted.IsSuccess, submitted.ErrorMessage);

        var run = await runtime.RunAsync(submitted.Value!.JobId);

        Assert.True(run.IsSuccess, run.ErrorMessage);
        Assert.True(
            string.Equals(
                DesignConversionThinClientStatus.Completed,
                run.Value!.ContractStatus,
                StringComparison.Ordinal),
            $"contract_status={run.Value.ContractStatus}; warnings={string.Join('|', run.Value.Warnings)}");
        var chosen = Assert.Single(evaluator.Responses).Value!;
        Assert.True(
            rejectedEvaluationSource.ResolvedReference is not null,
            $"rejected_source_call_count={rejectedEvaluationSource.CallCount}; " +
            $"lookup={rejectedEvaluationSource.Lookup?.ChosenJobId ?? "null"}; " +
            $"warnings={string.Join('|', run.Value.Warnings)}");
        var records = await db.ListRecordsAsync(
            new DesignConversionLocalArtifactDbQuery(JobId: submitted.Value.JobId));
        Assert.True(records.IsSuccess, records.ErrorMessage);
        var stored = Assert.Single(records.Value!);
        Assert.True(
            stored.DpoCandidates.Count == expectedDpoCandidateCount,
            $"dpo_candidate_count={stored.DpoCandidates.Count}; " +
            $"warnings={string.Join('|', run.Value.Warnings)}; " +
            $"metadata={string.Join('|', stored.Metadata.OrderBy(pair => pair.Key).Select(pair => $"{pair.Key}={pair.Value}"))}");
        if (expectedDpoCandidateCount > 0)
        {
            await AssertPostEvaluationPersistenceBoundaryAsync(
                db.AllowedArtifactRootDirectory,
                stored);
        }

        return new RuntimeCreatedDpoCandidateRuntimeExecution(
            chosen.JobId,
            stored,
            run.Value,
            rejectedEvaluationSource,
            teacherAuthoredFixture.PairEvaluator);
    }

    private static DesignConversionLocalIndependentPreferenceEvaluatorRuntime
        OwnLocalIndependentPreferenceEvaluatorRuntime(
            IDesignConversionLocalArtifactDb artifactDb,
            DesignConversionLocalIndependentPreferenceEvaluator evaluator)
    {
        var owned =
            DesignConversionLocalIndependentPreferenceEvaluatorRuntime
                .OwnForTesting(artifactDb, evaluator);
        Assert.True(owned.IsSuccess, owned.ErrorMessage);
        return owned.Value!;
    }

    private static TeacherAuthoredRuntimeCandidateFixture
        CreateTeacherAuthoredRuntimeCandidateFixture(
        IDesignConversionLocalArtifactDb artifactDb,
        bool coordinatorOwnsPreferenceEvaluation = false,
        Func<DateTimeOffset>? utcNow = null,
        Func<int, IReadOnlyDictionary<string, string>?>? routeBindingByEvaluationCall = null)
    {
        var handoff = new DesignConversionRuntimeRejectedEvaluationHandoff();
        var pairEvaluator =
            new TeacherAuthoredMeasuredRuntimeCandidateEvaluator(
                routeBindingByEvaluationCall);
        var evaluator = new DesignConversionImprovementLoopThinClientJobEvaluator(
            pairEvaluator,
            new DesignConversionDpoTeacherAuthoringCandidateStrategy(
                new SingleCandidateStrategy(),
                artifactDb),
            handoff,
            coordinatorOwnsPreferenceEvaluation ? artifactDb : null,
            utcNow);
        var rejectedEvaluationSource = new ScriptedRuntimeRejectedEvaluationSource(
            handoff.ResolveMeasuredRejectedEvaluationAsync);
        return new TeacherAuthoredRuntimeCandidateFixture(
            evaluator,
            rejectedEvaluationSource,
            pairEvaluator);
    }

    private static IReadOnlyDictionary<string, string> BuildW053RouteAdmissionBinding(
        string tenantIdRef,
        string scopeRef,
        string? rawTenant = null,
        string? rawScope = null,
        string? rawToken = null)
    {
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [DesignConversionThinClientJobRuntime.ServerOwnedRouteAdmissionTenantBoundMetadataKey] = bool.TrueString,
            [DesignConversionThinClientJobRuntime.ServerOwnedRouteAdmissionTenantIdRefMetadataKey] = tenantIdRef,
            [DesignConversionThinClientJobRuntime.ServerOwnedRouteAdmissionScopeRefMetadataKey] = scopeRef
        };
        if (rawTenant is not null)
            metadata["server_owned_route_admission_raw_tenant"] = rawTenant;
        if (rawScope is not null)
            metadata["server_owned_route_admission_raw_scope"] = rawScope;
        if (rawToken is not null)
            metadata["server_owned_route_admission_raw_token"] = rawToken;
        return metadata;
    }

    private static string BuildW053RouteAdmissionScopeRef(string source)
        => "sha256:design-conversion-route-admission-scope:" +
           Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(source)))
               .ToLowerInvariant();

    internal static DesignConversionInternalEvaluationResult CreateMeasuredRuntimeDpoEvaluation(
        bool canPersistTrainingCandidate,
        string jobId,
        string requestId,
        string outputId,
        string candidateId,
        string verdictId,
        double score,
        bool passed,
        string htmlSuffix)
    {
        var resolution = new RenderQaResolution("desktop", 1440, 900, 1);
        var expected = new RenderQaImageRecord(
            "expected-image",
            "expected",
            resolution,
            "artifact://expected",
            "sha256:expected",
            new Dictionary<string, string>());
        var actual = expected with
        {
            ImageId = "actual-image",
            Label = "actual",
            Uri = "artifact://actual",
            Sha256 = "sha256:actual"
        };
        var request = new SubmittedDesignConversionRequest(
            RequestId: requestId,
            SourceKind: DesignConversionSourceKind.Figma,
            Html: "<main class=\"source\">Source</main>",
            Css: ".source{display:grid;gap:4px;}",
            DesignTreePayload: new DesignTreePayload(
                "design-tree",
                "json",
                "{\"node\":\"frame\"}",
                "sha256:design-tree",
                new Dictionary<string, string>()),
            OriginalTreePayload: new DesignTreePayload(
                "original-tree",
                "json",
                "{\"node\":\"frame\"}",
                "sha256:original-tree",
                new Dictionary<string, string>()),
            Screenshots:
            [
                new DesignRenderArtifact(
                    "source-screenshot",
                    "screenshot",
                    1440,
                    900,
                    "artifact://source-screenshot",
                    "sha256:source-screenshot",
                    new Dictionary<string, string>())
            ],
            DesignRenders: [],
            Budget: new DesignConversionBudgetMetadata(
                "user-redacted",
                "free",
                1,
                1,
                0m,
                new Dictionary<string, string>()),
            Provenance: new Dictionary<string, string>
            {
                ["source_manifest_ref"] = "manifest://runtime-source"
            });
        var verdict = new VisualVerdict(
            VerdictId: verdictId,
            Score: score,
            Defects:
            [
                new VisualDefect(
                    DefectId: $"{verdictId}:spacing",
                    Category: DesignConversionNegativeReasonTaxonomy.Spacing,
                    Severity: passed ? "info" : "major",
                    Description: "Teacher evidence compares visual spacing.",
                    X: 20,
                    Y: 40,
                    Width: 100,
                    Height: 30,
                    EvidenceImageIds: ["expected", "actual"])
            ],
            EvidenceRefs: [$"visual-verdict:{verdictId}"],
            Passed: passed);
        var qaResult = new RenderQaResultRecord(
            ResultId: $"{outputId}-qa-result",
            RequestId: request.RequestId,
            Expected: expected,
            Actual: actual,
            Verdict: verdict,
            Metrics: new Dictionary<string, double> { ["pixel_delta"] = passed ? 0.01 : 0.18 },
            Warnings: []);
        var output = new DesignConversionImprovementOutput(
            OutputId: outputId,
            CandidateId: candidateId,
            Html: $"<main class=\"{htmlSuffix}\">Source</main>",
            Css: $".{htmlSuffix}{{display:grid;gap:{(passed ? 16 : 1)}px;}}",
            Verdict: verdict,
            QaResults: [qaResult],
            EvidenceRefs: [$"qa-teacher:{outputId}"],
            Accepted: passed);
        var candidate = new DesignConversionImprovementCandidate(
            CandidateId: candidateId,
            RequestId: request.RequestId,
            Html: output.Html,
            Css: output.Css,
            ChangeReasons: ["measured_spacing"],
            EvidenceRefs: ["runtime-candidate:evidence"],
            Metadata: new Dictionary<string, string>());
        var runtimeFlow = new DesignConversionRuntimeFlowResult(
            Request: request,
            CriticOutput: new DesignConversionCriticUnitOutput(
                OutputId: "critic-output",
                Verdict: verdict,
                QaEvidenceRefs: ["qa:evidence"],
                AiTeacherEvidenceRefs: [],
                DeterministicLogicClaimedAsLearning: false),
            ImproverOutput: new DesignConversionImproverUnitOutput(
                OutputId: "improver-output",
                Candidate: candidate,
                Output: output,
                QaEvidenceRefs: ["qa:evidence"],
                AiTeacherEvidenceRefs: [],
                DeterministicLogicClaimedAsLearning: false),
            Candidate: candidate,
            Output: output,
            UnitExecutionOrder: [DesignConversionDpoUnits.CriticUnit, DesignConversionDpoUnits.ImproverUnit],
            EvidenceRefs: ["runtime-flow:evidence"],
            DeterministicLogicClaimedAsLearning: false);
        var render = new DesignConversionRenderResult(
            $"{outputId}-render-result",
            $"{outputId}-render-request",
            actual,
            ElapsedMs: 12,
            Warnings: []);
        var comparison = new DesignConversionQaComparisonArtifact(
            $"{outputId}-comparison-artifact",
            $"{outputId}-comparison-request",
            expected,
            actual,
            verdict,
            "artifact://diff",
            "sha256:diff",
            new Dictionary<string, double> { ["pixel_delta"] = 0.08 },
            EvidenceRefs: ["comparison:evidence"]);
        var judge = new DesignConversionVisualJudgeResult(
            JudgeResultId: $"{outputId}-judge-result",
            JudgeRequestId: $"{outputId}-judge-request",
            Status: ExternalHttpDesignConversionVisualJudge.StatusOk,
            IsBlocked: false,
            Verdict: verdict,
            Provider: new DesignConversionVisualJudgeProviderMetadata(
                ProviderName: "typed-fixture-visual-judge",
                EndpointHost: "typed-fixture.visual.test",
                ModelId: "typed-fixture-model",
                ExternalCallExecuted: true,
                Metadata: new Dictionary<string, string>()),
            EstimatedCostUsd: 0.01m,
            TokenMetadata: new Dictionary<string, double>(),
            EvidenceRefs: ["judge:evidence"],
            Metadata: new Dictionary<string, string>());
        var boundary = new DesignConversionVisualQaBoundaryTrace(
            MultiResolutionRender: new DesignConversionViewportCoverage(
                IsMultiResolution: false,
                IsComplete: true,
                RequiredViewportIds: ["desktop"],
                ObservedViewportIds: ["desktop"],
                MissingViewportIds: [],
                EvidenceRefs: ["viewport:evidence"]),
            VisualJudgeStatus: ExternalHttpDesignConversionVisualJudge.StatusOk,
            VisualJudgeProviderBoundary: DesignConversionVisualQaBoundaryPolicy.ConfiguredOrLocalLabelerBoundary,
            ExternalAiVisualJudgeExecuted: true,
            TierPolicy: new DesignConversionTierPolicyDecision(
                Tier: "free",
                RequiredScore: 0.8,
                BestScore: score,
                EstimatedCostUsd: 0.01m,
                BudgetUsd: 0m,
                RemainingBudgetUsd: 0m,
                AttemptCount: 1,
                MaxImprovementCandidates: 1,
                StopReason: passed
                    ? DesignConversionVisualQaBoundaryPolicy.QualityReached
                    : DesignConversionVisualQaBoundaryPolicy.ContinueImprovement,
                ShouldContinue: false,
                EvidenceRefs: ["tier:evidence"],
                Metadata: new Dictionary<string, string>()),
            VisualDecision: new DesignConversionVisualComparisonDecision(
                Decision: passed
                    ? DesignConversionVisualQaBoundaryPolicy.DecisionAccepted
                    : DesignConversionVisualQaBoundaryPolicy.DecisionRetry,
                QualityScore: score,
                RequiredThreshold: 0.8,
                EstimatedCostUsd: 0.01m,
                BudgetUsd: 0m,
                RemainingBudgetUsd: 0m,
                CostBudgetMet: true,
                QualityThresholdMet: passed,
                CanRetry: !passed,
                CanPersistTrainingCandidate: canPersistTrainingCandidate,
                ProviderBoundaryStatus: DesignConversionVisualQaBoundaryPolicy.ConfiguredOrLocalLabelerBoundary,
                ExternalAiVisualJudgeExecuted: true,
                CorrectionFindings: [],
                EvidenceRefs: ["visual-decision:evidence"],
                Metadata: new Dictionary<string, string>()),
            ProductReady: false,
            TrainedFeatureClaimed: false,
            EvidenceRefs: ["boundary:evidence"],
            Warnings: ["product_not_ready"]);

        return new DesignConversionInternalEvaluationResult(
            JobId: jobId,
            SubmittedRequest: request,
            RenderResults: [render],
            ComparisonArtifacts: [comparison],
            VisualJudgeResults: [judge],
            QaResults: [],
            RuntimeFlowResult: runtimeFlow,
            Verdict: verdict,
            ImprovedHtml: output.Html,
            ImprovedCss: output.Css,
            EvidenceRefs: ["runtime:evidence"],
            Warnings: ["product_not_ready"],
            ScaffoldMarkers: new Dictionary<string, string>(),
            DeterministicLogicClaimedAsLearning: false,
            VisualQaBoundaryTrace: boundary)
        {
            VisualCostAccounting = DesignConversionVisualCostAccounting.FromTierPolicy(
                boundary.TierPolicy,
                evaluationAttemptCount: 1)
        };
    }

    private static DesignConversionInternalEvaluationResult CreateHandoffEvaluation(
        DesignConversionInternalEvaluateRequest root,
        int index,
        bool chosen)
    {
        var evaluation = CreateMeasuredRuntimeDpoEvaluation(
            canPersistTrainingCandidate: chosen,
            jobId: chosen
                ? $"{root.JobId}:loop-attempt:2"
                : root.JobId,
            requestId: chosen
                ? $"{root.RequestId}:runtime-improvement:2"
                : root.RequestId,
            outputId: $"thin-handoff-{index.ToString(CultureInfo.InvariantCulture)}-{(chosen ? "chosen" : "rejected")}-output",
            candidateId: $"thin-handoff-{index.ToString(CultureInfo.InvariantCulture)}-{(chosen ? "chosen" : "rejected")}-candidate",
            verdictId: $"thin-handoff-{index.ToString(CultureInfo.InvariantCulture)}-{(chosen ? "chosen" : "rejected")}-verdict",
            score: chosen ? 0.95 : 0.46,
            passed: chosen,
            htmlSuffix: chosen ? "handoff-chosen" : "handoff-rejected");
        return evaluation with
        {
            SubmittedRequest = evaluation.SubmittedRequest with
            {
                Provenance = new Dictionary<string, string>(root.Provenance)
                {
                    ["internal_service_job_id"] = evaluation.JobId
                }
            }
        };
    }

    private static IDesignConversionThinClientJobEvaluator CreateLocalPngInternalEvaluator(string artifactRoot)
        => new DesignConversionInternalServiceThinClientJobEvaluator(new DesignConversionInternalService(
            new DeterministicLocalDesignConversionPngRenderProvider(artifactRoot),
            new DeterministicDesignConversionComparisonProvider()));

    private static IDesignConversionThinClientJobEvaluator CreateVisualJudgeInternalEvaluator(
        IDesignConversionVisualJudge visualJudge)
        => new DesignConversionInternalServiceThinClientJobEvaluator(new DesignConversionInternalService(
            new DeterministicDesignConversionRenderProvider(),
            new DeterministicDesignConversionComparisonProvider(),
            visualJudge));

    private static DesignConversionInternalEvaluateRequest CreateLoopCostAccountingInternalRequest()
    {
        var submitted = CreateSubmittedRequest() with
        {
            RequestId = "thin-loop-cost-request-001",
            Budget = CreateSubmittedRequest().Budget with
            {
                Tier = "paid",
                MaxImprovementCandidates = 2,
                MaxRenderQaRuns = 1,
                BudgetUsd = 0.25m,
                Metadata = new Dictionary<string, string>
                {
                    ["quality_threshold"] = "0.95"
                }
            }
        };
        var resolutions = DesignConversionRenderResolutionFactory.CreateModelServerMinimumResponsiveMatrix(
            submitted.DesignRenders.Concat(submitted.Screenshots).ToArray());
        Assert.True(resolutions.IsSuccess, resolutions.ErrorMessage);
        var singleRequiredResolution = resolutions.Value!.Take(1).ToArray();
        Assert.Single(singleRequiredResolution);

        return new DesignConversionInternalEvaluateRequest(
            JobId: "thin-job-loop-cost-accounting",
            RequestId: submitted.RequestId,
            SourceKind: submitted.SourceKind,
            Html: submitted.Html,
            Css: submitted.Css,
            DesignTreePayload: submitted.DesignTreePayload,
            OriginalTreePayload: submitted.OriginalTreePayload,
            Screenshots: submitted.Screenshots,
            DesignRenders: submitted.DesignRenders,
            Budget: submitted.Budget,
            RenderResolutions: singleRequiredResolution,
            AiTeacherEvidenceRefs: [],
            DeterministicEvidenceRefs: new Dictionary<string, string>
            {
                ["thin_client_job_runtime"] = DesignConversionThinClientJobRuntime.RuntimeBoundary
            },
            Provenance: submitted.Provenance);
    }

    private static IReadOnlyDictionary<string, string> BuildServerOwnedReceipt(
        string attemptCount,
        string costPerAttemptUsd,
        string totalCostUsd)
        => new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountKey] = attemptCount,
            [ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountSourceKey] = ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountSourceValue,
            [ExternalHttpDesignConversionVisualJudge.ServerOwnedCostPerAttemptUsdKey] = costPerAttemptUsd,
            [ExternalHttpDesignConversionVisualJudge.ServerOwnedTotalCostUsdKey] = totalCostUsd,
            [ExternalHttpDesignConversionVisualJudge.ServerOwnedCostAccountingKey] = ExternalHttpDesignConversionVisualJudge.ServerOwnedCostAccountingFixedPerAttempt
        };

    private static IReadOnlyList<string> ServerOwnedReceiptKeys()
        =>
        [
            ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountKey,
            ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountSourceKey,
            ExternalHttpDesignConversionVisualJudge.ServerOwnedCostPerAttemptUsdKey,
            ExternalHttpDesignConversionVisualJudge.ServerOwnedTotalCostUsdKey,
            ExternalHttpDesignConversionVisualJudge.ServerOwnedCostAccountingKey
        ];

    private static void AssertNoUserOrBillingIdentityMetadataKeys(IReadOnlyDictionary<string, string> metadata)
    {
        foreach (var key in metadata.Keys)
        {
            var normalized = key.Replace("-", "_", StringComparison.Ordinal).ToLowerInvariant();
            Assert.DoesNotContain("billing", normalized, StringComparison.Ordinal);
            Assert.DoesNotContain("user_id", normalized, StringComparison.Ordinal);
            Assert.DoesNotContain("userid", normalized, StringComparison.Ordinal);
            Assert.DoesNotContain("identity", normalized, StringComparison.Ordinal);
            Assert.NotEqual("user", normalized);
            Assert.NotEqual("user_ref", normalized);
            Assert.NotEqual("reviewer_ref", normalized);
        }
    }

    private static string CreatePreChangeVisualCostLedgerJsonLineForTest(
        DesignConversionVisualCostLedgerRecord record,
        string? userRef = null,
        string? periodKind = null,
        DateTimeOffset? periodStartUtc = null,
        DateTimeOffset? periodEndExclusiveUtc = null)
    {
        var row = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["recordId"] = record.RecordId,
            ["ledgerKind"] = record.LedgerKind,
            ["schemaVersion"] = record.SchemaVersion,
            ["evaluationJobRef"] = record.EvaluationJobRef,
            ["submittedRequestRef"] = record.SubmittedRequestRef,
            ["totalCostUsd"] = record.TotalCostUsd,
            ["measurementComplete"] = record.MeasurementComplete,
            ["missingMeasurementCount"] = record.MissingMeasurementCount,
            ["evaluationAttemptCount"] = record.EvaluationAttemptCount,
            ["costSource"] = record.CostSource,
            ["recordedAtUtc"] = record.RecordedAtUtc,
            ["evidenceRefs"] = new[] { BuildCanonicalVisualCostEvidenceRefForTest(record.RecordId) },
            ["metadata"] = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["ledger_purpose"] = "operational_visual_cost",
                ["training_artifact_db"] = bool.FalseString,
                ["quota_enforcement"] = bool.FalseString,
                ["raw_payloads_persisted"] = bool.FalseString
            }
        };

        if (userRef is not null)
            row["userRef"] = userRef;
        if (periodKind is not null)
            row["periodKind"] = periodKind;
        if (periodStartUtc.HasValue)
            row["periodStartUtc"] = periodStartUtc.Value;
        if (periodEndExclusiveUtc.HasValue)
            row["periodEndExclusiveUtc"] = periodEndExclusiveUtc.Value;

        return JsonSerializer.Serialize(row, new JsonSerializerOptions(JsonSerializerDefaults.Web));
    }

    private static string BuildCanonicalVisualCostEvidenceRefForTest(string recordId)
        => $"{JsonlDesignConversionVisualCostLedger.EvidenceRefPrefix}-{Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(recordId))).ToLowerInvariant()}";

    private static DesignConversionVisualCostLedgerRecord CreateVisualCostLedgerRecordForTest(
        string jobId,
        string requestId,
        decimal totalCostUsd,
        DateTimeOffset recordedAtUtc,
        string? userRef = null,
        string? submittedTier = null,
        DateTimeOffset? periodStartUtc = null,
        DateTimeOffset? periodEndExclusiveUtc = null,
        bool measurementComplete = true,
        int missingMeasurementCount = 0,
        int evaluationAttemptCount = 1,
        string? costSource = null)
    {
        var evaluationJobRef = JsonlDesignConversionVisualCostLedger.BuildCanonicalEvaluationJobRef(jobId);
        var submittedRequestRef = JsonlDesignConversionVisualCostLedger.BuildCanonicalSubmittedRequestRef(requestId);
        var period = JsonlDesignConversionVisualCostLedger.BuildUtcCalendarMonthPeriod(recordedAtUtc);
        var accounting = new DesignConversionVisualCostAccounting(
            TotalCostUsd: totalCostUsd,
            MeasurementComplete: measurementComplete,
            MissingMeasurementCount: missingMeasurementCount,
            EvaluationAttemptCount: evaluationAttemptCount,
            CostSource: costSource ?? DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostSourceFixedPerRequest);

        return new DesignConversionVisualCostLedgerRecord(
            RecordId: JsonlDesignConversionVisualCostLedger.BuildRecordId(
                evaluationJobRef,
                submittedRequestRef,
                accounting),
            LedgerKind: JsonlDesignConversionVisualCostLedger.LedgerKind,
            SchemaVersion: JsonlDesignConversionVisualCostLedger.SchemaVersion,
            EvaluationJobRef: evaluationJobRef,
            SubmittedRequestRef: submittedRequestRef,
            TotalCostUsd: accounting.TotalCostUsd,
            MeasurementComplete: accounting.MeasurementComplete,
            MissingMeasurementCount: accounting.MissingMeasurementCount,
            EvaluationAttemptCount: accounting.EvaluationAttemptCount,
            CostSource: accounting.CostSource,
            RecordedAtUtc: recordedAtUtc,
            EvidenceRefs: [],
            Metadata: new Dictionary<string, string>()) with
            {
                UserRef = userRef ?? JsonlDesignConversionVisualCostLedger.BuildPseudonymousUserRef($"test-ledger-user:{jobId}").Value!,
                SubmittedTier = submittedTier ?? JsonlDesignConversionVisualCostLedger.UnknownSubmittedTier,
                PeriodKind = JsonlDesignConversionVisualCostLedger.UtcCalendarMonthPeriodKind,
                PeriodStartUtc = periodStartUtc ?? period.StartUtc,
                PeriodEndExclusiveUtc = periodEndExclusiveUtc ?? period.EndExclusiveUtc
            };
    }

    private static void AssertSameVisualCostLedgerRecord(
        DesignConversionVisualCostLedgerRecord expected,
        DesignConversionVisualCostLedgerRecord actual)
    {
        Assert.Equal(expected.RecordId, actual.RecordId);
        Assert.Equal(expected.LedgerKind, actual.LedgerKind);
        Assert.Equal(expected.SchemaVersion, actual.SchemaVersion);
        Assert.Equal(expected.EvaluationJobRef, actual.EvaluationJobRef);
        Assert.Equal(expected.SubmittedRequestRef, actual.SubmittedRequestRef);
        Assert.Equal(expected.TotalCostUsd, actual.TotalCostUsd);
        Assert.Equal(expected.MeasurementComplete, actual.MeasurementComplete);
        Assert.Equal(expected.MissingMeasurementCount, actual.MissingMeasurementCount);
        Assert.Equal(expected.EvaluationAttemptCount, actual.EvaluationAttemptCount);
        Assert.Equal(expected.CostSource, actual.CostSource);
        Assert.Equal(expected.RecordedAtUtc, actual.RecordedAtUtc);
        Assert.Equal(expected.UserRef, actual.UserRef);
        Assert.Equal(expected.SubmittedTier, actual.SubmittedTier);
        Assert.Equal(expected.PeriodKind, actual.PeriodKind);
        Assert.Equal(expected.PeriodStartUtc, actual.PeriodStartUtc);
        Assert.Equal(expected.PeriodEndExclusiveUtc, actual.PeriodEndExclusiveUtc);
        Assert.Equal(expected.EvidenceRefs, actual.EvidenceRefs);
        Assert.Equal(
            expected.Metadata.OrderBy(pair => pair.Key, StringComparer.Ordinal).ToArray(),
            actual.Metadata.OrderBy(pair => pair.Key, StringComparer.Ordinal).ToArray());
    }

    private static DesignConversionMachineCheckpoint CreateRuntimeLearnedInferenceCheckpoint()
        => new(
            SchemaVersion: DesignConversionMachine.CheckpointSchemaVersion,
            MachineId: DesignConversionMachine.MachineId,
            DomainTag: DesignConversionMachine.DomainTag,
            Version: "0.1.116",
            AccuracyAfterLastBatch: 0.72,
            HasTrainingScaffoldHistory: true,
            ExportedAtUtc: DateTimeOffset.UnixEpoch,
            CriticUnit: LearnedRuntimeUnit(
                DesignConversionRuntimeScaffoldCriticUnit.UnitId,
                DesignConversionRuntimeScaffoldCriticUnit.Role,
                "runtime_scaffold_input_evidence_only_with_local_learned_weight_artifact",
                LearnedWeight("source_kind:figma", 1.0, "source_kind_presence")),
            ImproverUnit: LearnedRuntimeUnit(
                DesignConversionRuntimeScaffoldImproverUnit.UnitId,
                DesignConversionRuntimeScaffoldImproverUnit.Role,
                "runtime_scaffold_candidate_shell_with_local_learned_weight_artifact",
                LearnedWeight("source_kind:figma", 0.7, "source_kind_presence"),
                LearnedWeight("correction_category:generic-visual-defect", 0.5, "chosen_correction_category_preference")),
            TrainingHistory: [],
            EvidenceRefs:
            [
                DesignConversionMachine.LearnedWeightsPersistedEvidence,
                DesignConversionMachine.LearnedWeightsSanitizedFieldsEvidence
            ],
            DeterministicLogicClaimedAsLearning: false);

    private static DesignConversionRuntimeUnitCheckpoint LearnedRuntimeUnit(
        string unitId,
        string unitRole,
        string implementationStatus,
        params DesignConversionLearnedWeightCheckpoint[] learnedWeights)
        => new(
            UnitId: unitId,
            UnitRole: unitRole,
            DomainTag: DesignConversionMachine.DomainTag,
            ImplementationStatus: implementationStatus,
            EvidenceRefs:
            [
                DesignConversionMachine.LearnedWeightsPersistedEvidence,
                DesignConversionMachine.LearnedWeightsSanitizedFieldsEvidence
            ],
            LearnedWeightsPresent: true,
            LearnedWeights: learnedWeights);

    private static DesignConversionLearnedWeightCheckpoint LearnedWeight(
        string featureKey,
        double weight,
        string derivation)
        => new(
            FeatureKey: featureKey,
            Weight: weight,
            ObservationCount: 4,
            LastBatchId: "pass116-runtime-learned-inference-test",
            Derivation: derivation,
            SourceIdentityHashes:
            [
                "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"
            ],
            EvidenceRefs:
            [
                DesignConversionMachine.LearnedWeightsPersistedEvidence,
                DesignConversionMachine.LearnedWeightsSanitizedFieldsEvidence,
                "DESIGN_CONVERSION_LEARNED_WEIGHT_NO_RAW_TREE_HTML_SCREENSHOT"
            ]);

    private static Func<DateTimeOffset> FixedClock()
    {
        var ticks = 0;
        return () => DateTimeOffset.UnixEpoch.AddSeconds(ticks++);
    }

    private static SubmittedDesignConversionRequest CreateSubmittedRequest()
        => new(
            RequestId: "thin-request-001",
            SourceKind: DesignConversionSourceKind.Figma,
            Html: "<main class=\"frame\">Design</main>",
            Css: ".frame{display:grid;gap:8px;}",
            DesignTreePayload: new DesignTreePayload(
                PayloadId: "tree-current",
                Format: "figma-json",
                Content: "{\"type\":\"FRAME\"}",
                Sha256: "sha256:tree-current",
                Metadata: new Dictionary<string, string>()),
            OriginalTreePayload: new DesignTreePayload(
                PayloadId: "tree-original",
                Format: "original-json",
                Content: "{\"type\":\"FRAME\"}",
                Sha256: "sha256:tree-original",
                Metadata: new Dictionary<string, string>()),
            Screenshots:
            [
                new DesignRenderArtifact(
                    ArtifactId: "screenshot-001",
                    Kind: "screenshot",
                    Width: 1440,
                    Height: 900,
                    Uri: "artifact://screenshot.png",
                    Sha256: "sha256:screenshot",
                    Metadata: new Dictionary<string, string>())
            ],
            DesignRenders:
            [
                new DesignRenderArtifact(
                    ArtifactId: "render-001",
                    Kind: "design-render",
                    Width: 1440,
                    Height: 900,
                    Uri: "artifact://render.png",
                    Sha256: "sha256:render",
                    Metadata: new Dictionary<string, string>())
            ],
            Budget: new DesignConversionBudgetMetadata(
                UserId: "user-redacted",
                Tier: "thin-client",
                MaxImprovementCandidates: 1,
                MaxRenderQaRuns: 1,
                BudgetUsd: 0.25m,
                Metadata: new Dictionary<string, string>()),
            Provenance: new Dictionary<string, string>
            {
                ["boundary"] = "thin-client-runtime-test"
            });

    private static void AssertThinClientNonReadyVisualDecision(
        DesignConversionThinClientStatusResult status,
        string expectedVisualDecision,
        string expectedStopReason)
    {
        Assert.Equal(DesignConversionThinClientStatus.NotReady, status.ContractStatus);
        Assert.Equal(DesignConversionThinClientStatus.NotReady, status.Job.Status);
        Assert.Equal(expectedVisualDecision, status.Provenance["final_visual_decision"]);
        Assert.Equal(expectedStopReason, status.Provenance["tier_policy_stop_reason"]);
        Assert.Equal(DesignConversionThinClientStatus.NotReady, status.Provenance["thin_client_terminal_status"]);
        Assert.Contains($"final_visual_decision={expectedVisualDecision}", status.Warnings);
        Assert.Contains($"tier_policy_stop_reason={expectedStopReason}", status.Warnings);
        Assert.Contains($"thin_client_terminal_status={DesignConversionThinClientStatus.NotReady}", status.Warnings);
        Assert.True(status.QualityMetrics.Passed.HasValue);
        Assert.False(status.QualityMetrics.Passed.Value);
    }

    private static void AssertThinClientQuarantinedVisualDecision(
        DesignConversionThinClientStatusResult status)
    {
        Assert.Equal(DesignConversionThinClientStatus.Quarantined, status.ContractStatus);
        Assert.Equal(DesignConversionThinClientStatus.Quarantined, status.Job.Status);
        Assert.Equal(
            DesignConversionVisualQaBoundaryPolicy.DecisionQuarantine,
            status.Provenance["final_visual_decision"]);
        Assert.Equal(
            DesignConversionVisualQaBoundaryPolicy.RenderManifestIncomplete,
            status.Provenance["tier_policy_stop_reason"]);
        Assert.Equal(
            DesignConversionThinClientStatus.Quarantined,
            status.Provenance["thin_client_terminal_status"]);
        Assert.Contains(
            $"final_visual_decision={DesignConversionVisualQaBoundaryPolicy.DecisionQuarantine}",
            status.Warnings);
        Assert.Contains(
            $"tier_policy_stop_reason={DesignConversionVisualQaBoundaryPolicy.RenderManifestIncomplete}",
            status.Warnings);
        Assert.Contains(
            $"thin_client_terminal_status={DesignConversionThinClientStatus.Quarantined}",
            status.Warnings);
    }

    private static void AssertStaleRunningFailure(
        DesignConversionThinClientStatusResult status,
        TimeSpan expectedTimeout)
    {
        var timeoutSeconds = expectedTimeout.TotalSeconds.ToString("0.###", CultureInfo.InvariantCulture);

        Assert.Equal(DesignConversionThinClientStatus.Failed, status.ContractStatus);
        Assert.Equal(DesignConversionThinClientStatus.Failed, status.Job.Status);
        Assert.Equal(
            DesignConversionThinClientJobRuntime.StaleRunningTimeoutErrorCode,
            status.Job.Metadata["error_code"]);
        Assert.Equal(
            DesignConversionThinClientJobRuntime.StaleRunningTimeoutSafeMessage,
            status.Message);
        Assert.Empty(status.Artifacts);
        Assert.False(status.QualityMetrics.Available);
        Assert.Equal("stale_running_cleanup", status.QualityMetrics.Source);
        Assert.Equal(
            "runtime_output_not_produced_stale_running_timeout",
            status.QualityMetrics.UnavailableReason);
        Assert.Equal(bool.TrueString, status.Job.Metadata["stale_running_cleanup"]);
        Assert.Equal(timeoutSeconds, status.Job.Metadata["stale_running_timeout_seconds"]);
        Assert.Equal(bool.FalseString, status.Job.Metadata["runtime_output_produced"]);
        Assert.Equal(bool.FalseString, status.Job.Metadata["evaluator_reinvoked_after_stale_cleanup"]);
        Assert.Equal(
            DesignConversionThinClientJobRuntime.StaleRunningTimeoutErrorCode,
            status.Provenance["internal_evaluator_error_code"]);
        Assert.Equal(bool.TrueString, status.Provenance["stale_running_cleanup"]);
        Assert.Equal(timeoutSeconds, status.Provenance["stale_running_timeout_seconds"]);
        Assert.Equal(bool.FalseString, status.Provenance["runtime_output_produced"]);
        Assert.Equal(bool.FalseString, status.Provenance["evaluator_reinvoked_after_stale_cleanup"]);
        Assert.Equal(DesignConversionThinClientStatus.Failed, status.Provenance["thin_client_terminal_status"]);
        Assert.Contains(
            $"internal_evaluator_error_code={DesignConversionThinClientJobRuntime.StaleRunningTimeoutErrorCode}",
            status.Warnings);
        Assert.Contains("stale_running_cleanup=true", status.Warnings);
        Assert.Contains($"stale_running_timeout_seconds={timeoutSeconds}", status.Warnings);
        Assert.Contains("runtime_output_produced=false", status.Warnings);
        Assert.Contains("evaluator_reinvoked_after_stale_cleanup=false", status.Warnings);
        Assert.Contains($"thin_client_terminal_status={DesignConversionThinClientStatus.Failed}", status.Warnings);
    }

    private static void AssertNoPrivateSubmittedPayloadLeakage(
        DesignConversionThinClientStatusResult status,
        SubmittedDesignConversionRequest request)
    {
        var serialized = JsonSerializer.Serialize(status);
        Assert.DoesNotContain(request.Html, serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(request.Css, serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(request.DesignTreePayload.Content, serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(request.OriginalTreePayload.Content, serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("private-frame", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("raw-tree-private", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("C:/Users/lubao", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("exact_prompt", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("private.fig", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("external-provider-payload", serialized, StringComparison.OrdinalIgnoreCase);
    }

    private static void AssertNoDependencyPrivateMarkerLeakage(
        DesignConversionThinClientStatusResult status,
        params string[] privateMarkers)
    {
        var serialized = JsonSerializer.Serialize(status);
        foreach (var marker in privateMarkers)
        {
            Assert.DoesNotContain(marker, status.Message, StringComparison.Ordinal);
            Assert.All(
                status.Warnings,
                warning => Assert.DoesNotContain(marker, warning, StringComparison.Ordinal));
            Assert.All(
                status.Job.Metadata,
                metadata =>
                {
                    Assert.DoesNotContain(marker, metadata.Key, StringComparison.Ordinal);
                    Assert.DoesNotContain(marker, metadata.Value, StringComparison.Ordinal);
                });
            Assert.All(
                status.Provenance,
                provenance =>
                {
                    Assert.DoesNotContain(marker, provenance.Key, StringComparison.Ordinal);
                    Assert.DoesNotContain(marker, provenance.Value, StringComparison.Ordinal);
                });
            Assert.DoesNotContain(marker, serialized, StringComparison.Ordinal);
        }
    }

    private static SubmittedDesignConversionRequest CreatePrivateSubmittedRequest()
    {
        var request = CreateSubmittedRequest();
        return request with
        {
            Html = "<main class=\"private-frame\" data-path=\"C:/Users/lubao/private.html\">exact_prompt design</main>",
            Css = ".private-frame{background:url('C:/Users/lubao/private.png');}",
            DesignTreePayload = request.DesignTreePayload with
            {
                Content = "{\"type\":\"FRAME\",\"raw\":\"raw-tree-private\",\"path\":\"C:/Users/lubao/tree.json\"}",
                Metadata = new Dictionary<string, string>
                {
                    ["raw_tree_payload"] = "raw-tree-private"
                }
            },
            OriginalTreePayload = request.OriginalTreePayload with
            {
                Content = "{\"type\":\"FRAME\",\"raw\":\"raw-tree-private-original\"}",
                Metadata = new Dictionary<string, string>
                {
                    ["raw_tree_payload"] = "raw-tree-private-original"
                }
            },
            Provenance = new Dictionary<string, string>
            {
                ["boundary"] = "thin-client-runtime-test",
                ["source_manifest_ref"] = "file:///C:/Users/lubao/private.fig",
                ["exact_prompt"] = "make the private button blue",
                ["external_provider_payload"] = "external-provider-payload"
            }
        };
    }

    private static SubmittedDesignConversionRequest CreateRuntimeCandidateEligiblePrivateSubmittedRequest()
    {
        var request = CreatePrivateSubmittedRequest();
        return request with
        {
            Screenshots =
            [
                request.Screenshots[0],
                new DesignRenderArtifact(
                    ArtifactId: "candidate-eligible-screenshot-mobile",
                    Kind: "screenshot",
                    Width: 390,
                    Height: 844,
                    Uri: "artifact://candidate-eligible-screenshot-mobile.png",
                    Sha256: "sha256:candidate-eligible-screenshot-mobile",
                    Metadata: new Dictionary<string, string>())
            ],
            DesignRenders =
            [
                request.DesignRenders[0],
                new DesignRenderArtifact(
                    ArtifactId: "candidate-eligible-render-mobile",
                    Kind: "design-render",
                    Width: 390,
                    Height: 844,
                    Uri: "artifact://candidate-eligible-render-mobile.png",
                    Sha256: "sha256:candidate-eligible-render-mobile",
                    Metadata: new Dictionary<string, string>())
            ],
            Budget = request.Budget with { MaxRenderQaRuns = 2 }
        };
    }

    private static SubmittedDesignConversionRequest
        CreateTeacherAuthoredRuntimeCandidateEligibleSubmittedRequest()
    {
        var request = CreateSubmittedRequest();
        return request with
        {
            Screenshots =
            [
                request.Screenshots[0],
                new DesignRenderArtifact(
                    ArtifactId: "teacher-authored-candidate-eligible-screenshot-mobile",
                    Kind: "screenshot",
                    Width: 390,
                    Height: 844,
                    Uri: "artifact://teacher-authored-candidate-eligible-screenshot-mobile.png",
                    Sha256: "sha256:teacher-authored-candidate-eligible-screenshot-mobile",
                    Metadata: new Dictionary<string, string>())
            ],
            DesignRenders =
            [
                request.DesignRenders[0],
                new DesignRenderArtifact(
                    ArtifactId: "teacher-authored-candidate-eligible-render-mobile",
                    Kind: "design-render",
                    Width: 390,
                    Height: 844,
                    Uri: "artifact://teacher-authored-candidate-eligible-render-mobile.png",
                    Sha256: "sha256:teacher-authored-candidate-eligible-render-mobile",
                    Metadata: new Dictionary<string, string>())
            ],
            Budget = request.Budget with
            {
                MaxRenderQaRuns = 2,
                MaxImprovementIterations = 1,
                MaxTeacherCalls = 2
            }
        };
    }

    private static DesignRenderArtifact WithDeviceScaleFactor(
        DesignRenderArtifact artifact,
        string deviceScaleFactor)
        => artifact with
        {
            Metadata = new Dictionary<string, string>
            {
                [DesignConversionRenderResolutionFactory.DeviceScaleFactorMetadataKey] = deviceScaleFactor
            }
        };

    private static SubmittedDesignConversionRequest CreateFourViewportPrivateSubmittedRequest()
    {
        var request = CreatePrivateSubmittedRequest();
        var renders = new[]
        {
            FourViewportRender("pass159-mobile", 375, 812),
            FourViewportRender("pass159-tablet", 768, 1024),
            FourViewportRender("pass159-desktop", 1440, 900),
            FourViewportRender("pass159-wide", 1920, 1080)
        };

        return request with
        {
            RequestId = "pass159-four-viewport-private-request",
            Html = "<main class=\"private-frame\" data-secret=\"PRIVATE_PASS159_SECRET\">exact_prompt design</main>",
            Css = ".private-frame{background:url('C:/Users/lubao/private-pass159.png');color:#123456;}",
            Screenshots = [],
            DesignRenders = renders,
            Budget = request.Budget with
            {
                MaxRenderQaRuns = 4
            },
            Provenance = new Dictionary<string, string>
            {
                ["boundary"] = "pass159-thin-client-runtime-test",
                ["source_manifest_ref"] = "file:///C:/Users/lubao/private-pass159.fig",
                ["exact_prompt"] = "make the private dashboard responsive",
                ["external_provider_payload"] = "external-provider-payload"
            }
        };
    }

    private static DesignRenderArtifact FourViewportRender(string id, int width, int height)
        => new(
            ArtifactId: id,
            Kind: "design-render",
            Width: width,
            Height: height,
            Uri: $"artifact://pass159/{id}.png",
            Sha256: $"sha256:{id}",
            Metadata: new Dictionary<string, string>());

    private static string CreateBudgetStoreTempRoot(string scope)
        => Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            $"thin-client-runtime-visual-cost-budget-{scope}");

    private static JsonDesignConversionPostEvaluationStateStore
        CreatePostEvaluationStateStore(string scope)
        => new(CreateBudgetStoreTempRoot("post-evaluation-state-" + scope));

    private static SubmittedDesignConversionRequest WithTrustedVisualCostBudget(
        SubmittedDesignConversionRequest request,
        string monthlyLimitUsd,
        decimal budgetUsd,
        string tier = "paid",
        string userRefSource = "platform-user-visual-cost-alpha",
        string tenantRefSource = "tenant-visual-cost-alpha")
    {
        var trustedUserRef = JsonlDesignConversionVisualCostLedger
            .BuildPseudonymousUserRef(userRefSource)
            .Value!;
        var tenantRef = JsonlDesignConversionVisualCostLedger
            .BuildPseudonymousUserRef(tenantRefSource)
            .Value!;
        var metadata = new Dictionary<string, string>(request.Budget.Metadata, StringComparer.Ordinal)
        {
            ["trusted_visual_cost_entitlement_bound"] = bool.TrueString,
            ["trusted_visual_cost_entitlement_authority"] = "platform_tenant_configuration",
            ["trusted_visual_cost_user_identity_authority"] = "verified_inbound_principal",
            ["trusted_visual_cost_user_ref"] = trustedUserRef,
            ["trusted_visual_cost_tenant_id_ref"] = tenantRef,
            ["server_assigned_design_tier"] = tier,
            ["server_assigned_monthly_visual_cost_limit_usd"] = monthlyLimitUsd,
            ["design_tier_authority"] = "platform_tenant_configuration",
            ["monthly_visual_cost_limit_authority"] = "platform_tenant_configuration",
            ["user_identity_authority"] = "verified_inbound_principal",
            ["trusted_visual_cost_period_kind"] = JsonlDesignConversionVisualCostLedger.UtcCalendarMonthPeriodKind,
            ["trusted_visual_cost_raw_user_exposed"] = bool.FalseString,
            ["trusted_visual_cost_reservation_store_wired"] = bool.TrueString
        };
        var provenance = new Dictionary<string, string>(request.Provenance, StringComparer.Ordinal)
        {
            ["trusted_visual_cost_entitlement_bound"] = bool.TrueString,
            ["trusted_visual_cost_entitlement_authority"] = "platform_tenant_configuration",
            ["trusted_visual_cost_entitlement_user_identity_authority"] = "verified_inbound_principal",
            ["trusted_visual_cost_entitlement_user_ref"] = trustedUserRef,
            ["trusted_visual_cost_entitlement_tenant_id_ref"] = tenantRef,
            ["trusted_visual_cost_entitlement_design_tier"] = tier,
            ["trusted_visual_cost_entitlement_monthly_limit_usd"] = monthlyLimitUsd,
            ["trusted_visual_cost_entitlement_period_kind"] = JsonlDesignConversionVisualCostLedger.UtcCalendarMonthPeriodKind,
            ["trusted_visual_cost_entitlement_raw_user_exposed"] = bool.FalseString,
            ["trusted_visual_cost_reservation_store_wired"] = bool.TrueString
        };

        return request with
        {
            Budget = request.Budget with
            {
                UserId = "server-owned-redacted-user",
                Tier = tier,
                BudgetUsd = budgetUsd,
                Metadata = metadata
            },
            Provenance = provenance
        };
    }

    private static DesignConversionVisualCostBudgetPeriodQuery BuildTrustedBudgetPeriodQuery(
        SubmittedDesignConversionRequest request)
    {
        var compositeIdentity = string.Join(
            "|",
            "tenant",
            request.Budget.Metadata["trusted_visual_cost_tenant_id_ref"],
            "user",
            request.Budget.Metadata["trusted_visual_cost_user_ref"]);
        var storeUserRef = JsonlDesignConversionVisualCostLedger.BuildPseudonymousUserRef(compositeIdentity);
        Assert.True(storeUserRef.IsSuccess, storeUserRef.ErrorMessage);
        var period = JsonlDesignConversionVisualCostLedger.BuildUtcCalendarMonthPeriod(DateTimeOffset.UnixEpoch);
        return new DesignConversionVisualCostBudgetPeriodQuery(
            storeUserRef.Value!,
            period.StartUtc,
            period.EndExclusiveUtc);
    }

    private sealed class RecordingEvaluator(IDesignConversionThinClientJobEvaluator inner) : IDesignConversionThinClientJobEvaluator
    {
        public List<DesignConversionInternalEvaluateRequest> Requests { get; } = [];
        public List<OperationResult<DesignConversionInternalEvaluationResult>> Responses { get; } = [];

        public async Task<OperationResult<DesignConversionInternalEvaluationResult>> EvaluateAsync(
            DesignConversionInternalEvaluateRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            var response = await inner.EvaluateAsync(request, cancellationToken);
            Responses.Add(response);
            return response;
        }
    }

    private sealed class ThinRuntimeLocalIndependentEvaluationClient
        : ILegalTeacherEvaluationInferenceClient
    {
        internal const string ModelId = "qwen-thin-runtime-evaluator-test:9b";
        internal static readonly string ManifestDigest = new('c', 64);
        internal static readonly string ModelLayerDigest = new('d', 64);
        private int _callCount;

        internal int CallCount => Volatile.Read(ref _callCount);
        internal bool Disposed { get; private set; }

        public Task<OperationResult<TeacherEvaluationInferenceResult>>
            EvaluateAsync(
                string prompt,
                ILegalTeacherCallPermit callPermit,
                CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var permit = callPermit.TryIssue();
            if (!permit.IsSuccess)
            {
                return Task.FromResult(
                    permit.MapError<TeacherEvaluationInferenceResult>());
            }

            var call = Interlocked.Increment(ref _callCount);
            using var document = JsonDocument.Parse(prompt);
            var root = document.RootElement;
            var output = JsonSerializer.Serialize(new
            {
                schema = DesignConversionLocalIndependentPreferenceEvaluator
                    .ResultSchema,
                scorer_request_sha256 = root
                    .GetProperty("scorer_request_sha256").GetString(),
                generated_evaluation_identity = root
                    .GetProperty("generated_evaluation_identity").GetString(),
                incumbent_evaluation_identity = root
                    .GetProperty("incumbent_evaluation_identity").GetString(),
                preferred_direction =
                    DesignConversionIndependentPreferenceEvidenceContract
                        .GeneratedSide
            });
            var provenance = new TrainingTeacherProvenance(
                TrainingTeacherSourceKind.ExternalTeacherOutput,
                OpenWeightTeacherFamily.Qwen,
                TrainingTeacherAccessMode.LocalHostedOpenWeight,
                TrainingTeacherHostingProvider.Ollama,
                ModelId,
                ManifestDigest,
                TrainingTeacherModelLicense.Apache20,
                ProviderTermsChecked: false,
                ProviderTermsAllowTraining: false,
                ClosedProprietaryModel: false,
                TeacherOutputUsedForTraining: false,
                RequestId: "thin-runtime-evaluation-request-" + call,
                InvocationCount: 1)
            {
                ModelArtifactDigestKind =
                    LegalTrainingTeacherAdmissionAuthority
                        .OllamaModelLayerSha256DigestKind,
                ModelArtifactDigest = ModelLayerDigest
            };
            return Task.FromResult(
                OperationResult<TeacherEvaluationInferenceResult>.Ok(
                    new TeacherEvaluationInferenceResult(output, provenance)
                    {
                        InferenceProfile =
                            LocalQwenOllamaInferenceProfileAuthority.Evaluation
                    }));
        }

        public void Dispose() => Disposed = true;
    }

    private sealed class UnavailableThinRuntimeLocalIndependentEvaluationClient
        : ILegalTeacherEvaluationInferenceClient
    {
        internal const string ErrorCode =
            "THIN_RUNTIME_LOCAL_INDEPENDENT_EVALUATOR_UNAVAILABLE";
        internal const string PrivateFailureMarker =
            "PRIVATE_THIN_RUNTIME_EVALUATOR_FAILURE_OUTPUT";
        private int _callCount;
        private int _disposed;

        internal int CallCount => Volatile.Read(ref _callCount);
        internal bool Disposed => Volatile.Read(ref _disposed) == 1;

        public Task<OperationResult<TeacherEvaluationInferenceResult>>
            EvaluateAsync(
                string prompt,
                ILegalTeacherCallPermit callPermit,
                CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var permit = callPermit.TryIssue();
            if (!permit.IsSuccess)
            {
                return Task.FromResult(
                    permit.MapError<TeacherEvaluationInferenceResult>());
            }

            Interlocked.Increment(ref _callCount);
            return Task.FromResult(
                OperationResult<TeacherEvaluationInferenceResult>.Fail(
                    PrivateFailureMarker,
                    ErrorCode));
        }

        public void Dispose() => Interlocked.Exchange(ref _disposed, 1);
    }

    private sealed class MeasuredRuntimeCandidateEvaluator : IDesignConversionThinClientJobEvaluator
    {
        public Task<OperationResult<DesignConversionInternalEvaluationResult>> EvaluateAsync(
            DesignConversionInternalEvaluateRequest request,
            CancellationToken cancellationToken = default)
            => Task.FromResult(OperationResult<DesignConversionInternalEvaluationResult>.Ok(
                CreateMeasuredRuntimeDpoEvaluation(
                    canPersistTrainingCandidate: true,
                    jobId: request.JobId,
                    requestId: request.RequestId,
                    outputId: "runtime-chosen-output",
                    candidateId: "runtime-chosen-candidate",
                    verdictId: "runtime-chosen-verdict",
                    score: 0.82,
                     passed: true,
                     htmlSuffix: "chosen")));
    }

    private sealed class TeacherAuthoredMeasuredRuntimeCandidateEvaluator(
        Func<int, IReadOnlyDictionary<string, string>?>? routeBindingByEvaluationCall = null)
        : IDesignConversionThinClientJobEvaluator,
          IDesignConversionDpoPolicyPairRuntimeEvaluator
    {
        private int _callCount;

        public List<DesignConversionDpoPolicyRuntimePair> RuntimePairs { get; } = [];

        public Task<OperationResult<DesignConversionInternalEvaluationResult>>
            EvaluateAsync(
                DesignConversionInternalEvaluateRequest request,
                CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var callCount = Interlocked.Increment(ref _callCount);
            var generatedCandidateId = request.RequestId.StartsWith(
                    "teacher-authored-request:",
                    StringComparison.Ordinal)
                ? "teacher-authored:" +
                  request.RequestId["teacher-authored-request:".Length..]
                : "runtime-incumbent-candidate";
            var generated = CreateMeasuredRuntimeDpoEvaluation(
                canPersistTrainingCandidate: callCount > 1,
                jobId: request.JobId,
                requestId: request.RequestId,
                outputId: $"runtime-teacher-authored-output-{callCount}",
                candidateId: generatedCandidateId,
                verdictId: $"runtime-teacher-authored-verdict-{callCount}",
                score: callCount > 1 ? 0.82 : 0.46,
                passed: callCount > 1,
                htmlSuffix: callCount > 1 ? "teacher-authored" : "incumbent");
            var provenance = new Dictionary<string, string>(request.Provenance)
            {
                ["internal_service_job_id"] = request.JobId
            };
            foreach (var pair in routeBindingByEvaluationCall?.Invoke(callCount)
                                 ?? new Dictionary<string, string>())
            {
                provenance[pair.Key] = pair.Value;
            }
            var submitted = generated.SubmittedRequest with
            {
                RequestId = request.RequestId,
                SourceKind = request.SourceKind,
                Html = request.Html,
                Css = request.Css,
                DesignTreePayload = request.DesignTreePayload,
                OriginalTreePayload = request.OriginalTreePayload,
                Screenshots = request.Screenshots,
                DesignRenders = request.DesignRenders,
                Budget = request.Budget,
                Provenance = provenance
            };
            var candidate = generated.RuntimeFlowResult.Candidate with
            {
                CandidateId = generatedCandidateId,
                RequestId = request.RequestId,
                Html = request.Html,
                Css = request.Css
            };
            var output = generated.RuntimeFlowResult.Output with
            {
                CandidateId = generatedCandidateId,
                Html = request.Html,
                Css = request.Css
            };
            var evaluation = generated with
            {
                SubmittedRequest = submitted,
                QaResults = generated.RuntimeFlowResult.Output.QaResults,
                VisualQaBoundaryTrace = generated.VisualQaBoundaryTrace with
                {
                    TierPolicy = generated.VisualQaBoundaryTrace.TierPolicy with
                    {
                        ShouldContinue = callCount == 1
                    }
                },
                RuntimeFlowResult = generated.RuntimeFlowResult with
                {
                    Request = submitted,
                    Candidate = candidate,
                    Output = output,
                    ImproverOutput = generated.RuntimeFlowResult.ImproverOutput with
                    {
                        Candidate = candidate,
                        Output = output
                    }
                }
            };

            return Task.FromResult(
                OperationResult<DesignConversionInternalEvaluationResult>.Ok(
                    evaluation));
        }

        Task<OperationResult<DesignConversionInternalEvaluationResult>>
            IDesignConversionDpoPolicyPairRuntimeEvaluator
                .ApplyDpoPolicyRuntimePairAsync(
                    DesignConversionInternalEvaluationResult evaluation,
                    DesignConversionDpoPolicyRuntimePair pair,
                    CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            RuntimePairs.Add(pair);
            return Task.FromResult(
                OperationResult<DesignConversionInternalEvaluationResult>.Ok(
                    evaluation));
        }
    }

    private sealed class CancellationIgnoringGateEvaluator : IDesignConversionThinClientJobEvaluator
    {
        private readonly TaskCompletionSource<bool> _release =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource<bool> Entered { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public void Release()
            => _release.TrySetResult(true);

        public async Task<OperationResult<DesignConversionInternalEvaluationResult>> EvaluateAsync(
            DesignConversionInternalEvaluateRequest request,
            CancellationToken cancellationToken = default)
        {
            Entered.TrySetResult(true);
            await _release.Task;
            return OperationResult<DesignConversionInternalEvaluationResult>.Ok(
                CreateMeasuredRuntimeDpoEvaluation(
                    canPersistTrainingCandidate: true,
                    jobId: request.JobId,
                    requestId: request.RequestId,
                    outputId: "runtime-caller-cancellation-chosen-output",
                    candidateId: "runtime-caller-cancellation-chosen-candidate",
                    verdictId: "runtime-caller-cancellation-chosen-verdict",
                    score: 0.82,
                    passed: true,
                    htmlSuffix: "caller-cancellation-chosen"));
        }
    }

    private sealed class BlockingEvaluator(IDesignConversionThinClientJobEvaluator inner) : IDesignConversionThinClientJobEvaluator
    {
        private readonly TaskCompletionSource<bool> _release = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public List<DesignConversionInternalEvaluateRequest> Requests { get; } = [];
        public List<OperationResult<DesignConversionInternalEvaluationResult>> Responses { get; } = [];
        public TaskCompletionSource<DesignConversionInternalEvaluateRequest> Invoked { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public void Release()
            => _release.TrySetResult(true);

        public async Task<OperationResult<DesignConversionInternalEvaluationResult>> EvaluateAsync(
            DesignConversionInternalEvaluateRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            Invoked.TrySetResult(request);
            await _release.Task.WaitAsync(cancellationToken);
            var response = await inner.EvaluateAsync(request, cancellationToken);
            Responses.Add(response);
            return response;
        }
    }

    private sealed class PostEvaluationOwnershipBarrier
    {
        private readonly TaskCompletionSource<bool> _release =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource<bool> Entered { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public void Release()
            => _release.TrySetResult(true);

        public async Task WaitAsync()
        {
            Entered.TrySetResult(true);
            await _release.Task;
        }
    }

    private sealed class ManualFinalizationDeadline
    {
        private readonly TaskCompletionSource<bool> _expired =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TimeSpan? RequestedTimeout { get; private set; }

        public Task WaitAsync(TimeSpan timeout, CancellationToken cancellationToken)
        {
            RequestedTimeout = timeout;
            return _expired.Task;
        }

        public void Expire()
            => _expired.TrySetResult(true);
    }

    private sealed class EventRecordingEvaluator(
        IDesignConversionThinClientJobEvaluator inner,
        List<string> events) : IDesignConversionThinClientJobEvaluator
    {
        public List<DesignConversionInternalEvaluateRequest> Requests { get; } = [];
        public List<OperationResult<DesignConversionInternalEvaluationResult>> Responses { get; } = [];

        public async Task<OperationResult<DesignConversionInternalEvaluationResult>> EvaluateAsync(
            DesignConversionInternalEvaluateRequest request,
            CancellationToken cancellationToken = default)
        {
            events.Add("evaluate");
            Requests.Add(request);
            var response = await inner.EvaluateAsync(request, cancellationToken);
            Responses.Add(response);
            return response;
        }
    }

    private sealed class ScriptedRuntimeRejectedEvaluationSource(
        Func<
            DesignConversionRuntimeRejectedEvaluationLookup,
            CancellationToken,
            Task<OperationResult<DesignConversionRuntimeRejectedEvaluationReference?>>> resolve)
        : IDesignConversionRuntimeRejectedEvaluationSource
    {
        public int CallCount { get; private set; }

        public DesignConversionRuntimeRejectedEvaluationLookup? Lookup { get; private set; }

        public DesignConversionRuntimeRejectedEvaluationReference? ResolvedReference { get; private set; }

        public CancellationToken CapturedCancellationToken { get; private set; }

        public TaskCompletionSource<bool> Returned { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public async Task<OperationResult<DesignConversionRuntimeRejectedEvaluationReference?>>
            ResolveMeasuredRejectedEvaluationAsync(
                DesignConversionRuntimeRejectedEvaluationLookup lookup,
                CancellationToken cancellationToken = default)
        {
            CallCount++;
            Lookup = lookup;
            CapturedCancellationToken = cancellationToken;
            try
            {
                var result = await resolve(lookup, cancellationToken);
                ResolvedReference = result?.Value;
                return result!;
            }
            finally
            {
                Returned.TrySetResult(true);
            }
        }
    }

    private sealed class IncompleteAccountingEvaluator(
        IDesignConversionThinClientJobEvaluator inner) : IDesignConversionThinClientJobEvaluator
    {
        public async Task<OperationResult<DesignConversionInternalEvaluationResult>> EvaluateAsync(
            DesignConversionInternalEvaluateRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await inner.EvaluateAsync(request, cancellationToken);
            if (!result.IsSuccess || result.Value is null)
                return result;

            return OperationResult<DesignConversionInternalEvaluationResult>.Ok(
                result.Value with
                {
                    VisualCostAccounting = DesignConversionVisualCostAccounting.Unavailable
                });
        }
    }

    private sealed class CancellingAfterInvocationEvaluator(
        CancellationTokenSource cancellationSource) : IDesignConversionThinClientJobEvaluator
    {
        public Task<OperationResult<DesignConversionInternalEvaluationResult>> EvaluateAsync(
            DesignConversionInternalEvaluateRequest request,
            CancellationToken cancellationToken = default)
        {
            cancellationSource.Cancel();
            throw new OperationCanceledException(cancellationToken);
        }
    }

    private sealed class ThrowingPostEvaluationStateStore(
        bool throwRead,
        bool throwWrite)
        : IDesignConversionPostEvaluationStateStore
    {
        private int _writeCalls;
        private DesignConversionPostEvaluationState? _state;

        public Task<OperationResult<DesignConversionPostEvaluationState>> WriteAsync(
            DesignConversionPostEvaluationState state,
            CancellationToken cancellationToken = default)
        {
            var writeCall = Interlocked.Increment(ref _writeCalls);
            if (throwWrite && writeCall > 1)
            {
                return Task.FromException<
                    OperationResult<DesignConversionPostEvaluationState>>(
                    new InvalidOperationException(
                        "injected-post-evaluation-write-failure"));
            }

            _state = state;
            return Task.FromResult(
                OperationResult<DesignConversionPostEvaluationState>.Ok(state));
        }

        public Task<OperationResult<DesignConversionPostEvaluationState?>> ReadAsync(
            string stateRef,
            CancellationToken cancellationToken = default)
            => throwRead
                ? Task.FromException<
                    OperationResult<DesignConversionPostEvaluationState?>>(
                    new InvalidOperationException(
                        "injected-post-evaluation-read-failure"))
                : Task.FromResult(
                    OperationResult<DesignConversionPostEvaluationState?>.Ok(_state));

        public Task<OperationResult<IReadOnlyList<DesignConversionPostEvaluationState>>> ListAsync(
            CancellationToken cancellationToken = default)
            => Task.FromResult(
                OperationResult<IReadOnlyList<DesignConversionPostEvaluationState>>.Ok(
                    _state is null
                        ? Array.Empty<DesignConversionPostEvaluationState>()
                        : [_state]));
    }

    private sealed class EventRecordingVisualCostBudgetStore(
        JsonlDesignConversionVisualCostBudgetStore inner,
        List<string> events,
        Func<
            DesignConversionVisualCostSettlementRequest,
            CancellationToken,
            Task<OperationResult<DesignConversionVisualCostSettlementResult>>>? settleOverride = null,
        Func<
            DesignConversionVisualCostReleaseRequest,
            CancellationToken,
            Task<OperationResult<DesignConversionVisualCostReleaseResult>>>? releaseOverride = null,
        Func<
            DesignConversionVisualCostBudgetPeriodQuery,
            CancellationToken,
            Task<OperationResult<DesignConversionVisualCostBudgetPeriodState>>>? readOverride = null,
        Func<
            DesignConversionVisualCostReservationRequest,
            CancellationToken,
            Task<OperationResult<DesignConversionVisualCostReservationResult>>>? reserveOverride = null)
        : IDesignConversionVisualCostBudgetStore,
          IDesignConversionPostEvaluationStateRootProvider
    {
        private int _releaseCalls;
        private int _settlementCalls;

        public JsonlDesignConversionVisualCostBudgetStore Inner { get; } = inner;

        public string PostEvaluationStateRootDirectory =>
            Path.Combine(
                Inner.RootDirectory,
                JsonDesignConversionPostEvaluationStateStore.DirectoryName);

        public int ReleaseCalls => Volatile.Read(ref _releaseCalls);

        public int SettlementCalls => Volatile.Read(ref _settlementCalls);

        public TaskCompletionSource<bool> ReleaseObserved { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public async Task<OperationResult<DesignConversionVisualCostReservationResult>> TryReserveAsync(
            DesignConversionVisualCostReservationRequest request,
            CancellationToken cancellationToken = default)
        {
            events.Add("reserve");
            return reserveOverride is null
                ? await Inner.TryReserveAsync(request, cancellationToken)
                : await reserveOverride(request, cancellationToken);
        }

        public async Task<OperationResult<DesignConversionVisualCostSettlementResult>> SettleAsync(
            DesignConversionVisualCostSettlementRequest request,
            CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref _settlementCalls);
            events.Add("settle:" + request.ActualCostUsd.ToString("0.####", CultureInfo.InvariantCulture));
            return settleOverride is null
                ? await Inner.SettleAsync(request, cancellationToken)
                : await settleOverride(request, cancellationToken);
        }

        public async Task<OperationResult<DesignConversionVisualCostReleaseResult>> ReleaseAsync(
            DesignConversionVisualCostReleaseRequest request,
            CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref _releaseCalls);
            events.Add("release");
            try
            {
                return releaseOverride is null
                    ? await Inner.ReleaseAsync(request, cancellationToken)
                    : await releaseOverride(request, cancellationToken);
            }
            finally
            {
                ReleaseObserved.TrySetResult(true);
            }
        }

        public Task<OperationResult<DesignConversionVisualCostBudgetPeriodState>> ReadPeriodAsync(
            DesignConversionVisualCostBudgetPeriodQuery query,
            CancellationToken cancellationToken = default)
            => readOverride is null
                ? Inner.ReadPeriodAsync(query, cancellationToken)
                : readOverride(query, cancellationToken);
    }

    private sealed class RecordingVisualCostLedger(
        IDesignConversionVisualCostLedger inner,
        Func<
            DesignConversionVisualCostLedgerRecord,
            CancellationToken,
            Task<OperationResult<DesignConversionVisualCostLedgerRecord>>>? appendOverride = null)
        : IDesignConversionVisualCostLedger
    {
        public List<DesignConversionVisualCostLedgerRecord> AttemptedRecords { get; } = [];

        public async Task<OperationResult<DesignConversionVisualCostLedgerRecord>> AppendRecordAsync(
            DesignConversionVisualCostLedgerRecord record,
            CancellationToken cancellationToken = default)
        {
            AttemptedRecords.Add(record);
            return appendOverride is null
                ? await inner.AppendRecordAsync(record, cancellationToken)
                : await appendOverride(record, cancellationToken);
        }

        public Task<OperationResult<IReadOnlyList<DesignConversionVisualCostLedgerRecord>>> ListRecordsAsync(
            CancellationToken cancellationToken = default)
            => inner.ListRecordsAsync(cancellationToken);

        public Task<OperationResult<DesignConversionVisualCostLedgerPeriodAggregate>> AggregatePeriodAsync(
            string userRef,
            DateTimeOffset periodStartUtc,
            DateTimeOffset periodEndExclusiveUtc,
            CancellationToken cancellationToken = default)
            => inner.AggregatePeriodAsync(userRef, periodStartUtc, periodEndExclusiveUtc, cancellationToken);
    }

    private sealed class RecordingLocalArtifactDb(
        IDesignConversionLocalArtifactDb inner,
        Func<
            DesignConversionLocalArtifactDbRecord,
            CancellationToken,
            Task<OperationResult<DesignConversionLocalArtifactDbRecord>>>? appendOverride = null,
        Func<
            DesignConversionLocalArtifactDbQuery?,
            CancellationToken,
            Task<OperationResult<IReadOnlyList<DesignConversionLocalArtifactDbRecord>>>>? listOverride = null)
        : IDesignConversionLocalArtifactDb,
          IDesignConversionPostEvaluationStateRootProvider
    {
        private int _listCalls;

        public List<DesignConversionLocalArtifactDbRecord> AttemptedRecords { get; } = [];

        public string PostEvaluationStateRootDirectory =>
            inner is JsonlDesignConversionLocalArtifactDb json
                ? Path.Combine(
                    json.AllowedArtifactRootDirectory,
                    JsonDesignConversionPostEvaluationStateStore.DirectoryName)
                : Path.Combine(
                    Path.GetTempPath(),
                    "llm-mvp-core-post-evaluation-test-state",
                    System.Runtime.CompilerServices.RuntimeHelpers
                        .GetHashCode(inner)
                        .ToString(CultureInfo.InvariantCulture));

        public int ListCalls => Volatile.Read(ref _listCalls);

        public async Task<OperationResult<DesignConversionLocalArtifactDbRecord>> AppendRecordAsync(
            DesignConversionLocalArtifactDbRecord record,
            CancellationToken ct = default)
        {
            AttemptedRecords.Add(record);
            return appendOverride is null
                ? await inner.AppendRecordAsync(record, ct)
                : await appendOverride(record, ct);
        }

        public Task<OperationResult<IReadOnlyList<DesignConversionLocalArtifactDbRecord>>> ListRecordsAsync(
            DesignConversionLocalArtifactDbQuery? query = null,
            CancellationToken ct = default)
        {
            Interlocked.Increment(ref _listCalls);
            return listOverride is null
                ? inner.ListRecordsAsync(query, ct)
                : listOverride(query, ct);
        }

        public Task<OperationResult<IReadOnlyList<DesignConversionDpoTrainingRecord>>> QueryApprovedTrainingCandidatesAsync(
            DesignConversionLocalArtifactDbQuery? query = null,
            CancellationToken ct = default)
            => inner.QueryApprovedTrainingCandidatesAsync(query, ct);

        public Task<OperationResult<DesignConversionApprovedDpoHandoffArtifact>> ExportApprovedRuntimeDpoCandidatesAsync(
            DesignConversionLocalArtifactDbQuery? query = null,
            DateTimeOffset? exportedAtUtc = null,
            CancellationToken ct = default)
            => inner.ExportApprovedRuntimeDpoCandidatesAsync(query, exportedAtUtc, ct);

        public Task<OperationResult<DesignConversionTrainingIntakeRecord>> CreateTrainingIntakeAsync(
            DesignConversionApprovedDpoHandoffArtifact handoff,
            string requestedByRef,
            string reason,
            DateTimeOffset? requestedAtUtc = null,
            CancellationToken ct = default,
            IReadOnlyDictionary<string, string>? admissionBoundaryMetadata = null)
            => inner.CreateTrainingIntakeAsync(handoff, requestedByRef, reason, requestedAtUtc, ct, admissionBoundaryMetadata);

        public Task<OperationResult<IReadOnlyList<DesignConversionTrainingIntakeRecord>>> ListTrainingIntakeRequestsAsync(
            CancellationToken ct = default)
            => inner.ListTrainingIntakeRequestsAsync(ct);

        public Task<OperationResult<DesignConversionTrainingIntakePreflightResult>> RunTrainingIntakePreflightAsync(
            string intakeId,
            CancellationToken ct = default)
            => inner.RunTrainingIntakePreflightAsync(intakeId, ct);

        public Task<OperationResult<DesignConversionTrainingIntakeRunResult>> RunTrainingIntakeAsync(
            string intakeId,
            DesignConversionTrainingPipelineOptions? options = null,
            CancellationToken ct = default,
            IReadOnlyDictionary<string, string>? admissionBoundaryMetadata = null)
            => inner.RunTrainingIntakeAsync(intakeId, options, ct, admissionBoundaryMetadata);

        public Task<OperationResult<DesignConversionTrainingIntakeLocalPromotionResult>> RunTrainingIntakeAndPromoteLocalAsync(
            string intakeId,
            string promotionOutputDirectory,
            DesignConversionTrainingPipelineOptions? options = null,
            CancellationToken ct = default)
            => inner.RunTrainingIntakeAndPromoteLocalAsync(intakeId, promotionOutputDirectory, options, ct);

        public Task<OperationResult<DesignConversionTrainingIntakeLocalPromotionRollbackResult>> RollbackTrainingIntakeLocalPromotionAsync(
            string promotedLocalModelRef,
            string requestedByRef,
            string reason,
            DateTimeOffset? requestedAtUtc = null,
            CancellationToken ct = default)
            => inner.RollbackTrainingIntakeLocalPromotionAsync(promotedLocalModelRef, requestedByRef, reason, requestedAtUtc, ct);

        public Task<OperationResult<DesignConversionDpoCandidateApprovalResult>> ApproveRuntimeCreatedDpoCandidateAsync(
            string localArtifactRecordId,
            string candidateOrTrainingRecordId,
            string reviewerRef,
            string reason,
            DateTimeOffset? approvedAtUtc = null,
            CancellationToken ct = default)
            => inner.ApproveRuntimeCreatedDpoCandidateAsync(
                localArtifactRecordId,
                candidateOrTrainingRecordId,
                reviewerRef,
                reason,
                approvedAtUtc,
                ct);

        public Task<OperationResult<DesignConversionDpoDisplayResult>> QueryDisplayRowsAsync(
            DesignConversionDpoDisplayQuery? query = null,
            CancellationToken ct = default)
            => inner.QueryDisplayRowsAsync(query, ct);
    }

    private sealed class ReceiptVisualJudge(
        IReadOnlyDictionary<string, string> providerReceipt,
        IReadOnlyDictionary<string, string> resultReceipt,
        decimal estimatedCostUsd) : IDesignConversionVisualJudge
    {
        public int InvocationCount { get; private set; }

        public Task<OperationResult<DesignConversionVisualJudgeResult>> JudgeAsync(
            DesignConversionVisualJudgeRequest request,
            CancellationToken cancellationToken = default)
        {
            InvocationCount++;
            var evidenceRefs = new[]
            {
                $"visual-judge-request:{request.JudgeRequestId}",
                "receipt_visual_judge_test_double"
            };
            var verdict = new VisualVerdict(
                VerdictId: $"{request.JudgeRequestId}:receipt-verdict",
                Score: 0.93,
                Defects: [],
                EvidenceRefs: evidenceRefs,
                Passed: true);
            var providerMetadata = new Dictionary<string, string>(providerReceipt, StringComparer.Ordinal)
            {
                ["configured"] = bool.TrueString,
                ["provider"] = "receipt-visual-judge",
                ["network_call_executed"] = bool.TrueString,
                ["external_ai_visual_judge_executed"] = bool.TrueString,
                ["visual_ai_executed"] = bool.TrueString,
                ["cost_source"] = DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostSourceFixedPerRequest,
                ["untrusted_provider_extra"] = "must_not_be_persisted"
            };
            var resultMetadata = new Dictionary<string, string>(resultReceipt, StringComparer.Ordinal)
            {
                ["status"] = ExternalHttpDesignConversionVisualJudge.StatusOk,
                ["external_ai_visual_judge_executed"] = bool.TrueString,
                ["visual_ai_executed"] = bool.TrueString,
                ["network_call_executed"] = bool.TrueString,
                ["cost_source"] = DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostSourceFixedPerRequest,
                ["untrusted_result_extra"] = "must_not_be_persisted"
            };

            return Task.FromResult(OperationResult<DesignConversionVisualJudgeResult>.Ok(new DesignConversionVisualJudgeResult(
                JudgeResultId: $"{request.JudgeRequestId}:receipt-result",
                JudgeRequestId: request.JudgeRequestId,
                Status: ExternalHttpDesignConversionVisualJudge.StatusOk,
                IsBlocked: false,
                Verdict: verdict,
                Provider: new DesignConversionVisualJudgeProviderMetadata(
                    ProviderName: "receipt-visual-judge",
                    EndpointHost: "receipt.visual.test",
                    ModelId: "receipt-model-v1",
                    ExternalCallExecuted: true,
                    Metadata: providerMetadata),
                EstimatedCostUsd: estimatedCostUsd,
                TokenMetadata: new Dictionary<string, double>(),
                EvidenceRefs: evidenceRefs,
                Metadata: resultMetadata)));
        }
    }

    private sealed class ScriptedCostVisualJudge(params VisualJudgeCostScript[] scripts) : IDesignConversionVisualJudge
    {
        public List<DesignConversionVisualJudgeRequest> Requests { get; } = [];

        public Task<OperationResult<DesignConversionVisualJudgeResult>> JudgeAsync(
            DesignConversionVisualJudgeRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            var script = scripts[Math.Min(Requests.Count - 1, scripts.Length - 1)];
            var passed = script.Score >= 0.95;
            var evidenceRefs = new[] { $"visual-judge:{request.JudgeRequestId}:scripted-cost" };
            var costUsd = script.CostUsd.ToString("0.####", CultureInfo.InvariantCulture);
            var serverOwnedReceipt = BuildServerOwnedReceipt(
                attemptCount: "1",
                costPerAttemptUsd: costUsd,
                totalCostUsd: costUsd);
            var verdict = new VisualVerdict(
                VerdictId: $"{request.JudgeRequestId}:scripted-cost-verdict",
                Score: script.Score,
                Defects: passed
                    ? []
                    :
                    [
                        new VisualDefect(
                            DefectId: $"{request.JudgeRequestId}:scripted-cost-defect",
                            Category: "quality",
                            Severity: "minor",
                            Description: "Scripted visual score below threshold for loop cost accounting.",
                            X: 0,
                            Y: 0,
                            Width: 1,
                            Height: 1,
                            EvidenceImageIds: [request.ExpectedScreenshot.ImageId, request.RenderedImage.ImageId])
                    ],
                EvidenceRefs: evidenceRefs,
                Passed: passed);
            var providerMetadata = new Dictionary<string, string>(serverOwnedReceipt, StringComparer.Ordinal)
            {
                ["provider"] = "scripted-cost-visual-judge",
                ["network_call_executed"] = bool.TrueString,
                ["external_ai_visual_judge_executed"] = bool.TrueString,
                ["visual_ai_executed"] = bool.TrueString,
                ["cost_source"] = DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostSourceFixedPerRequest
            };
            var resultMetadata = new Dictionary<string, string>(serverOwnedReceipt, StringComparer.Ordinal)
            {
                ["status"] = ExternalHttpDesignConversionVisualJudge.StatusOk,
                ["network_call_executed"] = bool.TrueString,
                ["external_ai_visual_judge_executed"] = bool.TrueString,
                ["visual_ai_executed"] = bool.TrueString,
                ["cost_source"] = DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostSourceFixedPerRequest
            };

            return Task.FromResult(OperationResult<DesignConversionVisualJudgeResult>.Ok(new DesignConversionVisualJudgeResult(
                JudgeResultId: $"{request.JudgeRequestId}:scripted-cost-result",
                JudgeRequestId: request.JudgeRequestId,
                Status: ExternalHttpDesignConversionVisualJudge.StatusOk,
                IsBlocked: false,
                Verdict: verdict,
                Provider: new DesignConversionVisualJudgeProviderMetadata(
                    ProviderName: "scripted-cost-visual-judge",
                    EndpointHost: "scripted.visual.test",
                    ModelId: "scripted-cost-v1",
                    ExternalCallExecuted: true,
                    Metadata: providerMetadata),
                EstimatedCostUsd: script.CostUsd,
                TokenMetadata: new Dictionary<string, double>(),
                EvidenceRefs: evidenceRefs,
                Metadata: resultMetadata)));
        }
    }

    private sealed record VisualJudgeCostScript(double Score, decimal CostUsd);

    private sealed class SingleCandidateStrategy : IDesignConversionImprovementCandidateStrategy
    {
        public List<DesignConversionCandidateUpdateRequest> Requests { get; } = [];

        public Task<OperationResult<DesignConversionImprovementCandidate>> CreateNextCandidateAsync(
            DesignConversionCandidateUpdateRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            return Task.FromResult(OperationResult<DesignConversionImprovementCandidate>.Ok(
                new DesignConversionImprovementCandidate(
                    CandidateId: $"thin-loop-cost-candidate-{request.CompletedAttemptNumber + 1}",
                    RequestId: $"{request.CurrentRequest.RequestId}-candidate-{request.CompletedAttemptNumber + 1}",
                    Html: $"{request.CurrentRequest.Html}<section data-loop-cost=\"candidate-{request.CompletedAttemptNumber + 1}\"></section>",
                    Css: $"{request.CurrentRequest.Css}.loop-cost-{request.CompletedAttemptNumber + 1}{{outline:0;}}",
                    ChangeReasons: ["scripted loop cost accounting candidate"],
                    EvidenceRefs: [$"candidate:thin-loop-cost:{request.CompletedAttemptNumber + 1}"],
                    Metadata: new Dictionary<string, string>
                    {
                        ["candidate_strategy"] = nameof(SingleCandidateStrategy)
                    })));
        }
    }

    private sealed class FailingOnCallEvaluator(
        IDesignConversionThinClientJobEvaluator inner,
        int failOnCall) : IDesignConversionThinClientJobEvaluator
    {
        private int _callCount;

        public Task<OperationResult<DesignConversionInternalEvaluationResult>> EvaluateAsync(
            DesignConversionInternalEvaluateRequest request,
            CancellationToken cancellationToken = default)
        {
            _callCount++;
            return _callCount == failOnCall
                ? Task.FromResult(OperationResult<DesignConversionInternalEvaluationResult>.Fail(
                    "Scripted evaluation failure for loop cost accounting.",
                    "SCRIPTED_LOOP_COST_EVALUATION_FAILED"))
                : inner.EvaluateAsync(request, cancellationToken);
        }
    }

    private sealed class FailingEvaluator(string message, string code) : IDesignConversionThinClientJobEvaluator
    {
        public Task<OperationResult<DesignConversionInternalEvaluationResult>> EvaluateAsync(
            DesignConversionInternalEvaluateRequest request,
            CancellationToken cancellationToken = default)
            => Task.FromResult(OperationResult<DesignConversionInternalEvaluationResult>.Fail(message, code));
    }

    private sealed class FailingVisualCostLedger(string message, string code) : IDesignConversionVisualCostLedger
    {
        public List<DesignConversionVisualCostLedgerRecord> AttemptedRecords { get; } = [];

        public Task<OperationResult<DesignConversionVisualCostLedgerRecord>> AppendRecordAsync(
            DesignConversionVisualCostLedgerRecord record,
            CancellationToken cancellationToken = default)
        {
            AttemptedRecords.Add(record);
            return Task.FromResult(OperationResult<DesignConversionVisualCostLedgerRecord>.Fail(message, code));
        }

        public Task<OperationResult<IReadOnlyList<DesignConversionVisualCostLedgerRecord>>> ListRecordsAsync(
            CancellationToken cancellationToken = default)
            => Task.FromResult(OperationResult<IReadOnlyList<DesignConversionVisualCostLedgerRecord>>.Ok([]));

        public Task<OperationResult<DesignConversionVisualCostLedgerPeriodAggregate>> AggregatePeriodAsync(
            string userRef,
            DateTimeOffset periodStartUtc,
            DateTimeOffset periodEndExclusiveUtc,
            CancellationToken cancellationToken = default)
            => Task.FromResult(OperationResult<DesignConversionVisualCostLedgerPeriodAggregate>.Fail(message, code));
    }
}
