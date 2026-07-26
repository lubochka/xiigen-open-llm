// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using AgentInfrastructure;
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
            visualCostLedger: new JsonlDesignConversionVisualCostLedger(ledgerRoot));
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
        Assert.Equal(DesignConversionThinClientStatus.Completed, result.Value!.ContractStatus);
        var mapped = Assert.Single(evaluator.Requests);
        Assert.Equal(0, mapped.Budget.MaxImprovementCandidates);
        Assert.Equal(
            expectedEffectiveMaxAttempts,
            DesignConversionVisualQaBoundaryPolicy.ResolveEffectiveTierMaxImprovementCandidates(
                mapped.Budget.MaxImprovementCandidates,
                mapped.Budget.Tier));
    }

    [Fact]
    public async Task RunAsync_MapsSubmittedRequestToInternalServiceAndCompletesStatus()
    {
        var evaluator = new RecordingEvaluator(CreateDeterministicInternalEvaluator());
        var runtime = new DesignConversionThinClientJobRuntime(evaluator, FixedClock());
        var submitted = runtime.Submit(CreateSubmittedRequest());
        Assert.True(submitted.IsSuccess, submitted.ErrorMessage);

        var result = await runtime.RunAsync(submitted.Value!.JobId);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionThinClientStatus.Completed, result.Value!.ContractStatus);
        Assert.Equal(DesignConversionThinClientStatus.Completed, result.Value.Job.Status);
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
        Assert.Equal(DesignConversionThinClientStatus.Completed, stored.Value!.Job.Status);
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
        var baselineEvaluator = new RecordingEvaluator(CreateDeterministicInternalEvaluator());
        var baselineRuntime = new DesignConversionThinClientJobRuntime(baselineEvaluator, FixedClock());
        var baselineSubmitted = baselineRuntime.Submit(CreatePrivateSubmittedRequest());
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
        var learnedRuntime = new DesignConversionThinClientJobRuntime(learnedEvaluator, FixedClock());
        var learnedSubmitted = learnedRuntime.Submit(CreatePrivateSubmittedRequest());
        Assert.True(learnedSubmitted.IsSuccess, learnedSubmitted.ErrorMessage);
        var learnedStatus = await learnedRuntime.RunAsync(learnedSubmitted.Value!.JobId);
        Assert.True(learnedStatus.IsSuccess, learnedStatus.ErrorMessage);

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
        Assert.DoesNotContain("C:/Users/", learnedEvidenceText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("exact_prompt", learnedEvidenceText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("private.fig", learnedEvidenceText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("external-provider-payload", learnedEvidenceText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ModelUploadReady", learnedEvidenceText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RunAsync_WithConfiguredLocalArtifactDbPersistsSanitizedCompletedEvaluation()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "thin-client-runtime-artifact-db");
        var db = new JsonlDesignConversionLocalArtifactDb(root);
        var evaluator = new RecordingEvaluator(CreateDeterministicInternalEvaluator());
        var runtime = new DesignConversionThinClientJobRuntime(evaluator, FixedClock(), db);
        var submitted = runtime.Submit(CreatePrivateSubmittedRequest());
        Assert.True(submitted.IsSuccess, submitted.ErrorMessage);

        var result = await runtime.RunAsync(submitted.Value!.JobId);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionThinClientStatus.Completed, result.Value!.ContractStatus);
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
            Assert.DoesNotContain("C:/Users/", artifact.UriRef, StringComparison.OrdinalIgnoreCase);
        });
        Assert.Equal(bool.TrueString, stored.Metadata["local_artifact_db"]);
        Assert.Equal(bool.TrueString, stored.Metadata["storage_training_artifact_db"]);
        Assert.Equal(bool.TrueString, stored.Metadata["product_not_ready"]);
        Assert.Equal(bool.FalseString, stored.Metadata["production_db_ready"]);
        Assert.Equal(bool.TrueString, stored.Metadata["training_candidate_only"]);
        Assert.Equal(bool.TrueString, stored.Metadata["runtime_created_dpo_candidate"]);
        Assert.Equal("1", stored.Metadata["runtime_created_dpo_candidate_count"]);
        Assert.Equal("runtime-candidate-review-pending", stored.Metadata["runtime_created_dpo_candidate_review_status"]);
        Assert.Equal(bool.TrueString, stored.Metadata["review_required"]);
        Assert.Equal("pending", stored.Metadata["approval_status"]);
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
        var candidate = Assert.Single(stored.DpoCandidates);
        Assert.True(candidate.TrainingCandidateOnly);
        Assert.True(candidate.ProductNotReady);
        Assert.False(candidate.IsQuarantined);
        Assert.Empty(candidate.BlockReasons);
        Assert.Equal(DesignConversionDpoUnits.ImproverUnit, candidate.UnitId);
        Assert.Equal(bool.TrueString, candidate.TrainingRecord.Provenance.Metadata["runtime_created_dpo_candidate"]);
        Assert.Equal(bool.TrueString, candidate.TrainingRecord.Provenance.Metadata["review_required"]);
        Assert.Equal("pending", candidate.TrainingRecord.Provenance.Metadata["approval_status"]);
        Assert.Equal("runtime-candidate-review-pending", candidate.TrainingRecord.Provenance.Metadata["review_status"]);
        Assert.Equal(bool.FalseString, candidate.TrainingRecord.Provenance.Metadata["approved_for_training"]);
        Assert.Equal(bool.TrueString, candidate.TrainingRecord.Provenance.Metadata["training_candidate_only"]);
        Assert.Equal(bool.FalseString, candidate.TrainingRecord.Provenance.Metadata["production_dpo"]);
        Assert.Equal(bool.TrueString, candidate.TrainingRecord.Provenance.Metadata["product_not_ready"]);
        Assert.Equal(bool.FalseString, candidate.TrainingRecord.Provenance.Metadata["model_readiness_claimed"]);
        Assert.Equal(bool.TrueString, candidate.TrainingRecord.Provenance.Metadata["held_out_not_proven"]);
        Assert.Equal(bool.TrueString, candidate.TrainingRecord.Provenance.Metadata["high_quality_negative_pair_not_proven"]);
        Assert.Equal(bool.TrueString, candidate.TrainingRecord.Provenance.Metadata["source_disjoint_not_proven"]);
        Assert.Equal(bool.TrueString, candidate.TrainingRecord.License.Metadata["review_required"]);
        Assert.Equal("pending", candidate.TrainingRecord.License.Metadata["approval_status"]);
        Assert.Equal(bool.FalseString, candidate.TrainingRecord.License.Metadata["approved_for_training"]);
        Assert.Equal(bool.TrueString, candidate.TrainingRecord.License.Metadata["held_out_not_proven"]);
        Assert.Equal(bool.FalseString, candidate.TrainingRecord.License.Metadata["real_training_ready"]);
        Assert.StartsWith("dcm-chosen_html-", candidate.TrainingRecord.Chosen.Html, StringComparison.Ordinal);
        Assert.StartsWith("dcm-rejected_html-", candidate.TrainingRecord.Rejected.Html, StringComparison.Ordinal);
        Assert.DoesNotContain("<main", candidate.TrainingRecord.Chosen.Html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(".private-frame", candidate.TrainingRecord.Chosen.Css, StringComparison.OrdinalIgnoreCase);

        var display = await db.QueryDisplayRowsAsync(new DesignConversionDpoDisplayQuery(IncludeBlocked: true));
        Assert.True(display.IsSuccess, display.ErrorMessage);
        var displayRow = Assert.Single(display.Value!.Rows);
        Assert.True(displayRow.TrainingCandidateOnly);
        Assert.True(displayRow.ProductNotReady);
        Assert.True(displayRow.UsefulnessScore > 0);
        Assert.Equal("runtime-candidate-review-pending", displayRow.ReadinessBoundary);
        Assert.Equal(bool.TrueString, displayRow.BoundaryMetadata["review_required"]);
        Assert.Equal("pending", displayRow.BoundaryMetadata["approval_status"]);
        Assert.Equal("runtime-candidate-review-pending", displayRow.BoundaryMetadata["review_status"]);
        Assert.Equal(bool.FalseString, displayRow.BoundaryMetadata["approved_for_training"]);
        Assert.Equal(bool.FalseString, displayRow.BoundaryMetadata["model_readiness_claimed"]);
        Assert.Contains("runtime-candidate-review-pending", displayRow.UsefulnessFactors);
        Assert.Contains("source_disjoint=not_proven", displayRow.UsefulnessFactors);
        Assert.Contains("product_not_ready", display.Value.Warnings);
        Assert.Contains("runtime_created_dpo_candidate_review_pending", display.Value.Warnings);

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
        Assert.Contains("runtime-candidate-review-pending", persistedText, StringComparison.Ordinal);
        Assert.Contains("approval_status", persistedText, StringComparison.Ordinal);
        Assert.Contains("production_db_ready", persistedText, StringComparison.Ordinal);
        Assert.Contains("production_db_ready:false", indexText, StringComparison.Ordinal);
        Assert.DoesNotContain("<main", persistedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(".private-frame", persistedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("raw-tree-private", persistedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("C:/Users/", persistedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("exact_prompt", persistedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("private.fig", persistedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("external-provider-payload", persistedText, StringComparison.OrdinalIgnoreCase);
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
        Assert.Equal(DesignConversionThinClientStatus.Completed, firstRun.Value!.ContractStatus);
        Assert.Equal(DesignConversionThinClientStatus.Completed, secondRun.Value!.Job.Status);
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
        var runtime = new DesignConversionThinClientJobRuntime(evaluator, FixedClock(), visualCostLedger: ledger);
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
        Assert.Equal(DesignConversionThinClientStatus.Completed, firstRun.Value!.ContractStatus);
        Assert.Equal(DesignConversionThinClientStatus.Completed, replay.Value!.ContractStatus);
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
            visualCostLedger: new JsonlDesignConversionVisualCostLedger(ledgerRoot));
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
        Assert.DoesNotContain("C:/Users/", serializedReadback, StringComparison.OrdinalIgnoreCase);
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
        var runtime = new DesignConversionThinClientJobRuntime(evaluator, FixedClock(), visualCostLedger: ledger);
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

        var runtime = new DesignConversionThinClientJobRuntime(evaluator, NonUtcClock, visualCostLedger: ledger);
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
            visualCostLedger: ledger);
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
        Assert.Equal(DesignConversionThinClientStatus.Completed, result.Value!.ContractStatus);
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
            Assert.DoesNotContain("C:/Users/", artifact.UriRef, StringComparison.OrdinalIgnoreCase);
        });

        var pngFiles = Directory.GetFiles(artifactRoot, "*.png", SearchOption.AllDirectories);
        Assert.Equal(4, pngFiles.Length);
        Assert.All(pngFiles, path => Assert.True(new FileInfo(path).Length > 1000, path));

        var persistedText = await File.ReadAllTextAsync(Path.Combine(dbRoot, "conversion-results.jsonl"));
        Assert.Contains("render_artifact_refs", persistedText, StringComparison.Ordinal);
        Assert.DoesNotContain("PRIVATE_PASS159_SECRET", persistedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("<main", persistedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(".private-frame", persistedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("C:/Users/", persistedText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ApproveRuntimeCreatedDpoCandidateAsync_ApprovesSelectedCandidateAndKeepsReadinessBoundaries()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "thin-client-runtime-approval");
        var db = new JsonlDesignConversionLocalArtifactDb(root);
        var evaluator = new RecordingEvaluator(CreateDeterministicInternalEvaluator());
        var runtime = new DesignConversionThinClientJobRuntime(evaluator, FixedClock(), db);
        var submitted = runtime.Submit(CreatePrivateSubmittedRequest());
        Assert.True(submitted.IsSuccess, submitted.ErrorMessage);
        var run = await runtime.RunAsync(submitted.Value!.JobId);
        Assert.True(run.IsSuccess, run.ErrorMessage);
        var listed = await db.ListRecordsAsync(new DesignConversionLocalArtifactDbQuery(JobId: submitted.Value.JobId));
        Assert.True(listed.IsSuccess, listed.ErrorMessage);
        var stored = Assert.Single(listed.Value!);
        var candidate = Assert.Single(stored.DpoCandidates);
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
            new DesignConversionLocalArtifactDbQuery(JobId: submitted.Value.JobId));
        var beforeHandoff = await api.ExportApprovedRuntimeDpoCandidatesAsync(
            new DesignConversionLocalArtifactDbQuery(JobId: submitted.Value.JobId),
            DateTimeOffset.UnixEpoch.AddMinutes(107));

        Assert.False(badId.IsSuccess);
        Assert.Equal("DESIGN_CONVERSION_DPO_CANDIDATE_NOT_FOUND", badId.ErrorCode);
        Assert.False(missingReviewer.IsSuccess);
        Assert.Equal("DESIGN_CONVERSION_DPO_APPROVAL_REVIEWER_REQUIRED", missingReviewer.ErrorCode);
        Assert.False(missingReason.IsSuccess);
        Assert.Equal("DESIGN_CONVERSION_DPO_APPROVAL_REASON_REQUIRED", missingReason.ErrorCode);
        Assert.True(beforeApproval.IsSuccess, beforeApproval.ErrorMessage);
        Assert.False(Assert.Single(beforeApproval.Value!.Rows).TrainReady);
        Assert.Equal("pending", Assert.Single(beforeApproval.Value.Rows).ApprovalStatus);
        Assert.True(beforeApprovedTraining.IsSuccess, beforeApprovedTraining.ErrorMessage);
        Assert.Empty(beforeApprovedTraining.Value!);
        Assert.True(beforeHandoff.IsSuccess, beforeHandoff.ErrorMessage);
        Assert.Equal(0, beforeHandoff.Value!.ApprovedCandidateCount);
        Assert.Empty(beforeHandoff.Value.Rows);
        Assert.Contains("no_approved_candidates", beforeHandoff.Value.Warnings);
        Assert.Contains("training_handoff_only", beforeHandoff.Value.Warnings);
        Assert.Equal(bool.TrueString, beforeHandoff.Value.BoundaryMetadata["not_trained"]);
        Assert.Equal(bool.TrueString, beforeHandoff.Value.BoundaryMetadata["weights_not_produced"]);
        Assert.Equal(bool.TrueString, beforeHandoff.Value.BoundaryMetadata["model_upload_not_ready"]);

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
        Assert.Equal("reviewer:pass106-local", approval.Value.ApprovedByRef);
        Assert.Equal(approvedAt, approval.Value.ApprovedAtUtc);
        Assert.Contains("runtime_created_dpo_candidate_approved_for_training", approval.Value.EvidenceRefs);
        Assert.Equal(bool.TrueString, approval.Value.BoundaryMetadata["approved_for_training"]);
        Assert.Equal(bool.TrueString, approval.Value.BoundaryMetadata["product_not_ready"]);
        Assert.Equal(bool.FalseString, approval.Value.BoundaryMetadata["model_readiness_claimed"]);
        Assert.Equal(bool.FalseString, approval.Value.BoundaryMetadata["public_api_verified"]);
        Assert.Equal(bool.FalseString, approval.Value.BoundaryMetadata["production_db_ready"]);
        Assert.Equal(bool.FalseString, approval.Value.BoundaryMetadata["trained_model_ready"]);
        Assert.Equal(bool.TrueString, approval.Value.BoundaryMetadata["held_out_not_proven"]);
        Assert.Equal(bool.TrueString, approval.Value.BoundaryMetadata["source_disjoint_not_proven"]);

        var approvedTraining = await db.QueryApprovedTrainingCandidatesAsync(
            new DesignConversionLocalArtifactDbQuery(JobId: submitted.Value.JobId));
        var readback = await api.QueryLocalArtifactReadbackAsync(
            new DesignConversionLocalArtifactReadbackApiRequest(JobId: submitted.Value.JobId));
        var usefulness = await api.QueryAsync(new DesignConversionDpoUsefulnessApiRequest(IncludeBlocked: true));
        var handoff = await api.ExportApprovedRuntimeDpoCandidatesAsync(
            new DesignConversionLocalArtifactDbQuery(JobId: submitted.Value.JobId),
            DateTimeOffset.UnixEpoch.AddMinutes(107));
        var afterListed = await db.ListRecordsAsync(new DesignConversionLocalArtifactDbQuery(JobId: submitted.Value.JobId));
        var persistedText = await File.ReadAllTextAsync(Path.Combine(root, "conversion-results.jsonl"));
        var indexText = await File.ReadAllTextAsync(Path.Combine(root, "conversion-results.index.json"));
        var serializedApproval = JsonSerializer.Serialize(approval.Value);
        var serializedHandoff = JsonSerializer.Serialize(handoff.Value);

        Assert.True(approvedTraining.IsSuccess, approvedTraining.ErrorMessage);
        var approvedRecord = Assert.Single(approvedTraining.Value!);
        Assert.Equal(candidate.RecordId, approvedRecord.RecordId);
        Assert.Equal("approved", approvedRecord.Provenance.Metadata["approval_status"]);
        Assert.Equal(bool.TrueString, approvedRecord.Provenance.Metadata["approved_for_training"]);
        Assert.Equal("runtime-candidate-review-approved", approvedRecord.Provenance.Metadata["review_status"]);
        Assert.Equal(bool.FalseString, approvedRecord.Provenance.Metadata["model_readiness_claimed"]);
        Assert.Equal(bool.FalseString, approvedRecord.Provenance.Metadata["public_api_verified"]);
        Assert.Equal(bool.FalseString, approvedRecord.Provenance.Metadata["production_db_ready"]);
        Assert.Equal(bool.FalseString, approvedRecord.Provenance.Metadata["trained_model_ready"]);
        Assert.Equal(bool.TrueString, approvedRecord.Provenance.Metadata["held_out_not_proven"]);
        Assert.Equal(bool.TrueString, approvedRecord.Provenance.Metadata["source_disjoint_not_proven"]);
        Assert.True(readback.IsSuccess, readback.ErrorMessage);
        Assert.Equal("0", readback.Value!.BoundaryMetadata["runtime_review_pending_count"]);
        Assert.Equal("1", readback.Value.BoundaryMetadata["runtime_review_approved_count"]);
        Assert.Equal("approved", readback.Value.BoundaryMetadata["approval_status"]);
        Assert.Equal("1", readback.Value.BoundaryMetadata["approved_for_training_count"]);
        Assert.Equal(bool.FalseString, readback.Value.BoundaryMetadata["public_api_verified"]);
        Assert.Equal(bool.FalseString, readback.Value.BoundaryMetadata["production_db_ready"]);
        Assert.Equal(bool.FalseString, readback.Value.BoundaryMetadata["trained_model_ready"]);
        Assert.Equal(bool.TrueString, readback.Value.BoundaryMetadata["held_out_not_proven"]);
        Assert.Equal(bool.TrueString, readback.Value.BoundaryMetadata["source_disjoint_not_proven"]);
        var readbackRow = Assert.Single(readback.Value.Rows);
        Assert.Equal("runtime-candidate-review-approved", readbackRow.DpoReviewStatus);
        Assert.Equal("approved", readbackRow.DpoApprovalStatus);
        Assert.Equal(bool.TrueString, readbackRow.BoundaryMetadata["approved_for_training"]);
        Assert.True(readbackRow.ProductNotReady);
        Assert.False(readbackRow.ProductionDpo);
        Assert.False(readbackRow.ModelReadinessClaimed);
        Assert.True(readbackRow.HeldOutNotProven);
        Assert.True(readbackRow.SourceDisjointNotProven);
        Assert.True(usefulness.IsSuccess, usefulness.ErrorMessage);
        Assert.Equal(1, usefulness.Value!.TrainReadyCount);
        Assert.Equal("0", usefulness.Value.BoundaryMetadata["runtime_review_pending_count"]);
        Assert.Equal("1", usefulness.Value.BoundaryMetadata["runtime_review_approved_count"]);
        var usefulnessRow = Assert.Single(usefulness.Value.Rows);
        Assert.Equal("train-ready", usefulnessRow.ReviewStatus);
        Assert.Equal("approved", usefulnessRow.ApprovalStatus);
        Assert.True(usefulnessRow.TrainReady);
        Assert.True(usefulnessRow.TrainingCandidateOnly);
        Assert.True(usefulnessRow.ProductNotReady);
        Assert.False(usefulnessRow.ProductionDpo);
        Assert.False(usefulnessRow.ModelReadinessClaimed);
        Assert.True(usefulnessRow.HeldOutNotProven);
        Assert.True(usefulnessRow.SourceDisjointNotProven);
        Assert.Equal(bool.TrueString, usefulnessRow.BoundaryMetadata["approved_for_training"]);
        Assert.True(handoff.IsSuccess, handoff.ErrorMessage);
        Assert.StartsWith("approved-dpo-handoff-", handoff.Value!.ArtifactId, StringComparison.Ordinal);
        Assert.StartsWith("memory://design-conversion/dpo-handoff/approved-dpo-handoff-", handoff.Value.ArtifactRef, StringComparison.Ordinal);
        Assert.Equal(JsonlDesignConversionLocalArtifactDb.ApprovedDpoHandoffSchemaVersion, handoff.Value.SchemaVersion);
        Assert.Equal(1, handoff.Value.ApprovedCandidateCount);
        Assert.Contains("training_handoff_only", handoff.Value.Warnings);
        Assert.Contains("not_trained", handoff.Value.Warnings);
        Assert.Contains("weights_not_produced", handoff.Value.Warnings);
        Assert.Contains("model_upload_not_ready", handoff.Value.Warnings);
        Assert.Contains("held_out_not_proven", handoff.Value.Warnings);
        Assert.Contains("source_disjoint_not_proven", handoff.Value.Warnings);
        Assert.Equal(bool.TrueString, handoff.Value.BoundaryMetadata["training_handoff_only"]);
        Assert.Equal(bool.TrueString, handoff.Value.BoundaryMetadata["not_trained"]);
        Assert.Equal(bool.TrueString, handoff.Value.BoundaryMetadata["weights_not_produced"]);
        Assert.Equal(bool.TrueString, handoff.Value.BoundaryMetadata["model_upload_not_ready"]);
        Assert.Equal(bool.FalseString, handoff.Value.BoundaryMetadata["trained_model_ready"]);
        Assert.Equal(bool.FalseString, handoff.Value.BoundaryMetadata["model_readiness_claimed"]);
        var handoffRow = Assert.Single(handoff.Value.Rows);
        Assert.Equal(stored.RecordId, handoffRow.LocalArtifactRecordId);
        Assert.Equal(candidate.CandidateId, handoffRow.CandidateId);
        Assert.Equal(candidate.RecordId, handoffRow.TrainingRecordId);
        Assert.Equal("approved", handoffRow.ApprovalStatus);
        Assert.True(handoffRow.ApprovedForTraining);
        Assert.True(handoffRow.TrainingHandoffOnly);
        Assert.Equal("reviewer:pass106-local", handoffRow.ApprovedByRef);
        Assert.Equal("runtime-candidate-review-approved", handoffRow.ReviewStatus);
        Assert.StartsWith("sha256:", handoffRow.RecordHash, StringComparison.Ordinal);
        Assert.StartsWith("dpo-handoff-internal-unit-state-", handoffRow.InternalUnitStateHash, StringComparison.Ordinal);
        Assert.StartsWith("dpo-handoff-source-structure-", handoffRow.SourceStructureHash, StringComparison.Ordinal);
        Assert.StartsWith("dpo-handoff-chosen-payload-", handoffRow.ChosenPayloadHash, StringComparison.Ordinal);
        Assert.StartsWith("dpo-handoff-rejected-payload-", handoffRow.RejectedPayloadHash, StringComparison.Ordinal);
        Assert.Equal(bool.TrueString, handoffRow.BoundaryMetadata["training_handoff_only"]);
        Assert.Equal(bool.TrueString, handoffRow.BoundaryMetadata["not_trained"]);
        Assert.Equal(bool.FalseString, handoffRow.BoundaryMetadata["trained_model_ready"]);
        Assert.Equal(bool.FalseString, handoffRow.BoundaryMetadata["raw_html_included"]);
        Assert.Equal(bool.FalseString, handoffRow.BoundaryMetadata["raw_css_included"]);

        var intake = await api.CreateTrainingIntakeAsync(
            handoff.Value,
            "requester:pass108-local",
            "future ILearningMachine intake only; no training started",
            DateTimeOffset.UnixEpoch.AddMinutes(108));
        var listedIntakes = await api.ListTrainingIntakeRequestsAsync();
        var intakeText = await File.ReadAllTextAsync(Path.Combine(root, "training-intake-requests.jsonl"));
        var serializedIntake = JsonSerializer.Serialize(intake.Value);

        Assert.True(intake.IsSuccess, intake.ErrorMessage);
        Assert.StartsWith("training-intake-", intake.Value!.IntakeId, StringComparison.Ordinal);
        Assert.Equal(handoff.Value.ArtifactId, intake.Value.SourceHandoffArtifactId);
        Assert.Equal(1, intake.Value.ApprovedCandidateCount);
        Assert.Equal(1, intake.Value.AcceptedCandidateCount);
        Assert.True(intake.Value.TrainingIntakeOnly);
        Assert.True(intake.Value.TrainingNotStarted);
        Assert.True(intake.Value.NotTrained);
        Assert.True(intake.Value.WeightsNotProduced);
        Assert.True(intake.Value.CheckpointNotMutated);
        Assert.True(intake.Value.ModelUploadNotReady);
        Assert.True(intake.Value.ProductNotReady);
        Assert.True(intake.Value.HeldOutNotProven);
        Assert.True(intake.Value.SourceDisjointNotProven);
        Assert.Equal(bool.TrueString, intake.Value.BoundaryMetadata["training_intake_only"]);
        Assert.Equal(bool.TrueString, intake.Value.BoundaryMetadata["training_not_started"]);
        Assert.Equal(bool.TrueString, intake.Value.BoundaryMetadata["not_trained"]);
        Assert.Equal(bool.TrueString, intake.Value.BoundaryMetadata["weights_not_produced"]);
        Assert.Equal(bool.TrueString, intake.Value.BoundaryMetadata["checkpoint_not_mutated"]);
        Assert.Equal(bool.TrueString, intake.Value.BoundaryMetadata["model_upload_not_ready"]);
        Assert.Equal(bool.FalseString, intake.Value.BoundaryMetadata["training_started"]);
        Assert.Equal(bool.FalseString, intake.Value.BoundaryMetadata["trained_model_ready"]);
        Assert.Equal(bool.FalseString, intake.Value.BoundaryMetadata["model_readiness_claimed"]);
        Assert.Equal("future_ILearningMachine", intake.Value.BoundaryMetadata["target_training_system"]);
        var intakeCandidate = Assert.Single(intake.Value.CandidateRefs);
        Assert.Equal(handoffRow.CandidateId, intakeCandidate.CandidateId);
        Assert.Equal(handoffRow.TrainingRecordId, intakeCandidate.TrainingRecordId);
        Assert.Equal(handoffRow.RecordHash, intakeCandidate.HandoffRecordHash);
        Assert.Equal(handoffRow.SourceManifestRef, intakeCandidate.SourceManifestRef);
        Assert.StartsWith("sha256:", intakeCandidate.SourceManifestHash, StringComparison.Ordinal);
        Assert.StartsWith("sha256:", intakeCandidate.SourceIdentityHash, StringComparison.Ordinal);
        Assert.Equal(handoffRow.ChosenPayloadHash, intakeCandidate.ChosenPayloadHash);
        Assert.Equal("proven", intake.Value.BoundaryMetadata["source_manifest_status"]);
        Assert.Equal("blocked_same_source", intake.Value.BoundaryMetadata["source_disjoint_status"]);
        Assert.True(listedIntakes.IsSuccess, listedIntakes.ErrorMessage);
        Assert.Single(listedIntakes.Value!);

        var beforePreflightText = await File.ReadAllTextAsync(Path.Combine(root, "training-intake-requests.jsonl"));
        var preflight = await api.RunTrainingIntakePreflightAsync(intake.Value.IntakeId);
        var afterPreflightText = await File.ReadAllTextAsync(Path.Combine(root, "training-intake-requests.jsonl"));
        var serializedPreflight = JsonSerializer.Serialize(preflight.Value);
        Assert.True(preflight.IsSuccess, preflight.ErrorMessage);
        Assert.StartsWith("training-intake-preflight-", preflight.Value!.PreflightId, StringComparison.Ordinal);
        Assert.Equal(intake.Value.IntakeId, preflight.Value.TrainingIntakeId);
        Assert.Equal(1, preflight.Value.AcceptedCandidateCount);
        Assert.False(preflight.Value.MayStartTraining);
        Assert.Equal("proven", preflight.Value.SourceManifestStatus);
        Assert.Equal("blocked_same_source", preflight.Value.SourceDisjointStatus);
        Assert.False(preflight.Value.TrainingStarted);
        Assert.False(preflight.Value.CheckpointMutated);
        Assert.False(preflight.Value.WeightsProduced);
        Assert.False(preflight.Value.ModelUploadReady);
        Assert.Contains("source_disjoint_blocked_same_source", preflight.Value.BlockReasons);
        Assert.Equal(bool.TrueString, preflight.Value.BoundaryMetadata["preflight_only"]);
        Assert.Equal(bool.FalseString, preflight.Value.BoundaryMetadata["may_start_training"]);
        Assert.Equal("proven", preflight.Value.BoundaryMetadata["source_manifest_status"]);
        Assert.Equal("blocked_same_source", preflight.Value.BoundaryMetadata["source_disjoint_status"]);
        Assert.Equal(bool.FalseString, preflight.Value.BoundaryMetadata["training_started"]);
        Assert.Equal(bool.FalseString, preflight.Value.BoundaryMetadata["checkpoint_mutated"]);
        Assert.Equal(bool.FalseString, preflight.Value.BoundaryMetadata["weights_produced"]);
        Assert.Equal(bool.FalseString, preflight.Value.BoundaryMetadata["model_upload_ready"]);
        Assert.Equal(beforePreflightText, afterPreflightText);
        Assert.True(afterListed.IsSuccess, afterListed.ErrorMessage);
        var afterStored = Assert.Single(afterListed.Value!);
        Assert.Equal("approved", afterStored.Metadata["approval_status"]);
        Assert.Equal(bool.TrueString, afterStored.Metadata["approved_for_training"]);
        Assert.Equal("runtime-candidate-review-approved", afterStored.Metadata["review_status"]);
        Assert.Contains("runtime_created_dpo_candidate_approved_for_training", persistedText, StringComparison.Ordinal);
        Assert.Contains("approval_status=approved", persistedText, StringComparison.Ordinal);
        Assert.Contains("approved_for_training=true", persistedText, StringComparison.Ordinal);
        Assert.Contains("\"approvedTrainingCandidateCount\": 1", indexText, StringComparison.Ordinal);
        Assert.DoesNotContain("<main", persistedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(".private-frame", persistedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("raw-tree-private", persistedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("C:/Users/", persistedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("exact_prompt", persistedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("private.fig", persistedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("external-provider-payload", persistedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("<main", serializedApproval, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(".private-frame", serializedApproval, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("raw-tree-private", serializedApproval, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("C:/Users/", serializedApproval, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("exact_prompt", serializedApproval, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("private.fig", serializedApproval, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("external-provider-payload", serializedApproval, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("<main", serializedHandoff, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(".private-frame", serializedHandoff, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("raw-tree-private", serializedHandoff, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("C:/Users/", serializedHandoff, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("exact_prompt", serializedHandoff, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("private.fig", serializedHandoff, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("external-provider-payload", serializedHandoff, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("<main", serializedIntake, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(".private-frame", serializedIntake, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("raw-tree-private", serializedIntake, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("C:/Users/", serializedIntake, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("exact_prompt", serializedIntake, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("private.fig", serializedIntake, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("external-provider-payload", serializedIntake, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("<main", serializedPreflight, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(".private-frame", serializedPreflight, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("raw-tree-private", serializedPreflight, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("C:/Users/", serializedPreflight, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("exact_prompt", serializedPreflight, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("private.fig", serializedPreflight, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("external-provider-payload", serializedPreflight, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("<main", intakeText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(".private-frame", intakeText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("raw-tree-private", intakeText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("C:/Users/", intakeText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("exact_prompt", intakeText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("private.fig", intakeText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("external-provider-payload", intakeText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task QueryLocalArtifactReadbackAsync_ReturnsRuntimePersistedRecordAndCreatedDpoRows()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "thin-client-runtime-readback-api");
        var db = new JsonlDesignConversionLocalArtifactDb(root);
        var evaluator = new RecordingEvaluator(CreateDeterministicInternalEvaluator());
        var runtime = new DesignConversionThinClientJobRuntime(evaluator, FixedClock(), db);
        var submitted = runtime.Submit(CreatePrivateSubmittedRequest());
        Assert.True(submitted.IsSuccess, submitted.ErrorMessage);
        var run = await runtime.RunAsync(submitted.Value!.JobId);
        Assert.True(run.IsSuccess, run.ErrorMessage);

        var api = new DesignConversionDpoUsefulnessApi(db);
        var readback = await api.QueryLocalArtifactReadbackAsync(
            new DesignConversionLocalArtifactReadbackApiRequest(JobId: submitted.Value.JobId));

        Assert.True(readback.IsSuccess, readback.ErrorMessage);
        Assert.Equal(DesignConversionDpoUsefulnessApi.LocalArtifactReadbackApiShape, readback.Value!.ApiShape);
        Assert.False(readback.Value.ProductReady);
        Assert.Equal(1, readback.Value.RecordCount);
        Assert.Equal(1, readback.Value.RuntimeCreatedRecordCount);
        Assert.Equal(1, readback.Value.DpoCandidateCount);
        Assert.Contains("runtime_created_artifact_record_review_only", readback.Value.Warnings);
        Assert.Contains("runtime_created_dpo_candidates_available", readback.Value.Warnings);
        Assert.Contains("runtime_created_dpo_candidate_review_pending", readback.Value.Warnings);
        Assert.Contains("runtime_created_dpo_candidate_approval_required", readback.Value.Warnings);
        Assert.DoesNotContain("dpo_rows_not_created_without_dpo_candidates", readback.Value.Warnings);
        Assert.Equal(bool.FalseString, readback.Value.BoundaryMetadata["public_api_verified"]);
        Assert.Equal(bool.FalseString, readback.Value.BoundaryMetadata["production_db_ready"]);
        Assert.Equal(bool.FalseString, readback.Value.BoundaryMetadata["trained_model_ready"]);
        Assert.Equal("1", readback.Value.BoundaryMetadata["dpo_candidate_count"]);
        Assert.Equal("1", readback.Value.BoundaryMetadata["runtime_review_pending_count"]);
        Assert.Equal("1", readback.Value.BoundaryMetadata["review_required_count"]);
        Assert.Equal("pending", readback.Value.BoundaryMetadata["approval_status"]);
        Assert.Equal(bool.FalseString, readback.Value.BoundaryMetadata["model_readiness_claimed"]);

        var row = Assert.Single(readback.Value.Rows);
        Assert.Equal(submitted.Value.JobId, row.JobId);
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
            Assert.DoesNotContain("C:/Users/", render.UriRef, StringComparison.OrdinalIgnoreCase);
        });
        Assert.Equal(DefaultLocalDesignConversionVisualJudge.StatusOk, row.VisualJudgeStatus);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.LocalVisualLabelerMarker, row.VisualJudgeProviderBoundary);
        Assert.False(row.ExternalVisualAiCallExecuted);
        Assert.Equal(1, row.DpoCandidateCount);
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
        var usefulnessRow = Assert.Single(dpoUsefulness.Value!.Rows);
        Assert.Contains("runtime_created_dpo_candidate_review_pending", dpoUsefulness.Value.Warnings);
        Assert.Contains("runtime_created_dpo_candidate_approval_required", dpoUsefulness.Value.Warnings);
        Assert.Equal("1", dpoUsefulness.Value.BoundaryMetadata["runtime_review_pending_count"]);
        Assert.Equal("runtime-candidate-review-pending", usefulnessRow.ReviewStatus);
        Assert.True(usefulnessRow.ReviewRequired);
        Assert.Equal("pending", usefulnessRow.ApprovalStatus);
        Assert.False(usefulnessRow.TrainReady);
        Assert.Equal(0, dpoUsefulness.Value.TrainReadyCount);
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

        var serialized = JsonSerializer.Serialize(readback.Value);
        Assert.DoesNotContain("<main", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(".private-frame", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("raw-tree-private", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("C:/Users/", serialized, StringComparison.OrdinalIgnoreCase);
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
            FixedClock());
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
        Assert.Equal(DesignConversionThinClientStatus.Completed, firstRun.Value!.ContractStatus);
        Assert.Equal(DesignConversionThinClientStatus.Completed, replay.Value!.ContractStatus);
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
        AssertThinClientCompleted(firstRun.Value!);
        AssertThinClientCompleted(secondRun.Value!);
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
    [InlineData("paid-default", 0.00, "0.25")]
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
        AssertThinClientCompleted(status);
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
        Assert.Equal(DesignConversionThinClientStatus.Completed, result.Value!.ContractStatus);
        Assert.Equal(["evaluate"], events);
        Assert.Single(evaluator.Requests);
    }

    [Fact]
    public async Task RunAsync_ReturnsCompletedStatusWithoutRerunningTerminalJob()
    {
        var evaluator = new RecordingEvaluator(CreateDeterministicInternalEvaluator());
        var runtime = new DesignConversionThinClientJobRuntime(evaluator, FixedClock());
        var submitted = runtime.Submit(CreateSubmittedRequest());
        Assert.True(submitted.IsSuccess, submitted.ErrorMessage);

        var firstRun = await runtime.RunAsync(submitted.Value!.JobId);
        var secondRun = await runtime.RunAsync(submitted.Value.JobId);

        Assert.True(firstRun.IsSuccess, firstRun.ErrorMessage);
        Assert.True(secondRun.IsSuccess, secondRun.ErrorMessage);
        Assert.Equal(DesignConversionThinClientStatus.Completed, secondRun.Value!.Job.Status);
        Assert.Single(evaluator.Requests);
        Assert.Equal(firstRun.Value!.Artifacts.Select(artifact => artifact.ArtifactId), secondRun.Value.Artifacts.Select(artifact => artifact.ArtifactId));
    }

    private static IDesignConversionThinClientJobEvaluator CreateDeterministicInternalEvaluator()
        => new DesignConversionInternalServiceThinClientJobEvaluator(new DesignConversionInternalService(
            new DeterministicDesignConversionRenderProvider(),
            new DeterministicDesignConversionComparisonProvider()));

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
                LearnedWeight("source_kind:figma", 1.0, "source_kind_presence"),
                LearnedWeight("visual:chosen_score", 0.8, "chosen_visual_score")),
            ImproverUnit: LearnedRuntimeUnit(
                DesignConversionRuntimeScaffoldImproverUnit.UnitId,
                DesignConversionRuntimeScaffoldImproverUnit.Role,
                "runtime_scaffold_candidate_shell_with_local_learned_weight_artifact",
                LearnedWeight("source_kind:figma", 0.7, "source_kind_presence"),
                LearnedWeight("visual:chosen_score", 0.5, "chosen_visual_score")),
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

    private static void AssertThinClientCompleted(DesignConversionThinClientStatusResult status)
        => Assert.True(
            string.Equals(status.ContractStatus, DesignConversionThinClientStatus.Completed, StringComparison.Ordinal),
            string.Join(
                " | ",
                $"expected={DesignConversionThinClientStatus.Completed}",
                $"actual={status.ContractStatus}",
                $"job_status={status.Job.Status}",
                $"error_code={status.Job.Metadata.GetValueOrDefault("error_code", "<none>")}",
                $"warnings={string.Join(",", status.Warnings)}",
                $"provenance={JsonSerializer.Serialize(status.Provenance)}"));

    private static SubmittedDesignConversionRequest CreatePrivateSubmittedRequest()
    {
        var request = CreateSubmittedRequest();
        return request with
        {
            Html = "<main class=\"private-frame\" data-path=\"C:/Users/private-user/private.html\">exact_prompt design</main>",
            Css = ".private-frame{background:url('C:/Users/private-user/private.png');}",
            DesignTreePayload = request.DesignTreePayload with
            {
                Content = "{\"type\":\"FRAME\",\"raw\":\"raw-tree-private\",\"path\":\"C:/Users/private-user/tree.json\"}",
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
                ["source_manifest_ref"] = "file:///C:/Users/private-user/private.fig",
                ["exact_prompt"] = "make the private button blue",
                ["external_provider_payload"] = "external-provider-payload"
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
            Css = ".private-frame{background:url('C:/Users/private-user/private-pass159.png');color:#123456;}",
            Screenshots = [],
            DesignRenders = renders,
            Budget = request.Budget with
            {
                MaxRenderQaRuns = 4
            },
            Provenance = new Dictionary<string, string>
            {
                ["boundary"] = "pass159-thin-client-runtime-test",
                ["source_manifest_ref"] = "file:///C:/Users/private-user/private-pass159.fig",
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

    private sealed class EventRecordingVisualCostBudgetStore(
        JsonlDesignConversionVisualCostBudgetStore inner,
        List<string> events) : IDesignConversionVisualCostBudgetStore
    {
        public JsonlDesignConversionVisualCostBudgetStore Inner { get; } = inner;

        public async Task<OperationResult<DesignConversionVisualCostReservationResult>> TryReserveAsync(
            DesignConversionVisualCostReservationRequest request,
            CancellationToken cancellationToken = default)
        {
            events.Add("reserve");
            return await Inner.TryReserveAsync(request, cancellationToken);
        }

        public async Task<OperationResult<DesignConversionVisualCostSettlementResult>> SettleAsync(
            DesignConversionVisualCostSettlementRequest request,
            CancellationToken cancellationToken = default)
        {
            events.Add("settle:" + request.ActualCostUsd.ToString("0.####", CultureInfo.InvariantCulture));
            return await Inner.SettleAsync(request, cancellationToken);
        }

        public async Task<OperationResult<DesignConversionVisualCostReleaseResult>> ReleaseAsync(
            DesignConversionVisualCostReleaseRequest request,
            CancellationToken cancellationToken = default)
        {
            events.Add("release");
            return await Inner.ReleaseAsync(request, cancellationToken);
        }

        public Task<OperationResult<DesignConversionVisualCostBudgetPeriodState>> ReadPeriodAsync(
            DesignConversionVisualCostBudgetPeriodQuery query,
            CancellationToken cancellationToken = default)
            => Inner.ReadPeriodAsync(query, cancellationToken);
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
