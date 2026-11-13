// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Buffers.Binary;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using DesignConversionDomain;
using ModelServer.DesignConversion;

public sealed class DesignConversionHumanApprovalTests
{
    private const string TenantA = "tenant:design-conversion:approval-alpha";
    private const string TenantB = "tenant:design-conversion:approval-beta";
    private const string ActorA = "human-approver-alpha";
    private const string ActorB = "human-approver-beta";
    private const string ProductionTrustedKey = "render_evidence_production_trusted";
    private const string ApprovalEvidenceAllowedKey = "approval_evidence_allowed";
    private const string ExternalVisualAiComparisonReviewUriPrefix =
        "artifact://design-conversion/human-approval/external-visual-ai-comparison/";

    [Fact]
    public async Task DesignConversionHumanApproval_HappyChallengeApprovalPersistsAcceptedForUserRecord()
    {
        var fixture = await CreateReadyFixtureAsync(nameof(DesignConversionHumanApproval_HappyChallengeApprovalPersistsAcceptedForUserRecord));

        var challenge = await IssueChallengeAsync(fixture);
        Assert.True(challenge.IsSuccess, challenge.ErrorMessage);
        var approval = await ApproveAsync(fixture, challenge.Value!);
        var records = await fixture.Api.ListHumanApprovalRecordsAsync();

        Assert.True(approval.IsSuccess, approval.ErrorMessage);
        Assert.True(records.IsSuccess, records.ErrorMessage);
        var record = Assert.Single(records.Value!);
        Assert.Equal(challenge.Value!.ChallengeId, record.ChallengeId);
        Assert.Equal(fixture.Submit.Value!.JobId, record.JobId);
        Assert.True(record.AcceptedForUser);
        Assert.False(record.ProductReady);
        Assert.Contains("DESIGN_CONVERSION_NON_DPO_HUMAN_APPROVAL_ACCEPTED_FOR_USER", approval.Value!.EvidenceRefs);
        Assert.Contains("accepted-for-user", record.BoundaryMetadata["human_approval_status"]);
        await AssertVisualCostLedgerDurablyRecordedAsync(fixture);
    }

    [Fact]
    public async Task DesignConversionHumanApproval_ExternalVisualAiLoopbackReceipt_AllowsApprovalButNotTrainingOrPublishing()
    {
        await using var server = await LoopbackVisualJudgeServer.StartAsync(score: 0.94);
        var fixture = await CreateReadyFixtureAsync(
            nameof(DesignConversionHumanApproval_ExternalVisualAiLoopbackReceipt_AllowsApprovalButNotTrainingOrPublishing),
            comparisonProviderFactory: artifactRoot =>
                DesignConversionExternalVisualAiReceiptTestHarness.CreateExternalHttpComparisonProvider(server, artifactRoot),
            useArtifactUris: false);

        Assert.True(fixture.Submit.IsSuccess, fixture.Submit.ErrorMessage);
        Assert.Equal(DesignConversionThinClientStatus.Completed, fixture.Submit.Value!.ContractStatus);
        Assert.Empty(fixture.Capture.LastSuccess!.ViewportFailures);
        Assert.Equal(4, server.RequestCount);
        Assert.All(
            server.RequestBodies,
            body => Assert.True(
                Encoding.UTF8.GetByteCount(body) < 1_000_000,
                "Human-approval loopback evidence must use compact real PNG payloads."));
        Assert.False(fixture.Submit.Value!.ProductReady);
        Assert.False(fixture.Submit.Value.TrainingReadiness.ProductReady);
        Assert.False(fixture.Submit.Value.ModelTrainingVerified);
        Assert.False(fixture.Submit.Value.ModelUploadReady);
        Assert.False(fixture.Submit.Value.DeploymentVerified);

        var challenge = await IssueChallengeAsync(fixture);
        Assert.True(challenge.IsSuccess, challenge.ErrorMessage);
        Assert.Equal(4, challenge.Value!.RequiredViewportIds.Count);
        Assert.All(challenge.Value.ComparisonRefs, AssertTrustedExternalVisualAiComparisonReviewRef);

        var approval = await ApproveAsync(
            fixture,
            challenge.Value,
            "external-loopback-human-approval-op",
            "human approved server-owned external visual AI receipt refs for every viewport");
        var records = await fixture.Api.ListHumanApprovalRecordsAsync();

        Assert.True(approval.IsSuccess, approval.ErrorMessage);
        Assert.True(records.IsSuccess, records.ErrorMessage);
        var record = Assert.Single(records.Value!);
        Assert.Equal(challenge.Value.ChallengeId, record.ChallengeId);
        Assert.True(record.AcceptedForUser);
        Assert.False(record.ProductReady);
        Assert.False(record.DpoEligible);
        Assert.False(record.TrainedOrLearnedProvenance);
        Assert.False(record.ModelWeightsPublished);
        Assert.Equal(challenge.Value.RequiredViewportIds, record.RequiredViewportIds);
        Assert.All(record.ComparisonRefs, AssertTrustedExternalVisualAiComparisonReviewRef);
        Assert.Equal(bool.FalseString, record.BoundaryMetadata["product_ready"]);
        Assert.Equal(bool.FalseString, record.BoundaryMetadata["dpo_eligible"]);
        Assert.Equal(bool.FalseString, record.BoundaryMetadata["model_weights_published"]);
    }

    [Fact]
    public async Task DesignConversionHumanApproval_ForgedExternalVisualAiReceipt_RejectsChallengeAndStoresNoApproval()
    {
        var fixture = await CreateReadyFixtureAsync(
            nameof(DesignConversionHumanApproval_ForgedExternalVisualAiReceipt_RejectsChallengeAndStoresNoApproval),
            comparisonProvider: new DesignConversionExternalVisualAiComparisonProvider(new ForgedReceiptVisualJudge()),
            useArtifactUris: true);

        Assert.True(fixture.Submit.IsSuccess, fixture.Submit.ErrorMessage);
        Assert.False(fixture.Submit.Value!.ProductReady);
        Assert.NotEqual(DesignConversionThinClientStatus.Completed, fixture.Submit.Value.ContractStatus);
        Assert.False(fixture.Submit.Value.ModelTrainingVerified);
        Assert.False(fixture.Submit.Value.ModelUploadReady);
        Assert.False(fixture.Submit.Value.DeploymentVerified);

        var rejected = await IssueChallengeAsync(fixture);
        var records = await fixture.Api.ListHumanApprovalRecordsAsync();

        Assert.False(rejected.IsSuccess);
        Assert.Equal("DESIGN_CONVERSION_APPROVAL_PNG_PROOF_INVALID", rejected.ErrorCode);
        Assert.True(records.IsSuccess, records.ErrorMessage);
        Assert.Empty(records.Value!);
    }

    [Theory]
    [InlineData(DesignConversionThinClientStatus.ContractOnlyNoRuntime)]
    [InlineData(DesignConversionThinClientStatus.Submitted)]
    [InlineData(DesignConversionThinClientStatus.Running)]
    [InlineData(DesignConversionThinClientStatus.Failed)]
    [InlineData(DesignConversionThinClientStatus.Cancelled)]
    [InlineData("unknown-status")]
    public async Task DesignConversionHumanApproval_NonEvaluatedOrFailedRuntimeStatusRejectsEvenWithCapturedEvaluation(
        string contractStatus)
    {
        var fixture = await CreateReadyFixtureAsync(
            $"{nameof(DesignConversionHumanApproval_NonEvaluatedOrFailedRuntimeStatusRejectsEvenWithCapturedEvaluation)}-{contractStatus}");
        Assert.NotNull(fixture.Capture.LastSuccess);

        var status = fixture.Submit.Value!.RuntimeStatus with
        {
            ContractStatus = contractStatus,
            Job = fixture.Submit.Value.RuntimeStatus.Job with { Status = contractStatus }
        };
        var service = new DesignConversionHumanApprovalService(
            new InMemoryDesignConversionHumanApprovalStore(),
            fixture.Clock.UtcNow);

        var rejected = await service.IssueChallengeAsync(
            new DesignConversionHumanApprovalChallengeIssueRequest(
                fixture.Submit.Value.JobId,
                Actor(TenantA, ActorA)),
            status,
            fixture.Capture.LastSuccess);

        Assert.False(rejected.IsSuccess);
        Assert.Equal("DESIGN_CONVERSION_APPROVAL_SNAPSHOT_NOT_COMPLETED", rejected.ErrorCode);
    }

    public static IEnumerable<object?[]> ExternalVisualAiCanonicalTrustMarkerMutationCases()
    {
        yield return ["production_trusted_false", ProductionTrustedKey, bool.FalseString];
        yield return ["approval_allowed_false", ApprovalEvidenceAllowedKey, bool.FalseString];
        yield return ["classification_untrusted", DesignConversionComparisonEvidenceTrustBoundary.TrustClassificationKey, "untrusted_injected_comparison_provider_evidence"];
        yield return ["receipt_validation_reason_invalid", "server_owned_external_visual_ai_receipt_validation_reason", "forged"];
        yield return ["network_call_false", "network_call_executed", bool.FalseString];
        yield return ["receipt_marker_false", ExternalHttpDesignConversionVisualJudge.ServerCallReceiptKey, bool.FalseString];
        yield return ["receipt_marker_missing", ExternalHttpDesignConversionVisualJudge.ServerCallReceiptKey, null];
        yield return ["receipt_version_mismatch", ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVersionKey, "design_conversion_visual_provider_call_receipt_v0"];
        yield return ["receipt_external_call_false", ExternalHttpDesignConversionVisualJudge.ServerCallReceiptExternalCallExecutedKey, bool.FalseString];
        yield return ["receipt_response_identity_false", ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseIdentityVerifiedKey, bool.FalseString];
        yield return ["receipt_response_schema_false", ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseSchemaVerifiedKey, bool.FalseString];
        yield return ["receipt_provider_name_mismatch", ExternalHttpDesignConversionVisualJudge.ServerCallReceiptProviderNameKey, "anthropic"];
        yield return ["missing_comparison_artifact_receipt_id", ExternalHttpDesignConversionVisualJudge.ServerCallReceiptComparisonArtifactIdKey, null];
        yield return ["server_bound_expected_sha_mismatch", "server_bound_expected_sha256", ShaRef("forged-server-bound-expected")];
    }

    [Theory]
    [MemberData(nameof(ExternalVisualAiCanonicalTrustMarkerMutationCases))]
    public async Task DesignConversionHumanApproval_ExternalVisualAiCanonicalTrustMarkerMutation_RejectsChallenge(
        string caseName,
        string metadataKey,
        string? replacementValue)
    {
        await using var server = await LoopbackVisualJudgeServer.StartAsync(score: 0.94);
        var fixture = await CreateReadyFixtureAsync(
            $"external-trust-mutation-{caseName}",
            comparisonProviderFactory: artifactRoot =>
                DesignConversionExternalVisualAiReceiptTestHarness.CreateExternalHttpComparisonProvider(server, artifactRoot),
            useArtifactUris: false,
            evaluationMutation: evaluation => MutateComparisonMetadata(evaluation, metadataKey, replacementValue));

        Assert.True(fixture.Submit.IsSuccess, fixture.Submit.ErrorMessage);
        Assert.Equal(4, server.RequestCount);
        Assert.All(
            server.RequestBodies,
            body => Assert.True(
                Encoding.UTF8.GetByteCount(body) < 1_000_000,
                "Human-approval loopback evidence must use compact real PNG payloads."));

        var rejected = await IssueChallengeAsync(fixture);
        var records = await fixture.Api.ListHumanApprovalRecordsAsync();

        Assert.False(rejected.IsSuccess);
        Assert.Equal("DESIGN_CONVERSION_APPROVAL_PNG_PROOF_INVALID", rejected.ErrorCode);
        Assert.Contains("comparison ref", rejected.ErrorMessage, StringComparison.Ordinal);
        Assert.True(records.IsSuccess, records.ErrorMessage);
        Assert.Empty(records.Value!);
    }

    [Theory]
    [InlineData(390, 844, "source-390x844", "3", "source-390x844@3", "source-390x844@3")]
    [InlineData(390, 844, "source-390x844~mobile-menu", null, "source-390x844~mobile-menu", "source-390x844~mobile-menu")]
    [InlineData(390, 844, "source-390x844~mobile-menu", null, "source-390x844@3~mobile-menu", "")]
    [InlineData(390, 844, "source-390x844@3~mobile-menu", null, "source-390x844@3~mobile-menu", "source-390x844@3~mobile-menu")]
    [InlineData(390, 844, "source-390x844@3~mobile-menu", "3", "source-390x844@3~mobile-menu", "source-390x844@3~mobile-menu")]
    [InlineData(390, 844, "source-390x844~mobile-menu", null, "source-391x844~mobile-menu", "")]
    [InlineData(390, 844, "source-390x844@2~mobile-menu", "2", "source-390x844@3~mobile-menu", "")]
    [InlineData(390, 844, "source-999x999", null, "source-390x844@3", "")]
    [InlineData(390, 844, "source-390x844@3", "2", "source-390x844@3", "")]
    [InlineData(390, 844, "source-390x844", "PRIVATE_BAD_SCALE_TOKEN", "source-390x844@3", "")]
    [InlineData(390, 844, null, null, "source-390x844@2|source-390x844@3", "")]
    [InlineData(390, 844, null, null, "source-390x844~mobile-menu|source-390x844~mobile-cart", "")]
    [InlineData(1440, 900, "source-1440x900", "1", "source-1440x900", "source-1440x900")]
    public void DesignConversionHumanApproval_TargetViewportResolverBindsCanonicalScaleAndFailsClosed(
        int width,
        int height,
        string? viewportId,
        string? deviceScaleFactor,
        string requiredViewportIdsText,
        string expected)
    {
        var metadata = new Dictionary<string, string>();
        if (!string.IsNullOrWhiteSpace(viewportId))
            metadata["viewport_id"] = viewportId;
        if (!string.IsNullOrWhiteSpace(deviceScaleFactor))
            metadata[DesignConversionRenderResolutionFactory.DeviceScaleFactorMetadataKey] = deviceScaleFactor;

        var actual = DesignConversionHumanApprovalService.ResolveTargetViewportId(
            width,
            height,
            metadata,
            requiredViewportIdsText.Split('|', StringSplitOptions.RemoveEmptyEntries));

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("viewportId", "mobile-menu", "source-390x844~mobile-menu", "source-390x844~mobile-menu")]
    [InlineData("viewportId", "MOBILE-MENU", "source-390x844~mobile-menu", "")]
    [InlineData("VIEWPORT_LABEL", "mobile-cart", "source-390x844~mobile-cart", "source-390x844~mobile-cart")]
    public void DesignConversionHumanApproval_TargetViewportResolverAcceptsViewportIdentityAliases(
        string metadataKey,
        string viewportIdentity,
        string requiredViewportId,
        string expected)
    {
        var actual = DesignConversionHumanApprovalService.ResolveTargetViewportId(
            390,
            844,
            new Dictionary<string, string>
            {
                [metadataKey] = viewportIdentity
            },
            [requiredViewportId]);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(390, 844, "3", 1170, 2532, true)]
    [InlineData(390, 844, "3", 1, 1, false)]
    [InlineData(390, 844, null, 390, 844, true)]
    [InlineData(390, 844, "PRIVATE_BAD_SCALE_TOKEN", 390, 844, false)]
    public void DesignConversionHumanApproval_PngProofDimensionsBindPhysicalScale(
        int logicalWidth,
        int logicalHeight,
        string? deviceScaleFactor,
        int pngWidth,
        int pngHeight,
        bool expected)
    {
        var metadata = new Dictionary<string, string>
        {
            ["viewport_width"] = logicalWidth.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["viewport_height"] = logicalHeight.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["png_width"] = pngWidth.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["png_height"] = pngHeight.ToString(System.Globalization.CultureInfo.InvariantCulture)
        };
        if (!string.IsNullOrWhiteSpace(deviceScaleFactor))
            metadata[DesignConversionRenderResolutionFactory.DeviceScaleFactorMetadataKey] = deviceScaleFactor;

        var actual = DesignConversionHumanApprovalService.MetadataDimensionsMatchPngProof(
            metadata,
            logicalWidth,
            logicalHeight);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void DesignConversionHumanApproval_PngProofDimensionsRejectMissingOrPartialPngPair(
        bool includePngWidth,
        bool includePngHeight)
    {
        var metadata = new Dictionary<string, string>
        {
            ["viewport_width"] = "390",
            ["viewport_height"] = "844"
        };
        if (includePngWidth)
            metadata["png_width"] = "390";
        if (includePngHeight)
            metadata["png_height"] = "844";

        var actual = DesignConversionHumanApprovalService.MetadataDimensionsMatchPngProof(
            metadata,
            expectedLogicalWidth: 390,
            expectedLogicalHeight: 844);

        Assert.False(actual);
    }

    [Fact]
    public async Task DesignConversionHumanApproval_ChallengeRejectsMismatchedTargetViewportMetadataAtTargetValidation()
    {
        var fixture = await CreateReadyFixtureAsync(
            nameof(DesignConversionHumanApproval_ChallengeRejectsMismatchedTargetViewportMetadataAtTargetValidation),
            assertSubmitSuccess: false,
            mobileViewportIdMetadata: "source-999x999");

        var records = await fixture.Api.ListHumanApprovalRecordsAsync();

        Assert.False(fixture.Submit.IsSuccess);
        Assert.Equal("RENDER_RESOLUTION_VIEWPORT_ID_CONFLICT", fixture.Submit.ErrorCode);
        Assert.Null(fixture.Capture.LastSuccess);
        Assert.True(records.IsSuccess, records.ErrorMessage);
        Assert.Empty(records.Value!);
    }

    [Fact]
    public async Task DesignConversionHumanApproval_ChallengeRejectsWrongScaledTargetPngDimensionsAtPngProofValidation()
    {
        var fixture = await CreateReadyFixtureAsync(
            nameof(DesignConversionHumanApproval_ChallengeRejectsWrongScaledTargetPngDimensionsAtPngProofValidation),
            evaluationMutation: evaluation => MutateSubmittedScreenshotMetadata(
                evaluation,
                screenshot => screenshot.Width == 360 && screenshot.Height == 800,
                metadata => MutateMetadata(
                    MutateMetadata(metadata, "png_width", "1"),
                    "png_height",
                    "1")));

        var rejected = await IssueChallengeAsync(fixture);

        Assert.False(rejected.IsSuccess);
        Assert.Equal("DESIGN_CONVERSION_APPROVAL_PNG_PROOF_INVALID", rejected.ErrorCode);
        Assert.Contains("target screenshot ref", rejected.ErrorMessage, StringComparison.Ordinal);
        Assert.DoesNotContain("source-360x800", rejected.ErrorMessage, StringComparison.Ordinal);
    }

    [Fact]
    public async Task DesignConversionHumanApproval_ChallengeRejectsMissingServerOwnedEvidenceAndHandlerMissingConfigSanitizes503()
    {
        var root = NewRoot(nameof(DesignConversionHumanApproval_ChallengeRejectsMissingServerOwnedEvidenceAndHandlerMissingConfigSanitizes503));
        var clock = new MutableClock();
        var artifactRoot = Path.Combine(root, "render-artifacts");
        var service = CreateService(artifactRoot);
        var visualCostLedger = new JsonlDesignConversionVisualCostLedger(Path.Combine(root, "visual-cost-ledger"));
        var runtime = new DesignConversionThinClientJobRuntime(
            new DesignConversionInternalServiceThinClientJobEvaluator(service),
            clock.UtcNow,
            artifactDb: new JsonlDesignConversionLocalArtifactDb(Path.Combine(root, "artifact-db")),
            visualCostLedger: visualCostLedger);
        var apiWithoutCapture = new DesignConversionModelServerApi(
            runtime,
            humanApprovalStore: new JsonlDesignConversionHumanApprovalStore(Path.Combine(root, "approval.jsonl")),
            utcNow: clock.UtcNow);
        var submit = await SubmitValidRequestAsync(
            apiWithoutCapture,
            "missing-server-owned-evidence",
            artifactRoot,
            includeProof: true);
        Assert.True(submit.IsSuccess, submit.ErrorMessage);

        var rejected = await apiWithoutCapture.IssueHumanApprovalChallengeAsync(
            new DesignConversionHumanApprovalChallengeIssueRequest(
                submit.Value!.JobId,
                Actor(TenantA, ActorA)));

        Assert.False(rejected.IsSuccess);
        Assert.Equal("DESIGN_CONVERSION_APPROVAL_SNAPSHOT_EVIDENCE_INCOMPLETE", rejected.ErrorCode);

        var handler = DesignConversionModelServerHumanApprovalHandler.CreateNotReady(
            "DESIGN_CONVERSION_HUMAN_APPROVAL_ROUTE_NOT_READY",
            "C:\\Users\\lubao\\private\\secret\\raw_provider_payload.json");
        var response = await handler.HandleAsync(new DesignConversionModelServerHttpRequest(
            Method: "POST",
            Path: DesignConversionModelServerHumanApprovalHandler.ChallengeEndpointPath,
            BodyJson: JsonSerializer.Serialize(new DesignConversionModelServerHumanApprovalChallengeHttpRequest(
                submit.Value.JobId,
                Actor(TenantA, ActorA)),
                DesignConversionModelServerHttpHandler.JsonOptions),
            ContentType: "application/json"));

        Assert.Equal(503, response.StatusCode);
        Assert.Contains("bounded_non_ready", response.BodyJson, StringComparison.Ordinal);
        Assert.Contains("human-approval-route-not-ready-reason:", response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain("C:\\Users\\lubao", response.BodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("raw_provider_payload", response.BodyJson, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task DesignConversionHumanApproval_ApprovalRejectsIncompleteViewportAcknowledgments()
    {
        var fixture = await CreateReadyFixtureAsync(nameof(DesignConversionHumanApproval_ApprovalRejectsIncompleteViewportAcknowledgments));
        var challenge = (await IssueChallengeAsync(fixture)).Value!;

        var rejected = await fixture.Api.ApproveHumanConversionAsync(new DesignConversionHumanApprovalDecisionRequest(
            challenge.ChallengeId,
            challenge.JobId,
            Actor(TenantA, ActorA),
            challenge.RequiredViewportIds.Take(1).ToArray(),
            OperationKey: "ack-missing-one-viewport",
            Reason: "missing mobile acknowledgment"));

        Assert.False(rejected.IsSuccess);
        Assert.Equal("DESIGN_CONVERSION_APPROVAL_VIEWPORT_ACK_MISMATCH", rejected.ErrorCode);
    }

    [Fact]
    public async Task DesignConversionHumanApproval_StaleSnapshotRevisionRejects()
    {
        var fixture = await CreateReadyFixtureAsync(nameof(DesignConversionHumanApproval_StaleSnapshotRevisionRejects));
        var challenge = (await IssueChallengeAsync(fixture)).Value!;
        var secondSubmit = await SubmitValidRequestAsync(
            fixture.Api,
            "stale-revision-second-job",
            fixture.ArtifactRoot,
            includeProof: true);
        Assert.True(secondSubmit.IsSuccess, secondSubmit.ErrorMessage);

        var rejected = await fixture.Api.ApproveHumanConversionAsync(new DesignConversionHumanApprovalDecisionRequest(
            challenge.ChallengeId,
            secondSubmit.Value!.JobId,
            Actor(TenantA, ActorA),
            challenge.RequiredViewportIds,
            OperationKey: "stale-revision-op",
            Reason: "attempt stale approval against another completed revision"));

        Assert.False(rejected.IsSuccess);
        Assert.Equal("DESIGN_CONVERSION_APPROVAL_SNAPSHOT_STALE", rejected.ErrorCode);
    }

    [Fact]
    public async Task DesignConversionHumanApproval_CrossTenantActorRejects()
    {
        var fixture = await CreateReadyFixtureAsync(nameof(DesignConversionHumanApproval_CrossTenantActorRejects));
        var challenge = (await IssueChallengeAsync(fixture)).Value!;

        var rejected = await fixture.Api.ApproveHumanConversionAsync(new DesignConversionHumanApprovalDecisionRequest(
            challenge.ChallengeId,
            challenge.JobId,
            Actor(TenantB, ActorB),
            challenge.RequiredViewportIds,
            OperationKey: "cross-tenant-op",
            Reason: "wrong tenant attempts approval"));

        Assert.False(rejected.IsSuccess);
        Assert.Equal("DESIGN_CONVERSION_APPROVAL_AUTHORIZATION_DENIED", rejected.ErrorCode);
    }

    [Fact]
    public async Task DesignConversionHumanApproval_ExpiredChallengeRejects()
    {
        var fixture = await CreateReadyFixtureAsync(nameof(DesignConversionHumanApproval_ExpiredChallengeRejects));
        var challenge = await fixture.Api.IssueHumanApprovalChallengeAsync(
            new DesignConversionHumanApprovalChallengeIssueRequest(
                fixture.Submit.Value!.JobId,
                Actor(TenantA, ActorA),
                TimeSpan.FromSeconds(1)));
        Assert.True(challenge.IsSuccess, challenge.ErrorMessage);
        fixture.Clock.Advance(TimeSpan.FromSeconds(2));

        var rejected = await ApproveAsync(fixture, challenge.Value!, "expired-challenge-op");

        Assert.False(rejected.IsSuccess);
        Assert.Equal("DESIGN_CONVERSION_APPROVAL_CHALLENGE_EXPIRED", rejected.ErrorCode);
    }

    [Fact]
    public async Task DesignConversionHumanApproval_ReplayAndConcurrentDoubleSubmitExactlyOneSuccess()
    {
        var fixture = await CreateReadyFixtureAsync(nameof(DesignConversionHumanApproval_ReplayAndConcurrentDoubleSubmitExactlyOneSuccess));
        var challenge = (await IssueChallengeAsync(fixture)).Value!;

        var attempts = await Task.WhenAll(
            ApproveAsync(fixture, challenge, "concurrent-double-submit-op"),
            ApproveAsync(fixture, challenge, "concurrent-double-submit-op"));
        var replay = await ApproveAsync(fixture, challenge, "concurrent-replay-new-op");
        var records = await fixture.Api.ListHumanApprovalRecordsAsync();

        Assert.Equal(1, attempts.Count(result => result.IsSuccess));
        Assert.Equal(1, attempts.Count(result => !result.IsSuccess));
        Assert.False(replay.IsSuccess);
        Assert.Equal("DESIGN_CONVERSION_APPROVAL_CHALLENGE_REPLAYED", replay.ErrorCode);
        Assert.True(records.IsSuccess, records.ErrorMessage);
        Assert.Single(records.Value!);
    }

    [Fact]
    public async Task DesignConversionHumanApproval_ForgedArtifactProofRefRejects()
    {
        var fixture = await CreateReadyFixtureAsync(
            nameof(DesignConversionHumanApproval_ForgedArtifactProofRefRejects),
            evaluationMutation: evaluation => MutateSubmittedScreenshotMetadata(
                evaluation,
                _ => true,
                metadata =>
                {
                    var mutated = new Dictionary<string, string>(metadata, StringComparer.Ordinal)
                    {
                        ["media_type"] = "application/octet-stream",
                        ["content_type"] = "application/octet-stream"
                    };
                    mutated.Remove("png_width");
                    mutated.Remove("png_height");
                    return mutated;
                }));

        var rejected = await fixture.Api.IssueHumanApprovalChallengeAsync(
            new DesignConversionHumanApprovalChallengeIssueRequest(
                fixture.Submit.Value!.JobId,
                Actor(TenantA, ActorA)));

        Assert.False(rejected.IsSuccess);
        Assert.Equal("DESIGN_CONVERSION_APPROVAL_PNG_PROOF_INVALID", rejected.ErrorCode);
    }

    [Fact]
    public async Task DesignConversionHumanApproval_ChallengeRejectsComparisonProofWithMissingTrustKeys()
    {
        var fixture = await CreateReadyFixtureAsync(
            nameof(DesignConversionHumanApproval_ChallengeRejectsComparisonProofWithMissingTrustKeys),
            evaluationMutation: evaluation => MutateComparisonMetadata(
                MutateComparisonMetadata(evaluation, ProductionTrustedKey, null),
                ApprovalEvidenceAllowedKey,
                null));

        var rejected = await IssueChallengeAsync(fixture);

        Assert.False(rejected.IsSuccess);
        Assert.Equal("DESIGN_CONVERSION_APPROVAL_SYNTHETIC_RENDER_PROOF_REJECTED", rejected.ErrorCode);
        Assert.Contains("scaffold, synthetic, or metadata-only proof", rejected.ErrorMessage, StringComparison.Ordinal);
    }

    [Fact]
    public async Task DesignConversionHumanApproval_ChallengeRejectsComparisonProofWithExplicitFalseTrust()
    {
        var fixture = await CreateReadyFixtureAsync(
            nameof(DesignConversionHumanApproval_ChallengeRejectsComparisonProofWithExplicitFalseTrust),
            evaluationMutation: evaluation => MutateComparisonMetadata(
                MutateComparisonMetadata(evaluation, ProductionTrustedKey, bool.FalseString),
                ApprovalEvidenceAllowedKey,
                bool.FalseString));

        var rejected = await IssueChallengeAsync(fixture);

        Assert.False(rejected.IsSuccess);
        Assert.Equal("DESIGN_CONVERSION_APPROVAL_SYNTHETIC_RENDER_PROOF_REJECTED", rejected.ErrorCode);
        Assert.Contains("scaffold, synthetic, or metadata-only proof", rejected.ErrorMessage, StringComparison.Ordinal);
    }

    [Fact]
    public async Task DesignConversionHumanApproval_ChallengeRejectsUnknownComparisonProviderForgingTrustKeys()
    {
        var fixture = await CreateReadyFixtureAsync(
            nameof(DesignConversionHumanApproval_ChallengeRejectsUnknownComparisonProviderForgingTrustKeys),
            evaluationMutation: evaluation => MutateComparisonMetadata(
                MutateComparisonMetadata(
                    evaluation,
                    "provider_kind",
                    "unknown-forged-comparison-provider"),
                "comparison_provider_kind",
                "unknown-forged-comparison-provider"));

        var rejected = await IssueChallengeAsync(fixture);

        Assert.False(rejected.IsSuccess);
        Assert.Equal("DESIGN_CONVERSION_APPROVAL_SYNTHETIC_RENDER_PROOF_REJECTED", rejected.ErrorCode);
        Assert.Contains("scaffold, synthetic, or metadata-only proof", rejected.ErrorMessage, StringComparison.Ordinal);
    }

    [Fact]
    public async Task DesignConversionHumanApproval_ServerOwnedLocalPngComparisonMetadataSurvivesInternalEvaluation()
    {
        var root = NewRoot(nameof(DesignConversionHumanApproval_ServerOwnedLocalPngComparisonMetadataSurvivesInternalEvaluation));
        var artifactRoot = Path.Combine(root, "render-artifacts");
        var clock = new MutableClock();
        var visualCostLedger = new JsonlDesignConversionVisualCostLedger(Path.Combine(root, "visual-cost-ledger"));
        var evaluator = new CapturingThinClientJobEvaluator(
            new DesignConversionInternalServiceThinClientJobEvaluator(CreateService(artifactRoot)));
        var runtime = new DesignConversionThinClientJobRuntime(
            evaluator,
            clock.UtcNow,
            artifactDb: new JsonlDesignConversionLocalArtifactDb(Path.Combine(root, "artifact-db")),
            visualCostLedger: visualCostLedger);
        var api = new DesignConversionModelServerApi(
            runtime,
            humanApprovalStore: new JsonlDesignConversionHumanApprovalStore(Path.Combine(root, "approval.jsonl")),
            utcNow: clock.UtcNow);

        var submit = await SubmitValidRequestAsync(
            api,
            "server-owned-local-png-metadata",
            artifactRoot,
            includeProof: true);

        Assert.True(submit.IsSuccess, submit.ErrorMessage);
        Assert.NotNull(evaluator.LastSuccess);
        Assert.Equal(4, evaluator.LastSuccess!.RenderResults.Count);
        foreach (var render in evaluator.LastSuccess.RenderResults)
        {
            var inspected = DesignConversionBrowserPngEvidenceInspector.Inspect(
                new Uri(render.Image.Uri).LocalPath);
            Assert.True(inspected.IsSuccess, inspected.ErrorMessage);
            Assert.True(inspected.Value!.HasNonBlankPixels);
            Assert.Equal(render.Image.Resolution.Width, inspected.Value.PngWidth);
            Assert.Equal(render.Image.Resolution.Height, inspected.Value.PngHeight);
        }

        Assert.Equal(4, evaluator.LastSuccess!.ComparisonArtifacts.Count);
        foreach (var comparison in evaluator.LastSuccess.ComparisonArtifacts)
        {
            var metadata = comparison.Metadata;
            Assert.Equal(bool.TrueString, metadata["comparison_evidence_server_canonicalized"]);
            Assert.Equal(bool.TrueString, metadata[ProductionTrustedKey]);
            Assert.Equal(bool.TrueString, metadata[ApprovalEvidenceAllowedKey]);
            Assert.Equal(
                "server_owned_local_png_byte_inspected_comparison_evidence",
                metadata["comparison_evidence_trust_classification"]);
            Assert.Equal(bool.FalseString, metadata["product_ready"]);
            Assert.Equal(bool.FalseString, metadata["dpo_eligible"]);
            Assert.Equal(bool.FalseString, metadata["model_weights_published"]);
            Assert.Equal(bool.FalseString, metadata["model_training_verified"]);
        }
    }

    [Fact]
    public async Task DesignConversionHumanApproval_RecordFlagsFalseAndNoRawPayload()
    {
        var fixture = await CreateReadyFixtureAsync(
            nameof(DesignConversionHumanApproval_RecordFlagsFalseAndNoRawPayload),
            privatePayload: true);
        var challenge = (await IssueChallengeAsync(fixture)).Value!;
        var approved = await ApproveAsync(fixture, challenge, "record-flags-op", "human accepted SECRET_TOKEN_123 raw_provider_payload");
        Assert.True(approved.IsSuccess, approved.ErrorMessage);

        var records = await fixture.Api.ListHumanApprovalRecordsAsync();
        Assert.True(records.IsSuccess, records.ErrorMessage);
        var record = Assert.Single(records.Value!);
        var serialized = JsonSerializer.Serialize(record, DesignConversionModelServerHttpHandler.JsonOptions);

        Assert.False(record.ProductReady);
        Assert.False(record.DpoEligible);
        Assert.False(record.ModelWeightsPublished);
        Assert.False(record.TrainedOrLearnedProvenance);
        Assert.Equal(bool.FalseString, record.BoundaryMetadata["raw_html_included"]);
        Assert.Equal(bool.FalseString, record.BoundaryMetadata["raw_css_included"]);
        Assert.Equal(bool.FalseString, record.BoundaryMetadata["screenshot_bytes_included"]);
        Assert.DoesNotContain("<main", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("PRIVATE_TOKEN", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SECRET_TOKEN_123", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("raw_provider_payload", serialized, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task DesignConversionHumanApproval_FreshStoreServiceReconstructionReadsApprovalAndRejectsReusedChallengeOp()
    {
        var fixture = await CreateReadyFixtureAsync(nameof(DesignConversionHumanApproval_FreshStoreServiceReconstructionReadsApprovalAndRejectsReusedChallengeOp));
        var challenge = (await IssueChallengeAsync(fixture)).Value!;
        var approved = await ApproveAsync(fixture, challenge, "durable-reconstruction-op");
        Assert.True(approved.IsSuccess, approved.ErrorMessage);

        var reloadedStore = new JsonlDesignConversionHumanApprovalStore(fixture.JournalPath);
        var reloadedService = new DesignConversionHumanApprovalService(reloadedStore, fixture.Clock.UtcNow);
        var records = await reloadedService.ListApprovalRecordsAsync();
        Assert.True(records.IsSuccess, records.ErrorMessage);
        var record = Assert.Single(records.Value!);

        var reusedChallenge = await reloadedStore.AppendApprovalIfOperationKeyUnusedAsync(record with
        {
            ApprovalId = record.ApprovalId + ":reused-challenge"
        });
        var reusedOperation = await reloadedStore.AppendApprovalIfOperationKeyUnusedAsync(record with
        {
            ApprovalId = record.ApprovalId + ":reused-operation",
            ChallengeId = record.ChallengeId + ":new-challenge"
        });

        Assert.False(reusedChallenge.IsSuccess);
        Assert.Equal("DESIGN_CONVERSION_APPROVAL_CHALLENGE_REPLAYED", reusedChallenge.ErrorCode);
        Assert.False(reusedOperation.IsSuccess);
        Assert.Equal("DESIGN_CONVERSION_APPROVAL_OPERATION_REPLAYED", reusedOperation.ErrorCode);
    }

    private static async Task<ApprovalFixture> CreateReadyFixtureAsync(
        string name,
        bool includeProof = true,
        bool privatePayload = false,
        bool assertSubmitSuccess = true,
        string? mobileViewportIdMetadata = null,
        string? mobileDeviceScaleFactorMetadata = null,
        int? mobilePngWidthMetadata = null,
        int? mobilePngHeightMetadata = null,
        IDesignConversionComparisonProvider? comparisonProvider = null,
        Func<string, IDesignConversionComparisonProvider>? comparisonProviderFactory = null,
        bool useArtifactUris = false,
        Func<DesignConversionInternalEvaluationResult, DesignConversionInternalEvaluationResult>? evaluationMutation = null)
    {
        var root = NewRoot(name);
        var artifactRoot = Path.Combine(root, "render-artifacts");
        var clock = new MutableClock();
        var journalPath = Path.Combine(root, "human-approval.jsonl");
        var visualCostLedger = new JsonlDesignConversionVisualCostLedger(Path.Combine(root, "visual-cost-ledger"));
        var store = new JsonlDesignConversionHumanApprovalStore(journalPath);
        var service = CreateService(
            artifactRoot,
            comparisonProviderFactory?.Invoke(artifactRoot) ?? comparisonProvider);
        IDesignConversionThinClientJobEvaluator evaluator =
            new DesignConversionInternalServiceThinClientJobEvaluator(service);
        if (evaluationMutation is not null)
            evaluator = new MutatingThinClientJobEvaluator(evaluator, evaluationMutation);

        var capture = new CapturingThinClientJobEvaluator(evaluator);
        var api = new DesignConversionModelServerApi(
            capture,
            clock.UtcNow,
            artifactDb: new JsonlDesignConversionLocalArtifactDb(Path.Combine(root, "artifact-db")),
            humanApprovalStore: store,
            visualCostLedger: visualCostLedger);
        var submit = await SubmitValidRequestAsync(
            api,
            name,
            artifactRoot,
            includeProof,
            privatePayload,
            mobileViewportIdMetadata,
            mobileDeviceScaleFactorMetadata,
            mobilePngWidthMetadata,
            mobilePngHeightMetadata,
            useArtifactUris);
        if (assertSubmitSuccess)
            Assert.True(submit.IsSuccess, submit.ErrorMessage);

        return new ApprovalFixture(root, artifactRoot, journalPath, visualCostLedger, clock, api, submit, capture);
    }

    private static async Task AssertVisualCostLedgerDurablyRecordedAsync(ApprovalFixture fixture)
    {
        var records = await fixture.VisualCostLedger.ListRecordsAsync();
        Assert.True(records.IsSuccess, records.ErrorMessage);
        var ledgerRecord = Assert.Single(records.Value!);
        Assert.Equal(JsonlDesignConversionVisualCostLedger.LedgerKind, ledgerRecord.LedgerKind);
        Assert.Equal(JsonlDesignConversionVisualCostLedger.SchemaVersion, ledgerRecord.SchemaVersion);
        Assert.True(JsonlDesignConversionVisualCostLedger.IsCanonicalRecordId(ledgerRecord.RecordId));
        Assert.Equal(
            JsonlDesignConversionVisualCostLedger.BuildCanonicalEvaluationJobRef(fixture.Submit.Value!.JobId),
            ledgerRecord.EvaluationJobRef);
        Assert.Equal(
            JsonlDesignConversionVisualCostLedger.BuildCanonicalSubmittedRequestRef(fixture.Submit.Value.RequestId),
            ledgerRecord.SubmittedRequestRef);
        Assert.True(ledgerRecord.MeasurementComplete);
        Assert.Equal(0, ledgerRecord.MissingMeasurementCount);
        Assert.Equal(JsonlDesignConversionVisualCostLedger.UtcCalendarMonthPeriodKind, ledgerRecord.PeriodKind);

        var serialized = JsonSerializer.Serialize(ledgerRecord, DesignConversionModelServerHttpHandler.JsonOptions);
        Assert.DoesNotContain("<main", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("<style", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("PRIVATE_TOKEN", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("raw_provider_payload", serialized, StringComparison.OrdinalIgnoreCase);
    }

    private static DesignConversionInternalService CreateService(
        string artifactRoot,
        IDesignConversionComparisonProvider? comparisonProvider = null)
        => new(
            new BrowserBackedDesignConversionRenderProvider(new ApprovalBrowserAdapter(artifactRoot)),
            comparisonProvider ?? new ServerOwnedLocalPngComparisonProvider(artifactRoot),
            new DefaultLocalDesignConversionVisualJudge(),
            routingFlow: null);

    private static async Task<OperationResult<DesignConversionModelServerApiResult>> SubmitValidRequestAsync(
        DesignConversionModelServerApi api,
        string name,
        string artifactRoot,
        bool includeProof = true,
        bool privatePayload = false,
        string? mobileViewportIdMetadata = null,
        string? mobileDeviceScaleFactorMetadata = null,
        int? mobilePngWidthMetadata = null,
        int? mobilePngHeightMetadata = null,
        bool useArtifactUris = false)
        => await api.SubmitAndEvaluateAsync(CreateRequest(
            name,
            artifactRoot,
            includeProof,
            privatePayload,
            mobileViewportIdMetadata,
            mobileDeviceScaleFactorMetadata,
            mobilePngWidthMetadata,
            mobilePngHeightMetadata,
            useArtifactUris));

    private static async Task<OperationResult<DesignConversionHumanApprovalChallenge>> IssueChallengeAsync(
        ApprovalFixture fixture)
        => await fixture.Api.IssueHumanApprovalChallengeAsync(new DesignConversionHumanApprovalChallengeIssueRequest(
            fixture.Submit.Value!.JobId,
            Actor(TenantA, ActorA)));

    private static async Task<OperationResult<DesignConversionHumanApprovalResult>> ApproveAsync(
        ApprovalFixture fixture,
        DesignConversionHumanApprovalChallenge challenge,
        string operationKey = "human-approval-op",
        string reason = "human approved all required viewport refs")
        => await fixture.Api.ApproveHumanConversionAsync(new DesignConversionHumanApprovalDecisionRequest(
            challenge.ChallengeId,
            challenge.JobId,
            Actor(TenantA, ActorA),
            challenge.RequiredViewportIds,
            operationKey,
            reason));

    private static DesignConversionHumanApprovalActor Actor(string tenantId, string actorId)
        => new(
            TenantId: tenantId,
            ActorId: actorId,
            Roles: [DesignConversionHumanApprovalService.RequiredApproverRole],
            Authenticated: true);

    private static void AssertTrustedExternalVisualAiComparisonReviewRef(
        DesignConversionHumanApprovalSnapshotRef reference)
    {
        Assert.Equal("comparison", reference.RefKind);
        Assert.Equal(ExternalVisualAiComparisonReviewUriPrefix + ShortDigest(reference.Sha256), reference.UriRef);
        Assert.StartsWith("sha256:", reference.Sha256, StringComparison.Ordinal);
        Assert.Equal(71, reference.Sha256.Length);
        Assert.Equal("application/ref", reference.Metadata["media_type"]);
        Assert.Equal("application/ref", reference.Metadata["proof_media_type"]);
        Assert.Equal("external_visual_ai_comparison_receipt_refs_only", reference.Metadata["human_review_reference_kind"]);
        Assert.Equal(bool.TrueString, reference.Metadata["server_owned_refs_only_human_review_reference"]);
        Assert.Equal(bool.TrueString, reference.Metadata["refs_only_comparison_artifact_reference"]);
        Assert.Equal(bool.FalseString, reference.Metadata["diff_png_fabricated"]);
        Assert.Equal(bool.FalseString, reference.Metadata["raw_html_included"]);
        Assert.Equal(bool.FalseString, reference.Metadata["raw_css_included"]);
        Assert.Equal(bool.FalseString, reference.Metadata["raw_design_tree_included"]);
        Assert.Equal(DesignConversionExternalVisualAiComparisonProvider.ProviderKind, reference.Metadata["provider_kind"]);
        Assert.Equal(DesignConversionExternalVisualAiComparisonProvider.ProviderKind, reference.Metadata["comparison_provider_kind"]);
        Assert.Equal(bool.TrueString, reference.Metadata[ProductionTrustedKey]);
        Assert.Equal(bool.TrueString, reference.Metadata[ApprovalEvidenceAllowedKey]);
        Assert.Equal(bool.TrueString, reference.Metadata[DesignConversionComparisonEvidenceTrustBoundary.ServerCanonicalizedKey]);
        Assert.Equal(
            DesignConversionComparisonEvidenceTrustBoundary.ServerOwnedExternalVisualAiReceiptValidatedClassification,
            reference.Metadata[DesignConversionComparisonEvidenceTrustBoundary.TrustClassificationKey]);
        Assert.Equal(bool.TrueString, reference.Metadata["server_owned_external_visual_ai_receipt_validated"]);
        Assert.Equal("valid", reference.Metadata["server_owned_external_visual_ai_receipt_validation_reason"]);
        Assert.Equal(bool.TrueString, reference.Metadata["visual_comparison_proven"]);
        Assert.Equal(bool.TrueString, reference.Metadata["network_call_executed"]);
        Assert.Equal(bool.TrueString, reference.Metadata["real_external_provider_executed"]);
        Assert.Equal(bool.TrueString, reference.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptKey]);
        Assert.Equal(
            ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVersion,
            reference.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVersionKey]);
        Assert.Equal(bool.TrueString, reference.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptExternalCallExecutedKey]);
        Assert.Equal(bool.TrueString, reference.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseIdentityVerifiedKey]);
        Assert.Equal(bool.TrueString, reference.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseSchemaVerifiedKey]);
        Assert.NotEmpty(reference.Metadata["provider_name"]);
        Assert.NotEmpty(reference.Metadata["provider_model"]);
        Assert.NotEmpty(reference.Metadata["provider_endpoint_host"]);
        Assert.Equal(reference.Metadata["provider_name"], reference.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptProviderNameKey]);
        Assert.Equal(reference.Metadata["provider_model"], reference.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptProviderModelKey]);
        Assert.Equal(reference.Metadata["provider_endpoint_host"], reference.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptEndpointHostKey]);
        Assert.NotEmpty(reference.Metadata["server_bound_job_id"]);
        Assert.NotEmpty(reference.Metadata["server_bound_request_id"]);
        Assert.NotEmpty(reference.Metadata["server_bound_comparison_request_id"]);
        Assert.Equal(reference.Metadata["server_bound_job_id"], reference.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptJobIdKey]);
        Assert.Equal(reference.Metadata["server_bound_request_id"], reference.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptRequestIdKey]);
        Assert.Equal(reference.Metadata["server_bound_request_id"], reference.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptRootRequestIdKey]);
        Assert.Equal(reference.Metadata["server_bound_comparison_request_id"], reference.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptJudgeRequestIdKey]);
        Assert.Equal(reference.Metadata["server_bound_comparison_request_id"], reference.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptProviderRequestIdKey]);
        Assert.Equal(reference.Metadata["server_bound_comparison_request_id"], reference.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptComparisonRequestIdKey]);
        Assert.StartsWith("sha256:", reference.Metadata["expected_image_sha256"], StringComparison.Ordinal);
        Assert.StartsWith("sha256:", reference.Metadata["rendered_image_sha256"], StringComparison.Ordinal);
        Assert.Equal(reference.Metadata["comparison_request_id"], reference.Metadata["server_bound_comparison_request_id"]);
        Assert.Equal(reference.Metadata["expected_image_sha256"], reference.Metadata["server_bound_expected_sha256"]);
        Assert.Equal(reference.Metadata["rendered_image_sha256"], reference.Metadata["server_bound_rendered_sha256"]);
        Assert.NotEmpty(reference.Metadata["external_visual_ai_receipt_id"]);
        Assert.NotEmpty(reference.Metadata["external_visual_ai_receipt_visual_result_id"]);
        Assert.Equal(reference.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptIdKey], reference.Metadata["external_visual_ai_receipt_id"]);
        Assert.Equal(reference.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVisualResultIdKey], reference.Metadata["external_visual_ai_receipt_visual_result_id"]);
        Assert.Equal(reference.RefId, reference.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptComparisonArtifactIdKey]);
        Assert.Equal(reference.RefId, reference.Metadata["external_visual_ai_receipt_comparison_artifact_id"]);
        Assert.Equal(reference.RefId, reference.Metadata["comparison_artifact_receipt_id"]);
        Assert.DoesNotContain("<main", JsonSerializer.Serialize(reference), StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("<style", JsonSerializer.Serialize(reference), StringComparison.OrdinalIgnoreCase);
    }

    private static DesignConversionInternalEvaluationResult MutateComparisonMetadata(
        DesignConversionInternalEvaluationResult evaluation,
        string metadataKey,
        string? replacementValue)
        => evaluation with
        {
            ComparisonArtifacts = evaluation.ComparisonArtifacts
                .Select(artifact => artifact with
                {
                    Metadata = MutateMetadata(artifact.Metadata, metadataKey, replacementValue)
                })
                .ToArray()
        };

    private static DesignConversionInternalEvaluationResult MutateSubmittedScreenshotMetadata(
        DesignConversionInternalEvaluationResult evaluation,
        Func<DesignRenderArtifact, bool> predicate,
        Func<IReadOnlyDictionary<string, string>, IReadOnlyDictionary<string, string>> mutate)
        => evaluation with
        {
            SubmittedRequest = evaluation.SubmittedRequest with
            {
                Screenshots = evaluation.SubmittedRequest.Screenshots
                    .Select(screenshot => predicate(screenshot)
                        ? screenshot with { Metadata = mutate(screenshot.Metadata) }
                        : screenshot)
                    .ToArray()
            }
        };

    private static IReadOnlyDictionary<string, string> MutateMetadata(
        IReadOnlyDictionary<string, string> metadata,
        string metadataKey,
        string? replacementValue)
    {
        var mutated = new Dictionary<string, string>(metadata, StringComparer.Ordinal);
        if (replacementValue is null)
            mutated.Remove(metadataKey);
        else
            mutated[metadataKey] = replacementValue;
        return mutated;
    }

    private static string ShortDigest(string digest)
    {
        var clean = digest.StartsWith("sha256:", StringComparison.OrdinalIgnoreCase)
            ? digest["sha256:".Length..]
            : digest;
        return clean.Length <= 16 ? clean : clean[..16];
    }

    private static DesignConversionModelServerApiRequest CreateRequest(
        string name,
        string artifactRoot,
        bool includeProof,
        bool privatePayload,
        string? mobileViewportIdMetadata = null,
        string? mobileDeviceScaleFactorMetadata = null,
        int? mobilePngWidthMetadata = null,
        int? mobilePngHeightMetadata = null,
        bool useArtifactUris = false)
    {
        var safeName = SafeId(name);
        var requestId = $"human-approval-{ShortDigest(ShaRef(name))}-{Guid.NewGuid():N}";
        var html = privatePayload
            ? "<main data-secret=\"PRIVATE_TOKEN\"><h1>Approval private dashboard</h1></main>"
            : "<main class=\"approval-screen\"><h1>Approval dashboard</h1></main>";
        var css = privatePayload
            ? ".approval-screen{color:#111827}.secret{content:\"PRIVATE_TOKEN\";}"
            : ".approval-screen{display:grid;gap:16px;color:#111827;background:#f8fafc;}";
        return new DesignConversionModelServerApiRequest(
            RequestId: requestId,
            SourceKind: DesignConversionSourceKind.Figma,
            DesignTree: new DesignConversionModelServerPayloadRef(
                RefId: $"tree-{safeName}",
                Kind: "figma-json",
                Sha256: ShaRef($"tree-{safeName}"),
                Payload: privatePayload
                    ? "{\"type\":\"FRAME\",\"name\":\"PRIVATE_TOKEN raw_provider_payload\"}"
                    : "{\"type\":\"FRAME\",\"name\":\"approval-safe\"}",
                Format: "figma-json"),
            OriginalTree: new DesignConversionModelServerPayloadRef(
                RefId: $"original-tree-{safeName}",
                Kind: "figma-json",
                Sha256: ShaRef($"original-tree-{safeName}"),
                Payload: "{\"type\":\"FRAME\",\"name\":\"approval-original\"}",
                Format: "figma-json"),
            Html: html,
            Css: css,
            Screenshots:
            [
                Artifact(
                    artifactRoot,
                    safeName,
                    "mobile",
                    360,
                    800,
                    includeProof,
                    requestId,
                    html,
                    css,
                    mobileViewportIdMetadata,
                    mobileDeviceScaleFactorMetadata,
                    mobilePngWidthMetadata,
                    mobilePngHeightMetadata,
                    useArtifactUris),
                Artifact(artifactRoot, safeName, "tablet", 768, 1024, includeProof, requestId, html, css, useArtifactUri: useArtifactUris),
                Artifact(artifactRoot, safeName, "laptop", 1280, 720, includeProof, requestId, html, css, useArtifactUri: useArtifactUris),
                Artifact(artifactRoot, safeName, "desktop", 1920, 1080, includeProof, requestId, html, css, useArtifactUri: useArtifactUris)
            ],
            DesignRenders: [],
            Budget: new DesignConversionModelServerBudget(
                UserId: "approval-test-user",
                Tier: "bounded-test",
                MaxImprovementCandidates: 1,
                MaxRenderQaRuns: 4,
                BudgetUsd: 0.25m),
            Metadata: new Dictionary<string, string>
            {
                ["approval_test"] = safeName,
                ["raw_payload_expected_in_response"] = bool.FalseString
            });
    }

    private static DesignConversionModelServerArtifactRef Artifact(
        string artifactRoot,
        string safeName,
        string viewport,
        int width,
        int height,
        bool includeProof,
        string requestId,
        string html,
        string css,
        string? viewportIdMetadata = null,
        string? deviceScaleFactorMetadata = null,
        int? pngWidthMetadata = null,
        int? pngHeightMetadata = null,
        bool useArtifactUri = false)
    {
        var pngWidth = pngWidthMetadata ?? width;
        var pngHeight = pngHeightMetadata ?? height;
        var metadata = includeProof
            ? new Dictionary<string, string>
            {
                ["media_type"] = "image/png",
                ["content_type"] = "image/png",
                ["png_width"] = pngWidth.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["png_height"] = pngHeight.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["viewport_width"] = width.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["viewport_height"] = height.ToString(System.Globalization.CultureInfo.InvariantCulture)
            }
            : new Dictionary<string, string>
            {
                ["media_type"] = "application/octet-stream"
            };
        if (!string.IsNullOrWhiteSpace(viewportIdMetadata))
            metadata["viewport_id"] = viewportIdMetadata;
        if (!string.IsNullOrWhiteSpace(deviceScaleFactorMetadata))
            metadata[DesignConversionRenderResolutionFactory.DeviceScaleFactorMetadataKey] = deviceScaleFactorMetadata;

        var sha = WriteTargetPng(
            artifactRoot,
            safeName,
            viewport,
            requestId,
            html,
            css,
            width,
            height,
            deviceScaleFactorMetadata);

        return new DesignConversionModelServerArtifactRef(
            ArtifactId: $"approval-{safeName}-{viewport}",
            Kind: "screenshot",
            Width: width,
            Height: height,
            Uri: useArtifactUri
                ? $"artifact://design-conversion-human-approval/{safeName}/{viewport}.png"
                : $"sha256:{sha}",
            Sha256: $"sha256:{sha}",
            Metadata: metadata);
    }

    private static string WriteTargetPng(
        string artifactRoot,
        string safeName,
        string viewport,
        string requestId,
        string html,
        string css,
        int width,
        int height,
        string? deviceScaleFactorMetadata)
    {
        var deviceScaleFactor = ResolveDeviceScaleFactor(deviceScaleFactorMetadata);
        var baseResolutionId = $"source-{width}x{height}";
        var resolution = new RenderQaResolution(
            deviceScaleFactor == 1.0
                ? baseResolutionId
                : string.Create(
                    System.Globalization.CultureInfo.InvariantCulture,
                    $"{baseResolutionId}@{deviceScaleFactor:R}"),
            width,
            height,
            deviceScaleFactor);
        var provider = new BrowserBackedDesignConversionRenderProvider(new ApprovalBrowserAdapter(artifactRoot));
        var render = provider.RenderAsync(new DesignConversionRenderRequest(
                RenderRequestId: $"target-{safeName}-{viewport}-browser-png",
                RequestId: requestId,
                Html: html,
                Css: css,
                Resolution: resolution,
                Metadata: new Dictionary<string, string>()))
            .GetAwaiter()
            .GetResult();
        if (!render.IsSuccess)
            throw new InvalidOperationException(render.ErrorMessage);

        return render.Value!.Image.Sha256["sha256:".Length..];
    }

    private static double ResolveDeviceScaleFactor(string? deviceScaleFactorMetadata)
        => double.TryParse(
                deviceScaleFactorMetadata,
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture,
                out var parsed) &&
           double.IsFinite(parsed) &&
           parsed > 0
            ? parsed
            : 1.0;

    private static string NewRoot(string name)
    {
        var configuredRoot = Environment.GetEnvironmentVariable("XIIGEN_HUMAN_APPROVAL_TEST_ROOT");
        var root = string.IsNullOrWhiteSpace(configuredRoot)
            ? Path.Combine(Path.GetTempPath(), "xiigen-human-approval")
            : configuredRoot;
        var path = Path.Combine(root, SafeId(name), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private static string SafeId(string value)
        => new(value
            .Select(character => char.IsAsciiLetterOrDigit(character) ? char.ToLowerInvariant(character) : '-')
            .ToArray());

    private static string ShaRef(string value)
        => "sha256:" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    private sealed record ApprovalFixture(
        string Root,
        string ArtifactRoot,
        string JournalPath,
        JsonlDesignConversionVisualCostLedger VisualCostLedger,
        MutableClock Clock,
        DesignConversionModelServerApi Api,
        OperationResult<DesignConversionModelServerApiResult> Submit,
        CapturingThinClientJobEvaluator Capture);

    private sealed class MutableClock
    {
        private DateTimeOffset _now = DateTimeOffset.Parse("2026-07-10T10:00:00Z");

        public DateTimeOffset UtcNow() => _now;

        public void Advance(TimeSpan duration) => _now = _now.Add(duration);
    }

    private sealed class PassingComparisonProvider(
        IReadOnlyDictionary<string, string>? metadataOverrides = null) : IDesignConversionComparisonProvider
    {
        public Task<OperationResult<DesignConversionQaComparisonArtifact>> CompareAsync(
            DesignConversionQaComparisonRequest request,
            CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
                return Task.FromResult(OperationResult<DesignConversionQaComparisonArtifact>.Fail(
                    "Approval comparison cancelled.",
                    "CANCELLED"));

            var diffSha = ShaRef($"diff-{request.ComparisonRequestId}");
            var verdict = new VisualVerdict(
                VerdictId: $"{request.ComparisonRequestId}:approval-verdict",
                Score: 0.96,
                Defects: [],
                EvidenceRefs: [$"approval-comparison:{request.ComparisonRequestId}", "approval-comparison:refs-only"],
                Passed: true);
            var artifact = new DesignConversionQaComparisonArtifact(
                ArtifactId: $"{request.ComparisonRequestId}:approval-comparison-artifact",
                ComparisonRequestId: request.ComparisonRequestId,
                Expected: request.Expected,
                Actual: request.Actual,
                Verdict: verdict,
                DiffImageUri: $"artifact://design-conversion-human-approval/diff/{Uri.EscapeDataString(request.ComparisonRequestId)}.png",
                DiffImageSha256: diffSha,
                Metrics: new Dictionary<string, double> { ["visual_distance"] = 0.04 },
                EvidenceRefs: [$"approval-comparison-artifact:{request.ComparisonRequestId}"])
            {
                Metadata = new Dictionary<string, string>
                    {
                        ["provider_kind"] = "approval-test-passing-comparison",
                        ["media_type"] = "image/png",
                        ["refs_only"] = bool.TrueString
                    }
                    .Concat(metadataOverrides ?? new Dictionary<string, string>())
                    .GroupBy(pair => pair.Key, StringComparer.Ordinal)
                    .ToDictionary(group => group.Key, group => group.Last().Value, StringComparer.Ordinal)
            };

            return Task.FromResult(OperationResult<DesignConversionQaComparisonArtifact>.Ok(artifact));
        }
    }

    private sealed class MutatingThinClientJobEvaluator(
        IDesignConversionThinClientJobEvaluator inner,
        Func<DesignConversionInternalEvaluationResult, DesignConversionInternalEvaluationResult> mutate) : IDesignConversionThinClientJobEvaluator
    {
        private readonly IDesignConversionThinClientJobEvaluator _inner = inner;
        private readonly Func<DesignConversionInternalEvaluationResult, DesignConversionInternalEvaluationResult> _mutate = mutate;

        public async Task<OperationResult<DesignConversionInternalEvaluationResult>> EvaluateAsync(
            DesignConversionInternalEvaluateRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _inner.EvaluateAsync(request, cancellationToken);
            return result.IsSuccess && result.Value is not null
                ? OperationResult<DesignConversionInternalEvaluationResult>.Ok(_mutate(result.Value))
                : result;
        }
    }

    private sealed class CapturingThinClientJobEvaluator(
        IDesignConversionThinClientJobEvaluator inner) : IDesignConversionThinClientJobEvaluator
    {
        private readonly IDesignConversionThinClientJobEvaluator _inner = inner;

        public DesignConversionInternalEvaluationResult? LastSuccess { get; private set; }

        public async Task<OperationResult<DesignConversionInternalEvaluationResult>> EvaluateAsync(
            DesignConversionInternalEvaluateRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _inner.EvaluateAsync(request, cancellationToken);
            if (result.IsSuccess)
                LastSuccess = result.Value;

            return result;
        }
    }

    private sealed class ApprovalBrowserAdapter(string artifactRoot) : IDesignConversionBrowserProcessAdapter
    {
        private readonly string _artifactRoot = artifactRoot;

        public async Task<OperationResult<DesignConversionBrowserCaptureResult>> CaptureScreenshotAsync(
            DesignConversionBrowserCaptureRequest request,
            CancellationToken cancellationToken = default)
        {
            var physicalResolution = DesignConversionBrowserPhysicalScreenshotResolution.Resolve(
                request.Resolution,
                "APPROVAL_BROWSER_CAPTURE_RESOLUTION_INVALID",
                "Approval browser adapter");
            if (!physicalResolution.IsSuccess)
                return physicalResolution.MapError<DesignConversionBrowserCaptureResult>();

            var physicalWidth = physicalResolution.Value!.PhysicalWidth;
            var physicalHeight = physicalResolution.Value.PhysicalHeight;
            var colorSeed = SHA256.HashData(Encoding.UTF8.GetBytes(
                $"{request.RequestId}|{request.Resolution.ResolutionId}|{physicalWidth}|{physicalHeight}"));
            var bytes = SolidRgbaPng(
                physicalWidth,
                physicalHeight,
                red: (byte)(32 + (colorSeed[0] % 160)),
                green: (byte)(32 + (colorSeed[1] % 160)),
                blue: (byte)(32 + (colorSeed[2] % 160)),
                alpha: 255);
            var sha = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
            var captureRoot = Path.Combine(_artifactRoot, "approval-browser-captures");
            Directory.CreateDirectory(captureRoot);
            var path = Path.Combine(captureRoot, $"{SafeId(request.RequestId)}-{SafeId(request.Resolution.ResolutionId)}-{sha[..16]}.png");
            await File.WriteAllBytesAsync(path, bytes, cancellationToken);

            return OperationResult<DesignConversionBrowserCaptureResult>.Ok(new DesignConversionBrowserCaptureResult(
                ScreenshotUri: new Uri(path).AbsoluteUri,
                ScreenshotSha256: sha,
                ElapsedMs: 3,
                Metadata: new Dictionary<string, string>
                {
                    ["browser_adapter_kind"] = "approval-browser-adapter",
                    ["browser_process_boundary"] = "adapter_supplied_inspectable_png_file"
                },
                Warnings: []));
        }
    }

    private static byte[] SolidRgbaPng(
        int width,
        int height,
        byte red,
        byte green,
        byte blue,
        byte alpha)
    {
        var stride = checked(width * 4);
        var scanlines = new byte[checked(height * (stride + 1))];
        for (var y = 0; y < height; y++)
        {
            var rowStart = y * (stride + 1);
            scanlines[rowStart] = 0;
            for (var x = 0; x < width; x++)
            {
                var offset = rowStart + 1 + (x * 4);
                scanlines[offset] = red;
                scanlines[offset + 1] = green;
                scanlines[offset + 2] = blue;
                scanlines[offset + 3] = alpha;
            }
        }

        using var png = new MemoryStream();
        png.Write([137, 80, 78, 71, 13, 10, 26, 10]);
        Span<byte> ihdr = stackalloc byte[13];
        BinaryPrimitives.WriteInt32BigEndian(ihdr[..4], width);
        BinaryPrimitives.WriteInt32BigEndian(ihdr.Slice(4, 4), height);
        ihdr[8] = 8;
        ihdr[9] = 6;
        WritePngChunk(png, "IHDR", ihdr);
        using var compressed = new MemoryStream();
        using (var zlib = new ZLibStream(compressed, CompressionLevel.Optimal, leaveOpen: true))
            zlib.Write(scanlines);
        WritePngChunk(png, "IDAT", compressed.ToArray());
        WritePngChunk(png, "IEND", []);
        return png.ToArray();
    }

    private static void WritePngChunk(Stream stream, string type, ReadOnlySpan<byte> data)
    {
        Span<byte> length = stackalloc byte[4];
        BinaryPrimitives.WriteInt32BigEndian(length, data.Length);
        stream.Write(length);
        var typeBytes = Encoding.ASCII.GetBytes(type);
        stream.Write(typeBytes);
        stream.Write(data);
        Span<byte> crc = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(crc, Crc32(typeBytes, data));
        stream.Write(crc);
    }

    private static uint Crc32(ReadOnlySpan<byte> typeBytes, ReadOnlySpan<byte> data)
    {
        var crc = 0xffffffffu;
        foreach (var value in typeBytes)
        {
            crc ^= value;
            for (var bit = 0; bit < 8; bit++)
                crc = (crc & 1) == 1 ? (crc >> 1) ^ 0xedb88320u : crc >> 1;
        }

        foreach (var value in data)
        {
            crc ^= value;
            for (var bit = 0; bit < 8; bit++)
                crc = (crc & 1) == 1 ? (crc >> 1) ^ 0xedb88320u : crc >> 1;
        }

        return ~crc;
    }
}
