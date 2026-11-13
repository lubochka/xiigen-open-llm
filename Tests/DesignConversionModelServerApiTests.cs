// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Net;
using System.Buffers.Binary;
using System.Globalization;
using System.IO.Compression;
using System.Reflection;
using System.Reflection.Emit;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ContinuousLearning;
using DesignConversionDomain;
using DPO;
using DPO.Models;
using LoRA;
using ModelServer.Contracts;
using ModelServer.DesignConversion;
using Tokenizer;
using TransformerModel;
using TransformerModel.Config;

public sealed class DesignConversionModelServerApiTests : IDisposable
{
    private const string TestPostEvaluationStateRootPrefix =
        "design-conversion-modelserver-api-post-evaluation-";
    private const string MaxTeacherCallsExpectedTeacherModelId = "qwen-test:9b";
    private static readonly string MaxTeacherCallsExpectedTeacherManifestDigest = new('c', 64);
    private static readonly string MaxTeacherCallsExpectedTeacherModelLayerDigest = new('d', 64);
    private static readonly string LegalTeacherModelLayerDigest = new('e', 64);
    private readonly string _testPostEvaluationStateRoot = Path.Combine(
        Path.GetTempPath(),
        TestPostEvaluationStateRootPrefix + Guid.NewGuid().ToString("N"));

    private IDesignConversionPostEvaluationStateStore TestPostEvaluationStateStore =>
        new JsonDesignConversionPostEvaluationStateStore(_testPostEvaluationStateRoot);

    public void Dispose()
    {
        var root = Path.TrimEndingDirectorySeparator(
            Path.GetFullPath(_testPostEvaluationStateRoot));
        var tempRoot = Path.TrimEndingDirectorySeparator(
            Path.GetFullPath(Path.GetTempPath()));
        if (string.Equals(
                Path.GetDirectoryName(root),
                tempRoot,
                OperatingSystem.IsWindows()
                    ? StringComparison.OrdinalIgnoreCase
                    : StringComparison.Ordinal) &&
            Path.GetFileName(root).StartsWith(
                TestPostEvaluationStateRootPrefix,
                StringComparison.Ordinal) &&
            Directory.Exists(root))
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void Constructor_ComposesTeacherAuthoringOnlyForArtifactDbAndPreservesCustomFallback()
    {
        var evaluator = new FixedThinClientJobEvaluator(_ =>
            throw new InvalidOperationException(
                "Composition inspection must not execute evaluation."));
        var custom = new RecordingUnexpectedCandidateStrategy();
        var artifactDb = new TamperingLocalArtifactDb(
            PersistenceReadbackTamperMode.None);

        var withArtifactDb = new DesignConversionModelServerApi(
            evaluator,
            utcNow: null,
            artifactDb: artifactDb,
            improvementCandidateStrategy: custom);
        var withoutArtifactDb = new DesignConversionModelServerApi(
            evaluator,
            improvementCandidateStrategy: custom);

        var composed = ReadComposedCandidateStrategy(withArtifactDb);
        var unchanged = ReadComposedCandidateStrategy(withoutArtifactDb);
        var authoring = Assert.IsType<
            DesignConversionDpoTeacherAuthoringCandidateStrategy>(composed);
        Assert.Same(custom, ReadWrappedCandidateStrategy(authoring));
        Assert.Same(custom, unchanged);
        Assert.Empty(custom.Requests);
    }

    [Fact]
    public async Task SubmitAndEvaluateAsync_ValidRequest_CallsRuntimeServiceAndReturnsEvaluationEvidence()
    {
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(CreateVerdict("pass117-valid-verdict", 0.91, passed: true));
        var service = new DesignConversionInternalService(renderProvider, comparisonProvider, routingFlow: null);
        var api = new DesignConversionModelServerApi(
            service,
            () => DateTimeOffset.Parse("2026-07-02T10:00:00Z"),
            postEvaluationStateStore: TestPostEvaluationStateStore);

        var result = await api.SubmitAndEvaluateAsync(CreateRequest("pass117-valid-api-request"));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionThinClientStatus.NotReady, result.Value!.ContractStatus);
        Assert.Equal(DesignConversionThinClientStatus.NotReady, result.Value.RuntimeStatus.Job.Status);
        Assert.Equal(
            DesignConversionVisualQaBoundaryPolicy.DecisionBlocked,
            result.Value.RuntimeStatus.Provenance["final_visual_decision"]);
        Assert.Equal(
            DesignConversionVisualQaBoundaryPolicy.ProviderRequired,
            result.Value.RuntimeStatus.Provenance["tier_policy_stop_reason"]);
        Assert.False(result.Value.ProductReady);
        Assert.False(result.Value.TrainingReadiness.ProductReady);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["product_ready"]);
        Assert.Equal(
            DesignConversionThinClientStatus.NotReady,
            result.Value.BoundaryMetadata["runtime_contract_status"]);
        Assert.Equal(
            ["source-1440x900", DesignConversionRenderResolutionFactory.ServerOwnedMobileResolutionId],
            renderProvider.Requests.Select(renderRequest => renderRequest.Resolution.ResolutionId).ToArray());
        var comparisonRequest = Assert.Single(comparisonProvider.Requests);
        Assert.Equal("source-1440x900", comparisonRequest.Expected.Resolution.ResolutionId);
        Assert.Equal("source-1440x900", comparisonRequest.Actual.Resolution.ResolutionId);
        Assert.DoesNotContain(
            comparisonProvider.Requests,
            request => request.Actual.Resolution.ResolutionId == DesignConversionRenderResolutionFactory.ServerOwnedMobileResolutionId);
        Assert.Contains(DesignConversionInternalService.ServiceFirstMarker, result.Value.EvidenceRefs);
        Assert.Contains(DesignConversionInternalService.ViewportExpectedReferenceMissingStatus, result.Value.Warnings);
        Assert.Contains(
            "viewport_operation_failure:stage=expected_reference",
            result.Value.EvidenceRefs);
        Assert.Contains(
            $"viewport_operation_failure:error_code={DesignConversionInternalService.ExpectedRenderResolutionMissingCode}",
            result.Value.EvidenceRefs);
        Assert.Equal(
            "not_proven",
            result.Value.TrainingReadiness.MeasurementStatus);
        Assert.Equal("not_proven", result.Value.BoundaryMetadata["measurement_status"]);
        Assert.False(result.Value.DeterministicLogicClaimedAsLearning);
        Assert.Equal("1", result.Value.BoundaryMetadata["render_qa_result_count"]);
        Assert.Equal("1", result.Value.BoundaryMetadata["comparison_artifact_count"]);
        Assert.Equal("2", result.Value.BoundaryMetadata["multi_viewport_required_count"]);
        Assert.Equal("2", result.Value.BoundaryMetadata["multi_viewport_observed_count"]);
        Assert.Equal(bool.TrueString, result.Value.BoundaryMetadata["multi_viewport_complete"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["browser_render_executed"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["browser_e2e_verified"]);
        Assert.Equal(
            "runtime_not_completed",
            result.Value.BoundaryMetadata["browser_e2e_failure_reason"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["runtime_ready_verified"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["host_readiness_verified"]);
        AssertVisualFidelityFalse(result.Value.BoundaryMetadata, "runtime_not_completed");
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["persistence_verified"]);
        Assert.Equal(bool.TrueString, result.Value.BoundaryMetadata["browser_render_not_executed"]);
        Assert.Equal(bool.TrueString, result.Value.BoundaryMetadata["visual_ai_not_executed"]);
        Assert.Equal(NoConfiguredVisualJudge.Status, result.Value.BoundaryMetadata["visual_judge_status"]);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.ProviderRequired, result.Value.BoundaryMetadata["visual_judge_provider_boundary"]);
        Assert.Equal(
            "no_visual_provider_artifact",
            result.Value.BoundaryMetadata["visual_ai_provider_fallback_reason"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["external_visual_ai_executed"]);
        Assert.Equal("0", result.Value.BoundaryMetadata["estimated_visual_judge_cost_usd"]);
        Assert.Contains(DesignConversionVisualQaBoundaryPolicy.VisualJudgeProviderRequiredMarker, result.Value.EvidenceRefs);
        AssertNoTrustedVisualProviderVerdictMetadata(result.Value.BoundaryMetadata);
        var request = CreateRequest("pass117-valid-api-request");
        var resultJson = JsonSerializer.Serialize(result.Value);
        Assert.DoesNotContain(request.Html, resultJson, StringComparison.Ordinal);
        Assert.DoesNotContain(request.Css, resultJson, StringComparison.Ordinal);
        Assert.DoesNotContain(request.Screenshots![0].Uri, resultJson, StringComparison.Ordinal);
        Assert.DoesNotContain(request.Screenshots[0].Sha256, resultJson, StringComparison.Ordinal);
        Assert.DoesNotContain(request.DesignRenders![0].Uri, resultJson, StringComparison.Ordinal);
        Assert.DoesNotContain(request.DesignRenders[0].Sha256, resultJson, StringComparison.Ordinal);
        Assert.DoesNotContain("file:", resultJson, StringComparison.OrdinalIgnoreCase);

        var status = api.GetJobResult(result.Value.JobId);
        Assert.True(status.IsSuccess, status.ErrorMessage);
        Assert.Equal(DesignConversionThinClientStatus.NotReady, status.Value!.ContractStatus);
        Assert.False(status.Value!.ProductReady);
        Assert.Equal(bool.FalseString, status.Value.BoundaryMetadata["product_ready"]);
        Assert.Equal(
            DesignConversionThinClientStatus.NotReady,
            status.Value.BoundaryMetadata["runtime_contract_status"]);
        Assert.Equal("2", status.Value.BoundaryMetadata["multi_viewport_required_count"]);
        Assert.Equal("2", status.Value.BoundaryMetadata["multi_viewport_observed_count"]);
        Assert.Equal(bool.TrueString, status.Value.BoundaryMetadata["multi_viewport_complete"]);
        Assert.Equal(bool.FalseString, status.Value.BoundaryMetadata["browser_e2e_verified"]);
        Assert.Equal("runtime_not_completed", status.Value.BoundaryMetadata["browser_e2e_failure_reason"]);
        AssertVisualFidelityFalse(status.Value.BoundaryMetadata, "runtime_not_completed");
        Assert.Contains(DesignConversionInternalService.ViewportExpectedReferenceMissingStatus, status.Value.RuntimeStatus.Warnings);
        Assert.Contains(
            "viewport_operation_failure:stage=expected_reference",
            status.Value.EvidenceRefs);
        Assert.Contains(
            $"viewport_operation_failure:error_code={DesignConversionInternalService.ExpectedRenderResolutionMissingCode}",
            status.Value.EvidenceRefs);
        Assert.DoesNotContain(
            $"tier_policy:stop_reason={DesignConversionVisualQaBoundaryPolicy.RenderManifestIncomplete}",
            result.Value.EvidenceRefs);
        Assert.DoesNotContain(
            $"tier_policy:stop_reason={DesignConversionVisualQaBoundaryPolicy.RenderManifestIncomplete}",
            status.Value.EvidenceRefs);
        var statusJson = JsonSerializer.Serialize(status.Value);
        Assert.DoesNotContain(request.Html, statusJson, StringComparison.Ordinal);
        Assert.DoesNotContain(request.Css, statusJson, StringComparison.Ordinal);
        Assert.DoesNotContain(request.Screenshots![0].Uri, statusJson, StringComparison.Ordinal);
        Assert.DoesNotContain(request.DesignRenders![0].Uri, statusJson, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SubmitAndEvaluateAsync_RealLocalArtifactDbFreshReadbackVerifiesPersistence()
    {
        const string privateMarker = "PRIVATE_PERSISTENCE_VERIFIED_SECRET";
        var artifactDbRoot = CreateArtifactDbTempRoot("persistence-positive");
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(artifactDbRoot);
        var api = CreateApi(artifactDb);
        var baseRequest = CreateRequest("persistence-positive-real-jsonl-readback");
        var request = baseRequest with
        {
            DesignTree = baseRequest.DesignTree with
            {
                Payload = $"{{\"secret\":\"{privateMarker}\",\"kind\":\"figma\"}}"
            },
            OriginalTree = baseRequest.OriginalTree with
            {
                Payload = $"{{\"secret\":\"original-{privateMarker}\",\"kind\":\"figma\"}}"
            },
            Html = $"<main data-secret=\"{privateMarker}\">Dashboard</main>",
            Css = $".screen{{background:url('{privateMarker}');}}",
            Metadata = new Dictionary<string, string>(baseRequest.Metadata ?? new Dictionary<string, string>())
            {
                ["persistence_verified"] = bool.TrueString,
                ["local_artifact_db_persistence_verified"] = bool.TrueString,
                ["artifact_db_record_source"] = "caller_spoof"
            }
        };

        var result = await api.SubmitAndEvaluateAsync(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.False(result.Value!.ProductReady);
        Assert.False(result.Value.PublicHttpEndpointVerified);
        Assert.False(result.Value.DeploymentVerified);
        Assert.False(result.Value.ModelTrainingVerified);
        Assert.False(result.Value.ModelUploadReady);
        AssertPersistenceVerified(result.Value.BoundaryMetadata, result.Value.JobId, request.RequestId);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["persistence_caller_claim_accepted"]);
        Assert.Equal(bool.TrueString, result.Value.BoundaryMetadata["persistence_status_spoof_rejected"]);

        var status = await api.GetJobResultAsync(result.Value.JobId);
        Assert.True(status.IsSuccess, status.ErrorMessage);
        AssertPersistenceVerified(status.Value!.BoundaryMetadata, result.Value.JobId, request.RequestId);

        var records = await artifactDb.ListRecordsAsync(new DesignConversionLocalArtifactDbQuery(
            JobId: result.Value.JobId,
            RequestId: request.RequestId));
        Assert.True(records.IsSuccess, records.ErrorMessage);
        var record = Assert.Single(records.Value!);
        Assert.Equal(nameof(DesignConversionThinClientJobRuntime), record.Metadata["artifact_db_record_source"]);
        Assert.Equal(bool.TrueString, record.Metadata["local_artifact_db"]);
        Assert.Equal(bool.TrueString, record.Metadata["storage_training_artifact_db"]);

        var publicJson = JsonSerializer.Serialize(result.Value, DesignConversionModelServerHttpHandler.JsonOptions);
        AssertNoPersistencePublicLeak(publicJson, privateMarker, request.Html, request.Css, artifactDbRoot);
    }

    [Fact]
    public async Task GetVisualCostRecordsAsync_CompletedJobReturnsExactCostRecordAndPrivacySafeReadback()
    {
        const string rawUserId = "raw-user-id-visual-cost-api-91B7";
        const string privateMarker = "PRIVATE_VISUAL_COST_API_PAYLOAD_8F3C";
        const decimal expectedTotalCostUsd = 0.0600m;
        const decimal perViewportCostUsd = 0.0300m;
        var artifactDbRoot = CreateArtifactDbTempRoot("visual-cost-api-exact");
        var ledgerRoot = Path.Combine(
            Path.GetTempPath(),
            $"model-server-api-visual-cost-ledger-{Guid.NewGuid():N}");

        try
        {
            var artifactDb = new JsonlDesignConversionLocalArtifactDb(artifactDbRoot);
            var ledger = new JsonlDesignConversionVisualCostLedger(ledgerRoot);
            var visualJudge = new VisualCostReceiptJudge(perViewportCostUsd);
            var service = new DesignConversionInternalService(
                new RecordingRenderProvider(),
                new RecordingComparisonProvider(CreateVerdict("visual-cost-api-exact-verdict", 0.94, passed: true)),
                visualJudge);
            var api = new DesignConversionModelServerApi(
                new DesignConversionInternalServiceThinClientJobEvaluator(service),
                () => DateTimeOffset.Parse("2026-07-15T09:30:00Z"),
                artifactDb,
                visualCostLedger: ledger);
            var baseRequest = CreateResponsiveSubmitPathRequest("visual-cost-api-exact-request");
            var request = baseRequest with
            {
                Html = $"<main data-private=\"{privateMarker}\">Cost readback</main>",
                Css = $".screen{{--private:'{privateMarker}';display:grid;}}",
                DesignTree = baseRequest.DesignTree with
                {
                    Payload = $"{{\"secret\":\"{privateMarker}\",\"kind\":\"figma\"}}"
                },
                OriginalTree = baseRequest.OriginalTree with
                {
                    Payload = $"{{\"secret\":\"original-{privateMarker}\",\"kind\":\"figma\"}}"
                },
                Budget = baseRequest.Budget! with
                {
                    UserId = rawUserId,
                    Tier = "paid",
                    MaxImprovementCandidates = 1,
                    MaxRenderQaRuns = 2,
                    BudgetUsd = 0.25m
                }
            };

            var submitted = await api.SubmitAndEvaluateAsync(request);
            Assert.True(submitted.IsSuccess, submitted.ErrorMessage);
            var submittedValue = submitted.Value!;
            Assert.Equal(DesignConversionThinClientStatus.Completed, submittedValue.ContractStatus);
            Assert.Equal("2", submittedValue.BoundaryMetadata["multi_viewport_required_count"]);
            Assert.Equal("2", submittedValue.BoundaryMetadata["multi_viewport_observed_count"]);
            Assert.Equal(bool.TrueString, submittedValue.BoundaryMetadata["multi_viewport_complete"]);
            Assert.Equal("2", submittedValue.BoundaryMetadata["render_qa_result_count"]);
            Assert.Equal("2", submittedValue.BoundaryMetadata["comparison_artifact_count"]);
            Assert.Equal(
                expectedTotalCostUsd.ToString("0.####", CultureInfo.InvariantCulture),
                submittedValue.BoundaryMetadata["estimated_visual_judge_cost_usd"]);
            Assert.Equal(
                DesignConversionVisualQaBoundaryPolicy.DecisionAccepted,
                submittedValue.RuntimeStatus.Provenance["final_visual_decision"]);
            Assert.Equal(
                DesignConversionVisualQaBoundaryPolicy.QualityReached,
                submittedValue.RuntimeStatus.Provenance["tier_policy_stop_reason"]);
            Assert.Contains(
                "tier_policy:external_cost_measurement_complete=true",
                submittedValue.EvidenceRefs);
            Assert.Contains(
                "visual_decision:external_cost_measurement_complete=true",
                submittedValue.EvidenceRefs);
            var readback = await api.GetVisualCostRecordsAsync(submittedValue.JobId);
            var secondReadback = await api.GetVisualCostRecordsAsync(submittedValue.JobId);

            Assert.Equal(2, visualJudge.InvocationCount);
            Assert.True(readback.IsSuccess, readback.ErrorMessage);
            Assert.True(secondReadback.IsSuccess, secondReadback.ErrorMessage);
            Assert.Single(readback.Value!.Records);
            Assert.Single(secondReadback.Value!.Records);
            var record = Assert.Single(readback.Value.Records);
            Assert.Equal(expectedTotalCostUsd, record.TotalCostUsd);
            Assert.True(record.MeasurementComplete);
            Assert.Equal(0, record.MissingMeasurementCount);
            Assert.Equal(1, record.EvaluationAttemptCount);
            Assert.Equal(
                DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostSourceFixedPerRequest,
                record.CostSource);
            Assert.Equal(1, readback.Value.Summary.RecordCount);
            Assert.Equal(expectedTotalCostUsd, readback.Value.Summary.TotalCostUsd);
            Assert.True(readback.Value.Summary.MeasurementComplete);
            Assert.Equal(0, readback.Value.Summary.MissingMeasurementCount);
            Assert.Equal(1, readback.Value.Summary.EvaluationAttemptCount);
            Assert.Equal(record.CostSource, readback.Value.Summary.CostSource);
            Assert.Equal(bool.TrueString, readback.Value.BoundaryMetadata["runtime_visual_cost_records_readback_invoked"]);
            Assert.Equal(bool.TrueString, readback.Value.BoundaryMetadata["visual_cost_response_refs_only"]);
            Assert.Equal(bool.FalseString, readback.Value.BoundaryMetadata["raw_job_id_included"]);
            Assert.Equal(bool.FalseString, readback.Value.BoundaryMetadata["raw_request_id_included"]);
            Assert.Equal(bool.FalseString, readback.Value.BoundaryMetadata["sensitive_principal_data_included"]);

            var serialized = JsonSerializer.Serialize(readback.Value, DesignConversionModelServerHttpHandler.JsonOptions);
            Assert.DoesNotContain(rawUserId, serialized, StringComparison.Ordinal);
            Assert.DoesNotContain(submittedValue.JobId, serialized, StringComparison.Ordinal);
            Assert.DoesNotContain(request.RequestId, serialized, StringComparison.Ordinal);
            Assert.DoesNotContain(request.Html, serialized, StringComparison.Ordinal);
            Assert.DoesNotContain(request.Css, serialized, StringComparison.Ordinal);
            Assert.DoesNotContain(privateMarker, serialized, StringComparison.Ordinal);
            Assert.DoesNotContain(artifactDbRoot, serialized, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(ledgerRoot, serialized, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("billing", serialized, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("tenant", serialized, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("account", serialized, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            DeleteDirectoryIfExists(artifactDbRoot);
            DeleteDirectoryIfExists(ledgerRoot);
        }
    }

    [Fact]
    public async Task GetVisualCostRecordsAsync_AggregatesMultipleRuntimeRecordsAndUnknownJobIsEmpty()
    {
        const string jobId = "thin-job-visual-cost-api-aggregate";
        var records = new[]
        {
            CreateVisualCostLedgerRecordForApiTest(
                jobId,
                "visual-cost-api-aggregate-request-a",
                0.0300m,
                measurementComplete: true,
                missingMeasurementCount: 0,
                evaluationAttemptCount: 1,
                costSource: DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostSourceFixedPerRequest,
                recordedAtUtc: DateTimeOffset.UnixEpoch.AddSeconds(20)),
            CreateVisualCostLedgerRecordForApiTest(
                jobId,
                "visual-cost-api-aggregate-request-b",
                0.0400m,
                measurementComplete: false,
                missingMeasurementCount: 1,
                evaluationAttemptCount: 2,
                costSource: DesignConversionVisualCostAccounting.UnknownCostSource,
                recordedAtUtc: DateTimeOffset.UnixEpoch.AddSeconds(10))
        };
        var api = new DesignConversionModelServerApi(
            new DesignConversionInternalService(
                new RecordingRenderProvider(),
                new RecordingComparisonProvider(CreateVerdict("visual-cost-api-aggregate-verdict", 0.91, passed: true))),
            () => DateTimeOffset.Parse("2026-07-15T09:35:00Z"),
            visualCostLedger: new SeededVisualCostLedger(records));

        var readback = await api.GetVisualCostRecordsAsync(jobId);
        var unknown = await api.GetVisualCostRecordsAsync("thin-job-visual-cost-api-unknown");

        Assert.True(readback.IsSuccess, readback.ErrorMessage);
        Assert.Equal(2, readback.Value!.Records.Count);
        Assert.Equal(
            records.OrderBy(record => record.RecordedAtUtc).Select(record => record.RecordId),
            readback.Value.Records.Select(record => record.RecordId));
        Assert.Equal(2, readback.Value.Summary.RecordCount);
        Assert.Equal(0.0700m, readback.Value.Summary.TotalCostUsd);
        Assert.False(readback.Value.Summary.MeasurementComplete);
        Assert.Equal(1, readback.Value.Summary.MissingMeasurementCount);
        Assert.Equal(3, readback.Value.Summary.EvaluationAttemptCount);
        Assert.Equal(DesignConversionVisualCostAccounting.MixedCostSource, readback.Value.Summary.CostSource);

        Assert.True(unknown.IsSuccess, unknown.ErrorMessage);
        Assert.Empty(unknown.Value!.Records);
        Assert.Equal(0, unknown.Value.Summary.RecordCount);
        Assert.Equal(0m, unknown.Value.Summary.TotalCostUsd);
        Assert.False(unknown.Value.Summary.MeasurementComplete);
        Assert.Equal(DesignConversionVisualCostAccounting.UnknownCostSource, unknown.Value.Summary.CostSource);
        Assert.Equal(bool.TrueString, unknown.Value.BoundaryMetadata["visual_cost_unknown_job_empty_success"]);
    }

    [Fact]
    public async Task GetVisualCostRecordsAsync_CorruptLedgerReturnsStableSafeError()
    {
        var ledgerRoot = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "model-server-api-visual-cost-corrupt-ledger");
        Directory.CreateDirectory(ledgerRoot);
        await File.WriteAllTextAsync(
            Path.Combine(ledgerRoot, JsonlDesignConversionVisualCostLedger.RecordsFileName),
            "{ not-json");
        var api = new DesignConversionModelServerApi(
            new DesignConversionInternalService(
                new RecordingRenderProvider(),
                new RecordingComparisonProvider(CreateVerdict("visual-cost-api-corrupt-verdict", 0.91, passed: true))),
            () => DateTimeOffset.Parse("2026-07-15T09:40:00Z"),
            visualCostLedger: new JsonlDesignConversionVisualCostLedger(ledgerRoot));

        var readback = await api.GetVisualCostRecordsAsync("thin-job-visual-cost-corrupt");

        Assert.False(readback.IsSuccess);
        Assert.Equal(DesignConversionThinClientJobRuntime.VisualCostRecordsReadFailedErrorCode, readback.ErrorCode);
        Assert.Equal(DesignConversionThinClientJobRuntime.VisualCostRecordsReadFailedSafeMessage, readback.ErrorMessage);
        Assert.DoesNotContain(ledgerRoot, readback.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(JsonlDesignConversionVisualCostLedger.RecordsFileName, readback.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("thin-job-visual-cost-corrupt", readback.ErrorMessage, StringComparison.Ordinal);
        Assert.DoesNotContain("not-json", readback.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SubmitAndEvaluateAsync_CallerSpoofedPersistenceWithoutDbProofStaysFalse()
    {
        var api = CreateApi();
        var baseRequest = CreateRequest("persistence-spoof-no-db");
        var request = baseRequest with
        {
            Metadata = new Dictionary<string, string>(baseRequest.Metadata ?? new Dictionary<string, string>())
            {
                ["persistence_verified"] = bool.TrueString,
                ["local_artifact_db_persistence_verified"] = bool.TrueString,
                ["artifact_db_record_source"] = nameof(DesignConversionThinClientJobRuntime),
                ["storage_training_artifact_db"] = bool.TrueString,
                ["local_artifact_db"] = bool.TrueString
            }
        };

        var result = await api.SubmitAndEvaluateAsync(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        AssertPersistenceNotVerified(result.Value!.BoundaryMetadata, "local_artifact_db_not_configured");
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["persistence_caller_claim_accepted"]);
        Assert.Equal(bool.TrueString, result.Value.BoundaryMetadata["persistence_status_spoof_rejected"]);

        var status = await api.GetJobResultAsync(result.Value.JobId);
        Assert.True(status.IsSuccess, status.ErrorMessage);
        AssertPersistenceNotVerified(status.Value!.BoundaryMetadata, "local_artifact_db_not_configured");
    }

    [Theory]
    [InlineData(PersistenceReadbackTamperMode.AppendFailure)]
    [InlineData(PersistenceReadbackTamperMode.AppendException)]
    public async Task SubmitAndEvaluateAsync_EligibleDpoArtifactDbFailureFailsClosed(
        PersistenceReadbackTamperMode mode)
    {
        var artifactDb = new TamperingLocalArtifactDb(mode);
        using var teacherAssociation = new ArtifactDbDpoTeacherAssociationFixture(
            artifactDb,
            supportIndependentPreferenceEvidence: true);
        var runtime = CreateTeacherAuthoredDirectRuntime(artifactDb);
        var api = new DesignConversionModelServerApi(runtime);
        var baseRequest = CreateResponsiveSubmitPathRequest($"persistence-{mode}");
        var request = baseRequest with
        {
            Budget = baseRequest.Budget! with { BudgetUsd = 1.00m }
        };

        var result = await api.SubmitAndEvaluateAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal(1, teacherAssociation.Teacher.InvocationCount);
        Assert.Equal(
            DesignConversionThinClientJobRuntime.LocalArtifactDbDpoPersistenceFailedErrorCode,
            result.ErrorCode);
        Assert.Equal(
            DesignConversionThinClientJobRuntime.LocalArtifactDbDpoPersistenceSafeMessage,
            result.ErrorMessage);
        Assert.DoesNotContain("Scripted append failure", result.ErrorMessage, StringComparison.Ordinal);
        Assert.DoesNotContain("Fake append exception", result.ErrorMessage, StringComparison.Ordinal);
        Assert.DoesNotContain(artifactDb.RootDirectory, result.ErrorMessage, StringComparison.OrdinalIgnoreCase);

        var run = runtime.GetStatus($"thin-job-{request.RequestId}");
        Assert.True(run.IsSuccess, run.ErrorMessage);
        Assert.Equal(DesignConversionThinClientStatus.Failed, run.Value!.ContractStatus);
        Assert.Equal(DesignConversionThinClientStatus.Failed, run.Value.Job.Status);
        Assert.Empty(run.Value.Artifacts);
        Assert.Equal(bool.FalseString, run.Value.Job.Metadata["local_artifact_db_persistence_verified"]);
        Assert.Equal(bool.TrueString, run.Value.Job.Metadata["local_artifact_db_persistence_failed"]);
        Assert.Equal("failed_closed", run.Value.Job.Metadata["local_artifact_db_persistence_status"]);
        Assert.Equal(
            DesignConversionThinClientJobRuntime.LocalArtifactDbDpoPersistenceFailedErrorCode,
            run.Value.Job.Metadata["local_artifact_db_persistence_error_code"]);
        Assert.Equal(bool.TrueString, run.Value.Job.Metadata["local_artifact_db_persistence_required"]);
        Assert.Equal(bool.TrueString, run.Value.Job.Metadata["runtime_created_dpo_candidate"]);
        Assert.Equal(bool.FalseString, run.Value.Job.Metadata["storage_training_artifact_db"]);
        Assert.Equal(bool.FalseString, run.Value.Job.Metadata["local_artifact_db"]);
        Assert.Contains("local_artifact_db_persistence_failed=true", run.Value.Warnings);
        Assert.Contains("local_artifact_db_persistence_status=failed_closed", run.Value.Warnings);
        Assert.DoesNotContain(
            run.Value.Warnings,
            warning => warning.Contains("non_durable_result_preserved", StringComparison.Ordinal));
        Assert.Equal(1, artifactDb.AppendCalls);

        var status = await api.GetJobResultAsync(run.Value.Job.JobId);
        Assert.True(status.IsSuccess, status.ErrorMessage);
        Assert.Equal(DesignConversionThinClientStatus.Failed, status.Value!.ContractStatus);
        AssertPersistenceNotVerified(status.Value!.BoundaryMetadata, "runtime_append_success_not_reported");
        Assert.Equal(bool.FalseString, status.Value.BoundaryMetadata["persistence_server_append_reported"]);

        var records = await artifactDb.ListRecordsAsync();
        Assert.True(records.IsSuccess, records.ErrorMessage);
        Assert.Empty(records.Value!);
        Assert.Equal(1, artifactDb.AppendCalls);
        var journalPath = Path.Combine(
            artifactDb.RootDirectory,
            "conversion-results.jsonl");
        Assert.True(
            !File.Exists(journalPath) || new FileInfo(journalPath).Length == 0,
            $"Failed append unexpectedly changed the journal: {journalPath}");
    }

    [Theory]
    [InlineData(PersistenceReadbackTamperMode.AppendFailure, "scripted_persistence_failure")]
    [InlineData(PersistenceReadbackTamperMode.AppendException, "local_artifact_db_persistence_exception")]
    public async Task RunAsync_OptionalArtifactDbFailurePreservesDiagnosticsButDefersReadiness(
        PersistenceReadbackTamperMode mode,
        string expectedErrorCode)
    {
        var artifactDb = new TamperingLocalArtifactDb(mode);
        var service = new DesignConversionInternalService(
            new RecordingRenderProvider(),
            new RecordingComparisonProvider(CreateVerdict($"persistence-{mode}-verdict", 0.90, passed: true)),
            routingFlow: null);
        var runtime = new DesignConversionThinClientJobRuntime(
            new DesignConversionInternalServiceThinClientJobEvaluator(service),
            () => DateTimeOffset.Parse("2026-07-02T10:00:00Z"),
            artifactDb);
        var api = new DesignConversionModelServerApi(runtime);
        var submitted = CreateSubmittedRuntimeRequest(
            $"persistence-{mode}",
            new Dictionary<string, string>
            {
                ["source_manifest_ref"] = $"persistence-{mode}-manifest"
            });

        var submit = runtime.Submit(submitted);
        Assert.True(submit.IsSuccess, submit.ErrorMessage);
        var run = await runtime.RunAsync(submit.Value!.JobId);

        Assert.True(run.IsSuccess, run.ErrorMessage);
        Assert.Equal(DesignConversionThinClientStatus.NotReady, run.Value!.ContractStatus);
        Assert.Equal(DesignConversionThinClientStatus.NotReady, run.Value.Job.Status);
        Assert.Equal(
            DesignConversionThinClientStatus.NotReady,
            run.Value.Provenance["thin_client_terminal_status"]);
        Assert.NotEmpty(run.Value.Artifacts);
        Assert.Equal(bool.FalseString, run.Value.Job.Metadata["local_artifact_db_persistence_verified"]);
        Assert.Equal(bool.TrueString, run.Value.Job.Metadata["local_artifact_db_persistence_failed"]);
        Assert.Equal("non_durable_optional", run.Value.Job.Metadata["local_artifact_db_persistence_status"]);
        Assert.Equal(expectedErrorCode, run.Value.Job.Metadata["local_artifact_db_persistence_error_code"]);
        Assert.Equal(bool.FalseString, run.Value.Job.Metadata["local_artifact_db_autowrite"]);
        Assert.Equal(bool.FalseString, run.Value.Job.Metadata["local_artifact_db_persistence_required"]);
        Assert.Equal(bool.FalseString, run.Value.Job.Metadata["storage_training_artifact_db"]);
        Assert.Equal(bool.FalseString, run.Value.Job.Metadata["local_artifact_db"]);
        Assert.Contains("local_artifact_db_persistence_failed=true", run.Value.Warnings);
        Assert.Contains("local_artifact_db_persistence_status=non_durable_optional", run.Value.Warnings);
        Assert.Contains("local_artifact_db_autowrite=false", run.Value.Warnings);
        Assert.Contains(
            $"thin_client_terminal_status={DesignConversionThinClientStatus.NotReady}",
            run.Value.Warnings);
        Assert.Equal(1, artifactDb.AppendCalls);

        var status = await api.GetJobResultAsync(submit.Value.JobId);
        Assert.True(status.IsSuccess, status.ErrorMessage);
        Assert.Equal(DesignConversionThinClientStatus.NotReady, status.Value!.ContractStatus);
        Assert.Equal(
            DesignConversionThinClientStatus.NotReady,
            status.Value.BoundaryMetadata["runtime_contract_status"]);
        Assert.Equal(bool.FalseString, status.Value.BoundaryMetadata["product_ready"]);
        AssertPersistenceNotVerified(status.Value.BoundaryMetadata, "runtime_append_success_not_reported");
        Assert.Equal(bool.FalseString, status.Value.BoundaryMetadata["persistence_server_append_reported"]);
        var publicStatusJson = JsonSerializer.Serialize(status.Value, DesignConversionModelServerHttpHandler.JsonOptions);
        Assert.DoesNotContain(submitted.Html, publicStatusJson, StringComparison.Ordinal);
        Assert.DoesNotContain(submitted.Css, publicStatusJson, StringComparison.Ordinal);

        var records = await artifactDb.ListRecordsAsync();
        Assert.True(records.IsSuccess, records.ErrorMessage);
        Assert.Empty(records.Value!);
        Assert.Equal(1, artifactDb.AppendCalls);
    }

    [Theory]
    [InlineData(PersistenceReadbackTamperMode.AppendFailure, "scripted_persistence_failure")]
    [InlineData(PersistenceReadbackTamperMode.AppendException, "local_artifact_db_persistence_exception")]
    public async Task SubmitAndEvaluateAsync_AcceptedVisualDecisionWithDpoCandidateArtifactDbFailureFailsClosed(
        PersistenceReadbackTamperMode mode,
        string expectedErrorCode)
    {
        const int providerEndpointCount = 2;
        const int expectedAttemptCount = 2;
        var requestId = $"accepted-visual-artifact-db-{mode}";
        var baseRequest = CreateTrustedMultiAttemptRequest(requestId);
        var request = baseRequest with
        {
            Budget = baseRequest.Budget! with { BudgetUsd = 1.00m }
        };
        using var fixture = await CreateServerOwnedVisualProviderFixtureAsync(request);
        var artifactDb = new TamperingLocalArtifactDb(mode);
        using var teacherAssociation = new ArtifactDbDpoTeacherAssociationFixture(
            artifactDb,
            supportIndependentPreferenceEvidence: true);
        var requiredViewportCount = fixture.Request.Screenshots!
            .Concat(fixture.Request.DesignRenders!)
            .Select(artifact => (artifact.Width, artifact.Height))
            .Distinct()
            .Count();
        var callsPerAttempt = providerEndpointCount * requiredViewportCount;
        var expectedProviderCallCount = callsPerAttempt * expectedAttemptCount;
        var providerCallOrdinal = 0;
        var handler = new EchoingRealVisualJudgeHttpHandler(_ =>
            Interlocked.Increment(ref providerCallOrdinal) <= callsPerAttempt ? 0.72 : 0.973,
            lowerScoreIssueCategory: "spacing",
            lowerScoreIssueSeverity: "minor");
        var api = CreateBrowserReceiptRuntimeShapedReceiptApi(
            handler,
            artifactDb: artifactDb,
            serverOwnedCostPerRequestUsd: "0.0125",
            serverOwnedImageArtifactRoot: fixture.ArtifactRoot);

        var result = await api.SubmitAndEvaluateAsync(fixture.Request);

        Assert.False(result.IsSuccess);
        Assert.Equal(1, teacherAssociation.Teacher.InvocationCount);
        Assert.Equal(
            DesignConversionThinClientJobRuntime.LocalArtifactDbDpoPersistenceFailedErrorCode,
            result.ErrorCode);
        Assert.Equal(
            DesignConversionThinClientJobRuntime.LocalArtifactDbDpoPersistenceSafeMessage,
            result.ErrorMessage);
        Assert.DoesNotContain(expectedErrorCode, result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Scripted append failure", result.ErrorMessage, StringComparison.Ordinal);
        Assert.DoesNotContain("Fake append exception", result.ErrorMessage, StringComparison.Ordinal);
        Assert.DoesNotContain(artifactDb.RootDirectory, result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(expectedProviderCallCount, handler.CallCount);
        Assert.Equal(expectedProviderCallCount, providerCallOrdinal);

        var status = await api.GetJobResultAsync($"thin-job-{fixture.Request.RequestId}");
        Assert.True(status.IsSuccess, status.ErrorMessage);
        var statusValue = status.Value!;
        Assert.Equal(DesignConversionThinClientStatus.Failed, statusValue.ContractStatus);
        Assert.Equal(DesignConversionThinClientStatus.Failed, statusValue.RuntimeStatus.ContractStatus);
        Assert.Equal(DesignConversionThinClientStatus.Failed, statusValue.RuntimeStatus.Job.Status);
        Assert.Equal(DesignConversionThinClientStatus.Failed, statusValue.BoundaryMetadata["runtime_contract_status"]);
        Assert.Equal(bool.FalseString, statusValue.BoundaryMetadata["product_ready"]);
        Assert.Empty(statusValue.RuntimeStatus.Artifacts);
        Assert.Equal(bool.FalseString, statusValue.RuntimeStatus.Job.Metadata["local_artifact_db_persistence_verified"]);
        Assert.Equal(bool.TrueString, statusValue.RuntimeStatus.Job.Metadata["local_artifact_db_persistence_failed"]);
        Assert.Equal("failed_closed", statusValue.RuntimeStatus.Job.Metadata["local_artifact_db_persistence_status"]);
        Assert.Equal(
            DesignConversionThinClientJobRuntime.LocalArtifactDbDpoPersistenceFailedErrorCode,
            statusValue.RuntimeStatus.Job.Metadata["local_artifact_db_persistence_error_code"]);
        Assert.Equal(bool.TrueString, statusValue.RuntimeStatus.Job.Metadata["local_artifact_db_persistence_required"]);
        Assert.Equal(bool.TrueString, statusValue.RuntimeStatus.Job.Metadata["runtime_created_dpo_candidate"]);
        Assert.Contains("local_artifact_db_persistence_status=failed_closed", statusValue.RuntimeStatus.Warnings);
        Assert.DoesNotContain(
            statusValue.RuntimeStatus.Warnings,
            warning => warning.Contains("non_durable_result_preserved", StringComparison.Ordinal));
        AssertPersistenceNotVerified(statusValue.BoundaryMetadata, "runtime_append_success_not_reported");
        Assert.Equal(bool.FalseString, statusValue.BoundaryMetadata["persistence_server_append_reported"]);
        var publicStatusJson = JsonSerializer.Serialize(statusValue, DesignConversionModelServerHttpHandler.JsonOptions);
        Assert.DoesNotContain(fixture.Request.Html, publicStatusJson, StringComparison.Ordinal);
        Assert.DoesNotContain(fixture.Request.Css, publicStatusJson, StringComparison.Ordinal);

        var records = await artifactDb.ListRecordsAsync();
        Assert.True(records.IsSuccess, records.ErrorMessage);
        Assert.Empty(records.Value!);
        Assert.Equal(1, artifactDb.AppendCalls);
        var journalPath = Path.Combine(
            artifactDb.RootDirectory,
            "conversion-results.jsonl");
        Assert.True(
            !File.Exists(journalPath) || new FileInfo(journalPath).Length == 0,
            $"Failed append unexpectedly changed the journal: {journalPath}");
    }

    [Fact]
    public async Task HttpEvaluate_TeacherAuthoringWithoutIndependentReceiptCompletesWithoutDpoCandidate()
    {
        const string privateMarker = "PRIVATE_DPO_NO_DB_HTTP_SECRET";
        var rawRequest = CreateTrustedMultiAttemptRequest("dpo-no-db-http") with
        {
            Html = $"<main data-private=\"{privateMarker}\">Dashboard</main>",
            Css = $".screen{{--private:'{privateMarker}';}}"
        };
        using var fixture = await CreateServerOwnedVisualProviderFixtureAsync(rawRequest);
        var teacherAuthoringDb = new TamperingLocalArtifactDb(PersistenceReadbackTamperMode.None);
        using var teacherAssociation = new ArtifactDbDpoTeacherAssociationFixture(
            teacherAuthoringDb);
        const int providerEndpointCount = 2;
        var requiredViewportCount = fixture.Request.Screenshots!
            .Concat(fixture.Request.DesignRenders!)
            .Select(artifact => (artifact.Width, artifact.Height))
            .Distinct()
            .Count();
        var callsPerAttempt = providerEndpointCount * requiredViewportCount;
        var providerCallOrdinal = 0;
        var visualProvider = new EchoingRealVisualJudgeHttpHandler(_ =>
            Interlocked.Increment(ref providerCallOrdinal) <= callsPerAttempt ? 0.72 : 0.973,
            lowerScoreIssueCategory: "spacing",
            lowerScoreIssueSeverity: "minor");
        var api = CreateBrowserReceiptRuntimeShapedReceiptApi(
            visualProvider,
            artifactDb: null,
            serverOwnedCostPerRequestUsd: "0.0125",
            serverOwnedImageArtifactRoot: fixture.ArtifactRoot,
            postEvaluationStateRoot: Path.Combine(
                fixture.ArtifactRoot,
                "dpo-no-db-http-post-evaluation-state"),
            teacherAuthoringArtifactDb: teacherAuthoringDb);
        var handler = new DesignConversionModelServerHttpHandler(api);
        var request = fixture.Request;

        var response = await handler.HandleAsync(new DesignConversionModelServerHttpRequest(
            Method: "POST",
            Path: DesignConversionModelServerHttpHandler.EndpointPath,
            BodyJson: JsonSerializer.Serialize(request, DesignConversionModelServerHttpHandler.JsonOptions),
            ContentType: "application/json"));

        Assert.Equal(200, response.StatusCode);
        using var document = JsonDocument.Parse(response.BodyJson);
        Assert.False(document.RootElement.GetProperty("productReady").GetBoolean());
        Assert.Equal(1, teacherAssociation.Teacher.InvocationCount);
        Assert.Equal(callsPerAttempt * 2, visualProvider.CallCount);
        Assert.DoesNotContain(privateMarker, response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain("<main", response.BodyJson, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task HttpEvaluate_SingleEvaluationWithoutRejectedPairReturnsOkWithoutCandidate()
    {
        var service = new DesignConversionInternalService(
            new RecordingRenderProvider(),
            new RecordingComparisonProvider(CreateVerdict(
                "dpo-no-db-http-single-evaluation-verdict",
                0.95,
                passed: true)),
            new VisualCostReceiptJudge(estimatedCostUsd: 0.01m),
            routingFlow: null);
        var budgetRoot = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "dpo-no-db-http-single-evaluation-budget");
        try
        {
            var api = new DesignConversionModelServerApi(
                new DesignConversionInternalServiceThinClientJobEvaluator(service),
                () => DateTimeOffset.Parse("2026-07-02T10:00:00Z"),
                visualCostBudgetStore: new JsonlDesignConversionVisualCostBudgetStore(budgetRoot));
            var handler = new DesignConversionModelServerHttpHandler(api);
            var response = await handler.HandleAsync(new DesignConversionModelServerHttpRequest(
                Method: "POST",
                Path: DesignConversionModelServerHttpHandler.EndpointPath,
                BodyJson: JsonSerializer.Serialize(
                    CreateResponsiveSubmitPathRequest("dpo-no-db-http-single-evaluation"),
                    DesignConversionModelServerHttpHandler.JsonOptions),
                ContentType: "application/json"));

            Assert.Equal(200, response.StatusCode);
            Assert.DoesNotContain(
                DesignConversionThinClientJobRuntime.LocalArtifactDbDpoPersistenceRequiredErrorCode,
                response.BodyJson,
                StringComparison.Ordinal);
        }
        finally
        {
            if (Directory.Exists(budgetRoot))
                Directory.Delete(budgetRoot, recursive: true);
        }
    }

    [Fact]
    public async Task RunAsync_ArtifactDbCancelledAppendReturnsSafeTerminalFailure()
    {
        var artifactDb = new TamperingLocalArtifactDb(PersistenceReadbackTamperMode.AppendCancellation);
        var service = new DesignConversionInternalService(
            new RecordingRenderProvider(),
            new RecordingComparisonProvider(CreateVerdict("persistence-append-cancelled-verdict", 0.90, passed: true)),
            routingFlow: null);
        var runtime = new DesignConversionThinClientJobRuntime(
            new DesignConversionInternalServiceThinClientJobEvaluator(service),
            () => DateTimeOffset.Parse("2026-07-02T10:00:00Z"),
            artifactDb);
        var submitted = CreateSubmittedRuntimeRequest(
            "persistence-append-cancelled",
            new Dictionary<string, string>
            {
                ["source_manifest_ref"] = "persistence-append-cancelled-manifest"
            });

        var submit = runtime.Submit(submitted);
        Assert.True(submit.IsSuccess, submit.ErrorMessage);

        var result = await runtime.RunAsync(submit.Value!.JobId);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionThinClientStatus.Failed, result.Value!.Job.Status);
        Assert.Equal(
            DesignConversionThinClientJobRuntime.PostEvaluationFinalizationCancelledErrorCode,
            result.Value.Job.Metadata["error_code"]);
        Assert.Equal(0, runtime.LiveRunLeaseCount);
        Assert.Equal(1, artifactDb.AppendCalls);
        var records = await artifactDb.ListRecordsAsync();
        Assert.True(records.IsSuccess, records.ErrorMessage);
        Assert.Empty(records.Value!);
    }

    [Theory]
    [InlineData(PersistenceReadbackTamperMode.EmptyReadback)]
    [InlineData(PersistenceReadbackTamperMode.MismatchedDpoCandidateIdentity)]
    public async Task RunAsync_EligibleDpoAppendWithoutMatchingFreshReadbackFailsClosed(
        PersistenceReadbackTamperMode mode)
    {
        var artifactDb = new TamperingLocalArtifactDb(mode);
        using var teacherAssociation = new ArtifactDbDpoTeacherAssociationFixture(
            artifactDb,
            supportIndependentPreferenceEvidence: true);
        var runtime = CreateTeacherAuthoredDirectRuntime(artifactDb);
        var request = CreateResponsiveSubmitPathRequest($"dpo-readback-{mode}");
        var submitted = CreateSubmittedRuntimeRequest(
            request,
            new Dictionary<string, string>
            {
                ["source_manifest_ref"] = $"dpo-readback-{mode}-manifest"
            });
        var submit = runtime.Submit(submitted);
        Assert.True(submit.IsSuccess, submit.ErrorMessage);

        var run = await runtime.RunAsync(submit.Value!.JobId);

        Assert.True(run.IsSuccess, run.ErrorMessage);
        Assert.Equal(1, teacherAssociation.Teacher.InvocationCount);
        Assert.Equal(DesignConversionThinClientStatus.Failed, run.Value!.ContractStatus);
        Assert.Equal(
            DesignConversionThinClientJobRuntime.LocalArtifactDbDpoReadbackMismatchErrorCode,
            run.Value.Job.Metadata["error_code"]);
        Assert.Equal("failed_closed", run.Value.Job.Metadata["local_artifact_db_persistence_status"]);
        Assert.Equal(1, artifactDb.AppendCalls);
        Assert.Equal(1, artifactDb.ListCalls);
        Assert.Equal(submit.Value.JobId, artifactDb.LastQuery?.JobId);
        Assert.Equal(request.RequestId, artifactDb.LastQuery?.RequestId);
    }

    [Fact]
    public async Task RunAsync_EligibleDpoAppendAndMatchingFreshReadbackCompletesWithDurableIdentity()
    {
        var artifactDb = new TamperingLocalArtifactDb(PersistenceReadbackTamperMode.None);
        using var teacherAssociation = new ArtifactDbDpoTeacherAssociationFixture(
            artifactDb,
            supportIndependentPreferenceEvidence: true);
        var runtime = CreateTeacherAuthoredDirectRuntime(artifactDb);
        var request = CreateResponsiveSubmitPathRequest("dpo-readback-match");
        var submitted = CreateSubmittedRuntimeRequest(
            request,
            new Dictionary<string, string>
            {
                ["source_manifest_ref"] = "dpo-readback-match-manifest"
            });
        var submit = runtime.Submit(submitted);
        Assert.True(submit.IsSuccess, submit.ErrorMessage);

        var run = await runtime.RunAsync(submit.Value!.JobId);

        Assert.True(run.IsSuccess, run.ErrorMessage);
        Assert.Equal(1, teacherAssociation.Teacher.InvocationCount);
        Assert.Equal(DesignConversionThinClientStatus.Completed, run.Value!.ContractStatus);
        Assert.Equal("true", run.Value.Job.Metadata["local_artifact_db_persistence_verified"]);
        Assert.Equal(1, artifactDb.AppendCalls);
        Assert.Equal(1, artifactDb.ListCalls);
        Assert.Equal(submit.Value.JobId, artifactDb.LastQuery?.JobId);
        Assert.Equal(request.RequestId, artifactDb.LastQuery?.RequestId);

        var records = await artifactDb.ListRecordsAsync(artifactDb.LastQuery);
        Assert.True(records.IsSuccess, records.ErrorMessage);
        var durable = Assert.Single(records.Value!);
        Assert.Equal(run.Value.Job.Metadata["local_artifact_db_record_id"], durable.RecordId);
        Assert.Equal(submit.Value.JobId, durable.JobId);
        Assert.Equal(request.RequestId, durable.RequestId);
        Assert.True(durable.Metadata.ContainsKey(
            JsonlDesignConversionLocalArtifactDb.ImmutableAppendFingerprintMetadataKey));
        var candidates = durable.DpoCandidates.ToArray();
        Assert.Equal(
            new[]
            {
                DesignConversionDpoUnits.CriticUnit,
                DesignConversionDpoUnits.ImproverUnit
            },
            candidates.Select(candidate => candidate.UnitId));
        Assert.All(candidates, candidate =>
        {
            Assert.Equal(candidate.TrainingRecord.RecordId, candidate.RecordId);
            Assert.False(string.IsNullOrWhiteSpace(candidate.CandidateId));
        });
    }

    [Fact]
    public async Task RunAsync_TeacherAuthoringWithoutIndependentReceipt_PersistsZeroDpoCandidates()
    {
        var artifactDb = new TamperingLocalArtifactDb(PersistenceReadbackTamperMode.None);
        using var teacherAssociation = new ArtifactDbDpoTeacherAssociationFixture(artifactDb);
        var runtime = CreateTeacherAuthoredDirectRuntime(artifactDb);
        var request = CreateResponsiveSubmitPathRequest("dpo-no-independent-receipt");
        var submitted = CreateSubmittedRuntimeRequest(
            request,
            new Dictionary<string, string>
            {
                ["source_manifest_ref"] = "dpo-no-independent-receipt-manifest"
            });
        var submit = runtime.Submit(submitted);
        Assert.True(submit.IsSuccess, submit.ErrorMessage);

        var run = await runtime.RunAsync(submit.Value!.JobId);

        Assert.True(run.IsSuccess, run.ErrorMessage);
        Assert.Equal(1, teacherAssociation.Teacher.InvocationCount);
        var records = await artifactDb.ListRecordsAsync(new DesignConversionLocalArtifactDbQuery(
            JobId: submit.Value.JobId,
            RequestId: request.RequestId));
        Assert.True(records.IsSuccess, records.ErrorMessage);
        Assert.Empty(Assert.Single(records.Value!).DpoCandidates);
    }

    [Theory]
    [MemberData(nameof(PersistenceReadbackTamperCases))]
    public async Task SubmitAndEvaluateAsync_FreshReadbackMustMatchServerOwnedPersistenceRecord(
        PersistenceReadbackTamperMode mode,
        string expectedReason)
    {
        var artifactDb = new TamperingLocalArtifactDb(mode);
        var api = CreateApi(artifactDb);

        var result = await api.SubmitAndEvaluateAsync(CreateRequest($"persistence-fake-readback-{mode}"));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        AssertPersistenceNotVerified(result.Value!.BoundaryMetadata, expectedReason);
        Assert.Equal(bool.TrueString, result.Value.BoundaryMetadata["persistence_server_append_reported"]);
        Assert.Equal(
            mode is PersistenceReadbackTamperMode.ReadbackFailure or PersistenceReadbackTamperMode.ReadbackException
                ? bool.FalseString
                : bool.TrueString,
            result.Value.BoundaryMetadata["persistence_fresh_readback_success"]);
        Assert.Equal(result.Value.JobId, artifactDb.LastQuery?.JobId);
        Assert.Equal(result.Value.RequestId, artifactDb.LastQuery?.RequestId);

        var status = await api.GetJobResultAsync(result.Value.JobId);
        Assert.True(status.IsSuccess, status.ErrorMessage);
        AssertPersistenceNotVerified(status.Value!.BoundaryMetadata, expectedReason);
    }

    [Theory]
    [InlineData(PersistenceReadbackTamperMode.ReadbackForeignCancellation)]
    [InlineData(PersistenceReadbackTamperMode.ReadbackFatalGraph)]
    public async Task SubmitAndEvaluateAsync_FreshReadbackForeignCancellationOrFatalGraphEscapes(
        PersistenceReadbackTamperMode mode)
    {
        var artifactDb = new TamperingLocalArtifactDb(mode);
        var api = CreateApi(artifactDb);

        var exception = await Record.ExceptionAsync(
            () => api.SubmitAndEvaluateAsync(CreateRequest($"persistence-escape-{mode}")));

        AssertReadbackExceptionGraphEscaped(exception, mode);
        Assert.Equal(1, artifactDb.ListCalls);
    }

    [Fact]
    public async Task SubmitAndEvaluateAsync_NullBudgetFallbackRunsRequiredBrowserQaViewportMatrix()
    {
        const string privateMarker = "PRIVATE_N6_3_NULL_BUDGET_SECRET_TOKEN";
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(CreateVerdict("n6-3-null-budget-two-viewports-verdict", 0.91, passed: true));
        var service = new DesignConversionInternalService(renderProvider, comparisonProvider, routingFlow: null);
        var evaluator = new RecordingThinClientJobEvaluator(service);
        var api = new DesignConversionModelServerApi(
            evaluator,
            () => DateTimeOffset.Parse("2026-07-11T00:00:00Z"),
            postEvaluationStateStore: TestPostEvaluationStateStore);
        var baseRequest = CreateRequest("n6-3-null-budget-two-viewports-api-request");
        var mobileScreenshot = new DesignConversionModelServerArtifactRef(
            ArtifactId: "screenshot-mobile",
            Kind: "screenshot",
            Width: 390,
            Height: 844,
            Uri: "artifact://n6-3/screenshot-mobile.png",
            Sha256: "sha256:screenshot-mobile",
            Metadata: new Dictionary<string, string>
            {
                [DesignConversionRenderResolutionFactory.DeviceScaleFactorMetadataKey] = "3"
            });
        var mobileDesignRender = new DesignConversionModelServerArtifactRef(
            ArtifactId: "design-render-mobile",
            Kind: "design-render",
            Width: 390,
            Height: 844,
            Uri: "artifact://n6-3/design-render-mobile.png",
            Sha256: "sha256:design-render-mobile");
        var request = baseRequest with
        {
            DesignTree = baseRequest.DesignTree with { Payload = $"{{\"name\":\"{privateMarker}\"}}" },
            OriginalTree = baseRequest.OriginalTree with { Payload = $"{{\"name\":\"original-{privateMarker}\"}}" },
            Html = $"<main data-secret=\"{privateMarker}\">Dashboard</main>",
            Css = $".screen{{background:url('{privateMarker}');}}",
            Screenshots = baseRequest.Screenshots!.Append(mobileScreenshot).ToArray(),
            DesignRenders = baseRequest.DesignRenders!.Append(mobileDesignRender).ToArray(),
            Budget = null,
            Metadata = new Dictionary<string, string>
            {
                ["private_note"] = privateMarker,
                [DesignConversionThinClientJobRuntime.ServerOwnedRequiredBrowserQaMatrixProvenanceKey] = bool.FalseString
            }
        };

        var result = await api.SubmitAndEvaluateAsync(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var runtimeRequest = Assert.Single(evaluator.Requests);
        Assert.Equal(4, runtimeRequest.Budget.MaxRenderQaRuns);
        Assert.Equal(
            bool.TrueString,
            runtimeRequest.Provenance[DesignConversionThinClientJobRuntime.ServerOwnedRequiredBrowserQaMatrixProvenanceKey]);
        Assert.Equal(
            bool.FalseString,
            runtimeRequest.Provenance[$"request_metadata:{DesignConversionThinClientJobRuntime.ServerOwnedRequiredBrowserQaMatrixProvenanceKey}"]);
        Assert.Equal(
            [
                DesignConversionRenderResolutionFactory.RequiredBrowserQaMobileResolutionId,
                DesignConversionRenderResolutionFactory.RequiredBrowserQaTabletResolutionId,
                DesignConversionRenderResolutionFactory.RequiredBrowserQaLaptopResolutionId,
                DesignConversionRenderResolutionFactory.RequiredBrowserQaDesktopResolutionId
            ],
            renderProvider.Requests.Select(renderRequest => renderRequest.Resolution.ResolutionId).ToArray());
        Assert.Equal(
            renderProvider.Requests.Select(renderRequest => renderRequest.Resolution.ResolutionId),
            runtimeRequest.RenderResolutions.Select(resolution => resolution.ResolutionId));
        Assert.Equal([360, 768, 1280, 1920], renderProvider.Requests.Select(renderRequest => renderRequest.Resolution.Width).ToArray());
        Assert.All(renderProvider.Requests, renderRequest => Assert.Equal(1.0, renderRequest.Resolution.DeviceScaleFactor));
        Assert.Empty(comparisonProvider.Requests);
        Assert.Contains(DesignConversionInternalService.ViewportExpectedReferenceMissingStatus, result.Value!.Warnings);
        Assert.Contains("viewport_operation_failure:stage=expected_reference", result.Value.EvidenceRefs);
        Assert.Contains(
            $"viewport_operation_failure:error_code={DesignConversionInternalService.ExpectedRenderResolutionMissingCode}",
            result.Value.EvidenceRefs);
        Assert.False(result.Value.DeterministicLogicClaimedAsLearning);
        Assert.Equal("0", result.Value!.BoundaryMetadata["render_qa_result_count"]);
        Assert.Equal("4", result.Value.BoundaryMetadata["multi_viewport_required_count"]);
        Assert.Equal("4", result.Value.BoundaryMetadata["multi_viewport_observed_count"]);
        Assert.Equal(bool.TrueString, result.Value.BoundaryMetadata["multi_viewport_complete"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["browser_e2e_verified"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["product_ready"]);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.ProviderRequired, result.Value.BoundaryMetadata["visual_judge_status"]);

        var resultJson = JsonSerializer.Serialize(result.Value);
        Assert.DoesNotContain(privateMarker, resultJson, StringComparison.Ordinal);
        Assert.DoesNotContain(request.Html, resultJson, StringComparison.Ordinal);
        Assert.DoesNotContain(request.Css, resultJson, StringComparison.Ordinal);
        foreach (var artifact in request.Screenshots!.Concat(request.DesignRenders!))
        {
            Assert.DoesNotContain(artifact.Uri, resultJson, StringComparison.Ordinal);
            Assert.DoesNotContain(artifact.Sha256, resultJson, StringComparison.Ordinal);
        }
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["raw_html_included"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["raw_design_tree_included"]);

        var status = api.GetJobResult(result.Value.JobId);
        Assert.True(status.IsSuccess, status.ErrorMessage);
        Assert.Equal("4", status.Value!.BoundaryMetadata["multi_viewport_required_count"]);
        Assert.Equal("4", status.Value.BoundaryMetadata["multi_viewport_observed_count"]);
        Assert.Equal(bool.TrueString, status.Value.BoundaryMetadata["multi_viewport_complete"]);
        Assert.Equal(bool.FalseString, status.Value.BoundaryMetadata["browser_e2e_verified"]);
        Assert.Equal(bool.FalseString, status.Value.BoundaryMetadata["product_ready"]);

        var statusJson = JsonSerializer.Serialize(status.Value);
        Assert.DoesNotContain(privateMarker, statusJson, StringComparison.Ordinal);
        Assert.DoesNotContain(request.Html, statusJson, StringComparison.Ordinal);
        Assert.DoesNotContain(request.Css, statusJson, StringComparison.Ordinal);
        Assert.Equal(bool.FalseString, status.Value.BoundaryMetadata["raw_html_included"]);
        Assert.Equal(bool.FalseString, status.Value.BoundaryMetadata["raw_design_tree_included"]);
    }

    [Fact]
    public async Task SubmitAndEvaluateAsync_SameSizeViewportIdentitiesRemainSeparateAndBindExpectedArtifacts()
    {
        const string privateMarker = "PRIVATE_SAME_SIZE_VIEWPORT_IDENTITY_SECRET_TOKEN";
        const string privateOutputPath = @"C:\private\design-to-code\mobile-menu-output.png";
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(CreateVerdict("same-size-identity-verdict", 0.91, passed: true));
        var service = new DesignConversionInternalService(renderProvider, comparisonProvider, routingFlow: null);
        var api = new DesignConversionModelServerApi(
            service,
            () => DateTimeOffset.Parse("2026-07-21T00:00:00Z"),
            postEvaluationStateStore: TestPostEvaluationStateStore);
        var baseRequest = CreateRequest("same-size-viewport-identity-api-request");
        var request = baseRequest with
        {
            DesignTree = baseRequest.DesignTree with { Payload = $"{{\"name\":\"{privateMarker}\",\"kind\":\"figma\"}}" },
            OriginalTree = baseRequest.OriginalTree with { Payload = $"{{\"name\":\"original-{privateMarker}\",\"kind\":\"figma\"}}" },
            Html = $"<main data-secret=\"{privateMarker}\">Dashboard</main>",
            Css = $".screen{{background:url('{privateMarker}');}}",
            Screenshots =
            [
                new DesignConversionModelServerArtifactRef(
                    ArtifactId: "screenshot-mobile-menu",
                    Kind: "screenshot",
                    Width: 390,
                    Height: 844,
                    Uri: "artifact://same-size-identity/screenshot-mobile-menu.png",
                    Sha256: "sha256:screenshot-mobile-menu",
                    Metadata: new Dictionary<string, string>
                    {
                        ["viewportId"] = "mobile-menu",
                        ["private_output_path"] = privateOutputPath
                    }),
                new DesignConversionModelServerArtifactRef(
                    ArtifactId: "screenshot-mobile-cart",
                    Kind: "screenshot",
                    Width: 390,
                    Height: 844,
                    Uri: "artifact://same-size-identity/screenshot-mobile-cart.png",
                    Sha256: "sha256:screenshot-mobile-cart",
                    Metadata: new Dictionary<string, string>
                    {
                        ["viewport_label"] = "mobile-cart"
                    })
            ],
            DesignRenders =
            [
                new DesignConversionModelServerArtifactRef(
                    ArtifactId: "design-render-mobile-menu",
                    Kind: "design-render",
                    Width: 390,
                    Height: 844,
                    Uri: "artifact://same-size-identity/design-render-mobile-menu.png",
                    Sha256: "sha256:design-render-mobile-menu",
                    Metadata: new Dictionary<string, string>
                    {
                        ["viewport_label"] = "mobile-menu"
                    }),
                new DesignConversionModelServerArtifactRef(
                    ArtifactId: "design-render-mobile-cart",
                    Kind: "design-render",
                    Width: 390,
                    Height: 844,
                    Uri: "artifact://same-size-identity/design-render-mobile-cart.png",
                    Sha256: "sha256:design-render-mobile-cart",
                    Metadata: new Dictionary<string, string>
                    {
                        ["viewportId"] = "mobile-cart"
                    })
            ],
            Budget = baseRequest.Budget! with
            {
                MaxImprovementCandidates = 1,
                MaxRenderQaRuns = 2
            },
            Metadata = new Dictionary<string, string>
            {
                ["private_note"] = privateMarker,
                ["private_output_path"] = privateOutputPath
            }
        };

        var result = await api.SubmitAndEvaluateAsync(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(
            ["source-390x844~mobile-menu", "source-390x844~mobile-cart"],
            renderProvider.Requests.Select(renderRequest => renderRequest.Resolution.ResolutionId).ToArray());
        Assert.Equal("3", result.Value!.BoundaryMetadata["multi_viewport_required_count"]);
        Assert.Equal("2", result.Value.BoundaryMetadata["multi_viewport_observed_count"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["multi_viewport_complete"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["product_ready"]);
        Assert.False(result.Value.ProductReady);
        Assert.Equal(2, comparisonProvider.Requests.Count);

        Assert.Equal("design-render-mobile-menu:expected:1", comparisonProvider.Requests[0].Expected.ImageId);
        Assert.Equal("artifact://same-size-identity/design-render-mobile-menu.png", comparisonProvider.Requests[0].Expected.Uri);
        Assert.Equal("sha256:design-render-mobile-menu", comparisonProvider.Requests[0].Expected.Sha256);
        Assert.Equal("source-390x844~mobile-menu", comparisonProvider.Requests[0].Expected.Metadata["viewport_id"]);

        Assert.Equal("design-render-mobile-cart:expected:2", comparisonProvider.Requests[1].Expected.ImageId);
        Assert.Equal("artifact://same-size-identity/design-render-mobile-cart.png", comparisonProvider.Requests[1].Expected.Uri);
        Assert.Equal("sha256:design-render-mobile-cart", comparisonProvider.Requests[1].Expected.Sha256);
        Assert.Equal("source-390x844~mobile-cart", comparisonProvider.Requests[1].Expected.Metadata["viewport_id"]);
        Assert.NotEqual(comparisonProvider.Requests[0].Expected.Sha256, comparisonProvider.Requests[1].Expected.Sha256);

        var resultJson = JsonSerializer.Serialize(result.Value);
        Assert.DoesNotContain(privateMarker, resultJson, StringComparison.Ordinal);
        Assert.DoesNotContain(privateOutputPath, resultJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(privateMarker, comparisonProvider.Requests[0].Expected.Metadata.Values);
        Assert.DoesNotContain(privateOutputPath, comparisonProvider.Requests[0].Expected.Metadata.Values);

        var status = api.GetJobResult(result.Value.JobId);
        Assert.True(status.IsSuccess, status.ErrorMessage);
        Assert.Equal("3", status.Value!.BoundaryMetadata["multi_viewport_required_count"]);
        Assert.Equal("2", status.Value.BoundaryMetadata["multi_viewport_observed_count"]);
        Assert.Equal(bool.FalseString, status.Value.BoundaryMetadata["multi_viewport_complete"]);
        Assert.False(status.Value.ProductReady);
    }

    [Fact]
    public async Task SubmitAndEvaluateAsync_MissingRequiredViewportPreservesIncompleteMetadata()
    {
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(CreateVerdict("pass144-missing-viewport-verdict", 0.91, passed: true));
        var service = new DesignConversionInternalService(renderProvider, comparisonProvider, routingFlow: null);
        var api = new DesignConversionModelServerApi(
            service,
            () => DateTimeOffset.Parse("2026-07-11T00:00:00Z"),
            postEvaluationStateStore: TestPostEvaluationStateStore);
        var baseRequest = CreateRequest("pass144-missing-viewport-api-request");
        var requestMetadata = new Dictionary<string, string>(baseRequest.Metadata!, StringComparer.Ordinal)
        {
            [DesignConversionThinClientJobRuntime.ServerOwnedRequiredBrowserQaMatrixProvenanceKey] = bool.TrueString
        };
        var request = baseRequest with
        {
            Screenshots = CreateRequiredBrowserQaArtifacts("pass144-source", "screenshot"),
            DesignRenders = CreateRequiredBrowserQaArtifacts("pass144-design", "design-render"),
            Budget = baseRequest.Budget with { MaxRenderQaRuns = 1 },
            Metadata = requestMetadata
        };
        var callerRequiredBrowserQaMatrixKey =
            $"request_metadata:{DesignConversionThinClientJobRuntime.ServerOwnedRequiredBrowserQaMatrixProvenanceKey}";

        var result = await api.SubmitAndEvaluateAsync(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionThinClientStatus.NotReady, result.Value!.ContractStatus);
        Assert.Equal(
            DesignConversionThinClientStatus.NotReady,
            result.Value.BoundaryMetadata["runtime_contract_status"]);
        Assert.False(result.Value!.ProductReady);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["product_ready"]);
        Assert.Equal("4", result.Value!.BoundaryMetadata["multi_viewport_required_count"]);
        Assert.Equal("1", result.Value.BoundaryMetadata["multi_viewport_observed_count"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["multi_viewport_complete"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["browser_e2e_verified"]);
        Assert.Equal(
            "runtime_not_completed",
            result.Value.BoundaryMetadata["browser_e2e_failure_reason"]);
        AssertVisualFidelityFalse(result.Value.BoundaryMetadata, "runtime_not_completed");
        Assert.Equal(
            bool.FalseString,
            result.Value.RuntimeStatus.Provenance[DesignConversionThinClientJobRuntime.ServerOwnedRequiredBrowserQaMatrixProvenanceKey]);
        Assert.False(result.Value.RuntimeStatus.Provenance.ContainsKey(callerRequiredBrowserQaMatrixKey));
        Assert.Contains(
            $"tier_policy:stop_reason={DesignConversionVisualQaBoundaryPolicy.ProviderRequired}",
            result.Value.EvidenceRefs);
        Assert.Contains(
            "MULTI_RESOLUTION_RENDER_EVIDENCE_INCOMPLETE",
            result.Value.Warnings);
        Assert.Contains(
            DesignConversionVisualQaBoundaryPolicy.VisualJudgeProviderRequiredMarker,
            result.Value.EvidenceRefs);
        Assert.Equal(
            DesignConversionVisualQaBoundaryPolicy.DecisionBlocked,
            result.Value.RuntimeStatus.Provenance["final_visual_decision"]);
        Assert.Equal(
            DesignConversionVisualQaBoundaryPolicy.ProviderRequired,
            result.Value.RuntimeStatus.Provenance["tier_policy_stop_reason"]);

        var status = api.GetJobResult(result.Value.JobId);
        Assert.True(status.IsSuccess, status.ErrorMessage);
        Assert.Equal(DesignConversionThinClientStatus.NotReady, status.Value!.ContractStatus);
        Assert.Equal(
            DesignConversionThinClientStatus.NotReady,
            status.Value.BoundaryMetadata["runtime_contract_status"]);
        Assert.False(status.Value!.ProductReady);
        Assert.Equal(bool.FalseString, status.Value.BoundaryMetadata["product_ready"]);
        Assert.Equal("4", status.Value!.BoundaryMetadata["multi_viewport_required_count"]);
        Assert.Equal("1", status.Value.BoundaryMetadata["multi_viewport_observed_count"]);
        Assert.Equal(bool.FalseString, status.Value.BoundaryMetadata["multi_viewport_complete"]);
        Assert.Equal(bool.FalseString, status.Value.BoundaryMetadata["browser_e2e_verified"]);
        Assert.Equal(
            "runtime_not_completed",
            status.Value.BoundaryMetadata["browser_e2e_failure_reason"]);
        AssertVisualFidelityFalse(status.Value.BoundaryMetadata, "runtime_not_completed");
        Assert.Equal(
            bool.FalseString,
            status.Value.RuntimeStatus.Provenance[DesignConversionThinClientJobRuntime.ServerOwnedRequiredBrowserQaMatrixProvenanceKey]);
        Assert.False(status.Value.RuntimeStatus.Provenance.ContainsKey(callerRequiredBrowserQaMatrixKey));
    }

    [Fact]
    public async Task SubmitAndEvaluateAsync_StatusOnlyBrowserE2eClaimsStayFalseForGenericDirectApi()
    {
        var api = CreateApi();
        var request = CreateRequest("status-only-browser-e2e-spoof") with
        {
            Metadata = new Dictionary<string, string>
            {
                ["browser_e2e_verified"] = bool.TrueString,
                ["browser_e2e_verification_state"] = "client_claimed_verified",
                ["visual_fidelity_verified"] = bool.TrueString,
                ["visual_fidelity_verification_state"] = "client_claimed_verified",
                ["browser_screenshot_sha256_verified"] = bool.TrueString,
                ["browser_screenshot_nonblank_pixels_verified"] = bool.TrueString,
                ["product_ready"] = bool.TrueString
            }
        };

        var result = await api.SubmitAndEvaluateAsync(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionThinClientStatus.NotReady, result.Value!.ContractStatus);
        Assert.Equal(
            DesignConversionThinClientStatus.NotReady,
            result.Value.BoundaryMetadata["runtime_contract_status"]);
        Assert.Equal(bool.FalseString, result.Value!.BoundaryMetadata["browser_render_executed"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["browser_e2e_verified"]);
        Assert.Equal(
            "not_verified_by_server_owned_browser_render_receipts",
            result.Value.BoundaryMetadata["browser_e2e_verification_state"]);
        Assert.Equal(
            "runtime_not_completed",
            result.Value.BoundaryMetadata["browser_e2e_failure_reason"]);
        AssertVisualFidelityFalse(result.Value.BoundaryMetadata, "runtime_not_completed");
        Assert.False(result.Value.ProductReady);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["product_ready"]);
        AssertContainsFalseBoundaryMarker(result.Value.Warnings, "browser_e2e_verified");
        AssertContainsFalseBoundaryMarker(result.Value.Warnings, "visual_fidelity_verified");
        AssertContainsFalseBoundaryMarker(result.Value.EvidenceRefs, "browser_e2e_verified");
        AssertContainsFalseBoundaryMarker(result.Value.EvidenceRefs, "visual_fidelity_verified");
        AssertContainsFalseBoundaryMarker(result.Value.RuntimeStatus.Warnings, "browser_e2e_verified");
        AssertContainsFalseBoundaryMarker(result.Value.RuntimeStatus.Warnings, "visual_fidelity_verified");

        var status = api.GetJobResult(result.Value.JobId);
        Assert.True(status.IsSuccess, status.ErrorMessage);
        Assert.Equal(DesignConversionThinClientStatus.NotReady, status.Value!.ContractStatus);
        Assert.Equal(
            DesignConversionThinClientStatus.NotReady,
            status.Value.BoundaryMetadata["runtime_contract_status"]);
        Assert.Equal("runtime_not_completed", status.Value.BoundaryMetadata["browser_e2e_failure_reason"]);
        AssertVisualFidelityFalse(status.Value.BoundaryMetadata, "runtime_not_completed");
        AssertContainsFalseBoundaryMarker(status.Value.EvidenceRefs, "browser_e2e_verified");
        AssertContainsFalseBoundaryMarker(status.Value.EvidenceRefs, "visual_fidelity_verified");
        AssertContainsFalseBoundaryMarker(status.Value.RuntimeStatus.Warnings, "browser_e2e_verified");
        AssertContainsFalseBoundaryMarker(status.Value.RuntimeStatus.Warnings, "visual_fidelity_verified");
    }

    [Theory]
    [InlineData("source", "DESIGN_CONVERSION_SOURCE_TREE_REQUIRED")]
    [InlineData("original", "DESIGN_CONVERSION_ORIGINAL_TREE_REQUIRED")]
    [InlineData("html", "DESIGN_CONVERSION_HTML_REQUIRED")]
    [InlineData("css", "DESIGN_CONVERSION_CSS_REQUIRED")]
    [InlineData("screenshot", "DESIGN_CONVERSION_SCREENSHOT_OR_RENDER_REQUIRED")]
    public async Task SubmitAndEvaluateAsync_MissingRequiredInputs_FailsWithClearError(
        string missing,
        string expectedErrorCode)
    {
        var api = CreateApi();
        var request = CreateRequest($"pass117-missing-{missing}");
        request = missing switch
        {
            "source" => request with { DesignTree = null! },
            "original" => request with { OriginalTree = null! },
            "html" => request with { Html = "" },
            "css" => request with { Css = "" },
            "screenshot" => request with { Screenshots = [], DesignRenders = [] },
            _ => request
        };

        var result = await api.SubmitAndEvaluateAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal(expectedErrorCode, result.ErrorCode);
        Assert.Contains("DesignConversion", result.ErrorMessage);
    }

    [Fact]
    public async Task SubmitAndEvaluateAsync_UnsupportedArtifactProofRefs_FailBeforeRuntimeEvaluation()
    {
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(CreateVerdict("pass167-unsupported-artifact-ref-verdict", 0.91, passed: true));
        var service = new DesignConversionInternalService(renderProvider, comparisonProvider, routingFlow: null);
        var api = new DesignConversionModelServerApi(service, () => DateTimeOffset.Parse("2026-07-09T10:00:00Z"));
        var request = CreateRequest("pass167-unsupported-artifact-proof-ref");
        var unsupportedScreenshot = request.Screenshots![0] with
        {
            ArtifactId = "unsupported-screenshot-proof",
            Uri = "https://client.example.invalid/screenshot-proof.png",
            Sha256 = "sha256:unsupported-screenshot-proof",
            Metadata = new Dictionary<string, string>
            {
                ["proof_kind"] = "unsupported_external_uri_only"
            }
        };

        var result = await api.SubmitAndEvaluateAsync(request with
        {
            Screenshots = [unsupportedScreenshot],
            DesignRenders = []
        });

        Assert.False(result.IsSuccess);
        Assert.Equal("DESIGN_CONVERSION_ARTIFACT_REF_PROOF_REQUIRED", result.ErrorCode);
        Assert.Contains("refs-only", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(renderProvider.Requests);
        Assert.Empty(comparisonProvider.Requests);
    }

    [Fact]
    public async Task SubmitAndEvaluateAsync_ServerOwnedLocalArtifactProof_AcceptsInRootStrictPngWithMarkerLookingBusinessText()
    {
        var artifactRoot = CreateServerOwnedArtifactTempRoot();
        try
        {
            var fixture = await CreateServerOwnedPngArtifactAsync(artifactRoot, "server-owned-accepted");
            var metadata = (fixture.ArtifactRef.Metadata ?? new Dictionary<string, string>())
                .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
            metadata["business_account"] = "dummy_corp";
            metadata["campaign_label"] = "placeholder launch proof";
            var businessNamedProof = fixture.ArtifactRef with
            {
                ArtifactId = "dummy_corp-placeholder-campaign-screenshot",
                Metadata = metadata
            };

            var probe = await SubmitWithServerOwnedProofAsync(
                artifactRoot,
                businessNamedProof);

            Assert.True(probe.Result.IsSuccess, probe.Result.ErrorMessage);
            Assert.NotEmpty(probe.RenderProvider.Requests);
            Assert.NotEmpty(probe.ComparisonProvider.Requests);
        }
        finally
        {
            DeleteDirectoryIfExists(artifactRoot);
        }
    }

    [Fact]
    public async Task SubmitAndEvaluateAsync_ServerOwnedLocalArtifactProof_RejectsTraversalOutsideRootBeforeRuntime()
    {
        var tempRoot = CreateServerOwnedArtifactTempRoot();
        var artifactRoot = Path.Combine(tempRoot, "root");
        try
        {
            var fixture = await CreateServerOwnedPngArtifactAsync(artifactRoot, "server-owned-traversal");
            var outsidePath = Path.Combine(tempRoot, "outside.png");
            File.Copy(fixture.LocalPath, outsidePath);
            var traversalUri = DirectoryFileUri(artifactRoot) + "../outside.png";
            var probe = await SubmitWithServerOwnedProofAsync(
                artifactRoot,
                fixture.ArtifactRef with
                {
                    Uri = traversalUri,
                    Sha256 = Sha256FilePrefixed(outsidePath)
                });

            AssertServerOwnedProofRejectedBeforeRuntime(probe);
            Assert.Contains("traversal", probe.Result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            DeleteDirectoryIfExists(tempRoot);
        }
    }

    [Fact]
    public async Task SubmitAndEvaluateAsync_ServerOwnedLocalArtifactProof_RejectsOutsideRootFileBeforeRuntime()
    {
        var tempRoot = CreateServerOwnedArtifactTempRoot();
        var artifactRoot = Path.Combine(tempRoot, "root");
        var outsideRoot = Path.Combine(tempRoot, "outside-root");
        try
        {
            var fixture = await CreateServerOwnedPngArtifactAsync(outsideRoot, "server-owned-outside-root");

            var probe = await SubmitWithServerOwnedProofAsync(
                artifactRoot,
                fixture.ArtifactRef);

            AssertServerOwnedProofRejectedBeforeRuntime(probe);
            Assert.Contains("escaped", probe.Result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            DeleteDirectoryIfExists(tempRoot);
        }
    }

    [Fact]
    public async Task SubmitAndEvaluateAsync_ServerOwnedLocalArtifactProof_RejectsUncNetworkLocalRefBeforeRuntime()
    {
        var artifactRoot = CreateServerOwnedArtifactTempRoot();
        try
        {
            var networkArtifact = new DesignConversionModelServerArtifactRef(
                ArtifactId: "server-owned-unc-network",
                Kind: "screenshot",
                Width: 1440,
                Height: 900,
                Uri: @"\\design-conversion-proof.invalid\share\screenshot.png",
                Sha256: "sha256:" + new string('2', 64));

            var probe = await SubmitWithServerOwnedProofAsync(artifactRoot, networkArtifact);

            AssertServerOwnedProofRejectedBeforeRuntime(probe);
            Assert.Contains("network", probe.Result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            DeleteDirectoryIfExists(artifactRoot);
        }
    }

    [DesignConversionReparsePointFact]
    public async Task SubmitAndEvaluateAsync_ServerOwnedLocalArtifactProof_RejectsReparseFileEscapeBeforeRuntime()
    {
        var tempRoot = CreateServerOwnedArtifactTempRoot();
        var artifactRoot = Path.Combine(tempRoot, "root");
        var outsideRoot = Path.Combine(tempRoot, "outside-root");
        try
        {
            var fixture = await CreateServerOwnedPngArtifactAsync(outsideRoot, "server-owned-reparse-escape");
            Directory.CreateDirectory(artifactRoot);
            var symlinkPath = Path.Combine(artifactRoot, "linked-outside.png");
            try
            {
                File.CreateSymbolicLink(symlinkPath, fixture.LocalPath);
            }
            catch (Exception ex) when (ex is IOException or NotSupportedException or PlatformNotSupportedException or UnauthorizedAccessException)
            {
                throw new InvalidOperationException(
                    $"OS denied or failed creating DesignConversion reparse escape test symlink: {ex.GetType().FullName}: {ex.Message}",
                    ex);
            }

            var probe = await SubmitWithServerOwnedProofAsync(
                artifactRoot,
                fixture.ArtifactRef with
                {
                    Uri = new Uri(symlinkPath).AbsoluteUri
                });

            AssertServerOwnedProofRejectedBeforeRuntime(probe);
            Assert.Contains("reparse", probe.Result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            DeleteDirectoryIfExists(tempRoot);
        }
    }

    [Fact]
    public async Task SubmitAndEvaluateAsync_ServerOwnedLocalArtifactProof_RejectsArtifactUriBeforeRuntime()
    {
        var artifactRoot = CreateServerOwnedArtifactTempRoot();
        try
        {
            var fixture = await CreateServerOwnedPngArtifactAsync(artifactRoot, "server-owned-artifact-uri");

            var probe = await SubmitWithServerOwnedProofAsync(
                artifactRoot,
                fixture.ArtifactRef with
                {
                    Uri = "artifact://pass/server-owned/screenshot.png"
                });

            AssertServerOwnedProofRejectedBeforeRuntime(probe);
            Assert.Contains("file refs", probe.Result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            DeleteDirectoryIfExists(artifactRoot);
        }
    }

    [Fact]
    public async Task SubmitAndEvaluateAsync_ServerOwnedLocalArtifactProof_RejectsHashMismatchBeforeRuntime()
    {
        var artifactRoot = CreateServerOwnedArtifactTempRoot();
        try
        {
            var fixture = await CreateServerOwnedPngArtifactAsync(artifactRoot, "server-owned-hash-mismatch");

            var probe = await SubmitWithServerOwnedProofAsync(
                artifactRoot,
                fixture.ArtifactRef with
                {
                    Sha256 = "sha256:" + new string('0', 64)
                });

            AssertServerOwnedProofRejectedBeforeRuntime(probe);
            Assert.Contains("SHA-256", probe.Result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            DeleteDirectoryIfExists(artifactRoot);
        }
    }

    [Fact]
    public async Task SubmitAndEvaluateAsync_ServerOwnedLocalArtifactProof_RejectsMissingLocalFileBeforeRuntime()
    {
        var artifactRoot = CreateServerOwnedArtifactTempRoot();
        try
        {
            var missingPath = Path.Combine(artifactRoot, "missing.png");
            var missingArtifact = new DesignConversionModelServerArtifactRef(
                ArtifactId: "server-owned-missing",
                Kind: "screenshot",
                Width: 1440,
                Height: 900,
                Uri: new Uri(missingPath).AbsoluteUri,
                Sha256: "sha256:" + new string('1', 64));

            var probe = await SubmitWithServerOwnedProofAsync(artifactRoot, missingArtifact);

            AssertServerOwnedProofRejectedBeforeRuntime(probe);
            Assert.Contains("not found", probe.Result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            DeleteDirectoryIfExists(artifactRoot);
        }
    }

    [Fact]
    public async Task SubmitAndEvaluateAsync_ServerOwnedLocalArtifactProof_RejectsNonPngFileBeforeRuntime()
    {
        var artifactRoot = CreateServerOwnedArtifactTempRoot();
        try
        {
            Directory.CreateDirectory(artifactRoot);
            var textPath = Path.Combine(artifactRoot, "not-a-png.txt");
            await File.WriteAllTextAsync(textPath, "not a png");
            var textArtifact = new DesignConversionModelServerArtifactRef(
                ArtifactId: "server-owned-non-png",
                Kind: "screenshot",
                Width: 1440,
                Height: 900,
                Uri: new Uri(textPath).AbsoluteUri,
                Sha256: Sha256FilePrefixed(textPath));

            var probe = await SubmitWithServerOwnedProofAsync(artifactRoot, textArtifact);

            AssertServerOwnedProofRejectedBeforeRuntime(probe);
            Assert.Contains("PNG", probe.Result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            DeleteDirectoryIfExists(artifactRoot);
        }
    }

    [Fact]
    public async Task SubmitAndEvaluateAsync_ServerOwnedLocalArtifactProof_RejectsBlankPngBeforeRuntime()
    {
        var artifactRoot = CreateServerOwnedArtifactTempRoot();
        try
        {
            var blankArtifact = CreateBlankServerOwnedPngArtifact(
                artifactRoot,
                "server-owned-blank-png");

            var probe = await SubmitWithServerOwnedProofAsync(artifactRoot, blankArtifact);

            AssertServerOwnedProofRejectedBeforeRuntime(probe);
            Assert.Contains("nonblank", probe.Result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            DeleteDirectoryIfExists(artifactRoot);
        }
    }

    [Fact]
    public async Task SubmitAndEvaluateAsync_ServerOwnedLocalArtifactProof_RejectsOversizeBeforeRuntime()
    {
        var artifactRoot = CreateServerOwnedArtifactTempRoot();
        try
        {
            var fixture = await CreateServerOwnedPngArtifactAsync(artifactRoot, "server-owned-oversize");
            var maxBytes = Math.Max(1, new FileInfo(fixture.LocalPath).Length - 1);

            var probe = await SubmitWithServerOwnedProofAsync(
                artifactRoot,
                fixture.ArtifactRef,
                maxArtifactBytes: maxBytes);

            AssertServerOwnedProofRejectedBeforeRuntime(probe);
            Assert.Contains("size bound", probe.Result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            DeleteDirectoryIfExists(artifactRoot);
        }
    }

    [Fact]
    public async Task SubmitAndEvaluateAsync_ServerOwnedLocalArtifactProof_RejectsMalformedEscapedFileRefBeforeRuntimeWithoutRawPathLeakage()
    {
        var artifactRoot = CreateServerOwnedArtifactTempRoot();
        try
        {
            var fixture = await CreateServerOwnedPngArtifactAsync(artifactRoot, "server-owned-malformed-escape");

            var probe = await SubmitWithServerOwnedProofAsync(
                artifactRoot,
                fixture.ArtifactRef with
                {
                    Uri = DirectoryFileUri(artifactRoot) + "%"
                });

            AssertServerOwnedProofRejectedBeforeRuntime(probe);
            Assert.DoesNotContain(artifactRoot, probe.Result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            DeleteDirectoryIfExists(artifactRoot);
        }
    }

    [Fact]
    public async Task SubmitAndEvaluateAsync_ServerOwnedLocalArtifactProof_RejectsDeclaredDimensionsFromDifferentPngBeforeRuntime()
    {
        var artifactRoot = CreateServerOwnedArtifactTempRoot();
        try
        {
            var declared = await CreateServerOwnedPngArtifactAsync(
                artifactRoot,
                "server-owned-declared-dimensions",
                width: 1440,
                height: 900);
            var actual = await CreateServerOwnedPngArtifactAsync(
                artifactRoot,
                "server-owned-actual-dimensions",
                width: 800,
                height: 600);

            var probe = await SubmitWithServerOwnedProofAsync(
                artifactRoot,
                actual.ArtifactRef with
                {
                    Width = declared.ArtifactRef.Width,
                    Height = declared.ArtifactRef.Height
                });

            AssertServerOwnedProofRejectedBeforeRuntime(probe);
            Assert.Contains("dimensions", probe.Result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            DeleteDirectoryIfExists(artifactRoot);
        }
    }

    // Постоянный замок границы подделки. Метка "репозиторный договор обучающих
    // пар действителен" внесена в список серверных фактов без права на власть,
    // поэтому детектор её не судит. Эта проверка закрепляет, что внесение не
    // стало дырой: присланная клиентом метка с тем же именем серверной не
    // становится и детектор её по-прежнему ловит.
    [Fact]
    public async Task SubmitAndEvaluateAsync_ClientMetadataCannotForgeServerOwnedRepositoryOwnedDpoValidity()
    {
        var service = new DesignConversionInternalService(
            new RecordingRenderProvider(),
            new RecordingComparisonProvider(
                CreateVerdict("client-forge-guard-verdict", 0.91, passed: true)),
            routingFlow: null);
        var api = new DesignConversionModelServerApi(
            service,
            () => DateTimeOffset.Parse("2026-11-09T10:00:00Z"),
            postEvaluationStateStore: TestPostEvaluationStateStore);
        // Клиент пытается прислать ровно то имя, которое внесено в список
        // разрешённых серверных фактов. Значение берём ПРОТИВОПОЛОЖНОЕ тому,
        // что вычислит сервер: только так видно, чьё значение победило.
        var forgedKey = DesignConversionRepositoryOwnedDpoTransactionContext
            .ValidationMetadataKey;
        var serverComputedValue = DesignConversionRepositoryOwnedDpoTransactionContext
            .Disabled.Validate().IsSuccess.ToString();
        Assert.Equal(bool.TrueString, serverComputedValue);
        var request = CreateRequest("client-forge-guard-request") with
        {
            Metadata = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [forgedKey] = bool.FalseString
            }
        };

        var result = await api.SubmitAndEvaluateAsync(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var provenance = result.Value!.RuntimeStatus.Provenance;

        // Первое и главное: голое имя остаётся за серверной стороной. Клиент
        // прислал False, сервер вычислил True — в происхождении состояния
        // стоит серверное значение, значит присланным клиент его не подменил.
        Assert.Equal(serverComputedValue, provenance[forgedKey]);

        // Второе: пара меток связна. Режим несёт "включён", и он правдиво
        // False, потому что договора в запросе не было.
        Assert.Equal(
            bool.FalseString,
            provenance[DesignConversionRepositoryOwnedDpoTransactionContext
                .EnabledMetadataKey]);

        // Третье: присланное клиентом в происхождение состояния не попадает
        // вовсе — ни голым именем, ни под пространством имён запроса. Это и
        // есть причина, по которой внесение серверного факта в список
        // разрешённых не стало дырой подделки.
        Assert.DoesNotContain(
            "request_metadata:" + forgedKey,
            provenance.Keys);

        // Четвёртое: заявка об обученном выводе не возникает, то есть внесение
        // метки в список работает и попытка клиента её не оживила.
        Assert.Equal(
            bool.FalseString,
            result.Value.BoundaryMetadata[
                "learned_inference_status_provenance_claim_present"]);
        Assert.Equal(
            bool.FalseString,
            result.Value.BoundaryMetadata[
                "learned_inference_status_provenance_claim_accepted"]);
    }

    [Fact]
    public async Task SubmitAndEvaluateAsync_LearnedRuntimeFlow_IsInjectableAndSurfacesLearnedEvidence()
    {
        var machine = new DesignConversionMachine();
        var training = await DesignConversionLegalTeacherCapabilityTestFactory
            .TrainMachineStrictAsync(machine, new LearningBatch<DesignConversionDomain.DesignConversionDomain>(
            BatchId: "pass117-learned-api-batch",
            DomainTag: DesignConversionMachine.DomainTag,
            TrainingItems:
            [
                new LearningTrainingItem(
                    ItemId: "pass117-learned-critic",
                    Prompt: JsonSerializer.Serialize(new Dictionary<string, object>
                    {
                        ["unit_id"] = DesignConversionRuntimeScaffoldCriticUnit.UnitId,
                        ["source_kind"] = "Figma",
                        ["source_manifest_hash"] = "pass117-source-manifest",
                        ["source_identity_hash"] = "pass117-source-identity",
                        ["source_structure_hash"] = "pass117-source-structure"
                    }),
                    ChosenResponse: "{\"html\":\"<main class=\\\"layout-a\\\"></main>\",\"css\":\".layout-a{display:grid;gap:16px}\"}",
                    RejectedResponse: "{\"html\":\"<main class=\\\"layout-b\\\"></main>\",\"css\":\".layout-b{display:block;gap:2px}\"}")
            ],
            SourceIds: ["pass117-source-disjoint-local"]));
        Assert.True(training.IsSuccess, training.ErrorMessage);

        var checkpoint = machine.ExportCheckpoint();
        Assert.True(checkpoint.IsSuccess, checkpoint.ErrorMessage);
        Assert.True(DesignConversionMachine.CheckpointHasRealLearnedWeights(checkpoint.Value));

        var service = new DesignConversionInternalService(
            new RecordingRenderProvider(),
            new RecordingComparisonProvider(CreateVerdict("pass117-learned-verdict", 0.82, passed: true)),
            machine.CreateRuntimeRoutingFlow());
        var api = new DesignConversionModelServerApi(
            service,
            () => DateTimeOffset.Parse("2026-07-02T10:00:00Z"),
            postEvaluationStateStore: TestPostEvaluationStateStore);
        var request = CreateRequest("pass117-learned-api-request");

        var result = await api.SubmitAndEvaluateAsync(request with
        {
            Budget = request.Budget! with { MaxTeacherCalls = 1 }
        });

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.True(result.Value!.LearnedInferenceApplied);
        Assert.False(result.Value.TrainedModelAuthority);
        Assert.True(result.Value.NotTrainedModelAuthority);
        Assert.False(result.Value.ModelTrainingVerified);
        Assert.False(result.Value.ModelUploadReady);
        Assert.False(result.Value.DeploymentVerified);
        Assert.False(result.Value.ProductReady);
        Assert.Equal(
            bool.TrueString,
            result.Value.RuntimeStatus.Provenance[
                DesignConversionImprovementLoopCoordinator.MaxTeacherCallsRuntimeEnforcementMetadataKey]);
        Assert.Contains(DesignConversionMachine.LearnedInferenceAppliedEvidence, result.Value.EvidenceRefs);
        Assert.Equal(bool.TrueString, result.Value.BoundaryMetadata["LearnedInferenceApplied"]);
        Assert.Equal(bool.TrueString, result.Value.BoundaryMetadata["learned_inference_server_owned_evidence_verified"]);
        Assert.False(
            bool.Parse(result.Value.BoundaryMetadata["learned_inference_status_provenance_claim_present"]),
            $"Unexpected truthy status provenance: {TruthyProvenanceKeys(result.Value.RuntimeStatus.Provenance)}");
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["learned_inference_status_provenance_claim_accepted"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["trained_model_authority"]);
        Assert.Equal(bool.TrueString, result.Value.BoundaryMetadata["not_trained_model_authority"]);
    }

    [Theory]
    [MemberData(nameof(ForgedLearnedStatusProvenanceCases))]
    public async Task StatusReadback_ForgedLearnedStatusProvenance_DoesNotClaimLearnedInference(
        string caseName,
        IReadOnlyDictionary<string, string> forgedProvenance)
    {
        var runtime = new DesignConversionThinClientJobRuntime(
            new DesignConversionInternalService(
                new RecordingRenderProvider(),
                new RecordingComparisonProvider(CreateVerdict($"{caseName}-verdict", 0.91, passed: true)),
                routingFlow: null),
            () => DateTimeOffset.Parse("2026-07-10T10:00:00Z"));
        var submitted = CreateSubmittedRuntimeRequest($"{caseName}-request", forgedProvenance);
        var submit = runtime.Submit(submitted);
        Assert.True(submit.IsSuccess, submit.ErrorMessage);
        var run = await runtime.RunAsync(submit.Value!.JobId);
        Assert.True(run.IsSuccess, run.ErrorMessage);

        var api = new DesignConversionModelServerApi(runtime);
        var status = api.GetJobResult(submit.Value.JobId);
        Assert.True(status.IsSuccess, status.ErrorMessage);
        Assert.False(status.Value!.LearnedInferenceApplied);
        Assert.False(status.Value.ProductReady);
        Assert.False(status.Value.TrainedModelAuthority);
        Assert.True(status.Value.NotTrainedModelAuthority);
        Assert.Equal(bool.FalseString, status.Value.BoundaryMetadata["learned_inference_applied"]);
        Assert.Equal(bool.FalseString, status.Value.BoundaryMetadata["LearnedInferenceApplied"]);
        Assert.Equal(bool.FalseString, status.Value.BoundaryMetadata["learned_inference_server_owned_evidence_verified"]);
        Assert.Equal(bool.TrueString, status.Value.BoundaryMetadata["learned_inference_status_provenance_claim_present"]);
        Assert.Equal(bool.FalseString, status.Value.BoundaryMetadata["learned_inference_status_provenance_claim_accepted"]);
        Assert.Equal(bool.TrueString, status.Value.BoundaryMetadata["learned_inference_unverified_status_provenance_downgraded"]);
        Assert.Equal(bool.FalseString, status.Value.BoundaryMetadata["model_training_verified"]);
        Assert.Equal(bool.FalseString, status.Value.BoundaryMetadata["product_ready"]);
        Assert.Equal(bool.FalseString, status.Value.BoundaryMetadata[DesignConversionModelAuthorityBoundary.TrainedModelAuthorityKey]);
        Assert.Equal(bool.TrueString, status.Value.BoundaryMetadata[DesignConversionModelAuthorityBoundary.NotTrainedModelAuthorityKey]);

        var handler = new DesignConversionModelServerStatusHandler(api);
        var response = await handler.HandleAsync(new DesignConversionModelServerHttpRequest(
            Method: "GET",
            Path: $"/api/design-conversion/status/{submit.Value.JobId}",
            BodyJson: null));
        using var document = JsonDocument.Parse(response.BodyJson);
        var root = document.RootElement;
        Assert.Equal(200, root.GetProperty("statusCode").GetInt32());
        Assert.False(root.GetProperty("learnedInferenceApplied").GetBoolean());
        Assert.False(root.GetProperty("productReady").GetBoolean());
        var metadata = root.GetProperty("boundaryMetadata");
        Assert.Equal(bool.FalseString, metadata.GetProperty("learned_inference_applied").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("learned_inference_status_provenance_claim_present").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("learned_inference_status_provenance_claim_accepted").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("learned_inference_unverified_status_provenance_downgraded").GetString());
        Assert.False(root.GetProperty("trainedModelAuthority").GetBoolean());
        Assert.True(root.GetProperty("notTrainedModelAuthority").GetBoolean());
        Assert.Equal(bool.FalseString, metadata.GetProperty(DesignConversionModelAuthorityBoundary.TrainedModelAuthorityKey).GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty(DesignConversionModelAuthorityBoundary.NotTrainedModelAuthorityKey).GetString());
    }

    [Fact]
    public async Task DirectDpoAuthority_ServerOwnedRuntimeEvidencePropagatesThroughApiHttpAndStatus()
    {
        var evaluator = new FixedThinClientJobEvaluator(CreateServerOwnedDirectDpoAuthorityEvaluation);
        var api = new DesignConversionModelServerApi(
            evaluator,
            () => DateTimeOffset.Parse("2026-07-10T10:20:00Z"),
            postEvaluationStateStore: TestPostEvaluationStateStore);
        var forgedRequestMetadata = new Dictionary<string, string>
        {
            [DesignConversionModelAuthorityBoundary.TrainedModelAuthorityKey] = bool.FalseString,
            [DesignConversionModelAuthorityBoundary.NotTrainedModelAuthorityKey] = bool.TrueString,
            [DesignConversionModelAuthorityBoundary.BoundarySourceKey] =
                DesignConversionModelAuthorityBoundary.NoTrustedDirectDpoAuthoritySource
        };
        var request = CreateRequest("direct-dpo-authority-server-owned-request") with
        {
            Metadata = forgedRequestMetadata
        };

        var result = await api.SubmitAndEvaluateAsync(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.True(result.Value!.LearnedInferenceApplied);
        Assert.True(result.Value.TrainedModelAuthority);
        Assert.False(result.Value.NotTrainedModelAuthority);
        Assert.False(result.Value.ModelUploadReady);
        Assert.False(result.Value.DeploymentVerified);
        Assert.False(result.Value.ProductReady);
        Assert.Equal(bool.TrueString, result.Value.BoundaryMetadata[DesignConversionModelAuthorityBoundary.TrainedModelAuthorityKey]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata[DesignConversionModelAuthorityBoundary.NotTrainedModelAuthorityKey]);
        Assert.Equal(
            DesignConversionModelAuthorityBoundary.TrustedDirectDpoRuntimeFlowAuthoritySource,
            result.Value.BoundaryMetadata[DesignConversionModelAuthorityBoundary.BoundarySourceKey]);
        Assert.Equal(bool.TrueString, result.Value.BoundaryMetadata["direct_dpo_policy_acceptance_authority"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["baseline_visual_verdict_acceptance_authority"]);
        Assert.Contains(DesignConversionMachine.DpoPolicyDirectInferenceAcceptanceAuthorityEvidence, result.Value.EvidenceRefs);

        var jobStatus = await api.GetJobResultAsync(result.Value.JobId);
        Assert.True(jobStatus.IsSuccess, jobStatus.ErrorMessage);
        Assert.True(jobStatus.Value!.TrainedModelAuthority);
        Assert.False(jobStatus.Value.NotTrainedModelAuthority);
        Assert.False(jobStatus.Value.DeploymentVerified);
        Assert.False(jobStatus.Value.ProductReady);
        Assert.Equal(bool.TrueString, jobStatus.Value.BoundaryMetadata[DesignConversionModelAuthorityBoundary.TrainedModelAuthorityKey]);
        Assert.Equal(bool.FalseString, jobStatus.Value.BoundaryMetadata[DesignConversionModelAuthorityBoundary.NotTrainedModelAuthorityKey]);

        var evaluateHandler = new DesignConversionModelServerHttpHandler(api);
        var httpRequest = request with
        {
            RequestId = "direct-dpo-authority-server-owned-http-request"
        };
        var evaluateResponse = await evaluateHandler.HandleAsync(new DesignConversionModelServerHttpRequest(
            Method: "POST",
            Path: DesignConversionModelServerHttpHandler.EndpointPath,
            BodyJson: JsonSerializer.Serialize(httpRequest, DesignConversionModelServerHttpHandler.JsonOptions)));
        Assert.Equal(200, evaluateResponse.StatusCode);
        using var evaluateDocument = JsonDocument.Parse(evaluateResponse.BodyJson);
        var evaluateRoot = evaluateDocument.RootElement;
        var evaluateMetadata = evaluateRoot.GetProperty("boundaryMetadata");
        Assert.True(evaluateRoot.GetProperty("trainedModelAuthority").GetBoolean());
        Assert.False(evaluateRoot.GetProperty("notTrainedModelAuthority").GetBoolean());
        Assert.False(evaluateRoot.GetProperty("modelUploadReady").GetBoolean());
        Assert.False(evaluateRoot.GetProperty("deploymentVerified").GetBoolean());
        Assert.False(evaluateRoot.GetProperty("productReady").GetBoolean());
        Assert.Equal(bool.TrueString, evaluateMetadata.GetProperty(DesignConversionModelAuthorityBoundary.TrainedModelAuthorityKey).GetString());
        Assert.Equal(bool.FalseString, evaluateMetadata.GetProperty(DesignConversionModelAuthorityBoundary.NotTrainedModelAuthorityKey).GetString());

        var statusHandler = new DesignConversionModelServerStatusHandler(api);
        var statusResponse = await statusHandler.HandleAsync(new DesignConversionModelServerHttpRequest(
            Method: "GET",
            Path: $"/api/design-conversion/status/{evaluateRoot.GetProperty("jobId").GetString()}",
            BodyJson: null));
        using var statusDocument = JsonDocument.Parse(statusResponse.BodyJson);
        var statusRoot = statusDocument.RootElement;
        var statusMetadata = statusRoot.GetProperty("boundaryMetadata");
        Assert.True(statusRoot.GetProperty("trainedModelAuthority").GetBoolean());
        Assert.False(statusRoot.GetProperty("notTrainedModelAuthority").GetBoolean());
        Assert.False(statusRoot.GetProperty("deploymentVerified").GetBoolean());
        Assert.False(statusRoot.GetProperty("productReady").GetBoolean());
        Assert.Equal(bool.TrueString, statusMetadata.GetProperty(DesignConversionModelAuthorityBoundary.TrainedModelAuthorityKey).GetString());
        Assert.Equal(bool.FalseString, statusMetadata.GetProperty(DesignConversionModelAuthorityBoundary.NotTrainedModelAuthorityKey).GetString());
    }

    [Fact]
    public async Task SubmitAndEvaluateAsync_CallerForgedAuthorityMetadataDoesNotClaimTrainedAuthority()
    {
        var service = new DesignConversionInternalService(
            new RecordingRenderProvider(),
            new RecordingComparisonProvider(CreateVerdict("caller-forged-authority-metadata-verdict", 0.91, passed: true)),
            routingFlow: null);
        var api = new DesignConversionModelServerApi(
            service,
            () => DateTimeOffset.Parse("2026-07-10T10:25:00Z"),
            postEvaluationStateStore: TestPostEvaluationStateStore);
        var request = CreateRequest("caller-forged-authority-metadata-request") with
        {
            Metadata = new Dictionary<string, string>
            {
                [DesignConversionModelAuthorityBoundary.TrainedModelAuthorityKey] = bool.TrueString,
                [DesignConversionModelAuthorityBoundary.NotTrainedModelAuthorityKey] = bool.FalseString,
                [DesignConversionModelAuthorityBoundary.BoundarySourceKey] =
                    DesignConversionModelAuthorityBoundary.TrustedDirectDpoRuntimeFlowAuthoritySource
            }
        };

        var result = await api.SubmitAndEvaluateAsync(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.False(result.Value!.TrainedModelAuthority);
        Assert.True(result.Value.NotTrainedModelAuthority);
        Assert.False(result.Value.ProductReady);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata[DesignConversionModelAuthorityBoundary.TrainedModelAuthorityKey]);
        Assert.Equal(bool.TrueString, result.Value.BoundaryMetadata[DesignConversionModelAuthorityBoundary.NotTrainedModelAuthorityKey]);
        Assert.Equal(
            DesignConversionModelAuthorityBoundary.NoTrustedDirectDpoAuthoritySource,
            result.Value.BoundaryMetadata[DesignConversionModelAuthorityBoundary.BoundarySourceKey]);
    }

    [Fact]
    public async Task StatusReadback_ServerOwnedBenignProvenanceFacts_DoNotClaimLearnedInference()
    {
        var runtime = new DesignConversionThinClientJobRuntime(
            new DesignConversionInternalService(
                new RecordingRenderProvider(),
                new RecordingComparisonProvider(CreateVerdict("benign-server-owned-provenance-verdict", 0.91, passed: true)),
                routingFlow: null),
            () => DateTimeOffset.Parse("2026-07-10T10:00:00Z"));
        var submitted = CreateSubmittedRuntimeRequest(
            "benign-server-owned-provenance-request",
            new Dictionary<string, string>
            {
                [DesignConversionModelServerApi.ServerOwnedRouteAdmissionTenantBoundProvenanceKey] = bool.TrueString,
                [DesignConversionImprovementLoopCoordinator.MaxTeacherCallsContractSuppliedMetadataKey] = bool.TrueString,
                [DesignConversionImprovementLoopCoordinator.MaxTeacherCallsRuntimeEnforcementMetadataKey] = bool.TrueString,
                [DesignConversionImprovementLoopCoordinator.MaxTeacherCallsCallCapEnforcedMetadataKey] = bool.TrueString,
                [DesignConversionImprovementLoopCoordinator.MaxTeacherCallsCapReachedMetadataKey] = bool.TrueString,
                ["visual_cost_ledger_measurement_complete"] = bool.TrueString,
                ["visual_cost_ledger_persistence_verified"] = bool.TrueString,
                ["local_artifact_db_status_restored"] = bool.TrueString,
                ["local_artifact_db_persistence_verified"] = bool.TrueString,
                ["storage_training_artifact_db"] = bool.TrueString,
                ["local_artifact_db"] = bool.TrueString,
                ["external_ai_visual_judge_executed"] = bool.TrueString,
                ["runtime_created_dpo_candidate"] = bool.TrueString,
                ["runtime_created_dpo_candidate_review_pending"] = bool.TrueString,
                ["independent_preference_proven"] = bool.TrueString,
                ["review_required"] = bool.TrueString,
                ["approval_required"] = bool.TrueString,
                ["training_candidate_only"] = bool.TrueString,
                ["not_training_grade"] = bool.TrueString,
                ["high_quality_negative_pair_not_proven"] = bool.TrueString,
                ["held_out_not_proven"] = bool.TrueString,
                ["source_disjoint_not_proven"] = bool.TrueString,
                ["product_not_ready"] = bool.TrueString,
                ["unknown_future_authority_key"] = bool.FalseString,
                ["unknown_future_authority_scalar"] = "checkpoint_promoted"
            });
        var submit = runtime.Submit(submitted);
        Assert.True(submit.IsSuccess, submit.ErrorMessage);
        var run = await runtime.RunAsync(submit.Value!.JobId);
        Assert.True(run.IsSuccess, run.ErrorMessage);

        var api = new DesignConversionModelServerApi(runtime);
        var status = api.GetJobResult(submit.Value.JobId);
        Assert.True(status.IsSuccess, status.ErrorMessage);
        Assert.False(status.Value!.LearnedInferenceApplied);
        Assert.False(status.Value.ProductReady);
        Assert.False(status.Value.TrainedModelAuthority);
        Assert.True(status.Value.NotTrainedModelAuthority);
        Assert.Equal(bool.FalseString, status.Value.BoundaryMetadata["learned_inference_applied"]);
        Assert.Equal(bool.FalseString, status.Value.BoundaryMetadata["LearnedInferenceApplied"]);
        Assert.Equal(bool.FalseString, status.Value.BoundaryMetadata["learned_inference_server_owned_evidence_verified"]);
        Assert.False(
            bool.Parse(status.Value.BoundaryMetadata["learned_inference_status_provenance_claim_present"]),
            $"Unexpected truthy status provenance: {TruthyProvenanceKeys(status.Value.RuntimeStatus.Provenance)}");
        Assert.Equal(bool.FalseString, status.Value.BoundaryMetadata["learned_inference_status_provenance_claim_accepted"]);
        Assert.Equal(bool.FalseString, status.Value.BoundaryMetadata["learned_inference_unverified_status_provenance_downgraded"]);
    }

    [Fact]
    public async Task DirectInternalServiceRoute_PreservesLegacyExecutionMetadataOnly()
    {
        var critic = new MetadataRecordingCriticUnit();
        var service = new DesignConversionInternalService(
            new RecordingRenderProvider(),
            new RecordingComparisonProvider(CreateVerdict("direct-metadata-verdict", 0.91, passed: true)),
            new DesignConversionRoutingFlow(critic));
        var api = new DesignConversionModelServerApi(
            service,
            () => DateTimeOffset.Parse("2026-07-07T09:59:00Z"),
            postEvaluationStateStore: TestPostEvaluationStateStore);

        var result = await api.SubmitAndEvaluateAsync(CreateRequest("direct-metadata-request"));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.NotNull(critic.Metadata);
        Assert.Equal(2, critic.Metadata.Count);
        Assert.Equal(nameof(DesignConversionInternalService), critic.Metadata["internal_service"]);
        Assert.Equal(DesignConversionInternalService.ServiceFirstMarker, critic.Metadata["service_first_marker"]);
        Assert.DoesNotContain("runtime_flow_input", critic.Metadata.Keys);
        Assert.DoesNotContain(DesignConversionRuntimeFlowResultCapture.MetadataKey, critic.Metadata.Keys);
    }

    [Fact]
    public async Task CreateMachineBackedLocal_WiresMachineRuntimeFlowIntoServerApi()
    {
        var machine = new DesignConversionMachine();
        var training = await DesignConversionLegalTeacherCapabilityTestFactory
            .TrainMachineStrictAsync(machine, new LearningBatch<DesignConversionDomain.DesignConversionDomain>(
            BatchId: "pass151-machine-backed-api-batch",
            DomainTag: DesignConversionMachine.DomainTag,
            TrainingItems:
            [
                new LearningTrainingItem(
                    ItemId: "pass151-machine-backed-critic",
                    Prompt: JsonSerializer.Serialize(new Dictionary<string, object>
                    {
                        ["unit_id"] = DesignConversionRuntimeScaffoldCriticUnit.UnitId,
                        ["source_kind"] = "Figma",
                        ["source_manifest_hash"] = "pass151-source-manifest",
                        ["source_identity_hash"] = "pass151-source-identity",
                        ["source_structure_hash"] = "pass151-source-structure"
                    }),
                    ChosenResponse: "{\"html\":\"<main class=\\\"layout-a\\\"></main>\",\"css\":\".layout-a{display:grid;gap:16px}\"}",
                    RejectedResponse: "{\"html\":\"<main class=\\\"layout-b\\\"></main>\",\"css\":\".layout-b{display:block;gap:2px}\"}")
            ],
            SourceIds: ["pass151-source-disjoint-local"]));
        Assert.True(training.IsSuccess, training.ErrorMessage);

        var checkpoint = machine.ExportCheckpoint();
        Assert.True(checkpoint.IsSuccess, checkpoint.ErrorMessage);
        Assert.True(DesignConversionMachine.CheckpointHasRealLearnedWeights(checkpoint.Value));

        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(CreateVerdict("pass151-machine-backed-verdict", 0.62, passed: false));
        var api = DesignConversionModelServerApi.CreateMachineBackedLocal(
            machine,
            renderProvider,
            comparisonProvider,
            utcNow: () => DateTimeOffset.Parse("2026-07-07T10:00:00Z"),
            postEvaluationStateStore: TestPostEvaluationStateStore);
        var request = CreateRequest("pass151-machine-backed-api-request");

        var result = await api.SubmitAndEvaluateAsync(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.NotEmpty(renderProvider.Requests);
        Assert.NotEmpty(comparisonProvider.Requests);
        Assert.True(result.Value!.LearnedInferenceApplied);
        const string expectedImprovedHtml = "<main class=\"screen dcm-local-improved\"><h1>Dashboard</h1></main>";
        var expectedImprovedCss = $"{request.Css}{Environment.NewLine}.dcm-local-improved{{gap:12px;padding:16px;}}{Environment.NewLine}";
        static string ContentSha256(string content)
            => "sha256:" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(content))).ToLowerInvariant();
        var improvedHtmlArtifact = Assert.Single(
            result.Value.RuntimeStatus.Artifacts,
            artifact => string.Equals(artifact.Kind, "improved-html", StringComparison.Ordinal));
        var improvedCssArtifact = Assert.Single(
            result.Value.RuntimeStatus.Artifacts,
            artifact => string.Equals(artifact.Kind, "improved-css", StringComparison.Ordinal));
        Assert.Equal(ContentSha256(expectedImprovedHtml), improvedHtmlArtifact.Sha256);
        Assert.Equal(ContentSha256(expectedImprovedCss), improvedCssArtifact.Sha256);
        Assert.NotEqual(ContentSha256(request.Html), improvedHtmlArtifact.Sha256);
        Assert.NotEqual(ContentSha256(request.Css), improvedCssArtifact.Sha256);
        Assert.False(result.Value.TrainedModelAuthority);
        Assert.True(result.Value.NotTrainedModelAuthority);
        Assert.False(result.Value.ProductReady);
        Assert.False(result.Value.ModelUploadReady);
        Assert.Contains(DesignConversionMachine.LearnedInferenceAppliedEvidence, result.Value.EvidenceRefs);
        Assert.Contains(DesignConversionMachine.LocalCheckpointFeatureAdjustmentEvidence, result.Value.EvidenceRefs);
        Assert.Contains("logical-flow:design-conversion-routing-flow", result.Value.EvidenceRefs);
        Assert.Contains("logical-orchestrator-selected-flow:design-conversion-routing-flow", result.Value.EvidenceRefs);
        Assert.Contains(DesignConversionMachine.RuntimeScaffoldNotTrainedModelWarning, result.Value.Warnings);
        Assert.Contains(
            "DESIGN_CONVERSION_RUNTIME_LOCAL_CHECKPOINT_FEATURE_ADJUSTMENT_APPLIED_NOT_TRAINED_AUTHORITY",
            result.Value.Warnings);
        Assert.Equal(bool.TrueString, result.Value.BoundaryMetadata["LearnedInferenceApplied"]);
        Assert.Equal("1", result.Value.BoundaryMetadata["LocalCheckpointFeatureContributionCount"]);
        Assert.Equal(bool.TrueString, result.Value.BoundaryMetadata["local_checkpoint_weights_configured"]);
        Assert.Equal(bool.TrueString, result.Value.BoundaryMetadata["candidate_html_changed"]);
        Assert.Equal(bool.TrueString, result.Value.BoundaryMetadata["candidate_css_changed"]);
        Assert.Equal("spacing", result.Value.BoundaryMetadata["local_improver_selected_rule_ids"]);
        Assert.Equal(
            "critic_local_checkpoint_feature_adjusted_score",
            result.Value.BoundaryMetadata["logical_flow_primary_metric_name"]);
        Assert.Equal(
            "DESIGN_CONVERSION_INPUT_EVIDENCE_NEEDS_IMPROVEMENT",
            result.Value.BoundaryMetadata["logical_flow_overall_verdict"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["trained_model_authority"]);
        Assert.Equal(bool.TrueString, result.Value.BoundaryMetadata["not_trained_model_authority"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["product_ready"]);
    }

    [Fact]
    public async Task SubmitAndEvaluateAsync_ConfiguredTestProviderVisualJudgeFactoryBlocksBeforeFakeHandler()
    {
        var secret = "pass152-" + Guid.NewGuid().ToString("N");
        const string privateMarker = "PRIVATE_PASS152_VENDOR_VISUAL_JUDGE";
        var handler = new VendorVisualJudgeFakeHttpHandler();
        var artifactRoot = CreateServerOwnedVisualArtifactRoot();
        Directory.CreateDirectory(artifactRoot);
        var visualCostLedger = CreateServerOwnedVisualCostLedger(artifactRoot);
        var utcNow = DateTimeOffset.Parse("2026-07-07T12:00:00Z");
        try
        {
            var input = new DesignConversionExternalVisualProviderConfigInput(
                Endpoint: "https://visual-judge.local.test/v1/judge",
                ProviderName: "vendor-visual-judge-test",
                ModelId: "vendor-vision-local-test",
                ApiKey: secret,
                ApiKeyHeaderName: "X-Test-Key",
                ApiKeyScheme: string.Empty,
                Metadata: new Dictionary<string, string>
                {
                    [DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker] = bool.TrueString,
                    ["vendor_visual_judge_config_execution_slice"] = "local_fake_handler_only"
                },
                ServerOwnedImageArtifactRoot: artifactRoot);
            var comparisonProvider = DesignConversionExternalVisualProviderConfigIntake.TryCreateComparisonProvider(
                input,
                handler);
            var visualJudge = DesignConversionExternalVisualProviderConfigIntake.TryCreateVisualJudge(input, handler);
            var request = CreateRequest("pass152-vendor-visual-judge-config-execution") with
            {
                Html = $"<main data-private=\"{privateMarker}\">Vendor visual judge seam</main>",
                Css = $".screen{{background-image:url('{privateMarker}');display:grid;gap:16px;}}",
                Metadata = new Dictionary<string, string>
                {
                    ["private_note"] = privateMarker,
                    ["slice"] = "VendorVisualJudgeConfigExecution"
                }
            };

            Assert.True(comparisonProvider.IsSuccess, comparisonProvider.ErrorMessage);
            Assert.True(visualJudge.IsSuccess, visualJudge.ErrorMessage);

            var service = new DesignConversionInternalService(
                new RecordingRenderProvider(),
                comparisonProvider.Value!,
                visualJudge.Value);
            var api = new DesignConversionModelServerApi(
                service,
                () => utcNow,
                visualCostLedger: visualCostLedger,
                postEvaluationStateStore: TestPostEvaluationStateStore);

            var result = await api.SubmitAndEvaluateAsync(request);

            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.Equal(0, handler.CallCount);
            Assert.Empty(handler.CapturedApiKeys);
            Assert.Empty(handler.CapturedBodies);

            Assert.Equal(DesignConversionThinClientStatus.NotReady, result.Value!.ContractStatus);
            Assert.Equal(
                DesignConversionVisualQaBoundaryPolicy.DecisionBlocked,
                result.Value.RuntimeStatus.Provenance["final_visual_decision"]);
            Assert.Equal(
                DesignConversionVisualQaBoundaryPolicy.ProviderRequired,
                result.Value.RuntimeStatus.Provenance["tier_policy_stop_reason"]);
            Assert.Equal(
                DesignConversionThinClientStatus.NotReady,
                result.Value.BoundaryMetadata["runtime_contract_status"]);
            Assert.False(result.Value.ProductReady);
            Assert.False(result.Value.PublicHttpEndpointVerified);
            Assert.False(result.Value.ModelUploadReady);
            Assert.False(result.Value.BoundaryMetadata["product_ready"] is "True");
            Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["browser_e2e_verified"]);
            Assert.Equal(
                "runtime_not_completed",
                result.Value.BoundaryMetadata["browser_e2e_failure_reason"]);
            AssertVisualFidelityFalse(result.Value.BoundaryMetadata, "runtime_not_completed");
            AssertContainsFalseBoundaryMarker(result.Value.Warnings, "browser_e2e_verified");
            AssertContainsFalseBoundaryMarker(result.Value.EvidenceRefs, "browser_e2e_verified");
            AssertContainsFalseBoundaryMarker(result.Value.EvidenceRefs, "visual_fidelity_verified");
            AssertContainsFalseBoundaryMarker(result.Value.RuntimeStatus.Warnings, "browser_e2e_verified");
            AssertContainsFalseBoundaryMarker(result.Value.RuntimeStatus.Warnings, "visual_fidelity_verified");
            Assert.Equal(
                ExternalHttpDesignConversionVisualJudge.TestProviderNoNetworkStatus,
                result.Value.BoundaryMetadata["visual_judge_status"]);
            Assert.Equal(
                DesignConversionVisualQaBoundaryPolicy.ProviderRequired,
                result.Value.BoundaryMetadata["visual_judge_provider_boundary"]);
            Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["external_visual_ai_configured"]);
            Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["external_visual_ai_executed"]);
            Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["visual_ai_executed"]);
            Assert.Equal(bool.TrueString, result.Value.BoundaryMetadata["visual_ai_not_executed"]);
            Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["network_call_executed"]);
            Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["external_network_call_executed"]);
            Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["provider_response_identity_verified"]);
            Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["provider_response_schema_verified"]);
            Assert.Equal("0", result.Value.BoundaryMetadata["estimated_visual_judge_cost_usd"]);
            Assert.Contains(DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker, result.Value.EvidenceRefs);
            Assert.Contains(ExternalHttpDesignConversionVisualJudge.TestProviderNoNetworkEvidenceMarker, result.Value.EvidenceRefs);
            Assert.Contains(DesignConversionVisualQaBoundaryPolicy.VisualAiExecutedFalseMarker, result.Value.EvidenceRefs);
            AssertNoTrustedVisualProviderVerdictMetadata(result.Value.BoundaryMetadata);

            var status = api.GetJobResult(result.Value.JobId);
            Assert.True(status.IsSuccess, status.ErrorMessage);
            Assert.Equal(DesignConversionThinClientStatus.NotReady, status.Value!.ContractStatus);
            Assert.Equal(
                DesignConversionThinClientStatus.NotReady,
                status.Value.BoundaryMetadata["runtime_contract_status"]);
            Assert.Equal(
                "runtime_not_completed",
                status.Value.BoundaryMetadata["browser_e2e_failure_reason"]);
            AssertVisualFidelityFalse(status.Value.BoundaryMetadata, "runtime_not_completed");
            AssertContainsFalseBoundaryMarker(status.Value.EvidenceRefs, "browser_e2e_verified");
            AssertContainsFalseBoundaryMarker(status.Value.EvidenceRefs, "visual_fidelity_verified");
            AssertContainsFalseBoundaryMarker(status.Value.RuntimeStatus.Warnings, "browser_e2e_verified");
            AssertContainsFalseBoundaryMarker(status.Value.RuntimeStatus.Warnings, "visual_fidelity_verified");

            var ledgerReadback = await visualCostLedger.ListRecordsAsync();
            Assert.True(ledgerReadback.IsSuccess, ledgerReadback.ErrorMessage);
            var costRecord = Assert.Single(ledgerReadback.Value!);
            Assert.Equal(JsonlDesignConversionVisualCostLedger.LedgerKind, costRecord.LedgerKind);
            Assert.Equal(JsonlDesignConversionVisualCostLedger.SchemaVersion, costRecord.SchemaVersion);
            Assert.Equal(
                JsonlDesignConversionVisualCostLedger.BuildCanonicalEvaluationJobRef(result.Value.JobId),
                costRecord.EvaluationJobRef);
            Assert.Equal(
                JsonlDesignConversionVisualCostLedger.BuildCanonicalSubmittedRequestRef(request.RequestId),
                costRecord.SubmittedRequestRef);
            Assert.Equal(0m, costRecord.TotalCostUsd);
            Assert.True(costRecord.MeasurementComplete);
            Assert.Equal(0, costRecord.MissingMeasurementCount);
            Assert.Equal(1, costRecord.EvaluationAttemptCount);
            Assert.Equal(
                DesignConversionVisualCostAccounting.UnknownCostSource,
                costRecord.CostSource);
            Assert.Equal(
                JsonlDesignConversionVisualCostLedger.NormalizeSubmittedTier(request.Budget!.Tier),
                costRecord.SubmittedTier);
            Assert.Equal(JsonlDesignConversionVisualCostLedger.UtcCalendarMonthPeriodKind, costRecord.PeriodKind);
            var period = JsonlDesignConversionVisualCostLedger.BuildUtcCalendarMonthPeriod(utcNow);
            Assert.Equal(period.StartUtc, costRecord.PeriodStartUtc);
            Assert.Equal(period.EndExclusiveUtc, costRecord.PeriodEndExclusiveUtc);

            var serialized = JsonSerializer.Serialize(result.Value);
            Assert.DoesNotContain(secret, serialized, StringComparison.Ordinal);
            Assert.DoesNotContain(privateMarker, serialized, StringComparison.Ordinal);
            Assert.DoesNotContain(request.Html, serialized, StringComparison.Ordinal);
            Assert.DoesNotContain(request.Css, serialized, StringComparison.Ordinal);
            var serializedStatus = JsonSerializer.Serialize(status.Value);
            Assert.DoesNotContain(secret, serializedStatus, StringComparison.Ordinal);
            Assert.DoesNotContain(privateMarker, serializedStatus, StringComparison.Ordinal);
            Assert.DoesNotContain(request.Html, serializedStatus, StringComparison.Ordinal);
            Assert.DoesNotContain(request.Css, serializedStatus, StringComparison.Ordinal);
            var serializedCostRecord = JsonSerializer.Serialize(costRecord);
            Assert.DoesNotContain(secret, serializedCostRecord, StringComparison.Ordinal);
            Assert.DoesNotContain(privateMarker, serializedCostRecord, StringComparison.Ordinal);
            Assert.DoesNotContain(request.Html, serializedCostRecord, StringComparison.Ordinal);
            Assert.DoesNotContain(request.Css, serializedCostRecord, StringComparison.Ordinal);
        }
        finally
        {
            DeleteDirectoryIfExists(artifactRoot);
        }
    }

    [Fact]
    public async Task ServerOwnedVisualProviderFixture_DisposeDeletesArtifactRoot()
    {
        string artifactRoot;
        string materializedPngPath;

        using (var fixture = await CreateServerOwnedVisualProviderFixtureAsync(
                   CreateRequest("server-owned-visual-provider-fixture-dispose-deletes-root")))
        {
            artifactRoot = fixture.ArtifactRoot;
            Assert.True(Directory.Exists(artifactRoot));

            var pngFiles = Directory.GetFiles(artifactRoot, "*.png", SearchOption.AllDirectories);
            Assert.NotEmpty(pngFiles);
            materializedPngPath = pngFiles[0];
            Assert.True(File.Exists(materializedPngPath));
        }

        Assert.False(Directory.Exists(artifactRoot));
        Assert.False(File.Exists(materializedPngPath));
    }

    [Fact]
    public async Task SubmitAndEvaluateAsync_TrustedRealProviderTelemetrySetsBoundaryNetworkBooleansAndStatusReadback()
    {
        var handler = new EchoingRealVisualJudgeHttpHandler();
        using var fixture = await CreateServerOwnedVisualProviderFixtureAsync(
            CreateRequest("trusted-network-boundary-request"));
        var input = new DesignConversionExternalVisualProviderConfigInput(
            Endpoint: "https://visual-judge.example/v1/judge",
            ProviderName: "openai-loopback-visual-provider",
            ModelId: "gpt-loopback-visual-verifier-v1",
            ApiKey: "trusted-network-boundary-test-key",
            ApiKeyHeaderName: "X-Visual-Judge-Key",
            ApiKeyScheme: string.Empty,
            ServerOwnedImageArtifactRoot: fixture.ArtifactRoot);
        var comparisonProvider = DesignConversionExternalVisualProviderConfigIntake.TryCreateComparisonProvider(
            input,
            handler);
        var visualJudge = DesignConversionExternalVisualProviderConfigIntake.TryCreateVisualJudge(
            input,
            handler);
        Assert.True(comparisonProvider.IsSuccess, comparisonProvider.ErrorMessage);
        Assert.True(visualJudge.IsSuccess, visualJudge.ErrorMessage);

        var service = new DesignConversionInternalService(
            fixture.RenderProvider,
            comparisonProvider.Value!,
            visualJudge.Value);
        var api = new DesignConversionModelServerApi(
            service,
            () => DateTimeOffset.Parse("2026-07-11T00:30:00Z"),
            visualCostLedger: CreateServerOwnedVisualCostLedger(fixture.ArtifactRoot),
            postEvaluationStateStore: TestPostEvaluationStateStore);

        var result = await api.SubmitAndEvaluateAsync(fixture.Request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(2, handler.CallCount);
        Assert.False(result.Value!.ProductReady);
        Assert.False(result.Value.ModelUploadReady);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["product_ready"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["ModelUploadReady"]);
        Assert.Equal(bool.TrueString, result.Value.BoundaryMetadata["external_visual_ai_executed"]);
        AssertTrustedVisualProviderVerdictMetadata(result.Value.BoundaryMetadata);
        Assert.Equal(bool.TrueString, result.Value.BoundaryMetadata["network_call_executed"]);
        Assert.Equal(bool.TrueString, result.Value.BoundaryMetadata["external_network_call_executed"]);
        Assert.Equal(bool.TrueString, result.Value.BoundaryMetadata["external_provider_called"]);
        Assert.Equal(bool.TrueString, result.Value.BoundaryMetadata["real_external_provider_executed"]);
        Assert.Equal(bool.TrueString, result.Value.BoundaryMetadata["provider_response_identity_verified"]);
        Assert.Equal(bool.TrueString, result.Value.BoundaryMetadata["provider_response_schema_verified"]);
        Assert.Equal(
            bool.TrueString,
            result.Value.BoundaryMetadata[DesignConversionModelServerNetworkBoundary.ServerOwnedVisualProviderNetworkCallVerifiedKey]);

        var status = api.GetJobResult(result.Value.JobId);
        Assert.True(status.IsSuccess, status.ErrorMessage);
        AssertTrustedVisualProviderVerdictMetadata(status.Value!.BoundaryMetadata);
        Assert.Equal(bool.TrueString, status.Value!.BoundaryMetadata["network_call_executed"]);
        Assert.Equal(bool.TrueString, status.Value.BoundaryMetadata["external_network_call_executed"]);
        Assert.Equal(bool.TrueString, status.Value.BoundaryMetadata["external_provider_called"]);
        Assert.Equal(bool.FalseString, status.Value.BoundaryMetadata["product_ready"]);

        var statusHandler = new DesignConversionModelServerStatusHandler(api);
        var statusResponse = await statusHandler.HandleAsync(new DesignConversionModelServerHttpRequest(
            Method: "GET",
            Path: $"/api/design-conversion/status/{result.Value.JobId}",
            BodyJson: null));
        using var statusDocument = JsonDocument.Parse(statusResponse.BodyJson);
        var statusMetadata = statusDocument.RootElement.GetProperty("boundaryMetadata");
        AssertTrustedVisualProviderVerdictMetadata(statusMetadata);
        Assert.Equal(bool.TrueString, statusMetadata.GetProperty("network_call_executed").GetString());
        Assert.Equal(bool.TrueString, statusMetadata.GetProperty("external_network_call_executed").GetString());
        Assert.Equal(bool.TrueString, statusMetadata.GetProperty("external_provider_called").GetString());
        Assert.Equal(bool.FalseString, statusMetadata.GetProperty("product_ready").GetString());
    }

    [Fact]
    public async Task SubmitAndEvaluateAsync_RuntimeShapedReceiptTrustsRealProviderAndStatusReadback()
    {
        var handler = new EchoingRealVisualJudgeHttpHandler();
        using var fixture = await CreateServerOwnedVisualProviderFixtureAsync(
            CreateRequest("non-dpo-visual-provider-runtime-verifier-b9c8f055a3bd"));
        var api = CreateRuntimeShapedReceiptApi(
            handler,
            serverOwnedImageArtifactRoot: fixture.ArtifactRoot,
            renderProvider: fixture.RenderProvider);

        var result = await api.SubmitAndEvaluateAsync(fixture.Request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(2, handler.CallCount);
        Assert.Equal(
            "thin-job-non-dpo-visual-provider-runtime-verifier-b9c8f055a3bd",
            result.Value!.JobId);
        Assert.Equal(
            "non-dpo-visual-provider-runtime-verifier-b9c8f055a3bd",
            result.Value.RequestId);
        Assert.False(result.Value!.ProductReady);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["product_ready"]);
        Assert.DoesNotContain("visual-judge-result:-result", result.Value.EvidenceRefs);
        Assert.DoesNotContain("comparison-artifact:-result:comparison-artifact", result.Value.EvidenceRefs);
        Assert.DoesNotContain("qa-comparison-artifact:-result:comparison-artifact", result.Value.EvidenceRefs);
        Assert.All(
            new[]
            {
                "network_call_executed",
                "external_network_call_executed",
                "external_provider_called",
                "real_external_provider_executed",
                DesignConversionModelServerNetworkBoundary.ServerOwnedVisualProviderNetworkCallVerifiedKey
            },
            key => Assert.Equal(bool.TrueString, result.Value.BoundaryMetadata[key]));
        AssertTrustedReceiptNetworkBoundary(
            result.Value.BoundaryMetadata,
            result.Value.JobId,
            result.Value.RequestId,
            "openai-loopback-visual-provider",
            "gpt-loopback-visual-verifier-v1");

        var status = api.GetJobResult(result.Value.JobId);
        Assert.True(status.IsSuccess, status.ErrorMessage);
        AssertTrustedReceiptNetworkBoundary(
            status.Value!.BoundaryMetadata,
            result.Value.JobId,
            result.Value.RequestId,
            "openai-loopback-visual-provider",
            "gpt-loopback-visual-verifier-v1");
        Assert.Equal(bool.FalseString, status.Value.BoundaryMetadata["product_ready"]);

        var statusHandler = new DesignConversionModelServerStatusHandler(api);
        var statusResponse = await statusHandler.HandleAsync(new DesignConversionModelServerHttpRequest(
            Method: "GET",
            Path: $"/api/design-conversion/status/{result.Value.JobId}",
            BodyJson: null));
        using var statusDocument = JsonDocument.Parse(statusResponse.BodyJson);
        var statusMetadata = statusDocument.RootElement.GetProperty("boundaryMetadata");
        AssertTrustedReceiptNetworkBoundary(statusMetadata, result.Value.JobId, result.Value.RequestId);
        Assert.Equal(bool.FalseString, statusMetadata.GetProperty("product_ready").GetString());
    }

    [Fact]
    public async Task SubmitAndEvaluateAsync_TrustedRealProviderMultiAttemptPersistsRootBoundFinalRecord()
    {
        const string requestId = "trusted-provider-multi-attempt-root-boundary";
        const int providerEndpointCount = 2;
        const int expectedAttemptCount = 2;
        using var fixture = await CreateServerOwnedVisualProviderFixtureAsync(CreateTrustedMultiAttemptRequest(requestId));
        var request = fixture.Request;
        var requiredViewportCount = request.Screenshots!
            .Concat(request.DesignRenders!)
            .Select(artifact => (artifact.Width, artifact.Height))
            .Distinct()
            .Count();
        Assert.Equal(2, requiredViewportCount);
        var callsPerAttempt = providerEndpointCount * requiredViewportCount;
        var expectedProviderCallCount = callsPerAttempt * expectedAttemptCount;
        var artifactDb = new TamperingLocalArtifactDb(PersistenceReadbackTamperMode.None);
        var providerCallOrdinal = 0;
        var handler = new EchoingRealVisualJudgeHttpHandler(_ =>
            Interlocked.Increment(ref providerCallOrdinal) <= callsPerAttempt ? 0.72 : 0.973,
            lowerScoreIssueCategory: "spacing",
            lowerScoreIssueSeverity: "minor");
        var api = CreateBrowserReceiptRuntimeShapedReceiptApi(
            handler,
            artifactDb: artifactDb,
            serverOwnedCostPerRequestUsd: "0.0125",
            serverOwnedImageArtifactRoot: fixture.ArtifactRoot);

        var result = await api.SubmitAndEvaluateAsync(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal("pro", request.Budget.Tier);
        Assert.Equal($"thin-job-{requestId}", result.Value!.JobId);
        Assert.Equal(requestId, result.Value.RequestId);
        Assert.True(
            handler.CallCount == expectedProviderCallCount,
            $"Expected {providerEndpointCount} endpoints * {requiredViewportCount} viewports * " +
            $"{expectedAttemptCount} attempts = {expectedProviderCallCount} HTTP calls; actual={handler.CallCount}; " +
            $"judge_request_ids={string.Join("|", handler.CapturedJudgeRequestIds)}; " +
            $"request_ids={string.Join("|", handler.CapturedRequestIds)}; " +
            $"warnings={string.Join("|", result.Value.Warnings)}; " +
            $"visual_score={result.Value.BoundaryMetadata.GetValueOrDefault("visual_provider_visual_judge_score", "missing")}");
        Assert.Equal(expectedProviderCallCount, providerCallOrdinal);
        Assert.Contains("submit_path_improvement_loop_attempt_count=2", result.Value.Warnings);
        Assert.Contains(
            $"submit_path_improvement_loop_max_attempts_source={DesignConversionVisualQaBoundaryPolicy.LegacyMaxImprovementCandidatesIterationSource}",
            result.Value.Warnings);
        Assert.Contains(
            $"submit_path_improvement_loop_stop_reason={DesignConversionVisualQaBoundaryPolicy.QualityReached}",
            result.Value.Warnings);
        Assert.Equal(
            requiredViewportCount.ToString(CultureInfo.InvariantCulture),
            result.Value.BoundaryMetadata["multi_viewport_required_count"]);
        Assert.Equal(
            requiredViewportCount.ToString(CultureInfo.InvariantCulture),
            result.Value.BoundaryMetadata["multi_viewport_observed_count"]);
        Assert.Equal(bool.TrueString, result.Value.BoundaryMetadata["multi_viewport_complete"]);
        Assert.Equal(bool.TrueString, result.Value.BoundaryMetadata["browser_e2e_verified"]);
        var boundaryMetadata = result.Value.BoundaryMetadata;
        Assert.Equal(bool.TrueString, boundaryMetadata["external_visual_ai_executed"]);
        Assert.Equal(bool.TrueString, result.Value.BoundaryMetadata["network_call_executed"]);
        Assert.Equal(bool.TrueString, result.Value.BoundaryMetadata["provider_response_identity_verified"]);
        Assert.Equal(bool.TrueString, result.Value.BoundaryMetadata["provider_response_schema_verified"]);
        Assert.Equal(
            result.Value.JobId,
            result.Value.BoundaryMetadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptJobIdKey]);
        Assert.Equal(
            requestId,
            result.Value.BoundaryMetadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptRequestIdKey]);
        Assert.Equal(
            requestId,
            result.Value.BoundaryMetadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptRootRequestIdKey]);
        Assert.Equal(
            bool.TrueString,
            result.Value.BoundaryMetadata[DesignConversionModelServerNetworkBoundary.ServerOwnedVisualProviderNetworkCallVerifiedKey]);
        Assert.Equal(
            "comparison_artifact_provider_identity_schema_and_network_evidence_verified",
            result.Value.BoundaryMetadata[DesignConversionModelServerNetworkBoundary.ServerOwnedVisualProviderNetworkCallVerificationKey]);
        Assert.Equal(
            "openai-loopback-visual-provider",
            result.Value.BoundaryMetadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptProviderNameKey]);
        Assert.Equal(
            "gpt-loopback-visual-verifier-v1",
            result.Value.BoundaryMetadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptProviderModelKey]);
        Assert.Equal(
            bool.TrueString,
            result.Value.BoundaryMetadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptExternalCallExecutedKey]);
        Assert.Equal(
            bool.TrueString,
            result.Value.BoundaryMetadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseIdentityVerifiedKey]);
        Assert.Equal(
            bool.TrueString,
            result.Value.BoundaryMetadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseSchemaVerifiedKey]);

        var finalLoopJobId = $"{result.Value.JobId}:loop-attempt:2";
        var finalLoopRequestId = $"{requestId}:runtime-improvement:2";
        var rootComparisonRequestIds = handler.CapturedJudgeRequestIds
            .Where(value => value.StartsWith($"{result.Value.JobId}:comparison-request:", StringComparison.Ordinal))
            .ToArray();
        var rootVisualJudgeRequestIds = handler.CapturedJudgeRequestIds
            .Where(value => value.StartsWith($"{result.Value.JobId}:visual-judge-request:", StringComparison.Ordinal))
            .ToArray();
        var finalLoopComparisonRequestIds = handler.CapturedJudgeRequestIds
            .Where(value => value.StartsWith($"{finalLoopJobId}:comparison-request:", StringComparison.Ordinal))
            .ToArray();
        var finalLoopVisualJudgeRequestIds = handler.CapturedJudgeRequestIds
            .Where(value => value.StartsWith($"{finalLoopJobId}:visual-judge-request:", StringComparison.Ordinal))
            .ToArray();
        Assert.Equal(requiredViewportCount, rootComparisonRequestIds.Length);
        Assert.Equal(requiredViewportCount, rootVisualJudgeRequestIds.Length);
        Assert.Equal(requiredViewportCount, finalLoopComparisonRequestIds.Length);
        Assert.Equal(requiredViewportCount, finalLoopVisualJudgeRequestIds.Length);
        for (var viewport = 1; viewport <= requiredViewportCount; viewport++)
        {
            Assert.Contains(
                $"{result.Value.JobId}:comparison-request:{viewport}",
                rootComparisonRequestIds);
            Assert.Contains(
                $"{result.Value.JobId}:visual-judge-request:{viewport}",
                rootVisualJudgeRequestIds);
            Assert.Contains(
                $"{finalLoopJobId}:comparison-request:{viewport}",
                finalLoopComparisonRequestIds);
            Assert.Contains(
                $"{finalLoopJobId}:visual-judge-request:{viewport}",
                finalLoopVisualJudgeRequestIds);
        }

        var expectedSanitizedComparisonRequestIds = Enumerable.Range(1, requiredViewportCount)
            .Select(viewport => $"{result.Value.JobId}:comparison-request:{viewport}")
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToArray();
        var expectedSanitizedVisualJudgeRequestIds = Enumerable.Range(1, requiredViewportCount)
            .Select(viewport => $"{result.Value.JobId}:visual-judge-request:{viewport}")
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToArray();
        var receiptComparisonRequestIds = result.Value.BoundaryMetadata[
                ExternalHttpDesignConversionVisualJudge.ServerCallReceiptComparisonRequestIdKey]
            .Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToArray();
        var receiptVisualJudgeRequestIds = result.Value.BoundaryMetadata[
                ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVisualJudgeRequestIdKey]
            .Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToArray();
        Assert.True(
            expectedSanitizedComparisonRequestIds.SequenceEqual(receiptComparisonRequestIds, StringComparer.Ordinal),
            "Comparison receipt IDs mismatch." + Environment.NewLine +
            $"Expected: {JsonSerializer.Serialize(expectedSanitizedComparisonRequestIds)}" + Environment.NewLine +
            $"Actual: {JsonSerializer.Serialize(receiptComparisonRequestIds)}");
        Assert.True(
            expectedSanitizedVisualJudgeRequestIds.SequenceEqual(receiptVisualJudgeRequestIds, StringComparer.Ordinal),
            "Visual judge receipt IDs mismatch." + Environment.NewLine +
            $"Expected: {JsonSerializer.Serialize(expectedSanitizedVisualJudgeRequestIds)}" + Environment.NewLine +
            $"Actual: {JsonSerializer.Serialize(receiptVisualJudgeRequestIds)}");

        var statusHandler = new DesignConversionModelServerStatusHandler(api);
        var statusResponse = await statusHandler.HandleAsync(new DesignConversionModelServerHttpRequest(
            Method: "GET",
            Path: $"/api/design-conversion/status/{result.Value.JobId}",
            BodyJson: null));
        using var statusDocument = JsonDocument.Parse(statusResponse.BodyJson);
        var statusMetadata = statusDocument.RootElement.GetProperty("boundaryMetadata");
        AssertTrustedVisualProviderVerdictMetadata(statusMetadata);
        Assert.Equal(bool.TrueString, statusMetadata.GetProperty("network_call_executed").GetString());
        Assert.Equal(bool.TrueString, statusMetadata.GetProperty("external_network_call_executed").GetString());
        Assert.Equal(bool.TrueString, statusMetadata.GetProperty("external_provider_called").GetString());
        Assert.Equal(bool.TrueString, statusMetadata.GetProperty("real_external_provider_executed").GetString());
        Assert.Equal(
            bool.TrueString,
            statusMetadata.GetProperty(DesignConversionModelServerNetworkBoundary.ServerOwnedVisualProviderNetworkCallVerifiedKey).GetString());
        var statusReceiptComparisonRequestIds = statusMetadata
            .GetProperty(ExternalHttpDesignConversionVisualJudge.ServerCallReceiptComparisonRequestIdKey)
            .GetString()!
            .Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToArray();
        var statusReceiptVisualJudgeRequestIds = statusMetadata
            .GetProperty(ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVisualJudgeRequestIdKey)
            .GetString()!
            .Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToArray();
        Assert.Equal(expectedSanitizedComparisonRequestIds, statusReceiptComparisonRequestIds);
        Assert.Equal(expectedSanitizedVisualJudgeRequestIds, statusReceiptVisualJudgeRequestIds);

        Assert.Equal(
            callsPerAttempt,
            handler.CapturedRequestIds.Count(value => string.Equals(value, requestId, StringComparison.Ordinal)));
        Assert.Equal(
            callsPerAttempt,
            handler.CapturedRequestIds.Count(value => string.Equals(value, finalLoopRequestId, StringComparison.Ordinal)));

        var records = await artifactDb.ListRecordsAsync(new DesignConversionLocalArtifactDbQuery(
            JobId: result.Value.JobId,
            RequestId: requestId));
        Assert.True(records.IsSuccess, records.ErrorMessage);
        var record = Assert.Single(records.Value!);
        Assert.Equal(1, artifactDb.AppendCalls);
        Assert.Equal(result.Value.JobId, record.JobId);
        Assert.Equal(requestId, record.RequestId);
        Assert.Equal("0.973", record.Metadata["final_visual_score"]);
        Assert.Equal("2", record.Metadata["attempt_count"]);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.QualityReached, record.VisualJudgeStatus.TierDecision);
        Assert.Equal(nameof(DesignConversionThinClientJobRuntime), record.Metadata["artifact_db_single_writer_owner"]);
        Assert.Equal(requiredViewportCount, record.RenderArtifactRefs.Count);
        Assert.All(record.RenderArtifactRefs, renderRef =>
        {
            Assert.StartsWith($"{result.Value.JobId}:render-request:", renderRef.ArtifactId, StringComparison.Ordinal);
            Assert.Contains($"render-result:{renderRef.ArtifactId}", result.Value.EvidenceRefs);
            Assert.Contains($"render-result:{renderRef.ArtifactId}", record.EvidenceRefs);
        });

        var finalLoopJobRef = $"submit_path_improvement_loop_final_evaluation_job_ref={DesignConversionDpoLeakageGuard.HashRef("dcm-submit-path-final-job", finalLoopJobId)}";
        var finalLoopRequestRef = $"submit_path_improvement_loop_final_evaluation_request_ref={DesignConversionDpoLeakageGuard.HashRef("dcm-submit-path-final-request", finalLoopRequestId)}";
        var publicResponseJson = JsonSerializer.Serialize(
            result.Value,
            DesignConversionModelServerHttpHandler.JsonOptions);
        var persistedRecordJson = JsonSerializer.Serialize(
            record,
            DesignConversionModelServerHttpHandler.JsonOptions);
        Assert.DoesNotContain(finalLoopJobId, publicResponseJson, StringComparison.Ordinal);
        Assert.DoesNotContain(finalLoopRequestId, publicResponseJson, StringComparison.Ordinal);
        Assert.DoesNotContain(finalLoopJobId, persistedRecordJson, StringComparison.Ordinal);
        Assert.DoesNotContain(finalLoopRequestId, persistedRecordJson, StringComparison.Ordinal);
        Assert.DoesNotContain(":loop-ref:", publicResponseJson, StringComparison.Ordinal);
        Assert.DoesNotContain(":loop-ref:", persistedRecordJson, StringComparison.Ordinal);
        Assert.Contains(finalLoopJobRef, publicResponseJson, StringComparison.Ordinal);
        Assert.Contains(finalLoopRequestRef, publicResponseJson, StringComparison.Ordinal);
        Assert.Contains(finalLoopJobRef, persistedRecordJson, StringComparison.Ordinal);
        Assert.Contains(finalLoopRequestRef, persistedRecordJson, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(FinalLoopReceiptRootTamperMode.JobId)]
    [InlineData(FinalLoopReceiptRootTamperMode.ProviderRequestId)]
    [InlineData(FinalLoopReceiptRootTamperMode.ComparisonVisualJudgeRequestId)]
    [InlineData(FinalLoopReceiptRootTamperMode.JudgeComparisonArtifactId)]
    [InlineData(FinalLoopReceiptRootTamperMode.EndpointHost)]
    [InlineData(FinalLoopReceiptRootTamperMode.ProviderAndReceiptEndpointHost)]
    public async Task SubmitAndEvaluateAsync_PartialFinalReceiptRootTamperStaysUntrustedAndRedacted(
        FinalLoopReceiptRootTamperMode tamperMode)
    {
        const int providerEndpointCount = 2;
        const int expectedViewportCount = 2;
        const int expectedAttemptCount = 2;
        var requestId = $"trusted-provider-partial-root-tamper-{tamperMode}";
        var rootJobId = $"thin-job-{requestId}";
        var finalLoopJobId = $"{rootJobId}:loop-attempt:2";
        var finalLoopRequestId = $"{requestId}:runtime-improvement:2";
        using var fixture = await CreateServerOwnedVisualProviderFixtureAsync(CreateTrustedMultiAttemptRequest(requestId));
        var request = fixture.Request;
        var callsPerAttempt = providerEndpointCount * expectedViewportCount;
        var expectedProviderCallCount = callsPerAttempt * expectedAttemptCount;
        var artifactDb = new TamperingLocalArtifactDb(PersistenceReadbackTamperMode.None);
        var providerCallOrdinal = 0;
        var handler = new EchoingRealVisualJudgeHttpHandler(_ =>
            Interlocked.Increment(ref providerCallOrdinal) <= callsPerAttempt ? 0.72 : 0.973,
            lowerScoreIssueCategory: "spacing",
            lowerScoreIssueSeverity: "minor");
        FinalLoopReceiptRootTamperingComparisonProvider? tamperingProvider = null;
        FinalLoopReceiptRootTamperingVisualJudge? tamperingVisualJudge = null;
        var api = CreateBrowserReceiptRuntimeShapedReceiptApi(
            handler,
            configureComparisonProvider: inner =>
            {
                if (tamperMode == FinalLoopReceiptRootTamperMode.JudgeComparisonArtifactId)
                    return inner;

                tamperingProvider = new FinalLoopReceiptRootTamperingComparisonProvider(
                    inner,
                    rootJobId,
                    finalLoopJobId,
                    tamperMode);
                return tamperingProvider;
            },
            configureVisualJudge: inner =>
            {
                if (tamperMode != FinalLoopReceiptRootTamperMode.JudgeComparisonArtifactId)
                    return inner;

                tamperingVisualJudge = new FinalLoopReceiptRootTamperingVisualJudge(
                    inner,
                    rootJobId,
                    finalLoopJobId);
                return tamperingVisualJudge;
            },
            artifactDb: artifactDb,
            serverOwnedCostPerRequestUsd: "0.0125",
            serverOwnedImageArtifactRoot: fixture.ArtifactRoot);

        var result = await api.SubmitAndEvaluateAsync(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(rootJobId, result.Value!.JobId);
        Assert.Equal(requestId, result.Value.RequestId);
        Assert.Equal(expectedProviderCallCount, handler.CallCount);
        Assert.Equal(expectedProviderCallCount, providerCallOrdinal);
        Assert.Equal(
            1,
            (tamperingProvider?.TamperedReceiptCount ?? 0) +
            (tamperingVisualJudge?.TamperedReceiptCount ?? 0));
        Assert.Contains("submit_path_improvement_loop_attempt_count=2", result.Value.Warnings);
        Assert.Contains(
            $"submit_path_improvement_loop_stop_reason={DesignConversionVisualQaBoundaryPolicy.QualityReached}",
            result.Value.Warnings);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["external_visual_ai_executed"]);
        AssertUntrustedReceiptNetworkBoundary(result.Value.BoundaryMetadata);

        var records = await artifactDb.ListRecordsAsync(new DesignConversionLocalArtifactDbQuery(
            JobId: rootJobId,
            RequestId: requestId));
        Assert.True(records.IsSuccess, records.ErrorMessage);
        var record = Assert.Single(records.Value!);
        Assert.Equal(1, artifactDb.AppendCalls);
        Assert.Equal(rootJobId, record.JobId);
        Assert.Equal(requestId, record.RequestId);
        Assert.Equal("0.973", record.Metadata["final_visual_score"]);
        Assert.Equal("2", record.Metadata["attempt_count"]);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.QualityReached, record.VisualJudgeStatus.TierDecision);

        var publicResponseJson = JsonSerializer.Serialize(
            result.Value,
            DesignConversionModelServerHttpHandler.JsonOptions);
        var persistedRecordJson = JsonSerializer.Serialize(
            record,
            DesignConversionModelServerHttpHandler.JsonOptions);
        Assert.DoesNotContain(finalLoopJobId, publicResponseJson, StringComparison.Ordinal);
        Assert.DoesNotContain(finalLoopRequestId, publicResponseJson, StringComparison.Ordinal);
        Assert.DoesNotContain(finalLoopJobId, persistedRecordJson, StringComparison.Ordinal);
        Assert.DoesNotContain(finalLoopRequestId, persistedRecordJson, StringComparison.Ordinal);
        Assert.Contains("dcm-untrusted-final-loop-job-", publicResponseJson, StringComparison.Ordinal);
        Assert.Contains("dcm-untrusted-final-loop-job-", persistedRecordJson, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SubmitAndEvaluateAsync_BrowserBackedTrustedReceiptPreservesDiagnosticsButDefersReadinessWhenRuntimeNotCompleted()
    {
        var handler = new EchoingRealVisualJudgeHttpHandler();
        using var fixture = await CreateServerOwnedVisualProviderFixtureAsync(
            CreateSingleAttemptRequest("browser-backed-visual-fidelity-trusted-receipt"));
        var api = CreateBrowserReceiptRuntimeShapedReceiptApi(
            handler,
            serverOwnedImageArtifactRoot: fixture.ArtifactRoot);

        var result = await api.SubmitAndEvaluateAsync(fixture.Request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionThinClientStatus.NotReady, result.Value!.ContractStatus);
        Assert.Equal(
            DesignConversionThinClientStatus.NotReady,
            result.Value.BoundaryMetadata["runtime_contract_status"]);
        Assert.Equal(2, handler.CallCount);
        Assert.Equal(DesignConversionThinClientStatus.NotReady, result.Value!.ContractStatus);
        Assert.False(result.Value.ProductReady);
        Assert.Equal(
            DesignConversionVisualQaBoundaryPolicy.DecisionBlocked,
            result.Value.RuntimeStatus.Provenance["final_visual_decision"]);
        Assert.Equal(
            DesignConversionVisualQaBoundaryPolicy.ViewportOperationFailed,
            result.Value.RuntimeStatus.Provenance["tier_policy_stop_reason"]);
        AssertTrustedReceiptNetworkBoundary(
            result.Value.BoundaryMetadata,
            result.Value.JobId,
            result.Value.RequestId,
            "openai-loopback-visual-provider",
            "gpt-loopback-visual-verifier-v1");
        AssertRuntimeDeferredTrustedVisualFidelityMetadata(result.Value.BoundaryMetadata);
        AssertContainsFalseBoundaryMarker(result.Value.Warnings, "browser_e2e_verified");
        AssertContainsFalseBoundaryMarker(result.Value.Warnings, "visual_fidelity_verified");
        AssertContainsFalseBoundaryMarker(result.Value.EvidenceRefs, "browser_e2e_verified");
        AssertContainsFalseBoundaryMarker(result.Value.EvidenceRefs, "visual_fidelity_verified");
        AssertContainsFalseBoundaryMarker(result.Value.RuntimeStatus.Warnings, "browser_e2e_verified");
        AssertContainsFalseBoundaryMarker(result.Value.RuntimeStatus.Warnings, "visual_fidelity_verified");

        var status = api.GetJobResult(result.Value.JobId);
        Assert.True(status.IsSuccess, status.ErrorMessage);
        Assert.Equal(DesignConversionThinClientStatus.NotReady, status.Value!.ContractStatus);
        Assert.Equal(
            DesignConversionThinClientStatus.NotReady,
            status.Value.BoundaryMetadata["runtime_contract_status"]);
        AssertRuntimeDeferredTrustedVisualFidelityMetadata(status.Value.BoundaryMetadata);
        Assert.Equal(bool.FalseString, status.Value.BoundaryMetadata["product_ready"]);
        AssertContainsFalseBoundaryMarker(status.Value.EvidenceRefs, "browser_e2e_verified");
        AssertContainsFalseBoundaryMarker(status.Value.EvidenceRefs, "visual_fidelity_verified");
        AssertContainsFalseBoundaryMarker(status.Value.RuntimeStatus.Warnings, "browser_e2e_verified");
        AssertContainsFalseBoundaryMarker(status.Value.RuntimeStatus.Warnings, "visual_fidelity_verified");

        var resultJson = JsonSerializer.Serialize(result.Value, DesignConversionModelServerHttpHandler.JsonOptions);
        var statusJson = JsonSerializer.Serialize(status.Value, DesignConversionModelServerHttpHandler.JsonOptions);
        Assert.DoesNotContain(fixture.Request.Html, resultJson, StringComparison.Ordinal);
        Assert.DoesNotContain(fixture.Request.Css, resultJson, StringComparison.Ordinal);
        Assert.DoesNotContain(fixture.Request.Html, statusJson, StringComparison.Ordinal);
        Assert.DoesNotContain(fixture.Request.Css, statusJson, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SubmitAndEvaluateAsync_BrowserBackedLocalProviderKeepsVisualFidelityFalse()
    {
        var api = new DesignConversionModelServerApi(
            new DesignConversionInternalService(
                new BrowserBackedDesignConversionRenderProvider(new TrustedBrowserReceiptBrowserAdapter()),
                new RecordingComparisonProvider(CreateVerdict("browser-backed-local-provider-verdict", 0.973, passed: true)),
                routingFlow: null),
            () => DateTimeOffset.Parse("2026-07-11T00:52:00Z"),
            postEvaluationStateStore: TestPostEvaluationStateStore);

        var request = CreateSingleAttemptRequest("browser-backed-local-provider-visual-fidelity");
        var result = await api.SubmitAndEvaluateAsync(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionThinClientStatus.NotReady, result.Value!.ContractStatus);
        Assert.Equal(
            DesignConversionThinClientStatus.NotReady,
            result.Value.BoundaryMetadata["runtime_contract_status"]);
        Assert.Equal(bool.FalseString, result.Value!.BoundaryMetadata["browser_e2e_verified"]);
        Assert.Equal(
            "runtime_not_completed",
            result.Value.BoundaryMetadata["browser_e2e_failure_reason"]);
        AssertVisualFidelityFalse(result.Value.BoundaryMetadata, "runtime_not_completed");
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["product_ready"]);
        AssertContainsFalseBoundaryMarker(result.Value.Warnings, "browser_e2e_verified");
        AssertContainsFalseBoundaryMarker(result.Value.EvidenceRefs, "browser_e2e_verified");
        AssertContainsFalseBoundaryMarker(result.Value.RuntimeStatus.Warnings, "browser_e2e_verified");
        AssertContainsFalseBoundaryMarker(result.Value.Warnings, "visual_fidelity_verified");
        AssertContainsFalseBoundaryMarker(result.Value.EvidenceRefs, "visual_fidelity_verified");
        AssertContainsFalseBoundaryMarker(result.Value.RuntimeStatus.Warnings, "visual_fidelity_verified");

        var status = api.GetJobResult(result.Value.JobId);
        Assert.True(status.IsSuccess, status.ErrorMessage);
        Assert.Equal(DesignConversionThinClientStatus.NotReady, status.Value!.ContractStatus);
        Assert.Equal(
            DesignConversionThinClientStatus.NotReady,
            status.Value.BoundaryMetadata["runtime_contract_status"]);
        Assert.Equal(bool.FalseString, status.Value!.BoundaryMetadata["browser_e2e_verified"]);
        Assert.Equal(
            "runtime_not_completed",
            status.Value.BoundaryMetadata["browser_e2e_failure_reason"]);
        AssertVisualFidelityFalse(status.Value.BoundaryMetadata, "runtime_not_completed");
        Assert.Equal(bool.FalseString, status.Value.BoundaryMetadata["product_ready"]);
        AssertContainsFalseBoundaryMarker(status.Value.EvidenceRefs, "browser_e2e_verified");
        AssertContainsFalseBoundaryMarker(status.Value.RuntimeStatus.Warnings, "browser_e2e_verified");
        AssertContainsFalseBoundaryMarker(status.Value.EvidenceRefs, "visual_fidelity_verified");
        AssertContainsFalseBoundaryMarker(status.Value.RuntimeStatus.Warnings, "visual_fidelity_verified");

        var resultJson = JsonSerializer.Serialize(result.Value, DesignConversionModelServerHttpHandler.JsonOptions);
        var statusJson = JsonSerializer.Serialize(status.Value, DesignConversionModelServerHttpHandler.JsonOptions);
        Assert.DoesNotContain(request.Html, resultJson, StringComparison.Ordinal);
        Assert.DoesNotContain(request.Css, resultJson, StringComparison.Ordinal);
        Assert.DoesNotContain(request.Html, statusJson, StringComparison.Ordinal);
        Assert.DoesNotContain(request.Css, statusJson, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SubmitAndEvaluateAsync_BrowserBackedSingleViewportCapOneRequiresResponsiveComplementAndStaysNonReady()
    {
        var handler = new ScriptedRealVisualJudgeHttpHandler(
            score: 0.973,
            passed: true,
            confidence: 0.917,
            issueSeverity: "info");
        var baseRequest = CreateSingleAttemptRequest("browser-backed-single-viewport-cap-one");
        var request = baseRequest with
        {
            Budget = baseRequest.Budget! with { MaxRenderQaRuns = 1 }
        };
        using var fixture = await CreateServerOwnedVisualProviderFixtureAsync(request);
        var api = CreateBrowserReceiptRuntimeShapedReceiptApi(
            handler,
            serverOwnedImageArtifactRoot: fixture.ArtifactRoot);

        var result = await api.SubmitAndEvaluateAsync(fixture.Request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionThinClientStatus.Quarantined, result.Value!.ContractStatus);
        Assert.Equal(
            DesignConversionThinClientStatus.Quarantined,
            result.Value.BoundaryMetadata["runtime_contract_status"]);
        Assert.False(result.Value!.ProductReady);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["product_ready"]);
        Assert.Equal("2", result.Value.BoundaryMetadata["multi_viewport_required_count"]);
        Assert.Equal("1", result.Value.BoundaryMetadata["multi_viewport_observed_count"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["multi_viewport_complete"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["browser_e2e_verified"]);
        Assert.Equal(
            "runtime_not_completed",
            result.Value.BoundaryMetadata["browser_e2e_failure_reason"]);
        AssertVisualFidelityFalse(result.Value.BoundaryMetadata, "runtime_not_completed");
        Assert.Contains(
            $"tier_policy:stop_reason={DesignConversionVisualQaBoundaryPolicy.RenderManifestIncomplete}",
            result.Value.EvidenceRefs);

        var status = api.GetJobResult(result.Value.JobId);
        Assert.True(status.IsSuccess, status.ErrorMessage);
        Assert.Equal(DesignConversionThinClientStatus.Quarantined, status.Value!.ContractStatus);
        Assert.Equal(
            DesignConversionThinClientStatus.Quarantined,
            status.Value.BoundaryMetadata["runtime_contract_status"]);
        Assert.False(status.Value!.ProductReady);
        Assert.Equal(bool.FalseString, status.Value.BoundaryMetadata["product_ready"]);
        Assert.Equal("2", status.Value.BoundaryMetadata["multi_viewport_required_count"]);
        Assert.Equal("1", status.Value.BoundaryMetadata["multi_viewport_observed_count"]);
        Assert.Equal(bool.FalseString, status.Value.BoundaryMetadata["multi_viewport_complete"]);
        Assert.Equal(bool.FalseString, status.Value.BoundaryMetadata["browser_e2e_verified"]);
        Assert.Equal(
            "runtime_not_completed",
            status.Value.BoundaryMetadata["browser_e2e_failure_reason"]);
        AssertVisualFidelityFalse(status.Value.BoundaryMetadata, "runtime_not_completed");
    }

    [Theory]
    [InlineData(VisualFidelityNegativeMode.ReceiptMismatch, "runtime_not_completed")]
    [InlineData(VisualFidelityNegativeMode.ResponseIdentityMismatch, "runtime_not_completed")]
    [InlineData(VisualFidelityNegativeMode.ResponseSchemaMismatch, "runtime_not_completed")]
    [InlineData(VisualFidelityNegativeMode.NaNScore, "runtime_not_completed")]
    [InlineData(VisualFidelityNegativeMode.BelowThreshold, "runtime_not_completed")]
    [InlineData(VisualFidelityNegativeMode.MissingConfidence, "runtime_not_completed")]
    [InlineData(VisualFidelityNegativeMode.BlockingIssue, "runtime_not_completed")]
    [InlineData(VisualFidelityNegativeMode.IncompleteViewport, "runtime_not_completed")]
    public async Task SubmitAndEvaluateAsync_BrowserBackedVisualFidelityNegativeMatrixStaysFalse(
        VisualFidelityNegativeMode mode,
        string expectedReason)
    {
        var handler = mode switch
        {
            VisualFidelityNegativeMode.BelowThreshold => new ScriptedRealVisualJudgeHttpHandler(score: 0.82, passed: true, confidence: 0.917, issueSeverity: "info"),
            VisualFidelityNegativeMode.MissingConfidence => new ScriptedRealVisualJudgeHttpHandler(score: 0.973, passed: true, confidence: null, issueSeverity: "info"),
            VisualFidelityNegativeMode.BlockingIssue => new ScriptedRealVisualJudgeHttpHandler(score: 0.973, passed: true, confidence: 0.917, issueSeverity: "major"),
            _ => new ScriptedRealVisualJudgeHttpHandler(score: 0.973, passed: true, confidence: 0.917, issueSeverity: "info")
        };
        var request = CreateSingleAttemptRequest($"browser-backed-visual-fidelity-negative-{mode}");
        if (mode == VisualFidelityNegativeMode.IncompleteViewport)
            request = AddUnrenderedMobileViewport(request);
        using var fixture = await CreateServerOwnedVisualProviderFixtureAsync(request);
        var api = CreateBrowserReceiptRuntimeShapedReceiptApi(
            handler,
            mode == VisualFidelityNegativeMode.ReceiptMismatch
                ? comparison => new TamperedReceiptComparisonProvider(comparison, RuntimeReceiptTamperMode.ComparisonArtifact)
                : null,
            mode switch
            {
                VisualFidelityNegativeMode.ResponseIdentityMismatch => judge => new TamperedVisualJudge(
                    judge,
                    VisualJudgeTamperMode.ResponseIdentityMismatch),
                VisualFidelityNegativeMode.ResponseSchemaMismatch => judge => new TamperedVisualJudge(
                    judge,
                    VisualJudgeTamperMode.ResponseSchemaMismatch),
                VisualFidelityNegativeMode.NaNScore => judge => new TamperedVisualJudge(
                    judge,
                    VisualJudgeTamperMode.NaNScore),
                _ => null
            },
            serverOwnedImageArtifactRoot: fixture.ArtifactRoot);

        var result = await api.SubmitAndEvaluateAsync(fixture.Request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(ExpectedVisualFidelityNegativeRuntimeStatus(mode), result.Value!.ContractStatus);
        AssertVisualFidelityFalse(result.Value!.BoundaryMetadata, expectedReason);
        AssertVisualFidelityNegativeMatrixDiagnostics(result.Value.BoundaryMetadata, mode);

        var status = api.GetJobResult(result.Value.JobId);
        Assert.True(status.IsSuccess, status.ErrorMessage);
        Assert.Equal(ExpectedVisualFidelityNegativeRuntimeStatus(mode), status.Value!.ContractStatus);
        AssertVisualFidelityFalse(status.Value!.BoundaryMetadata, expectedReason);
        AssertVisualFidelityNegativeMatrixDiagnostics(status.Value.BoundaryMetadata, mode);

        var resultJson = JsonSerializer.Serialize(result.Value, DesignConversionModelServerHttpHandler.JsonOptions);
        var statusJson = JsonSerializer.Serialize(status.Value, DesignConversionModelServerHttpHandler.JsonOptions);
        Assert.DoesNotContain(request.Html, resultJson, StringComparison.Ordinal);
        Assert.DoesNotContain(request.Css, resultJson, StringComparison.Ordinal);
        Assert.DoesNotContain(request.Html, statusJson, StringComparison.Ordinal);
        Assert.DoesNotContain(request.Css, statusJson, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SubmitAndEvaluateAsync_BrowserBackedProviderPassedFalseAboveThresholdRemainsDiagnostic()
    {
        var handler = new ScriptedRealVisualJudgeHttpHandler(
            score: 0.973,
            passed: false,
            confidence: 0.917,
            issueSeverity: "info");
        using var fixture = await CreateServerOwnedVisualProviderFixtureAsync(
            CreateSingleAttemptRequest("browser-backed-provider-passed-false-diagnostic"));
        var api = CreateBrowserReceiptRuntimeShapedReceiptApi(
            handler,
            serverOwnedImageArtifactRoot: fixture.ArtifactRoot);

        var result = await api.SubmitAndEvaluateAsync(fixture.Request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionThinClientStatus.NotReady, result.Value!.ContractStatus);
        Assert.False(result.Value.ProductReady);
        Assert.Equal(
            DesignConversionVisualQaBoundaryPolicy.DecisionBlocked,
            result.Value.RuntimeStatus.Provenance["final_visual_decision"]);
        Assert.Equal(
            DesignConversionVisualQaBoundaryPolicy.ViewportOperationFailed,
            result.Value.RuntimeStatus.Provenance["tier_policy_stop_reason"]);
        Assert.Equal(
            DesignConversionThinClientStatus.NotReady,
            result.Value.BoundaryMetadata["runtime_contract_status"]);
        AssertTrustedReceiptNetworkBoundary(
            result.Value.BoundaryMetadata,
            result.Value.JobId,
            result.Value.RequestId,
            "openai-loopback-visual-provider",
            "gpt-loopback-visual-verifier-v1");
        AssertRuntimeDeferredTrustedVisualFidelityMetadata(result.Value.BoundaryMetadata);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["visual_provider_reported_passed"]);
        Assert.Equal(bool.TrueString, result.Value.BoundaryMetadata["visual_provider_server_passed_mismatch"]);
        AssertContainsFalseBoundaryMarker(result.Value.EvidenceRefs, "browser_e2e_verified");
        AssertContainsFalseBoundaryMarker(result.Value.EvidenceRefs, "visual_fidelity_verified");
        AssertContainsFalseBoundaryMarker(result.Value.RuntimeStatus.Warnings, "browser_e2e_verified");
        AssertContainsFalseBoundaryMarker(result.Value.RuntimeStatus.Warnings, "visual_fidelity_verified");

        var status = api.GetJobResult(result.Value.JobId);
        Assert.True(status.IsSuccess, status.ErrorMessage);
        Assert.Equal(DesignConversionThinClientStatus.NotReady, status.Value!.ContractStatus);
        Assert.Equal(
            DesignConversionThinClientStatus.NotReady,
            status.Value.BoundaryMetadata["runtime_contract_status"]);
        AssertRuntimeDeferredTrustedVisualFidelityMetadata(status.Value.BoundaryMetadata);
        Assert.Equal(bool.FalseString, status.Value.BoundaryMetadata["visual_provider_reported_passed"]);
        Assert.Equal(bool.TrueString, status.Value.BoundaryMetadata["visual_provider_server_passed_mismatch"]);
        AssertContainsFalseBoundaryMarker(status.Value.EvidenceRefs, "browser_e2e_verified");
        AssertContainsFalseBoundaryMarker(status.Value.EvidenceRefs, "visual_fidelity_verified");
        AssertContainsFalseBoundaryMarker(status.Value.RuntimeStatus.Warnings, "browser_e2e_verified");
        AssertContainsFalseBoundaryMarker(status.Value.RuntimeStatus.Warnings, "visual_fidelity_verified");

        var resultJson = JsonSerializer.Serialize(result.Value, DesignConversionModelServerHttpHandler.JsonOptions);
        var statusJson = JsonSerializer.Serialize(status.Value, DesignConversionModelServerHttpHandler.JsonOptions);
        Assert.DoesNotContain(fixture.Request.Html, resultJson, StringComparison.Ordinal);
        Assert.DoesNotContain(fixture.Request.Css, resultJson, StringComparison.Ordinal);
        Assert.DoesNotContain(fixture.Request.Html, statusJson, StringComparison.Ordinal);
        Assert.DoesNotContain(fixture.Request.Css, statusJson, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(RuntimeReceiptTamperMode.RootRequest)]
    [InlineData(RuntimeReceiptTamperMode.JobPrefix)]
    [InlineData(RuntimeReceiptTamperMode.ProviderCallReceiptId)]
    [InlineData(RuntimeReceiptTamperMode.ProviderRequest)]
    [InlineData(RuntimeReceiptTamperMode.VisualResult)]
    [InlineData(RuntimeReceiptTamperMode.ComparisonArtifact)]
    [InlineData(RuntimeReceiptTamperMode.Provider)]
    [InlineData(RuntimeReceiptTamperMode.Model)]
    public async Task SubmitAndEvaluateAsync_RuntimeShapedReceiptMismatchKeepsBoundaryNetworkFalse(
        RuntimeReceiptTamperMode mode)
    {
        var handler = new EchoingRealVisualJudgeHttpHandler();
        using var fixture = await CreateServerOwnedVisualProviderFixtureAsync(
            CreateRequest($"runtime-shaped-receipt-mismatch-{mode}"));
        var api = CreateRuntimeShapedReceiptApi(
            handler,
            comparison => new TamperedReceiptComparisonProvider(comparison, mode),
            serverOwnedImageArtifactRoot: fixture.ArtifactRoot,
            renderProvider: fixture.RenderProvider);

        var result = await api.SubmitAndEvaluateAsync(fixture.Request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(2, handler.CallCount);
        Assert.False(result.Value!.ProductReady);
        AssertUntrustedReceiptNetworkBoundary(result.Value.BoundaryMetadata);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["product_ready"]);

        var status = api.GetJobResult(result.Value.JobId);
        Assert.True(status.IsSuccess, status.ErrorMessage);
        AssertUntrustedReceiptNetworkBoundary(status.Value!.BoundaryMetadata);
        Assert.Equal(bool.FalseString, status.Value.BoundaryMetadata["product_ready"]);
    }

    [Fact]
    public async Task SubmitAndEvaluateAsync_IndividuallyTrustedCrossedReceiptPairFailsClosedNetworkAuthority()
    {
        var handler = new EchoingRealVisualJudgeHttpHandler();
        using var fixture = await CreateServerOwnedVisualProviderFixtureAsync(
            CreateRequest("runtime-shaped-crossed-trusted-receipt-pair"));
        CapturingTrustedReceiptComparisonProvider? comparisonCapture = null;
        CrossedTrustedReceiptVisualJudge? visualJudgeCapture = null;
        var api = CreateBrowserReceiptRuntimeShapedReceiptApi(
            handler,
            configureComparisonProvider: inner =>
                comparisonCapture = new CapturingTrustedReceiptComparisonProvider(inner),
            configureVisualJudge: inner =>
                visualJudgeCapture = new CrossedTrustedReceiptVisualJudge(inner),
            serverOwnedImageArtifactRoot: fixture.ArtifactRoot);

        var result = await api.SubmitAndEvaluateAsync(fixture.Request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(2, handler.CallCount);
        Assert.NotNull(comparisonCapture?.Artifact);
        Assert.NotNull(visualJudgeCapture?.Result);
        var artifact = comparisonCapture!.Artifact!;
        var visualJudgeResult = visualJudgeCapture!.Result!;
        Assert.True(DesignConversionModelServerNetworkBoundary.IsTrustedVisualProviderNetworkCall(
            artifact,
            result.Value!.JobId,
            result.Value.RequestId,
            providerResponseIdentityVerified: true,
            providerResponseSchemaVerified: true));
        Assert.True(DesignConversionModelServerNetworkBoundary.IsTrustedVisualJudgeNetworkCall(
            visualJudgeResult,
            result.Value.JobId,
            result.Value.RequestId));
        Assert.True(DesignConversionModelServerNetworkBoundary.MatchesTrustedVisualJudge(
            artifact,
            visualJudgeResult));
        Assert.NotEqual(
            artifact.ComparisonRequestId,
            visualJudgeResult.Metadata[
                ExternalHttpDesignConversionVisualJudge.ServerCallReceiptComparisonRequestIdKey]);
        var crossedPairMetadata = InvokeBuildVisualProviderMetadata(
            [artifact],
            [visualJudgeResult],
            result.Value.JobId,
            result.Value.RequestId);
        AssertUntrustedReceiptNetworkAuthority(crossedPairMetadata);
        Assert.Equal(bool.TrueString, crossedPairMetadata["visual_provider_output_observed"]);
        Assert.Equal("openai-loopback-visual-provider", crossedPairMetadata["provider_name"]);
        Assert.Equal("gpt-loopback-visual-verifier-v1", crossedPairMetadata["provider_model"]);
        AssertNoAuthorityReceiptTuple(crossedPairMetadata);
        AssertUntrustedReceiptNetworkBoundary(result.Value.BoundaryMetadata);
        Assert.Equal(bool.TrueString, result.Value.BoundaryMetadata["visual_provider_output_observed"]);
        Assert.Equal("openai-loopback-visual-provider", result.Value.BoundaryMetadata["provider_name"]);
        Assert.Equal("gpt-loopback-visual-verifier-v1", result.Value.BoundaryMetadata["provider_model"]);
        AssertNoAuthorityReceiptTuple(result.Value.BoundaryMetadata);

        var status = api.GetJobResult(result.Value.JobId);
        Assert.True(status.IsSuccess, status.ErrorMessage);
        AssertUntrustedReceiptNetworkBoundary(status.Value!.BoundaryMetadata);
        AssertNoAuthorityReceiptTuple(status.Value.BoundaryMetadata);
    }

    [Fact]
    public async Task StatusReadback_ForgedNetworkStatusProvenanceDoesNotClaimBoundaryNetworkCall()
    {
        var runtime = new DesignConversionThinClientJobRuntime(
            new DesignConversionInternalService(
                new RecordingRenderProvider(),
                new RecordingComparisonProvider(CreateVerdict("forged-network-status-verdict", 0.91, passed: true)),
                routingFlow: null),
            () => DateTimeOffset.Parse("2026-07-11T00:35:00Z"));
        var submitted = CreateSubmittedRuntimeRequest(
            "forged-network-status-request",
            new Dictionary<string, string>
            {
                ["network_call_executed"] = bool.TrueString,
                ["external_network_call_executed"] = bool.TrueString,
                ["external_provider_called"] = bool.TrueString,
                ["real_external_provider_executed"] = bool.TrueString,
                [DesignConversionModelServerNetworkBoundary.ServerOwnedVisualProviderNetworkCallVerifiedKey] = bool.TrueString,
                ["provider_response_identity_verified"] = bool.TrueString,
                ["provider_response_schema_verified"] = bool.TrueString
            });
        var submit = runtime.Submit(submitted);
        Assert.True(submit.IsSuccess, submit.ErrorMessage);
        var run = await runtime.RunAsync(submit.Value!.JobId);
        Assert.True(run.IsSuccess, run.ErrorMessage);

        var api = new DesignConversionModelServerApi(runtime);
        var status = api.GetJobResult(submit.Value.JobId);

        Assert.True(status.IsSuccess, status.ErrorMessage);
        AssertNoTrustedVisualProviderVerdictMetadata(status.Value!.BoundaryMetadata);
        Assert.Equal(bool.FalseString, status.Value!.BoundaryMetadata["network_call_executed"]);
        Assert.Equal(bool.FalseString, status.Value.BoundaryMetadata["external_network_call_executed"]);
        Assert.Equal(bool.FalseString, status.Value.BoundaryMetadata["external_provider_called"]);
        Assert.Equal(
            bool.FalseString,
            status.Value.BoundaryMetadata[DesignConversionModelServerNetworkBoundary.ServerOwnedVisualProviderNetworkCallVerifiedKey]);
        Assert.Equal(bool.FalseString, status.Value.BoundaryMetadata["product_ready"]);
    }

    [Theory]
    [InlineData(ForgedNetworkArtifactMode.MalformedMissingSchema)]
    [InlineData(ForgedNetworkArtifactMode.TimeoutNoProviderEvidence)]
    [InlineData(ForgedNetworkArtifactMode.EvidenceRequestMismatch)]
    public async Task SubmitAndEvaluateAsync_UntrustedProviderNetworkEvidenceKeepsBoundaryNetworkFalse(
        ForgedNetworkArtifactMode mode)
    {
        var artifactRoot = CreateServerOwnedVisualArtifactRoot();
        var service = new DesignConversionInternalService(
            new RecordingRenderProvider(),
            new ForgedNetworkComparisonProvider(mode),
            visualJudge: null,
            routingFlow: null);
        var api = new DesignConversionModelServerApi(
            service,
            () => DateTimeOffset.Parse("2026-07-11T00:40:00Z"),
            visualCostLedger: CreateServerOwnedVisualCostLedger(artifactRoot),
            postEvaluationStateStore: TestPostEvaluationStateStore);

        var result = await api.SubmitAndEvaluateAsync(CreateRequest($"untrusted-network-{mode}"));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        AssertNoTrustedVisualProviderVerdictMetadata(result.Value!.BoundaryMetadata);
        Assert.Equal(bool.FalseString, result.Value!.BoundaryMetadata["network_call_executed"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["external_network_call_executed"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["external_provider_called"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["real_external_provider_executed"]);
        Assert.Equal(
            bool.FalseString,
            result.Value.BoundaryMetadata[DesignConversionModelServerNetworkBoundary.ServerOwnedVisualProviderNetworkCallVerifiedKey]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["visual_fidelity_verified"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["product_ready"]);
    }

    [Fact]
    public void DesignConversionLoraCheckpointConfiguration_OwnsCanonicalTargetStorage()
    {
        var callerTargets = new[] { "v_proj", "q_proj", "q_proj" };
        var snapshot = new DesignConversionLoraCheckpointConfiguration(new LoraConfig
        {
            Rank = 2,
            Alpha = 4,
            Dropout = 0,
            TargetModules = callerTargets
        });

        callerTargets[0] = "o_proj";
        Assert.Equal(["q_proj", "v_proj"], snapshot.TargetModules);
        Assert.Throws<NotSupportedException>(() =>
            ((IList<string>)snapshot.TargetModules)[0] = "o_proj");
        var copiedView = snapshot.TargetModules.ToArray();
        copiedView[0] = "o_proj";
        var copiedConfig = snapshot.ToLoraConfig();
        copiedConfig.TargetModules[0] = "o_proj";
        Assert.Equal(["q_proj", "v_proj"], snapshot.TargetModules);

        var equalSnapshot = new DesignConversionLoraCheckpointConfiguration(new LoraConfig
        {
            Rank = 2,
            Alpha = 4,
            Dropout = 0,
            TargetModules = ["q_proj", "v_proj"]
        });
        var differentSnapshot = new DesignConversionLoraCheckpointConfiguration(new LoraConfig
        {
            Rank = 4,
            Alpha = 4,
            Dropout = 0,
            TargetModules = ["q_proj", "v_proj"]
        });
        Assert.Equal(snapshot, equalSnapshot);
        Assert.Equal(snapshot.GetHashCode(), equalSnapshot.GetHashCode());
        Assert.NotEqual(snapshot, differentSnapshot);
    }

    [Fact]
    public void DesignConversionDpoTrainingHandoff_LoraBoundariesUseNonFatalExceptionFilters()
    {
        AssertNonFatalFilters(
            nameof(DesignConversionDpoTrainingHandoff.ExecuteWithAuthorityAsync),
            [
                typeof(ServerOwnedLegalTrainingTeacherAdmissionAuthority),
                typeof(IEnumerable<DesignConversionDpoTrainingRecord>),
                typeof(IDesignConversionDpoPolicyBaseProvider),
                typeof(BpeTokenizer),
                typeof(string),
                typeof(LoraConfig),
                typeof(int),
                typeof(CancellationToken)
            ],
            minimumClassifierFilterCount: 1);
        AssertNonFatalFilters(
            nameof(DesignConversionDpoTrainingHandoff.ConvertWithAuthorityAsync),
            [
                typeof(ServerOwnedLegalTrainingTeacherAdmissionAuthority),
                typeof(DesignConversionDpoTrainingRecord),
                typeof(string),
                typeof(IDesignConversionDpoPolicyBaseProvider),
                typeof(DesignConversionLoraCheckpointConfiguration),
                typeof(string),
                typeof(CancellationToken)
            ],
            minimumClassifierFilterCount: 2);

        Assert.False(SnapshotExceptionGraphClassifier.IsNonFatal(
            new AggregateException(new InvalidOperationException(
                "outer",
                new OperationCanceledException("nested cancellation")))));
        Assert.False(SnapshotExceptionGraphClassifier.IsNonFatal(
            new AggregateException(new InvalidOperationException(
                "outer",
                new OutOfMemoryException("nested fatal")))));
        Assert.True(SnapshotExceptionGraphClassifier.IsNonFatal(
            new InvalidOperationException("ordinary validation failure")));
    }

    [Fact]
    public void DesignConversionDpoTrainingHandoff_PublicApiRequiresOpaqueServerOwnedAuthority()
    {
        var staticExecuteParameters = new[]
        {
            typeof(IEnumerable<DesignConversionDpoTrainingRecord>),
            typeof(IDesignConversionDpoPolicyBaseProvider),
            typeof(BpeTokenizer),
            typeof(string),
            typeof(LoraConfig),
            typeof(int),
            typeof(CancellationToken)
        };
        var staticConvertParameters = new[]
        {
            typeof(DesignConversionDpoTrainingRecord),
            typeof(string),
            typeof(IDesignConversionDpoPolicyBaseProvider),
            typeof(DesignConversionLoraCheckpointConfiguration),
            typeof(string),
            typeof(CancellationToken)
        };
        Type[] authorityExecuteParameters =
        [
            typeof(ServerOwnedLegalTrainingTeacherAdmissionAuthority),
            .. staticExecuteParameters
        ];
        Type[] authorityConvertParameters =
        [
            typeof(ServerOwnedLegalTrainingTeacherAdmissionAuthority),
            .. staticConvertParameters
        ];

        var publicStaticMethods = GetRelevantHandoffMethods(
            typeof(DesignConversionDpoTrainingHandoff),
            BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
        Assert.Equal(4, publicStaticMethods.Length);
        AssertExactPublicMethodSignature(
            publicStaticMethods,
            nameof(DesignConversionDpoTrainingHandoff.ExecuteAsync),
            staticExecuteParameters);
        AssertExactPublicMethodSignature(
            publicStaticMethods,
            nameof(DesignConversionDpoTrainingHandoff.ExecuteAsync),
            authorityExecuteParameters);
        AssertExactPublicMethodSignature(
            publicStaticMethods,
            nameof(DesignConversionDpoTrainingHandoff.ConvertPolicyCheckpointToRuntimeCheckpointAsync),
            staticConvertParameters);
        AssertExactPublicMethodSignature(
            publicStaticMethods,
            nameof(DesignConversionDpoTrainingHandoff.ConvertPolicyCheckpointToRuntimeCheckpointAsync),
            authorityConvertParameters);

        var legacyMethods = publicStaticMethods
            .Where(method => method.GetParameters().All(parameter =>
                parameter.ParameterType != typeof(ServerOwnedLegalTrainingTeacherAdmissionAuthority)))
            .ToArray();
        var authorityMethods = publicStaticMethods
            .Where(method => method.GetParameters().Any(parameter =>
                parameter.ParameterType == typeof(ServerOwnedLegalTrainingTeacherAdmissionAuthority)))
            .ToArray();
        Assert.Equal(2, legacyMethods.Length);
        Assert.Equal(2, authorityMethods.Length);
        Assert.All(legacyMethods, method =>
            Assert.NotNull(method.GetCustomAttribute<ObsoleteAttribute>()));
        Assert.All(authorityMethods, method =>
            Assert.Null(method.GetCustomAttribute<ObsoleteAttribute>()));
        Assert.All(publicStaticMethods, method =>
            Assert.DoesNotContain(method.GetParameters(), parameter =>
                parameter.ParameterType == typeof(LegalTrainingTeacherAdmissionAuthority)
                || parameter.ParameterType == typeof(LegalTrainingTeacherPolicyEntry)
                || parameter.ParameterType == typeof(LlmModel)));

        var internalExecute = FindDeclaredMethod(
            typeof(DesignConversionDpoTrainingHandoff),
            nameof(DesignConversionDpoTrainingHandoff.ExecuteWithAuthorityAsync),
            BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly,
            [
                typeof(ServerOwnedLegalTrainingTeacherAdmissionAuthority),
                typeof(IEnumerable<DesignConversionDpoTrainingRecord>),
                typeof(IDesignConversionDpoPolicyBaseProvider),
                typeof(BpeTokenizer),
                typeof(string),
                typeof(LoraConfig),
                typeof(int),
                typeof(CancellationToken)
            ]);
        var internalConvert = FindDeclaredMethod(
            typeof(DesignConversionDpoTrainingHandoff),
            nameof(DesignConversionDpoTrainingHandoff.ConvertWithAuthorityAsync),
            BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly,
            [
                typeof(ServerOwnedLegalTrainingTeacherAdmissionAuthority),
                typeof(DesignConversionDpoTrainingRecord),
                typeof(string),
                typeof(IDesignConversionDpoPolicyBaseProvider),
                typeof(DesignConversionLoraCheckpointConfiguration),
                typeof(string),
                typeof(CancellationToken)
            ]);
        Assert.True(internalExecute.IsAssembly);
        Assert.True(internalConvert.IsAssembly);
        Assert.False(internalExecute.IsPublic);
        Assert.False(internalConvert.IsPublic);
        Assert.DoesNotContain(
            typeof(DesignConversionDpoTrainingHandoff)
                .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static),
            field => field.FieldType == typeof(LegalTrainingTeacherAdmissionAuthority)
                     || field.FieldType == typeof(ServerOwnedLegalTrainingTeacherAdmissionAuthority));
        Assert.Null(typeof(DesignConversionDpoTrainingHandoff).Assembly.GetType(
            "ModelServer.DesignConversion.DesignConversionDpoTrainingHandoffService",
            throwOnError: false,
            ignoreCase: false));
    }

    [Fact]
    public async Task DesignConversionDpoTrainingHandoff_LegacyAndMissingAuthorityFailClosedBeforeEffects()
    {
        var record = BuildDpoHandoffRecord();
        var legacyRows = new ModelServerMoveNextProbeEnumerable<DesignConversionDpoTrainingRecord>([record]);
        var nullAuthorityRows = new ModelServerMoveNextProbeEnumerable<DesignConversionDpoTrainingRecord>([record]);
        var unavailableRows = new ModelServerMoveNextProbeEnumerable<DesignConversionDpoTrainingRecord>([record]);
        var cancelledAuthorityRows = new ModelServerMoveNextProbeEnumerable<DesignConversionDpoTrainingRecord>([record]);
        var provider = new DesignConversionDpoPolicyBaseTestProvider();
        var root = Path.Combine(Path.GetTempPath(), $"handoff-authority-zero-touch-{Guid.NewGuid():N}");
        var checkpointPath = Path.Combine(root, "missing-policy.bin");
        var runtimePath = Path.Combine(root, "runtime.json");

#pragma warning disable CS0618
        var legacyExecute = await DesignConversionDpoTrainingHandoff.ExecuteAsync(
            legacyRows,
            provider,
            null!,
            root);
        var legacyConvert = await DesignConversionDpoTrainingHandoff
            .ConvertPolicyCheckpointToRuntimeCheckpointAsync(
                null!,
                checkpointPath,
                provider,
                null,
                runtimePath);
        using var cancelled = new CancellationTokenSource();
        cancelled.Cancel();
        var legacyCancelled = await DesignConversionDpoTrainingHandoff.ExecuteAsync(
            legacyRows,
            provider,
            null!,
            root,
            cancellationToken: cancelled.Token);
        var legacyConvertCancelled = await DesignConversionDpoTrainingHandoff
            .ConvertPolicyCheckpointToRuntimeCheckpointAsync(
                null!,
                checkpointPath,
                provider,
                null,
                runtimePath,
                cancelled.Token);
#pragma warning restore CS0618

        var nullAuthority = await DesignConversionDpoTrainingHandoff.ExecuteAsync(
            null!,
            nullAuthorityRows,
            provider,
            null!,
            root);
        var unavailableAuthority = await DesignConversionDpoTrainingHandoff.ExecuteAsync(
            ServerOwnedLegalTrainingTeacherAdmissionAuthority.Unavailable(),
            unavailableRows,
            provider,
            null!,
            root);
        var nullConvert = await DesignConversionDpoTrainingHandoff
            .ConvertPolicyCheckpointToRuntimeCheckpointAsync(
                null!,
                null!,
                checkpointPath,
                provider,
                null,
                runtimePath);
        var unavailableConvert = await DesignConversionDpoTrainingHandoff
            .ConvertPolicyCheckpointToRuntimeCheckpointAsync(
                ServerOwnedLegalTrainingTeacherAdmissionAuthority.Unavailable(),
                null!,
                checkpointPath,
                provider,
                null,
                runtimePath);
        var cancelledAuthority = await DesignConversionDpoTrainingHandoff.ExecuteAsync(
            null!,
            cancelledAuthorityRows,
            provider,
            null!,
            root,
            cancellationToken: cancelled.Token);
        var cancelledConvert = await DesignConversionDpoTrainingHandoff
            .ConvertPolicyCheckpointToRuntimeCheckpointAsync(
                null!,
                null!,
                checkpointPath,
                provider,
                null,
                runtimePath,
                cancelled.Token);

        foreach (var result in new[] { legacyExecute, nullAuthority, unavailableAuthority })
        {
            Assert.False(result.IsSuccess);
            Assert.Equal(
                LegalTrainingTeacherAdmissionAuthority.ConfigurationRequiredErrorCode,
                result.ErrorCode);
        }
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.ConfigurationRequiredErrorCode,
            legacyConvert.ErrorCode);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.ConfigurationRequiredErrorCode,
            nullConvert.ErrorCode);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.ConfigurationRequiredErrorCode,
            unavailableConvert.ErrorCode);
        Assert.Equal("CANCELLED", legacyCancelled.ErrorCode);
        Assert.Equal("CANCELLED", legacyConvertCancelled.ErrorCode);
        Assert.Equal("CANCELLED", cancelledAuthority.ErrorCode);
        Assert.Equal("CANCELLED", cancelledConvert.ErrorCode);
        Assert.Equal(0, legacyRows.MoveNextCallCount);
        Assert.Equal(0, nullAuthorityRows.MoveNextCallCount);
        Assert.Equal(0, unavailableRows.MoveNextCallCount);
        Assert.Equal(0, cancelledAuthorityRows.MoveNextCallCount);
        Assert.Equal(0, provider.InvocationCount);
        Assert.False(Directory.Exists(root));
    }

    [Fact]
    public async Task DesignConversionDpoTrainingHandoff_InvalidLoraFailsBeforeRecordEnumerationOrEffects()
    {
        var records = new ModelServerMoveNextProbeEnumerable<DesignConversionDpoTrainingRecord>(
            [BuildDpoHandoffRecord()],
            throwOnMoveNextCall: 1,
            new InvalidOperationException("records must not be enumerated before LoRA validation"));
        var provider = new DesignConversionDpoPolicyBaseTestProvider();
        var root = Path.Combine(
            Path.GetTempPath(),
            $"handoff-invalid-lora-zero-touch-{Guid.NewGuid():N}");

        var result = await DesignConversionDpoTrainingHandoff.ExecuteAsync(
            DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
            records,
            provider,
            BuildDpoHandoffTokenizer(),
            root,
            new LoraConfig
            {
                Rank = 0,
                Alpha = 4,
                Dropout = 0,
                TargetModules = ["q_proj", "v_proj"]
            });

        Assert.False(result.IsSuccess);
        Assert.Equal("DESIGN_CONVERSION_DPO_LORA_CONFIG_INVALID", result.ErrorCode);
        Assert.Equal(0, records.MoveNextCallCount);
        Assert.Equal(0, provider.InvocationCount);
        Assert.Null(provider.LastModelSet);
        Assert.False(Directory.Exists(root));
    }

    [Fact]
    public async Task DesignConversionDpoTrainingHandoff_ExactQwenDigestAdmitsAndOneCharacterMismatchFailsBeforeEffects()
    {
        const string modelId = "qwen3.5:9b";
        const string privateRequest = "PRIVATE-QWEN-HANDOFF-REQUEST";
        var manifestDigest = new string('d', 64);
        var layerDigest = new string('c', 64);
        var mismatchManifestDigest = manifestDigest[..^1] + "e";
        var exactCapability = DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(
            maxTeacherCalls: 1,
            modelId: modelId,
            expectedOllamaManifestDigest: manifestDigest,
            expectedOllamaModelLayerDigest: layerDigest);
        var mismatchCapability = DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(
            maxTeacherCalls: 1,
            modelId: modelId,
            expectedOllamaManifestDigest: manifestDigest,
            expectedOllamaModelLayerDigest: layerDigest);
        var source = BuildDpoHandoffRecord();
        var exactProvenance = new TrainingTeacherProvenance(
            TrainingTeacherSourceKind.ExternalTeacherOutput,
            OpenWeightTeacherFamily.Qwen,
            TrainingTeacherAccessMode.LocalHostedOpenWeight,
            TrainingTeacherHostingProvider.Ollama,
            modelId,
            manifestDigest,
            TrainingTeacherModelLicense.Apache20,
            ProviderTermsChecked: false,
            ProviderTermsAllowTraining: false,
            ClosedProprietaryModel: false,
            TeacherOutputUsedForTraining: true,
            privateRequest,
            InvocationCount: 1)
        {
            ModelArtifactDigestKind =
                LegalTrainingTeacherAdmissionAuthority.OllamaModelLayerSha256DigestKind,
            ModelArtifactDigest = layerDigest
        };
        var exactRecord = source with
        {
            TeacherProvenance = exactProvenance,
            Chosen = source.Chosen with
            {
                Verdict = CreateVerdict("exact-qwen-low-delta-chosen", 0.52, passed: true),
                Accepted = true
            },
            Rejected = source.Rejected with
            {
                Verdict = CreateVerdict("exact-qwen-low-delta-rejected", 0.49, passed: false),
                Accepted = false
            }
        };
        var mismatchRecord = exactRecord with
        {
            RecordId = "qwen-one-character-manifest-mismatch",
            TeacherProvenance = exactProvenance with
            {
                ModelVersion = mismatchManifestDigest,
                RequestId = privateRequest + "-MISMATCH"
            }
        };
        var exactProvider = new DesignConversionDpoPolicyBaseTestProvider();
        var mismatchProvider = new DesignConversionDpoPolicyBaseTestProvider();
        var exactRoot = Path.Combine(Path.GetTempPath(), $"handoff-exact-qwen-{Guid.NewGuid():N}");
        var mismatchRoot = Path.Combine(Path.GetTempPath(), $"handoff-mismatch-qwen-{Guid.NewGuid():N}");

        var exact = await DesignConversionDpoTrainingHandoff.ExecuteAsync(
            exactCapability,
            [exactRecord],
            exactProvider,
            BuildDpoHandoffTokenizer(),
            exactRoot);
        var mismatch = await DesignConversionDpoTrainingHandoff.ExecuteAsync(
            mismatchCapability,
            [mismatchRecord],
            mismatchProvider,
            BuildDpoHandoffTokenizer(),
            mismatchRoot);

        Assert.True(exact.IsSuccess, exact.ErrorMessage);
        Assert.Equal(1, exact.Value!.LegalTeacherAdmission.ExternalTeacherInvocationCount);
        Assert.Equal(0, exact.Value.DpoStepCount);
        Assert.False(Directory.Exists(exactRoot));
        Assert.Equal(0, exactProvider.InvocationCount);
        Assert.False(mismatch.IsSuccess);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.NotAllowlistedErrorCode,
            mismatch.ErrorCode);
        Assert.Equal(0, mismatchProvider.InvocationCount);
        Assert.False(Directory.Exists(mismatchRoot));
        var serializedMismatch = JsonSerializer.Serialize(mismatch);
        Assert.DoesNotContain(privateRequest, serializedMismatch, StringComparison.Ordinal);
        Assert.DoesNotContain(modelId, serializedMismatch, StringComparison.Ordinal);
        Assert.DoesNotContain(manifestDigest, serializedMismatch, StringComparison.Ordinal);
        Assert.DoesNotContain(mismatchManifestDigest, serializedMismatch, StringComparison.Ordinal);
        Assert.DoesNotContain(layerDigest, serializedMismatch, StringComparison.Ordinal);
    }

    [Fact]
    public void DpoTrainerPolicyCheckpointBridge_SourceEncoderSignalFeedsPolicyAndRuntimePrompts()
    {
        var record = BuildDpoPolicyBridgeSourceEncodedRecord();
        var policyPrompt = DesignConversionDpoPolicyCheckpointBridge.BuildPolicyPromptPreviewForTests(record);
        var runtimePrompt = DesignConversionDpoPolicyCheckpointBridge.BuildPolicyRuntimeBridgePromptPreviewForTests(
            record,
            "policy-checkpoint-0001.json",
            new string('a', 64));

        Assert.StartsWith("srcenc ", policyPrompt, StringComparison.Ordinal);
        var encodedWindow = Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(policyPrompt).Take(64).ToArray());
        Assert.Contains("srcenc", encodedWindow, StringComparison.Ordinal);
        Assert.Contains("vecp=", encodedWindow, StringComparison.Ordinal);
        Assert.Contains("schema=", policyPrompt, StringComparison.Ordinal);
        Assert.Contains("sig=", policyPrompt, StringComparison.Ordinal);
        Assert.Contains("vecp=", policyPrompt, StringComparison.Ordinal);
        Assert.DoesNotContain("vech=", policyPrompt, StringComparison.Ordinal);
        Assert.DoesNotContain("manifest=", policyPrompt, StringComparison.Ordinal);
        Assert.DoesNotContain("structure=", policyPrompt, StringComparison.Ordinal);
        Assert.DoesNotContain(record.Provenance.Metadata["source_encoder_vector_hash"], policyPrompt, StringComparison.Ordinal);
        Assert.DoesNotContain(record.SourceStructurePayload, policyPrompt, StringComparison.Ordinal);

        Assert.Equal(
            DesignConversionDpoDatasetBuilder.BuildOptimizerPromptPayload(
                record,
                DesignConversionMachine.DomainTag),
            runtimePrompt);
        using var promptJson = JsonDocument.Parse(runtimePrompt);
        var root = promptJson.RootElement;
        Assert.Equal(
            ["domain_tag", "unit_id", "source_kind", "internal_vector", "target", "source", "source_encoder"],
            root.EnumerateObject().Select(property => property.Name).ToArray());
        Assert.Equal(record.InternalVectorPayload, root.GetProperty("internal_vector").EnumerateArray().Select(value => value.GetDouble()).ToArray());
        Assert.Contains("FRAME", root.GetProperty("source").GetProperty("structure_payload").GetString(), StringComparison.Ordinal);
        Assert.False(root.GetProperty("source").TryGetProperty("identity_hash", out _));
        Assert.False(root.GetProperty("source_encoder").TryGetProperty("vector_hash", out _));
        Assert.DoesNotContain(record.Provenance.Metadata["source_encoder_vector_hash"], runtimePrompt, StringComparison.Ordinal);
        Assert.DoesNotContain("pair_family", runtimePrompt, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("checkpoint", runtimePrompt, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("C:/Users/lubao", runtimePrompt, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("private/source.fig", runtimePrompt, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void DpoTrainerPolicyCheckpointBridge_PreferenceTextContainsOnlyCanonicalHtmlCss()
    {
        var record = BuildDpoHandoffRecord();
        var preferenceText = DesignConversionDpoPolicyCheckpointBridge.BuildPolicyPreferenceTextPreviewForTests(record);
        var combinedPreferenceText = $"{preferenceText.Chosen} {preferenceText.Rejected}";

        using var chosenJson = JsonDocument.Parse(preferenceText.Chosen);
        using var rejectedJson = JsonDocument.Parse(preferenceText.Rejected);
        Assert.Equal(["html", "css"], chosenJson.RootElement.EnumerateObject().Select(property => property.Name).ToArray());
        Assert.Equal(["html", "css"], rejectedJson.RootElement.EnumerateObject().Select(property => property.Name).ToArray());
        Assert.Equal(record.Chosen.Html, chosenJson.RootElement.GetProperty("html").GetString());
        Assert.Equal(record.Chosen.Css, chosenJson.RootElement.GetProperty("css").GetString());
        Assert.Equal(record.Rejected.Html, rejectedJson.RootElement.GetProperty("html").GetString());
        Assert.Equal(record.Rejected.Css, rejectedJson.RootElement.GetProperty("css").GetString());
        foreach (var forbidden in new[] { "quality_role", "visual_score", "defect", "accepted", "passed", "score", "verdict", "candidate_id", "output_id" })
            Assert.DoesNotContain(forbidden, combinedPreferenceText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void DpoTrainerPolicyCheckpointBridge_RuntimeResponsesContainOnlyCanonicalHtmlCss()
    {
        var record = BuildDpoHandoffRecord();
        var item = DesignConversionDpoPolicyCheckpointBridge.BuildPolicyRuntimeBridgeItemPreviewForTests(
            record,
            "policy-checkpoint-0001.json",
            new string('b', 64));
        var combinedResponses = $"{item.ChosenResponse} {item.RejectedResponse}";

        using var chosenJson = JsonDocument.Parse(item.ChosenResponse);
        using var rejectedJson = JsonDocument.Parse(item.RejectedResponse);
        var chosen = chosenJson.RootElement;
        var rejected = rejectedJson.RootElement;

        Assert.Equal(["html", "css"], chosen.EnumerateObject().Select(property => property.Name).ToArray());
        Assert.Equal(["html", "css"], rejected.EnumerateObject().Select(property => property.Name).ToArray());
        Assert.Equal(record.Chosen.Html, chosen.GetProperty("html").GetString());
        Assert.Equal(record.Chosen.Css, chosen.GetProperty("css").GetString());
        Assert.Equal(record.Rejected.Html, rejected.GetProperty("html").GetString());
        Assert.Equal(record.Rejected.Css, rejected.GetProperty("css").GetString());
        foreach (var forbidden in new[] { "quality_role", "visual_score", "defect", "accepted", "passed", "score", "verdict", "candidate_id", "output_id" })
            Assert.DoesNotContain(forbidden, combinedResponses, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void DpoTrainerPolicyCheckpointBridge_SourceEncoderSourceIdsOmitMissingMetadataValues()
    {
        var sourceEncodedRecord = BuildDpoPolicyBridgeSourceEncodedRecord();
        var sourceEncodedIds = DesignConversionDpoPolicyCheckpointBridge.BuildSourceSignalSourceIdsPreviewForTests(sourceEncodedRecord);
        var legacyRecord = sourceEncodedRecord with
        {
            Provenance = sourceEncodedRecord.Provenance with
            {
                Metadata = new Dictionary<string, string> { ["split"] = "legacy-row-without-source-encoder-metadata" }
            }
        };

        var sourceIds = DesignConversionDpoPolicyCheckpointBridge.BuildSourceSignalSourceIdsPreviewForTests(legacyRecord);

        Assert.All(sourceEncodedIds, sourceId => Assert.DoesNotContain(":missing", sourceId, StringComparison.OrdinalIgnoreCase));
        Assert.Contains(
            sourceEncodedIds,
            sourceId => sourceId.EndsWith(sourceEncodedRecord.Provenance.Metadata["source_encoder_vector_hash"], StringComparison.Ordinal));
        Assert.All(sourceIds, sourceId => Assert.DoesNotContain(":missing", sourceId, StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(
            sourceIds,
            sourceId => sourceId.StartsWith("dpo_policy_bridge_source_encoder:", StringComparison.Ordinal));
        Assert.DoesNotContain(
            sourceIds,
            sourceId => sourceId.StartsWith("dpo_policy_bridge_source_encoder_schema:", StringComparison.Ordinal));
        Assert.DoesNotContain(
            sourceIds,
            sourceId => sourceId.StartsWith("dpo_policy_bridge_source_encoder_vector_hash:", StringComparison.Ordinal));
        Assert.Contains(
            sourceIds,
            sourceId => sourceId.StartsWith("dpo_policy_bridge_source_vector_payload_hash:dpo-policy-source-vector-", StringComparison.Ordinal));
    }

    [Fact]
    public async Task DesignConversionDpoTrainingHandoff_MissingTeacherProvenanceFailsBeforeCheckpointOrAdapterSideEffects()
    {
        var checkpointDir = Path.Combine(
            Path.GetTempPath(),
            $"design-conversion-legal-gate-{Guid.NewGuid():N}");
        var provider = new DesignConversionDpoPolicyBaseTestProvider();
        var tokenizer = BuildDpoHandoffTokenizer();
        var oldSerializedRecord = JsonSerializer.Serialize(
            BuildDpoHandoffRecord() with { TeacherProvenance = null });
        var record = Assert.IsType<DesignConversionDpoTrainingRecord>(
            JsonSerializer.Deserialize<DesignConversionDpoTrainingRecord>(oldSerializedRecord));

        try
        {
            var result = await DesignConversionDpoTrainingHandoff.ExecuteAsync(
                DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                [record],
                provider,
                tokenizer,
                checkpointDir,
                new LoraConfig
                {
                    Rank = 2,
                    Alpha = 4,
                    Dropout = 0,
                    TargetModules = ["q_proj", "v_proj"]
                },
                maxStepCount: 1);

            Assert.False(result.IsSuccess);
            Assert.Equal(
                LegalTrainingTeacherAdmissionAuthority.ProvenanceRequiredErrorCode,
                result.ErrorCode);
            Assert.False(Directory.Exists(checkpointDir));
            Assert.Equal(0, provider.InvocationCount);
            Assert.Null(provider.LastModelSet);
        }
        finally
        {
            if (Directory.Exists(checkpointDir))
                Directory.Delete(checkpointDir, recursive: true);
        }
    }

    [Fact]
    public void DpoTrainingContentCapture_MaxPlusOneAndSizeLimitsPrecedeMissingTeacherProvenance()
    {
        var missingProvenance = BuildDpoHandoffRecord() with
        {
            TeacherProvenance = null
        };
        var over = new ModelServerMoveNextProbeEnumerable<
            DesignConversionDpoTrainingRecord>(
            Enumerable.Repeat(
                missingProvenance,
                BoundedDpoTrainingContentProjector.MaximumRowCount + 1));

        var overResult = DesignConversionDpoTrainingContentAuthority
            .CaptureRecords(over);

        Assert.False(overResult.IsSuccess);
        Assert.Equal(
            BoundedDpoTrainingContentProjector.LimitExceededErrorCode,
            overResult.ErrorCode);
        Assert.Equal(
            BoundedDpoTrainingContentProjector.MaximumRowCount + 1,
            over.MoveNextCallCount);
        Assert.Equal(
            BoundedDpoTrainingContentProjector.MaximumRowCount + 1,
            over.YieldedCount);

        var oversizedResult = DesignConversionDpoTrainingContentAuthority
            .CaptureRecords(
            [
                missingProvenance with
                {
                    RecordId = new string(
                        'x',
                        BoundedDpoTrainingContentProjector
                            .MaximumTextUtf8Bytes + 1)
                }
            ]);

        Assert.False(oversizedResult.IsSuccess);
        Assert.Equal(
            BoundedDpoTrainingContentProjector.LimitExceededErrorCode,
            oversizedResult.ErrorCode);
    }

    [Fact]
    public async Task DesignConversionDpoTrainingHandoff_MaxPlusOneRowsFailBeforeContentOrModelFileEffects()
    {
        var exactCheckpointDir = Path.Combine(
            Path.GetTempPath(),
            $"design-conversion-legal-row-exact-{Guid.NewGuid():N}");
        var overCheckpointDir = Path.Combine(
            Path.GetTempPath(),
            $"design-conversion-legal-row-over-{Guid.NewGuid():N}");
        var exactProvider = new DesignConversionDpoPolicyBaseTestProvider();
        var overProvider = new DesignConversionDpoPolicyBaseTestProvider();
        var tokenizer = BuildDpoHandoffTokenizer();
        var sourceRecord = BuildDpoHandoffRecord();
        var lowDeltaRecord = sourceRecord with
        {
            RecordId = "r",
            UnitId = "u",
            InternalUnitStatePayload = "s",
            InternalVectorPayload = [0.25],
            SourceStructurePayload = "s",
            UsefulnessScore = 0,
            NegativeQualityReason = "n",
            NegativeReasonTaxonomy = "n",
            Provenance = sourceRecord.Provenance with
            {
                SourceRequestId = "s",
                SourceKind = "s",
                SourceManifestRef = "s",
                ReviewerRef = "s",
                Metadata = new Dictionary<string, string>()
            },
            License = sourceRecord.License with
            {
                LicenseId = "l",
                RightsScope = "l",
                Metadata = new Dictionary<string, string>()
            },
            Chosen = sourceRecord.Chosen with
            {
                OutputId = "c",
                CandidateId = "c",
                Html = "c",
                Css = "c",
                Verdict = CreateVerdict("c", 0.52, passed: true) with
                {
                    Defects = [],
                    EvidenceRefs = []
                },
                QaResults = [],
                EvidenceRefs = [],
                Accepted = true
            },
            Rejected = sourceRecord.Rejected with
            {
                OutputId = "r",
                CandidateId = "r",
                Html = "r",
                Css = "r",
                Verdict = CreateVerdict("r", 0.49, passed: false) with
                {
                    Defects = [],
                    EvidenceRefs = []
                },
                QaResults = [],
                EvidenceRefs = [],
                Accepted = false
            }
        };
        var exact = new ModelServerMoveNextProbeEnumerable<DesignConversionDpoTrainingRecord>(
            Enumerable.Range(
                    0,
                    LegalTrainingTeacherAdmissionAuthority.MaxBatchRows)
                .Select(index => lowDeltaRecord with
                {
                    RecordId = $"r{index:x}",
                    TeacherProvenance = TrainingTeacherProvenance.Deterministic(
                        $"q{index:x}")
                }));
        var over = new ModelServerMoveNextProbeEnumerable<DesignConversionDpoTrainingRecord>(
            Enumerable.Repeat(
                lowDeltaRecord with { TeacherProvenance = null },
                LegalTrainingTeacherAdmissionAuthority.MaxBatchRows + 1));

        try
        {
            var exactResult = await DesignConversionDpoTrainingHandoff.ExecuteAsync(
                DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                exact,
                exactProvider,
                tokenizer,
                exactCheckpointDir,
                new LoraConfig
                {
                    Rank = 2,
                    Alpha = 4,
                    Dropout = 0,
                    TargetModules = ["q_proj", "v_proj"]
                },
                maxStepCount: 1);
            Assert.True(exactResult.IsSuccess, exactResult.ErrorMessage);
            Assert.Equal(0, exactResult.Value!.DpoStepCount);
            Assert.Equal(
                LegalTrainingTeacherAdmissionAuthority.MaxBatchRows + 1,
                exact.MoveNextCallCount);
            Assert.Equal(
                LegalTrainingTeacherAdmissionAuthority.MaxBatchRows,
                exact.YieldedCount);
            Assert.False(Directory.Exists(exactCheckpointDir));
            Assert.Equal(0, exactProvider.InvocationCount);
            Assert.Null(exactProvider.LastModelSet);

            var result = await DesignConversionDpoTrainingHandoff.ExecuteAsync(
                DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                over,
                overProvider,
                tokenizer,
                overCheckpointDir,
                new LoraConfig
                {
                    Rank = 2,
                    Alpha = 4,
                    Dropout = 0,
                    TargetModules = ["q_proj", "v_proj"]
                },
                maxStepCount: 1);

            Assert.False(result.IsSuccess);
            Assert.Equal(
                BoundedDpoTrainingContentProjector.LimitExceededErrorCode,
                result.ErrorCode);
            Assert.Equal(
                LegalTrainingTeacherAdmissionAuthority.MaxBatchRows + 1,
                over.MoveNextCallCount);
            Assert.Equal(
                LegalTrainingTeacherAdmissionAuthority.MaxBatchRows + 1,
                over.YieldedCount);
            Assert.False(Directory.Exists(overCheckpointDir));
            Assert.Equal(0, overProvider.InvocationCount);
            Assert.Null(overProvider.LastModelSet);
        }
        finally
        {
            if (Directory.Exists(exactCheckpointDir))
                Directory.Delete(exactCheckpointDir, recursive: true);
            if (Directory.Exists(overCheckpointDir))
                Directory.Delete(overCheckpointDir, recursive: true);
        }
    }

    [Fact]
    public async Task DesignConversionDpoTrainingHandoff_RecordEnumerationFailsClosedButCancellationAndFatalGraphsEscape()
    {
        var checkpointDir = Path.Combine(
            Path.GetTempPath(),
            $"design-conversion-legal-enumeration-{Guid.NewGuid():N}");
        var provider = new DesignConversionDpoPolicyBaseTestProvider();
        var tokenizer = BuildDpoHandoffTokenizer();
        var record = BuildDpoHandoffRecord();

        try
        {
            var ordinary = await DesignConversionDpoTrainingHandoff.ExecuteAsync(
                DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                new ModelServerMoveNextProbeEnumerable<DesignConversionDpoTrainingRecord>(
                    [record],
                    throwOnMoveNextCall: 2,
                    new InvalidOperationException("ordinary-record-enumeration")),
                provider,
                tokenizer,
                checkpointDir);
            Assert.Equal(
                LegalTrainingTeacherAdmissionAuthority.EnumerationFailedErrorCode,
                ordinary.ErrorCode);

            await Assert.ThrowsAsync<OperationCanceledException>(() =>
                DesignConversionDpoTrainingHandoff.ExecuteAsync(
                    DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                    new ModelServerMoveNextProbeEnumerable<DesignConversionDpoTrainingRecord>(
                        [record],
                        throwOnMoveNextCall: 2,
                        new OperationCanceledException("record-enumeration-cancelled")),
                    provider,
                    tokenizer,
                    checkpointDir));
            await Assert.ThrowsAsync<AggregateException>(() =>
                DesignConversionDpoTrainingHandoff.ExecuteAsync(
                    DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                    new ModelServerMoveNextProbeEnumerable<DesignConversionDpoTrainingRecord>(
                        [record],
                        throwOnMoveNextCall: 2,
                        new AggregateException(
                            new InvalidOperationException(
                                "outer",
                                new OperationCanceledException("nested-cancellation")))),
                    provider,
                    tokenizer,
                    checkpointDir));
            await Assert.ThrowsAsync<AggregateException>(() =>
                DesignConversionDpoTrainingHandoff.ExecuteAsync(
                    DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                    new ModelServerMoveNextProbeEnumerable<DesignConversionDpoTrainingRecord>(
                        [record],
                        throwOnMoveNextCall: 2,
                        new AggregateException(
                            new InvalidOperationException(
                                "outer",
                                new OutOfMemoryException("nested-fatal")))),
                    provider,
                    tokenizer,
                    checkpointDir));

            Assert.False(Directory.Exists(checkpointDir));
            Assert.Equal(0, provider.InvocationCount);
            Assert.Null(provider.LastModelSet);
        }
        finally
        {
            if (Directory.Exists(checkpointDir))
                Directory.Delete(checkpointDir, recursive: true);
        }
    }

    [Fact]
    public async Task DesignConversionDpoTrainingHandoff_BridgeRejectsExternalTeacherBeforeValidCheckpointReadOrRuntimeMutation()
    {
        const string privateRequest = "PRIVATE-BRIDGE-TEACHER-REQUEST";
        var privateVersion = new string('a', 64);
        var root = Path.Combine(
            Path.GetTempPath(),
            $"design-conversion-legal-bridge-{Guid.NewGuid():N}");
        var checkpointPath = Path.Combine(root, "valid-policy.bin");
        var runtimePath = Path.Combine(root, "runtime", "checkpoint.json");
        var lora = new DesignConversionLoraCheckpointConfiguration(new LoraConfig
        {
            Rank = 2,
            Alpha = 4,
            Dropout = 0,
            TargetModules = ["q_proj", "v_proj"]
        });
        var checkpointModel = new LlmModel(BuildDpoHandoffTinyConfig());
        LoraApplicator.Apply(checkpointModel, lora.ToLoraConfig());
        Directory.CreateDirectory(root);
        checkpointModel.SaveCheckpoint(checkpointPath);
        var checkpointHashBefore = Convert.ToHexString(
            SHA256.HashData(File.ReadAllBytes(checkpointPath)));
        var checkpointLastWriteBefore = File.GetLastWriteTimeUtc(checkpointPath);
        var provider = new DesignConversionDpoPolicyBaseTestProvider();
        var record = BuildDpoHandoffRecord() with
        {
            TeacherProvenance = new TrainingTeacherProvenance(
                TrainingTeacherSourceKind.ExternalTeacherOutput,
                OpenWeightTeacherFamily.Qwen,
                TrainingTeacherAccessMode.LocalHostedOpenWeight,
                TrainingTeacherHostingProvider.Ollama,
                "qwen3.5-9b",
                privateVersion,
                TrainingTeacherModelLicense.Apache20,
                ProviderTermsChecked: false,
                ProviderTermsAllowTraining: false,
                ClosedProprietaryModel: false,
                TeacherOutputUsedForTraining: true,
                privateRequest,
                InvocationCount: 1)
            {
                ModelArtifactDigestKind =
                    LegalTrainingTeacherAdmissionAuthority.OllamaModelLayerSha256DigestKind,
                ModelArtifactDigest = LegalTeacherModelLayerDigest
            }
        };

        try
        {
            var result = await DesignConversionDpoTrainingHandoff
                .ConvertPolicyCheckpointToRuntimeCheckpointAsync(
                    DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                    record,
                    checkpointPath,
                    provider,
                    lora,
                    runtimePath);

            Assert.False(result.IsSuccess);
            Assert.Equal(
                LegalTrainingTeacherAdmissionAuthority.NotAllowlistedErrorCode,
                result.ErrorCode);
            Assert.Equal(
                checkpointHashBefore,
                Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(checkpointPath))));
            Assert.Equal(
                checkpointLastWriteBefore,
                File.GetLastWriteTimeUtc(checkpointPath));
            Assert.Equal(0, provider.InvocationCount);
            Assert.Null(provider.LastModelSet);
            Assert.False(Directory.Exists(Path.GetDirectoryName(runtimePath)!));
            Assert.False(File.Exists(runtimePath));
            var serialized = JsonSerializer.Serialize(result);
            Assert.DoesNotContain(privateRequest, serialized, StringComparison.Ordinal);
            Assert.DoesNotContain(privateVersion, serialized, StringComparison.Ordinal);
            Assert.DoesNotContain("qwen3.5-9b", serialized, StringComparison.Ordinal);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void DesignConversionDpoTrainingHandoff_LegalMetadataContainsOnlySanitizedReceipts()
    {
        const string privateRequest = "PRIVATE-METADATA-TEACHER-REQUEST";
        var authorityResult = LegalTrainingTeacherAdmissionAuthority.Create(
            [new LegalTrainingTeacherPolicyEntry(
                OpenWeightTeacherFamily.Qwen,
                TrainingTeacherAccessMode.LocalHostedOpenWeight,
                TrainingTeacherHostingProvider.Ollama,
                "qwen3.5-9b",
                MaxTeacherCallsExpectedTeacherManifestDigest,
                TrainingTeacherModelLicense.Apache20,
                $"sha256:{new string('a', 64)}",
                $"sha256:{new string('b', 64)}")
            {
                ModelArtifactDigestKind =
                    LegalTrainingTeacherAdmissionAuthority.OllamaModelLayerSha256DigestKind,
                ModelArtifactDigest = LegalTeacherModelLayerDigest
            }],
            maxTeacherCalls: 1,
            Enumerable.Range(1, 32).Select(static value => (byte)value).ToArray());
        Assert.True(authorityResult.IsSuccess, authorityResult.ErrorMessage);
        var admission = authorityResult.Value!.ValidateBatch([
            new TrainingTeacherProvenance(
                TrainingTeacherSourceKind.ExternalTeacherOutput,
                OpenWeightTeacherFamily.Qwen,
                TrainingTeacherAccessMode.LocalHostedOpenWeight,
                TrainingTeacherHostingProvider.Ollama,
                "qwen3.5-9b",
                MaxTeacherCallsExpectedTeacherManifestDigest,
                TrainingTeacherModelLicense.Apache20,
                ProviderTermsChecked: false,
                ProviderTermsAllowTraining: false,
                ClosedProprietaryModel: false,
                TeacherOutputUsedForTraining: true,
                privateRequest,
                InvocationCount: 1)
            {
                ModelArtifactDigestKind =
                    LegalTrainingTeacherAdmissionAuthority.OllamaModelLayerSha256DigestKind,
                ModelArtifactDigest = LegalTeacherModelLayerDigest
            }
        ]);
        Assert.True(admission.IsSuccess, admission.ErrorMessage);

        var metadata = DesignConversionDpoTrainingHandoff.BuildLegalTeacherMetadata(
            admission.Value!);
        var serialized = JsonSerializer.Serialize(metadata);

        Assert.DoesNotContain(privateRequest, serialized, StringComparison.Ordinal);
        Assert.DoesNotContain("qwen3.5-9b", serialized, StringComparison.Ordinal);
        Assert.DoesNotContain(
            MaxTeacherCallsExpectedTeacherManifestDigest,
            serialized,
            StringComparison.Ordinal);
        Assert.Contains("sha256:", serialized, StringComparison.Ordinal);
        Assert.Contains("LocalHostedOpenWeight", serialized, StringComparison.Ordinal);
        Assert.Contains("Ollama", serialized, StringComparison.Ordinal);
        Assert.Contains(
            LegalTrainingTeacherAdmissionAuthority.SelfHostedNoProviderTermsEvidenceMode,
            serialized,
            StringComparison.Ordinal);
        Assert.Contains($"sha256:{new string('a', 64)}", serialized, StringComparison.Ordinal);
        Assert.Contains($"sha256:{new string('b', 64)}", serialized, StringComparison.Ordinal);
        Assert.DoesNotContain("C:/", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("https://", serialized, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task DesignConversionDpoTrainingHandoff_ServerAuthorityAdmitsExternalLowDeltaWithoutTrainingOrRawIdentityLeakage()
    {
        const string privateRequest = "PRIVATE-SERVICE-TEACHER-REQUEST";
        const string privateModel = "qwen3-private-service-model:test";
        var privateVersion = new string('c', 64);
        var capability = DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(
            maxTeacherCalls: 1,
            modelId: privateModel,
            expectedOllamaManifestDigest: privateVersion,
            expectedOllamaModelLayerDigest: LegalTeacherModelLayerDigest);
        var comparisonCapability = DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(
            maxTeacherCalls: 1,
            modelId: privateModel,
            expectedOllamaManifestDigest: privateVersion,
            expectedOllamaModelLayerDigest: LegalTeacherModelLayerDigest);

        var teacherProvenance = new TrainingTeacherProvenance(
            TrainingTeacherSourceKind.ExternalTeacherOutput,
            OpenWeightTeacherFamily.Qwen,
            TrainingTeacherAccessMode.LocalHostedOpenWeight,
            TrainingTeacherHostingProvider.Ollama,
            privateModel,
            privateVersion,
            TrainingTeacherModelLicense.Apache20,
            ProviderTermsChecked: false,
            ProviderTermsAllowTraining: false,
            ClosedProprietaryModel: false,
            TeacherOutputUsedForTraining: true,
            privateRequest,
            InvocationCount: 1)
        {
            ModelArtifactDigestKind =
                LegalTrainingTeacherAdmissionAuthority.OllamaModelLayerSha256DigestKind,
            ModelArtifactDigest = LegalTeacherModelLayerDigest
        };
        var sourceRecord = BuildDpoHandoffRecord();
        var lowDeltaRecord = sourceRecord with
        {
            RecordId = "external-authority-low-delta-record",
            TeacherProvenance = teacherProvenance,
            Chosen = sourceRecord.Chosen with
            {
                Verdict = CreateVerdict("external-low-delta-chosen", 0.52, passed: true),
                Accepted = true
            },
            Rejected = sourceRecord.Rejected with
            {
                Verdict = CreateVerdict("external-low-delta-rejected", 0.49, passed: false),
                Accepted = false
            }
        };
        var staticProvider = new DesignConversionDpoPolicyBaseTestProvider();
        var tokenizer = BuildDpoHandoffTokenizer();
        var checkpointDir = Path.Combine(
            Path.GetTempPath(),
            $"design-conversion-external-low-delta-{Guid.NewGuid():N}");

        try
        {
            var staticResult = await DesignConversionDpoTrainingHandoff.ExecuteAsync(
                capability,
                [lowDeltaRecord],
                staticProvider,
                tokenizer,
                checkpointDir,
                maxStepCount: 1);
            Assert.True(staticResult.IsSuccess, staticResult.ErrorMessage);
            Assert.Equal(0, staticResult.Value!.DpoStepCount);
            Assert.Empty(staticResult.Value.MetricFilePaths);
            Assert.Empty(staticResult.Value.PolicyCheckpointFilePaths);
            Assert.Empty(staticResult.Value.IngredientIds);
            Assert.False(Directory.Exists(checkpointDir));
            Assert.Equal(0, staticProvider.InvocationCount);
            Assert.Null(staticProvider.LastModelSet);

            var comparisonAdmission = comparisonCapability.ValidateBatch([teacherProvenance]);
            Assert.True(comparisonAdmission.IsSuccess, comparisonAdmission.ErrorMessage);
            Assert.Equal(
                comparisonAdmission.Value!.PolicyIdentityRef,
                staticResult.Value.LegalTeacherAdmission.PolicyIdentityRef);
            Assert.Equal(
                comparisonAdmission.Value.ExternalTeacherInvocationCount,
                staticResult.Value.LegalTeacherAdmission.ExternalTeacherInvocationCount);
            Assert.Equal(string.Empty, comparisonAdmission.Value.TrainingContentBatchRef);
            Assert.StartsWith(
                "sha256:",
                staticResult.Value.LegalTeacherAdmission.TrainingContentBatchRef,
                StringComparison.Ordinal);
            var legacyReceipt = Assert.Single(comparisonAdmission.Value.Receipts);
            var contentBoundReceipt = Assert.Single(
                staticResult.Value.LegalTeacherAdmission.Receipts);
            Assert.Equal(string.Empty, legacyReceipt.TrainingContentRef);
            Assert.StartsWith(
                "sha256:",
                contentBoundReceipt.TrainingContentRef,
                StringComparison.Ordinal);
            Assert.Equal(
                legacyReceipt with
                {
                    TrainingContentRef = contentBoundReceipt.TrainingContentRef
                },
                contentBoundReceipt);
            Assert.All(
                comparisonAdmission.Value.EvidenceRefs,
                evidence => Assert.Contains(
                    evidence,
                    staticResult.Value.LegalTeacherAdmission.EvidenceRefs));
            Assert.Contains(
                staticResult.Value.LegalTeacherAdmission.EvidenceRefs,
                evidence => evidence.StartsWith(
                    "legal_teacher_admission:training_content_batch_ref=sha256:",
                    StringComparison.Ordinal));

            var metadata = DesignConversionDpoTrainingHandoff.BuildLegalTeacherMetadata(
                staticResult.Value.LegalTeacherAdmission);
            var serializedAdmission = JsonSerializer.Serialize(staticResult.Value.LegalTeacherAdmission);
            var serializedResult = JsonSerializer.Serialize(staticResult.Value);
            var serializedMetadata = JsonSerializer.Serialize(metadata);
            var serializedEvidence = JsonSerializer.Serialize(staticResult.Value.EvidenceRefs);
            foreach (var rawIdentity in new[] { privateRequest, privateModel, privateVersion })
            {
                Assert.DoesNotContain(rawIdentity, serializedAdmission, StringComparison.Ordinal);
                Assert.DoesNotContain(rawIdentity, serializedResult, StringComparison.Ordinal);
                Assert.DoesNotContain(rawIdentity, serializedMetadata, StringComparison.Ordinal);
                Assert.DoesNotContain(rawIdentity, serializedEvidence, StringComparison.Ordinal);
            }
            Assert.Contains("sha256:", serializedAdmission, StringComparison.Ordinal);
        }
        finally
        {
            if (Directory.Exists(checkpointDir))
                Directory.Delete(checkpointDir, recursive: true);
        }
    }

    [Fact]
    public async Task DesignConversionDpoTrainingHandoff_ExecutesLogicalDpoUnitThroughStepVector()
    {
        var tokenizer = BuildDpoHandoffTokenizer();
        var provider = new DesignConversionDpoPolicyBaseTestProvider();
        var checkpointDir = Path.Combine(Path.GetTempPath(), $"design-conversion-dpo-handoff-{Guid.NewGuid():N}");

        try
        {
            var record = BuildDpoHandoffRecord();

            var result = await DesignConversionDpoTrainingHandoff.ExecuteAsync(
                DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                [record],
                provider,
                tokenizer,
                checkpointDir,
                new LoraConfig { Rank = 2, Alpha = 4, Dropout = 0, TargetModules = ["v_proj", "q_proj", "q_proj"] },
                maxStepCount: 1);

            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.Equal(1, provider.InvocationCount);
            Assert.NotNull(provider.LastModelSet);
            Assert.Equal(1, result.Value!.RecordCount);
            Assert.Equal(1, result.Value.PreferenceCount);
            Assert.Equal(1, result.Value.DpoStepCount);
            Assert.True(result.Value.QualityGate.PassesQualityGate, result.Value.QualityGate.GateReason);
            Assert.Equal(1, result.Value.QualityGate.EvaluatedRecordCount);
            Assert.Equal(0.96, result.Value.QualityGate.PreferredScore, precision: 2);
            Assert.Equal(0.21, result.Value.QualityGate.BaselineScore, precision: 2);
            Assert.Equal(0.75, result.Value.QualityGate.ScoreDelta, precision: 2);
            Assert.Equal(0.10, result.Value.QualityGate.Threshold, precision: 2);
            Assert.Equal("local_ablation_quality_gate_passed", result.Value.QualityGate.GateReason);
            Assert.Contains(DesignConversionDpoTrainerRouteBGate.TrainerSourceSystem, result.Value.IngredientIds);
            Assert.Contains(DesignConversionDpoTrainerRouteBGate.StepVectorIngredient, result.Value.IngredientIds);
            Assert.Contains(DesignConversionDpoTrainerRouteBGate.GateId, result.Value.EvidenceRefs);
            Assert.Contains("DESIGN_CONVERSION_DPO_HANDOFF_LOCAL_ABLATION_GATE_PASSED", result.Value.EvidenceRefs);
            Assert.Contains("DESIGN_CONVERSION_DPO_HANDOFF_NO_REAL_TRAINED_WEIGHTS_CLAIM", result.Value.QualityGate.EvidenceRefs);
            Assert.Single(result.Value.MetricFilePaths);
            Assert.True(File.Exists(result.Value.MetricFilePaths[0]));
            Assert.Single(result.Value.PolicyCheckpointFilePaths);
            var policyCheckpointPath = Path.Combine(checkpointDir, DpoTrainer.BuildPolicyCheckpointFileName(1));
            Assert.Equal(Path.GetFullPath(policyCheckpointPath), Path.GetFullPath(result.Value.PolicyCheckpointFilePaths[0]));
            Assert.True(File.Exists(policyCheckpointPath), "DPO handoff must leave a loadable trained policy checkpoint, not metrics only.");
            Assert.False(File.Exists(policyCheckpointPath + ".tmp"), "DPO handoff must not leave a partial policy checkpoint temp file.");
            Assert.Equal(["q_proj", "v_proj"], result.Value.LoraConfiguration.TargetModules);
            Assert.Equal(unchecked((int)0xC14F5241), BinaryPrimitives.ReadInt32LittleEndian(File.ReadAllBytes(policyCheckpointPath).AsSpan(0, sizeof(int))));

            var freshLoadedPolicy = new LlmModel(provider.LastModelSet!.Policy.Config);
            var attached = LoraApplicator.Apply(freshLoadedPolicy, result.Value.LoraConfiguration.ToLoraConfig());
            Assert.Equal(2, attached.Count);
            freshLoadedPolicy.LoadCheckpoint(policyCheckpointPath);

            var serialized = JsonSerializer.Serialize(result.Value);
            Assert.DoesNotContain("ProductReady", serialized, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("deploy", serialized, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("upload", serialized, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("public endpoint", serialized, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("public_http_endpoint", serialized, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (Directory.Exists(checkpointDir))
                Directory.Delete(checkpointDir, recursive: true);
        }
    }

    [Fact]
    public async Task DesignConversionDpoTrainingHandoff_RejectsMaximumAboveLogicalBoundWithoutArtifacts()
    {
        var tokenizer = BuildDpoHandoffTokenizer();
        var provider = new DesignConversionDpoPolicyBaseTestProvider();
        var checkpointDir = Path.Combine(
            Path.GetTempPath(),
            $"design-conversion-dpo-invalid-maximum-{Guid.NewGuid():N}");

        var result = await DesignConversionDpoTrainingHandoff.ExecuteAsync(
            DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
            [BuildDpoHandoffRecord()],
            provider,
            tokenizer,
            checkpointDir,
            new LoraConfig
            {
                Rank = 2,
                Alpha = 4,
                Dropout = 0,
                TargetModules = ["q_proj", "v_proj"]
            },
            maxStepCount: LogicalDpoTrainingUnit.MaximumSupportedMaxStepCount + 1);

        Assert.False(result.IsSuccess);
        Assert.Equal(LogicalDpoTrainingUnit.MaxStepCountOutOfRangeErrorCode, result.ErrorCode);
        Assert.False(Directory.Exists(checkpointDir));
    }

    [Theory]
    [InlineData("after-policy")]
    [InlineData("before-runtime-commit")]
    [InlineData("before-load-gate")]
    public async Task DpoTrainingHandoff_RuntimePublicationFaultRestoresPriorPointerAndDeletesOwnedSibling(
        string faultStage)
    {
        var root = Path.Combine(Path.GetTempPath(), $"handoff-publish-fault-{Guid.NewGuid():N}");
        var trainingDirectory = Path.Combine(root, "training");
        var runtimeDirectory = Path.Combine(root, "runtime");
        var runtimePath = Path.Combine(runtimeDirectory, "runtime.json");
        var priorRuntime = Encoding.UTF8.GetBytes("prior-runtime-pointer");
        try
        {
            var record = BuildDpoHandoffRecord();
            var handoff = await DesignConversionDpoTrainingHandoff.ExecuteAsync(
                DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                [record],
                new DesignConversionDpoPolicyBaseTestProvider(),
                BuildDpoHandoffTokenizer(),
                trainingDirectory,
                new LoraConfig { Rank = 2, Alpha = 4, Dropout = 0, TargetModules = ["q_proj", "v_proj"] },
                maxStepCount: 1);
            Assert.True(handoff.IsSuccess, handoff.ErrorMessage);
            var sourcePolicyPath = Assert.Single(handoff.Value!.PolicyCheckpointFilePaths);
            var sourcePolicyBytes = await File.ReadAllBytesAsync(sourcePolicyPath);
            Directory.CreateDirectory(runtimeDirectory);
            await File.WriteAllBytesAsync(runtimePath, priorRuntime);
            bool? publicPointerHadPriorBytesAtFault = null;

            DesignConversionDpoTrainingHandoff.RuntimeTransactionStageObserverForTesting = stage =>
            {
                if (string.Equals(stage, faultStage, StringComparison.Ordinal))
                {
                    var visibleRuntime = File.ReadAllBytes(runtimePath);
                    publicPointerHadPriorBytesAtFault =
                        priorRuntime.AsSpan().SequenceEqual(visibleRuntime);
                    throw new IOException($"injected-{faultStage}");
                }
            };
            var result = await DesignConversionDpoTrainingHandoff
                .ConvertPolicyCheckpointToRuntimeCheckpointAsync(
                    DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                    record,
                    sourcePolicyPath,
                    new DesignConversionDpoPolicyBaseTestProvider(),
                    handoff.Value.LoraConfiguration,
                    runtimePath);

            Assert.False(result.IsSuccess);
            Assert.Equal("DESIGN_CONVERSION_RUNTIME_CHECKPOINT_WRITE_FAILED", result.ErrorCode);
            Assert.True(publicPointerHadPriorBytesAtFault);
            Assert.Equal(priorRuntime, await File.ReadAllBytesAsync(runtimePath));
            Assert.Equal(sourcePolicyBytes, await File.ReadAllBytesAsync(sourcePolicyPath));
            Assert.False(File.Exists(Path.Combine(runtimeDirectory, Path.GetFileName(sourcePolicyPath))));
            Assert.Empty(Directory.EnumerateFiles(runtimeDirectory, "*.tmp", SearchOption.TopDirectoryOnly));
        }
        finally
        {
            DesignConversionDpoTrainingHandoff.RuntimeTransactionStageObserverForTesting = null;
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [DesignConversionReparsePointFact]
    public async Task DpoTrainingHandoff_RuntimePublicationRejectsRuntimeLeafSymlinkWithoutTouchingTarget()
    {
        var root = Path.Combine(Path.GetTempPath(), $"handoff-runtime-leaf-link-{Guid.NewGuid():N}");
        var trainingDirectory = Path.Combine(root, "training");
        var runtimeDirectory = Path.Combine(root, "runtime");
        var runtimePath = Path.Combine(runtimeDirectory, "runtime.json");
        var sentinelPath = Path.Combine(root, "external-sentinel.json");
        var sentinel = Encoding.UTF8.GetBytes("external-runtime-sentinel");
        try
        {
            var record = BuildDpoHandoffRecord();
            var handoff = await DesignConversionDpoTrainingHandoff.ExecuteAsync(
                DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                [record], new DesignConversionDpoPolicyBaseTestProvider(), BuildDpoHandoffTokenizer(),
                trainingDirectory,
                new LoraConfig { Rank = 2, Alpha = 4, Dropout = 0, TargetModules = ["q_proj", "v_proj"] },
                maxStepCount: 1);
            Assert.True(handoff.IsSuccess, handoff.ErrorMessage);
            var sourcePolicyPath = Assert.Single(handoff.Value!.PolicyCheckpointFilePaths);
            Directory.CreateDirectory(runtimeDirectory);
            await File.WriteAllBytesAsync(sentinelPath, sentinel);
            File.CreateSymbolicLink(runtimePath, sentinelPath);

            var result = await DesignConversionDpoTrainingHandoff
                .ConvertPolicyCheckpointToRuntimeCheckpointAsync(
                    DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                    record, sourcePolicyPath, new DesignConversionDpoPolicyBaseTestProvider(),
                    handoff.Value.LoraConfiguration, runtimePath);

            Assert.False(result.IsSuccess);
            Assert.Equal("DESIGN_CONVERSION_RUNTIME_CHECKPOINT_WRITE_FAILED", result.ErrorCode);
            Assert.Equal(sentinel, await File.ReadAllBytesAsync(sentinelPath));
            Assert.False(File.Exists(Path.Combine(runtimeDirectory, Path.GetFileName(sourcePolicyPath))));
            Assert.Empty(Directory.EnumerateFiles(runtimeDirectory, "*.tmp", SearchOption.TopDirectoryOnly));
        }
        finally
        {
            if (File.Exists(runtimePath)) File.Delete(runtimePath);
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }

    [DesignConversionReparsePointFact]
    public async Task DpoTrainingHandoff_RuntimePublicationRejectsDanglingRuntimeLeafSymlinkWithoutRuntimeDestinationMutation()
    {
        var root = Path.Combine(Path.GetTempPath(), $"handoff-runtime-dangling-link-{Guid.NewGuid():N}");
        var trainingDirectory = Path.Combine(root, "training");
        var runtimeDirectory = Path.Combine(root, "runtime");
        var runtimePath = Path.Combine(runtimeDirectory, "runtime.json");
        var missingTargetPath = Path.Combine(root, "missing-runtime-target.json");
        try
        {
            var record = BuildDpoHandoffRecord();
            var handoff = await DesignConversionDpoTrainingHandoff.ExecuteAsync(
                DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                [record], new DesignConversionDpoPolicyBaseTestProvider(), BuildDpoHandoffTokenizer(),
                trainingDirectory,
                new LoraConfig { Rank = 2, Alpha = 4, Dropout = 0, TargetModules = ["q_proj", "v_proj"] },
                maxStepCount: 1);
            Assert.True(handoff.IsSuccess, handoff.ErrorMessage);
            var sourcePolicyPath = Assert.Single(handoff.Value!.PolicyCheckpointFilePaths);
            Directory.CreateDirectory(runtimeDirectory);
            File.CreateSymbolicLink(runtimePath, missingTargetPath);

            var result = await DesignConversionDpoTrainingHandoff
                .ConvertPolicyCheckpointToRuntimeCheckpointAsync(
                    DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                    record, sourcePolicyPath, new DesignConversionDpoPolicyBaseTestProvider(),
                    handoff.Value.LoraConfiguration, runtimePath);

            Assert.False(result.IsSuccess);
            Assert.Equal("DESIGN_CONVERSION_RUNTIME_CHECKPOINT_WRITE_FAILED", result.ErrorCode);
            Assert.True((File.GetAttributes(runtimePath) & FileAttributes.ReparsePoint) != 0);
            Assert.False(File.Exists(missingTargetPath));
            Assert.False(File.Exists(Path.Combine(runtimeDirectory, Path.GetFileName(sourcePolicyPath))));
            Assert.Empty(Directory.EnumerateFiles(runtimeDirectory, "*.tmp", SearchOption.TopDirectoryOnly));
        }
        finally
        {
            try { File.Delete(runtimePath); }
            catch (FileNotFoundException) { }
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }

    [DesignConversionReparsePointTheory]
    [InlineData("symlink", "DESIGN_CONVERSION_RUNTIME_PUBLICATION_STATE_UNCERTAIN")]
    [InlineData("identical-physical-replacement", "DESIGN_CONVERSION_RUNTIME_CHECKPOINT_WRITE_FAILED")]
    public async Task DpoTrainingHandoff_FinalPrecommitIdentityCheckRejectsMutatedRuntimeLeaf(
        string mutation,
        string expectedErrorCode)
    {
        var root = Path.Combine(Path.GetTempPath(), $"handoff-runtime-precommit-mutation-{mutation}-{Guid.NewGuid():N}");
        var trainingDirectory = Path.Combine(root, "training");
        var runtimeDirectory = Path.Combine(root, "runtime");
        var runtimePath = Path.Combine(runtimeDirectory, "runtime.json");
        var priorRuntime = Encoding.UTF8.GetBytes("prior-runtime-pointer");
        var foreignTargetPath = Path.Combine(root, "foreign-runtime-target.json");
        var replacementPath = Path.Combine(root, "replacement-runtime.json");
        var foreignTarget = Encoding.UTF8.GetBytes("foreign-runtime-target");
        try
        {
            var record = BuildDpoHandoffRecord();
            var handoff = await DesignConversionDpoTrainingHandoff.ExecuteAsync(
                DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                [record], new DesignConversionDpoPolicyBaseTestProvider(), BuildDpoHandoffTokenizer(),
                trainingDirectory,
                new LoraConfig { Rank = 2, Alpha = 4, Dropout = 0, TargetModules = ["q_proj", "v_proj"] },
                maxStepCount: 1);
            Assert.True(handoff.IsSuccess, handoff.ErrorMessage);
            var sourcePolicyPath = Assert.Single(handoff.Value!.PolicyCheckpointFilePaths);
            Directory.CreateDirectory(runtimeDirectory);
            await File.WriteAllBytesAsync(runtimePath, priorRuntime);
            if (mutation == "symlink")
                await File.WriteAllBytesAsync(foreignTargetPath, foreignTarget);
            else
                await File.WriteAllBytesAsync(replacementPath, priorRuntime);

            DesignConversionDpoTrainingHandoff.RuntimeTransactionStageObserverForTesting = stage =>
            {
                if (stage != "before-runtime-commit")
                    return;
                File.Delete(runtimePath);
                if (mutation == "symlink")
                    File.CreateSymbolicLink(runtimePath, foreignTargetPath);
                else
                    File.Move(replacementPath, runtimePath);
            };

            var result = await DesignConversionDpoTrainingHandoff
                .ConvertPolicyCheckpointToRuntimeCheckpointAsync(
                    DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                    record, sourcePolicyPath, new DesignConversionDpoPolicyBaseTestProvider(),
                    handoff.Value.LoraConfiguration, runtimePath);

            Assert.False(result.IsSuccess);
            Assert.Equal(expectedErrorCode, result.ErrorCode);
            if (mutation == "symlink")
            {
                Assert.True((File.GetAttributes(runtimePath) & FileAttributes.ReparsePoint) != 0);
                Assert.Equal(foreignTarget, await File.ReadAllBytesAsync(foreignTargetPath));
            }
            else
            {
                Assert.Equal(priorRuntime, await File.ReadAllBytesAsync(runtimePath));
                Assert.False(File.Exists(replacementPath));
            }
            Assert.False(File.Exists(Path.Combine(runtimeDirectory, Path.GetFileName(sourcePolicyPath))));
            Assert.Empty(Directory.EnumerateFiles(runtimeDirectory, "*.tmp", SearchOption.TopDirectoryOnly));
        }
        finally
        {
            DesignConversionDpoTrainingHandoff.RuntimeTransactionStageObserverForTesting = null;
            try { File.Delete(runtimePath); }
            catch (FileNotFoundException) { }
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }

    [Theory]
    [InlineData("policy", "DESIGN_CONVERSION_DPO_POLICY_CHECKPOINT_TOO_LARGE")]
    [InlineData("runtime", "DESIGN_CONVERSION_RUNTIME_CHECKPOINT_TOO_LARGE")]
    public async Task DpoTrainingHandoff_RuntimePublicationEnforcesPayloadBoundsBeforePointerMutation(
        string boundary,
        string expectedErrorCode)
    {
        var root = Path.Combine(Path.GetTempPath(), $"handoff-publish-bound-{boundary}-{Guid.NewGuid():N}");
        var trainingDirectory = Path.Combine(root, "training");
        var runtimeDirectory = Path.Combine(root, "runtime");
        var runtimePath = Path.Combine(runtimeDirectory, "runtime.json");
        try
        {
            var record = BuildDpoHandoffRecord();
            var handoff = await DesignConversionDpoTrainingHandoff.ExecuteAsync(
                DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                [record], new DesignConversionDpoPolicyBaseTestProvider(), BuildDpoHandoffTokenizer(),
                trainingDirectory,
                new LoraConfig { Rank = 2, Alpha = 4, Dropout = 0, TargetModules = ["q_proj", "v_proj"] },
                maxStepCount: 1);
            Assert.True(handoff.IsSuccess, handoff.ErrorMessage);
            var sourcePolicyPath = Assert.Single(handoff.Value!.PolicyCheckpointFilePaths);
            if (boundary == "policy")
                DesignConversionDpoRuntimeCheckpointPublisher.PolicyLimitForTesting = 1;
            else
                DesignConversionDpoRuntimeCheckpointPublisher.RuntimeLimitForTesting = 1;

            var result = await DesignConversionDpoTrainingHandoff
                .ConvertPolicyCheckpointToRuntimeCheckpointAsync(
                    DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                    record, sourcePolicyPath, new DesignConversionDpoPolicyBaseTestProvider(),
                    handoff.Value.LoraConfiguration, runtimePath);

            Assert.False(result.IsSuccess);
            Assert.Equal(expectedErrorCode, result.ErrorCode);
            Assert.False(File.Exists(runtimePath));
            Assert.False(File.Exists(Path.Combine(runtimeDirectory, Path.GetFileName(sourcePolicyPath))));
            if (Directory.Exists(runtimeDirectory))
                Assert.Empty(Directory.EnumerateFiles(runtimeDirectory, "*.tmp", SearchOption.TopDirectoryOnly));
        }
        finally
        {
            DesignConversionDpoRuntimeCheckpointPublisher.PolicyLimitForTesting = null;
            DesignConversionDpoRuntimeCheckpointPublisher.RuntimeLimitForTesting = null;
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task DpoTrainingHandoff_LockReleaseObserverFailureCannotReverseCommittedPublication()
    {
        var root = Path.Combine(Path.GetTempPath(), $"handoff-lock-release-{Guid.NewGuid():N}");
        var trainingDirectory = Path.Combine(root, "training");
        var runtimeDirectory = Path.Combine(root, "runtime");
        var runtimePath = Path.Combine(runtimeDirectory, "runtime.json");
        var releaseNotifications = 0;
        try
        {
            var record = BuildDpoHandoffRecord();
            var handoff = await DesignConversionDpoTrainingHandoff.ExecuteAsync(
                DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                [record], new DesignConversionDpoPolicyBaseTestProvider(), BuildDpoHandoffTokenizer(),
                trainingDirectory,
                new LoraConfig { Rank = 2, Alpha = 4, Dropout = 0, TargetModules = ["q_proj", "v_proj"] },
                maxStepCount: 1);
            Assert.True(handoff.IsSuccess, handoff.ErrorMessage);
            var sourcePolicyPath = Assert.Single(handoff.Value!.PolicyCheckpointFilePaths);
            DesignConversionDpoTrainingHandoff.RuntimeTransactionStageObserverForTesting = stage =>
            {
                if (stage == "before-lock-release")
                {
                    Interlocked.Increment(ref releaseNotifications);
                    throw new IOException("injected-release-observer-failure");
                }
            };

            var result = await DesignConversionDpoTrainingHandoff
                .ConvertPolicyCheckpointToRuntimeCheckpointAsync(
                    DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                    record, sourcePolicyPath, new DesignConversionDpoPolicyBaseTestProvider(),
                    handoff.Value.LoraConfiguration, runtimePath);

            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.Equal(1, releaseNotifications);
            var loaded = await DesignConversionMachine.LoadCheckpointAsync(runtimePath);
            Assert.True(loaded.IsSuccess, loaded.ErrorMessage);
            Assert.Empty(Directory.EnumerateFiles(runtimeDirectory, "*.tmp", SearchOption.TopDirectoryOnly));
        }
        finally
        {
            DesignConversionDpoTrainingHandoff.RuntimeTransactionStageObserverForTesting = null;
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task DpoTrainingHandoff_PostCommitFatalObserverGraphEscapesWithoutReversingPublication()
    {
        var root = Path.Combine(Path.GetTempPath(), $"handoff-post-commit-fatal-{Guid.NewGuid():N}");
        var trainingDirectory = Path.Combine(root, "training");
        var runtimeDirectory = Path.Combine(root, "runtime");
        var runtimePath = Path.Combine(runtimeDirectory, "runtime.json");
        try
        {
            var record = BuildDpoHandoffRecord();
            var handoff = await DesignConversionDpoTrainingHandoff.ExecuteAsync(
                DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                [record], new DesignConversionDpoPolicyBaseTestProvider(), BuildDpoHandoffTokenizer(),
                trainingDirectory,
                new LoraConfig { Rank = 2, Alpha = 4, Dropout = 0, TargetModules = ["q_proj", "v_proj"] },
                maxStepCount: 1);
            Assert.True(handoff.IsSuccess, handoff.ErrorMessage);
            var sourcePolicyPath = Assert.Single(handoff.Value!.PolicyCheckpointFilePaths);
            DesignConversionDpoTrainingHandoff.RuntimeTransactionStageObserverForTesting = stage =>
            {
                if (stage == "after-runtime")
                    throw new AggregateException(
                        "constructed-fatal-graph",
                        new OutOfMemoryException("constructed-test-fatal"));
            };

            await Assert.ThrowsAsync<AggregateException>(() =>
                DesignConversionDpoTrainingHandoff.ConvertPolicyCheckpointToRuntimeCheckpointAsync(
                    DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                    record, sourcePolicyPath, new DesignConversionDpoPolicyBaseTestProvider(),
                    handoff.Value.LoraConfiguration, runtimePath));

            var loaded = await DesignConversionMachine.LoadCheckpointAsync(runtimePath);
            Assert.True(loaded.IsSuccess, loaded.ErrorMessage);
            var checkpointRef = Assert.IsType<DesignConversionDpoPolicyCheckpointRef>(
                loaded.Value!.Checkpoint.DpoPolicyCheckpointRef);
            Assert.True(File.Exists(Path.Combine(runtimeDirectory, checkpointRef.PolicyCheckpointFileName)));
            Assert.Empty(Directory.EnumerateFiles(runtimeDirectory, "*.tmp", SearchOption.TopDirectoryOnly));
        }
        finally
        {
            DesignConversionDpoTrainingHandoff.RuntimeTransactionStageObserverForTesting = null;
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task DpoTrainingHandoff_MutexReleaseFalseReportsOperationalFaultWithoutReversingPublication()
    {
        var root = Path.Combine(Path.GetTempPath(), $"handoff-lock-release-false-{Guid.NewGuid():N}");
        var trainingDirectory = Path.Combine(root, "training");
        var runtimeDirectory = Path.Combine(root, "runtime");
        var runtimePath = Path.Combine(runtimeDirectory, "runtime.json");
        var stages = new List<string>();
        try
        {
            var record = BuildDpoHandoffRecord();
            var handoff = await DesignConversionDpoTrainingHandoff.ExecuteAsync(
                DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                [record], new DesignConversionDpoPolicyBaseTestProvider(), BuildDpoHandoffTokenizer(),
                trainingDirectory,
                new LoraConfig { Rank = 2, Alpha = 4, Dropout = 0, TargetModules = ["q_proj", "v_proj"] },
                maxStepCount: 1);
            Assert.True(handoff.IsSuccess, handoff.ErrorMessage);
            var sourcePolicyPath = Assert.Single(handoff.Value!.PolicyCheckpointFilePaths);
            DesignConversionDpoRuntimeCheckpointPublisher.MutexReleaseForTesting = () => false;
            DesignConversionDpoTrainingHandoff.RuntimeTransactionStageObserverForTesting = stages.Add;

            var result = await DesignConversionDpoTrainingHandoff
                .ConvertPolicyCheckpointToRuntimeCheckpointAsync(
                    DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                    record, sourcePolicyPath, new DesignConversionDpoPolicyBaseTestProvider(),
                    handoff.Value.LoraConfiguration, runtimePath);

            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.Equal(1, stages.Count(stage => stage == "mutex-release-failed"));
            var loaded = await DesignConversionMachine.LoadCheckpointAsync(runtimePath);
            Assert.True(loaded.IsSuccess, loaded.ErrorMessage);
            Assert.Empty(Directory.EnumerateFiles(runtimeDirectory, "*.tmp", SearchOption.TopDirectoryOnly));
        }
        finally
        {
            DesignConversionDpoRuntimeCheckpointPublisher.MutexReleaseForTesting = null;
            DesignConversionDpoTrainingHandoff.RuntimeTransactionStageObserverForTesting = null;
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task DpoTrainingHandoff_RuntimePublicationRejectsImmutableSiblingCollisionWithoutMutation()
    {
        var root = Path.Combine(Path.GetTempPath(), $"handoff-publish-collision-{Guid.NewGuid():N}");
        var trainingDirectory = Path.Combine(root, "training");
        var runtimeDirectory = Path.Combine(root, "runtime");
        var runtimePath = Path.Combine(runtimeDirectory, "runtime.json");
        var priorRuntime = Encoding.UTF8.GetBytes("prior-runtime-pointer");
        var foreignPolicy = new byte[] { 9, 8, 7, 6 };
        try
        {
            var record = BuildDpoHandoffRecord();
            var handoff = await DesignConversionDpoTrainingHandoff.ExecuteAsync(
                DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                [record], new DesignConversionDpoPolicyBaseTestProvider(), BuildDpoHandoffTokenizer(),
                trainingDirectory,
                new LoraConfig { Rank = 2, Alpha = 4, Dropout = 0, TargetModules = ["q_proj", "v_proj"] },
                maxStepCount: 1);
            Assert.True(handoff.IsSuccess, handoff.ErrorMessage);
            var sourcePolicyPath = Assert.Single(handoff.Value!.PolicyCheckpointFilePaths);
            Directory.CreateDirectory(runtimeDirectory);
            await File.WriteAllBytesAsync(runtimePath, priorRuntime);
            var siblingPath = Path.Combine(runtimeDirectory, Path.GetFileName(sourcePolicyPath));
            await File.WriteAllBytesAsync(siblingPath, foreignPolicy);

            var result = await DesignConversionDpoTrainingHandoff
                .ConvertPolicyCheckpointToRuntimeCheckpointAsync(
                    DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                    record, sourcePolicyPath, new DesignConversionDpoPolicyBaseTestProvider(),
                    handoff.Value.LoraConfiguration, runtimePath);

            Assert.False(result.IsSuccess);
            Assert.Equal("DESIGN_CONVERSION_DPO_POLICY_CHECKPOINT_COLLISION", result.ErrorCode);
            Assert.Equal(foreignPolicy, await File.ReadAllBytesAsync(siblingPath));
            Assert.Equal(priorRuntime, await File.ReadAllBytesAsync(runtimePath));
            Assert.Empty(Directory.EnumerateFiles(runtimeDirectory, "*.tmp", SearchOption.TopDirectoryOnly));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task DpoTrainingHandoff_RuntimePublicationSourceSiblingAliasIsNeverDeletedOnFailure()
    {
        var root = Path.Combine(Path.GetTempPath(), $"handoff-publish-alias-{Guid.NewGuid():N}");
        try
        {
            var record = BuildDpoHandoffRecord();
            var handoff = await DesignConversionDpoTrainingHandoff.ExecuteAsync(
                DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                [record], new DesignConversionDpoPolicyBaseTestProvider(), BuildDpoHandoffTokenizer(), root,
                new LoraConfig { Rank = 2, Alpha = 4, Dropout = 0, TargetModules = ["q_proj", "v_proj"] },
                maxStepCount: 1);
            Assert.True(handoff.IsSuccess, handoff.ErrorMessage);
            var sourcePolicyPath = Assert.Single(handoff.Value!.PolicyCheckpointFilePaths);
            var sourcePolicyBytes = await File.ReadAllBytesAsync(sourcePolicyPath);
            var runtimePath = Path.Combine(root, "runtime.json");
            DesignConversionDpoTrainingHandoff.RuntimeTransactionStageObserverForTesting = stage =>
            {
                if (stage == "after-policy") throw new IOException("injected-alias-failure");
            };

            var result = await DesignConversionDpoTrainingHandoff
                .ConvertPolicyCheckpointToRuntimeCheckpointAsync(
                    DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                    record, sourcePolicyPath, new DesignConversionDpoPolicyBaseTestProvider(),
                    handoff.Value.LoraConfiguration, runtimePath);

            Assert.False(result.IsSuccess);
            Assert.Equal(sourcePolicyBytes, await File.ReadAllBytesAsync(sourcePolicyPath));
            Assert.False(File.Exists(runtimePath));
            Assert.Empty(Directory.EnumerateFiles(root, "*.tmp", SearchOption.TopDirectoryOnly));
        }
        finally
        {
            DesignConversionDpoTrainingHandoff.RuntimeTransactionStageObserverForTesting = null;
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task DpoTrainingHandoff_RuntimePublicationPreservesIdenticalExistingSiblingOnInjectedFailure()
    {
        var root = Path.Combine(Path.GetTempPath(), $"handoff-publish-identical-{Guid.NewGuid():N}");
        var trainingDirectory = Path.Combine(root, "training");
        var runtimeDirectory = Path.Combine(root, "runtime");
        var runtimePath = Path.Combine(runtimeDirectory, "runtime.json");
        var priorRuntime = Encoding.UTF8.GetBytes("prior-runtime-pointer");
        try
        {
            var record = BuildDpoHandoffRecord();
            var handoff = await DesignConversionDpoTrainingHandoff.ExecuteAsync(
                DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                [record], new DesignConversionDpoPolicyBaseTestProvider(), BuildDpoHandoffTokenizer(),
                trainingDirectory,
                new LoraConfig { Rank = 2, Alpha = 4, Dropout = 0, TargetModules = ["q_proj", "v_proj"] },
                maxStepCount: 1);
            Assert.True(handoff.IsSuccess, handoff.ErrorMessage);
            var sourcePolicyPath = Assert.Single(handoff.Value!.PolicyCheckpointFilePaths);
            var first = await DesignConversionDpoTrainingHandoff
                .ConvertPolicyCheckpointToRuntimeCheckpointAsync(
                    DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                    record, sourcePolicyPath, new DesignConversionDpoPolicyBaseTestProvider(),
                    handoff.Value.LoraConfiguration, runtimePath);
            Assert.True(first.IsSuccess, first.ErrorMessage);
            var siblingPath = Path.Combine(runtimeDirectory, Path.GetFileName(sourcePolicyPath));
            var siblingBefore = await File.ReadAllBytesAsync(siblingPath);
            await File.WriteAllBytesAsync(runtimePath, priorRuntime);
            DesignConversionDpoTrainingHandoff.RuntimeTransactionStageObserverForTesting = stage =>
            {
                if (stage == "before-load-gate") throw new IOException("injected-identical-failure");
            };

            var failed = await DesignConversionDpoTrainingHandoff
                .ConvertPolicyCheckpointToRuntimeCheckpointAsync(
                    DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                    record, sourcePolicyPath, new DesignConversionDpoPolicyBaseTestProvider(),
                    handoff.Value.LoraConfiguration, runtimePath);

            Assert.False(failed.IsSuccess);
            Assert.Equal("DESIGN_CONVERSION_RUNTIME_CHECKPOINT_WRITE_FAILED", failed.ErrorCode);
            Assert.Equal(priorRuntime, await File.ReadAllBytesAsync(runtimePath));
            Assert.Equal(siblingBefore, await File.ReadAllBytesAsync(siblingPath));
            Assert.Empty(Directory.EnumerateFiles(runtimeDirectory, "*.tmp", SearchOption.TopDirectoryOnly));
        }
        finally
        {
            DesignConversionDpoTrainingHandoff.RuntimeTransactionStageObserverForTesting = null;
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task DpoTrainingHandoff_RollbackFailureReturnsPublicationStateUncertain()
    {
        var root = Path.Combine(Path.GetTempPath(), $"handoff-publish-rollback-failure-{Guid.NewGuid():N}");
        var trainingDirectory = Path.Combine(root, "training");
        var runtimeDirectory = Path.Combine(root, "runtime");
        var runtimePath = Path.Combine(runtimeDirectory, "runtime.json");
        var priorRuntime = Encoding.UTF8.GetBytes("prior-runtime-pointer");
        try
        {
            var record = BuildDpoHandoffRecord();
            var handoff = await DesignConversionDpoTrainingHandoff.ExecuteAsync(
                DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                [record], new DesignConversionDpoPolicyBaseTestProvider(), BuildDpoHandoffTokenizer(),
                trainingDirectory,
                new LoraConfig { Rank = 2, Alpha = 4, Dropout = 0, TargetModules = ["q_proj", "v_proj"] },
                maxStepCount: 1);
            Assert.True(handoff.IsSuccess, handoff.ErrorMessage);
            Directory.CreateDirectory(runtimeDirectory);
            await File.WriteAllBytesAsync(runtimePath, priorRuntime);
            var sourcePolicyPath = Assert.Single(handoff.Value!.PolicyCheckpointFilePaths);
            DesignConversionDpoTrainingHandoff.RuntimeTransactionStageObserverForTesting = stage =>
            {
                if (stage is "before-runtime-commit" or "before-runtime-rollback")
                    throw new IOException($"injected-{stage}");
            };

            var result = await DesignConversionDpoTrainingHandoff
                .ConvertPolicyCheckpointToRuntimeCheckpointAsync(
                    DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                    record, sourcePolicyPath, new DesignConversionDpoPolicyBaseTestProvider(),
                    handoff.Value.LoraConfiguration, runtimePath);

            Assert.False(result.IsSuccess);
            Assert.Equal(
                "DESIGN_CONVERSION_RUNTIME_PUBLICATION_STATE_UNCERTAIN",
                result.ErrorCode);
            Assert.DoesNotContain(root, result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(priorRuntime, await File.ReadAllBytesAsync(runtimePath));
            Assert.Empty(Directory.EnumerateFiles(runtimeDirectory, "*.tmp", SearchOption.TopDirectoryOnly));
        }
        finally
        {
            DesignConversionDpoTrainingHandoff.RuntimeTransactionStageObserverForTesting = null;
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task DpoTrainingHandoff_RuntimePublicationSerializesConcurrentWritersForSamePointer()
    {
        var root = Path.Combine(Path.GetTempPath(), $"handoff-publish-concurrent-{Guid.NewGuid():N}");
        var trainingDirectory = Path.Combine(root, "training");
        var runtimeDirectory = Path.Combine(root, "runtime");
        var runtimePath = Path.Combine(runtimeDirectory, "runtime.json");
        using var releaseFirst = new ManualResetEventSlim(false);
        var firstReached = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var secondLockAttempted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var arrivals = 0;
        var lockAttempts = 0;
        var locksAcquired = 0;
        try
        {
            var record = BuildDpoHandoffRecord();
            var handoff = await DesignConversionDpoTrainingHandoff.ExecuteAsync(
                DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                [record], new DesignConversionDpoPolicyBaseTestProvider(), BuildDpoHandoffTokenizer(),
                trainingDirectory,
                new LoraConfig { Rank = 2, Alpha = 4, Dropout = 0, TargetModules = ["q_proj", "v_proj"] },
                maxStepCount: 1);
            Assert.True(handoff.IsSuccess, handoff.ErrorMessage);
            var sourcePolicyPath = Assert.Single(handoff.Value!.PolicyCheckpointFilePaths);
            DesignConversionDpoTrainingHandoff.RuntimeTransactionStageObserverForTesting = stage =>
            {
                if (stage == "before-lock-attempt" &&
                    Interlocked.Increment(ref lockAttempts) == 2)
                    secondLockAttempted.TrySetResult();
                if (stage == "lock-acquired")
                    Interlocked.Increment(ref locksAcquired);
                if (stage == "after-policy" && Interlocked.Increment(ref arrivals) == 1)
                {
                    firstReached.TrySetResult();
                    releaseFirst.Wait(TimeSpan.FromSeconds(10));
                }
            };

            Task<OperationResult<ModelServer.DesignConversion.DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>> StartWriter() =>
                Task.Run(() => DesignConversionDpoTrainingHandoff
                    .ConvertPolicyCheckpointToRuntimeCheckpointAsync(
                        DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                        record, sourcePolicyPath, new DesignConversionDpoPolicyBaseTestProvider(),
                        handoff.Value.LoraConfiguration, runtimePath));
            var first = StartWriter();
            await firstReached.Task.WaitAsync(TimeSpan.FromSeconds(10));
            var second = StartWriter();
            await secondLockAttempted.Task.WaitAsync(TimeSpan.FromSeconds(10));
            Assert.Equal(1, Volatile.Read(ref arrivals));
            Assert.Equal(1, Volatile.Read(ref locksAcquired));
            Assert.False(second.IsCompleted);
            releaseFirst.Set();
            var results = await Task.WhenAll(first, second).WaitAsync(TimeSpan.FromSeconds(20));

            Assert.All(results, result => Assert.True(result.IsSuccess, result.ErrorMessage));
            Assert.Equal(2, arrivals);
            Assert.Equal(2, locksAcquired);
            Assert.True(File.Exists(runtimePath));
            var loaded = await DesignConversionMachine.LoadCheckpointAsync(runtimePath);
            Assert.True(loaded.IsSuccess, loaded.ErrorMessage);
            var checkpointRef = Assert.IsType<DesignConversionDpoPolicyCheckpointRef>(
                loaded.Value!.Checkpoint.DpoPolicyCheckpointRef);
            var siblingPath = Path.Combine(runtimeDirectory, checkpointRef.PolicyCheckpointFileName);
            var siblingBytes = await File.ReadAllBytesAsync(siblingPath);
            Assert.Equal(checkpointRef.PolicyCheckpointByteLength, siblingBytes.LongLength);
            Assert.Equal(
                checkpointRef.Sha256,
                Convert.ToHexString(SHA256.HashData(siblingBytes)).ToLowerInvariant());
            var gate = DesignConversionDpoTrainerRouteBGate.ValidateTypedRuntimeCheckpoint(
                checkpointRef, 1, loaded.Value.Checkpoint.EvidenceRefs, callerFreshLoadVerified: true);
            Assert.True(gate.IsSuccess, gate.ErrorMessage);
            Assert.Empty(Directory.EnumerateFiles(runtimeDirectory, "*.tmp", SearchOption.TopDirectoryOnly));
        }
        finally
        {
            releaseFirst.Set();
            DesignConversionDpoTrainingHandoff.RuntimeTransactionStageObserverForTesting = null;
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task DpoTrainerPolicyCheckpointBridge_FreshLoadsConvertedCheckpointIntoRuntimeScoring()
    {
        const string bridgeChosenHtml = "<main data-bridge-side=\"alpha\">bridge-alpha</main>";
        const string bridgeChosenCss = "[data-bridge-side=alpha]{display:grid;gap:23px}";
        const string bridgeRejectedHtml = "<main data-bridge-side=\"beta\">bridge-beta</main>";
        const string bridgeRejectedCss = "[data-bridge-side=beta]{display:block;gap:2px}";
        var tokenizer = BuildDpoHandoffTokenizer();
        var checkpointDir = Path.Combine(Path.GetTempPath(), $"design-conversion-dpo-policy-bridge-{Guid.NewGuid():N}");
        var runtimeCheckpointPath = Path.Combine(checkpointDir, "design-conversion-runtime-from-dpo-policy.json");
        LearningBatch<global::DesignConversionDomain.DesignConversionDomain>? capturedRuntimeOptimizerBatch = null;

        try
        {
            var sourceRecord = BuildDpoHandoffRecord();
            var record = sourceRecord with
            {
                Chosen = sourceRecord.Chosen with
                {
                    OutputId = "BRIDGE_OUTCOME_ONLY_CHOSEN_OUTPUT_ID",
                    CandidateId = "BRIDGE_OUTCOME_ONLY_CHOSEN_CANDIDATE_ID",
                    Html = bridgeChosenHtml,
                    Css = bridgeChosenCss,
                    EvidenceRefs = ["BRIDGE_OUTCOME_ONLY_CHOSEN_EVIDENCE"]
                },
                Rejected = sourceRecord.Rejected with
                {
                    OutputId = "BRIDGE_OUTCOME_ONLY_REJECTED_OUTPUT_ID",
                    CandidateId = "BRIDGE_OUTCOME_ONLY_REJECTED_CANDIDATE_ID",
                    Html = bridgeRejectedHtml,
                    Css = bridgeRejectedCss,
                    EvidenceRefs = ["BRIDGE_OUTCOME_ONLY_REJECTED_EVIDENCE"]
                }
            };
            var runtimeInput = DesignConversionTestFixtures.CreateInput(comparisonScore: 0.40, comparisonPassed: false);
            var baselineProbe = await RunMachineScoreAsync(
                new DesignConversionMachine(),
                runtimeInput,
                "dpo-policy-bridge-baseline-runtime");
            Assert.Equal(0.40, baselineProbe.Score, precision: 2);
            Assert.False(baselineProbe.Passed);

            var modelServerHandoffProvider = new DesignConversionDpoPolicyBaseTestProvider();
            var handoff = await DesignConversionDpoTrainingHandoff.ExecuteAsync(
                DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                [record],
                modelServerHandoffProvider,
                tokenizer,
                checkpointDir,
                new LoraConfig { Rank = 2, Alpha = 4, Dropout = 0, TargetModules = ["q_proj", "v_proj"] },
                maxStepCount: 1);
            Assert.True(handoff.IsSuccess, handoff.ErrorMessage);
            Assert.Equal(1, modelServerHandoffProvider.InvocationCount);
            Assert.NotNull(modelServerHandoffProvider.LastModelSet);
            AssertCurrentSeededDpoModelSet(
                modelServerHandoffProvider.LastModelSet,
                DesignConversionDpoPolicyModelRole.Policy,
                DesignConversionDpoPolicyModelRole.Reference);
            var policyCheckpointPath = Assert.Single(handoff.Value!.PolicyCheckpointFilePaths);
            Assert.True(File.Exists(policyCheckpointPath));

            var modelServerConvertProvider = new DesignConversionDpoPolicyBaseTestProvider();
            DesignConversionDpoTrainingHandoff.RuntimeOptimizerBatchObserverForTesting = batch =>
                capturedRuntimeOptimizerBatch = batch;
            var bridge = await DesignConversionDpoTrainingHandoff.ConvertPolicyCheckpointToRuntimeCheckpointAsync(
                DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                record,
                policyCheckpointPath,
                modelServerConvertProvider,
                handoff.Value.LoraConfiguration,
                runtimeCheckpointPath);
            Assert.True(bridge.IsSuccess, bridge.ErrorMessage);
            var runtimeOptimizerItem = Assert.Single(
                Assert.IsType<LearningBatch<global::DesignConversionDomain.DesignConversionDomain>>(
                    capturedRuntimeOptimizerBatch).TrainingItems);
            using var runtimeOptimizerPrompt = JsonDocument.Parse(runtimeOptimizerItem.Prompt);
            using var runtimeOptimizerChosen = JsonDocument.Parse(runtimeOptimizerItem.ChosenResponse);
            using var runtimeOptimizerRejected = JsonDocument.Parse(runtimeOptimizerItem.RejectedResponse);
            Assert.Equal(
                ["html", "css"],
                runtimeOptimizerChosen.RootElement.EnumerateObject().Select(property => property.Name).ToArray());
            Assert.Equal(
                ["html", "css"],
                runtimeOptimizerRejected.RootElement.EnumerateObject().Select(property => property.Name).ToArray());
            Assert.Equal(bridgeChosenHtml, runtimeOptimizerChosen.RootElement.GetProperty("html").GetString());
            Assert.Equal(bridgeChosenCss, runtimeOptimizerChosen.RootElement.GetProperty("css").GetString());
            Assert.Equal(bridgeRejectedHtml, runtimeOptimizerRejected.RootElement.GetProperty("html").GetString());
            Assert.Equal(bridgeRejectedCss, runtimeOptimizerRejected.RootElement.GetProperty("css").GetString());
            Assert.DoesNotContain(bridgeRejectedHtml, runtimeOptimizerItem.ChosenResponse, StringComparison.Ordinal);
            Assert.DoesNotContain(bridgeChosenHtml, runtimeOptimizerItem.RejectedResponse, StringComparison.Ordinal);
            var runtimeOptimizerText = string.Join(
                '\n',
                runtimeOptimizerItem.Prompt,
                runtimeOptimizerItem.ChosenResponse,
                runtimeOptimizerItem.RejectedResponse);
            foreach (var forbidden in new[]
                     {
                         "BRIDGE_OUTCOME_ONLY_CHOSEN_OUTPUT_ID", "BRIDGE_OUTCOME_ONLY_REJECTED_OUTPUT_ID",
                         "BRIDGE_OUTCOME_ONLY_CHOSEN_CANDIDATE_ID", "BRIDGE_OUTCOME_ONLY_REJECTED_CANDIDATE_ID",
                         "BRIDGE_OUTCOME_ONLY_CHOSEN_EVIDENCE", "BRIDGE_OUTCOME_ONLY_REJECTED_EVIDENCE",
                         "pass153-chosen-verdict", "pass153-rejected-verdict", "visual_hierarchy",
                         "local-test-reviewer", "usefulness", "accepted", "passed", "score", "verdict",
                         "candidate_id", "output_id", "qa_result", "evidence_refs",
                         Path.GetFileName(policyCheckpointPath), bridge.Value!.PolicyCheckpointSha256
                     })
            {
                Assert.DoesNotContain(forbidden, runtimeOptimizerText, StringComparison.OrdinalIgnoreCase);
            }
            Assert.Equal("figma", runtimeOptimizerPrompt.RootElement.GetProperty("source").GetProperty("kind").GetString());
            Assert.Contains("FRAME", runtimeOptimizerPrompt.RootElement.GetProperty("source").GetProperty("structure_payload").GetString(), StringComparison.Ordinal);
            Assert.Equal(
                ["domain_tag", "unit_id", "source_kind", "internal_vector", "target", "source", "source_encoder"],
                runtimeOptimizerPrompt.RootElement.EnumerateObject().Select(property => property.Name).ToArray());
            Assert.False(runtimeOptimizerPrompt.RootElement.GetProperty("source").TryGetProperty("manifest_hash", out _));
            Assert.False(runtimeOptimizerPrompt.RootElement.GetProperty("source_encoder").TryGetProperty("vector_hash", out _));
            Assert.Contains(
                $"dpo_policy_sha256:{bridge.Value.PolicyCheckpointSha256}",
                capturedRuntimeOptimizerBatch!.SourceIds);
            Assert.Contains(
                capturedRuntimeOptimizerBatch.SourceIds,
                sourceId => sourceId.StartsWith("dpo_policy_file:", StringComparison.Ordinal));
            Assert.Contains(
                "design_conversion_dpo_record:pass153-dpo-record",
                capturedRuntimeOptimizerBatch.SourceIds);
            Assert.Equal(1, modelServerConvertProvider.InvocationCount);
            Assert.NotNull(modelServerConvertProvider.LastModelSet);
            var modelServerProviderModelSet = modelServerConvertProvider.LastModelSet!;
            AssertCurrentSeededDpoModelSet(
                modelServerProviderModelSet,
                DesignConversionDpoPolicyModelRole.FreshLoad);
            var modelServerExpectedRandomContext = DesignConversionDpoPolicyBaseTestProvider
                .CreateCurrentRandomContext();
            var modelServerExpectedConfigSha256 =
                DesignConversionDpoPolicyBaseMetadata.ComputeConfigSha256(modelServerProviderModelSet.Policy.Config);
            var modelServerExpectedFreshLoadModel = new LlmModel(
                DesignConversionDpoPolicyBaseMetadata.CopyConfig(modelServerProviderModelSet.Reference.Config));
            DesignConversionDpoPolicyBaseTestProvider.CopyParameters(
                modelServerProviderModelSet.Reference,
                modelServerExpectedFreshLoadModel);
            var modelServerBareExpectedInitialWeightSha256 =
                ComputeExpectedDpoInitialWeightSha256(modelServerExpectedFreshLoadModel);
            var modelServerExpectedFreshLoadAdapters =
                DesignConversionDpoPolicyBaseMetadata.AttachSeededCanonicalLoraTopology(
                    modelServerExpectedFreshLoadModel,
                    modelServerExpectedRandomContext,
                    DesignConversionDpoPolicyModelRole.FreshLoad);
            Assert.NotEmpty(modelServerExpectedFreshLoadAdapters);
            var modelServerExpectedInitialWeightSha256 =
                ComputeExpectedDpoInitialWeightSha256(modelServerExpectedFreshLoadModel);
            Assert.False(ReferenceEquals(modelServerProviderModelSet.Policy, modelServerProviderModelSet.Reference));
            Assert.False(ReferenceEquals(modelServerProviderModelSet.Policy, modelServerProviderModelSet.FreshLoadTarget));
            Assert.False(ReferenceEquals(modelServerProviderModelSet.Reference, modelServerProviderModelSet.FreshLoadTarget));
            Assert.True(bridge.Value!.PolicyCheckpointFreshLoadVerified);
            Assert.Equal(handoff.Value.LoraConfiguration, bridge.Value.LoraConfiguration);
            Assert.True(File.Exists(runtimeCheckpointPath));
            Assert.Contains(DesignConversionMachine.DpoTrainerPolicyCheckpointFreshLoadedEvidence, bridge.Value.EvidenceRefs);
            Assert.Contains(DesignConversionMachine.DpoTrainerPolicyCheckpointConvertedEvidence, bridge.Value.EvidenceRefs);
            var modelServerTypedRef = bridge.Value.RuntimeCheckpoint.DpoPolicyCheckpointRef;
            Assert.NotNull(modelServerTypedRef);
            Assert.NotNull(bridge.Value.PolicyCheckpointRef);
            Assert.Same(modelServerTypedRef, bridge.Value.PolicyCheckpointRef);
            var modelServerTypedRefValue = modelServerTypedRef!;
            var policyCheckpointFileName = Path.GetFileName(policyCheckpointPath);
            var modelServerRuntimeCheckpointDirectory = Path.GetDirectoryName(runtimeCheckpointPath)!;
            var modelServerSiblingPolicyCheckpointPath = Path.Combine(
                modelServerRuntimeCheckpointDirectory,
                modelServerTypedRefValue.PolicyCheckpointFileName);
            Assert.Equal(policyCheckpointFileName, modelServerTypedRefValue.PolicyCheckpointFileName);
            Assert.Equal(policyCheckpointFileName, bridge.Value.PolicyCheckpointPath);
            Assert.False(Path.IsPathRooted(bridge.Value.PolicyCheckpointPath));
            Assert.Equal(Path.GetFileName(bridge.Value.PolicyCheckpointPath), bridge.Value.PolicyCheckpointPath);
            Assert.True(File.Exists(modelServerSiblingPolicyCheckpointPath));
            var modelServerPayloadBytes = await File.ReadAllBytesAsync(modelServerSiblingPolicyCheckpointPath);
            Assert.Equal(await File.ReadAllBytesAsync(policyCheckpointPath), modelServerPayloadBytes);
            Assert.Equal(bridge.Value.PolicyCheckpointSha256, modelServerTypedRefValue.Sha256);
            Assert.Equal(DesignConversionDpoTrainerRouteBGate.TrainerSourceSystem, modelServerTypedRefValue.SourceSystem);
            Assert.Equal(DesignConversionDpoTrainerRouteBGate.FreshLoadedSourceMechanism, modelServerTypedRefValue.SourceMechanism);
            Assert.Equal(
                DesignConversionMachine.DpoPolicyDirectInferenceRuntimeBridgeKind,
                modelServerTypedRefValue.RuntimeBridgeKind);
            Assert.True(modelServerTypedRefValue.FreshLoadVerified);
            Assert.True(modelServerTypedRefValue.ConvertedToRuntimeWeights);
            Assert.Equal(1, modelServerTypedRefValue.SourceRecordCount);
            Assert.Equal(1, modelServerTypedRefValue.BridgePreferenceCount);
            Assert.Equal(1, modelServerTypedRefValue.DpoStepCount);
            var modelServerProviderConfig = DesignConversionDpoPolicyBaseTestProvider.CreateConfig();
            Assert.Equal(modelServerProviderModelSet.Identity.ConfigurationKind, modelServerTypedRefValue.TrainingContext.ConfigKind);
            Assert.DoesNotContain("freshPolicyModel.Config", modelServerTypedRefValue.TrainingContext.ConfigKind, StringComparison.Ordinal);
            Assert.DoesNotContain("CreateTinyPolicyConfig", modelServerTypedRefValue.TrainingContext.ConfigKind, StringComparison.Ordinal);
            Assert.Equal(modelServerProviderConfig.VocabSize, modelServerTypedRefValue.TrainingContext.VocabSize);
            Assert.Equal(modelServerProviderConfig.ContextLen, modelServerTypedRefValue.TrainingContext.ContextLength);
            Assert.Equal(modelServerProviderConfig.HiddenDim, modelServerTypedRefValue.TrainingContext.HiddenDim);
            Assert.Equal(modelServerProviderConfig.NumLayers, modelServerTypedRefValue.TrainingContext.NumLayers);
            Assert.Equal(modelServerProviderConfig.NumHeads, modelServerTypedRefValue.TrainingContext.NumHeads);
            Assert.Equal(modelServerProviderConfig.NumKvHeads, modelServerTypedRefValue.TrainingContext.NumKvHeads);
            Assert.Equal(modelServerProviderConfig.FfnHiddenDim, modelServerTypedRefValue.TrainingContext.FfnHiddenDim);
            Assert.Equal(modelServerProviderConfig.RopeTheta, modelServerTypedRefValue.TrainingContext.RopeTheta);
            Assert.Equal(modelServerProviderConfig.RmsNormEps, modelServerTypedRefValue.TrainingContext.RmsNormEps);
            Assert.Equal(4, modelServerTypedRefValue.TrainingContext.SchemaVersion);
            Assert.Equal(
                modelServerExpectedRandomContext,
                modelServerTypedRefValue.TrainingContext.RandomContext);
            Assert.True(DesignConversionDpoPolicyRandomContextContract.HasRequiredEvidenceRefs(
                modelServerExpectedRandomContext,
                modelServerTypedRefValue.EvidenceRefs));
            Assert.All(
                DesignConversionDpoPolicyRandomContextContract.BuildRequiredEvidenceRefs(
                    modelServerExpectedRandomContext),
                required => Assert.Contains(required, modelServerTypedRefValue.EvidenceRefs));
            Assert.True(DesignConversionDpoLegalTeacherAdmissionCommitmentCodec.Validate(
                modelServerTypedRefValue.TrainingContext.LegalTeacherAdmissionCommitment));
            Assert.True(DesignConversionDpoLegalTeacherAdmissionCommitmentCodec.MatchesCheckpointEvidence(
                modelServerTypedRefValue.TrainingContext.LegalTeacherAdmissionCommitment,
                modelServerTypedRefValue.EvidenceRefs));
            Assert.Equal(modelServerProviderModelSet.Identity.ProviderKind, modelServerTypedRefValue.TrainingContext.ProviderKind);
            Assert.Equal(modelServerProviderModelSet.Identity.BaseModelId, modelServerTypedRefValue.TrainingContext.BaseModelId);
            Assert.Equal(modelServerProviderModelSet.Identity.BaseRevision, modelServerTypedRefValue.TrainingContext.BaseRevision);
            Assert.Equal(modelServerProviderModelSet.Identity.BaseCheckpointSha256, modelServerTypedRefValue.TrainingContext.BaseCheckpointSha256);
            Assert.Equal(modelServerExpectedConfigSha256, modelServerTypedRefValue.TrainingContext.ConfigSha256);
            Assert.NotEqual(
                modelServerBareExpectedInitialWeightSha256,
                modelServerExpectedInitialWeightSha256);
            Assert.Equal(modelServerExpectedInitialWeightSha256, modelServerTypedRefValue.TrainingContext.InitialWeightSha256);
            Assert.True(modelServerTypedRefValue.TrainingContext.RawPayloadPersisted);
            Assert.False(modelServerTypedRefValue.TrainingContext.AbsolutePathPersisted);
            var modelServerLora = modelServerTypedRefValue.TrainingContext.Lora;
            Assert.NotNull(modelServerLora);
            Assert.Equal(2, modelServerLora!.Rank);
            Assert.Equal(4, modelServerLora.Alpha);
            Assert.Equal(0, modelServerLora.Dropout);
            Assert.Equal(["q_proj", "v_proj"], modelServerLora.TargetModules);
            Assert.Equal(modelServerProviderConfig.NumLayers * modelServerLora.TargetModules.Count, modelServerLora.AdapterCount);
            Assert.Equal(modelServerExpectedFreshLoadAdapters.Count, modelServerLora.AdapterCount);
            Assert.Equal(modelServerPayloadBytes.Length, modelServerTypedRefValue.PolicyCheckpointByteLength);
            Assert.Equal(
                bridge.Value.PolicyCheckpointSha256,
                Convert.ToHexString(SHA256.HashData(modelServerPayloadBytes)).ToLowerInvariant());
            Assert.Contains(DesignConversionMachine.DpoTrainerPolicyCheckpointFreshLoadedEvidence, modelServerTypedRefValue.EvidenceRefs);
            Assert.Contains(DesignConversionMachine.DpoTrainerPolicyCheckpointConvertedEvidence, modelServerTypedRefValue.EvidenceRefs);
            Assert.Contains(DesignConversionDpoTrainerRouteBGate.GateId, bridge.Value.EvidenceRefs);
            Assert.Contains(DesignConversionDpoTrainerRouteBGate.GateId, modelServerTypedRefValue.EvidenceRefs);
            Assert.Contains(DesignConversionMachine.DpoPolicyDirectInferenceConfiguredEvidence, modelServerTypedRefValue.EvidenceRefs);
            Assert.Contains(DesignConversionMachine.DpoPolicyLoraAppliedEvidence, modelServerTypedRefValue.EvidenceRefs);
            Assert.Contains("10.LoRA:LoraApplicator", modelServerTypedRefValue.EvidenceRefs);
            Assert.Contains($"dpo_policy_lora_rank:{modelServerLora.Rank}", modelServerTypedRefValue.EvidenceRefs);
            Assert.Contains($"dpo_policy_lora_alpha:{modelServerLora.Alpha}", modelServerTypedRefValue.EvidenceRefs);
            Assert.Contains($"dpo_policy_lora_dropout:{modelServerLora.Dropout}", modelServerTypedRefValue.EvidenceRefs);
            Assert.Contains($"dpo_policy_lora_target_modules:{string.Join(",", modelServerLora.TargetModules)}", modelServerTypedRefValue.EvidenceRefs);
            Assert.Contains($"dpo_policy_lora_adapter_count:{modelServerLora.AdapterCount}", modelServerTypedRefValue.EvidenceRefs);
            Assert.Contains("DESIGN_CONVERSION_DPO_POLICY_CHECKPOINT_REF_TYPED", modelServerTypedRefValue.EvidenceRefs);
            Assert.Contains("DESIGN_CONVERSION_DPO_POLICY_CHECKPOINT_REF_NO_ABSOLUTE_PATH", modelServerTypedRefValue.EvidenceRefs);
            Assert.Contains("DESIGN_CONVERSION_DPO_POLICY_CHECKPOINT_REF_PAYLOAD_FILE_SIBLING", modelServerTypedRefValue.EvidenceRefs);
            Assert.DoesNotContain("DESIGN_CONVERSION_DPO_POLICY_CHECKPOINT_REF_RAW_PAYLOAD_EMBEDDED", modelServerTypedRefValue.EvidenceRefs);
            Assert.Contains("DESIGN_CONVERSION_DPO_POLICY_BASE_PROVIDER_INJECTED", modelServerTypedRefValue.EvidenceRefs);
            Assert.Contains("DESIGN_CONVERSION_DPO_POLICY_BASE_IDENTITY_VALIDATED", modelServerTypedRefValue.EvidenceRefs);
            Assert.Contains($"dpo_policy_base_provider_kind:{modelServerProviderModelSet.Identity.ProviderKind}", modelServerTypedRefValue.EvidenceRefs);
            Assert.Contains($"dpo_policy_base_configuration_kind:{modelServerProviderModelSet.Identity.ConfigurationKind}", modelServerTypedRefValue.EvidenceRefs);
            Assert.Contains($"dpo_policy_base_model_id:{modelServerProviderModelSet.Identity.BaseModelId}", modelServerTypedRefValue.EvidenceRefs);
            Assert.Contains($"dpo_policy_base_revision:{modelServerProviderModelSet.Identity.BaseRevision}", modelServerTypedRefValue.EvidenceRefs);
            Assert.Contains($"dpo_policy_base_checkpoint_sha256:{modelServerProviderModelSet.Identity.BaseCheckpointSha256}", modelServerTypedRefValue.EvidenceRefs);
            Assert.Contains($"dpo_policy_config_sha256:{modelServerExpectedConfigSha256}", modelServerTypedRefValue.EvidenceRefs);
            Assert.Contains($"dpo_policy_initial_weight_sha256:{modelServerExpectedInitialWeightSha256}", modelServerTypedRefValue.EvidenceRefs);
            Assert.Contains(bridge.Value.RuntimeCheckpoint.CriticUnit.LearnedWeights ?? [], weight =>
                weight.EvidenceRefs.Contains(DesignConversionMachine.DpoTrainerPolicyCheckpointFreshLoadedEvidence, StringComparer.Ordinal) &&
                weight.EvidenceRefs.Contains(DesignConversionMachine.DpoTrainerPolicyCheckpointConvertedEvidence, StringComparer.Ordinal) &&
                weight.EvidenceRefs.Contains($"dpo_policy_checkpoint_sha256:{bridge.Value.PolicyCheckpointSha256}", StringComparer.Ordinal));
            var freshLoadTargetAfterBridge = modelServerProviderModelSet.FreshLoadTarget;
            var parameterCountAfterBridge = freshLoadTargetAfterBridge.Parameters().Count();
            var firstReapply = LoraApplicator.Apply(freshLoadTargetAfterBridge, handoff.Value.LoraConfiguration.ToLoraConfig());
            var secondReapply = LoraApplicator.Apply(freshLoadTargetAfterBridge, handoff.Value.LoraConfiguration.ToLoraConfig());
            Assert.Equal(parameterCountAfterBridge, freshLoadTargetAfterBridge.Parameters().Count());
            Assert.True(firstReapply.Zip(secondReapply).All(pair => ReferenceEquals(pair.First, pair.Second)));

            var loaded = await DesignConversionMachine.LoadCheckpointAsync(runtimeCheckpointPath);
            Assert.True(loaded.IsSuccess, loaded.ErrorMessage);
            var modelServerJson = await File.ReadAllTextAsync(runtimeCheckpointPath);
            using var modelServerJsonDocument = JsonDocument.Parse(modelServerJson);
            var modelServerTypedRefJson = modelServerJsonDocument.RootElement.GetProperty("dpoPolicyCheckpointRef");
            Assert.Equal(modelServerTypedRefValue.PolicyCheckpointFileName, modelServerTypedRefJson.GetProperty("policyCheckpointFileName").GetString());
            Assert.Equal(modelServerTypedRefValue.Sha256, modelServerTypedRefJson.GetProperty("sha256").GetString());
            Assert.Equal(
                modelServerTypedRefValue.PolicyCheckpointByteLength,
                modelServerTypedRefJson.GetProperty("policyCheckpointByteLength").GetInt32());
            var modelServerTypedRefTrainingContextJson = modelServerTypedRefJson.GetProperty("trainingContext");
            AssertCurrentRandomContextJson(
                modelServerTypedRefTrainingContextJson.GetProperty("randomContext"),
                modelServerExpectedRandomContext);
            var modelServerTypedRefLoraJson = modelServerTypedRefTrainingContextJson.GetProperty("lora");
            Assert.Equal(modelServerLora.Rank, modelServerTypedRefLoraJson.GetProperty("rank").GetInt32());
            Assert.Equal(modelServerLora.Alpha, modelServerTypedRefLoraJson.GetProperty("alpha").GetSingle());
            Assert.Equal(modelServerLora.Dropout, modelServerTypedRefLoraJson.GetProperty("dropout").GetSingle());
            Assert.Equal(
                modelServerLora.TargetModules,
                modelServerTypedRefLoraJson.GetProperty("targetModules").EnumerateArray().Select(target => target.GetString() ?? string.Empty).ToArray());
            Assert.Equal(modelServerLora.AdapterCount, modelServerTypedRefLoraJson.GetProperty("adapterCount").GetInt32());
            Assert.Contains(
                modelServerTypedRefJson.GetProperty("evidenceRefs").EnumerateArray(),
                evidence => string.Equals(
                    evidence.GetString(),
                    "DESIGN_CONVERSION_DPO_POLICY_CHECKPOINT_REF_PAYLOAD_FILE_SIBLING",
                    StringComparison.Ordinal));
            Assert.False(modelServerTypedRefJson.TryGetProperty("rawPayloadBase64", out _));
            Assert.False(modelServerTypedRefJson.TryGetProperty("rawPayloadByteLength", out _));
            Assert.False(modelServerTypedRefJson.TryGetProperty("policyCheckpointPath", out _));
            var modelServerPayloadBase64 = Convert.ToBase64String(modelServerPayloadBytes);
            var normalizedModelServerJson = modelServerJson.Replace("\\\\", "\\");
            Assert.DoesNotContain(modelServerPayloadBase64, modelServerJson, StringComparison.Ordinal);
            Assert.DoesNotContain(policyCheckpointPath, normalizedModelServerJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(Path.GetFullPath(modelServerSiblingPolicyCheckpointPath), normalizedModelServerJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(Path.GetFullPath(checkpointDir), normalizedModelServerJson, StringComparison.OrdinalIgnoreCase);
            Assert.NotNull(loaded.Value!.Checkpoint.DpoPolicyCheckpointRef);
            Assert.Equal(modelServerTypedRefValue.PolicyCheckpointFileName, loaded.Value.Checkpoint.DpoPolicyCheckpointRef!.PolicyCheckpointFileName);
            Assert.Equal(modelServerTypedRefValue.Sha256, loaded.Value.Checkpoint.DpoPolicyCheckpointRef.Sha256);
            Assert.Equal(modelServerTypedRefValue.SourceMechanism, loaded.Value.Checkpoint.DpoPolicyCheckpointRef.SourceMechanism);
            Assert.Equal(modelServerTypedRefValue.BridgePreferenceCount, loaded.Value.Checkpoint.DpoPolicyCheckpointRef.BridgePreferenceCount);
            Assert.Equal(modelServerTypedRefValue.TrainingContext.ConfigKind, loaded.Value.Checkpoint.DpoPolicyCheckpointRef.TrainingContext.ConfigKind);
            Assert.Equal(
                modelServerExpectedRandomContext,
                loaded.Value.Checkpoint.DpoPolicyCheckpointRef.TrainingContext.RandomContext);
            var loadedModelServerLora = loaded.Value.Checkpoint.DpoPolicyCheckpointRef.TrainingContext.Lora;
            Assert.NotNull(loadedModelServerLora);
            Assert.Equal(modelServerLora.Rank, loadedModelServerLora!.Rank);
            Assert.Equal(modelServerLora.Alpha, loadedModelServerLora.Alpha);
            Assert.Equal(modelServerLora.Dropout, loadedModelServerLora.Dropout);
            Assert.Equal(modelServerLora.TargetModules, loadedModelServerLora.TargetModules);
            Assert.Equal(modelServerLora.AdapterCount, loadedModelServerLora.AdapterCount);
            Assert.Equal(modelServerTypedRefValue.PolicyCheckpointByteLength, loaded.Value.Checkpoint.DpoPolicyCheckpointRef.PolicyCheckpointByteLength);
            var freshLoadedProbe = await RunMachineScoreAsync(
                loaded.Value!.Machine,
                runtimeInput,
                "dpo-policy-bridge-fresh-loaded-runtime");

            AssertDirectPolicyConfiguredPairUnavailableProbe(
                freshLoadedProbe,
                baselineProbe,
                runtimeInput.Request.Html,
                runtimeInput.Request.Css,
                runtimeInput.Request.DesignTreePayload.Content,
                runtimeInput.Request.OriginalTreePayload.Content,
                checkpointDir,
                runtimeCheckpointPath,
                policyCheckpointPath,
                modelServerSiblingPolicyCheckpointPath);
            Assert.Contains(DesignConversionMachine.DpoPolicyLoraAppliedEvidence, freshLoadedProbe.EvidenceRefs);
            Assert.Contains(DesignConversionMachine.DpoPolicyRuntimeLoraTopologyEvidence, freshLoadedProbe.EvidenceRefs);
            Assert.False(
                freshLoadedProbe.Metrics.TryGetValue("local_checkpoint_feature_adjustment_applied", out var freshLoadedLocalAdjustment) &&
                string.Equals(freshLoadedLocalAdjustment, bool.TrueString, StringComparison.Ordinal));

            var domainRuntimeCheckpointPath = Path.Combine(checkpointDir, "design-conversion-domain-runtime-from-dpo-policy.json");
            var domainPolicyBaseProvider = new DesignConversionDpoPolicyBaseTestProvider();
            const int domainExpectedConvergedStepCount = 3;
            var domainBridgeOptions = new DesignConversionDpoPolicyCheckpointBridgeOptions(
                MaxBridgePreferenceCount: 1,
                DpoStepCount: DesignConversionDpoPolicyCheckpointBridgeOptions.DefaultDpoStepCount)
            {
                // One initial observation plus two stable comparisons is the convergence contract.
                DpoConvergenceThreshold = float.MaxValue,
                DpoConvergenceWindowSteps = 1
            };
            var domainBridge = await DesignConversionDpoPolicyCheckpointBridge.ExecuteAndConvertAsync(
                DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                [record],
                checkpointDir,
                "pass153-domain-dpo-policy-ref",
                domainRuntimeCheckpointPath,
                domainPolicyBaseProvider,
                domainBridgeOptions);
            Assert.True(domainBridge.IsSuccess, domainBridge.ErrorMessage);
            Assert.Equal(1, domainPolicyBaseProvider.InvocationCount);
            Assert.NotNull(domainPolicyBaseProvider.LastModelSet);
            var providerModelSet = domainPolicyBaseProvider.LastModelSet!;
            AssertCurrentSeededDpoModelSet(
                providerModelSet,
                DesignConversionDpoPolicyModelRole.Policy,
                DesignConversionDpoPolicyModelRole.Reference,
                DesignConversionDpoPolicyModelRole.FreshLoad);
            var domainExpectedRandomContext = DesignConversionDpoPolicyBaseTestProvider
                .CreateCurrentRandomContext();
            var domainExpectedConfigSha256 =
                DesignConversionDpoPolicyBaseMetadata.ComputeConfigSha256(providerModelSet.Policy.Config);
            var domainExpectedInitialWeightSha256 =
                DesignConversionDpoPolicyBaseMetadata.ComputeInitialWeightSha256(providerModelSet.Reference);
            Assert.False(ReferenceEquals(providerModelSet.Policy, providerModelSet.Reference));
            Assert.False(ReferenceEquals(providerModelSet.Policy, providerModelSet.FreshLoadTarget));
            Assert.False(ReferenceEquals(providerModelSet.Reference, providerModelSet.FreshLoadTarget));
            Assert.True(File.Exists(domainRuntimeCheckpointPath));
            var typedRef = domainBridge.Value!.RuntimeCheckpoint.DpoPolicyCheckpointRef;
            Assert.NotNull(typedRef);
            Assert.NotNull(domainBridge.Value.PolicyCheckpointRef);
            Assert.All(
                new IReadOnlyList<string>[]
                {
                    domainBridge.Value.EvidenceRefs,
                    domainBridge.Value.RuntimeCheckpoint.EvidenceRefs,
                    typedRef!.EvidenceRefs
                },
                evidenceRefs =>
                {
                    Assert.Contains(DesignConversionDpoTrainerRouteBGate.GateId, evidenceRefs);
                    Assert.Contains(
                        "teacher_provenance:source_kind=DeterministicNoExternalTeacher",
                        evidenceRefs);
                    Assert.Contains(
                        "legal_teacher_admission:external_invocation_count=0",
                        evidenceRefs);
                });
            var domainRuntimeCheckpointDirectory = Path.GetDirectoryName(domainRuntimeCheckpointPath)!;
            var domainSiblingPolicyCheckpointPath = Path.Combine(
                domainRuntimeCheckpointDirectory,
                typedRef!.PolicyCheckpointFileName);
            Assert.Equal(Path.GetFileName(domainBridge.Value.PolicyCheckpointPath), typedRef.PolicyCheckpointFileName);
            Assert.Equal(Path.GetFileName(domainBridge.Value.PolicyCheckpointPath), domainBridge.Value.PolicyCheckpointPath);
            Assert.False(Path.IsPathRooted(domainBridge.Value.PolicyCheckpointPath));
            Assert.True(File.Exists(domainSiblingPolicyCheckpointPath));
            var domainPayloadBytes = await File.ReadAllBytesAsync(domainSiblingPolicyCheckpointPath);
            Assert.Equal(domainBridge.Value.PolicyCheckpointSha256, typedRef.Sha256);
            Assert.Equal(DesignConversionDpoTrainerRouteBGate.TrainerSourceSystem, typedRef.SourceSystem);
            Assert.Equal(DesignConversionDpoTrainerRouteBGate.FreshLoadedSourceMechanism, typedRef.SourceMechanism);
            Assert.Equal(
                DesignConversionMachine.DpoPolicyDirectInferenceRuntimeBridgeKind,
                typedRef.RuntimeBridgeKind);
            Assert.True(typedRef.FreshLoadVerified);
            Assert.True(typedRef.ConvertedToRuntimeWeights);
            Assert.Equal(1, typedRef.SourceRecordCount);
            Assert.Equal(domainBridge.Value.BridgePreferenceCount, typedRef.BridgePreferenceCount);
            Assert.Equal(domainExpectedConvergedStepCount, domainBridge.Value.DpoStepCount);
            Assert.Equal(domainExpectedConvergedStepCount, typedRef.DpoStepCount);
            var domainProviderConfig = DesignConversionDpoPolicyBaseTestProvider.CreateConfig();
            var domainLoraConfig = new LoraConfig { Rank = 2, Alpha = 4, Dropout = 0, TargetModules = ["q_proj", "v_proj"] };
            var domainLora = typedRef.TrainingContext.Lora;
            Assert.NotNull(domainLora);
            Assert.Equal(2, domainLora!.Rank);
            Assert.Equal(4, domainLora.Alpha);
            Assert.Equal(0, domainLora.Dropout);
            Assert.Equal(["q_proj", "v_proj"], domainLora.TargetModules);
            Assert.Equal(domainProviderConfig.NumLayers * domainLora.TargetModules.Count, domainLora.AdapterCount);
            Assert.Equal(providerModelSet.Identity.ConfigurationKind, typedRef.TrainingContext.ConfigKind);
            Assert.DoesNotContain("CreateTinyPolicyConfig", typedRef.TrainingContext.ConfigKind, StringComparison.Ordinal);
            Assert.Equal(domainProviderConfig.VocabSize, typedRef.TrainingContext.VocabSize);
            Assert.Equal(domainProviderConfig.ContextLen, typedRef.TrainingContext.ContextLength);
            Assert.Equal(domainProviderConfig.HiddenDim, typedRef.TrainingContext.HiddenDim);
            Assert.Equal(domainProviderConfig.NumLayers, typedRef.TrainingContext.NumLayers);
            Assert.Equal(domainProviderConfig.NumHeads, typedRef.TrainingContext.NumHeads);
            Assert.Equal(domainProviderConfig.NumKvHeads, typedRef.TrainingContext.NumKvHeads);
            Assert.Equal(domainProviderConfig.FfnHiddenDim, typedRef.TrainingContext.FfnHiddenDim);
            Assert.Equal(domainProviderConfig.RopeTheta, typedRef.TrainingContext.RopeTheta);
            Assert.Equal(domainProviderConfig.RmsNormEps, typedRef.TrainingContext.RmsNormEps);
            Assert.Equal(4, typedRef.TrainingContext.SchemaVersion);
            Assert.Equal(domainExpectedRandomContext, typedRef.TrainingContext.RandomContext);
            Assert.True(DesignConversionDpoPolicyRandomContextContract.HasRequiredEvidenceRefs(
                domainExpectedRandomContext,
                typedRef.EvidenceRefs));
            Assert.All(
                DesignConversionDpoPolicyRandomContextContract.BuildRequiredEvidenceRefs(
                    domainExpectedRandomContext),
                required => Assert.Contains(required, typedRef.EvidenceRefs));
            Assert.True(DesignConversionDpoLegalTeacherAdmissionCommitmentCodec.Validate(
                typedRef.TrainingContext.LegalTeacherAdmissionCommitment));
            Assert.True(DesignConversionDpoLegalTeacherAdmissionCommitmentCodec.MatchesCheckpointEvidence(
                typedRef.TrainingContext.LegalTeacherAdmissionCommitment,
                typedRef.EvidenceRefs));
            Assert.Equal(providerModelSet.Identity.ProviderKind, typedRef.TrainingContext.ProviderKind);
            Assert.Equal(providerModelSet.Identity.BaseModelId, typedRef.TrainingContext.BaseModelId);
            Assert.Equal(providerModelSet.Identity.BaseRevision, typedRef.TrainingContext.BaseRevision);
            Assert.Equal(providerModelSet.Identity.BaseCheckpointSha256, typedRef.TrainingContext.BaseCheckpointSha256);
            Assert.Equal(domainExpectedConfigSha256, typedRef.TrainingContext.ConfigSha256);
            Assert.Equal(domainExpectedInitialWeightSha256, typedRef.TrainingContext.InitialWeightSha256);
            Assert.Equal(
                domainExpectedInitialWeightSha256,
                DesignConversionDpoPolicyBaseMetadata.ComputeInitialWeightSha256(providerModelSet.Reference));
            Assert.Equal(
                DesignConversionDpoPolicyBaseMetadata.ComputeInitialWeightSha256(providerModelSet.Policy),
                DesignConversionDpoPolicyBaseMetadata.ComputeInitialWeightSha256(providerModelSet.FreshLoadTarget));
            Assert.True(typedRef.TrainingContext.RawPayloadPersisted);
            Assert.False(typedRef.TrainingContext.AbsolutePathPersisted);
            Assert.Equal(domainPayloadBytes.Length, typedRef.PolicyCheckpointByteLength);
            Assert.Equal(
                domainBridge.Value.PolicyCheckpointSha256,
                Convert.ToHexString(SHA256.HashData(domainPayloadBytes)).ToLowerInvariant());
            Assert.Equal(300, typedRef.TrainingContext.VocabSize);
            Assert.Equal(17, typedRef.TrainingContext.ContextLength);
            Assert.Equal(12, typedRef.TrainingContext.HiddenDim);
            Assert.Equal("test_in_memory", typedRef.TrainingContext.ProviderKind);
            Assert.Equal("test.non_default.policy.v2", typedRef.TrainingContext.ConfigKind);
            Assert.Equal("test-org/non-default-policy", typedRef.TrainingContext.BaseModelId);
            Assert.Equal("revision-v1", typedRef.TrainingContext.BaseRevision);
            var suppliedModelSet = Assert.IsType<DesignConversionDpoPolicyBaseModelSet>(providerModelSet);
            Assert.NotSame(suppliedModelSet.Policy, suppliedModelSet.Reference);
            Assert.NotSame(suppliedModelSet.Policy, suppliedModelSet.FreshLoadTarget);
            Assert.NotSame(suppliedModelSet.Reference, suppliedModelSet.FreshLoadTarget);
            Assert.Equal(
                ComputeExpectedDpoConfigSha256(suppliedModelSet.Policy.Config),
                typedRef.TrainingContext.ConfigSha256);
            Assert.Equal(
                ComputeExpectedDpoInitialWeightSha256(suppliedModelSet.Reference),
                typedRef.TrainingContext.InitialWeightSha256);
            Assert.Equal(
                suppliedModelSet.Identity.BaseCheckpointSha256,
                typedRef.TrainingContext.BaseCheckpointSha256);
            Assert.NotEqual(
                typedRef.TrainingContext.BaseCheckpointSha256,
                typedRef.TrainingContext.InitialWeightSha256);
            Assert.All(suppliedModelSet.Reference.GetCheckpointParameterEntries(),
                entry => Assert.False(entry.Parameter.Value.RequiresGrad));
            AssertDpoModelParametersBitwiseEqual(suppliedModelSet.Policy, suppliedModelSet.FreshLoadTarget);
            var domainPolicyLoraEntries = suppliedModelSet.Policy.GetCheckpointParameterEntries()
                .Where(entry => entry.Key.Contains(".lora_", StringComparison.Ordinal))
                .ToArray();
            Assert.Equal(domainLora.AdapterCount * 2, domainPolicyLoraEntries.Length);
            Assert.Contains(domainPolicyLoraEntries, entry => entry.Key.Contains("q_proj.lora_a", StringComparison.Ordinal));
            Assert.Contains(domainPolicyLoraEntries, entry => entry.Key.Contains("q_proj.lora_b", StringComparison.Ordinal));
            Assert.Contains(domainPolicyLoraEntries, entry => entry.Key.Contains("v_proj.lora_a", StringComparison.Ordinal));
            Assert.Contains(domainPolicyLoraEntries, entry => entry.Key.Contains("v_proj.lora_b", StringComparison.Ordinal));
            Assert.Contains(DesignConversionMachine.DpoTrainerPolicyCheckpointFreshLoadedEvidence, typedRef.EvidenceRefs);
            Assert.Contains(DesignConversionMachine.DpoTrainerPolicyCheckpointConvertedEvidence, typedRef.EvidenceRefs);
            Assert.Contains(DesignConversionMachine.DpoPolicyLoraAppliedEvidence, typedRef.EvidenceRefs);
            Assert.Contains("10.LoRA:LoraApplicator", typedRef.EvidenceRefs);
            Assert.Contains("10.LoRA:LoraApplicator", domainBridge.Value.IngredientIds);
            Assert.Contains($"dpo_policy_lora_rank:{domainLora.Rank}", typedRef.EvidenceRefs);
            Assert.Contains($"dpo_policy_lora_alpha:{domainLora.Alpha}", typedRef.EvidenceRefs);
            Assert.Contains($"dpo_policy_lora_dropout:{domainLora.Dropout}", typedRef.EvidenceRefs);
            Assert.Contains($"dpo_policy_lora_target_modules:{string.Join(",", domainLora.TargetModules)}", typedRef.EvidenceRefs);
            Assert.Contains($"dpo_policy_lora_adapter_count:{domainLora.AdapterCount}", typedRef.EvidenceRefs);
            Assert.Contains("DESIGN_CONVERSION_DPO_POLICY_CHECKPOINT_REF_TYPED", typedRef.EvidenceRefs);
            Assert.Contains("DESIGN_CONVERSION_DPO_POLICY_CHECKPOINT_REF_PAYLOAD_FILE_SIBLING", typedRef.EvidenceRefs);
            Assert.DoesNotContain("DESIGN_CONVERSION_DPO_POLICY_CHECKPOINT_REF_RAW_PAYLOAD_EMBEDDED", typedRef.EvidenceRefs);
            Assert.Contains("DESIGN_CONVERSION_DPO_POLICY_BASE_PROVIDER_INJECTED", typedRef.EvidenceRefs);
            Assert.Contains("DESIGN_CONVERSION_DPO_POLICY_BASE_IDENTITY_VALIDATED", typedRef.EvidenceRefs);
            Assert.Contains($"dpo_policy_base_provider_kind:{providerModelSet.Identity.ProviderKind}", typedRef.EvidenceRefs);
            Assert.Contains($"dpo_policy_base_configuration_kind:{providerModelSet.Identity.ConfigurationKind}", typedRef.EvidenceRefs);
            Assert.Contains($"dpo_policy_base_model_id:{providerModelSet.Identity.BaseModelId}", typedRef.EvidenceRefs);
            Assert.Contains($"dpo_policy_base_revision:{providerModelSet.Identity.BaseRevision}", typedRef.EvidenceRefs);
            Assert.Contains($"dpo_policy_base_checkpoint_sha256:{providerModelSet.Identity.BaseCheckpointSha256}", typedRef.EvidenceRefs);
            Assert.Contains($"dpo_policy_config_sha256:{domainExpectedConfigSha256}", typedRef.EvidenceRefs);
            Assert.Contains($"dpo_policy_initial_weight_sha256:{domainExpectedInitialWeightSha256}", typedRef.EvidenceRefs);
            Assert.Contains("teacher_provenance:source_kind=DeterministicNoExternalTeacher", typedRef.EvidenceRefs);
            Assert.Contains("legal_teacher_admission:max_teacher_calls=100", typedRef.EvidenceRefs);
            Assert.Contains("teacher_provenance:source_kind=DeterministicNoExternalTeacher", domainBridge.Value.EvidenceRefs);

            var domainJson = await File.ReadAllTextAsync(domainRuntimeCheckpointPath);
            using var domainJsonDocument = JsonDocument.Parse(domainJson);
            var typedRefJson = domainJsonDocument.RootElement.GetProperty("dpoPolicyCheckpointRef");
            Assert.Equal(typedRef.PolicyCheckpointFileName, typedRefJson.GetProperty("policyCheckpointFileName").GetString());
            Assert.Equal(typedRef.Sha256, typedRefJson.GetProperty("sha256").GetString());
            Assert.Equal(typedRef.PolicyCheckpointByteLength, typedRefJson.GetProperty("policyCheckpointByteLength").GetInt32());
            var typedRefTrainingContextJson = typedRefJson.GetProperty("trainingContext");
            AssertCurrentRandomContextJson(
                typedRefTrainingContextJson.GetProperty("randomContext"),
                domainExpectedRandomContext);
            var typedRefLoraJson = typedRefTrainingContextJson.GetProperty("lora");
            Assert.Equal(domainLora.Rank, typedRefLoraJson.GetProperty("rank").GetInt32());
            Assert.Equal(domainLora.Alpha, typedRefLoraJson.GetProperty("alpha").GetSingle());
            Assert.Equal(domainLora.Dropout, typedRefLoraJson.GetProperty("dropout").GetSingle());
            Assert.Equal(
                domainLora.TargetModules,
                typedRefLoraJson.GetProperty("targetModules").EnumerateArray().Select(target => target.GetString() ?? string.Empty).ToArray());
            Assert.Equal(domainLora.AdapterCount, typedRefLoraJson.GetProperty("adapterCount").GetInt32());
            Assert.Contains(
                typedRefJson.GetProperty("evidenceRefs").EnumerateArray(),
                evidence => string.Equals(
                    evidence.GetString(),
                    "DESIGN_CONVERSION_DPO_POLICY_CHECKPOINT_REF_PAYLOAD_FILE_SIBLING",
                    StringComparison.Ordinal));
            Assert.False(typedRefJson.TryGetProperty("rawPayloadBase64", out _));
            Assert.False(typedRefJson.TryGetProperty("rawPayloadByteLength", out _));
            Assert.False(typedRefJson.TryGetProperty("policyCheckpointPath", out _));
            var domainPayloadBase64 = Convert.ToBase64String(domainPayloadBytes);
            var normalizedDomainJson = domainJson.Replace("\\\\", "\\");
            Assert.DoesNotContain(domainPayloadBase64, domainJson, StringComparison.Ordinal);
            Assert.DoesNotContain(Path.GetFullPath(domainSiblingPolicyCheckpointPath), normalizedDomainJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(Path.GetFullPath(checkpointDir), normalizedDomainJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("CreateTinyPolicyConfig", domainJson, StringComparison.Ordinal);
            Assert.DoesNotContain("rawHtml", domainJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("rawCss", domainJson, StringComparison.OrdinalIgnoreCase);

            var bareDomainPolicy = new LlmModel(providerModelSet.Policy.Config);
            var bareDomainLoadFailure = Assert.ThrowsAny<Exception>(() =>
                bareDomainPolicy.LoadCheckpoint(domainSiblingPolicyCheckpointPath));
            Assert.Contains("LLM_CHECKPOINT_PARAMETER_", bareDomainLoadFailure.Message, StringComparison.Ordinal);
            var loraRoundTripPolicy = new LlmModel(providerModelSet.Policy.Config);
            var loraRoundTripAdapters = LoraApplicator.Apply(loraRoundTripPolicy, domainLoraConfig);
            Assert.Equal(domainLora.AdapterCount, loraRoundTripAdapters.Count);
            loraRoundTripPolicy.LoadCheckpoint(domainSiblingPolicyCheckpointPath);
            var roundTripLoraEntries = loraRoundTripPolicy.GetCheckpointParameterEntries()
                .Where(entry => entry.Key.Contains(".lora_", StringComparison.Ordinal))
                .ToArray();
            Assert.Equal(domainLora.AdapterCount * 2, roundTripLoraEntries.Length);
            AssertDpoModelParametersBitwiseEqual(providerModelSet.Policy, loraRoundTripPolicy);

            var domainLoaded = await DesignConversionMachine.LoadCheckpointAsync(domainRuntimeCheckpointPath);
            Assert.True(domainLoaded.IsSuccess, domainLoaded.ErrorMessage);
            Assert.NotNull(domainLoaded.Value!.Checkpoint.DpoPolicyCheckpointRef);
            Assert.Equal(typedRef.PolicyCheckpointFileName, domainLoaded.Value.Checkpoint.DpoPolicyCheckpointRef!.PolicyCheckpointFileName);
            Assert.Equal(typedRef.Sha256, domainLoaded.Value.Checkpoint.DpoPolicyCheckpointRef.Sha256);
            Assert.Equal(typedRef.SourceMechanism, domainLoaded.Value.Checkpoint.DpoPolicyCheckpointRef.SourceMechanism);
            Assert.Equal(typedRef.BridgePreferenceCount, domainLoaded.Value.Checkpoint.DpoPolicyCheckpointRef.BridgePreferenceCount);
            Assert.Equal(typedRef.TrainingContext.ConfigKind, domainLoaded.Value.Checkpoint.DpoPolicyCheckpointRef.TrainingContext.ConfigKind);
            Assert.Equal(
                domainExpectedRandomContext,
                domainLoaded.Value.Checkpoint.DpoPolicyCheckpointRef.TrainingContext.RandomContext);
            var loadedDomainLora = domainLoaded.Value.Checkpoint.DpoPolicyCheckpointRef.TrainingContext.Lora;
            Assert.NotNull(loadedDomainLora);
            Assert.Equal(domainLora.Rank, loadedDomainLora!.Rank);
            Assert.Equal(domainLora.Alpha, loadedDomainLora.Alpha);
            Assert.Equal(domainLora.Dropout, loadedDomainLora.Dropout);
            Assert.Equal(domainLora.TargetModules, loadedDomainLora.TargetModules);
            Assert.Equal(domainLora.AdapterCount, loadedDomainLora.AdapterCount);
            Assert.Equal(typedRef.PolicyCheckpointByteLength, domainLoaded.Value.Checkpoint.DpoPolicyCheckpointRef.PolicyCheckpointByteLength);

            var directProbe = await RunMachineScoreAsync(
                domainLoaded.Value.Machine,
                runtimeInput,
                "dpo-policy-bridge-domain-direct-runtime");
            AssertDirectPolicyConfiguredPairUnavailableProbe(
                directProbe,
                baselineProbe,
                runtimeInput.Request.Html,
                runtimeInput.Request.Css,
                runtimeInput.Request.DesignTreePayload.Content,
                runtimeInput.Request.OriginalTreePayload.Content,
                checkpointDir,
                domainRuntimeCheckpointPath,
                domainSiblingPolicyCheckpointPath,
                policyCheckpointPath);
            Assert.Contains(DesignConversionMachine.DpoPolicyLoraAppliedEvidence, directProbe.EvidenceRefs);
            Assert.Contains(DesignConversionMachine.DpoPolicyRuntimeLoraTopologyEvidence, directProbe.EvidenceRefs);
            Assert.False(
                directProbe.Metrics.TryGetValue("local_checkpoint_feature_adjustment_applied", out var directLocalAdjustment) &&
                string.Equals(directLocalAdjustment, bool.TrueString, StringComparison.Ordinal));
            var directProbePublicSurface = string.Join(
                "\n",
                directProbe.EvidenceRefs.Concat([
                    JsonSerializer.Serialize(directProbe.Metrics),
                    directProbe.PrimaryMetricName,
                    directProbe.ScalingSignalNote,
                    directProbe.Notes
                ]));
            Assert.DoesNotContain(runtimeInput.Request.Html, directProbePublicSurface, StringComparison.Ordinal);
            Assert.DoesNotContain(runtimeInput.Request.Css, directProbePublicSurface, StringComparison.Ordinal);
            Assert.DoesNotContain(runtimeInput.Request.DesignTreePayload.Content, directProbePublicSurface, StringComparison.Ordinal);
            Assert.DoesNotContain(runtimeInput.Request.OriginalTreePayload.Content, directProbePublicSurface, StringComparison.Ordinal);
            Assert.DoesNotContain(Path.GetFullPath(checkpointDir), directProbePublicSurface, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(Path.GetFullPath(domainRuntimeCheckpointPath), directProbePublicSurface, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(Path.GetFullPath(domainSiblingPolicyCheckpointPath), directProbePublicSurface, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(Path.GetFullPath(policyCheckpointPath), directProbePublicSurface, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            DesignConversionDpoTrainingHandoff.RuntimeOptimizerBatchObserverForTesting = null;
            if (Directory.Exists(checkpointDir))
                Directory.Delete(checkpointDir, recursive: true);
        }
    }

    [Fact]
    public async Task DpoTrainerPolicyCheckpointBridge_EvaluatedHeldOutAuthorityRoundTripsAndOwnsAcceptance()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "llm_mvp_core",
            $"dpo-policy-bridge-evaluated-authority-{Guid.NewGuid():N}");
        var output = Path.Combine(root, "output");
        var runtimeCheckpointPath = Path.Combine(output, "runtime.json");
        var runId = "evaluated-authority-roundtrip";

        try
        {
            var partition = BuildEvaluatedBridgePartitionForApiTest(runId, heldOutCount: 2);
            var provider = new DesignConversionDpoPolicyBaseTestProvider();
            var baselineInput = DesignConversionTestFixtures.CreateInput(
                comparisonScore: 0.40,
                comparisonPassed: false);
            var baselineProbe = await RunMachineScoreAsync(
                new DesignConversionMachine(),
                baselineInput,
                "dpo-policy-bridge-evaluated-authority-baseline");
            Assert.False(baselineProbe.Passed);

            var evaluated = await DesignConversionDpoPolicyCheckpointBridge
                .ExecuteEvaluatedAndConvertAsync(
                    DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                    partition,
                    output,
                    runId,
                    runtimeCheckpointPath,
                    provider,
                    new DesignConversionDpoPolicyCheckpointBridgeOptions(
                        MaxBridgePreferenceCount: 1,
                        DpoStepCount: 1),
                    ct: CancellationToken.None);

            Assert.True(evaluated.IsSuccess, evaluated.ErrorMessage);
            using var lease = evaluated.Value!;
            var bridge = lease.Result;
            var evaluation = lease.Evaluation;
            Assert.True(provider.InvocationCount > 0);
            Assert.True(evaluation.Accuracy >= 0d);
            Assert.NotNull(bridge.PolicyCheckpointRef);
            Assert.NotNull(bridge.RuntimeCheckpoint.DpoPolicyCheckpointRef);
            Assert.Equal(
                bridge.PolicyCheckpointRef,
                bridge.RuntimeCheckpoint.DpoPolicyCheckpointRef);
            var typedRef = bridge.PolicyCheckpointRef!;
            Assert.NotNull(typedRef.EvaluatedHeldOutAcceptanceAuthority);
            Assert.Equal(
                evaluation.HeldOutEvaluationInputSha256,
                typedRef.EvaluatedHeldOutAcceptanceAuthority!.HeldOutEvaluationInputSha256);
            Assert.Equal(
                evaluation.ChildPolicyCheckpointSha256,
                typedRef.EvaluatedHeldOutAcceptanceAuthority.ChildPolicyCheckpointSha256);
            Assert.Equal(evaluation.Accuracy, bridge.RuntimeCheckpoint.AccuracyAfterLastBatch);
            Assert.Equal(evaluation.Accuracy, bridge.RuntimeExport.AccuracyAtExport);
            Assert.Contains(
                DesignConversionMachine.DpoRuntimeCheckpointAccuracyFromHeldOutEvaluationEvidence,
                bridge.RuntimeCheckpoint.EvidenceRefs);
            Assert.Contains(
                DesignConversionMachine.DpoRuntimeCheckpointAccuracyCurrentCycleHeldOutEvaluationScopeEvidence,
                bridge.RuntimeCheckpoint.EvidenceRefs);
            Assert.Contains(
                $"dpo_runtime_checkpoint_accuracy_value:{evaluation.Accuracy.ToString("R", CultureInfo.InvariantCulture)}",
                bridge.RuntimeCheckpoint.EvidenceRefs);
            Assert.DoesNotContain(
                DesignConversionMachine.DpoRuntimeCheckpointAccuracyFromHeldOutEvaluationEvidence,
                typedRef.EvidenceRefs);

            var persistedJson = await File.ReadAllTextAsync(runtimeCheckpointPath);
            using var document = JsonDocument.Parse(persistedJson);
            var persistedRefJson = document.RootElement.GetProperty("dpoPolicyCheckpointRef");
            Assert.True(persistedRefJson.TryGetProperty("evaluatedHeldOutAcceptanceAuthority", out var persistedAuthority));
            Assert.Equal(
                evaluation.HeldOutEvaluationInputSha256,
                persistedAuthority.GetProperty("heldOutEvaluationInputSha256").GetString());
            Assert.DoesNotContain(root, persistedJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(baselineInput.Request.Html, persistedJson, StringComparison.Ordinal);
            Assert.DoesNotContain(baselineInput.Request.Css, persistedJson, StringComparison.Ordinal);

            var loaded = await DesignConversionMachine.LoadCheckpointWithEvaluationOptionsAsync(
                runtimeCheckpointPath,
                new DesignConversionMachineEvaluationOptions(
                    heldOutEvaluationAccuracyGateThreshold: 0d));
            Assert.True(loaded.IsSuccess, loaded.ErrorMessage);
            Assert.NotNull(loaded.Value!.Checkpoint.DpoPolicyCheckpointRef);
            Assert.NotNull(loaded.Value.Checkpoint.DpoPolicyCheckpointRef!.EvaluatedHeldOutAcceptanceAuthority);
            Assert.Equal(evaluation.Accuracy, loaded.Value.Checkpoint.AccuracyAfterLastBatch);

            var directProbe = await RunMachineScoreAsync(
                loaded.Value.Machine,
                baselineInput,
                "dpo-policy-bridge-evaluated-authority-direct");

            AssertDirectPolicyConfiguredPairUnavailableProbe(
                directProbe,
                baselineProbe,
                baselineInput.Request.Html,
                baselineInput.Request.Css,
                baselineInput.Request.DesignTreePayload.Content,
                baselineInput.Request.OriginalTreePayload.Content,
                root,
                runtimeCheckpointPath);
            Assert.DoesNotContain(
                DesignConversionMachine.DpoRuntimeCheckpointAccuracyFromHeldOutEvaluationEvidence,
                directProbe.EvidenceRefs);
            Assert.DoesNotContain(
                DesignConversionMachine.DpoRuntimeCheckpointAccuracyCurrentCycleHeldOutEvaluationScopeEvidence,
                directProbe.EvidenceRefs);
            Assert.Equal(bool.FalseString, directProbe.Metrics["direct_dpo_policy_acceptance_authority"]);
            Assert.Equal(bool.TrueString, directProbe.Metrics["baseline_visual_verdict_acceptance_authority"]);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task DpoTrainerPolicyCheckpointBridge_RejectsInjectedLegacyPolicyBaseProvider()
    {
        var checkpointDir = Path.Combine(Path.GetTempPath(), $"design-conversion-dpo-policy-provider-negative-{Guid.NewGuid():N}");
        var runtimeCheckpointPath = Path.Combine(checkpointDir, "design-conversion-runtime-from-invalid-provider.json");

        try
        {
            var provider = new DesignConversionDpoPolicyBaseTestProvider(modelSet =>
                OperationResult<DesignConversionDpoPolicyBaseModelSet>.Ok(modelSet with
                {
                    Identity = modelSet.Identity with
                    {
                        ConfigurationKind = "DesignConversionDpoPolicyCheckpointBridge.CreateTinyPolicyConfig"
                    }
                }));

            var result = await DesignConversionDpoPolicyCheckpointBridge.ExecuteAndConvertAsync(
                DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                [BuildDpoHandoffRecord()],
                checkpointDir,
                "pass153-invalid-provider-is-not-ignored",
                runtimeCheckpointPath,
                provider);

            Assert.False(result.IsSuccess);
            Assert.Equal(1, provider.InvocationCount);
            Assert.Equal("DESIGN_CONVERSION_DPO_POLICY_BASE_IDENTITY_INVALID", result.ErrorCode);
            Assert.False(
                File.Exists(runtimeCheckpointPath),
                "Ignoring the injected provider would silently create a runtime checkpoint from the default legacy base.");
        }
        finally
        {
            if (Directory.Exists(checkpointDir))
                Directory.Delete(checkpointDir, recursive: true);
        }
    }

    [Fact]
    public async Task DpoTrainerPolicyCheckpointBridge_RejectsMissingBareAndMismatchedLayoutsWithoutValueMutation()
    {
        var tokenizer = BuildDpoHandoffTokenizer();
        var handoffProvider = new DesignConversionDpoPolicyBaseTestProvider();
        var checkpointDir = Path.Combine(Path.GetTempPath(), $"design-conversion-dpo-policy-negative-bridge-{Guid.NewGuid():N}");
        var runtimeCheckpointPath = Path.Combine(checkpointDir, "runtime.json");

        try
        {
            var record = BuildDpoHandoffRecord();
            var handoff = await DesignConversionDpoTrainingHandoff.ExecuteAsync(
                DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                [record],
                handoffProvider,
                tokenizer,
                checkpointDir,
                new LoraConfig { Rank = 2, Alpha = 4, Dropout = 0, TargetModules = ["q_proj", "v_proj"] },
                maxStepCount: 1);
            Assert.True(handoff.IsSuccess, handoff.ErrorMessage);
            Assert.Equal(1, handoffProvider.InvocationCount);
            Assert.NotNull(handoffProvider.LastModelSet);
            var policyCheckpointPath = Assert.Single(handoff.Value!.PolicyCheckpointFilePaths);

            var missingConfigProvider = new DesignConversionDpoPolicyBaseTestProvider();
            var missingConfig = await DesignConversionDpoTrainingHandoff.ConvertPolicyCheckpointToRuntimeCheckpointAsync(
                DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                record,
                policyCheckpointPath,
                missingConfigProvider,
                loraConfiguration: null,
                runtimeCheckpointPath);
            Assert.False(missingConfig.IsSuccess);
            Assert.Equal("DESIGN_CONVERSION_DPO_POLICY_LORA_CONFIG_REQUIRED", missingConfig.ErrorCode);
            Assert.Equal(0, missingConfigProvider.InvocationCount);

            var bareModel = new LlmModel(handoffProvider.LastModelSet!.Policy.Config);
            var bareParameters = bareModel.Parameters().Select(parameter => parameter.Value).ToArray();
            var bareValues = bareParameters.Select(parameter => parameter.Data.ToArray()).ToArray();
            var bareFailure = Assert.ThrowsAny<Exception>(() => bareModel.LoadCheckpoint(policyCheckpointPath));
            Assert.Contains("LLM_CHECKPOINT_PARAMETER_COUNT_MISMATCH", bareFailure.Message, StringComparison.Ordinal);
            Assert.True(bareParameters.Select(parameter => parameter.Data).Zip(bareValues).All(pair => pair.First.SequenceEqual(pair.Second)));

            var invalidProvider = new DesignConversionDpoPolicyBaseTestProvider(modelSet =>
                OperationResult<DesignConversionDpoPolicyBaseModelSet>.Ok(modelSet with
                {
                    Identity = modelSet.Identity with
                    {
                        ConfigurationKind = "DesignConversionDpoTrainingHandoff.ConvertPolicyCheckpointToRuntimeCheckpointAsync:freshPolicyModel.Config"
                    }
                }));
            var invalidProviderResult = await DesignConversionDpoTrainingHandoff.ConvertPolicyCheckpointToRuntimeCheckpointAsync(
                DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                record,
                policyCheckpointPath,
                invalidProvider,
                handoff.Value.LoraConfiguration,
                runtimeCheckpointPath);
            Assert.False(invalidProviderResult.IsSuccess);
            Assert.Equal(1, invalidProvider.InvocationCount);
            Assert.Equal("DESIGN_CONVERSION_DPO_POLICY_BASE_IDENTITY_INVALID", invalidProviderResult.ErrorCode);
            Assert.False(File.Exists(runtimeCheckpointPath));

            var mismatchedConfiguration = new DesignConversionLoraCheckpointConfiguration(new LoraConfig
            {
                Rank = 2,
                Alpha = 4,
                Dropout = 0,
                TargetModules = ["o_proj", "v_proj"]
            });

            var mismatchedProvider = new DesignConversionDpoPolicyBaseTestProvider();
            var mismatched = await DesignConversionDpoTrainingHandoff.ConvertPolicyCheckpointToRuntimeCheckpointAsync(
                DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                record,
                policyCheckpointPath,
                mismatchedProvider,
                mismatchedConfiguration,
                runtimeCheckpointPath);
            Assert.False(mismatched.IsSuccess);
            Assert.Equal(1, mismatchedProvider.InvocationCount);
            Assert.Equal("DESIGN_CONVERSION_DPO_POLICY_CHECKPOINT_FRESH_LOAD_FAILED", mismatched.ErrorCode);
            Assert.Equal("Policy checkpoint fresh-load failed.", mismatched.ErrorMessage);
            Assert.DoesNotContain("LLM_CHECKPOINT_PARAMETER_", mismatched.ErrorMessage, StringComparison.Ordinal);
            Assert.False(File.Exists(runtimeCheckpointPath));
        }
        finally
        {
            if (Directory.Exists(checkpointDir))
                Directory.Delete(checkpointDir, recursive: true);
        }
    }

    [Fact]
    public async Task DpoTrainerPolicyCheckpointBridge_LegacyPromotionRejectsConvertedCheckpointWithoutEvaluatedDpoReadinessAuthority()
    {
        var tokenizer = BuildDpoHandoffTokenizer();
        var checkpointDir = Path.Combine(Path.GetTempPath(), $"design-conversion-dpo-policy-promotion-{Guid.NewGuid():N}");
        var runtimeCheckpointPath = Path.Combine(checkpointDir, "design-conversion-runtime-from-dpo-policy.json");

        try
        {
            Directory.CreateDirectory(checkpointDir);
            var record = BuildDpoHandoffRecord();
            var handoffProvider = new DesignConversionDpoPolicyBaseTestProvider();
            var handoff = await DesignConversionDpoTrainingHandoff.ExecuteAsync(
                DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                [record],
                handoffProvider,
                tokenizer,
                checkpointDir,
                new LoraConfig { Rank = 2, Alpha = 4, Dropout = 0, TargetModules = ["q_proj", "v_proj"] },
                maxStepCount: 1);
            Assert.True(handoff.IsSuccess, handoff.ErrorMessage);
            var policyCheckpointPath = Assert.Single(handoff.Value!.PolicyCheckpointFilePaths);

            var bridgeProvider = new DesignConversionDpoPolicyBaseTestProvider();
            var bridge = await DesignConversionDpoTrainingHandoff.ConvertPolicyCheckpointToRuntimeCheckpointAsync(
                DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                record,
                policyCheckpointPath,
                bridgeProvider,
                handoff.Value.LoraConfiguration,
                runtimeCheckpointPath);
            Assert.True(bridge.IsSuccess, bridge.ErrorMessage);

            var loaded = await DesignConversionMachine.LoadCheckpointAsync(runtimeCheckpointPath);
            Assert.True(loaded.IsSuccess, loaded.ErrorMessage);
            var artifacts = await WriteDpoBridgePromotionArtifactsAsync(checkpointDir, runtimeCheckpointPath);
            var pipelineResult = await BuildDpoBridgePromotionReadyPipelineResultAsync();
            Assert.Equal(
                DesignConversionMeasurementGateStatus.Failed,
                pipelineResult.LifecycleEvidence.MeasurementGateStatus);
            Assert.False(pipelineResult.LifecycleEvidence.PromotionUploadAllowed);
            Assert.Equal(
                "PROMOTION_BLOCKED_MEASUREMENT_GATE_NOT_PASSED",
                pipelineResult.LifecycleEvidence.PromotionStatus);
            Assert.Equal(
                DesignConversionLocalContinueTrainingProbe.VerifiedStatus,
                pipelineResult.LifecycleEvidence.ContinueTrainingStatus);
            Assert.True(pipelineResult.LifecycleEvidence.LocalContinueTrainingProbe?.RealTrainedWeightsContinued);
            Assert.Equal(0, pipelineResult.LifecycleEvidence.LocalContinueTrainingProbe?.HeldOutRecordUsageCount);
            Assert.Contains(
                "DESIGN_CONVERSION_CHILD_DPO_MEASUREMENT_NOT_AVAILABLE_IN_TRAINING_PIPELINE",
                pipelineResult.LifecycleEvidence.FailureReasons);
            Assert.Contains(
                "DESIGN_CONVERSION_SERVER_OWNED_PROMOTION_BUDGET_AUTHORITY_REQUIRED",
                pipelineResult.LifecycleEvidence.FailureReasons);
            Assert.Contains(
                "DESIGN_CONVERSION_SERVER_OWNED_PROMOTION_INTEGRITY_AUTHORITY_REQUIRED",
                pipelineResult.LifecycleEvidence.FailureReasons);
            Assert.DoesNotContain(
                DesignConversionTrainingPipeline.SourceDisjointHeldOutReadinessGatePassedEvidence,
                pipelineResult.LifecycleEvidence.EvidenceRefs);
            Assert.DoesNotContain(
                DesignConversionTrainingPipeline.SourceDisjointHeldOutReadinessGatePassedEvidence,
                pipelineResult.LifecycleEvidence.MeasurementGate!.EvidenceRefs);
            Assert.DoesNotContain(
                DesignConversionMachine.DpoTrainerPolicyCheckpointConvertedEvidence,
                pipelineResult.LifecycleEvidence.EvidenceRefs);

            var publication = await new DesignConversionPromotionPublisher(new LocalReceiptPromotionUploader()).PublishAsync(
                loaded.Value!.Machine,
                pipelineResult,
                "dpo-bridge-local-promotion-manifest",
                artifacts,
                BuildDpoBridgePromotionGate(
                    artifacts,
                    includeSourceDisjointHeldOutReadinessEvidence: false));
            Assert.True(publication.IsSuccess, publication.ErrorMessage);
            Assert.Equal(DesignConversionPromotionDecisionKind.Reject, publication.Value!.Decision.Kind);
            Assert.Contains(
                "DESIGN_CONVERSION_MEASUREMENT_GATE_NOT_PASSED:failed",
                publication.Value.Decision.Reasons);
            Assert.False(publication.Value.Manifest.LocalPromotionReceiptWritten);
            Assert.False(publication.Value.Manifest.ModelServerUploadAttempted);
            Assert.False(publication.Value.Manifest.ModelServerPromotionClaimed);
            Assert.Null(publication.Value.UploadReceipt);
            Assert.DoesNotContain(
                DesignConversionTrainingPipeline.SourceDisjointHeldOutReadinessGatePassedEvidence,
                publication.Value.Manifest.EvidenceRefs);

            var promoted = await DesignConversionPromotedLocalModelLoader.LoadAsync(
                publication.Value,
                runtimeCheckpointPath);
            Assert.False(promoted.IsSuccess);
            Assert.Equal("DESIGN_CONVERSION_LOCAL_MODEL_PROMOTION_REQUIRED", promoted.ErrorCode);
        }
        finally
        {
            if (Directory.Exists(checkpointDir))
                Directory.Delete(checkpointDir, recursive: true);
        }
    }

    [Fact]
    public async Task DpoTrainerPolicyCheckpointBridge_MetadataOnlyPromotedModelDoesNotClaimLearnedInference()
    {
        var service = new DesignConversionInternalService(
            new RecordingRenderProvider(),
            new RecordingComparisonProvider(CreateVerdict("dpo-bridge-metadata-only-verdict", 0.91, passed: true)),
            routingFlow: null);
        var api = new DesignConversionModelServerApi(
            service,
            () => DateTimeOffset.Parse("2026-07-08T10:05:00Z"),
            promotedLocalModel: BuildDpoTrainerPromotedLocalModel(
                DesignConversionTrainingPipeline.SourceDisjointHeldOutReadinessGatePassedEvidence),
            postEvaluationStateStore: TestPostEvaluationStateStore);

        var response = await api.SubmitAndEvaluateAsync(CreateRequest("dpo-bridge-metadata-only-request"));

        Assert.True(response.IsSuccess, response.ErrorMessage);
        Assert.False(response.Value!.LearnedInferenceApplied);
        Assert.False(response.Value.ModelTrainingVerified);
        Assert.False(response.Value.ModelUploadReady);
        Assert.False(response.Value.DeploymentVerified);
        Assert.False(response.Value.ProductReady);
        Assert.False(response.Value.TrainedModelAuthority);
        Assert.True(response.Value.NotTrainedModelAuthority);
        Assert.Contains(DesignConversionMachine.DpoTrainerPolicyCheckpointFreshLoadedEvidence, response.Value.EvidenceRefs);
        Assert.Contains(DesignConversionMachine.DpoTrainerPolicyCheckpointConvertedEvidence, response.Value.EvidenceRefs);
        Assert.DoesNotContain(DesignConversionMachine.DpoTrainerPolicyCheckpointRuntimeScoringEvidence, response.Value.EvidenceRefs);
        Assert.DoesNotContain(DesignConversionMachine.DpoPolicyDirectInferenceScoreEvidence, response.Value.EvidenceRefs);
        Assert.DoesNotContain(DesignConversionMachine.LearnedInferenceAppliedEvidence, response.Value.EvidenceRefs);
        Assert.Equal(bool.FalseString, response.Value.BoundaryMetadata["LearnedInferenceApplied"]);
        Assert.Equal(bool.FalseString, response.Value.BoundaryMetadata["learned_inference_server_owned_evidence_verified"]);

        var status = api.GetJobResult(response.Value.JobId);
        Assert.True(status.IsSuccess, status.ErrorMessage);
        Assert.False(status.Value!.LearnedInferenceApplied);
        Assert.Equal(bool.FalseString, status.Value.BoundaryMetadata["LearnedInferenceApplied"]);
    }

    [Fact]
    public async Task DpoTrainerPolicyCheckpointBridge_CallerForgedRuntimeEvidenceRefsDoNotClaimLearnedInference()
    {
        var forgedModel = BuildCallerForgedPromotedLocalModel(
            DesignConversionMachine.DpoTrainerPolicyCheckpointRuntimeScoringEvidence,
            DesignConversionMachine.DpoPolicyDirectInferenceScoreEvidence);
        var service = new DesignConversionInternalService(
            new RecordingRenderProvider(),
            new RecordingComparisonProvider(CreateVerdict("dpo-bridge-forged-runtime-token-verdict", 0.91, passed: true)),
            routingFlow: null);
        var api = new DesignConversionModelServerApi(
            service,
            () => DateTimeOffset.Parse("2026-07-08T10:10:00Z"),
            promotedLocalModel: forgedModel,
            postEvaluationStateStore: TestPostEvaluationStateStore);

        var response = await api.SubmitAndEvaluateAsync(CreateRequest("dpo-bridge-forged-runtime-token-request"));

        Assert.True(response.IsSuccess, response.ErrorMessage);
        Assert.False(response.Value!.LearnedInferenceApplied);
        Assert.False(response.Value.ModelTrainingVerified);
        Assert.False(response.Value.ModelUploadReady);
        Assert.False(response.Value.DeploymentVerified);
        Assert.False(response.Value.ProductReady);
        Assert.False(response.Value.TrainedModelAuthority);
        Assert.True(response.Value.NotTrainedModelAuthority);
        Assert.Contains(DesignConversionMachine.DpoTrainerPolicyCheckpointRuntimeScoringEvidence, response.Value.EvidenceRefs);
        Assert.Contains(DesignConversionMachine.DpoPolicyDirectInferenceScoreEvidence, response.Value.EvidenceRefs);
        Assert.Equal(bool.FalseString, response.Value.BoundaryMetadata["LearnedInferenceApplied"]);
        Assert.Equal(bool.FalseString, response.Value.BoundaryMetadata["learned_inference_server_owned_evidence_verified"]);
        Assert.Equal(bool.FalseString, response.Value.BoundaryMetadata["model_training_verified"]);
        Assert.Equal(bool.FalseString, response.Value.BoundaryMetadata["model_upload_ready"]);
        Assert.Equal(bool.FalseString, response.Value.BoundaryMetadata["deployment_verified"]);
        Assert.Equal(bool.FalseString, response.Value.BoundaryMetadata["product_ready"]);
        Assert.Equal(bool.FalseString, response.Value.BoundaryMetadata["trained_model_authority"]);
        Assert.Equal(bool.TrueString, response.Value.BoundaryMetadata["not_trained_model_authority"]);

        var status = api.GetJobResult(response.Value.JobId);
        Assert.True(status.IsSuccess, status.ErrorMessage);
        Assert.False(status.Value!.LearnedInferenceApplied);
        Assert.False(status.Value.DeploymentVerified);
        Assert.False(status.Value.ProductReady);
        Assert.False(status.Value.TrainedModelAuthority);
        Assert.True(status.Value.NotTrainedModelAuthority);
        Assert.Contains(DesignConversionMachine.DpoTrainerPolicyCheckpointRuntimeScoringEvidence, status.Value.EvidenceRefs);
        Assert.Contains(DesignConversionMachine.DpoPolicyDirectInferenceScoreEvidence, status.Value.EvidenceRefs);
        Assert.Equal(bool.FalseString, status.Value.BoundaryMetadata["LearnedInferenceApplied"]);
        Assert.Equal(bool.FalseString, status.Value.BoundaryMetadata["learned_inference_server_owned_evidence_verified"]);
        Assert.Equal(bool.FalseString, status.Value.BoundaryMetadata["product_ready"]);
        Assert.Equal(bool.FalseString, status.Value.BoundaryMetadata["trained_model_authority"]);
        Assert.Equal(bool.TrueString, status.Value.BoundaryMetadata["not_trained_model_authority"]);
    }

    [Fact]
    public async Task DpoTrainerPolicyCheckpointBridge_RejectsSameSourceHeldOutAndPassedStatusOverrideWithoutReadinessMarker()
    {
        var root = Path.Combine(Path.GetTempPath(), $"design-conversion-dpo-policy-heldout-gate-{Guid.NewGuid():N}");
        try
        {
            Directory.CreateDirectory(root);
            var sameSourcePipeline = await BuildDpoBridgePromotionPipelineResultAsync(
                BuildDpoBridgePromotionMeasurementGateRequest(
                    "dpo-bridge-same-source-self-eval",
                    sourceDisjoint: false));

            Assert.Equal(DesignConversionMeasurementGateStatus.Failed, sameSourcePipeline.LifecycleEvidence.MeasurementGateStatus);
            Assert.False(sameSourcePipeline.LifecycleEvidence.PromotionUploadAllowed);
            Assert.Equal("PROMOTION_BLOCKED_CALLER_ADMISSION_MEASUREMENT_NOT_PASSED", sameSourcePipeline.LifecycleEvidence.PromotionStatus);
            Assert.Contains("SOURCE_DISJOINT_PROOF_FAILED", sameSourcePipeline.LifecycleEvidence.FailureReasons);
            Assert.DoesNotContain(
                DesignConversionTrainingPipeline.SourceDisjointHeldOutReadinessGatePassedEvidence,
                sameSourcePipeline.LifecycleEvidence.EvidenceRefs);

            var manualPassedGate = BuildDpoBridgeManualPassedMeasurementGateWithoutReadinessMarker();
            var passedStatusOnlyPipeline = sameSourcePipeline with
            {
                LifecycleEvidence = sameSourcePipeline.LifecycleEvidence with
                {
                    MeasurementGate = manualPassedGate,
                    MeasurementGateStatus = DesignConversionMeasurementGateStatus.Passed,
                    CheckpointContainsRealTrainedWeights = true,
                    PromotionUploadAllowed = true,
                    EvidenceRefs = sameSourcePipeline.LifecycleEvidence.EvidenceRefs
                        .Concat(manualPassedGate.EvidenceRefs)
                        .Distinct(StringComparer.Ordinal)
                        .ToArray(),
                    FailureReasons = []
                }
            };
            var artifacts = await WriteDpoBridgePromotionArtifactsAsync(
                root,
                Path.Combine(root, "dpo-bridge-runtime-checkpoint.json"));
            var machine = new DesignConversionMachine();
            var checkpoint = await machine.SaveCheckpointAsync(artifacts.CheckpointPath);
            Assert.True(checkpoint.IsSuccess, checkpoint.ErrorMessage);

            var publication = await new DesignConversionPromotionPublisher(new LocalReceiptPromotionUploader()).PublishAsync(
                machine,
                passedStatusOnlyPipeline,
                "dpo-bridge-source-disjoint-heldout-required",
                artifacts,
                BuildDpoBridgePromotionGate(
                    artifacts,
                    includeSourceDisjointHeldOutReadinessEvidence: false));

            Assert.True(publication.IsSuccess, publication.ErrorMessage);
            Assert.Equal(DesignConversionPromotionDecisionKind.Reject, publication.Value!.Decision.Kind);
            Assert.Contains(
                DesignConversionTrainingPipeline.SourceDisjointHeldOutReadinessGateNotProvenReason,
                publication.Value.Decision.Reasons);
            Assert.Contains(
                DesignConversionTrainingPipeline.SourceDisjointHeldOutReadinessGateBlockedEvidence,
                publication.Value.Decision.EvidenceRefs);
            Assert.Null(publication.Value.UploadReceipt);
            Assert.False(publication.Value.Manifest.ModelServerUploadAttempted);
            Assert.False(publication.Value.Manifest.ModelServerPromotionClaimed);
            Assert.False(publication.Value.Manifest.LocalPromotionReceiptWritten);
            Assert.DoesNotContain(
                DesignConversionTrainingPipeline.SourceDisjointHeldOutReadinessGatePassedEvidence,
                publication.Value.Manifest.EvidenceRefs);

            var promoted = await DesignConversionPromotedLocalModelLoader.LoadAsync(
                publication.Value,
                artifacts.CheckpointPath);
            Assert.False(promoted.IsSuccess);
            Assert.Equal("DESIGN_CONVERSION_LOCAL_MODEL_PROMOTION_REQUIRED", promoted.ErrorCode);

            var forgedApiFactory = await DesignConversionModelServerApi.CreateFromTrainingIntakeLocalPromotionAsync(
                BuildForgedReadyDpoBridgeLocalPromotion(
                    passedStatusOnlyPipeline,
                    publication.Value,
                    root,
                    artifacts.CheckpointPath),
                new RecordingRenderProvider(),
                new RecordingComparisonProvider(CreateVerdict("dpo-bridge-forged-local-promotion-verdict", 0.91, passed: true)));
            Assert.False(forgedApiFactory.IsSuccess);
            Assert.Equal("DESIGN_CONVERSION_LOCAL_MODEL_PROMOTION_REQUIRED", forgedApiFactory.ErrorCode);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Theory]
    [InlineData("DesignConversionDpoPolicyCheckpointBridge.CreateTinyPolicyConfig")]
    [InlineData("DesignConversionDpoTrainingHandoff.ConvertPolicyCheckpointToRuntimeCheckpointAsync:freshPolicyModel.Config")]
    public async Task DpoTrainerPolicyCheckpointBridge_HistoricalV1ConfigurationKindFailsBeforeMutationOrArtifacts(
        string historicalConfigurationKind)
    {
        var outputDirectory = Path.Combine(
            Path.GetTempPath(),
            $"design-conversion-dpo-historical-config-kind-{Guid.NewGuid():N}");
        const string runId = "historical-config-kind-rejected";
        var runtimeCheckpointPath = Path.Combine(outputDirectory, "runtime.json");
        var policyCheckpointDirectory = Path.Combine(
            outputDirectory,
            $"{runId}-dpo-policy-checkpoints");
        var policyCheckpointPath = Path.Combine(
            policyCheckpointDirectory,
            DpoTrainer.BuildPolicyCheckpointFileName(1));
        var metricsPath = Path.Combine(policyCheckpointDirectory, "dpo_step_000001.json");
        IReadOnlyList<int[]>? initialPolicyBits = null;

        var provider = new DesignConversionDpoPolicyBaseTestProvider(modelSet =>
        {
            initialPolicyBits = SnapshotDpoModelParameterBits(modelSet.Policy);
            return OperationResult<DesignConversionDpoPolicyBaseModelSet>.Ok(modelSet with
            {
                Identity = modelSet.Identity with
                {
                    ConfigurationKind = historicalConfigurationKind
                }
            });
        });

        try
        {
            var result = await DesignConversionDpoPolicyCheckpointBridge.ExecuteAndConvertAsync(
                DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                [BuildDpoHandoffRecord()],
                outputDirectory,
                runId,
                runtimeCheckpointPath,
                provider);

            Assert.False(result.IsSuccess);
            Assert.Equal("DESIGN_CONVERSION_DPO_POLICY_BASE_IDENTITY_INVALID", result.ErrorCode);
            Assert.Null(result.Value);
            Assert.Equal(1, provider.InvocationCount);
            Assert.NotNull(initialPolicyBits);
            Assert.NotNull(provider.LastModelSet);
            var currentPolicyBits = SnapshotDpoModelParameterBits(provider.LastModelSet.Policy);
            Assert.Equal(initialPolicyBits.Count, currentPolicyBits.Count);
            for (var index = 0; index < initialPolicyBits.Count; index++)
                Assert.Equal(initialPolicyBits[index], currentPolicyBits[index]);

            Assert.False(Directory.Exists(outputDirectory));
            Assert.False(Directory.Exists(policyCheckpointDirectory));
            Assert.False(File.Exists(policyCheckpointPath));
            Assert.False(File.Exists(metricsPath));
            Assert.False(File.Exists(runtimeCheckpointPath));
        }
        finally
        {
            if (Directory.Exists(outputDirectory))
                Directory.Delete(outputDirectory, recursive: true);
        }
    }

    [Fact]
    public async Task DpoTrainerPolicyCheckpointBridge_MissingProviderFailsBeforeArtifacts()
    {
        var outputDirectory = Path.Combine(
            Path.GetTempPath(),
            $"design-conversion-dpo-provider-required-{Guid.NewGuid():N}");
        var runtimeCheckpointPath = Path.Combine(outputDirectory, "runtime.json");

        var result = await DesignConversionDpoPolicyCheckpointBridge.ExecuteAndConvertAsync(
            DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
            [BuildDpoHandoffRecord()],
            outputDirectory,
            "provider-required",
            runtimeCheckpointPath,
            policyBaseProvider: null);

        Assert.False(result.IsSuccess);
        Assert.Equal("DESIGN_CONVERSION_DPO_POLICY_BASE_PROVIDER_REQUIRED", result.ErrorCode);
        Assert.False(Directory.Exists(outputDirectory));
    }

    [Fact]
    public async Task DpoTrainerPolicyCheckpointBridge_PreCancelledTokenShortCircuitsBeforeAuthorityOrProviderWithoutArtifacts()
    {
        var outputDirectory = Path.Combine(
            Path.GetTempPath(),
            $"design-conversion-dpo-provider-pre-cancelled-{Guid.NewGuid():N}");
        var runtimeCheckpointPath = Path.Combine(outputDirectory, "runtime.json");
        using var cancellationSource = new CancellationTokenSource();
        cancellationSource.Cancel();
        var provider = new DesignConversionDpoPolicyBaseTestProvider();

        try
        {
            var result = await DesignConversionDpoPolicyCheckpointBridge.ExecuteAndConvertAsync(
                null!,
                [BuildDpoHandoffRecord()],
                outputDirectory,
                "provider-pre-cancelled",
                runtimeCheckpointPath,
                provider,
                options: null,
                ct: cancellationSource.Token);

            Assert.False(result.IsSuccess);
            Assert.Equal("CANCELLED", result.ErrorCode);
            Assert.Equal("DesignConversion DpoTrainer policy bridge cancelled.", result.ErrorMessage);
            Assert.Equal(0, provider.InvocationCount);
            Assert.Null(provider.LastModelSet);
            Assert.False(Directory.Exists(outputDirectory));
            Assert.False(File.Exists(runtimeCheckpointPath));
        }
        finally
        {
            if (Directory.Exists(outputDirectory))
                Directory.Delete(outputDirectory, recursive: true);
        }
    }

    [Fact]
    public async Task DpoTrainerPolicyCheckpointBridge_MissingTeacherProvenanceFailsBeforeProviderOrArtifacts()
    {
        var outputDirectory = Path.Combine(
            Path.GetTempPath(),
            $"design-conversion-dpo-missing-teacher-provenance-{Guid.NewGuid():N}");
        var runtimeCheckpointPath = Path.Combine(outputDirectory, "runtime.json");
        var provider = new DesignConversionDpoPolicyBaseTestProvider();

        try
        {
            var result = await DesignConversionDpoPolicyCheckpointBridge.ExecuteAndConvertAsync(
                DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                [BuildDpoHandoffRecord() with { TeacherProvenance = null }],
                outputDirectory,
                "missing-teacher-provenance",
                runtimeCheckpointPath,
                provider);

            Assert.False(result.IsSuccess);
            Assert.Equal(
                LegalTrainingTeacherAdmissionAuthority.ProvenanceRequiredErrorCode,
                result.ErrorCode);
            Assert.Equal(0, provider.InvocationCount);
            Assert.Null(provider.LastModelSet);
            Assert.Null(result.Value);
            Assert.False(Directory.Exists(outputDirectory));
            Assert.False(File.Exists(runtimeCheckpointPath));
        }
        finally
        {
            if (Directory.Exists(outputDirectory))
                Directory.Delete(outputDirectory, recursive: true);
        }
    }

    [Fact]
    public async Task DpoTrainerPolicyCheckpointBridge_ServerAuthorityRejectsNonAllowlistedQwenBeforeProviderOrArtifacts()
    {
        const string privateRequest = "PRIVATE-POLICY-BRIDGE-PUBLIC-TEACHER-REQUEST";
        const string privateModel = "qwen3.5-private:9b";
        var privateVersion = new string('b', 64);
        var outputDirectory = Path.Combine(
            Path.GetTempPath(),
            $"design-conversion-dpo-public-external-teacher-{Guid.NewGuid():N}");
        var runtimeCheckpointPath = Path.Combine(outputDirectory, "runtime.json");
        var provider = new DesignConversionDpoPolicyBaseTestProvider();
        var record = WithOpenTeacherPairAuthorship(BuildDpoHandoffRecord() with
        {
            TeacherProvenance = BuildExternalQwenTeacherProvenance(
                privateRequest,
                privateModel,
                privateVersion)
        });

        try
        {
            var result = await DesignConversionDpoPolicyCheckpointBridge.ExecuteAndConvertAsync(
                DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                [record],
                outputDirectory,
                "public-external-teacher-rejected",
                runtimeCheckpointPath,
                provider);

            Assert.False(result.IsSuccess);
            Assert.Equal(
                LegalTrainingTeacherAdmissionAuthority.NotAllowlistedErrorCode,
                result.ErrorCode);
            Assert.Equal(0, provider.InvocationCount);
            Assert.Null(provider.LastModelSet);
            Assert.Null(result.Value);
            Assert.False(Directory.Exists(outputDirectory));
            Assert.False(File.Exists(runtimeCheckpointPath));
            var serialized = JsonSerializer.Serialize(result);
            Assert.DoesNotContain(privateRequest, serialized, StringComparison.Ordinal);
            Assert.DoesNotContain(privateModel, serialized, StringComparison.Ordinal);
            Assert.DoesNotContain(privateVersion, serialized, StringComparison.Ordinal);
        }
        finally
        {
            if (Directory.Exists(outputDirectory))
                Directory.Delete(outputDirectory, recursive: true);
        }
    }

    [Fact]
    public async Task DpoTrainerPolicyCheckpointBridge_ClosedProprietaryTeacherDenialPrecedesAllowlistFailure()
    {
        var outputDirectory = Path.Combine(
            Path.GetTempPath(),
            $"design-conversion-dpo-closed-teacher-{Guid.NewGuid():N}");
        var runtimeCheckpointPath = Path.Combine(outputDirectory, "runtime.json");
        var provider = new DesignConversionDpoPolicyBaseTestProvider();
        var record = BuildDpoHandoffRecord() with
        {
            TeacherProvenance = BuildExternalQwenTeacherProvenance(
                "closed-policy-bridge-request",
                "qwen3.5-9b",
                new string('c', 64),
                closedProprietaryModel: true)
        };

        try
        {
            var result = await DesignConversionDpoPolicyCheckpointBridge.ExecuteAndConvertAsync(
                DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                [record],
                outputDirectory,
                "closed-teacher-rejected",
                runtimeCheckpointPath,
                provider);

            Assert.False(result.IsSuccess);
            Assert.Equal(
                LegalTrainingTeacherAdmissionAuthority.ClosedProprietaryDeniedErrorCode,
                result.ErrorCode);
            Assert.NotEqual(
                LegalTrainingTeacherAdmissionAuthority.NotAllowlistedErrorCode,
                result.ErrorCode);
            Assert.Equal(0, provider.InvocationCount);
            Assert.Null(provider.LastModelSet);
            Assert.Null(result.Value);
            Assert.False(Directory.Exists(outputDirectory));
            Assert.False(File.Exists(runtimeCheckpointPath));
        }
        finally
        {
            if (Directory.Exists(outputDirectory))
                Directory.Delete(outputDirectory, recursive: true);
        }
    }

    [Fact]
    public async Task DpoTrainerPolicyCheckpointBridge_NullAuthorityFailsBeforeOptionsRecordsProviderOrArtifacts()
    {
        var outputDirectory = Path.Combine(
            Path.GetTempPath(),
            $"design-conversion-dpo-null-teacher-authority-{Guid.NewGuid():N}");
        var runtimeCheckpointPath = Path.Combine(outputDirectory, "runtime.json");
        var provider = new DesignConversionDpoPolicyBaseTestProvider();

        try
        {
            var result = await DesignConversionDpoPolicyCheckpointBridge.ExecuteAndConvertAsync(
                null!,
                new ThrowOnAccessDpoTrainingRecordList(),
                outputDirectory,
                "null-teacher-authority",
                runtimeCheckpointPath,
                provider,
                new DesignConversionDpoPolicyCheckpointBridgeOptions(0, 0));

            Assert.False(result.IsSuccess);
            Assert.Equal(
                LegalTrainingTeacherAdmissionAuthority.ConfigurationRequiredErrorCode,
                result.ErrorCode);
            Assert.Equal(0, provider.InvocationCount);
            Assert.Null(provider.LastModelSet);
            Assert.Null(result.Value);
            Assert.False(Directory.Exists(outputDirectory));
            Assert.False(File.Exists(runtimeCheckpointPath));
        }
        finally
        {
            if (Directory.Exists(outputDirectory))
                Directory.Delete(outputDirectory, recursive: true);
        }
    }

    [Fact]
    public async Task DpoTrainerPolicyCheckpointBridge_ServerCapabilityAllowlistedQwenReachesProviderOnceWithoutIdentityLeakOrArtifacts()
    {
        const string privateRequest = "PRIVATE-POLICY-BRIDGE-ALLOWLISTED-REQUEST";
        const string privateModel = "qwen3.5-private:9b";
        var privateVersion = new string('c', 64);
        const string privateProviderFailure = "C:/private/provider/PRIVATE-POLICY-BRIDGE-PROVIDER-FAILURE";
        var authority = CreateServerOwnedQwenOllamaAuthority(
            privateModel,
            privateVersion,
            maxTeacherCalls: 1);
        var outputDirectory = Path.Combine(
            Path.GetTempPath(),
            $"design-conversion-dpo-allowlisted-teacher-{Guid.NewGuid():N}");
        var runtimeCheckpointPath = Path.Combine(outputDirectory, "runtime.json");
        var provider = new DesignConversionDpoPolicyBaseTestProvider(_ =>
            OperationResult<DesignConversionDpoPolicyBaseModelSet>.Fail(
                privateProviderFailure,
                "PRIVATE_POLICY_BRIDGE_PROVIDER_FAILURE"));
        var record = WithOpenTeacherPairAuthorship(BuildDpoHandoffRecord() with
        {
            TeacherProvenance = BuildExternalQwenTeacherProvenance(
                privateRequest,
                privateModel,
                privateVersion)
        });

        try
        {
            var result = await DesignConversionDpoPolicyCheckpointBridge.ExecuteAndConvertAsync(
                authority,
                [record],
                outputDirectory,
                "allowlisted-external-teacher",
                runtimeCheckpointPath,
                provider,
                options: null);

            Assert.False(result.IsSuccess);
            Assert.Equal("DESIGN_CONVERSION_DPO_POLICY_BASE_PROVIDER_FAILED", result.ErrorCode);
            Assert.Equal(1, provider.InvocationCount);
            Assert.NotNull(provider.LastModelSet);
            Assert.Null(result.Value);
            Assert.False(Directory.Exists(outputDirectory));
            Assert.False(File.Exists(runtimeCheckpointPath));
            var serialized = JsonSerializer.Serialize(result);
            foreach (var rawIdentity in new[]
                     {
                         privateRequest,
                         privateModel,
                         privateVersion,
                         privateProviderFailure
                     })
            {
                Assert.DoesNotContain(rawIdentity, serialized, StringComparison.Ordinal);
            }
        }
        finally
        {
            if (Directory.Exists(outputDirectory))
                Directory.Delete(outputDirectory, recursive: true);
        }
    }

    [Fact]
    public async Task DpoTrainerPolicyCheckpointBridge_ServerCapabilityPersistsSanitizedAllowlistedTeacherEvidence()
    {
        const string privateRequest = "PRIVATE-POLICY-BRIDGE-PERSISTENCE-REQUEST";
        const string privateModel = "qwen3-private-persistence-model:test";
        var privateVersion = new string('c', 64);
        var localRetentionPolicyRef = $"sha256:{new string('a', 64)}";
        var outputLicenseRef = $"sha256:{new string('b', 64)}";
        var authority = CreateServerOwnedQwenOllamaAuthority(
            privateModel,
            privateVersion,
            maxTeacherCalls: 1);

        var outputDirectory = Path.Combine(
            Path.GetTempPath(),
            $"design-conversion-dpo-persisted-external-teacher-{Guid.NewGuid():N}");
        var runtimeCheckpointPath = Path.Combine(outputDirectory, "runtime.json");
        var provider = new DesignConversionDpoPolicyBaseTestProvider();
        var record = WithOpenTeacherPairAuthorship(BuildDpoHandoffRecord() with
        {
            TeacherProvenance = BuildExternalQwenTeacherProvenance(
                privateRequest,
                privateModel,
                privateVersion)
        });

        try
        {
            var result = await DesignConversionDpoPolicyCheckpointBridge.ExecuteAndConvertAsync(
                authority,
                [record],
                outputDirectory,
                "allowlisted-external-teacher-persistence",
                runtimeCheckpointPath,
                provider,
                new DesignConversionDpoPolicyCheckpointBridgeOptions());

            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.Equal(1, provider.InvocationCount);
            Assert.NotNull(provider.LastModelSet);
            Assert.True(File.Exists(runtimeCheckpointPath));
            Assert.True(File.Exists(Path.Combine(outputDirectory, result.Value!.PolicyCheckpointPath)));
            var typedRef = Assert.IsType<DesignConversionDpoPolicyCheckpointRef>(
                result.Value.PolicyCheckpointRef);

            var evidenceSets = new IReadOnlyList<string>[]
            {
                result.Value.EvidenceRefs,
                result.Value.RuntimeCheckpoint.EvidenceRefs,
                typedRef.EvidenceRefs
            };
            Assert.All(
                evidenceSets,
                evidenceRefs =>
                {
                    Assert.Contains(
                        "teacher_provenance:source_kind=ExternalTeacherOutput",
                        evidenceRefs);
                    Assert.Contains(
                        "legal_teacher_admission:external_invocation_count=1",
                        evidenceRefs);
                    Assert.Contains(
                        evidenceRefs,
                        evidence => evidence.StartsWith(
                            "teacher_provenance:request_ref=sha256:",
                            StringComparison.Ordinal));
                    Assert.Contains(
                        evidenceRefs,
                        evidence => evidence.StartsWith(
                            "teacher_provenance:provider_ref=sha256:",
                            StringComparison.Ordinal));
                    Assert.Contains(
                        evidenceRefs,
                        evidence => evidence.StartsWith(
                            "teacher_provenance:model_ref=sha256:",
                            StringComparison.Ordinal));
                    Assert.Contains(
                        evidenceRefs,
                        evidence => evidence.StartsWith(
                            "teacher_provenance:model_version_ref=sha256:",
                            StringComparison.Ordinal));
                    Assert.Contains(
                        evidenceRefs,
                        evidence => evidence.StartsWith(
                            "teacher_provenance:license_ref=sha256:",
                            StringComparison.Ordinal));
                    Assert.Contains(
                        $"teacher_legal_evidence:local_retention_policy_ref={localRetentionPolicyRef}",
                        evidenceRefs);
                    Assert.Contains(
                        $"teacher_legal_evidence:output_license_ref={outputLicenseRef}",
                        evidenceRefs);
                });

            var serializedResult = JsonSerializer.Serialize(result.Value);
            var persistedRuntimeCheckpointJson = await File.ReadAllTextAsync(runtimeCheckpointPath);
            foreach (var rawIdentity in new[] { privateRequest, privateModel, privateVersion })
            {
                Assert.DoesNotContain(rawIdentity, serializedResult, StringComparison.Ordinal);
                Assert.DoesNotContain(rawIdentity, persistedRuntimeCheckpointJson, StringComparison.Ordinal);
            }
            Assert.Contains("sha256:", serializedResult, StringComparison.Ordinal);
            Assert.Contains("sha256:", persistedRuntimeCheckpointJson, StringComparison.Ordinal);
        }
        finally
        {
            if (Directory.Exists(outputDirectory))
                Directory.Delete(outputDirectory, recursive: true);
        }
    }

    [Fact]
    public async Task DpoTrainerPolicyCheckpointBridge_AliasedIncompatibleAndUnequalBasesFailBeforeArtifacts()
    {
        var providers = new IDesignConversionDpoPolicyBaseProvider[]
        {
            new DesignConversionDpoPolicyBaseTestProvider(modelSet =>
                OperationResult<DesignConversionDpoPolicyBaseModelSet>.Ok(
                    modelSet with { Reference = modelSet.Policy })),
            new DesignConversionDpoPolicyBaseTestProvider(modelSet =>
                OperationResult<DesignConversionDpoPolicyBaseModelSet>.Ok(
                    modelSet with
                    {
                        Reference = new LlmModel(modelSet.Policy.Config with
                        {
                            ContextLen = modelSet.Policy.Config.ContextLen + 1
                        })
                    })),
            new DesignConversionDpoPolicyBaseTestProvider(modelSet =>
            {
                var referenceEntry = modelSet.Reference.GetCheckpointParameterEntries()[0];
                referenceEntry.Parameter.Value.Data[0] = MathF.BitIncrement(
                    referenceEntry.Parameter.Value.Data[0]);
                return OperationResult<DesignConversionDpoPolicyBaseModelSet>.Ok(modelSet);
            })
        };

        for (var index = 0; index < providers.Length; index++)
        {
            var outputDirectory = Path.Combine(
                Path.GetTempPath(),
                $"design-conversion-dpo-invalid-model-set-{index}-{Guid.NewGuid():N}");
            var result = await DesignConversionDpoPolicyCheckpointBridge.ExecuteAndConvertAsync(
                DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                [BuildDpoHandoffRecord()],
                outputDirectory,
                $"invalid-model-set-{index}",
                Path.Combine(outputDirectory, "runtime.json"),
                providers[index]);

            Assert.False(result.IsSuccess);
            Assert.Equal("DESIGN_CONVERSION_DPO_POLICY_BASE_MODEL_SET_INVALID", result.ErrorCode);
            Assert.False(Directory.Exists(outputDirectory));
        }
    }

    [Fact]
    public async Task DpoTrainerPolicyCheckpointBridge_PublicModelIdentityCrossPlatform_WindowsDriveValueFailsBeforeArtifacts()
    {
        // Путь Windows обязан быть распознан даже тогда, когда серверный код исполняется в Linux.
        const string privateWindowsPath = "C:/private/model";
        var outputDirectory = Path.Combine(
            Path.GetTempPath(),
            $"design-conversion-dpo-private-model-identity-{Guid.NewGuid():N}");
        var provider = new DesignConversionDpoPolicyBaseTestProvider(modelSet =>
            OperationResult<DesignConversionDpoPolicyBaseModelSet>.Ok(modelSet with
            {
                Identity = modelSet.Identity with { BaseModelId = privateWindowsPath }
            }));

        try
        {
            // Серверная передача должна остановиться на проверке личности до записи checkpoint.
            var result = await DesignConversionDpoPolicyCheckpointBridge.ExecuteAndConvertAsync(
                DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                [BuildDpoHandoffRecord()],
                outputDirectory,
                "private-model-identity",
                Path.Combine(outputDirectory, "runtime.json"),
                provider);

            // Ошибка не раскрывает приватное значение и не оставляет частичных артефактов.
            Assert.False(result.IsSuccess);
            Assert.Equal("DESIGN_CONVERSION_DPO_POLICY_BASE_IDENTITY_INVALID", result.ErrorCode);
            Assert.DoesNotContain(privateWindowsPath, result.ErrorMessage, StringComparison.Ordinal);
            Assert.False(Directory.Exists(outputDirectory));
        }
        finally
        {
            // Уборка защищает соседние тесты даже при падении утверждения до исправления кода.
            if (Directory.Exists(outputDirectory))
                Directory.Delete(outputDirectory, recursive: true);
        }
    }

    [Fact]
    public async Task DpoTrainerPolicyCheckpointBridge_ProviderFailuresAreStableSanitizedAndSideEffectFree()
    {
        const string privateSentinel = "C:/private/provider/model.bin PRIVATE_PROVIDER_FAILURE_SENTINEL";
        var providers = new (IDesignConversionDpoPolicyBaseProvider Provider, string ErrorCode)[]
        {
            (new DesignConversionDpoPolicyBaseTestProvider(_ =>
                throw new InvalidOperationException(privateSentinel)),
                "DESIGN_CONVERSION_DPO_POLICY_BASE_PROVIDER_FAILED"),
            (new DesignConversionDpoPolicyBaseTestProvider(_ =>
                OperationResult<DesignConversionDpoPolicyBaseModelSet>.Fail(
                    privateSentinel,
                    "PRIVATE_PROVIDER_CODE")),
                "DESIGN_CONVERSION_DPO_POLICY_BASE_PROVIDER_FAILED"),
            (new DesignConversionDpoPolicyBaseTestProvider(_ =>
                OperationResult<DesignConversionDpoPolicyBaseModelSet>.Ok(null!)),
                "DESIGN_CONVERSION_DPO_POLICY_BASE_MODEL_SET_INVALID"),
            (new DesignConversionDpoPolicyBaseTestProvider(modelSet =>
                OperationResult<DesignConversionDpoPolicyBaseModelSet>.Ok(modelSet with
                {
                    Identity = modelSet.Identity with { BaseModelId = "C:/private/model" }
                })),
                "DESIGN_CONVERSION_DPO_POLICY_BASE_IDENTITY_INVALID")
        };

        for (var index = 0; index < providers.Length; index++)
        {
            var outputDirectory = Path.Combine(
                Path.GetTempPath(),
                $"design-conversion-dpo-provider-failure-{index}-{Guid.NewGuid():N}");
            var result = await DesignConversionDpoPolicyCheckpointBridge.ExecuteAndConvertAsync(
                DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                [BuildDpoHandoffRecord()],
                outputDirectory,
                $"provider-failure-{index}",
                Path.Combine(outputDirectory, "runtime.json"),
                providers[index].Provider);

            Assert.False(result.IsSuccess);
            Assert.Equal(providers[index].ErrorCode, result.ErrorCode);
            Assert.DoesNotContain(privateSentinel, result.ErrorMessage, StringComparison.Ordinal);
            Assert.DoesNotContain("C:/private", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
            Assert.False(Directory.Exists(outputDirectory));
        }
    }

    [Fact]
    public async Task DpoTrainingHandoff_ProviderPermitDrainsBeforeRevocationAndNextBoundaryIsDenied()
    {
        var harness = DesignConversionLegalTeacherCapabilityTestFactory.CreateRevocable();
        var provider = new BlockingDpoPolicyBaseProvider();
        var checkpointDirectory = Path.Combine(
            Path.GetTempPath(),
            $"handoff-provider-permit-drain-{Guid.NewGuid():N}");

        try
        {
            var execution = DesignConversionDpoTrainingHandoff.ExecuteAsync(
                harness.Authority,
                [BuildDpoHandoffRecord()],
                provider,
                BuildDpoHandoffTokenizer(),
                checkpointDirectory,
                new LoraConfig
                {
                    Rank = 2,
                    Alpha = 4,
                    Dropout = 0,
                    TargetModules = ["q_proj", "v_proj"]
                });
            await provider.Entered.WaitAsync(TimeSpan.FromSeconds(10));

            var revocation = harness.BeginRevocationAsync();
            Assert.False(revocation.IsCompleted);
            provider.Release();

            var result = await execution.WaitAsync(TimeSpan.FromSeconds(20));
            await revocation.WaitAsync(TimeSpan.FromSeconds(10));

            Assert.False(result.IsSuccess);
            Assert.Equal(
                ServerOwnedLegalTrainingTeacherAdmissionAuthority.AdmissionRevokedErrorCode,
                result.ErrorCode);
            Assert.Equal(1, provider.InvocationCount);
            Assert.NotNull(provider.LastModelSet);
            Assert.False(Directory.Exists(checkpointDirectory));
        }
        finally
        {
            provider.Release();
            if (Directory.Exists(checkpointDirectory))
                Directory.Delete(checkpointDirectory, recursive: true);
        }
    }

    [Fact]
    public async Task DpoTrainingHandoff_RevocationWinsOverCancellationAtPostProviderBoundary()
    {
        var harness = DesignConversionLegalTeacherCapabilityTestFactory.CreateRevocable();
        var provider = new BlockingDpoPolicyBaseProvider();
        using var cancellation = new CancellationTokenSource();
        var checkpointDirectory = Path.Combine(
            Path.GetTempPath(),
            $"handoff-revoke-cancel-precedence-{Guid.NewGuid():N}");

        try
        {
            var execution = DesignConversionDpoTrainingHandoff.ExecuteAsync(
                harness.Authority,
                [BuildDpoHandoffRecord()],
                provider,
                BuildDpoHandoffTokenizer(),
                checkpointDirectory,
                new LoraConfig
                {
                    Rank = 2,
                    Alpha = 4,
                    Dropout = 0,
                    TargetModules = ["q_proj", "v_proj"]
                },
                cancellationToken: cancellation.Token);
            await provider.Entered.WaitAsync(TimeSpan.FromSeconds(10));

            cancellation.Cancel();
            var revocation = harness.BeginRevocationAsync();
            provider.Release();

            var result = await execution.WaitAsync(TimeSpan.FromSeconds(20));
            await revocation.WaitAsync(TimeSpan.FromSeconds(10));

            Assert.False(result.IsSuccess);
            Assert.Equal(
                ServerOwnedLegalTrainingTeacherAdmissionAuthority.AdmissionRevokedErrorCode,
                result.ErrorCode);
            Assert.False(Directory.Exists(checkpointDirectory));
        }
        finally
        {
            provider.Release();
            if (Directory.Exists(checkpointDirectory))
                Directory.Delete(checkpointDirectory, recursive: true);
        }
    }

    [Fact]
    public async Task DpoTrainingHandoff_RevocationAtNoTrainingSuccessBoundaryDeniesSuccessWithoutProviderEffects()
    {
        var harness = DesignConversionLegalTeacherCapabilityTestFactory.CreateRevocable();
        var source = BuildDpoHandoffRecord();
        var lowDelta = source with
        {
            Chosen = source.Chosen with
            {
                Verdict = CreateVerdict("quality-revoked-chosen", 0.52, passed: true)
            },
            Rejected = source.Rejected with
            {
                Verdict = CreateVerdict("quality-revoked-rejected", 0.49, passed: false)
            }
        };
        var provider = new DesignConversionDpoPolicyBaseTestProvider();
        var checkpointDirectory = Path.Combine(
            Path.GetTempPath(),
            $"handoff-quality-final-revoked-{Guid.NewGuid():N}");
        DesignConversionDpoTrainingHandoff.MutationBoundaryObserverForTesting = boundary =>
        {
            if (string.Equals(boundary, "execute_quality_final", StringComparison.Ordinal))
                harness.LoseAdmissionLive();
        };

        try
        {
            var result = await DesignConversionDpoTrainingHandoff.ExecuteAsync(
                harness.Authority,
                [lowDelta],
                provider,
                BuildDpoHandoffTokenizer(),
                checkpointDirectory);

            Assert.False(result.IsSuccess);
            Assert.Equal(
                ServerOwnedLegalTrainingTeacherAdmissionAuthority.AdmissionRevokedErrorCode,
                result.ErrorCode);
            Assert.Equal(0, provider.InvocationCount);
            Assert.False(Directory.Exists(checkpointDirectory));
        }
        finally
        {
            DesignConversionDpoTrainingHandoff.MutationBoundaryObserverForTesting = null;
            if (Directory.Exists(checkpointDirectory))
                Directory.Delete(checkpointDirectory, recursive: true);
        }
    }

    [Theory]
    [InlineData("execute_policy_result", false, false)]
    [InlineData("execute_logical", false, false)]
    [InlineData("execute_final", true, true)]
    public async Task DpoTrainingHandoff_RevocationAtExecuteMutationBoundaryPreventsLaterEffects(
        string deniedBoundary,
        bool expectedCheckpoint,
        bool expectedAdapters)
    {
        var harness = DesignConversionLegalTeacherCapabilityTestFactory.CreateRevocable();
        IReadOnlyList<int[]>? initialPolicyBaseBits = null;
        IReadOnlyList<int[]>? initialReferenceBaseBits = null;
        IReadOnlyList<int[]>? initialFreshLoadBaseBits = null;
        var provider = new DesignConversionDpoPolicyBaseTestProvider(modelSet =>
        {
            initialPolicyBaseBits = SnapshotDpoBaseParameterBits(modelSet.Policy);
            initialReferenceBaseBits = SnapshotDpoBaseParameterBits(modelSet.Reference);
            initialFreshLoadBaseBits = SnapshotDpoBaseParameterBits(modelSet.FreshLoadTarget);
            return OperationResult<DesignConversionDpoPolicyBaseModelSet>.Ok(modelSet);
        });
        var checkpointDirectory = Path.Combine(
            Path.GetTempPath(),
            $"handoff-execute-boundary-{deniedBoundary}-{Guid.NewGuid():N}");
        DesignConversionDpoTrainingHandoff.MutationBoundaryObserverForTesting = boundary =>
        {
            if (string.Equals(boundary, deniedBoundary, StringComparison.Ordinal))
                harness.LoseAdmissionLive();
        };

        try
        {
            var result = await DesignConversionDpoTrainingHandoff.ExecuteAsync(
                harness.Authority,
                [BuildDpoHandoffRecord()],
                provider,
                BuildDpoHandoffTokenizer(),
                checkpointDirectory,
                new LoraConfig
                {
                    Rank = 2,
                    Alpha = 4,
                    Dropout = 0,
                    TargetModules = ["q_proj", "v_proj"]
                });

            Assert.False(result.IsSuccess);
            Assert.Equal(
                ServerOwnedLegalTrainingTeacherAdmissionAuthority.AdmissionRevokedErrorCode,
                result.ErrorCode);
            Assert.Equal(1, provider.InvocationCount);
            Assert.Equal(
                expectedCheckpoint,
                File.Exists(Path.Combine(
                    checkpointDirectory,
                    DpoTrainer.BuildPolicyCheckpointFileName(1))));
            var modelSet = Assert.IsType<DesignConversionDpoPolicyBaseModelSet>(
                provider.LastModelSet);
            Assert.NotNull(initialPolicyBaseBits);
            Assert.NotNull(initialReferenceBaseBits);
            Assert.NotNull(initialFreshLoadBaseBits);
            AssertDpoBaseParameterBitsEqual(initialPolicyBaseBits, modelSet.Policy);
            AssertDpoBaseParameterBitsEqual(initialReferenceBaseBits, modelSet.Reference);
            AssertDpoBaseParameterBitsEqual(initialFreshLoadBaseBits, modelSet.FreshLoadTarget);
            Assert.Equal(expectedAdapters, HasDpoTargetAdapters(modelSet.Policy));
            Assert.Equal(expectedAdapters, HasDpoTargetAdapters(modelSet.Reference));
            Assert.False(HasDpoTargetAdapters(modelSet.FreshLoadTarget));
        }
        finally
        {
            DesignConversionDpoTrainingHandoff.MutationBoundaryObserverForTesting = null;
            if (Directory.Exists(checkpointDirectory))
                Directory.Delete(checkpointDirectory, recursive: true);
        }
    }

    [Theory]
    [InlineData("policy_provider_and_validation", 0, false, false, false)]
    [InlineData("convert_policy_result", 1, false, false, false)]
    [InlineData("convert_lora", 1, false, false, false)]
    [InlineData("convert_fresh_load", 1, false, false, true)]
    [InlineData("convert_fresh_load_result", 1, true, false, true)]
    [InlineData("convert_train", 1, true, false, true)]
    [InlineData("convert_train_result", 1, true, false, true)]
    [InlineData("convert_publish", 1, true, false, true)]
    [InlineData("convert_final", 1, true, true, true)]
    public async Task DpoTrainingHandoff_RevocationAtConvertMutationBoundaryPreventsLaterEffects(
        string deniedBoundary,
        int expectedProviderInvocations,
        bool expectedFreshLoad,
        bool expectedPublishedRuntime,
        bool expectedFreshLoadTargetAdapters)
    {
        var harness = DesignConversionLegalTeacherCapabilityTestFactory.CreateRevocable();
        var root = Path.Combine(
            Path.GetTempPath(),
            $"handoff-boundary-{deniedBoundary}-{Guid.NewGuid():N}");
        var policyPath = Path.Combine(root, DpoTrainer.BuildPolicyCheckpointFileName(1));
        var runtimePath = Path.Combine(root, "runtime", "checkpoint.json");
        IReadOnlyList<int[]>? initialFreshLoadBaseBits = null;
        var provider = new DesignConversionDpoPolicyBaseTestProvider(modelSet =>
        {
            initialFreshLoadBaseBits = SnapshotDpoBaseParameterBits(
                modelSet.FreshLoadTarget);
            return OperationResult<DesignConversionDpoPolicyBaseModelSet>.Ok(modelSet);
        });
        var lora = new DesignConversionLoraCheckpointConfiguration(new LoraConfig
        {
            Rank = 2,
            Alpha = 4,
            Dropout = 0,
            TargetModules = ["q_proj", "v_proj"]
        });
        var freshLoadObserved = false;

        try
        {
            Directory.CreateDirectory(root);
            var source = new LlmModel(DesignConversionDpoPolicyBaseTestProvider.CreateConfig());
            source.BaseParameters[0].Value.Data[0] = 123.5f;
            _ = LoraApplicator.Apply(source, lora.ToLoraConfig());
            source.SaveCheckpoint(policyPath);
            DesignConversionDpoTrainingHandoff.MutationBoundaryObserverForTesting = boundary =>
            {
                if (string.Equals(boundary, deniedBoundary, StringComparison.Ordinal))
                    harness.LoseAdmissionLive();
            };
            DesignConversionDpoTrainingHandoff.PolicyCheckpointFreshLoadObserverForTesting =
                () => freshLoadObserved = true;

            var result = await DesignConversionDpoTrainingHandoff
                .ConvertPolicyCheckpointToRuntimeCheckpointAsync(
                    harness.Authority,
                    BuildDpoHandoffRecord(),
                    policyPath,
                    provider,
                    lora,
                    runtimePath);

            Assert.False(result.IsSuccess);
            Assert.Equal(
                ServerOwnedLegalTrainingTeacherAdmissionAuthority.AdmissionRevokedErrorCode,
                result.ErrorCode);
            Assert.Equal(expectedProviderInvocations, provider.InvocationCount);
            Assert.Equal(expectedFreshLoad, freshLoadObserved);
            Assert.Equal(expectedPublishedRuntime, File.Exists(runtimePath));
            if (expectedProviderInvocations == 1)
            {
                var modelSet = Assert.IsType<DesignConversionDpoPolicyBaseModelSet>(
                    provider.LastModelSet);
                Assert.NotNull(initialFreshLoadBaseBits);
                Assert.NotEqual(
                    BitConverter.SingleToInt32Bits(123.5f),
                    initialFreshLoadBaseBits[0][0]);
                Assert.Equal(
                    expectedFreshLoadTargetAdapters,
                    HasDpoTargetAdapters(modelSet.FreshLoadTarget));
                Assert.False(HasDpoTargetAdapters(modelSet.Policy));
                Assert.False(HasDpoTargetAdapters(modelSet.Reference));
                if (expectedFreshLoad)
                {
                    Assert.Equal(
                        BitConverter.SingleToInt32Bits(123.5f),
                        BitConverter.SingleToInt32Bits(
                            modelSet.FreshLoadTarget.BaseParameters[0].Value.Data[0]));
                }
                else
                {
                    AssertDpoBaseParameterBitsEqual(
                        initialFreshLoadBaseBits,
                        modelSet.FreshLoadTarget);
                }
            }
            if (!expectedPublishedRuntime)
            {
                Assert.False(Directory.Exists(Path.GetDirectoryName(runtimePath)!) &&
                             Directory.EnumerateFiles(
                                 Path.GetDirectoryName(runtimePath)!,
                                 "*.tmp",
                                 SearchOption.TopDirectoryOnly).Any());
            }
        }
        finally
        {
            DesignConversionDpoTrainingHandoff.MutationBoundaryObserverForTesting = null;
            DesignConversionDpoTrainingHandoff.PolicyCheckpointFreshLoadObserverForTesting = null;
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task DpoTrainingHandoff_RevocationWinsOverCancellationAfterFreshLoadPermitDrains()
    {
        var harness = DesignConversionLegalTeacherCapabilityTestFactory.CreateRevocable();
        using var cancellation = new CancellationTokenSource();
        var root = Path.Combine(
            Path.GetTempPath(),
            $"handoff-fresh-load-revoke-cancel-{Guid.NewGuid():N}");
        var policyPath = Path.Combine(root, DpoTrainer.BuildPolicyCheckpointFileName(1));
        var runtimePath = Path.Combine(root, "runtime", "checkpoint.json");
        var provider = new DesignConversionDpoPolicyBaseTestProvider();
        var lora = new DesignConversionLoraCheckpointConfiguration(new LoraConfig
        {
            Rank = 2,
            Alpha = 4,
            Dropout = 0,
            TargetModules = ["q_proj", "v_proj"]
        });
        Task? revocation = null;
        var revocationWasPending = false;

        try
        {
            Directory.CreateDirectory(root);
            var source = new LlmModel(DesignConversionDpoPolicyBaseTestProvider.CreateConfig());
            source.BaseParameters[0].Value.Data[0] = 123.5f;
            _ = LoraApplicator.Apply(source, lora.ToLoraConfig());
            source.SaveCheckpoint(policyPath);
            DesignConversionDpoTrainingHandoff.PolicyCheckpointFreshLoadObserverForTesting =
                () =>
                {
                    cancellation.Cancel();
                    revocation = harness.BeginRevocationAsync();
                    revocationWasPending = !revocation.IsCompleted;
                };

            var result = await DesignConversionDpoTrainingHandoff
                .ConvertPolicyCheckpointToRuntimeCheckpointAsync(
                    harness.Authority,
                    BuildDpoHandoffRecord(),
                    policyPath,
                    provider,
                    lora,
                    runtimePath,
                    cancellation.Token)
                .WaitAsync(TimeSpan.FromSeconds(20));
            Assert.NotNull(revocation);
            await revocation!.WaitAsync(TimeSpan.FromSeconds(10));

            Assert.True(revocationWasPending);
            Assert.False(result.IsSuccess);
            Assert.Equal(
                ServerOwnedLegalTrainingTeacherAdmissionAuthority.AdmissionRevokedErrorCode,
                result.ErrorCode);
            var modelSet = Assert.IsType<DesignConversionDpoPolicyBaseModelSet>(
                provider.LastModelSet);
            Assert.True(HasDpoTargetAdapters(modelSet.FreshLoadTarget));
            Assert.Equal(
                BitConverter.SingleToInt32Bits(123.5f),
                BitConverter.SingleToInt32Bits(
                    modelSet.FreshLoadTarget.BaseParameters[0].Value.Data[0]));
            Assert.False(File.Exists(runtimePath));
        }
        finally
        {
            DesignConversionDpoTrainingHandoff.PolicyCheckpointFreshLoadObserverForTesting = null;
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task DpoTrainingHandoff_DisposesLeaseAfterSuccessAndFailure()
    {
        ServerOwnedLegalTrainingTeacherAdmissionLease? capturedLease = null;
        var successCheckpointDirectory = Path.Combine(
            Path.GetTempPath(),
            $"handoff-lease-success-{Guid.NewGuid():N}");
        var failureCheckpointDirectory = Path.Combine(
            Path.GetTempPath(),
            $"handoff-lease-failure-{Guid.NewGuid():N}");
        DesignConversionDpoTrainingHandoff.TrainingLeaseObserverForTesting =
            lease => capturedLease = lease;
        try
        {
            var source = BuildDpoHandoffRecord();
            var lowDelta = source with
            {
                Chosen = source.Chosen with
                {
                    Verdict = CreateVerdict("lease-success-chosen", 0.52, passed: true)
                },
                Rejected = source.Rejected with
                {
                    Verdict = CreateVerdict("lease-success-rejected", 0.49, passed: false)
                }
            };
            var success = await DesignConversionDpoTrainingHandoff.ExecuteAsync(
                DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                [lowDelta],
                policyBaseProvider: null,
                tokenizer: BuildDpoHandoffTokenizer(),
                checkpointDirectory: successCheckpointDirectory);
            Assert.True(success.IsSuccess, success.ErrorMessage);
            Assert.NotNull(capturedLease);
            Assert.Equal(
                ServerOwnedLegalTrainingTeacherAdmissionAuthority.AdmissionRevokedErrorCode,
                capturedLease!.AcquireMutationPermit().ErrorCode);

            capturedLease = null;
            var failure = await DesignConversionDpoTrainingHandoff.ExecuteAsync(
                DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                [BuildDpoHandoffRecord()],
                policyBaseProvider: null,
                tokenizer: BuildDpoHandoffTokenizer(),
                checkpointDirectory: failureCheckpointDirectory);
            Assert.False(failure.IsSuccess);
            Assert.NotNull(capturedLease);
            Assert.Equal(
                ServerOwnedLegalTrainingTeacherAdmissionAuthority.AdmissionRevokedErrorCode,
                capturedLease!.AcquireMutationPermit().ErrorCode);
        }
        finally
        {
            DesignConversionDpoTrainingHandoff.TrainingLeaseObserverForTesting = null;
            if (Directory.Exists(successCheckpointDirectory))
                Directory.Delete(successCheckpointDirectory, recursive: true);
            if (Directory.Exists(failureCheckpointDirectory))
                Directory.Delete(failureCheckpointDirectory, recursive: true);
        }
    }

    [Fact]
    public async Task DpoTrainingHandoff_DeepSnapshotsCallerCollectionsBeforeAuthorityLease()
    {
        const string chosenOptimizerHtml = "<main data-handoff-side=\"alpha\">handoff-alpha</main>";
        const string chosenOptimizerCss = "[data-handoff-side=alpha]{display:grid;gap:19px}";
        const string rejectedOptimizerHtml = "<main data-handoff-side=\"beta\">handoff-beta</main>";
        const string rejectedOptimizerCss = "[data-handoff-side=beta]{display:block;gap:1px}";
        var vector = new List<double> { 0.25, -0.5, 0.75, 1.0 };
        var chosenEvidence = new[] { "owned-chosen-evidence" };
        var rejectedEvidence = new[] { "owned-rejected-evidence" };
        var chosenVerdictEvidence = new[] { "owned-chosen-verdict-evidence" };
        var rejectedVerdictEvidence = new[] { "owned-rejected-verdict-evidence" };
        var chosenDefectEvidence = new[] { "owned-chosen-defect-image" };
        var rejectedDefectEvidence = new[] { "owned-rejected-defect-image" };
        var provenanceMetadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["owned"] = "before"
        };
        var licenseMetadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["owned-license"] = "before"
        };
        var quarantineEvidence = new[] { "owned-quarantine-evidence" };
        var chosenQaMetrics = new Dictionary<string, double>(StringComparer.Ordinal)
        {
            ["owned-metric"] = 0.125
        };
        var chosenQaWarnings = new[] { "owned-warning" };
        var chosenExpectedMetadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["owned-expected"] = "before"
        };
        var chosenActualMetadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["owned-actual"] = "before"
        };
        var chosenQaVerdictEvidence = new[] { "owned-qa-verdict-evidence" };
        var chosenQaDefectEvidence = new[] { "owned-qa-defect-image" };
        var rejectedQaMetrics = new Dictionary<string, double>(StringComparer.Ordinal)
        {
            ["owned-rejected-metric"] = 0.875
        };
        var rejectedQaWarnings = new[] { "owned-rejected-warning" };
        var rejectedExpectedMetadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["owned-rejected-expected"] = "before"
        };
        var rejectedActualMetadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["owned-rejected-actual"] = "before"
        };
        var rejectedQaVerdictEvidence = new[] { "owned-rejected-qa-verdict-evidence" };
        var rejectedQaDefectEvidence = new[] { "owned-rejected-qa-defect-image" };
        var source = BuildDpoHandoffRecord();
        var sourceQa = Assert.Single(source.Chosen.QaResults);
        var defectTemplate = Assert.Single(source.Rejected.Verdict.Defects);
        var chosenDefects = new[]
        {
            defectTemplate with { EvidenceImageIds = chosenDefectEvidence }
        };
        var rejectedDefects = new[]
        {
            defectTemplate with { EvidenceImageIds = rejectedDefectEvidence }
        };
        var chosenQaDefects = new[]
        {
            defectTemplate with { EvidenceImageIds = chosenQaDefectEvidence }
        };
        var rejectedQaDefects = new[]
        {
            defectTemplate with { EvidenceImageIds = rejectedQaDefectEvidence }
        };
        var chosenQa = sourceQa with
        {
            Expected = sourceQa.Expected with { Metadata = chosenExpectedMetadata },
            Actual = sourceQa.Actual with { Metadata = chosenActualMetadata },
            Verdict = sourceQa.Verdict with
            {
                Defects = chosenQaDefects,
                EvidenceRefs = chosenQaVerdictEvidence
            },
            Metrics = chosenQaMetrics,
            Warnings = chosenQaWarnings
        };
        var rejectedQa = sourceQa with
        {
            ResultId = "owned-rejected-qa",
            Expected = sourceQa.Expected with { Metadata = rejectedExpectedMetadata },
            Actual = sourceQa.Actual with { Metadata = rejectedActualMetadata },
            Verdict = sourceQa.Verdict with
            {
                Defects = rejectedQaDefects,
                EvidenceRefs = rejectedQaVerdictEvidence
            },
            Metrics = rejectedQaMetrics,
            Warnings = rejectedQaWarnings
        };
        var chosenQaResults = new[] { chosenQa };
        var rejectedQaResults = new[] { rejectedQa };
        var record = source with
        {
            InternalVectorPayload = vector,
            Chosen = source.Chosen with
            {
                Html = chosenOptimizerHtml,
                Css = chosenOptimizerCss,
                Verdict = source.Chosen.Verdict with
                {
                    Defects = chosenDefects,
                    EvidenceRefs = chosenVerdictEvidence
                },
                QaResults = chosenQaResults,
                EvidenceRefs = chosenEvidence
            },
            Rejected = source.Rejected with
            {
                Html = rejectedOptimizerHtml,
                Css = rejectedOptimizerCss,
                Verdict = source.Rejected.Verdict with
                {
                    Defects = rejectedDefects,
                    EvidenceRefs = rejectedVerdictEvidence
                },
                QaResults = rejectedQaResults,
                EvidenceRefs = rejectedEvidence
            },
            Provenance = source.Provenance with { Metadata = provenanceMetadata },
            License = source.License with { Metadata = licenseMetadata },
            Quarantine = source.Quarantine with { EvidenceRefs = quarantineEvidence }
        };
        LogicalDpoTrainingRequest? capturedRequest = null;
        DesignConversionDpoTrainingRecord? capturedOwnedRecord = null;
        DesignConversionDpoTrainingHandoff.TrainingLeaseObserverForTesting = _ =>
        {
            vector.Clear();
            chosenEvidence[0] = "caller-mutated-evidence";
            rejectedEvidence[0] = "caller-mutated-rejected-evidence";
            chosenVerdictEvidence[0] = "caller-mutated-chosen-verdict-evidence";
            rejectedVerdictEvidence[0] = "caller-mutated-rejected-verdict-evidence";
            chosenDefectEvidence[0] = "caller-mutated-chosen-defect-image";
            rejectedDefectEvidence[0] = "caller-mutated-rejected-defect-image";
            chosenDefects[0] = defectTemplate with { DefectId = "caller-mutated-chosen-defect" };
            rejectedDefects[0] = defectTemplate with { DefectId = "caller-mutated-rejected-defect" };
            provenanceMetadata["owned"] = "after";
            provenanceMetadata["caller-added"] = "after";
            licenseMetadata["owned-license"] = "after";
            licenseMetadata["caller-added"] = "after";
            quarantineEvidence[0] = "caller-mutated-quarantine-evidence";
            chosenQaResults[0] = chosenQa with { ResultId = "caller-mutated-chosen-qa" };
            rejectedQaResults[0] = rejectedQa with { ResultId = "caller-mutated-rejected-qa" };
            chosenQaMetrics["owned-metric"] = 9.5;
            chosenQaMetrics["caller-added"] = 1;
            chosenQaWarnings[0] = "caller-mutated-warning";
            chosenExpectedMetadata["owned-expected"] = "after";
            chosenActualMetadata["owned-actual"] = "after";
            chosenQaVerdictEvidence[0] = "caller-mutated-qa-verdict-evidence";
            chosenQaDefectEvidence[0] = "caller-mutated-qa-defect-image";
            chosenQaDefects[0] = defectTemplate with { DefectId = "caller-mutated-qa-defect" };
            rejectedQaMetrics["owned-rejected-metric"] = 9.5;
            rejectedQaWarnings[0] = "caller-mutated-rejected-warning";
            rejectedExpectedMetadata["owned-rejected-expected"] = "after";
            rejectedActualMetadata["owned-rejected-actual"] = "after";
            rejectedQaVerdictEvidence[0] = "caller-mutated-rejected-qa-verdict-evidence";
            rejectedQaDefectEvidence[0] = "caller-mutated-rejected-qa-defect-image";
            rejectedQaDefects[0] = defectTemplate with
            {
                DefectId = "caller-mutated-rejected-qa-defect"
            };
        };
        DesignConversionDpoTrainingHandoff.TrainingRowsObserverForTesting = rows =>
            capturedOwnedRecord = Assert.Single(rows);
        DesignConversionDpoTrainingHandoff.TrainingRequestObserverForTesting =
            request => capturedRequest = request;
        var checkpointDirectory = Path.Combine(
            Path.GetTempPath(),
            $"handoff-deep-snapshot-{Guid.NewGuid():N}");

        try
        {
            var result = await DesignConversionDpoTrainingHandoff.ExecuteAsync(
                DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                [record],
                new DesignConversionDpoPolicyBaseTestProvider(),
                BuildDpoHandoffTokenizer(),
                checkpointDirectory,
                new LoraConfig
                {
                    Rank = 2,
                    Alpha = 4,
                    Dropout = 0,
                    TargetModules = ["q_proj", "v_proj"]
                });

            Assert.True(result.IsSuccess, result.ErrorMessage);
            var request = Assert.IsType<LogicalDpoTrainingRequest>(capturedRequest);
            Assert.NotNull(request.PreparedExecution);
            var preference = Assert.Single(request.Preferences);
            Assert.Equal(record.ChosenAuthorKind, preference.ChosenAuthorKind);
            Assert.Equal(record.RejectedAuthorKind, preference.RejectedAuthorKind);
            var batchAdmission = Assert.IsType<DpoTrainerPairAdmission>(
                Assert.Single(request.PreparedExecution!.OptimizerInputs)
                    .Batch.Batch.Admission);
            var expectedAdmission = DpoTrainerPairCustoms.CreateAdmitted(
                preference.Prompt,
                preference.Chosen,
                preference.Rejected,
                record.ChosenAuthorKind!.Value,
                record.RejectedAuthorKind!.Value,
                nameof(LogicalDpoTrainingUnit)).Admission;
            Assert.Equal(expectedAdmission.ContentFingerprintSha256,
                batchAdmission.ContentFingerprintSha256);
            Assert.Equal(nameof(LogicalDpoTrainingUnit),
                batchAdmission.IssuerName);
            Assert.Equal(
                [0.25f, -0.5f, 0.75f, 1.0f],
                Assert.IsType<float[]>(preference.TeachingVector));
            using var optimizerPrompt = JsonDocument.Parse(preference.Prompt);
            using var optimizerChosen = JsonDocument.Parse(preference.Chosen);
            using var optimizerRejected = JsonDocument.Parse(preference.Rejected);
            Assert.Equal(
                ["domain_tag", "unit_id", "source_kind", "internal_vector", "target", "source", "source_encoder"],
                optimizerPrompt.RootElement.EnumerateObject().Select(property => property.Name).ToArray());
            Assert.False(optimizerPrompt.RootElement.GetProperty("source").TryGetProperty("identity_hash", out _));
            Assert.False(optimizerPrompt.RootElement.GetProperty("source_encoder").TryGetProperty("vector_hash", out _));
            Assert.Equal(
                ["html", "css"],
                optimizerChosen.RootElement.EnumerateObject().Select(property => property.Name).ToArray());
            Assert.Equal(
                ["html", "css"],
                optimizerRejected.RootElement.EnumerateObject().Select(property => property.Name).ToArray());
            Assert.Equal(chosenOptimizerHtml, optimizerChosen.RootElement.GetProperty("html").GetString());
            Assert.Equal(chosenOptimizerCss, optimizerChosen.RootElement.GetProperty("css").GetString());
            Assert.Equal(rejectedOptimizerHtml, optimizerRejected.RootElement.GetProperty("html").GetString());
            Assert.Equal(rejectedOptimizerCss, optimizerRejected.RootElement.GetProperty("css").GetString());
            Assert.DoesNotContain(rejectedOptimizerHtml, preference.Chosen, StringComparison.Ordinal);
            Assert.DoesNotContain(chosenOptimizerHtml, preference.Rejected, StringComparison.Ordinal);
            var optimizerText = string.Join('\n', preference.Prompt, preference.Chosen, preference.Rejected);
            foreach (var forbidden in new[]
                     {
                         "pass153-dpo-record", "owned-chosen-evidence", "owned-rejected-evidence",
                         "owned-chosen-verdict-evidence", "owned-rejected-verdict-evidence",
                         "owned-chosen-defect-image", "owned-rejected-defect-image",
                         "owned-qa-verdict-evidence", "owned-rejected-qa-verdict-evidence",
                         "visual_hierarchy", "local-test-reviewer", "usefulness", "accepted", "passed",
                         "score", "verdict", "candidate_id", "output_id", "qa_result", "evidence_refs"
                     })
            {
                Assert.DoesNotContain(forbidden, optimizerText, StringComparison.OrdinalIgnoreCase);
            }
            Assert.Equal("figma", optimizerPrompt.RootElement.GetProperty("source").GetProperty("kind").GetString());
            Assert.Contains("FRAME", optimizerPrompt.RootElement.GetProperty("source").GetProperty("structure_payload").GetString(), StringComparison.Ordinal);

            var ownedRecord = Assert.IsType<DesignConversionDpoTrainingRecord>(
                capturedOwnedRecord);
            Assert.Equal("before", ownedRecord.Provenance.Metadata["owned"]);
            Assert.DoesNotContain("caller-added", ownedRecord.Provenance.Metadata.Keys);
            Assert.Equal("before", ownedRecord.License.Metadata["owned-license"]);
            Assert.DoesNotContain("caller-added", ownedRecord.License.Metadata.Keys);
            Assert.Equal("owned-quarantine-evidence", Assert.Single(ownedRecord.Quarantine.EvidenceRefs));
            Assert.Equal("owned-chosen-evidence", Assert.Single(ownedRecord.Chosen.EvidenceRefs));
            Assert.Equal("owned-rejected-evidence", Assert.Single(ownedRecord.Rejected.EvidenceRefs));
            Assert.Equal(
                "owned-chosen-verdict-evidence",
                Assert.Single(ownedRecord.Chosen.Verdict.EvidenceRefs));
            Assert.Equal(
                "owned-chosen-defect-image",
                Assert.Single(Assert.Single(ownedRecord.Chosen.Verdict.Defects).EvidenceImageIds));
            Assert.Equal(
                "owned-rejected-verdict-evidence",
                Assert.Single(ownedRecord.Rejected.Verdict.EvidenceRefs));
            Assert.Equal(
                "owned-rejected-defect-image",
                Assert.Single(Assert.Single(ownedRecord.Rejected.Verdict.Defects).EvidenceImageIds));

            var ownedChosenQa = Assert.Single(ownedRecord.Chosen.QaResults);
            Assert.Equal("owned-metric", Assert.Single(ownedChosenQa.Metrics).Key);
            Assert.Equal(0.125, ownedChosenQa.Metrics["owned-metric"]);
            Assert.Equal("owned-warning", Assert.Single(ownedChosenQa.Warnings));
            Assert.Equal("before", ownedChosenQa.Expected.Metadata["owned-expected"]);
            Assert.Equal("before", ownedChosenQa.Actual.Metadata["owned-actual"]);
            Assert.Equal(
                "owned-qa-verdict-evidence",
                Assert.Single(ownedChosenQa.Verdict.EvidenceRefs));
            Assert.Equal(
                "owned-qa-defect-image",
                Assert.Single(Assert.Single(ownedChosenQa.Verdict.Defects).EvidenceImageIds));

            var ownedRejectedQa = Assert.Single(ownedRecord.Rejected.QaResults);
            Assert.Equal("owned-rejected-qa", ownedRejectedQa.ResultId);
            Assert.Equal(0.875, ownedRejectedQa.Metrics["owned-rejected-metric"]);
            Assert.Equal("owned-rejected-warning", Assert.Single(ownedRejectedQa.Warnings));
            Assert.Equal(
                "before",
                ownedRejectedQa.Expected.Metadata["owned-rejected-expected"]);
            Assert.Equal(
                "before",
                ownedRejectedQa.Actual.Metadata["owned-rejected-actual"]);
            Assert.Equal(
                "owned-rejected-qa-verdict-evidence",
                Assert.Single(ownedRejectedQa.Verdict.EvidenceRefs));
            Assert.Equal(
                "owned-rejected-qa-defect-image",
                Assert.Single(Assert.Single(ownedRejectedQa.Verdict.Defects).EvidenceImageIds));
        }
        finally
        {
            DesignConversionDpoTrainingHandoff.TrainingLeaseObserverForTesting = null;
            DesignConversionDpoTrainingHandoff.TrainingRowsObserverForTesting = null;
            DesignConversionDpoTrainingHandoff.TrainingRequestObserverForTesting = null;
            if (Directory.Exists(checkpointDirectory))
                Directory.Delete(checkpointDirectory, recursive: true);
        }
    }

    [Fact]
    public async Task DesignConversionDpoTrainingHandoff_MissingPairAuthorsFailBeforeProvider()
    {
        var record = BuildDpoHandoffRecord() with
        {
            ChosenAuthorKind = null,
            RejectedAuthorKind = null
        };
        var provider = new DesignConversionDpoPolicyBaseTestProvider();
        var checkpointDirectory = Path.Combine(
            Path.GetTempPath(),
            $"handoff-missing-pair-authors-{Guid.NewGuid():N}");

        try
        {
            var result = await DesignConversionDpoTrainingHandoff.ExecuteAsync(
                DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                [record],
                provider,
                BuildDpoHandoffTokenizer(),
                checkpointDirectory,
                new LoraConfig
                {
                    Rank = 2,
                    Alpha = 4,
                    Dropout = 0,
                    TargetModules = ["q_proj", "v_proj"]
                });

            Assert.False(result.IsSuccess);
            Assert.Equal(
                LogicalDpoTrainingUnit.PreferenceBoundsInvalidErrorCode,
                result.ErrorCode);
            Assert.Equal(0, provider.InvocationCount);
            Assert.False(Directory.Exists(checkpointDirectory));
        }
        finally
        {
            if (Directory.Exists(checkpointDirectory))
                Directory.Delete(checkpointDirectory, recursive: true);
        }
    }

    [Fact]
    public async Task DpoTrainingHandoff_ProviderFatalExceptionGraphEscapesWithoutArtifacts()
    {
        var outputDirectory = Path.Combine(
            Path.GetTempPath(),
            $"design-conversion-dpo-provider-fatal-{Guid.NewGuid():N}");
        var fatal = new AggregateException(
            "constructed-fatal-graph",
            new OutOfMemoryException("constructed-test-fatal"));
        var provider = new DesignConversionDpoPolicyBaseTestProvider(_ => throw fatal);

        var thrown = await Assert.ThrowsAsync<AggregateException>(() =>
            DesignConversionDpoTrainingHandoff.ExecuteAsync(
                DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                [BuildDpoHandoffRecord()],
                provider,
                BuildDpoHandoffTokenizer(),
                outputDirectory,
                new LoraConfig
                {
                    Rank = 2,
                    Alpha = 4,
                    Dropout = 0,
                    TargetModules = ["q_proj", "v_proj"]
                },
                maxStepCount: 1));

        Assert.Same(fatal, thrown);
        Assert.False(Directory.Exists(outputDirectory));
    }

    [Fact]
    public async Task DpoTrainingHandoff_FreshLoadFatalExceptionGraphEscapesWithoutRuntimeArtifacts()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            $"design-conversion-dpo-fresh-load-fatal-{Guid.NewGuid():N}");
        var trainingDirectory = Path.Combine(root, "training");
        var runtimeDirectory = Path.Combine(root, "runtime");
        var runtimePath = Path.Combine(runtimeDirectory, "runtime.json");
        try
        {
            var record = BuildDpoHandoffRecord();
            var handoff = await DesignConversionDpoTrainingHandoff.ExecuteAsync(
                DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                [record],
                new DesignConversionDpoPolicyBaseTestProvider(),
                BuildDpoHandoffTokenizer(),
                trainingDirectory,
                new LoraConfig
                {
                    Rank = 2,
                    Alpha = 4,
                    Dropout = 0,
                    TargetModules = ["q_proj", "v_proj"]
                },
                maxStepCount: 1);
            Assert.True(handoff.IsSuccess, handoff.ErrorMessage);
            var sourcePolicyPath = Assert.Single(handoff.Value!.PolicyCheckpointFilePaths);
            var sourcePolicyBytes = await File.ReadAllBytesAsync(sourcePolicyPath);
            var fatal = new AggregateException(
                "constructed-fatal-graph",
                new OutOfMemoryException("constructed-test-fatal"));
            DesignConversionDpoTrainingHandoff.PolicyCheckpointFreshLoadObserverForTesting =
                () => throw fatal;

            var thrown = await Assert.ThrowsAsync<AggregateException>(() =>
                DesignConversionDpoTrainingHandoff.ConvertPolicyCheckpointToRuntimeCheckpointAsync(
                    DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                    record,
                    sourcePolicyPath,
                    new DesignConversionDpoPolicyBaseTestProvider(),
                    handoff.Value.LoraConfiguration,
                    runtimePath));

            Assert.Same(fatal, thrown);
            Assert.Equal(sourcePolicyBytes, await File.ReadAllBytesAsync(sourcePolicyPath));
            Assert.False(File.Exists(runtimePath));
            Assert.False(File.Exists(Path.Combine(runtimeDirectory, Path.GetFileName(sourcePolicyPath))));
            if (Directory.Exists(runtimeDirectory))
                Assert.Empty(Directory.EnumerateFiles(runtimeDirectory, "*.tmp", SearchOption.TopDirectoryOnly));
        }
        finally
        {
            DesignConversionDpoTrainingHandoff.PolicyCheckpointFreshLoadObserverForTesting = null;
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task DpoTrainerPolicyCheckpointBridge_InvokedProviderCancelsSuppliedTokenThenThrowsReturnsSanitizedCancelledWithoutArtifactsOrMutation()
    {
        const string privateSentinel = "C:/private/provider/cancelled/model.bin PRIVATE_PROVIDER_THROW_CANCELLED_SENTINEL";
        var outputDirectory = Path.Combine(
            Path.GetTempPath(),
            $"design-conversion-dpo-provider-throw-cancelled-{Guid.NewGuid():N}");
        var runtimeCheckpointPath = Path.Combine(outputDirectory, "runtime.json");
        using var cancellationSource = new CancellationTokenSource();
        IReadOnlyList<int[]>? initialPolicyBits = null;
        IReadOnlyList<int[]>? initialReferenceBits = null;
        IReadOnlyList<int[]>? initialFreshLoadBits = null;
        var provider = new DesignConversionDpoPolicyBaseTestProvider(modelSet =>
        {
            initialPolicyBits = SnapshotDpoModelParameterBits(modelSet.Policy);
            initialReferenceBits = SnapshotDpoModelParameterBits(modelSet.Reference);
            initialFreshLoadBits = SnapshotDpoModelParameterBits(modelSet.FreshLoadTarget);
            cancellationSource.Cancel();
            throw new OperationCanceledException(privateSentinel, cancellationSource.Token);
        });

        try
        {
            var result = await DesignConversionDpoPolicyCheckpointBridge.ExecuteAndConvertAsync(
                DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                [BuildDpoHandoffRecord()],
                outputDirectory,
                "provider-throw-cancelled",
                runtimeCheckpointPath,
                provider,
                cancellationSource.Token);

            Assert.False(result.IsSuccess);
            Assert.Equal("CANCELLED", result.ErrorCode);
            Assert.Equal("DesignConversion DpoTrainer policy bridge cancelled.", result.ErrorMessage);
            Assert.DoesNotContain(privateSentinel, result.ErrorMessage, StringComparison.Ordinal);
            Assert.DoesNotContain("C:/private", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
            Assert.True(cancellationSource.IsCancellationRequested);
            Assert.Equal(1, provider.InvocationCount);
            var modelSet = Assert.IsType<DesignConversionDpoPolicyBaseModelSet>(provider.LastModelSet);
            Assert.NotNull(initialPolicyBits);
            Assert.NotNull(initialReferenceBits);
            Assert.NotNull(initialFreshLoadBits);
            AssertLlmParameterBitsEqual(initialPolicyBits, modelSet.Policy);
            AssertLlmParameterBitsEqual(initialReferenceBits, modelSet.Reference);
            AssertLlmParameterBitsEqual(initialFreshLoadBits, modelSet.FreshLoadTarget);
            Assert.False(Directory.Exists(outputDirectory));
            Assert.False(File.Exists(runtimeCheckpointPath));
        }
        finally
        {
            if (Directory.Exists(outputDirectory))
                Directory.Delete(outputDirectory, recursive: true);
        }
    }

    [Fact]
    public async Task DpoTrainerPolicyCheckpointBridge_InvokedProviderReturningCancelledFailureReturnsSanitizedCancelledWithoutArtifactsOrMutation()
    {
        const string privateSentinel = "C:/private/provider/cancelled/result.bin PRIVATE_PROVIDER_RESULT_CANCELLED_SENTINEL";
        var outputDirectory = Path.Combine(
            Path.GetTempPath(),
            $"design-conversion-dpo-provider-result-cancelled-{Guid.NewGuid():N}");
        var runtimeCheckpointPath = Path.Combine(outputDirectory, "runtime.json");
        IReadOnlyList<int[]>? initialPolicyBits = null;
        IReadOnlyList<int[]>? initialReferenceBits = null;
        IReadOnlyList<int[]>? initialFreshLoadBits = null;
        var provider = new DesignConversionDpoPolicyBaseTestProvider(modelSet =>
        {
            initialPolicyBits = SnapshotDpoModelParameterBits(modelSet.Policy);
            initialReferenceBits = SnapshotDpoModelParameterBits(modelSet.Reference);
            initialFreshLoadBits = SnapshotDpoModelParameterBits(modelSet.FreshLoadTarget);
            return OperationResult<DesignConversionDpoPolicyBaseModelSet>.Fail(privateSentinel, "CANCELLED");
        });

        try
        {
            var result = await DesignConversionDpoPolicyCheckpointBridge.ExecuteAndConvertAsync(
                DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                [BuildDpoHandoffRecord()],
                outputDirectory,
                "provider-result-cancelled",
                runtimeCheckpointPath,
                provider);

            Assert.False(result.IsSuccess);
            Assert.Equal("CANCELLED", result.ErrorCode);
            Assert.Equal("DesignConversion DpoTrainer policy bridge cancelled.", result.ErrorMessage);
            Assert.DoesNotContain(privateSentinel, result.ErrorMessage, StringComparison.Ordinal);
            Assert.DoesNotContain("C:/private", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(1, provider.InvocationCount);
            var modelSet = Assert.IsType<DesignConversionDpoPolicyBaseModelSet>(provider.LastModelSet);
            Assert.NotNull(initialPolicyBits);
            Assert.NotNull(initialReferenceBits);
            Assert.NotNull(initialFreshLoadBits);
            AssertLlmParameterBitsEqual(initialPolicyBits, modelSet.Policy);
            AssertLlmParameterBitsEqual(initialReferenceBits, modelSet.Reference);
            AssertLlmParameterBitsEqual(initialFreshLoadBits, modelSet.FreshLoadTarget);
            Assert.False(Directory.Exists(outputDirectory));
            Assert.False(File.Exists(runtimeCheckpointPath));
        }
        finally
        {
            if (Directory.Exists(outputDirectory))
                Directory.Delete(outputDirectory, recursive: true);
        }
    }

    [Fact]
    public async Task DpoTrainerPolicyCheckpointBridge_ConvergenceStopsAtActualStepAndBindsFinalArtifacts()
    {
        const int requestedMaxStepCount = DesignConversionDpoPolicyCheckpointBridgeOptions.MaxAllowedDpoStepCount;
        const int expectedActualStepCount = 3;
        var outputDirectory = Path.Combine(
            Path.GetTempPath(),
            $"design-conversion-dpo-policy-converged-{Guid.NewGuid():N}");
        const string runId = "pass-options-dpo-converged";
        var runtimeCheckpointPath = Path.Combine(outputDirectory, "runtime.json");
        var provider = new DesignConversionDpoPolicyBaseTestProvider();
        var finalPolicyFileName = DpoTrainer.BuildPolicyCheckpointFileName(expectedActualStepCount);
        var rejectedMaxPolicyFileName = DpoTrainer.BuildPolicyCheckpointFileName(requestedMaxStepCount);
        var options = new DesignConversionDpoPolicyCheckpointBridgeOptions(
            MaxBridgePreferenceCount: 1,
            DpoStepCount: requestedMaxStepCount)
        {
            DpoConvergenceThreshold = float.MaxValue,
            DpoConvergenceWindowSteps = 1
        };

        try
        {
            var result = await DesignConversionDpoPolicyCheckpointBridge.ExecuteAndConvertAsync(
                DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                [BuildDpoHandoffRecord()],
                outputDirectory,
                runId,
                runtimeCheckpointPath,
                provider,
                options);

            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.Equal(1, provider.InvocationCount);
            var bridge = result.Value!;
            Assert.Equal(expectedActualStepCount, bridge.DpoStepCount);
            Assert.Equal(finalPolicyFileName, bridge.PolicyCheckpointPath);
            Assert.True(bridge.PolicyCheckpointFreshLoadVerified);
            Assert.True(File.Exists(Path.Combine(outputDirectory, finalPolicyFileName)));
            Assert.True(File.Exists(runtimeCheckpointPath));
            Assert.Contains($"dpo_policy_step_count:{expectedActualStepCount}", bridge.EvidenceRefs);
            Assert.DoesNotContain($"dpo_policy_step_count:{requestedMaxStepCount}", bridge.EvidenceRefs);

            var typedRef = Assert.IsType<DesignConversionDpoPolicyCheckpointRef>(
                bridge.RuntimeCheckpoint.DpoPolicyCheckpointRef);
            Assert.Same(typedRef, bridge.PolicyCheckpointRef);
            Assert.Equal(expectedActualStepCount, typedRef.DpoStepCount);
            Assert.Equal(finalPolicyFileName, typedRef.PolicyCheckpointFileName);
            Assert.Equal(bridge.PolicyCheckpointSha256, typedRef.Sha256);
            Assert.Equal(
                expectedActualStepCount,
                bridge.RuntimeCheckpoint.DpoPolicyCheckpointRef?.DpoStepCount);
            Assert.Equal(
                finalPolicyFileName,
                bridge.RuntimeCheckpoint.DpoPolicyCheckpointRef?.PolicyCheckpointFileName);
            Assert.Contains($"dpo_policy_step_count:{expectedActualStepCount}", typedRef.EvidenceRefs);
            Assert.DoesNotContain($"dpo_policy_step_count:{requestedMaxStepCount}", typedRef.EvidenceRefs);

            var trainerPolicyCheckpointDirectory = Path.Combine(outputDirectory, $"{runId}-dpo-policy-checkpoints");
            Assert.True(File.Exists(Path.Combine(
                trainerPolicyCheckpointDirectory,
                finalPolicyFileName)));
            Assert.True(File.Exists(Path.Combine(
                trainerPolicyCheckpointDirectory,
                DpoTrainer.BuildMetricsFileName(expectedActualStepCount))));
            foreach (var step in Enumerable.Range(expectedActualStepCount + 1, requestedMaxStepCount - expectedActualStepCount))
            {
                Assert.False(File.Exists(Path.Combine(
                    trainerPolicyCheckpointDirectory,
                    DpoTrainer.BuildPolicyCheckpointFileName(step))));
                Assert.False(File.Exists(Path.Combine(
                    trainerPolicyCheckpointDirectory,
                    DpoTrainer.BuildMetricsFileName(step))));
            }

            Assert.False(File.Exists(Path.Combine(outputDirectory, rejectedMaxPolicyFileName)));
            Assert.False(File.Exists(Path.Combine(
                outputDirectory,
                DpoTrainer.BuildPolicyCheckpointFileName(expectedActualStepCount - 1))));
            var runtimeJson = await File.ReadAllTextAsync(runtimeCheckpointPath);
            Assert.Contains(finalPolicyFileName, runtimeJson, StringComparison.Ordinal);
            Assert.DoesNotContain(rejectedMaxPolicyFileName, runtimeJson, StringComparison.Ordinal);
            Assert.DoesNotContain(
                $"dpo_policy_step_count:{requestedMaxStepCount}",
                runtimeJson,
                StringComparison.Ordinal);
        }
        finally
        {
            if (Directory.Exists(outputDirectory))
                Directory.Delete(outputDirectory, recursive: true);
        }
    }

    [Fact]
    public async Task DpoTrainerPolicyCheckpointBridge_OptionsFlowToPreferenceLimitStepCountRefEvidenceAndFileName()
    {
        var outputDirectory = Path.Combine(
            Path.GetTempPath(),
            $"design-conversion-dpo-policy-options-{Guid.NewGuid():N}");
        const string runId = "pass-options-dpo-step-two";
        var runtimeCheckpointPath = Path.Combine(outputDirectory, "runtime.json");
        var provider = new DesignConversionDpoPolicyBaseTestProvider();
        var firstRecord = BuildDpoHandoffRecord() with
        {
            RecordId = "pass-options-dpo-record-a",
            UsefulnessScore = 0.87
        };
        var secondRecord = BuildDpoHandoffRecord() with
        {
            RecordId = "pass-options-dpo-record-b",
            UsefulnessScore = 0.99
        };
        var options = new DesignConversionDpoPolicyCheckpointBridgeOptions(
            MaxBridgePreferenceCount: 1,
            DpoStepCount: 2)
        {
            DpoConvergenceThreshold = 0f
        };

        try
        {
            var result = await DesignConversionDpoPolicyCheckpointBridge.ExecuteAndConvertAsync(
                DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                [firstRecord, secondRecord],
                outputDirectory,
                runId,
                runtimeCheckpointPath,
                provider,
                options);

            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.Equal(1, provider.InvocationCount);
            Assert.Equal(2, result.Value!.SourceRecordCount);
            Assert.Equal(1, result.Value.BridgePreferenceCount);
            Assert.Equal(2, result.Value.DpoStepCount);
            Assert.Equal(DpoTrainer.BuildPolicyCheckpointFileName(2), result.Value.PolicyCheckpointPath);
            Assert.NotEqual(DpoTrainer.BuildPolicyCheckpointFileName(1), result.Value.PolicyCheckpointPath);
            Assert.Contains("dpo_policy_bridge_source_record_count:2", result.Value.EvidenceRefs);
            Assert.Contains("dpo_policy_bridge_preference_count:1", result.Value.EvidenceRefs);
            Assert.Contains("dpo_policy_step_count:2", result.Value.EvidenceRefs);
            Assert.DoesNotContain("dpo_policy_step_count:1", result.Value.EvidenceRefs);

            var typedRef = Assert.IsType<DesignConversionDpoPolicyCheckpointRef>(
                result.Value.RuntimeCheckpoint.DpoPolicyCheckpointRef);
            Assert.Same(typedRef, result.Value.PolicyCheckpointRef);
            Assert.Equal(2, typedRef.SourceRecordCount);
            Assert.Equal(1, typedRef.BridgePreferenceCount);
            Assert.Equal(2, typedRef.DpoStepCount);
            Assert.Equal(DpoTrainer.BuildPolicyCheckpointFileName(2), typedRef.PolicyCheckpointFileName);
            Assert.Contains("dpo_policy_bridge_source_record_count:2", typedRef.EvidenceRefs);
            Assert.Contains("dpo_policy_bridge_preference_count:1", typedRef.EvidenceRefs);
            Assert.Contains("dpo_policy_step_count:2", typedRef.EvidenceRefs);
            Assert.DoesNotContain("dpo_policy_step_count:1", typedRef.EvidenceRefs);

            var siblingPolicyCheckpointPath = Path.Combine(outputDirectory, typedRef.PolicyCheckpointFileName);
            var trainerPolicyCheckpointDirectory = Path.Combine(outputDirectory, $"{runId}-dpo-policy-checkpoints");
            Assert.True(File.Exists(siblingPolicyCheckpointPath));
            Assert.True(File.Exists(Path.Combine(
                trainerPolicyCheckpointDirectory,
                DpoTrainer.BuildPolicyCheckpointFileName(1))));
            Assert.True(File.Exists(Path.Combine(
                trainerPolicyCheckpointDirectory,
                DpoTrainer.BuildPolicyCheckpointFileName(2))));
            Assert.True(File.Exists(Path.Combine(
                trainerPolicyCheckpointDirectory,
                DpoTrainer.BuildMetricsFileName(1))));
            Assert.True(File.Exists(Path.Combine(
                trainerPolicyCheckpointDirectory,
                DpoTrainer.BuildMetricsFileName(2))));
            Assert.False(File.Exists(Path.Combine(
                outputDirectory,
                DpoTrainer.BuildPolicyCheckpointFileName(1))));
        }
        finally
        {
            if (Directory.Exists(outputDirectory))
                Directory.Delete(outputDirectory, recursive: true);
        }
    }

    [Fact]
    public async Task DpoTrainerPolicyCheckpointBridge_MaximumSupportedStepCountPublishesEveryStepAndFinalMetadata()
    {
        const int stepCount = LogicalDpoTrainingUnit.MaximumSupportedMaxStepCount;
        var outputDirectory = Path.Combine(
            Path.GetTempPath(),
            $"design-conversion-dpo-policy-max-step-{Guid.NewGuid():N}");
        const string runId = "pass-options-dpo-max-step";
        var runtimeCheckpointPath = Path.Combine(outputDirectory, "runtime.json");
        var provider = new DesignConversionDpoPolicyBaseTestProvider();

        try
        {
            var result = await DesignConversionDpoPolicyCheckpointBridge.ExecuteAndConvertAsync(
                DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                [BuildDpoHandoffRecord()],
                outputDirectory,
                runId,
                runtimeCheckpointPath,
                provider,
                new DesignConversionDpoPolicyCheckpointBridgeOptions(
                    MaxBridgePreferenceCount: 1,
                    DpoStepCount: stepCount)
                {
                    DpoConvergenceThreshold = 0f
                });

            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.Equal(1, provider.InvocationCount);
            var bridge = result.Value!;
            var finalPolicyFileName = DpoTrainer.BuildPolicyCheckpointFileName(stepCount);
            Assert.Equal(stepCount, bridge.DpoStepCount);
            Assert.Equal(finalPolicyFileName, bridge.PolicyCheckpointPath);
            Assert.True(bridge.PolicyCheckpointFreshLoadVerified);
            Assert.True(File.Exists(Path.Combine(outputDirectory, finalPolicyFileName)));
            Assert.True(File.Exists(runtimeCheckpointPath));
            Assert.Contains($"dpo_policy_step_count:{stepCount}", bridge.EvidenceRefs);

            var typedRef = Assert.IsType<DesignConversionDpoPolicyCheckpointRef>(
                bridge.RuntimeCheckpoint.DpoPolicyCheckpointRef);
            Assert.Same(typedRef, bridge.PolicyCheckpointRef);
            Assert.Equal(stepCount, typedRef.DpoStepCount);
            Assert.Equal(finalPolicyFileName, typedRef.PolicyCheckpointFileName);
            Assert.Equal(bridge.PolicyCheckpointSha256, typedRef.Sha256);
            Assert.True(typedRef.PolicyCheckpointByteLength > 0);
            Assert.Contains($"dpo_policy_step_count:{stepCount}", typedRef.EvidenceRefs);

            var trainerDirectory = Path.Combine(outputDirectory, $"{runId}-dpo-policy-checkpoints");
            foreach (var step in Enumerable.Range(1, stepCount))
            {
                Assert.True(File.Exists(Path.Combine(
                    trainerDirectory,
                    DpoTrainer.BuildPolicyCheckpointFileName(step))));
                Assert.True(File.Exists(Path.Combine(
                    trainerDirectory,
                    DpoTrainer.BuildMetricsFileName(step))));
            }

            Assert.False(File.Exists(Path.Combine(
                outputDirectory,
                DpoTrainer.BuildPolicyCheckpointFileName(stepCount - 1))));
        }
        finally
        {
            if (Directory.Exists(outputDirectory))
                Directory.Delete(outputDirectory, recursive: true);
        }
    }

    [Theory]
    [InlineData(8, 0)]
    [InlineData(8, LogicalDpoTrainingUnit.MaximumSupportedMaxStepCount + 1)]
    [InlineData(0, 1)]
    [InlineData(DesignConversionDpoPolicyCheckpointBridgeOptions.MaxAllowedBridgePreferenceCount + 1, 1)]
    public async Task DpoTrainerPolicyCheckpointBridge_InvalidOptionsFailBeforeProviderOrArtifacts(
        int maxBridgePreferenceCount,
        int dpoStepCount)
    {
        var outputDirectory = Path.Combine(
            Path.GetTempPath(),
            $"design-conversion-dpo-policy-invalid-options-{Guid.NewGuid():N}");
        var runtimeCheckpointPath = Path.Combine(outputDirectory, "runtime.json");
        var provider = new DesignConversionDpoPolicyBaseTestProvider();
        Assert.Equal(
            LogicalDpoTrainingUnit.MaximumSupportedMaxStepCount,
            DesignConversionDpoPolicyCheckpointBridgeOptions.MaxAllowedDpoStepCount);

        try
        {
            var result = await DesignConversionDpoPolicyCheckpointBridge.ExecuteAndConvertAsync(
                DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                [BuildDpoHandoffRecord()],
                outputDirectory,
                "pass-options-invalid",
                runtimeCheckpointPath,
                provider,
                new DesignConversionDpoPolicyCheckpointBridgeOptions(
                    MaxBridgePreferenceCount: maxBridgePreferenceCount,
                    DpoStepCount: dpoStepCount));

            Assert.False(result.IsSuccess);
            Assert.Equal("DESIGN_CONVERSION_DPO_POLICY_BRIDGE_OPTIONS_INVALID", result.ErrorCode);
            Assert.Equal("DesignConversion DpoTrainer policy bridge options are invalid.", result.ErrorMessage);
            Assert.Equal(0, provider.InvocationCount);
            Assert.Null(provider.LastModelSet);
            Assert.Null(result.Value);
            Assert.False(Directory.Exists(outputDirectory));
            Assert.False(File.Exists(runtimeCheckpointPath));
        }
        finally
        {
            if (Directory.Exists(outputDirectory))
                Directory.Delete(outputDirectory, recursive: true);
        }
    }

    [Fact]
    public void DpoTrainerPolicyCheckpointBridge_RequiresInjectedProviderAndDoesNotConstructLlmModel()
    {
        var executeMethods = typeof(DesignConversionDpoPolicyCheckpointBridge).GetMethods(
                BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(method => method.Name == nameof(DesignConversionDpoPolicyCheckpointBridge.ExecuteAndConvertAsync))
            .ToArray();
        Type[][] legacySignatures =
        [
            [
                typeof(IReadOnlyList<DesignConversionDpoTrainingRecord>),
                typeof(string),
                typeof(string),
                typeof(string),
                typeof(IDesignConversionDpoPolicyBaseProvider),
                typeof(CancellationToken)
            ],
            [
                typeof(IReadOnlyList<DesignConversionDpoTrainingRecord>),
                typeof(string),
                typeof(string),
                typeof(string),
                typeof(IDesignConversionDpoPolicyBaseProvider),
                typeof(DesignConversionDpoPolicyCheckpointBridgeOptions),
                typeof(CancellationToken)
            ],
            [
                typeof(IReadOnlyList<DesignConversionDpoTrainingRecord>),
                typeof(string),
                typeof(string),
                typeof(string),
                typeof(IDesignConversionDpoPolicyBaseProvider),
                typeof(DesignConversionDpoPolicyCheckpointBridgeOptions),
                typeof(DesignConversionDpoPolicyContinuationRequest),
                typeof(CancellationToken)
            ]
        ];
        var authoritySignatures = legacySignatures
            .Select(signature =>
                new[] { typeof(ServerOwnedLegalTrainingTeacherAdmissionAuthority) }
                    .Concat(signature)
                    .ToArray())
            .ToArray();
        var supportedSignatures = legacySignatures.Concat(authoritySignatures).ToArray();
        Assert.Equal(supportedSignatures.Length, executeMethods.Length);
        foreach (var supportedSignature in supportedSignatures)
        {
            AssertExactPublicMethodSignature(
                executeMethods,
                nameof(DesignConversionDpoPolicyCheckpointBridge.ExecuteAndConvertAsync),
                supportedSignature);
        }

        foreach (var executeMethod in executeMethods)
        {
            var methodParameters = executeMethod.GetParameters();
            Assert.DoesNotContain(
                methodParameters,
                parameter => parameter.ParameterType == typeof(LegalTrainingTeacherAdmissionAuthority) ||
                             parameter.ParameterType == typeof(LlmModel));
            var providerParameter = Assert.Single(
                methodParameters,
                parameter => parameter.ParameterType ==
                    typeof(IDesignConversionDpoPolicyBaseProvider));
            Assert.Equal(
                typeof(IDesignConversionDpoPolicyBaseProvider),
                providerParameter.ParameterType);
            Assert.False(providerParameter.HasDefaultValue);
            Assert.False(providerParameter.IsOptional);
            Assert.Equal(typeof(CancellationToken), methodParameters[^1].ParameterType);
            Assert.True(methodParameters[^1].HasDefaultValue);
            Assert.True(methodParameters[^1].IsOptional);

            if (methodParameters.Any(parameter =>
                    parameter.ParameterType == typeof(DesignConversionDpoPolicyContinuationRequest)))
            {
                Assert.Equal(
                    typeof(DesignConversionDpoPolicyContinuationRequest),
                    methodParameters[^2].ParameterType);
            }

            var isAuthorityFirst = methodParameters[0].ParameterType ==
                typeof(ServerOwnedLegalTrainingTeacherAdmissionAuthority);
            Assert.Equal(
                !isAuthorityFirst,
                executeMethod.GetCustomAttribute<ObsoleteAttribute>() is not null);
        }

        Assert.DoesNotContain(
            typeof(DesignConversionDpoPolicyCheckpointBridge).GetMethods(
                BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly),
            method => method.Name == "ExecuteWithAuthorityAsync" ||
                      method.GetParameters().Any(parameter =>
                          parameter.ParameterType ==
                          typeof(LegalTrainingTeacherAdmissionAuthority)));

        var oldShape = typeof(DesignConversionDpoPolicyCheckpointBridge).GetMethod(
            nameof(DesignConversionDpoPolicyCheckpointBridge.ExecuteAndConvertAsync),
            BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly,
            binder: null,
            types:
            [
                typeof(IReadOnlyList<DesignConversionDpoTrainingRecord>),
                typeof(string),
                typeof(string),
                typeof(string),
                typeof(IDesignConversionDpoPolicyBaseProvider),
                typeof(CancellationToken)
            ],
            modifiers: null);
        Assert.NotNull(oldShape);
        Assert.NotNull(oldShape.GetCustomAttribute<ObsoleteAttribute>());

        var optionsShape = typeof(DesignConversionDpoPolicyCheckpointBridge).GetMethod(
            nameof(DesignConversionDpoPolicyCheckpointBridge.ExecuteAndConvertAsync),
            BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly,
            binder: null,
            types:
            [
                typeof(IReadOnlyList<DesignConversionDpoTrainingRecord>),
                typeof(string),
                typeof(string),
                typeof(string),
                typeof(IDesignConversionDpoPolicyBaseProvider),
                typeof(DesignConversionDpoPolicyCheckpointBridgeOptions),
                typeof(CancellationToken)
            ],
            modifiers: null);
        Assert.NotNull(optionsShape);
        Assert.NotNull(optionsShape.GetCustomAttribute<ObsoleteAttribute>());

        var continuationShape = typeof(DesignConversionDpoPolicyCheckpointBridge).GetMethod(
            nameof(DesignConversionDpoPolicyCheckpointBridge.ExecuteAndConvertAsync),
            BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly,
            binder: null,
            types:
            [
                typeof(IReadOnlyList<DesignConversionDpoTrainingRecord>),
                typeof(string),
                typeof(string),
                typeof(string),
                typeof(IDesignConversionDpoPolicyBaseProvider),
                typeof(DesignConversionDpoPolicyCheckpointBridgeOptions),
                typeof(DesignConversionDpoPolicyContinuationRequest),
                typeof(CancellationToken)
            ],
            modifiers: null);
        Assert.NotNull(continuationShape);
        Assert.NotNull(continuationShape.GetCustomAttribute<ObsoleteAttribute>());

        Assert.DoesNotContain(
            typeof(DesignConversionDpoPolicyCheckpointBridge).GetMethods(
                BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly),
            method => method.Name == nameof(DesignConversionDpoPolicyCheckpointBridge.ExecuteAndConvertAsync) &&
                      method.GetParameters().Any(parameter =>
                          parameter.ParameterType == typeof(LlmModel)));

        var providerField = Assert.Single(
            typeof(JsonlDesignConversionLocalArtifactDb).GetFields(
                BindingFlags.Instance | BindingFlags.NonPublic),
            field => field.FieldType == typeof(IDesignConversionDpoPolicyBaseProvider));
        Assert.True(providerField.IsInitOnly);

        Assert.DoesNotContain(
            typeof(DesignConversionDpoPolicyCheckpointBridge).GetMethods(
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly),
            method => method.Name.Contains("CreateTinyPolicyConfig", StringComparison.Ordinal) ||
                      method.Name.Contains("CreateLlmModel", StringComparison.Ordinal));
        Assert.DoesNotContain(
            typeof(DesignConversionDpoPolicyCheckpointBridge).GetFields(
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic),
            field => field.Name.Contains("PolicyVocabSize", StringComparison.Ordinal) ||
                     field.Name.Contains("PolicyContextLength", StringComparison.Ordinal));

        var llmConstructor = Assert.Single(typeof(LlmModel).GetConstructors());
        var positiveControl = Assert.IsAssignableFrom<MethodInfo>(
            typeof(DesignConversionModelServerApiTests).GetMethod(
                nameof(ConstructLlmModelForIlDecoderPositiveControl),
                BindingFlags.Static | BindingFlags.NonPublic));
        Assert.True(
            IlConstructs(positiveControl, llmConstructor),
            "The IL decoder positive control must resolve the cross-assembly LlmModel constructor.");

        var methods = typeof(DesignConversionDpoPolicyCheckpointBridge).GetMethods(
            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
        Assert.All(methods, method => AssertIlDoesNotConstruct(method, llmConstructor));
        foreach (var moveNext in methods
                     .Select(method => method.GetCustomAttribute<
                         System.Runtime.CompilerServices.AsyncStateMachineAttribute>())
                     .Where(attribute => attribute is not null)
                     .Select(attribute => Assert.IsAssignableFrom<MethodInfo>(
                         attribute!.StateMachineType.GetMethod(
                             "MoveNext",
                             BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))))
        {
            AssertIlDoesNotConstruct(moveNext, llmConstructor);
        }
    }

    [Theory]
    [InlineData(PromotionHostArtifactDbMode.LocalNoRemote)]
    [InlineData(PromotionHostArtifactDbMode.RepositoryPromotion)]
    [InlineData(PromotionHostArtifactDbMode.HttpPromotion)]
    public void PromotionHostComposition_ForwardsDpoPolicyBaseProviderToArtifactDbAcrossLocalRepositoryAndHttpModes(
        PromotionHostArtifactDbMode mode)
    {
        var provider = new DesignConversionDpoPolicyBaseTestProvider(_ =>
            throw new InvalidOperationException(
                "PROMOTION_HOST_DPO_PROVIDER_IDENTITY_TRACE_SHOULD_NOT_BE_INVOKED"));
        var authority = DesignConversionLegalTeacherCapabilityTestFactory
            .CreateAdmitted(
                maxTeacherCalls: 1,
                modelId: "qwen3-promotion-host-authority:latest",
                expectedOllamaManifestDigest: new string('d', 64));
        using var legalTeacherAssociation =
            DesignConversionRetainedDpoPolicyBaseActivation
                .AssociateLegalTeacherAdmissionAuthority(provider, authority);
        var artifactRoot = CreateArtifactDbTempRoot($"promotion-host-dpo-provider-{mode}");
        using var promotionHttpClient = new HttpClient(
            new PromotionHostDpoProviderNoNetworkHttpHandler(),
            disposeHandler: true);

        try
        {
            var options = CreatePromotionHostDpoProviderOptions(
                mode,
                artifactRoot,
                provider,
                promotionHttpClient);
            var optionDescription = options.ToString();
            Assert.Null(typeof(DesignConversionModelServerPromotionHostOptions)
                .GetProperty(
                    "LegalTeacherAdmissionAuthority",
                    BindingFlags.Instance | BindingFlags.Public));
            Assert.DoesNotContain(
                nameof(LegalTrainingTeacherAdmissionAuthority),
                optionDescription,
                StringComparison.Ordinal);
            Assert.DoesNotContain("sha256:", optionDescription, StringComparison.Ordinal);
            var resolvedAuthority =
                DesignConversionRetainedDpoPolicyBaseActivation
                    .ResolveLegalTeacherAdmissionAuthority(provider);
            Assert.Same(authority, resolvedAuthority);
            var setup = InvokePromotionHostArtifactDbSetup(
                options,
                resolvedAuthority);
            var artifactDb = ReadPromotionHostSetupArtifactDb(setup);

            Assert.Same(provider, ReadDpoPolicyBaseProvider(artifactDb));
            Assert.Same(authority, ReadLegalTeacherAdmissionAuthority(artifactDb));
            Assert.Equal(
                mode != PromotionHostArtifactDbMode.LocalNoRemote,
                ReadPromotionHostSetupRemoteConfigured(setup));
            Assert.Equal(
                mode == PromotionHostArtifactDbMode.LocalNoRemote ? null : PromotionHostDpoProviderTenantId,
                ReadPromotionHostSetupExpectedTenantId(setup));
            Assert.Equal(
                mode == PromotionHostArtifactDbMode.RepositoryPromotion,
                ReadPromotionHostSetupActivatedModelCheckpointLoader(setup) is not null);
            Assert.Equal(0, provider.InvocationCount);
        }
        finally
        {
            DeleteDirectoryIfExists(artifactRoot);
        }
    }

    [Fact]
    public async Task DpoTrainerPolicyCheckpointBridge_BlocksLowDeltaLocalAblationGateWithoutDpoStep()
    {
        var checkpointDir = Path.Combine(Path.GetTempPath(), $"design-conversion-dpo-policy-low-delta-{Guid.NewGuid():N}");
        var runtimeCheckpointPath = Path.Combine(checkpointDir, "design-conversion-runtime-from-dpo-policy.json");

        try
        {
            var record = BuildDpoHandoffRecord();
            var lowDeltaRecord = record with
            {
                RecordId = "pass153-policy-bridge-low-delta-dpo-record",
                Chosen = record.Chosen with
                {
                    Verdict = CreateVerdict("pass153-policy-bridge-low-delta-chosen-verdict", 0.52, passed: true),
                    Accepted = true
                },
                Rejected = record.Rejected with
                {
                    Verdict = CreateVerdict("pass153-policy-bridge-low-delta-rejected-verdict", 0.49, passed: false),
                    Accepted = false
                }
            };
            Assert.Equal(
                TrainingTeacherSourceKind.DeterministicNoExternalTeacher,
                lowDeltaRecord.TeacherProvenance!.SourceKind);

            var lowDeltaProvider = new DesignConversionDpoPolicyBaseTestProvider();
            var result = await DesignConversionDpoPolicyCheckpointBridge.ExecuteAndConvertAsync(
                DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                [lowDeltaRecord],
                checkpointDir,
                "pass153-domain-dpo-policy-low-delta",
                runtimeCheckpointPath,
                lowDeltaProvider);

            Assert.False(result.IsSuccess);
            Assert.Equal(0, lowDeltaProvider.InvocationCount);
            Assert.Equal(
                "DESIGN_CONVERSION_DPO_POLICY_BRIDGE_LOCAL_ABLATION_GATE_FAILED",
                result.ErrorCode);
            Assert.Equal(
                "DesignConversion DpoTrainer policy bridge local ablation gate failed.",
                result.ErrorMessage);
            Assert.DoesNotContain("score_delta", result.ErrorMessage, StringComparison.Ordinal);
            Assert.DoesNotContain("preferred_score", result.ErrorMessage, StringComparison.Ordinal);
            Assert.DoesNotContain("baseline_score", result.ErrorMessage, StringComparison.Ordinal);
            Assert.False(File.Exists(runtimeCheckpointPath));
            Assert.False(
                Directory.Exists(checkpointDir),
                "Low-delta policy bridge rows must fail closed before DpoTrainer.StepVector can create policy checkpoints.");
        }
        finally
        {
            if (Directory.Exists(checkpointDir))
                Directory.Delete(checkpointDir, recursive: true);
        }
    }

    [Fact]
    public async Task DpoTrainerPolicyCheckpointBridge_LowDeltaLocalAblationGateDoesNotRequirePolicyBaseProvider()
    {
        var providerCheckpointDir = Path.Combine(Path.GetTempPath(), $"design-conversion-dpo-policy-low-delta-provider-{Guid.NewGuid():N}");
        var nullProviderCheckpointDir = Path.Combine(Path.GetTempPath(), $"design-conversion-dpo-policy-low-delta-null-provider-{Guid.NewGuid():N}");
        var providerRuntimeCheckpointPath = Path.Combine(providerCheckpointDir, "design-conversion-runtime-from-dpo-policy.json");
        var nullProviderRuntimeCheckpointPath = Path.Combine(nullProviderCheckpointDir, "design-conversion-runtime-from-dpo-policy.json");

        try
        {
            var record = BuildDpoHandoffRecord();
            var lowDeltaRecord = record with
            {
                RecordId = "pass153-policy-bridge-low-delta-null-provider-dpo-record",
                Chosen = record.Chosen with
                {
                    Verdict = CreateVerdict("pass153-policy-bridge-low-delta-null-provider-chosen-verdict", 0.52, passed: true),
                    Accepted = true
                },
                Rejected = record.Rejected with
                {
                    Verdict = CreateVerdict("pass153-policy-bridge-low-delta-null-provider-rejected-verdict", 0.49, passed: false),
                    Accepted = false
                }
            };

            var provider = new DesignConversionDpoPolicyBaseTestProvider();
            var providerResult = await DesignConversionDpoPolicyCheckpointBridge.ExecuteAndConvertAsync(
                DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                [lowDeltaRecord],
                providerCheckpointDir,
                "pass153-domain-dpo-policy-low-delta-provider-baseline",
                providerRuntimeCheckpointPath,
                provider);

            Assert.False(providerResult.IsSuccess);
            Assert.Equal(0, provider.InvocationCount);
            Assert.Equal(
                "DESIGN_CONVERSION_DPO_POLICY_BRIDGE_LOCAL_ABLATION_GATE_FAILED",
                providerResult.ErrorCode);
            Assert.Equal(
                "DesignConversion DpoTrainer policy bridge local ablation gate failed.",
                providerResult.ErrorMessage);
            Assert.DoesNotContain("score_delta", providerResult.ErrorMessage, StringComparison.Ordinal);
            Assert.False(File.Exists(providerRuntimeCheckpointPath));
            Assert.False(
                Directory.Exists(providerCheckpointDir),
                "Low-delta provider baseline must fail before bridge persistence.");

            var nullProviderResult = await DesignConversionDpoPolicyCheckpointBridge.ExecuteAndConvertAsync(
                DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                [lowDeltaRecord],
                nullProviderCheckpointDir,
                "pass153-domain-dpo-policy-low-delta-null-provider",
                nullProviderRuntimeCheckpointPath,
                (IDesignConversionDpoPolicyBaseProvider?)null);

            Assert.False(nullProviderResult.IsSuccess);
            Assert.Equal(providerResult.ErrorCode, nullProviderResult.ErrorCode);
            Assert.Equal(providerResult.ErrorMessage, nullProviderResult.ErrorMessage);
            Assert.False(File.Exists(nullProviderRuntimeCheckpointPath));
            Assert.False(
                Directory.Exists(nullProviderCheckpointDir),
                "Null provider must not create policy checkpoint output when local ablation already failed.");
        }
        finally
        {
            if (Directory.Exists(providerCheckpointDir))
                Directory.Delete(providerCheckpointDir, recursive: true);
            if (Directory.Exists(nullProviderCheckpointDir))
                Directory.Delete(nullProviderCheckpointDir, recursive: true);
        }
    }

    [Fact]
    public async Task DpoTrainerPolicyCheckpointBridge_NullPolicyBaseProviderFailsOnlyAtTrainingBoundary()
    {
        var checkpointDir = Path.Combine(Path.GetTempPath(), $"design-conversion-dpo-policy-null-provider-{Guid.NewGuid():N}");
        var runtimeCheckpointPath = Path.Combine(checkpointDir, "design-conversion-runtime-from-dpo-policy.json");

        try
        {
            var result = await DesignConversionDpoPolicyCheckpointBridge.ExecuteAndConvertAsync(
                DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                [BuildDpoHandoffRecord()],
                checkpointDir,
                "pass153-domain-dpo-policy-null-provider-trainable",
                runtimeCheckpointPath,
                (IDesignConversionDpoPolicyBaseProvider?)null);

            Assert.False(result.IsSuccess);
            Assert.Equal("DESIGN_CONVERSION_DPO_POLICY_BASE_PROVIDER_REQUIRED", result.ErrorCode);
            Assert.Equal("DesignConversion DPO policy base provider is required.", result.ErrorMessage);
            Assert.False(File.Exists(runtimeCheckpointPath));
            Assert.False(
                Directory.Exists(checkpointDir),
                "Missing provider must fail before bridge writes policy checkpoints or runtime checkpoints.");
        }
        finally
        {
            if (Directory.Exists(checkpointDir))
                Directory.Delete(checkpointDir, recursive: true);
        }
    }

    [Fact]
    public async Task DpoTrainerPolicyCheckpointBridge_RejectsExternalTeacherBeforeProviderOrDpoStep()
    {
        const string privateRequest = "PRIVATE-DOMAIN-BRIDGE-TEACHER-REQUEST";
        var privateVersion = new string('d', 64);
        var checkpointDir = Path.Combine(Path.GetTempPath(), $"design-conversion-dpo-policy-legal-teacher-{Guid.NewGuid():N}");
        var runtimeCheckpointPath = Path.Combine(checkpointDir, "design-conversion-runtime-from-dpo-policy.json");
        var provider = new DesignConversionDpoPolicyBaseTestProvider();
        var record = WithOpenTeacherPairAuthorship(BuildDpoHandoffRecord() with
        {
            TeacherProvenance = new TrainingTeacherProvenance(
                TrainingTeacherSourceKind.ExternalTeacherOutput,
                OpenWeightTeacherFamily.Qwen,
                TrainingTeacherAccessMode.LocalHostedOpenWeight,
                TrainingTeacherHostingProvider.Ollama,
                "qwen3.5-9b",
                privateVersion,
                TrainingTeacherModelLicense.Apache20,
                ProviderTermsChecked: false,
                ProviderTermsAllowTraining: false,
                ClosedProprietaryModel: false,
                TeacherOutputUsedForTraining: true,
                privateRequest,
                InvocationCount: 1)
            {
                ModelArtifactDigestKind =
                    LegalTrainingTeacherAdmissionAuthority.OllamaModelLayerSha256DigestKind,
                ModelArtifactDigest = LegalTeacherModelLayerDigest
            }
        });

        try
        {
            var result = await DesignConversionDpoPolicyCheckpointBridge.ExecuteAndConvertAsync(
                DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                [record],
                checkpointDir,
                "pass153-domain-dpo-policy-illegal-teacher",
                runtimeCheckpointPath,
                provider);

            Assert.False(result.IsSuccess);
            Assert.Equal(
                LegalTrainingTeacherAdmissionAuthority.NotAllowlistedErrorCode,
                result.ErrorCode);
            Assert.Equal(0, provider.InvocationCount);
            Assert.Null(provider.LastModelSet);
            Assert.False(File.Exists(runtimeCheckpointPath));
            Assert.False(
                Directory.Exists(checkpointDir),
                "Domain policy bridge must fail the legal teacher gate before provider creation, checkpoint output, or DpoTrainer.StepVector.");
            var serialized = JsonSerializer.Serialize(result);
            Assert.DoesNotContain(privateRequest, serialized, StringComparison.Ordinal);
            Assert.DoesNotContain(privateVersion, serialized, StringComparison.Ordinal);
            Assert.DoesNotContain("qwen3.5-9b", serialized, StringComparison.Ordinal);
        }
        finally
        {
            if (Directory.Exists(checkpointDir))
                Directory.Delete(checkpointDir, recursive: true);
        }
    }

    [Fact]
    public async Task DpoTrainerPolicyCheckpointBridge_ServerAuthorityAdmitsAllowlistedExternalTeacherWithoutRawIdentityLeakage()
    {
        const string privateRequest = "PRIVATE-DOMAIN-BRIDGE-ALLOWLISTED-TEACHER-REQUEST";
        const string privateModel = "qwen3-private-domain-bridge-model:test";
        var privateVersion = new string('c', 64);
        var checkpointDir = Path.Combine(Path.GetTempPath(), $"design-conversion-dpo-policy-allowlisted-teacher-{Guid.NewGuid():N}");
        var runtimeCheckpointPath = Path.Combine(checkpointDir, "design-conversion-runtime-from-dpo-policy.json");
        var provider = new DesignConversionDpoPolicyBaseTestProvider();
        var authority = CreateServerOwnedQwenOllamaAuthority(
            privateModel,
            privateVersion,
            maxTeacherCalls: 1);
        var record = WithOpenTeacherPairAuthorship(BuildDpoHandoffRecord() with
        {
            TeacherProvenance = CreateQwenOllamaTeacherProvenance(
                privateRequest,
                privateModel,
                privateVersion)
        });

        try
        {
            var result = await DesignConversionDpoPolicyCheckpointBridge.ExecuteAndConvertAsync(
                authority,
                [record],
                checkpointDir,
                "pass153-domain-dpo-policy-allowlisted-teacher",
                runtimeCheckpointPath,
                provider,
                options: null);

            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.Equal(1, provider.InvocationCount);
            Assert.NotNull(provider.LastModelSet);
            Assert.True(File.Exists(runtimeCheckpointPath));
            Assert.True(File.Exists(Path.Combine(
                Path.GetDirectoryName(runtimeCheckpointPath)!,
                result.Value!.PolicyCheckpointPath)));
            Assert.NotNull(result.Value.RuntimeCheckpoint.DpoPolicyCheckpointRef);

            var typedRef = result.Value.RuntimeCheckpoint.DpoPolicyCheckpointRef!;
            Assert.True(typedRef.FreshLoadVerified);
            Assert.True(typedRef.ConvertedToRuntimeWeights);
            Assert.Equal(1, typedRef.BridgePreferenceCount);
            Assert.Contains("teacher_provenance:source_kind=ExternalTeacherOutput", typedRef.EvidenceRefs);
            Assert.Contains("teacher_provenance:family=Qwen", typedRef.EvidenceRefs);
            Assert.Contains("teacher_provenance:access_mode=LocalHostedOpenWeight", typedRef.EvidenceRefs);
            Assert.Contains("teacher_provenance:hosting_provider=Ollama", typedRef.EvidenceRefs);
            Assert.Contains("teacher_provenance:model_license=Apache20", typedRef.EvidenceRefs);
            Assert.Contains("teacher_provenance:output_used_for_training=True", typedRef.EvidenceRefs);
            Assert.Contains("teacher_provenance:invocation_count=1", typedRef.EvidenceRefs);
            Assert.Contains("legal_teacher_admission:external_invocation_count=1", typedRef.EvidenceRefs);
            Assert.Contains("legal_teacher_admission:max_teacher_calls=1", typedRef.EvidenceRefs);
            Assert.Contains("teacher_legal_evidence:mode=SELF_HOSTED_NO_PROVIDER_TERMS", typedRef.EvidenceRefs);
            Assert.Contains(typedRef.EvidenceRefs, reference =>
                reference.StartsWith("teacher_provenance:request_ref=sha256:", StringComparison.Ordinal));
            Assert.Contains(typedRef.EvidenceRefs, reference =>
                reference.StartsWith("teacher_provenance:model_ref=sha256:", StringComparison.Ordinal));
            Assert.Contains(typedRef.EvidenceRefs, reference =>
                reference.StartsWith("teacher_provenance:model_version_ref=sha256:", StringComparison.Ordinal));

            var loaded = await DesignConversionMachine.LoadCheckpointAsync(runtimeCheckpointPath);
            Assert.True(loaded.IsSuccess, loaded.ErrorMessage);
            Assert.NotNull(loaded.Value!.Checkpoint.DpoPolicyCheckpointRef);
            Assert.Equal(typedRef.PolicyCheckpointFileName, loaded.Value.Checkpoint.DpoPolicyCheckpointRef!.PolicyCheckpointFileName);
            Assert.Equal(typedRef.Sha256, loaded.Value.Checkpoint.DpoPolicyCheckpointRef.Sha256);

            var publicSurface = string.Join(
                "\n",
                result.Value.EvidenceRefs
                    .Concat(typedRef.EvidenceRefs)
                    .Append(JsonSerializer.Serialize(result.Value.RuntimeCheckpoint.DpoPolicyCheckpointRef))
                    .Append(await File.ReadAllTextAsync(runtimeCheckpointPath)));
            Assert.Contains("sha256:", publicSurface, StringComparison.Ordinal);
            Assert.DoesNotContain(privateRequest, publicSurface, StringComparison.Ordinal);
            Assert.DoesNotContain(privateModel, publicSurface, StringComparison.Ordinal);
            Assert.DoesNotContain(privateVersion, publicSurface, StringComparison.Ordinal);
            Assert.DoesNotContain(Path.GetFullPath(checkpointDir), publicSurface, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (Directory.Exists(checkpointDir))
                Directory.Delete(checkpointDir, recursive: true);
        }
    }

    [Fact]
    public async Task SubmitAndEvaluateAsync_DpoTrainerPromotedLocalModelWithoutHeldOutGateStaysBoundedNonReady()
    {
        var service = new DesignConversionInternalService(
            new RecordingRenderProvider(),
            new RecordingComparisonProvider(CreateVerdict("dpo-bridge-missing-heldout-gate-verdict", 0.91, passed: true)),
            routingFlow: null);
        var api = new DesignConversionModelServerApi(
            service,
            () => DateTimeOffset.Parse("2026-07-08T10:30:00Z"),
            promotedLocalModel: BuildDpoTrainerPromotedLocalModel(),
            postEvaluationStateStore: TestPostEvaluationStateStore);

        var response = await api.SubmitAndEvaluateAsync(CreateRequest("dpo-bridge-missing-heldout-gate-request"));

        Assert.True(response.IsSuccess, response.ErrorMessage);
        Assert.False(response.Value!.ModelTrainingVerified);
        Assert.False(response.Value.ModelUploadReady);
        Assert.False(response.Value.DeploymentVerified);
        Assert.False(response.Value.ProductReady);
        Assert.False(response.Value.TrainedModelAuthority);
        Assert.True(response.Value.NotTrainedModelAuthority);
        Assert.Equal(
            "promoted_local_model_informational_verification_not_proven",
            response.Value.TrainingReadiness.Status);
        Assert.Equal(
            "blocked_source_disjoint_heldout_not_proven",
            response.Value.TrainingReadiness.SourceDisjointStatus);
        Assert.Contains(
            DesignConversionTrainingPipeline.SourceDisjointHeldOutReadinessGateNotProvenReason,
            response.Value.TrainingReadiness.Warnings);
        Assert.Equal(bool.FalseString, response.Value.BoundaryMetadata["product_ready"]);
        Assert.Equal(bool.FalseString, response.Value.BoundaryMetadata["trained_model_authority"]);
        Assert.Equal(bool.TrueString, response.Value.BoundaryMetadata["not_trained_model_authority"]);
        Assert.Equal("blocked_source_disjoint_heldout_not_proven", response.Value.BoundaryMetadata["source_disjoint_status"]);
        Assert.Equal("blocked_source_disjoint_heldout_not_proven",
            response.Value.BoundaryMetadata["local_promoted_source_disjoint_heldout_readiness_status"]);
        Assert.Equal(bool.FalseString,
            response.Value.BoundaryMetadata["local_promoted_source_disjoint_heldout_readiness_gate_passed"]);
    }

    [Fact]
    public async Task PublicApiConstructors_CallerSuppliedPromotedModelCannotForgeTrainingVerification()
    {
        var forgedModel = BuildCallerForgedPromotedLocalModel();
        var utcNow = () => DateTimeOffset.Parse("2026-07-08T10:45:00Z");

        DesignConversionInternalService CreateService()
            => new(
                new RecordingRenderProvider(),
                new RecordingComparisonProvider(CreateVerdict(
                    "public-constructor-forged-model-verdict",
                    0.91,
                    passed: true)),
                routingFlow: null);

        var evaluator = new DesignConversionInternalServiceThinClientJobEvaluator(CreateService());
        var artifactDbEvaluator = new DesignConversionInternalServiceThinClientJobEvaluator(CreateService());
        var runtimeEvaluator = new DesignConversionInternalServiceThinClientJobEvaluator(CreateService());
        var publicApis = new (string Name, DesignConversionModelServerApi Api)[]
        {
            (
                "service-constructor",
                new DesignConversionModelServerApi(
                    CreateService(),
                    utcNow,
                    promotedLocalModel: forgedModel,
                    postEvaluationStateStore: TestPostEvaluationStateStore)),
            (
                "evaluator-constructor",
                new DesignConversionModelServerApi(
                    evaluator,
                    utcNow,
                    promotedLocalModel: forgedModel,
                    postEvaluationStateStore: TestPostEvaluationStateStore)),
            (
                "evaluator-artifact-db-constructor",
                new DesignConversionModelServerApi(
                    artifactDbEvaluator,
                    utcNow,
                    artifactDb: null,
                    promotedLocalModel: forgedModel,
                    postEvaluationStateStore: TestPostEvaluationStateStore)),
            (
                "runtime-constructor",
                new DesignConversionModelServerApi(
                    new DesignConversionThinClientJobRuntime(
                        runtimeEvaluator,
                        utcNow,
                        postEvaluationStateStore: TestPostEvaluationStateStore),
                    promotedLocalModel: forgedModel,
                    utcNow: utcNow)),
            (
                "machine-backed-factory",
                DesignConversionModelServerApi.CreateMachineBackedLocal(
                    new DesignConversionMachine(),
                    new RecordingRenderProvider(),
                    new RecordingComparisonProvider(CreateVerdict(
                        "public-machine-factory-forged-model-verdict",
                        0.91,
                        passed: true)),
                    utcNow,
                    promotedLocalModel: forgedModel,
                    postEvaluationStateStore: TestPostEvaluationStateStore))
        };

        for (var index = 0; index < publicApis.Length; index++)
        {
            var (name, api) = publicApis[index];
            var response = await api.SubmitAndEvaluateAsync(
                CreateRequest($"public-authority-forgery-{index}-{name}"));

            Assert.True(response.IsSuccess, $"{name}: {response.ErrorCode} {response.ErrorMessage}");
            AssertCallerSuppliedModelIsInformationalOnly(response.Value!, name);

            var status = api.GetJobResult(response.Value!.JobId);
            Assert.True(status.IsSuccess, $"{name}: {status.ErrorCode} {status.ErrorMessage}");
            Assert.Equal(bool.FalseString, status.Value!.BoundaryMetadata["model_training_verified"]);
            Assert.Equal(bool.FalseString, status.Value.BoundaryMetadata["local_model_promotion_verified"]);
            Assert.Equal(bool.FalseString, status.Value.BoundaryMetadata["local_promoted_checkpoint_loaded"]);
            Assert.Equal(bool.FalseString, status.Value.BoundaryMetadata["local_promoted_checkpoint_real_weights"]);
            Assert.Equal("not_proven", status.Value.BoundaryMetadata["local_model_verification_authority"]);
        }
    }

    [Fact]
    public async Task PublicEvaluatorAndActivatedReadback_CannotForgeTrainingVerification()
    {
        var forgedModel = BuildCallerForgedPromotedLocalModel(
            "local_model_verification_authority=fresh_promotion_loader_result",
            "local_model_promotion_verified=True",
            "model_training_verified=True");
        var service = new DesignConversionInternalService(
            new RecordingRenderProvider(),
            new RecordingComparisonProvider(CreateVerdict(
                "public-evaluator-readback-forged-model-verdict",
                0.91,
                passed: true)),
            routingFlow: null);
        var forgedEvaluatorAndReadback = new ForgedPublicActivatedModelEvaluatorAndReadback(
            service,
            forgedModel);
        var api = new DesignConversionModelServerApi(
            forgedEvaluatorAndReadback,
            () => DateTimeOffset.Parse("2026-07-08T10:50:00Z"),
            promotedLocalModel: forgedModel,
            activatedModelRuntimeReadback: forgedEvaluatorAndReadback,
            postEvaluationStateStore: TestPostEvaluationStateStore);

        var response = await api.SubmitAndEvaluateAsync(
            CreateRequest("public-evaluator-readback-authority-forgery"));

        Assert.True(response.IsSuccess, response.ErrorMessage);
        AssertCallerSuppliedModelIsInformationalOnly(
            response.Value!,
            "public-evaluator-and-activated-readback");
        Assert.True(forgedEvaluatorAndReadback.PublicReadbackCallCount > 0);
    }

    [Fact]
    public void TrainingVerificationAuthority_IsAssemblyInternalAndAbsentFromPublicConstructors()
    {
        var modelServerAssembly = typeof(DesignConversionModelServerApi).Assembly;
        var verifiedReadbackType = modelServerAssembly.GetType(
            "ModelServer.DesignConversion.IDesignConversionVerifiedActivatedModelRuntimeReadback",
            throwOnError: true)!;
        Assert.False(verifiedReadbackType.IsPublic);
        Assert.False(verifiedReadbackType.IsNestedPublic);

        var productionFactory = typeof(DesignConversionModelServerApi).GetMethod(
            "CreateForProductionActivatedRuntime",
            BindingFlags.Static | BindingFlags.NonPublic);
        Assert.NotNull(productionFactory);
        Assert.True(productionFactory.IsAssembly);
        Assert.False(productionFactory.IsPublic);

        var promotionProofType = typeof(DesignConversionModelServerApi).GetNestedType(
            "PromotedLocalModelVerificationProof",
            BindingFlags.NonPublic);
        Assert.NotNull(promotionProofType);
        Assert.True(promotionProofType.IsNestedPrivate);

        var publicAuthorityParameters = typeof(DesignConversionModelServerApi)
            .GetConstructors(BindingFlags.Instance | BindingFlags.Public)
            .SelectMany(constructor => constructor.GetParameters())
            .Where(parameter =>
                parameter.ParameterType == verifiedReadbackType ||
                parameter.ParameterType == promotionProofType ||
                parameter.Name?.Contains("verificationAuthority", StringComparison.OrdinalIgnoreCase) == true ||
                parameter.Name?.Contains("verificationProof", StringComparison.OrdinalIgnoreCase) == true)
            .ToArray();
        Assert.Empty(publicAuthorityParameters);
    }

    [Fact]
    public async Task DesignConversionDpoTrainingHandoff_BlocksLowDeltaLocalAblationGateWithoutDpoStep()
    {
        var tokenizer = BuildDpoHandoffTokenizer();
        var provider = new DesignConversionDpoPolicyBaseTestProvider();
        var checkpointDir = Path.Combine(Path.GetTempPath(), $"design-conversion-dpo-handoff-low-delta-{Guid.NewGuid():N}");

        try
        {
            var record = BuildDpoHandoffRecord();
            var lowDeltaRecord = record with
            {
                RecordId = "pass153-low-delta-dpo-record",
                Chosen = record.Chosen with
                {
                    Verdict = CreateVerdict("pass153-low-delta-chosen-verdict", 0.52, passed: true),
                    Accepted = true
                },
                Rejected = record.Rejected with
                {
                    Verdict = CreateVerdict("pass153-low-delta-rejected-verdict", 0.49, passed: false),
                    Accepted = false
                }
            };

            var result = await DesignConversionDpoTrainingHandoff.ExecuteAsync(
                DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                [lowDeltaRecord],
                provider,
                tokenizer,
                checkpointDir,
                new LoraConfig { Rank = 2, Alpha = 4, Dropout = 0, TargetModules = ["q_proj", "v_proj"] },
                maxStepCount: 1);

            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.Equal(0, provider.InvocationCount);
            Assert.Null(provider.LastModelSet);
            Assert.Equal(1, result.Value!.RecordCount);
            Assert.Equal(1, result.Value.PreferenceCount);
            Assert.Equal(0, result.Value.DpoStepCount);
            Assert.Empty(result.Value.MetricFilePaths);
            Assert.Empty(result.Value.PolicyCheckpointFilePaths);
            Assert.Empty(result.Value.IngredientIds);
            Assert.False(Directory.Exists(checkpointDir));
            Assert.False(result.Value.QualityGate.PassesQualityGate);
            Assert.Equal(0.52, result.Value.QualityGate.PreferredScore, precision: 2);
            Assert.Equal(0.49, result.Value.QualityGate.BaselineScore, precision: 2);
            Assert.Equal(0.03, result.Value.QualityGate.ScoreDelta, precision: 2);
            Assert.Equal(0.10, result.Value.QualityGate.Threshold, precision: 2);
            Assert.Contains("score_delta_below_threshold:0.03<0.1", result.Value.QualityGate.GateReason);
            Assert.Contains("DESIGN_CONVERSION_DPO_HANDOFF_LOCAL_ABLATION_GATE_FAILED", result.Value.EvidenceRefs);
            Assert.Contains("passes_quality_gate:false", result.Value.QualityGate.EvidenceRefs);
            Assert.Equal(
                TrainingTeacherSourceKind.DeterministicNoExternalTeacher,
                Assert.Single(result.Value.LegalTeacherAdmission.Receipts).SourceKind);
            Assert.Equal(0, result.Value.LegalTeacherAdmission.ExternalTeacherInvocationCount);
            Assert.Contains(
                "teacher_provenance:source_kind=DeterministicNoExternalTeacher",
                result.Value.EvidenceRefs);

            var serialized = JsonSerializer.Serialize(result.Value);
            Assert.DoesNotContain("ProductReady", serialized, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("public_http_endpoint", serialized, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("deploy", serialized, StringComparison.OrdinalIgnoreCase);

            var missingProviderResult = await DesignConversionDpoTrainingHandoff.ExecuteAsync(
                DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                [lowDeltaRecord],
                (IDesignConversionDpoPolicyBaseProvider?)null,
                tokenizer,
                checkpointDir,
                new LoraConfig { Rank = 2, Alpha = 4, Dropout = 0, TargetModules = ["q_proj", "v_proj"] },
                maxStepCount: 1);

            Assert.True(missingProviderResult.IsSuccess, missingProviderResult.ErrorMessage);
            Assert.Equal(0, missingProviderResult.Value!.DpoStepCount);
            Assert.False(missingProviderResult.Value.QualityGate.PassesQualityGate);
            Assert.Equal(result.Value.QualityGate.GateReason, missingProviderResult.Value.QualityGate.GateReason);
        }
        finally
        {
            if (Directory.Exists(checkpointDir))
                Directory.Delete(checkpointDir, recursive: true);
        }
    }

    [Fact]
    public async Task DesignConversionDpoTrainingHandoff_NullPolicyBaseProviderFailsOnlyAtTrainingBoundary()
    {
        var tokenizer = BuildDpoHandoffTokenizer();
        var checkpointDir = Path.Combine(Path.GetTempPath(), $"design-conversion-dpo-handoff-null-provider-{Guid.NewGuid():N}");

        try
        {
            var result = await DesignConversionDpoTrainingHandoff.ExecuteAsync(
                DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                [BuildDpoHandoffRecord()],
                (IDesignConversionDpoPolicyBaseProvider?)null,
                tokenizer,
                checkpointDir,
                new LoraConfig { Rank = 2, Alpha = 4, Dropout = 0, TargetModules = ["q_proj", "v_proj"] },
                maxStepCount: 1);

            Assert.False(result.IsSuccess);
            Assert.Equal("DESIGN_CONVERSION_DPO_POLICY_BASE_PROVIDER_REQUIRED", result.ErrorCode);
            Assert.False(
                Directory.Exists(checkpointDir) && Directory.EnumerateFileSystemEntries(checkpointDir).Any(),
                "Missing provider must fail before training writes checkpoints or metrics.");
        }
        finally
        {
            if (Directory.Exists(checkpointDir))
                Directory.Delete(checkpointDir, recursive: true);
        }
    }

    [Fact]
    public async Task SubmitAndEvaluateAsync_DoesNotLeakRawPrivatePayloadsInResponse()
    {
        const string privateMarker = "PRIVATE_PASS117_SECRET_TOKEN";
        var api = CreateApi();
        var request = CreateRequest("pass117-private-api-request") with
        {
            DesignTree = new DesignConversionModelServerPayloadRef(
                RefId: "tree-ref",
                Kind: "figma-json",
                Payload: $"{{\"name\":\"{privateMarker}\"}}",
                Format: "figma-json"),
            OriginalTree = new DesignConversionModelServerPayloadRef(
                RefId: "original-tree-ref",
                Kind: "figma-json",
                Payload: $"{{\"name\":\"original-{privateMarker}\"}}",
                Format: "figma-json"),
            Html = $"<main data-secret=\"{privateMarker}\">Private</main>",
            Css = $".private{{background:url('{privateMarker}');}}",
            Metadata = new Dictionary<string, string>
            {
                ["private_note"] = privateMarker
            }
        };

        var result = await api.SubmitAndEvaluateAsync(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var json = JsonSerializer.Serialize(result.Value);
        Assert.DoesNotContain(privateMarker, json, StringComparison.Ordinal);
        Assert.DoesNotContain(request.Html, json, StringComparison.Ordinal);
        Assert.DoesNotContain(request.Css, json, StringComparison.Ordinal);
        Assert.Equal(bool.FalseString, result.Value!.BoundaryMetadata["raw_html_included"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["raw_design_tree_included"]);
    }

    [Fact]
    public async Task SubmitAndEvaluateAsync_RuntimeUnexpectedExceptionFailsClosedWithoutPrivateLeak()
    {
        const string privateExceptionMarker = "PRIVATE_API_BOUNDARY_RUNTIME_EXCEPTION_SECRET";
        const string privateRequestMarker = "PRIVATE_API_BOUNDARY_SUBMIT_REQUEST_SECRET";
        var rawDesignTree = $"{{\"type\":\"FRAME\",\"name\":\"{privateRequestMarker}\"}}";
        var rawOriginalTree = $"{{\"type\":\"FRAME\",\"name\":\"original-{privateRequestMarker}\"}}";
        var rawHtml = $"<main data-secret=\"{privateRequestMarker}\">Dashboard</main>";
        var rawCss = $".screen{{background-image:url('{privateRequestMarker}');}}";
        var api = new DesignConversionModelServerApi(
            new ThrowingThinClientJobEvaluator(privateExceptionMarker),
            () => DateTimeOffset.Parse("2026-07-02T10:00:00Z"),
            postEvaluationStateStore: TestPostEvaluationStateStore);
        var baseRequest = CreateRequest($"submit-boundary-{privateRequestMarker}");
        var request = baseRequest with
        {
            DesignTree = CreateRequest("submit-boundary-tree-base").DesignTree with { Payload = rawDesignTree },
            OriginalTree = CreateRequest("submit-boundary-original-base").OriginalTree with { Payload = rawOriginalTree },
            Html = rawHtml,
            Css = rawCss,
            Budget = baseRequest.Budget! with
            {
                BudgetUsd = 1.00m,
                MaxImprovementCandidates = 1,
                MaxImprovementIterations = 0
            },
            Metadata = new Dictionary<string, string>
            {
                ["private_note"] = privateRequestMarker
            }
        };

        OperationResult<DesignConversionModelServerApiResult>? result = null;
        var exception = await Record.ExceptionAsync(async () => result = await api.SubmitAndEvaluateAsync(request));

        Assert.Null(exception);
        Assert.NotNull(result);
        Assert.False(result!.IsSuccess);
        Assert.Equal(DesignConversionModelServerApi.UnexpectedRequestFailureErrorCode, result.ErrorCode);
        Assert.Equal(DesignConversionModelServerApi.UnexpectedRequestFailureErrorMessage, result.ErrorMessage);
        var errorFields = new[] { result.ErrorCode, result.ErrorMessage };
        foreach (var errorField in errorFields)
        {
            Assert.DoesNotContain(privateExceptionMarker, errorField, StringComparison.Ordinal);
            Assert.DoesNotContain(privateRequestMarker, errorField, StringComparison.Ordinal);
            Assert.DoesNotContain(rawHtml, errorField, StringComparison.Ordinal);
            Assert.DoesNotContain(rawCss, errorField, StringComparison.Ordinal);
            Assert.DoesNotContain(rawDesignTree, errorField, StringComparison.Ordinal);
            Assert.DoesNotContain(rawOriginalTree, errorField, StringComparison.Ordinal);
            Assert.DoesNotContain(request.RequestId, errorField, StringComparison.Ordinal);
            Assert.DoesNotContain(nameof(InvalidOperationException), errorField, StringComparison.Ordinal);
            Assert.DoesNotContain("Runtime evaluator failed", errorField, StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task GetJobResultAsync_StatusRestoreUnexpectedExceptionFailsClosedWithoutPrivateLeak()
    {
        const string privateExceptionMarker = "PRIVATE_API_BOUNDARY_STATUS_EXCEPTION_SECRET";
        const string privateJobMarker = "PRIVATE_API_BOUNDARY_STATUS_JOB_SECRET";
        var artifactDb = new TamperingLocalArtifactDb(
            PersistenceReadbackTamperMode.ReadbackException,
            privateExceptionMarker);
        var api = CreateApi(artifactDb);
        var jobId = $"job-{privateJobMarker}";

        OperationResult<DesignConversionModelServerStatusResponse>? result = null;
        var exception = await Record.ExceptionAsync(async () => result = await api.GetJobResultAsync(jobId));

        Assert.Null(exception);
        Assert.NotNull(result);
        Assert.False(result!.IsSuccess);
        Assert.Equal(DesignConversionModelServerApi.UnexpectedStatusFailureErrorCode, result.ErrorCode);
        Assert.Equal(DesignConversionModelServerApi.UnexpectedStatusFailureErrorMessage, result.ErrorMessage);
        var errorFields = new[] { result.ErrorCode, result.ErrorMessage };
        foreach (var errorField in errorFields)
        {
            Assert.DoesNotContain(privateExceptionMarker, errorField, StringComparison.Ordinal);
            Assert.DoesNotContain(privateJobMarker, errorField, StringComparison.Ordinal);
            Assert.DoesNotContain(jobId, errorField, StringComparison.Ordinal);
            Assert.DoesNotContain(nameof(InvalidOperationException), errorField, StringComparison.Ordinal);
            Assert.DoesNotContain("Fake readback exception", errorField, StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task DpoTrainerPolicyCheckpointBridge_UnavailableAuthorityFailsBeforeOptionsRecordsProviderOrArtifacts()
    {
        var outputDirectory = Path.Combine(
            Path.GetTempPath(),
            $"design-conversion-dpo-unavailable-teacher-authority-{Guid.NewGuid():N}");
        var runtimeCheckpointPath = Path.Combine(outputDirectory, "runtime.json");
        var provider = new DesignConversionDpoPolicyBaseTestProvider(_ =>
            throw new InvalidOperationException("provider-must-not-be-called"));

        try
        {
            var result = await DesignConversionDpoPolicyCheckpointBridge.ExecuteAndConvertAsync(
                ServerOwnedLegalTrainingTeacherAdmissionAuthority.Unavailable(),
                new ThrowOnAccessDpoTrainingRecordList(),
                outputDirectory,
                "unavailable-teacher-authority",
                runtimeCheckpointPath,
                provider,
                new DesignConversionDpoPolicyCheckpointBridgeOptions(0, 0));

            Assert.False(result.IsSuccess);
            Assert.Equal(
                LegalTrainingTeacherAdmissionAuthority.ConfigurationRequiredErrorCode,
                result.ErrorCode);
            Assert.Equal(0, provider.InvocationCount);
            Assert.Null(provider.LastModelSet);
            Assert.Null(result.Value);
            Assert.False(Directory.Exists(outputDirectory));
            Assert.False(File.Exists(runtimeCheckpointPath));
        }
        finally
        {
            if (Directory.Exists(outputDirectory))
                Directory.Delete(outputDirectory, recursive: true);
        }
    }

    [Fact]
    public async Task DpoTrainerPolicyCheckpointBridge_LegacyShimsFailClosedWithoutRecordsProviderOrArtifacts()
    {
        var outputDirectory = Path.Combine(
            Path.GetTempPath(),
            $"design-conversion-dpo-legacy-authority-shim-{Guid.NewGuid():N}");
        var runtimeCheckpointPath = Path.Combine(outputDirectory, "runtime.json");
        var provider = new DesignConversionDpoPolicyBaseTestProvider(_ =>
            throw new InvalidOperationException("provider-must-not-be-called"));
        var records = new ThrowOnAccessDpoTrainingRecordList();
        using var cancellationSource = new CancellationTokenSource();
        cancellationSource.Cancel();

        try
        {
#pragma warning disable CS0618
            var basic = await DesignConversionDpoPolicyCheckpointBridge.ExecuteAndConvertAsync(
                records,
                outputDirectory,
                "legacy-basic",
                runtimeCheckpointPath,
                provider,
                CancellationToken.None);
            var options = await DesignConversionDpoPolicyCheckpointBridge.ExecuteAndConvertAsync(
                records,
                outputDirectory,
                "legacy-options",
                runtimeCheckpointPath,
                provider,
                new DesignConversionDpoPolicyCheckpointBridgeOptions(0, 0),
                CancellationToken.None);
            var continuation = await DesignConversionDpoPolicyCheckpointBridge.ExecuteAndConvertAsync(
                records,
                outputDirectory,
                "legacy-continuation",
                runtimeCheckpointPath,
                provider,
                new DesignConversionDpoPolicyCheckpointBridgeOptions(0, 0),
                null!,
                CancellationToken.None);
            var preCancelled = await DesignConversionDpoPolicyCheckpointBridge.ExecuteAndConvertAsync(
                records,
                outputDirectory,
                "legacy-pre-cancelled",
                runtimeCheckpointPath,
                provider,
                cancellationSource.Token);
#pragma warning restore CS0618

            Assert.All(
                new[] { basic, options, continuation },
                result =>
                {
                    Assert.False(result.IsSuccess);
                    Assert.Equal(
                        LegalTrainingTeacherAdmissionAuthority.ConfigurationRequiredErrorCode,
                        result.ErrorCode);
                    Assert.Null(result.Value);
                });
            Assert.False(preCancelled.IsSuccess);
            Assert.Equal("CANCELLED", preCancelled.ErrorCode);
            Assert.Null(preCancelled.Value);
            Assert.Equal(0, provider.InvocationCount);
            Assert.Null(provider.LastModelSet);
            Assert.False(Directory.Exists(outputDirectory));
            Assert.False(File.Exists(runtimeCheckpointPath));
        }
        finally
        {
            if (Directory.Exists(outputDirectory))
                Directory.Delete(outputDirectory, recursive: true);
        }
    }

    [Theory]
    [InlineData(PersistenceReadbackTamperMode.ReadbackForeignCancellation)]
    [InlineData(PersistenceReadbackTamperMode.ReadbackFatalGraph)]
    public async Task GetJobResultAsync_StatusRestoreForeignCancellationOrFatalGraphEscapes(
        PersistenceReadbackTamperMode mode)
    {
        var artifactDb = new TamperingLocalArtifactDb(mode);
        var api = CreateApi(artifactDb);

        var exception = await Record.ExceptionAsync(
            () => api.GetJobResultAsync($"status-restore-escape-{mode}"));

        AssertReadbackExceptionGraphEscaped(exception, mode);
        Assert.Equal(1, artifactDb.ListCalls);
    }

    [Fact]
    public async Task EndpointRegistry_StatusRestoreUnexpectedException_ReturnsStatusFailedClosedHttp500WithoutPrivateLeak()
    {
        const string privateExceptionMarker = "PRIVATE_STATUS_ROUTE_EXCEPTION_SECRET";
        const string privateJobMarker = "PRIVATE_STATUS_ROUTE_JOB_SECRET";
        var artifactDb = new TamperingLocalArtifactDb(
            PersistenceReadbackTamperMode.ReadbackException,
            privateExceptionMarker);
        var api = CreateApi(artifactDb);
        var handler = new DesignConversionModelServerStatusHandler(api);
        var registry = new DesignConversionModelServerEndpointRegistry();
        var registered = registry.RegisterDesignConversionStatusReadback(handler);
        Assert.True(registered.IsSuccess, registered.ErrorMessage);
        var jobId = $"job-{privateJobMarker}";
        var request = new DesignConversionModelServerHttpRequest(
            Method: "GET",
            Path: DesignConversionModelServerStatusHandler.EndpointPathPrefix + jobId,
            BodyJson: null);

        OperationResult<DesignConversionModelServerHttpResponse>? invoked = null;
        var exception = await Record.ExceptionAsync(async () => invoked = await registry.InvokeAsync(request));

        Assert.Null(exception);
        Assert.NotNull(invoked);
        Assert.True(invoked!.IsSuccess, invoked.ErrorMessage);
        var response = Assert.IsType<DesignConversionModelServerHttpResponse>(invoked.Value);
        Assert.Equal("application/json; charset=utf-8", response.ContentType);
        Assert.Equal(500, response.StatusCode);
        Assert.Equal(
            DesignConversionModelServerStatusHandler.EndpointTemplatePath,
            response.Headers["x-design-conversion-path"]);

        using var document = JsonDocument.Parse(response.BodyJson);
        var root = document.RootElement;
        Assert.Equal(DesignConversionModelServerStatusHandler.EndpointTemplatePath, root.GetProperty("endpoint").GetString());
        Assert.Equal("GET", root.GetProperty("method").GetString());
        Assert.Equal(500, root.GetProperty("statusCode").GetInt32());
        Assert.Equal(DesignConversionModelServerApi.UnexpectedStatusFailureErrorCode, root.GetProperty("errorCode").GetString());
        Assert.Equal(DesignConversionModelServerApi.UnexpectedStatusFailureErrorMessage, root.GetProperty("errorMessage").GetString());

        var metadata = root.GetProperty("boundaryMetadata");
        Assert.Equal("500", metadata.GetProperty("http_status_code").GetString());
        Assert.Equal(DesignConversionModelServerStatusHandler.EndpointTemplatePath, metadata.GetProperty("http_path").GetString());
        Assert.Equal(DesignConversionModelServerStatusHandler.EndpointTemplatePath, metadata.GetProperty("route_admission_path").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_html_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_css_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_design_tree_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_user_private_data_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("response_contains_raw_private_data").GetString());

        Assert.DoesNotContain(privateExceptionMarker, response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(privateJobMarker, response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(jobId, response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(nameof(InvalidOperationException), response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain("Fake readback exception", response.BodyJson, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SubmitAndEvaluateAsync_BoundariesDoNotClaimDeploymentUploadOrFullReadiness()
    {
        var api = CreateApi();

        var result = await api.SubmitAndEvaluateAsync(CreateRequest("pass117-boundary-api-request"));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.False(result.Value!.PublicHttpEndpointVerified);
        Assert.False(result.Value.PlatformIntegrationVerified);
        Assert.False(result.Value.ModelTrainingVerified);
        Assert.False(result.Value.ModelUploadReady);
        Assert.False(result.Value.DeploymentVerified);
        Assert.False(result.Value.ProductReady);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["ModelUploadReady"]);
        Assert.Equal(bool.TrueString, result.Value.BoundaryMetadata["browser_render_not_executed"]);
        Assert.Equal(bool.TrueString, result.Value.BoundaryMetadata["visual_ai_not_executed"]);
        Assert.Equal("promotion_not_run", result.Value.BoundaryMetadata["promotion_status"]);
        Assert.Contains("deployment_verified=false", result.Value.Warnings);
        Assert.Contains(DesignConversionModelServerApi.HttpWiringBoundary, result.Value.Warnings);
    }

    [Fact]
    public async Task SubmitAndEvaluateAsync_MaxImprovementCandidatesPersistsOneRootBoundFinalRecord()
    {
        const string privateMarker = "PRIVATE_N8_13_SUBMIT_PATH_SECRET";
        var artifactDb = new TamperingLocalArtifactDb(PersistenceReadbackTamperMode.None);
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(CreateVerdict(
            "submit-path-loop-comparison-verdict",
            0.72,
            passed: false));
        var visualJudge = new SubmitPathScriptedVisualJudge(request =>
            request.Metadata["job_id"].Contains("loop-attempt:2", StringComparison.Ordinal)
                ? SubmitPathJudgeScript.FromScore(0.94, cost: 0.01m)
                : SubmitPathJudgeScript.FromScore(0.72, cost: 0.01m));
        var service = new DesignConversionInternalService(renderProvider, comparisonProvider, visualJudge);
        var api = new DesignConversionModelServerApi(
            new DesignConversionInternalServiceThinClientJobEvaluator(service),
            () => DateTimeOffset.Parse("2026-07-09T10:00:00Z"),
            artifactDb);
        var baseRequest = CreateRequest("submit-path-loop-request");
        var request = baseRequest with
        {
            Html = $"<main class=\"screen\" data-private=\"{privateMarker}\"><h1>Dashboard</h1></main>",
            Css = $".screen{{display:grid;gap:16px;--private:'{privateMarker}';}}",
            DesignTree = baseRequest.DesignTree with
            {
                Payload = $"{{\"type\":\"FRAME\",\"private\":\"{privateMarker}\"}}"
            },
            OriginalTree = baseRequest.OriginalTree with
            {
                Payload = $"{{\"type\":\"FRAME\",\"private\":\"original-{privateMarker}\"}}"
            },
            Budget = baseRequest.Budget! with { MaxImprovementCandidates = 3 },
            Screenshots =
            [
                new DesignConversionModelServerArtifactRef(
                    ArtifactId: "submit-path-loop-screenshot-desktop",
                    Kind: "screenshot",
                    Width: 1440,
                    Height: 900,
                    Uri: "artifact://submit-path-loop/screenshot-desktop.png",
                    Sha256: "sha256:submit-path-loop:screenshot-desktop"),
                new DesignConversionModelServerArtifactRef(
                    ArtifactId: "submit-path-loop-screenshot-mobile",
                    Kind: "screenshot",
                    Width: 390,
                    Height: 844,
                    Uri: "artifact://submit-path-loop/screenshot-mobile.png",
                    Sha256: "sha256:submit-path-loop:screenshot-mobile")
            ],
            DesignRenders =
            [
                new DesignConversionModelServerArtifactRef(
                    ArtifactId: "submit-path-loop-design-desktop",
                    Kind: "design-render",
                    Width: 1440,
                    Height: 900,
                    Uri: "artifact://submit-path-loop/design-desktop.png",
                    Sha256: "sha256:submit-path-loop:design-desktop"),
                new DesignConversionModelServerArtifactRef(
                    ArtifactId: "submit-path-loop-design-mobile",
                    Kind: "design-render",
                    Width: 390,
                    Height: 844,
                    Uri: "artifact://submit-path-loop/design-mobile.png",
                    Sha256: "sha256:submit-path-loop:design-mobile")
            ]
        };

        var result = await api.SubmitAndEvaluateAsync(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionThinClientStatus.Completed, result.Value!.ContractStatus);
        Assert.Equal(4, renderProvider.Requests.Count);
        Assert.Equal(4, comparisonProvider.Requests.Count);
        Assert.Equal(4, visualJudge.Requests.Count);
        Assert.Contains("dcm-local-improved", renderProvider.Requests[2].Html, StringComparison.Ordinal);
        Assert.Contains("dcm-local-improved", renderProvider.Requests[2].Css, StringComparison.Ordinal);
        Assert.Contains("dcm-local-improved", renderProvider.Requests[3].Html, StringComparison.Ordinal);
        Assert.Contains("dcm-local-improved", renderProvider.Requests[3].Css, StringComparison.Ordinal);
        Assert.Equal(result.Value.JobId, result.Value.RuntimeStatus.Provenance["internal_evaluation_job_id"]);
        AssertPersistenceVerified(result.Value.BoundaryMetadata, result.Value.JobId, request.RequestId);
        Assert.Contains(
            DesignConversionImprovementLoopThinClientJobEvaluator.SubmitPathLoopExecutedMarker,
            result.Value.EvidenceRefs);
        Assert.Contains(
            DesignConversionImprovementLoopThinClientJobEvaluator.SubmitPathLoopFinalEvaluationMarker,
            result.Value.EvidenceRefs);
        Assert.Contains("submit_path_improvement_loop_attempt_count=2", result.Value.Warnings);
        Assert.Contains(
            $"submit_path_improvement_loop_stop_reason={DesignConversionVisualQaBoundaryPolicy.QualityReached}",
            result.Value.Warnings);
        Assert.False(result.Value.ProductReady);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["product_ready"]);

        var records = await artifactDb.ListRecordsAsync(new DesignConversionLocalArtifactDbQuery(
            JobId: result.Value.JobId,
            RequestId: request.RequestId));
        Assert.True(records.IsSuccess, records.ErrorMessage);
        var record = Assert.Single(records.Value!);
        Assert.Equal(1, artifactDb.AppendCalls);
        Assert.Equal(result.Value.JobId, record.JobId);
        Assert.Equal(request.RequestId, result.Value.RequestId);
        Assert.Equal(request.RequestId, record.RequestId);
        Assert.Equal(nameof(DesignConversionThinClientJobRuntime), record.Metadata["artifact_db_single_writer_owner"]);
        Assert.Equal("root_submit_job_final_evaluation_result", record.Metadata["artifact_db_record_job_binding"]);
        Assert.Equal(bool.TrueString, record.Metadata["final_candidate_from_loop"]);
        Assert.Equal("2", record.Metadata["attempt_count"]);
        Assert.Equal("0.94", record.Metadata["final_visual_score"]);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.QualityReached, record.Metadata["final_loop_stop_reason"]);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.QualityReached, record.VisualJudgeStatus.TierDecision);

        var finalLoopJobId = $"{result.Value.JobId}:loop-attempt:2";
        var finalLoopRequestId = $"{request.RequestId}:runtime-improvement:2";
        var finalLoopJobRef = $"submit_path_improvement_loop_final_evaluation_job_ref={DesignConversionDpoLeakageGuard.HashRef("dcm-submit-path-final-job", finalLoopJobId)}";
        var finalLoopRequestRef = $"submit_path_improvement_loop_final_evaluation_request_ref={DesignConversionDpoLeakageGuard.HashRef("dcm-submit-path-final-request", finalLoopRequestId)}";
        Assert.Contains(finalLoopJobRef, result.Value.EvidenceRefs);
        Assert.Contains(finalLoopRequestRef, result.Value.EvidenceRefs);
        Assert.Contains(finalLoopJobRef, record.EvidenceRefs);
        Assert.Contains(finalLoopRequestRef, record.EvidenceRefs);
        Assert.DoesNotContain(finalLoopJobId, result.Value.RuntimeStatus.Provenance.Values);
        Assert.DoesNotContain(finalLoopRequestId, result.Value.RuntimeStatus.Provenance.Values);
        Assert.DoesNotContain(finalLoopJobId, record.Metadata.Values);
        Assert.DoesNotContain(finalLoopRequestId, record.Metadata.Values);
        Assert.Contains("improvement_loop:attempt=1", record.EvidenceRefs);
        Assert.Contains("improvement_loop:attempt=2", record.EvidenceRefs);
        Assert.Contains($"improvement_loop:stop_reason={DesignConversionVisualQaBoundaryPolicy.QualityReached}", record.EvidenceRefs);
        Assert.Contains("visual_decision:best_visual_score=0.94", result.Value.EvidenceRefs);
        Assert.Contains("visual_decision:best_visual_score=0.94", record.EvidenceRefs);

        Assert.All(record.RenderArtifactRefs, renderRef =>
        {
            Assert.StartsWith($"{result.Value.JobId}:render-request:", renderRef.ArtifactId, StringComparison.Ordinal);
            Assert.Contains($"render-result:{renderRef.ArtifactId}", result.Value.EvidenceRefs);
            Assert.Contains($"render-result:{renderRef.ArtifactId}", record.EvidenceRefs);
        });
        var finalJudgeRequestIds = Enumerable.Range(1, 2)
            .Select(index => $"{result.Value.JobId}:visual-judge-request:{index}")
            .ToArray();
        var receiptJudgeRequestIds = result.Value.BoundaryMetadata[
                ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVisualJudgeRequestIdKey]
            .Split('|', StringSplitOptions.RemoveEmptyEntries);
        var receiptProviderRequestIds = result.Value.BoundaryMetadata[
                ExternalHttpDesignConversionVisualJudge.ServerCallReceiptProviderRequestIdKey]
            .Split('|', StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(["none"], receiptJudgeRequestIds);
        Assert.Equal(["none"], receiptProviderRequestIds);
        Assert.Equal(
            bool.FalseString,
            result.Value.BoundaryMetadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptKey]);
        AssertUntrustedReceiptNetworkBoundary(result.Value.BoundaryMetadata);
        AssertNoAuthorityReceiptTuple(result.Value.BoundaryMetadata);
        var finalLoopJudgeRequestIds = Enumerable.Range(1, 2)
            .Select(index => $"{finalLoopJobId}:visual-judge-request:{index}")
            .ToArray();
        Assert.Equal(
            finalJudgeRequestIds.Concat(finalLoopJudgeRequestIds).ToArray(),
            visualJudge.Requests.Select(request => request.JudgeRequestId).ToArray());
        var persistedJudgeEvidenceRefs = record.VisualJudgeStatus.EvidenceRefs
            .Where(value => value.StartsWith("submit-path-visual-judge:", StringComparison.Ordinal))
            .ToArray();
        Assert.Equal(
            new[] { $"submit-path-visual-judge:{finalJudgeRequestIds[^1]}" },
            persistedJudgeEvidenceRefs);

        var htmlArtifact = Assert.Single(result.Value.RuntimeStatus.Artifacts, artifact => artifact.Kind == "improved-html");
        var cssArtifact = Assert.Single(result.Value.RuntimeStatus.Artifacts, artifact => artifact.Kind == "improved-css");
        Assert.Equal(htmlArtifact.Sha256, record.ConversionResult.HtmlSha256);
        Assert.Equal(cssArtifact.Sha256, record.ConversionResult.CssSha256);
        Assert.StartsWith("dcm-html_result-", record.ConversionResult.HtmlRef, StringComparison.Ordinal);
        Assert.StartsWith("dcm-css_result-", record.ConversionResult.CssRef, StringComparison.Ordinal);

        var status = await api.GetJobResultAsync(result.Value.JobId);
        Assert.True(status.IsSuccess, status.ErrorMessage);
        AssertPersistenceVerified(status.Value!.BoundaryMetadata, result.Value.JobId, request.RequestId);
        AssertUntrustedReceiptNetworkBoundary(status.Value.BoundaryMetadata);
        AssertNoAuthorityReceiptTuple(status.Value.BoundaryMetadata);
        Assert.Equal(1, artifactDb.AppendCalls);

        var replay = await api.SubmitAndEvaluateAsync(request);
        Assert.False(replay.IsSuccess);
        Assert.Equal("JOB_ALREADY_EXISTS", replay.ErrorCode);
        var statusAfterReplay = await api.GetJobResultAsync(result.Value.JobId);
        Assert.True(statusAfterReplay.IsSuccess, statusAfterReplay.ErrorMessage);
        AssertPersistenceVerified(statusAfterReplay.Value!.BoundaryMetadata, result.Value.JobId, request.RequestId);
        AssertUntrustedReceiptNetworkBoundary(statusAfterReplay.Value.BoundaryMetadata);
        AssertNoAuthorityReceiptTuple(statusAfterReplay.Value.BoundaryMetadata);
        Assert.Equal(1, artifactDb.AppendCalls);

        var allRecords = await artifactDb.ListRecordsAsync();
        Assert.True(allRecords.IsSuccess, allRecords.ErrorMessage);
        Assert.Single(allRecords.Value!);
        var publicResponseJson = JsonSerializer.Serialize(
            result.Value,
            DesignConversionModelServerHttpHandler.JsonOptions);
        var persistedRecordJson = JsonSerializer.Serialize(
            record,
            DesignConversionModelServerHttpHandler.JsonOptions);
        Assert.DoesNotContain(finalLoopJobId, publicResponseJson, StringComparison.Ordinal);
        Assert.DoesNotContain(finalLoopRequestId, publicResponseJson, StringComparison.Ordinal);
        Assert.DoesNotContain(finalLoopJobId, persistedRecordJson, StringComparison.Ordinal);
        Assert.DoesNotContain(finalLoopRequestId, persistedRecordJson, StringComparison.Ordinal);
        Assert.DoesNotContain(":loop-ref:", publicResponseJson, StringComparison.Ordinal);
        Assert.DoesNotContain(":loop-ref:", persistedRecordJson, StringComparison.Ordinal);
        Assert.Contains(finalLoopJobRef, publicResponseJson, StringComparison.Ordinal);
        Assert.Contains(finalLoopRequestRef, publicResponseJson, StringComparison.Ordinal);
        Assert.Contains(finalLoopJobRef, persistedRecordJson, StringComparison.Ordinal);
        Assert.Contains(finalLoopRequestRef, persistedRecordJson, StringComparison.Ordinal);
        AssertNoPersistencePublicLeak(
            publicResponseJson,
            privateMarker,
            request.Html,
            request.Css,
            artifactDb.RootDirectory);
        AssertNoPersistencePublicLeak(
            persistedRecordJson,
            privateMarker,
            request.Html,
            request.Css,
            artifactDb.RootDirectory);
        Assert.DoesNotContain(renderProvider.Requests[2].Html, publicResponseJson, StringComparison.Ordinal);
        Assert.DoesNotContain(renderProvider.Requests[2].Css, publicResponseJson, StringComparison.Ordinal);
        Assert.DoesNotContain(renderProvider.Requests[2].Html, persistedRecordJson, StringComparison.Ordinal);
        Assert.DoesNotContain(renderProvider.Requests[2].Css, persistedRecordJson, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SubmitAndEvaluateAsync_NegativeMaxImprovementCandidatesFailsBeforeRuntime()
    {
        const int maxImprovementCandidates = -1;
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(CreateVerdict(
            "submit-path-invalid-budget-comparison-verdict",
            0.72,
            passed: false));
        var visualJudge = new SubmitPathScriptedVisualJudge(_ => SubmitPathJudgeScript.FromScore(0.72, cost: 0.01m));
        var service = new DesignConversionInternalService(renderProvider, comparisonProvider, visualJudge);
        var api = new DesignConversionModelServerApi(
            service,
            () => DateTimeOffset.Parse("2026-07-09T10:00:00Z"),
            postEvaluationStateStore: TestPostEvaluationStateStore);
        var request = CreateRequest($"submit-path-invalid-budget-{maxImprovementCandidates}") with
        {
            Budget = CreateRequest("submit-path-invalid-budget-base").Budget! with
            {
                MaxImprovementCandidates = maxImprovementCandidates
            }
        };

        var result = await api.SubmitAndEvaluateAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal(DesignConversionInternalService.ImprovementCandidateBudgetRequiredCode, result.ErrorCode);
        Assert.Equal("DesignConversion improvement candidate budget must be greater than zero.", result.ErrorMessage);
        Assert.Empty(renderProvider.Requests);
        Assert.Empty(comparisonProvider.Requests);
        Assert.Empty(visualJudge.Requests);
    }

    [Fact]
    public async Task SubmitAndEvaluateAsync_MaxImprovementIterationsControlsSubmitPathLoopAttempts()
    {
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(CreateVerdict(
            "submit-path-typed-iterations-comparison-verdict",
            0.70,
            passed: false));
        var visualJudge = new SubmitPathScriptedVisualJudge(request =>
        {
            var jobId = request.Metadata["job_id"];
            return jobId.Contains("loop-attempt:3", StringComparison.Ordinal)
                ? SubmitPathJudgeScript.FromScore(0.82, cost: 0.01m)
                : jobId.Contains("loop-attempt:2", StringComparison.Ordinal)
                    ? SubmitPathJudgeScript.FromScore(0.76, cost: 0.01m)
                    : SubmitPathJudgeScript.FromScore(0.70, cost: 0.01m);
        });
        var service = new DesignConversionInternalService(renderProvider, comparisonProvider, visualJudge);
        var candidateStrategy = new SubmitPathScriptedCandidateStrategy(
            CreateSubmitPathCandidate("submit-path-typed-iterations-candidate-1", "PRIVATE_TYPED_ITERATIONS_CANDIDATE_ONE"),
            CreateSubmitPathCandidate("submit-path-typed-iterations-candidate-2", "PRIVATE_TYPED_ITERATIONS_CANDIDATE_TWO"));
        var api = new DesignConversionModelServerApi(
            new DesignConversionInternalServiceThinClientJobEvaluator(service),
            () => DateTimeOffset.Parse("2026-07-09T10:00:00Z"),
            new JsonlDesignConversionLocalArtifactDb(CreateArtifactDbTempRoot("typed-iterations")),
            improvementCandidateStrategy: candidateStrategy);
        var baseRequest = CreateResponsiveSubmitPathRequest("submit-path-typed-iterations-route");
        var request = baseRequest with
        {
            Budget = baseRequest.Budget! with
            {
                Tier = "internal",
                MaxImprovementCandidates = 4,
                BudgetUsd = 1.00m,
                MaxImprovementIterations = 2
            }
        };

        var result = await api.SubmitAndEvaluateAsync(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        AssertSubmitPathNonReadyVisualDecision(
            result.Value!,
            DesignConversionVisualQaBoundaryPolicy.DecisionBlocked,
            DesignConversionVisualQaBoundaryPolicy.BudgetCapReached);
        Assert.Equal(6, renderProvider.Requests.Count);
        Assert.Equal(6, comparisonProvider.Requests.Count);
        Assert.Equal(6, visualJudge.Requests.Count);
        Assert.Equal(3, renderProvider.Requests.Select(request => request.Metadata["job_id"]).Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(2, candidateStrategy.Requests.Count);
        Assert.Equal(1, candidateStrategy.Requests[0].CompletedAttemptNumber);
        Assert.Equal(2, candidateStrategy.Requests[1].CompletedAttemptNumber);
        Assert.Contains(
            DesignConversionImprovementLoopThinClientJobEvaluator.SubmitPathLoopExecutedMarker,
            result.Value.EvidenceRefs);
        Assert.Contains("submit_path_improvement_loop_attempt_count=3", result.Value.Warnings);
        Assert.Contains("submit_path_improvement_loop_max_attempts=3", result.Value.Warnings);
        Assert.Contains(
            $"submit_path_improvement_loop_max_attempts_source={DesignConversionVisualQaBoundaryPolicy.TypedMaxImprovementIterationsSource}",
            result.Value.Warnings);
        Assert.Contains(
            $"submit_path_improvement_loop_stop_reason={DesignConversionVisualQaBoundaryPolicy.BudgetCapReached}",
            result.Value.Warnings);
        Assert.Contains(
            $"submit_path_improvement_loop_max_attempts_source={DesignConversionVisualQaBoundaryPolicy.TypedMaxImprovementIterationsSource}",
            result.Value.EvidenceRefs);
        Assert.Contains("tier_policy:submitted_max_improvement_candidates=4", result.Value.EvidenceRefs);
        Assert.Contains("tier_policy:effective_max_improvement_candidates=3", result.Value.EvidenceRefs);
    }

    [Fact]
    // Премиса прежней проверки была неработоспособна: её оснастка создавала
    // корень репозитория БЕЗ запечатанного набора, поэтому репозиторный
    // маршрут не разрешался никогда и обновление кандидата не достигалось.
    // Оснастка починена, и проверка закрепляет объявленный договор продукта:
    // когда точный репозиторный набор уже запечатан на диске, цикл НЕ ДОЛЖЕН
    // синтезировать ещё один локальный учебный кандидат, потому что властью
    // следующего учебного шага являются сохранённые байты набора.
    public async Task SubmitAndEvaluateAsync_PublicRepositoryOwnedDpoContextIsHonouredBySealedDatasetReuseStop()
    {
        var artifactDbRoot = CreateArtifactDbTempRoot(
            "repository-owned-dpo-context-public-map");
        var repositoryRoot = CreateRepositoryOwnedDpoContextTempRoot(
            "repository-owned-dpo-context-public-map");
        try
        {
            var renderProvider = new RecordingRenderProvider();
            var comparisonProvider = new RecordingComparisonProvider(
                CreateVerdict(
                    "repository-owned-dpo-context-map-verdict",
                    0.70,
                    passed: false));
            var visualJudge = new SubmitPathScriptedVisualJudge(
                _ => SubmitPathJudgeScript.FromScore(0.70, cost: 0.01m));
            var service = new DesignConversionInternalService(
                renderProvider,
                comparisonProvider,
                visualJudge);
            var candidateStrategy = new SubmitPathScriptedCandidateStrategy(
                CreateSubmitPathCandidate(
                    "repository-owned-dpo-context-map-candidate",
                    "PRIVATE_REPOSITORY_OWNED_DPO_CONTEXT_MAP"));
            var api = new DesignConversionModelServerApi(
                new DesignConversionInternalServiceThinClientJobEvaluator(service),
                () => DateTimeOffset.Parse("2026-07-09T10:00:00Z"),
                new JsonlDesignConversionLocalArtifactDb(artifactDbRoot),
                improvementCandidateStrategy: candidateStrategy,
                postEvaluationStateStore: TestPostEvaluationStateStore);
            var context = CreateValidRepositoryOwnedDpoTransactionContext(
                repositoryRoot);
            // Запечатываем НАСТОЯЩИЙ набор по адресу и личности этого договора.
            // Без этого разрешение набора всегда недоступно, и ветка
            // переиспользования, которую проверка и обязана доказывать,
            // недостижима в принципе.
            // Хеш описи запоминаем: ниже он становится ожиданием для
            // доказательства, что продукт выдал опись именно этого набора.
            var sealedManifestSha256 =
                SealRepositoryOwnedDpoDatasetFor(context);
            var baseRequest = CreateResponsiveSubmitPathRequest(
                "repository-owned-dpo-context-public-map");
            var request = baseRequest with
            {
                RepositoryOwnedDpoTransactionContext = context,
                Budget = baseRequest.Budget! with
                {
                    Tier = "pro",
                    BudgetUsd = 1.00m,
                    MaxImprovementCandidates = 2,
                    MaxImprovementIterations = 1,
                    MaxTeacherCalls = 1
                }
            };

            var result = await api.SubmitAndEvaluateAsync(request);

            Assert.True(result.IsSuccess, result.ErrorMessage);

            // Нулевое: оснастка действительно довела маршрут до набора.
            // Предупреждения о недоступном наборе быть не должно — иначе
            // проверка снова судила бы не ту ветку.
            // Сверяем ПО НАЧАЛУ строки, а не на равенство: продукт всегда
            // приписывает к этому предупреждению двоеточие и код ошибки, и
            // сравнение на равенство с «голым» литералом не совпало бы
            // никогда — такая проверка была бы пустой и пропустила бы отказ.
            Assert.DoesNotContain(
                result.Value!.Warnings,
                warning => warning.StartsWith(
                    DesignConversionImprovementLoopCoordinator
                        .RepositoryOwnedDpoDatasetResolutionFailedWarning,
                    StringComparison.Ordinal));

            // Первое: цикл исполнялся, то есть остановка — завершение
            // маршрута, а не пропуск цикла.
            Assert.Contains(
                DesignConversionImprovementLoopThinClientJobEvaluator
                    .SubmitPathLoopExecutedMarker,
                result.Value.EvidenceRefs);

            // Второе: сработала именно досрочная остановка переиспользования.
            Assert.Contains(
                "submit_path_improvement_loop_stop_reason=" +
                DesignConversionImprovementLoopCoordinator
                    .RepositoryOwnedDpoDatasetExistingReusedStop,
                result.Value.Warnings);

            // Третье: договор соблюдён — личность запроса дошла до
            // доказательств завершения и совпадает по ХЕШУ с присланной.
            // Тождество объекта больше не проверяем: на этом маршруте оно
            // ничего не значит.
            Assert.Equal(
                context.RequestIdentity!.RequestIdentitySha256,
                result.Value.RuntimeStatus.Provenance[
                    DesignConversionRepositoryOwnedDpoTransactionContext
                        .RequestIdentitySha256MetadataKey]);
            Assert.Contains(
                "improvement_loop:" +
                DesignConversionRepositoryOwnedDpoTransactionContext
                    .ValidationMetadataKey +
                "=" + bool.TrueString,
                result.Value.EvidenceRefs);

            // Четвёртое: источник разрешения набора — ранее запечатанный
            // репозиторный набор. Вот это и есть доказательство
            // переиспользования, потому что при отказе разрешения продукт
            // пишет сюда «недоступен». Ожидаемое значение берём из самого
            // члена перечисления, а не вписываем строкой: при переименовании
            // члена проверка перестанет собираться, а не промолчит.
            Assert.Contains(
                "improvement_loop:" +
                DesignConversionImprovementLoopCoordinator
                    .RepositoryOwnedDpoDatasetResolutionSourceMetadataKey +
                "=" +
                RepositoryOwnedDpoDatasetResolutionSource
                    .ExistingSealedRepositoryDataset.ToString(),
                result.Value.EvidenceRefs);

            // Пятое: приписана отметка переиспользования. Продукт ставит её
            // только в ветке ранее запечатанного набора, поэтому это второе
            // независимое доказательство той же ветки.
            Assert.Contains(
                DesignConversionImprovementLoopCoordinator
                    .RepositoryOwnedDpoDatasetExistingReusedMarker,
                result.Value.EvidenceRefs);

            // Шестое: число пар взято из описи, прочитанной с диска, —
            // оснастка выше запечатывает ровно одну пару. Это и доказывает,
            // что набор прочитан как ранее запечатанный, а не собран заново.
            Assert.Contains(
                "improvement_loop:" +
                DesignConversionImprovementLoopCoordinator
                    .RepositoryOwnedDpoDatasetPairCountMetadataKey +
                "=1",
                result.Value.EvidenceRefs);

            // Седьмое: хеш описи равен хешу той описи, которую оснастка выше
            // запечатала на диск. Это сильнее числа пар: совпадение хеша
            // доказывает, что продукт прочитал именно наш набор, а не любой
            // другой набор с одной парой. Ожидание взято из результата
            // запечатывания, поэтому подогнать его под продукт нельзя.
            Assert.Contains(
                "improvement_loop:" +
                DesignConversionImprovementLoopCoordinator
                    .RepositoryOwnedDpoDatasetManifestSha256MetadataKey +
                "=" + sealedManifestSha256,
                result.Value.EvidenceRefs);

            // Восьмое: отметка состоявшегося разрешения набора. Её настоящая
            // ценность — в безусловности: продукт ставит эту отметку первой же
            // строкой метода выдачи доказательств разрешения, ДО всякого
            // ветвления (согласователь цикла улучшений, строка 3225), а три
            // величины выше выдаёт уже в успешной ветке того же метода (там
            // же, строки 3242, 3244 и 3246). Значит, если величина появилась,
            // отметка там уже стоит, и различать «откуда взялись величины»
            // отметка не может. Закреплять её всё равно нужно: она — то
            // единственное доказательство, что продукт дошёл до разрешения
            // набора и доложил об этом независимо от исхода, поэтому она
            // краснеет, если продукт перестанет докладывать о самом факте
            // разрешения. Прежнее обоснование «без неё все величины выше могли
            // бы появиться из другого места» опровергнуто независимой
            // проверкой второго круга и заменено этим.
            Assert.Contains(
                DesignConversionImprovementLoopCoordinator
                    .RepositoryOwnedDpoDatasetResolutionMarker,
                result.Value.EvidenceRefs);

            // Прежнее закрепление признака «локальный учебный кандидат не
            // создан» снято как неопровержимое: продукт печатает это значение
            // жёстко вписанной ложью во всех ЧЕТЫРЁХ местах выдачи и истиной
            // не печатает нигде. Три из них — записи в список доказательств
            // (согласователь цикла улучшений, строки 969, 3235 и 3248),
            // четвёртое — присваивание того же признака в открытых сведениях
            // результата, когда к результату подмешиваются данные о разрешении
            // набора (там же, строка 3269). Перечисление имён ключей (там же,
            // строка 4563) выдачей значения не является и в счёт не идёт.
            // Поэтому никакое изменение поведения продукта это утверждение
            // уронить не могло — оно падало бы только от удаления самой строки
            // выдачи. Объявленный договор «локальный кандидат не создан»
            // остаётся непокрытым и записан как отдельный долг покрытия.

            // Девятое — вспомогательное наблюдение о внедрённой стратегии
            // кандидатов, а НЕ договор: на этом маршруте её не опрашивают.
            // Само по себе пустое обращение остановку переиспользования не
            // доказывает: поток управления от неё не зависит, и при сломанной
            // остановке список обращений тоже остался бы пустым.
            Assert.Empty(candidateStrategy.Requests);
        }
        finally
        {
            DeleteDirectoryIfExists(artifactDbRoot);
            DeleteDirectoryIfExists(repositoryRoot);
        }
    }

    [Fact]
    // ЭТО ЗАКРЕПЛЕНИЕ НАБЛЮДЕНИЯ, А НЕ ОБЪЯВЛЕННЫЙ ДОГОВОР ПРОДУКТА.
    // Наблюдение: когда репозиторный набор обучающих пар недоступен (по
    // адресу договора ничего не запечатано), публичный вход отправки не
    // уходит на запасной путь и не синтезирует локального кандидата — он
    // отказывается продолжать учебный шаг, приписывает предупреждение о
    // недоступности набора и ставит причиной остановки «стратегия
    // кандидатов не справилась».
    // Честная граница: как продуктовое намерение это поведение НЕ объявлено.
    // Вопрос «отказывать или уходить на запасной путь при недоступном
    // наборе» задан владельцу и ею не решён. Проверка существует, чтобы
    // наблюдённое поведение нельзя было изменить молча: если владелец
    // выберет запасной путь, эта проверка должна осознанно покраснеть и быть
    // переписана вместе с решением, а не остаться незамеченной.
    // Честная граница МАРШРУТА — что именно здесь закреплено, а что нет.
    // Закреплён ответ ОДНОГО маршрута: публичного входа отправки, которому
    // оснастка передала базу артефактов. Настоящий механизм такой.
    // Во-первых, обёртку авторской стратегии учителя навешивает сам публичный
    // вход: когда база артефактов передана, он оборачивает внедрённую
    // оснасткой стратегию кандидатов в авторскую стратегию учителя (сервер
    // модели преобразования дизайна, файл
    // 25.ModelServer/DesignConversion/DesignConversionModelServerApi.cs,
    // строки 366-371). Обе проверки этого слайса базу артефактов передают,
    // поэтому обёртка здесь есть всегда.
    // Во-вторых, согласователь цикла улучшений обёрнутую стратегию оставляет
    // КАК ЕСТЬ и повторно её не оборачивает (согласователь цикла улучшений,
    // строки 430-435). Условие там составное, через «или», и срабатывает здесь
    // ПЕРВАЯ его часть: у самого согласователя база артефактов пуста
    // (`artifactDb is null`), а при пустой базе он вообще не заворачивает
    // стратегию. Вторая часть — «стратегия уже реализует договор авторской
    // стратегии учителя» — на этом маршруте тоже истинна, потому что обёртку
    // уже навесил публичный вход, но решение принимает не она: до неё
    // вычисление не доходит из-за короткого замыкания «или». Разница важна
    // ровно в одном: если бы базу артефактов согласователю передали, вывод
    // «оставляет КАК ЕСТЬ» держался бы уже на второй части, и только на ней.
    // В-третьих, из этого следует, что ветка авторской стратегии учителя
    // входима, и разрешение набора происходит на ПЕРВОМ обращении внутри
    // цикла (там же, строка 946), а не на втором. Второе обращение (там же,
    // строки 1540-1576) лежит внутри метода завершения цикла и в этих двух
    // проверках не достигается: первое обращение возвращает результат раньше
    // — либо остановкой переиспользования (там же, строка 990), либо отказом
    // «стратегия кандидатов не справилась» (там же, строка 980).
    // В-четвёртых, пар, авторованных учителем, здесь ноль не потому, что
    // стратегии нет, а потому, что первое обращение возвращает результат ДО
    // платного вызова учителя (там же, строка 1005).
    // НЕ покрыто: сторож платного вызова учителя — и здесь надо различать два
    // разных события, иначе видно противоречие там, где его нет.
    // Событие первое: заявка на платный вызов СОЗДАЁТСЯ И ВЫДАЁТСЯ УСПЕШНО.
    // При нуле наблюдённых обращений к учителю заявка выдаётся, проверка её
    // успешности проходит (согласователь цикла улучшений, строки 914-917), и
    // именно этот успех открывает путь дальше — к первому обращению внутри
    // цикла. Поэтому сторож на этом маршруте не «недостижим», как говорилось
    // раньше: эта прежняя формулировка была следствием той же ложной посылки.
    // Запись попытки 0004 говорит ровно то же самое словами «заявка выдаётся»,
    // и она права.
    // Событие второе: разрешение на САМ платный вызов учителя. Вот оно здесь
    // ни разу и не наступает — обе проверки возвращают результат до обращения
    // к учителю (там же, строка 1005), поэтому заявка так и остаётся
    // неиспользованной.
    // Отсюда и граница покрытия: эти две проверки НЕ проверяют поведение
    // сторожа при исчерпанном бюджете, то есть отказ в платном вызове. Они
    // проверяют только то, что выдача заявки не мешает пройти дальше.
    // НЕ покрыто: изъятие для чистого промаха на публичном входе отправки
    // мертво — но по другой причине, чем говорилось раньше. Ветка достижима;
    // мертва она потому, что у САМОГО согласователя база артефактов пуста:
    // публичный вход отдаёт её только как базу независимых предпочтений и
    // параметр базы артефактов согласователя не заполняет (оценщик цикла
    // улучшений, файл
    // 44.DesignConversionDomain/DesignConversionThinClientJobRuntime.cs,
    // строки 693-699), а условие чистого промаха требует непустой базы
    // артефактов (согласователь цикла улучшений, строка 955). На маршруте,
    // где у согласователя база артефактов есть, продукт, наоборот, РАЗРЕШАЕТ
    // создание набора при чистом промахе, и это поведение здесь не
    // закрепляется.
    // Поправка от независимой проверки второго круга: прежняя формулировка
    // этого блока утверждала, что публичный вход создаёт согласователь без
    // базы артефактов, «поэтому авторская стратегия учителя не подключается»
    // и «разрешение набора неизбежно приходит на ВТОРОЕ обращение». Верным в
    // ней было только одно — что база артефактов самого согласователя пуста.
    // Неверными были причинно-следственная связь и вывод о втором обращении:
    // обёртку навешивает публичный вход (строки 366-371 сервера модели),
    // согласователь её сохраняет (строки 430-435), и разрешение случается на
    // первом обращении. Вывод «изъятие для чистого промаха на этом маршруте
    // мертво» остался верным, но держится теперь на пустой базе артефактов
    // согласователя, а не на недостижимости ветки.
    // Итог: отсюда НЕЛЬЗЯ заключить, что отказ — правильное поведение
    // продукта вообще. Это ответ одного маршрута; решение о правильном
    // поведении остаётся за владельцем и остаётся открытым.
    public async Task SubmitAndEvaluateAsync_PublicRepositoryOwnedDpoContextWithoutSealedDatasetRefuses_ObservedBehaviourPin()
    {
        var artifactDbRoot = CreateArtifactDbTempRoot(
            "repository-owned-dpo-context-public-map-unavailable");
        var repositoryRoot = CreateRepositoryOwnedDpoContextTempRoot(
            "repository-owned-dpo-context-public-map-unavailable");
        try
        {
            var renderProvider = new RecordingRenderProvider();
            var comparisonProvider = new RecordingComparisonProvider(
                CreateVerdict(
                    "repository-owned-dpo-context-map-unavailable-verdict",
                    0.70,
                    passed: false));
            var visualJudge = new SubmitPathScriptedVisualJudge(
                _ => SubmitPathJudgeScript.FromScore(0.70, cost: 0.01m));
            var service = new DesignConversionInternalService(
                renderProvider,
                comparisonProvider,
                visualJudge);
            var candidateStrategy = new SubmitPathScriptedCandidateStrategy(
                CreateSubmitPathCandidate(
                    "repository-owned-dpo-context-map-unavailable-candidate",
                    "PRIVATE_REPOSITORY_OWNED_DPO_CONTEXT_MAP_UNAVAILABLE"));
            var api = new DesignConversionModelServerApi(
                new DesignConversionInternalServiceThinClientJobEvaluator(service),
                () => DateTimeOffset.Parse("2026-07-09T10:00:00Z"),
                new JsonlDesignConversionLocalArtifactDb(artifactDbRoot),
                improvementCandidateStrategy: candidateStrategy,
                postEvaluationStateStore: TestPostEvaluationStateStore);
            // Набор НЕ запечатываем: это и есть наблюдаемое условие.
            var context = CreateValidRepositoryOwnedDpoTransactionContext(
                repositoryRoot);
            var baseRequest = CreateResponsiveSubmitPathRequest(
                "repository-owned-dpo-context-public-map-unavailable");
            var request = baseRequest with
            {
                RepositoryOwnedDpoTransactionContext = context,
                Budget = baseRequest.Budget! with
                {
                    Tier = "pro",
                    BudgetUsd = 1.00m,
                    MaxImprovementCandidates = 2,
                    MaxImprovementIterations = 1,
                    MaxTeacherCalls = 1
                }
            };

            var result = await api.SubmitAndEvaluateAsync(request);

            // Наблюдение: сам вызов завершается успешно, отказ выражен
            // предупреждениями и причиной остановки, а не ошибкой вызова.
            Assert.True(result.IsSuccess, result.ErrorMessage);

            // Первое наблюдение: цикл дошёл до учебного шага.
            Assert.Contains(
                DesignConversionImprovementLoopThinClientJobEvaluator
                    .SubmitPathLoopExecutedMarker,
                result.Value!.EvidenceRefs);

            // Второе наблюдение: приписано предупреждение о недоступности
            // набора, и закрепляем его ПОЛНОЙ строкой — вместе с кодом ошибки
            // в хвосте. Весь диагноз несёт именно код: при сверке только по
            // началу строки подмена одного кода другим оставила бы проверку
            // зелёной, хотя причина отказа стала бы другой, а эта проверка
            // существует ровно затем, чтобы поведение нельзя было сменить
            // молча. Значение кода определено однозначно и потому безопасно
            // для закрепления: продукт пропускает код через чистильщик,
            // который допускает только заглавные латинские буквы, цифры и
            // подчёркивание, иначе подставляет код недоступности разрешителя
            // набора.
            Assert.Contains(
                DesignConversionImprovementLoopCoordinator
                    .RepositoryOwnedDpoDatasetResolutionFailedWarning +
                ":" + RepositoryOwnedDpoDatasetResolver.MissingErrorCode,
                result.Value.Warnings);
            // Прежняя дополнительная сверка по началу строки снята как
            // мёртвая: она стояла ПОСЛЕ точного совпадения полной строки и
            // строго им подчинена — если полная строка найдена, строка с
            // таким началом есть заведомо, и упасть такая сверка не могла
            // никогда. Её обоснование «ловит исчезновение самого
            // предупреждения независимо от кода» было ложным: исчезновение
            // ловит и точное совпадение выше.

            // Третье наблюдение: причина остановки — «стратегия кандидатов
            // не справилась», а не остановка переиспользования.
            Assert.Contains(
                "submit_path_improvement_loop_stop_reason=" +
                DesignConversionImprovementLoopCoordinator
                    .CandidateStrategyFailed,
                result.Value.Warnings);
            Assert.DoesNotContain(
                "submit_path_improvement_loop_stop_reason=" +
                DesignConversionImprovementLoopCoordinator
                    .RepositoryOwnedDpoDatasetExistingReusedStop,
                result.Value.Warnings);

            // Четвёртое наблюдение — вспомогательное, о внедрённой стратегии
            // кандидатов: на этом маршруте её не опрашивают. Это НЕ то же
            // самое, что «локальный учебный кандидат не создан»: пустой
            // список обращений говорит только о стратегии, поток управления
            // от него не зависит, и сам по себе он отсутствие запасного пути
            // не доказывает. Именно отсутствие запасного пути владелец и
            // может решить иначе.
            Assert.Empty(candidateStrategy.Requests);
        }
        finally
        {
            DeleteDirectoryIfExists(artifactDbRoot);
            DeleteDirectoryIfExists(repositoryRoot);
        }
    }

    // Отдельное доказательство, что отказ по негодному клиентскому договору не
    // ослаб вместе с обновлением премисы предыдущей проверки. Клиент включает
    // репозиторный маршрут, но присылает договор без адреса и без личности —
    // такой гейт проверки обязан отклонить, и подменить серверную личность
    // клиент не может.
    [Fact]
    public async Task SubmitAndEvaluateAsync_InvalidClientRepositoryOwnedDpoContextStaysRefused()
    {
        var artifactDbRoot = CreateArtifactDbTempRoot(
            "repository-owned-dpo-context-invalid-refusal");
        try
        {
            var renderProvider = new RecordingRenderProvider();
            var comparisonProvider = new RecordingComparisonProvider(
                CreateVerdict(
                    "repository-owned-dpo-context-invalid-verdict",
                    0.70,
                    passed: false));
            var visualJudge = new SubmitPathScriptedVisualJudge(
                _ => SubmitPathJudgeScript.FromScore(0.70, cost: 0.01m));
            var service = new DesignConversionInternalService(
                renderProvider,
                comparisonProvider,
                visualJudge);
            var candidateStrategy = new SubmitPathScriptedCandidateStrategy(
                CreateSubmitPathCandidate(
                    "repository-owned-dpo-context-invalid-candidate",
                    "PRIVATE_REPOSITORY_OWNED_DPO_CONTEXT_INVALID"));
            var api = new DesignConversionModelServerApi(
                new DesignConversionInternalServiceThinClientJobEvaluator(service),
                () => DateTimeOffset.Parse("2026-11-09T10:00:00Z"),
                new JsonlDesignConversionLocalArtifactDb(artifactDbRoot),
                improvementCandidateStrategy: candidateStrategy,
                postEvaluationStateStore: TestPostEvaluationStateStore);

            // Включённый маршрут без адреса и без личности: гейт проверки
            // договора отклоняет именно такие адреса, чтобы их нельзя было
            // считать совещательными данными.
            var invalidContext =
                new DesignConversionRepositoryOwnedDpoTransactionContext(
                    Enabled: true,
                    Address: null,
                    RequestIdentity: null);
            // Отказ гейта фиксируем прямо, до прогона: это предпосылка
            // проверки, а не её вывод.
            var gate = invalidContext.Validate();
            Assert.False(gate.IsSuccess);
            Assert.Equal(
                DesignConversionRepositoryOwnedDpoTransactionContext
                    .InvalidErrorCode,
                gate.ErrorCode);

            var baseRequest = CreateResponsiveSubmitPathRequest(
                "repository-owned-dpo-context-invalid-refusal");
            var request = baseRequest with
            {
                RepositoryOwnedDpoTransactionContext = invalidContext,
                Budget = baseRequest.Budget! with
                {
                    Tier = "pro",
                    BudgetUsd = 1.00m,
                    MaxImprovementCandidates = 2,
                    MaxImprovementIterations = 1,
                    MaxTeacherCalls = 1
                }
            };

            var result = await api.SubmitAndEvaluateAsync(request);

            Assert.True(result.IsSuccess, result.ErrorMessage);

            // Первое: отказ по негодному договору назван предупреждением с
            // точным кодом, а не проглочен.
            Assert.Contains(
                DesignConversionImprovementLoopCoordinator
                    .RepositoryOwnedDpoTransactionPreflightFailedWarning +
                ":" + DesignConversionRepositoryOwnedDpoTransactionContext
                    .InvalidErrorCode,
                result.Value!.Warnings);

            // Второе: метка действительности репозиторного договора на этом
            // маршруте ЛОЖНА — то есть негодный включённый договор годным не
            // объявляется.
            Assert.Contains(
                "improvement_loop:" +
                DesignConversionRepositoryOwnedDpoTransactionContext
                    .ValidationMetadataKey +
                "=" + bool.FalseString,
                result.Value.EvidenceRefs);

            // Третье: клиент не подменил серверную личность — личности в
            // доказательствах нет вовсе, потому что негодный договор её не
            // несёт.
            Assert.DoesNotContain(
                DesignConversionRepositoryOwnedDpoTransactionContext
                    .RequestIdentitySha256MetadataKey,
                result.Value.RuntimeStatus.Provenance.Keys);
        }
        finally
        {
            DeleteDirectoryIfExists(artifactDbRoot);
        }
    }

    [Fact]
    public async Task SubmitAndEvaluateAsync_MaxImprovementIterationsZeroRunsInitialEvaluationOnly()
    {
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(CreateVerdict(
            "submit-path-typed-zero-iterations-comparison-verdict",
            0.70,
            passed: false));
        var visualJudge = new SubmitPathScriptedVisualJudge(_ => SubmitPathJudgeScript.FromScore(0.70, cost: 0.01m));
        var service = new DesignConversionInternalService(renderProvider, comparisonProvider, visualJudge);
        var candidateStrategy = new RecordingUnexpectedCandidateStrategy();
        var api = new DesignConversionModelServerApi(
            new DesignConversionInternalServiceThinClientJobEvaluator(service),
            () => DateTimeOffset.Parse("2026-07-09T10:00:00Z"),
            new JsonlDesignConversionLocalArtifactDb(CreateArtifactDbTempRoot("typed-zero-iterations")),
            improvementCandidateStrategy: candidateStrategy);
        var baseRequest = CreateResponsiveSubmitPathRequest("submit-path-typed-zero-iterations-route");
        var request = baseRequest with
        {
            Budget = baseRequest.Budget! with
            {
                Tier = "pro",
                MaxImprovementCandidates = 4,
                BudgetUsd = 1.00m,
                MaxImprovementIterations = 0
            }
        };

        var result = await api.SubmitAndEvaluateAsync(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        AssertSubmitPathNonReadyVisualDecision(
            result.Value!,
            DesignConversionVisualQaBoundaryPolicy.DecisionBlocked,
            DesignConversionVisualQaBoundaryPolicy.BudgetCapReached);
        Assert.Equal(2, renderProvider.Requests.Count);
        Assert.Equal(2, comparisonProvider.Requests.Count);
        Assert.Equal(2, visualJudge.Requests.Count);
        Assert.Empty(candidateStrategy.Requests);
        Assert.DoesNotContain(
            DesignConversionImprovementLoopThinClientJobEvaluator.SubmitPathLoopExecutedMarker,
            result.Value.EvidenceRefs);
        Assert.DoesNotContain(result.Value.Warnings, warning =>
            warning.StartsWith("submit_path_improvement_loop_attempt_count=", StringComparison.Ordinal));
        Assert.Contains("tier_policy:submitted_max_improvement_candidates=4", result.Value.EvidenceRefs);
        Assert.Contains("tier_policy:effective_max_improvement_candidates=1", result.Value.EvidenceRefs);
    }

    public static TheoryData<int> InvalidTypedMaxImprovementIterationsCases()
        => new()
        {
            -1,
            DesignConversionImprovementLoopCoordinator.MaxAllowedAttempts,
            DesignConversionImprovementLoopCoordinator.MaxAllowedAttempts + 1
        };

    [Theory]
    [MemberData(nameof(InvalidTypedMaxImprovementIterationsCases))]
    public async Task SubmitAndEvaluateAsync_InvalidMaxImprovementIterationsFailsBeforeRuntime(
        int maxImprovementIterations)
    {
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(CreateVerdict(
            "submit-path-invalid-typed-iterations-comparison-verdict",
            0.70,
            passed: false));
        var visualJudge = new SubmitPathScriptedVisualJudge(_ => SubmitPathJudgeScript.FromScore(0.70, cost: 0.01m));
        var service = new DesignConversionInternalService(renderProvider, comparisonProvider, visualJudge);
        var api = new DesignConversionModelServerApi(service, () => DateTimeOffset.Parse("2026-07-09T10:00:00Z"));
        var request = CreateRequest($"submit-path-invalid-typed-iterations-{maxImprovementIterations}") with
        {
            Budget = CreateRequest("submit-path-invalid-typed-iterations-base").Budget! with
            {
                MaxImprovementIterations = maxImprovementIterations
            }
        };

        var result = await api.SubmitAndEvaluateAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.MaxImprovementIterationsInvalidErrorCode, result.ErrorCode);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.MaxImprovementIterationsInvalidErrorMessage(), result.ErrorMessage);
        Assert.Empty(renderProvider.Requests);
        Assert.Empty(comparisonProvider.Requests);
        Assert.Empty(visualJudge.Requests);
    }

    [Theory]
    [InlineData(null, false, false, false)]
    [InlineData(0, true, true, true)]
    [InlineData(7, true, true, false)]
    public async Task SubmitAndEvaluateAsync_MaxTeacherCallsSingleEvaluationReportsConfiguredCapWithoutLoopExecution(
        int? maxTeacherCalls,
        bool expectedSupplied,
        bool expectedRuntimeEnforcement,
        bool expectedCapReached)
    {
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(CreateVerdict(
            "submit-path-max-teacher-calls-contract-verdict",
            0.93,
            passed: true));
        var visualJudge = new SubmitPathScriptedVisualJudge(_ => SubmitPathJudgeScript.FromScore(0.93, cost: 0.01m));
        var service = new DesignConversionInternalService(renderProvider, comparisonProvider, visualJudge);
        var evaluator = new RecordingThinClientJobEvaluator(service);
        var suffix = maxTeacherCalls?.ToString(CultureInfo.InvariantCulture) ?? "null";
        var api = new DesignConversionModelServerApi(
            evaluator,
            () => DateTimeOffset.Parse("2026-07-09T10:00:00Z"),
            new JsonlDesignConversionLocalArtifactDb(CreateArtifactDbTempRoot($"max-teacher-calls-{suffix}")));
        var baseRequest = CreateResponsiveSubmitPathRequest($"submit-path-max-teacher-calls-{suffix}");
        var request = baseRequest with
        {
            Budget = baseRequest.Budget! with
            {
                Tier = "paid",
                MaxImprovementCandidates = 4,
                BudgetUsd = 1.00m,
                RenderQaPassThreshold = 0.92,
                MaxImprovementIterations = 0,
                MaxTeacherCalls = maxTeacherCalls
            }
        };

        var result = await api.SubmitAndEvaluateAsync(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionThinClientStatus.Completed, result.Value!.ContractStatus);
        var mapped = Assert.Single(evaluator.Requests);
        Assert.Equal(maxTeacherCalls, mapped.Budget.MaxTeacherCalls);
        Assert.Equal(0, mapped.Budget.MaxImprovementIterations);
        Assert.Equal(4, mapped.Budget.MaxImprovementCandidates);
        Assert.Equal(2, renderProvider.Requests.Count);
        Assert.Equal(2, comparisonProvider.Requests.Count);
        Assert.Equal(2, visualJudge.Requests.Count);
        Assert.DoesNotContain(
            DesignConversionImprovementLoopThinClientJobEvaluator.SubmitPathLoopExecutedMarker,
            result.Value.EvidenceRefs);
        Assert.DoesNotContain(result.Value.Warnings, warning =>
            warning.StartsWith("submit_path_improvement_loop_attempt_count=", StringComparison.Ordinal));
        Assert.All(visualJudge.Requests, judgeRequest =>
            Assert.Equal("0.92", judgeRequest.Metadata["quality_threshold"]));

        var expectedSuppliedValue = expectedSupplied ? bool.TrueString : bool.FalseString;
        var expectedRuntimeEnforcementValue = expectedRuntimeEnforcement ? bool.TrueString : bool.FalseString;
        var expectedCapReachedValue = expectedCapReached ? bool.TrueString : bool.FalseString;
        var expectedCallCapEnforcedValue = expectedSupplied ? bool.TrueString : bool.FalseString;
        var expectedValue = maxTeacherCalls?.ToString(CultureInfo.InvariantCulture) ?? "not_supplied";
        Assert.Equal(expectedSuppliedValue, result.Value.BoundaryMetadata["max_teacher_calls_contract_supplied"]);
        Assert.Equal(expectedValue, result.Value.BoundaryMetadata["max_teacher_calls_contract_value"]);
        Assert.Equal(
            DesignConversionVisualQaBoundaryPolicy.MaxTeacherCallsContractBoundary,
            result.Value.BoundaryMetadata["max_teacher_calls_contract_boundary"]);
        Assert.Equal(expectedRuntimeEnforcementValue, result.Value.BoundaryMetadata["max_teacher_calls_runtime_enforcement"]);
        Assert.Equal(expectedCallCapEnforcedValue, result.Value.BoundaryMetadata["max_teacher_calls_call_cap_enforced"]);
        Assert.Equal("0", result.Value.BoundaryMetadata["max_teacher_calls_observed_teacher_call_count"]);
        Assert.Equal(expectedCapReachedValue, result.Value.BoundaryMetadata["max_teacher_calls_cap_reached"]);
        Assert.Equal(expectedSuppliedValue, result.Value.RuntimeStatus.Job.Metadata["max_teacher_calls_contract_supplied"]);
        Assert.Equal(expectedValue, result.Value.RuntimeStatus.Job.Metadata["max_teacher_calls_contract_value"]);
        Assert.Equal(expectedRuntimeEnforcementValue, result.Value.RuntimeStatus.Job.Metadata["max_teacher_calls_runtime_enforcement"]);
        Assert.Equal(expectedCallCapEnforcedValue, result.Value.RuntimeStatus.Job.Metadata["max_teacher_calls_call_cap_enforced"]);
        Assert.Equal("0", result.Value.RuntimeStatus.Job.Metadata["max_teacher_calls_observed_teacher_call_count"]);
        Assert.Equal(expectedCapReachedValue, result.Value.RuntimeStatus.Job.Metadata["max_teacher_calls_cap_reached"]);
        Assert.DoesNotContain(result.Value.EvidenceRefs, reference =>
            reference.Contains("improvement_loop:max_teacher_calls_", StringComparison.OrdinalIgnoreCase));
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["raw_html_included"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["raw_design_tree_included"]);
        Assert.False(result.Value.ProductReady);
    }

    [Fact]
    public async Task SubmitAndEvaluateAsync_MaxTeacherCallsOneCapsSubmitPathTeacherCallsAndPublishesMetadata()
    {
        var comparisonProvider = new RecordingComparisonProvider(CreateVerdict(
            "submit-path-max-teacher-calls-loop-verdict",
            0.70,
            passed: false));
        var visualJudge = new SubmitPathScriptedVisualJudge(request =>
        {
            var jobId = request.Metadata["job_id"];
            var score = jobId.Contains("loop-attempt:3", StringComparison.Ordinal)
                ? 0.70
                : jobId.Contains("loop-attempt:2", StringComparison.Ordinal)
                ? 0.76
                : 0.70;
            return SubmitPathJudgeScript.FromExternalScore(score, cost: 0.01m);
        });
        var service = new DesignConversionInternalService(
            new DeterministicDesignConversionRenderProvider(),
            comparisonProvider,
            visualJudge);
        var candidateStrategy = new SubmitPathScriptedCandidateStrategy(
            CreateSubmitPathCandidate("submit-path-max-teacher-calls-candidate-1", "PRIVATE_MAX_TEACHER_ONE"),
            CreateSubmitPathCandidate("submit-path-max-teacher-calls-candidate-2", "PRIVATE_MAX_TEACHER_TWO"));
        using var teacherFixture = new MaxTeacherCallsTeacherFixture(
            "max-teacher-calls-loop",
            supportIndependentPreferenceEvidence: true);
        var api = new DesignConversionModelServerApi(
            new DesignConversionInternalServiceThinClientJobEvaluator(service),
            () => DateTimeOffset.Parse("2026-07-09T10:00:00Z"),
            teacherFixture.ArtifactDb,
            improvementCandidateStrategy: candidateStrategy);
        var baseRequest = CreateResponsiveSubmitPathRequest("submit-path-max-teacher-calls-loop");
        var budgetMetadata = new Dictionary<string, string>(
            baseRequest.Budget!.Metadata ?? new Dictionary<string, string>(),
            StringComparer.Ordinal)
        {
            ["max_consecutive_no_improvement"] = "1"
        };
        var request = baseRequest with
        {
            Budget = baseRequest.Budget! with
            {
                Tier = "pro",
                MaxImprovementCandidates = 4,
                BudgetUsd = 1.00m,
                Metadata = budgetMetadata,
                MaxImprovementIterations = 3,
                MaxTeacherCalls = 1
            }
        };

        var result = await api.SubmitAndEvaluateAsync(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(6, comparisonProvider.Requests.Count);
        Assert.Equal(6, visualJudge.Requests.Count);
        Assert.Equal(
            [2],
            candidateStrategy.Requests
                .Select(candidateRequest => candidateRequest.CompletedAttemptNumber)
                .ToArray());
        Assert.Equal(
            3,
            comparisonProvider.Requests
                .Select(comparisonRequest => comparisonRequest.RequestId)
                .Distinct(StringComparer.Ordinal)
                .Count());
        Assert.All(visualJudge.Requests, judgeRequest =>
            Assert.True(judgeRequest.Metadata.ContainsKey("job_id")));
        Assert.Equal(1, teacherFixture.Teacher.InvocationCount);
        Assert.Contains(
            DesignConversionImprovementLoopThinClientJobEvaluator.SubmitPathLoopExecutedMarker,
            result.Value!.EvidenceRefs);
        Assert.Contains("improvement_loop:max_teacher_call_observed=1", result.Value.EvidenceRefs);
        Assert.Contains("improvement_loop:max_teacher_calls_cap_reached=True", result.Value.EvidenceRefs);
        Assert.Contains("improvement_loop:max_teacher_calls_reached", result.Value.EvidenceRefs);
        Assert.DoesNotContain("improvement_loop:max_teacher_calls_reached_false", result.Value.EvidenceRefs);
        Assert.DoesNotContain(DesignConversionImprovementLoopCoordinator.NoExternalProviderMarker, result.Value.EvidenceRefs);
        Assert.Contains("submit_path_improvement_loop_attempt_count=3", result.Value.Warnings);
        Assert.Contains("submit_path_improvement_loop_max_attempts=4", result.Value.Warnings);
        Assert.Contains(
            $"submit_path_improvement_loop_stop_reason={DesignConversionImprovementLoopCoordinator.MaxTeacherCallsReached}",
            result.Value.Warnings);
        Assert.Contains("IMPROVEMENT_LOOP_MAX_TEACHER_CALLS_REACHED", result.Value.Warnings);
        Assert.Contains(
            "max_teacher_calls_observed_teacher_call_count=1",
            result.Value.Warnings);
        Assert.Contains("max_teacher_calls_cap_reached=True", result.Value.Warnings);
        Assert.DoesNotContain(result.Value.Warnings, warning =>
            warning.Contains("SUBMIT_PATH_SCRIPTED_CANDIDATE_MISSING", StringComparison.Ordinal) ||
            warning.Contains("IMPROVEMENT_LOOP_CANDIDATE_STRATEGY_FAILED", StringComparison.Ordinal));
        Assert.DoesNotContain(result.Value.EvidenceRefs, reference =>
            reference.Contains("SUBMIT_PATH_SCRIPTED_CANDIDATE_MISSING", StringComparison.Ordinal) ||
            reference.Contains("candidate_strategy_failed", StringComparison.Ordinal));
        Assert.Equal(bool.TrueString, result.Value.BoundaryMetadata["max_teacher_calls_contract_supplied"]);
        Assert.Equal("1", result.Value.BoundaryMetadata["max_teacher_calls_contract_value"]);
        Assert.Equal(
            DesignConversionVisualQaBoundaryPolicy.MaxTeacherCallsContractBoundary,
            result.Value.BoundaryMetadata["max_teacher_calls_contract_boundary"]);
        Assert.Equal(bool.TrueString, result.Value.BoundaryMetadata["max_teacher_calls_runtime_enforcement"]);
        Assert.Equal(bool.TrueString, result.Value.BoundaryMetadata["max_teacher_calls_call_cap_enforced"]);
        Assert.Equal("1", result.Value.BoundaryMetadata["max_teacher_calls_observed_teacher_call_count"]);
        Assert.Equal(bool.TrueString, result.Value.BoundaryMetadata["max_teacher_calls_cap_reached"]);
        Assert.Equal(bool.TrueString, result.Value.RuntimeStatus.Job.Metadata["max_teacher_calls_runtime_enforcement"]);
        Assert.Equal(bool.TrueString, result.Value.RuntimeStatus.Job.Metadata["max_teacher_calls_call_cap_enforced"]);
        Assert.Equal("1", result.Value.RuntimeStatus.Job.Metadata["max_teacher_calls_observed_teacher_call_count"]);
        Assert.Equal(bool.TrueString, result.Value.RuntimeStatus.Job.Metadata["max_teacher_calls_cap_reached"]);
        AssertSubmitPathNonReadyVisualDecision(
            result.Value,
            DesignConversionVisualQaBoundaryPolicy.DecisionRetry,
            DesignConversionVisualQaBoundaryPolicy.ContinueImprovement);
        Assert.Equal("true", result.Value.RuntimeStatus.Job.Metadata["local_artifact_db_persistence_verified"]);
        var durableRecords = await teacherFixture.ArtifactDb.ListRecordsAsync(new DesignConversionLocalArtifactDbQuery(
            JobId: result.Value.JobId,
            RequestId: request.RequestId));
        Assert.True(durableRecords.IsSuccess, durableRecords.ErrorMessage);
        var durableRecord = Assert.Single(durableRecords.Value!);
        Assert.Equal(result.Value.JobId, durableRecord.JobId);
        Assert.Equal(request.RequestId, durableRecord.RequestId);
        var durableCandidates = durableRecord.DpoCandidates.ToArray();
        Assert.Equal(
            new[]
            {
                DesignConversionDpoUnits.CriticUnit,
                DesignConversionDpoUnits.ImproverUnit
            },
            durableCandidates.Select(candidate => candidate.UnitId));
        Assert.Contains("improvement_loop:max_teacher_call_observed=1", durableRecord.EvidenceRefs);
        Assert.Contains("improvement_loop:max_teacher_calls_cap_reached=True", durableRecord.EvidenceRefs);
        Assert.Contains("improvement_loop:max_teacher_calls_reached", durableRecord.EvidenceRefs);
        Assert.DoesNotContain("improvement_loop:max_teacher_calls_reached_false", durableRecord.EvidenceRefs);
        Assert.DoesNotContain(DesignConversionImprovementLoopCoordinator.NoExternalProviderMarker, durableRecord.EvidenceRefs);
        Assert.Equal("3", durableRecord.Metadata["attempt_count"]);
        Assert.Equal(
            DesignConversionImprovementLoopCoordinator.MaxTeacherCallsReached,
            durableRecord.Metadata["final_loop_stop_reason"]);
        Assert.Equal(bool.TrueString, durableRecord.Metadata["max_teacher_calls_runtime_enforcement"]);
        Assert.Equal(bool.TrueString, durableRecord.Metadata["max_teacher_calls_call_cap_enforced"]);
        Assert.Equal("1", durableRecord.Metadata["max_teacher_calls_observed_teacher_call_count"]);
        Assert.Equal(bool.TrueString, durableRecord.Metadata["max_teacher_calls_cap_reached"]);
        Assert.DoesNotContain(durableRecord.Metadata["runtime_created_dpo_candidate_warnings"], "SUBMIT_PATH_SCRIPTED_CANDIDATE_MISSING", StringComparison.Ordinal);
        Assert.DoesNotContain(durableRecord.EvidenceRefs, reference =>
            reference.Contains("SUBMIT_PATH_SCRIPTED_CANDIDATE_MISSING", StringComparison.Ordinal) ||
            reference.Contains("candidate_strategy_failed", StringComparison.Ordinal));
        Assert.All(durableCandidates, candidate =>
        {
            var trainingRecord = candidate.TrainingRecord;
            Assert.Equal(bool.TrueString, trainingRecord.Provenance.Metadata["runtime_teacher_authored_candidate"]);
            Assert.Equal(bool.TrueString, trainingRecord.Provenance.Metadata["independent_preference_proven"]);
            Assert.False(trainingRecord.Provenance.Metadata.ContainsKey("runtime_teacher_selected_preference"));
            Assert.Equal(
                DesignConversionDpoTeacherAttestationRuntime.AuthoringSchema,
                trainingRecord.Provenance.Metadata["teacher_authoring_schema"]);
            Assert.Equal(bool.FalseString, trainingRecord.Provenance.Metadata["raw_teacher_prompt_persisted"]);
            Assert.Equal(bool.FalseString, trainingRecord.Provenance.Metadata["raw_teacher_output_persisted"]);
            Assert.Equal(bool.FalseString, trainingRecord.Provenance.Metadata["raw_teacher_rationale_persisted"]);
            Assert.DoesNotContain("synthetic_negative", trainingRecord.Provenance.Metadata.Keys);
            Assert.DoesNotContain("scaffold_negative", trainingRecord.Provenance.Metadata.Keys);
            Assert.DoesNotContain(trainingRecord.Provenance.Metadata, pair =>
                pair.Key.Contains("manual", StringComparison.OrdinalIgnoreCase) ||
                pair.Value.Contains("manual", StringComparison.OrdinalIgnoreCase));
            var teacherProvenance = Assert.IsType<TrainingTeacherProvenance>(trainingRecord.TeacherProvenance);
            Assert.Equal(TrainingTeacherSourceKind.ExternalTeacherOutput, teacherProvenance.SourceKind);
            Assert.Equal(OpenWeightTeacherFamily.Qwen, teacherProvenance.Family);
            Assert.Equal(TrainingTeacherAccessMode.LocalHostedOpenWeight, teacherProvenance.AccessMode);
            Assert.Equal(TrainingTeacherHostingProvider.Ollama, teacherProvenance.HostingProvider);
            Assert.Equal(MaxTeacherCallsExpectedTeacherModelId, teacherProvenance.ModelId);
            Assert.Equal(MaxTeacherCallsExpectedTeacherManifestDigest, teacherProvenance.ModelVersion);
            Assert.Equal(TrainingTeacherModelLicense.Apache20, teacherProvenance.ModelLicense);
            Assert.Equal(1, teacherProvenance.InvocationCount);
        });
        Assert.DoesNotContain(result.Value.Warnings, warning =>
            warning.StartsWith(
                DesignConversionImprovementLoopCoordinator.TeacherAuthoringErrorWarningPrefix,
                StringComparison.Ordinal));
        var publicProjection = JsonSerializer.Serialize(new { result.Value, durableRecord });
        Assert.DoesNotContain(teacherFixture.Teacher.LastPrompt!, publicProjection, StringComparison.Ordinal);
        Assert.DoesNotContain(teacherFixture.Teacher.LastOutput!, publicProjection, StringComparison.Ordinal);
        Assert.False(result.Value.ProductReady);
    }

    [Fact]
    public async Task SubmitAndEvaluateAsync_MaxTeacherCallsOneDoesNotOverwriteQualityReachedSubmitPathStopAfterTeacherCall()
    {
        var comparisonProvider = new RecordingComparisonProvider(CreateVerdict(
            "submit-path-max-teacher-calls-quality-verdict",
            0.72,
            passed: false));
        var visualJudge = new SubmitPathScriptedVisualJudge(request =>
            request.Metadata["job_id"].Contains("loop-attempt:2", StringComparison.Ordinal)
                ? SubmitPathJudgeScript.FromExternalScore(0.94, cost: 0.01m)
                : SubmitPathJudgeScript.FromExternalScore(0.72, cost: 0.01m));
        var service = new DesignConversionInternalService(
            new DeterministicDesignConversionRenderProvider(),
            comparisonProvider,
            visualJudge);
        var candidateStrategy = new SubmitPathScriptedCandidateStrategy(
            CreateSubmitPathCandidate("submit-path-max-teacher-calls-quality-candidate", "PRIVATE_MAX_TEACHER_QUALITY"));
        using var teacherFixture = new MaxTeacherCallsTeacherFixture(
            "max-teacher-calls-quality",
            supportIndependentPreferenceEvidence: true);
        var api = new DesignConversionModelServerApi(
            new DesignConversionInternalServiceThinClientJobEvaluator(service),
            () => DateTimeOffset.Parse("2026-07-09T10:00:00Z"),
            teacherFixture.ArtifactDb,
            improvementCandidateStrategy: candidateStrategy);
        var baseRequest = CreateResponsiveSubmitPathRequest("submit-path-max-teacher-calls-quality");
        var request = baseRequest with
        {
            Budget = baseRequest.Budget! with
            {
                Tier = "pro",
                MaxImprovementCandidates = 2,
                BudgetUsd = 1.00m,
                MaxImprovementIterations = 1,
                MaxTeacherCalls = 1
            }
        };

        var result = await api.SubmitAndEvaluateAsync(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(4, comparisonProvider.Requests.Count);
        Assert.Equal(4, visualJudge.Requests.Count);
        Assert.Empty(candidateStrategy.Requests);
        Assert.Equal(1, teacherFixture.Teacher.InvocationCount);
        Assert.Contains(
            $"submit_path_improvement_loop_stop_reason={DesignConversionVisualQaBoundaryPolicy.QualityReached}",
            result.Value!.Warnings);
        Assert.DoesNotContain(
            $"submit_path_improvement_loop_stop_reason={DesignConversionImprovementLoopCoordinator.MaxTeacherCallsReached}",
            result.Value.Warnings);
        Assert.Contains("improvement_loop:max_teacher_call_observed=1", result.Value.EvidenceRefs);
        Assert.Contains("improvement_loop:max_teacher_calls_cap_reached=True", result.Value.EvidenceRefs);
        Assert.Contains("improvement_loop:max_teacher_calls_reached", result.Value.EvidenceRefs);
        Assert.Equal("1", result.Value.BoundaryMetadata["max_teacher_calls_observed_teacher_call_count"]);
        Assert.Equal(bool.TrueString, result.Value.BoundaryMetadata["max_teacher_calls_cap_reached"]);
        Assert.DoesNotContain(result.Value.Warnings, warning =>
            warning.Contains("SUBMIT_PATH_SCRIPTED_CANDIDATE_MISSING", StringComparison.Ordinal) ||
            warning.Contains("IMPROVEMENT_LOOP_CANDIDATE_STRATEGY_FAILED", StringComparison.Ordinal));
        Assert.DoesNotContain(result.Value.EvidenceRefs, reference =>
            reference.Contains("SUBMIT_PATH_SCRIPTED_CANDIDATE_MISSING", StringComparison.Ordinal) ||
            reference.Contains("candidate_strategy_failed", StringComparison.Ordinal));

        var durableRecords = await teacherFixture.ArtifactDb.ListRecordsAsync(new DesignConversionLocalArtifactDbQuery(
            JobId: result.Value.JobId,
            RequestId: request.RequestId));
        Assert.True(durableRecords.IsSuccess, durableRecords.ErrorMessage);
        var durableRecord = Assert.Single(durableRecords.Value!);
        Assert.Equal(2, durableRecord.DpoCandidates.Count);
        Assert.Equal(
            DesignConversionVisualQaBoundaryPolicy.QualityReached,
            durableRecord.Metadata["final_loop_stop_reason"]);
        Assert.Equal("1", durableRecord.Metadata["max_teacher_calls_observed_teacher_call_count"]);
        Assert.Equal(bool.TrueString, durableRecord.Metadata["max_teacher_calls_cap_reached"]);
        Assert.Contains("improvement_loop:max_teacher_call_observed=1", durableRecord.EvidenceRefs);
        Assert.Contains("improvement_loop:max_teacher_calls_reached", durableRecord.EvidenceRefs);
        Assert.DoesNotContain(DesignConversionImprovementLoopCoordinator.NoExternalProviderMarker, durableRecord.EvidenceRefs);
    }

    [Fact]
    public async Task SubmitAndEvaluateAsync_InvalidMaxTeacherCallsFailsBeforeRuntimeEvaluator()
    {
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(CreateVerdict(
            "submit-path-invalid-max-teacher-calls-verdict",
            0.93,
            passed: true));
        var visualJudge = new SubmitPathScriptedVisualJudge(_ => SubmitPathJudgeScript.FromScore(0.93, cost: 0.01m));
        var service = new DesignConversionInternalService(renderProvider, comparisonProvider, visualJudge);
        var evaluator = new RecordingThinClientJobEvaluator(service);
        var api = new DesignConversionModelServerApi(evaluator, () => DateTimeOffset.Parse("2026-07-09T10:00:00Z"));
        var request = CreateRequest("submit-path-invalid-max-teacher-calls") with
        {
            Budget = CreateRequest("submit-path-invalid-max-teacher-calls-base").Budget! with
            {
                MaxTeacherCalls = -1
            }
        };

        var result = await api.SubmitAndEvaluateAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.MaxTeacherCallsInvalidErrorCode, result.ErrorCode);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.MaxTeacherCallsInvalidErrorMessage(), result.ErrorMessage);
        Assert.Empty(evaluator.Requests);
        Assert.Empty(renderProvider.Requests);
        Assert.Empty(comparisonProvider.Requests);
        Assert.Empty(visualJudge.Requests);
    }

    [Fact]
    public async Task SubmitAndEvaluateAsync_RenderQaPassThresholdMapsThroughRealRouteToVisualJudge()
    {
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(CreateVerdict(
            "submit-path-typed-threshold-comparison-verdict",
            0.93,
            passed: true));
        var visualJudge = new SubmitPathScriptedVisualJudge(_ => SubmitPathJudgeScript.FromScore(0.93, cost: 0.01m));
        var service = new DesignConversionInternalService(renderProvider, comparisonProvider, visualJudge);
        var api = new DesignConversionModelServerApi(
            new DesignConversionInternalServiceThinClientJobEvaluator(service),
            () => DateTimeOffset.Parse("2026-07-09T10:00:00Z"),
            new JsonlDesignConversionLocalArtifactDb(CreateArtifactDbTempRoot("typed-threshold")));
        var baseRequest = CreateResponsiveSubmitPathRequest("submit-path-typed-threshold-route");
        var request = baseRequest with
        {
            Budget = baseRequest.Budget! with
            {
                Tier = "paid",
                MaxImprovementCandidates = 1,
                Metadata = new Dictionary<string, string>
                {
                    ["quality_threshold"] = "0.99",
                    ["quality_threshold_source"] = "caller_forged_legacy_source"
                },
                RenderQaPassThreshold = 0.92
            },
            Metadata = new Dictionary<string, string>(baseRequest.Metadata ?? new Dictionary<string, string>())
            {
                ["quality_threshold"] = "0.10",
                ["quality_threshold_source"] = "caller_forged_route_source"
            }
        };

        var result = await api.SubmitAndEvaluateAsync(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionThinClientStatus.Completed, result.Value!.ContractStatus);
        Assert.NotEmpty(visualJudge.Requests);
        Assert.All(visualJudge.Requests, judgeRequest =>
        {
            Assert.Equal("0.92", judgeRequest.Metadata["quality_threshold"]);
            Assert.DoesNotContain("0.99", string.Join("|", judgeRequest.Metadata.Values), StringComparison.Ordinal);
            Assert.DoesNotContain("0.10", string.Join("|", judgeRequest.Metadata.Values), StringComparison.Ordinal);
        });
        var tierPolicyEvidenceRefs = result.Value.EvidenceRefs
            .Where(evidence => evidence.StartsWith("tier_policy:", StringComparison.Ordinal))
            .ToArray();
        Assert.Contains("tier_policy:quality_threshold=0.92", tierPolicyEvidenceRefs);
        Assert.Contains(
            $"tier_policy:quality_threshold_source={DesignConversionVisualQaBoundaryPolicy.TypedRenderQaPassThresholdSource}",
            tierPolicyEvidenceRefs);
        Assert.DoesNotContain(tierPolicyEvidenceRefs, evidence =>
            evidence.Contains("caller_forged_route_source", StringComparison.Ordinal) ||
            evidence.Contains("caller_forged_legacy_source", StringComparison.Ordinal));

        var callerRouteSourceProvenance = result.Value.RuntimeStatus.Provenance
            .Where(pair => string.Equals(pair.Value, "caller_forged_route_source", StringComparison.Ordinal))
            .ToArray();
        Assert.All(callerRouteSourceProvenance, pair =>
        {
            Assert.StartsWith("request_metadata:", pair.Key, StringComparison.Ordinal);
            Assert.NotEqual("quality_threshold_source", pair.Key);
        });
        Assert.False(result.Value.ProductReady);
    }

    public static TheoryData<double, string> InvalidTypedRenderQaPassThresholdCases()
        => new()
        {
            { double.NaN, "internal" },
            { double.PositiveInfinity, "internal" },
            { 0.0, "internal" },
            { -0.10, "internal" },
            { 1.01, "internal" },
            { 0.74, "free" },
            { 0.84, "paid" },
            { 0.89, "pro" }
        };

    [Theory]
    [MemberData(nameof(InvalidTypedRenderQaPassThresholdCases))]
    public async Task SubmitAndEvaluateAsync_InvalidTypedRenderQaPassThresholdFailsBeforeRuntime(
        double renderQaPassThreshold,
        string tier)
    {
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(CreateVerdict(
            "submit-path-invalid-typed-threshold-comparison-verdict",
            0.93,
            passed: true));
        var visualJudge = new SubmitPathScriptedVisualJudge(_ => SubmitPathJudgeScript.FromScore(0.93, cost: 0.01m));
        var service = new DesignConversionInternalService(renderProvider, comparisonProvider, visualJudge);
        var api = new DesignConversionModelServerApi(service, () => DateTimeOffset.Parse("2026-07-09T10:00:00Z"));
        var request = CreateRequest($"submit-path-invalid-typed-threshold-{tier}-{renderQaPassThreshold}") with
        {
            Budget = CreateRequest("submit-path-invalid-typed-threshold-base").Budget! with
            {
                Tier = tier,
                RenderQaPassThreshold = renderQaPassThreshold
            }
        };

        var result = await api.SubmitAndEvaluateAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.RenderQaPassThresholdInvalidErrorCode, result.ErrorCode);
        Assert.Empty(renderProvider.Requests);
        Assert.Empty(comparisonProvider.Requests);
        Assert.Empty(visualJudge.Requests);
    }

    [Theory]
    [InlineData("enterprise")]
    [InlineData("local")]
    public async Task SubmitAndEvaluateAsync_UnknownTierZeroMaxImprovementCandidatesFailsBeforeRuntime(string tier)
    {
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(CreateVerdict(
            "submit-path-unknown-tier-zero-budget-comparison-verdict",
            0.72,
            passed: false));
        var visualJudge = new SubmitPathScriptedVisualJudge(_ => SubmitPathJudgeScript.FromScore(0.72, cost: 0.01m));
        var service = new DesignConversionInternalService(renderProvider, comparisonProvider, visualJudge);
        var api = new DesignConversionModelServerApi(service, () => DateTimeOffset.Parse("2026-07-09T10:00:00Z"));
        var request = CreateRequest($"submit-path-unknown-tier-zero-budget-{tier}") with
        {
            Budget = CreateRequest("submit-path-unknown-tier-zero-budget-base").Budget! with
            {
                Tier = tier,
                MaxImprovementCandidates = 0
            }
        };

        var result = await api.SubmitAndEvaluateAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal(DesignConversionInternalService.ImprovementCandidateBudgetRequiredCode, result.ErrorCode);
        Assert.Equal("DesignConversion improvement candidate budget must be greater than zero.", result.ErrorMessage);
        Assert.Empty(renderProvider.Requests);
        Assert.Empty(comparisonProvider.Requests);
        Assert.Empty(visualJudge.Requests);
    }

    [Theory]
    [InlineData(
        " free ",
        "free",
        "0.05",
        1,
        DesignConversionVisualQaBoundaryPolicy.DecisionBlocked,
        DesignConversionVisualQaBoundaryPolicy.BudgetCapReached)]
    [InlineData(
        "PAID",
        "PAID",
        "0.25",
        2,
        DesignConversionVisualQaBoundaryPolicy.DecisionRetry,
        DesignConversionVisualQaBoundaryPolicy.ContinueImprovement)]
    [InlineData(
        "pro",
        "pro",
        "1",
        4,
        DesignConversionVisualQaBoundaryPolicy.DecisionRetry,
        DesignConversionVisualQaBoundaryPolicy.ContinueImprovement)]
    public async Task SubmitAndEvaluateAsync_ServerOwnedTierZeroBudgetUsesBudgetAndIterationEntitlements(
        string tier,
        string publishedTier,
        string expectedBudgetUsd,
        int expectedMaxAttempts,
        string expectedDecision,
        string expectedStopReason)
    {
        var normalizedTier = tier.Trim().ToLowerInvariant();
        var expectedEvaluationCount = string.Equals(
            expectedStopReason,
            DesignConversionVisualQaBoundaryPolicy.BudgetCapReached,
            StringComparison.Ordinal)
            ? 1
            : 2;
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(CreateVerdict(
            $"submit-path-{normalizedTier}-tier-zero-budget-comparison-verdict",
            0.70,
            passed: false));
        var visualJudge = new SubmitPathScriptedVisualJudge(_ => SubmitPathJudgeScript.FromScore(0.70, cost: 0m));
        var service = new DesignConversionInternalService(renderProvider, comparisonProvider, visualJudge);
        var evaluator = new RecordingThinClientJobEvaluator(service);
        var api = new DesignConversionModelServerApi(
            evaluator,
            () => DateTimeOffset.Parse("2026-07-09T10:00:00Z"),
            new JsonlDesignConversionLocalArtifactDb(CreateArtifactDbTempRoot($"tier-zero-{normalizedTier}")));
        var baseRequest = CreateResponsiveSubmitPathRequest($"submit-path-{normalizedTier}-tier-zero-budget");
        var request = baseRequest with
        {
            Budget = baseRequest.Budget! with
            {
                Tier = tier,
                MaxImprovementCandidates = 0,
                BudgetUsd = 0m
            }
        };

        var result = await api.SubmitAndEvaluateAsync(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        AssertSubmitPathNonReadyVisualDecision(
            result.Value!,
            expectedDecision,
            expectedStopReason);
        Assert.Equal(expectedEvaluationCount, evaluator.Requests.Count);
        Assert.All(evaluator.Requests, request => Assert.Equal(0m, request.Budget.BudgetUsd));
        Assert.Equal(expectedEvaluationCount * 2, renderProvider.Requests.Count);
        Assert.Equal(expectedEvaluationCount * 2, comparisonProvider.Requests.Count);
        Assert.Equal(expectedEvaluationCount * 2, visualJudge.Requests.Count);
        Assert.Equal(
            expectedEvaluationCount,
            visualJudge.Requests.Select(request => request.Metadata["job_id"]).Distinct(StringComparer.Ordinal).Count());
        Assert.Contains($"tier_policy:tier={publishedTier}", result.Value.EvidenceRefs);
        Assert.Contains("tier_policy:submitted_max_improvement_candidates=0", result.Value.EvidenceRefs);
        Assert.Contains(
            $"tier_policy:effective_max_improvement_candidates={expectedMaxAttempts}",
            result.Value.EvidenceRefs);
        Assert.Contains("tier_policy:budget_entitlement_default_applied=true", result.Value.EvidenceRefs);
        Assert.Contains(DesignConversionVisualQaBoundaryPolicy.TierBudgetEntitlementDefaultAppliedMarker, result.Value.EvidenceRefs);
        Assert.Contains(DesignConversionVisualQaBoundaryPolicy.TierIterationEntitlementDefaultAppliedMarker, result.Value.EvidenceRefs);
        Assert.Contains("tier_policy:iteration_entitlement_default_applied=true", result.Value.EvidenceRefs);
        if (string.Equals(expectedStopReason, DesignConversionVisualQaBoundaryPolicy.BudgetCapReached, StringComparison.Ordinal))
        {
            Assert.Contains($"tier_policy:cost_cap={expectedBudgetUsd}", result.Value.EvidenceRefs);
            Assert.DoesNotContain(
                DesignConversionImprovementLoopThinClientJobEvaluator.SubmitPathLoopExecutedMarker,
                result.Value.EvidenceRefs);
            Assert.DoesNotContain(
                result.Value.Warnings,
                warning => warning.StartsWith("submit_path_improvement_loop_attempt_count=", StringComparison.Ordinal));
            Assert.Contains("tier_policy:iteration_cap_reached", result.Value.EvidenceRefs);
        }
        else
        {
            Assert.DoesNotContain($"tier_policy:cost_cap={expectedBudgetUsd}", result.Value.EvidenceRefs);
            Assert.Contains(
                DesignConversionImprovementLoopThinClientJobEvaluator.SubmitPathLoopExecutedMarker,
                result.Value.EvidenceRefs);
            Assert.Contains(
                $"submit_path_improvement_loop_attempt_count={expectedEvaluationCount}",
                result.Value.Warnings);
            Assert.DoesNotContain("tier_policy:iteration_cap_reached", result.Value.EvidenceRefs);
        }
    }

    [Theory]
    [InlineData(51)]
    [InlineData(int.MaxValue)]
    public async Task SubmitAndEvaluateAsync_MaxImprovementCandidatesAboveCoordinatorCapFailsBeforeRuntime(int maxImprovementCandidates)
    {
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(CreateVerdict(
            "submit-path-over-cap-comparison-verdict",
            0.72,
            passed: false));
        var visualJudge = new SubmitPathScriptedVisualJudge(_ => SubmitPathJudgeScript.FromScore(0.72, cost: 0.01m));
        var service = new DesignConversionInternalService(renderProvider, comparisonProvider, visualJudge);
        var api = new DesignConversionModelServerApi(service, () => DateTimeOffset.Parse("2026-07-09T10:00:00Z"));
        var request = CreateRequest($"submit-path-over-cap-{maxImprovementCandidates}") with
        {
            Budget = CreateRequest("submit-path-over-cap-base").Budget! with
            {
                MaxImprovementCandidates = maxImprovementCandidates
            }
        };

        var result = await api.SubmitAndEvaluateAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal(DesignConversionModelServerImprovementLoopHandler.MaxAttemptsExceededErrorCode, result.ErrorCode);
        Assert.Equal(
            $"DesignConversion improvement-loop max attempts must be less than or equal to {DesignConversionImprovementLoopCoordinator.MaxAllowedAttempts.ToString(System.Globalization.CultureInfo.InvariantCulture)}.",
            result.ErrorMessage);
        Assert.Empty(renderProvider.Requests);
        Assert.Empty(comparisonProvider.Requests);
        Assert.Empty(visualJudge.Requests);
    }

    [Fact]
    public async Task SubmitAndEvaluateAsync_MaxAllowedImprovementCandidatesAcceptedButFreeTierStopsAfterOneAttempt()
    {
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(CreateVerdict(
            "submit-path-exact-cap-free-tier-comparison-verdict",
            0.70,
            passed: false));
        var visualJudge = new SubmitPathScriptedVisualJudge(_ => SubmitPathJudgeScript.FromScore(0.70, cost: 0.01m));
        var service = new DesignConversionInternalService(renderProvider, comparisonProvider, visualJudge);
        var api = new DesignConversionModelServerApi(
            service,
            () => DateTimeOffset.Parse("2026-07-09T10:00:00Z"),
            postEvaluationStateStore: TestPostEvaluationStateStore);
        var baseRequest = CreateRequest("submit-path-exact-cap-free-tier");
        var request = baseRequest with
        {
            Screenshots =
            [
                new DesignConversionModelServerArtifactRef(
                    ArtifactId: "submit-path-exact-cap-screenshot-desktop",
                    Kind: "screenshot",
                    Width: 1440,
                    Height: 900,
                    Uri: "artifact://submit-path-exact-cap/screenshot-desktop.png",
                    Sha256: "sha256:submit-path-exact-cap:screenshot-desktop"),
                new DesignConversionModelServerArtifactRef(
                    ArtifactId: "submit-path-exact-cap-screenshot-mobile",
                    Kind: "screenshot",
                    Width: 390,
                    Height: 844,
                    Uri: "artifact://submit-path-exact-cap/screenshot-mobile.png",
                    Sha256: "sha256:submit-path-exact-cap:screenshot-mobile")
            ],
            DesignRenders =
            [
                new DesignConversionModelServerArtifactRef(
                    ArtifactId: "submit-path-exact-cap-design-desktop",
                    Kind: "design-render",
                    Width: 1440,
                    Height: 900,
                    Uri: "artifact://submit-path-exact-cap/design-desktop.png",
                    Sha256: "sha256:submit-path-exact-cap:design-desktop"),
                new DesignConversionModelServerArtifactRef(
                    ArtifactId: "submit-path-exact-cap-design-mobile",
                    Kind: "design-render",
                    Width: 390,
                    Height: 844,
                    Uri: "artifact://submit-path-exact-cap/design-mobile.png",
                    Sha256: "sha256:submit-path-exact-cap:design-mobile")
            ],
            Budget = baseRequest.Budget! with
            {
                Tier = "free",
                MaxImprovementCandidates = DesignConversionImprovementLoopCoordinator.MaxAllowedAttempts,
                BudgetUsd = 1.00m
            }
        };

        var result = await api.SubmitAndEvaluateAsync(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        AssertSubmitPathNonReadyVisualDecision(
            result.Value!,
            DesignConversionVisualQaBoundaryPolicy.DecisionBlocked,
            DesignConversionVisualQaBoundaryPolicy.BudgetCapReached);
        Assert.Equal(2, renderProvider.Requests.Count);
        Assert.Equal(2, comparisonProvider.Requests.Count);
        Assert.Equal(2, visualJudge.Requests.Count);
        Assert.DoesNotContain(
            DesignConversionImprovementLoopThinClientJobEvaluator.SubmitPathLoopExecutedMarker,
            result.Value.EvidenceRefs);
        Assert.Contains(
            $"tier_policy:submitted_max_improvement_candidates={DesignConversionImprovementLoopCoordinator.MaxAllowedAttempts.ToString(System.Globalization.CultureInfo.InvariantCulture)}",
            result.Value.EvidenceRefs);
        Assert.Contains("tier_policy:effective_max_improvement_candidates=1", result.Value.EvidenceRefs);
        Assert.Contains("tier_policy:iteration_entitlement_default_applied=false", result.Value.EvidenceRefs);
        Assert.Contains(DesignConversionVisualQaBoundaryPolicy.TierIterationCapAppliedMarker, result.Value.EvidenceRefs);
        Assert.Contains("tier_policy:iteration_cap_reached", result.Value.EvidenceRefs);
    }

    [Fact]
    public async Task LocalHttpPostEvaluateAsync_ValidRequest_CallsInProcessApiAndReturnsSanitizedEvidence()
    {
        const string privateMarker = "PRIVATE_PASS122_HTTP_SECRET";
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(CreateVerdict("pass122-http-verdict", 0.93, passed: true));
        var service = new DesignConversionInternalService(renderProvider, comparisonProvider, routingFlow: null);
        var api = new DesignConversionModelServerApi(
            service,
            () => DateTimeOffset.Parse("2026-07-03T09:00:00Z"),
            postEvaluationStateStore: TestPostEvaluationStateStore);
        var handler = new DesignConversionModelServerHttpHandler(api);
        var apiRequest = CreateRequest("pass122-http-valid-request") with
        {
            DesignTree = new DesignConversionModelServerPayloadRef(
                RefId: "pass122-tree-ref",
                Kind: "figma-json",
                Payload: $"{{\"name\":\"{privateMarker}\"}}",
                Format: "figma-json"),
            OriginalTree = new DesignConversionModelServerPayloadRef(
                RefId: "pass122-original-ref",
                Kind: "figma-json",
                Payload: $"{{\"name\":\"original-{privateMarker}\"}}",
                Format: "figma-json"),
            Html = $"<main data-secret=\"{privateMarker}\">Dashboard</main>",
            Css = $".screen{{background:url('{privateMarker}');}}"
        };

        var httpResponse = await handler.HandleAsync(new DesignConversionModelServerHttpRequest(
            Method: "POST",
            Path: DesignConversionModelServerHttpHandler.EndpointPath,
            BodyJson: JsonSerializer.Serialize(apiRequest, DesignConversionModelServerHttpHandler.JsonOptions),
            ContentType: "application/json"));

        Assert.Equal(200, httpResponse.StatusCode);
        Assert.Equal("application/json; charset=utf-8", httpResponse.ContentType);
        Assert.NotEmpty(renderProvider.Requests);
        Assert.NotEmpty(comparisonProvider.Requests);
        Assert.DoesNotContain(privateMarker, httpResponse.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(apiRequest.Html, httpResponse.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(apiRequest.Css, httpResponse.BodyJson, StringComparison.Ordinal);
        Assert.Equal(DesignConversionModelServerHttpHandler.LocalHttpSeam, httpResponse.Headers["x-design-conversion-http-seam"]);
        Assert.Equal(bool.FalseString, httpResponse.Headers["x-design-conversion-public-deployment-verified"]);

        using var document = JsonDocument.Parse(httpResponse.BodyJson);
        var root = document.RootElement;
        Assert.Equal(DesignConversionModelServerHttpHandler.EndpointPath, root.GetProperty("endpoint").GetString());
        Assert.Equal("POST", root.GetProperty("method").GetString());
        Assert.Equal(DesignConversionModelServerHttpHandler.LocalHttpSeam, root.GetProperty("httpSeam").GetString());
        Assert.Equal(DesignConversionModelServerApi.ApiShape, root.GetProperty("apiShape").GetString());
        Assert.Equal(DesignConversionModelServerApi.RuntimeSurface, root.GetProperty("runtimeSurface").GetString());
        Assert.Equal(DesignConversionThinClientStatus.NotReady, root.GetProperty("contractStatus").GetString());
        Assert.False(root.GetProperty("learnedInferenceApplied").GetBoolean());
        Assert.False(root.GetProperty("trainedModelAuthority").GetBoolean());
        Assert.True(root.GetProperty("notTrainedModelAuthority").GetBoolean());
        Assert.False(root.GetProperty("modelTrainingVerified").GetBoolean());
        Assert.False(root.GetProperty("modelUploadReady").GetBoolean());
        Assert.False(root.GetProperty("deploymentVerified").GetBoolean());
        Assert.False(root.GetProperty("publicHttpEndpointVerified").GetBoolean());
        Assert.Contains(root.GetProperty("evidenceRefs").EnumerateArray(), item =>
            string.Equals(item.GetString(), DesignConversionInternalService.ServiceFirstMarker, StringComparison.Ordinal));

        var metadata = root.GetProperty("boundaryMetadata");
        Assert.Equal(bool.TrueString, metadata.GetProperty("local_http_host_seam_verified").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("socket_opened").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("public_http_endpoint_verified").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("cross_repo_caller_verified").GetString());
        Assert.Equal(DesignConversionThinClientStatus.NotReady, metadata.GetProperty("runtime_contract_status").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("browser_e2e_verified").GetString());
        Assert.Equal("runtime_not_completed", metadata.GetProperty("browser_e2e_failure_reason").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("visual_fidelity_verified").GetString());
        Assert.Equal("runtime_not_completed", metadata.GetProperty("visual_fidelity_failure_reason").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("ModelUploadReady").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("trained_model_authority").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("not_trained_model_authority").GetString());
        Assert.Equal(NoConfiguredVisualJudge.Status, metadata.GetProperty("visual_judge_status").GetString());
        Assert.Equal(
            DesignConversionVisualQaBoundaryPolicy.ProviderRequired,
            metadata.GetProperty("visual_judge_provider_boundary").GetString());
        Assert.Equal(
            "no_visual_provider_artifact",
            metadata.GetProperty("visual_ai_provider_fallback_reason").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("external_visual_ai_executed").GetString());
        Assert.Equal("0", metadata.GetProperty("estimated_visual_judge_cost_usd").GetString());
    }

    [Theory]
    [InlineData("missing-budget", 400, DesignConversionModelServerApi.TrustedVisualCostEntitlementRequiredErrorCode)]
    [InlineData("missing-tenant-claim", 403, "DESIGN_CONVERSION_ACTIVE_MODEL_TRUSTED_TENANT_CONTEXT_INVALID")]
    public async Task LocalHttpPostEvaluateAsync_AuthenticatedRouteMissingBudgetOrTenantClaimFailsBeforeRuntime(
        string mode,
        int expectedStatusCode,
        string expectedErrorCode)
    {
        var request = CreateTrustedVisualCostRouteRequest($"trusted-visual-cost-required-{mode}");
        if (string.Equals(mode, "missing-budget", StringComparison.Ordinal))
        {
            request = request with { Budget = null };
        }
        else
        {
            var metadata = new Dictionary<string, string>(request.Metadata!, StringComparer.Ordinal);
            metadata.Remove("tenant_id");
            request = request with { Metadata = metadata };
        }

        var probe = await SubmitTrustedVisualCostRouteAsync(request);

        AssertTrustedVisualCostRejectedBeforeRuntime(probe, expectedStatusCode, expectedErrorCode);
    }

    [Theory]
    [InlineData(DesignConversionModelServerApi.ServerAssignedDesignTierMetadataKey)]
    [InlineData(DesignConversionModelServerApi.ServerAssignedMonthlyVisualCostLimitUsdMetadataKey)]
    [InlineData(DesignConversionModelServerApi.DesignTierAuthorityMetadataKey)]
    [InlineData(DesignConversionModelServerApi.MonthlyVisualCostLimitAuthorityMetadataKey)]
    [InlineData(DesignConversionModelServerApi.UserIdentityAuthorityMetadataKey)]
    public async Task LocalHttpPostEvaluateAsync_AuthenticatedRouteMissingTrustedContractKeyFailsBeforeRuntime(
        string missingKey)
    {
        var request = CreateTrustedVisualCostRouteRequest($"trusted-visual-cost-missing-{missingKey}");
        var metadata = new Dictionary<string, string>(request.Metadata!, StringComparer.Ordinal);
        metadata.Remove(missingKey);
        request = request with { Metadata = metadata };

        var probe = await SubmitTrustedVisualCostRouteAsync(request);

        AssertTrustedVisualCostRejectedBeforeRuntime(
            probe,
            400,
            DesignConversionModelServerApi.TrustedVisualCostEntitlementRequiredErrorCode);
    }

    [Fact]
    public async Task LocalHttpPostEvaluateAsync_AuthenticatedRouteTenantMismatchFailsBeforeRuntime()
    {
        var request = CreateTrustedVisualCostRouteRequest("trusted-visual-cost-tenant-mismatch");
        var metadata = new Dictionary<string, string>(request.Metadata!, StringComparer.Ordinal)
        {
            ["tenant_id"] = "tenant-visual-cost-route-beta"
        };
        request = request with { Metadata = metadata };

        var probe = await SubmitTrustedVisualCostRouteAsync(request);

        AssertTrustedVisualCostRejectedBeforeRuntime(
            probe,
            403,
            "DESIGN_CONVERSION_ACTIVE_MODEL_TENANT_CONTEXT_MISMATCH",
            "tenant-visual-cost-route-beta");
    }

    [Fact]
    public async Task LocalHttpPostEvaluateAsync_AuthenticatedRouteDuplicateTenantAliasInRequestFailsBeforeRuntime()
    {
        var wrongTenant = "tenant-visual-cost-route-beta";
        var request = CreateTrustedVisualCostRouteRequest("trusted-visual-cost-tenant-duplicate-request-alias");
        var metadata = new Dictionary<string, string>(request.Metadata!, StringComparer.Ordinal)
        {
            ["tenantId"] = wrongTenant
        };
        request = request with { Metadata = metadata };

        var probe = await SubmitTrustedVisualCostRouteAsync(request);

        AssertTrustedVisualCostRejectedBeforeRuntime(
            probe,
            403,
            "DESIGN_CONVERSION_ACTIVE_MODEL_TRUSTED_TENANT_CONTEXT_INVALID",
            wrongTenant);
    }

    [Theory]
    [InlineData("same", TrustedVisualCostRouteTenant, "")]
    [InlineData("conflicting", "tenant-visual-cost-route-beta", "tenant-visual-cost-route-beta")]
    public async Task LocalHttpPostEvaluateAsync_AuthenticatedRouteDuplicateTenantClaimInBudgetFailsBeforeRuntime(
        string mode,
        string budgetTenant,
        string privateTenant)
    {
        var request = CreateTrustedVisualCostRouteRequest($"trusted-visual-cost-tenant-budget-duplicate-{mode}") with
        {
            Budget = CreateTrustedVisualCostRouteRequest($"trusted-visual-cost-tenant-budget-duplicate-base-{mode}").Budget! with
            {
                Metadata = new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["tenant_id"] = budgetTenant
                }
            }
        };

        var probe = await SubmitTrustedVisualCostRouteAsync(request);

        AssertTrustedVisualCostRejectedBeforeRuntime(
            probe,
            403,
            "DESIGN_CONVERSION_ACTIVE_MODEL_TRUSTED_TENANT_CONTEXT_INVALID",
            privateTenant);
    }

    [Theory]
    [InlineData("blank-user-id", "", "paid", DesignConversionModelServerApi.TrustedVisualCostEntitlementRequiredErrorCode)]
    [InlineData("spaced-user-id", " platform-user-visual-cost-alpha ", "paid", DesignConversionModelServerApi.TrustedVisualCostEntitlementInvalidErrorCode)]
    [InlineData("bearer-user-id", "Bearer PRIVATE_TRUSTED_VISUAL_COST_TOKEN", "paid", DesignConversionModelServerApi.TrustedVisualCostEntitlementInvalidErrorCode)]
    [InlineData("unknown-tier", "platform-user-visual-cost-alpha", "enterprise", DesignConversionModelServerApi.TrustedVisualCostEntitlementInvalidErrorCode)]
    [InlineData("uppercase-tier", "platform-user-visual-cost-alpha", "PAID", DesignConversionModelServerApi.TrustedVisualCostEntitlementInvalidErrorCode)]
    [InlineData("spaced-tier", "platform-user-visual-cost-alpha", " paid", DesignConversionModelServerApi.TrustedVisualCostEntitlementInvalidErrorCode)]
    public async Task LocalHttpPostEvaluateAsync_AuthenticatedRouteInvalidUserIdOrTierFailsPrivacySafeBeforeRuntime(
        string mode,
        string userId,
        string tier,
        string expectedErrorCode)
    {
        var request = CreateTrustedVisualCostRouteRequest(
            $"trusted-visual-cost-invalid-{mode}",
            userId: userId,
            tier: tier);

        var probe = await SubmitTrustedVisualCostRouteAsync(request);

        AssertTrustedVisualCostRejectedBeforeRuntime(probe, 400, expectedErrorCode, userId, tier);
    }

    [Fact]
    public async Task SubmitAndEvaluateAsync_ServerOwnedTierNegativeBudgetUsdFailsBeforeRuntimeReservationAndNormalization()
    {
        const string requestId = "trusted-visual-cost-negative-budget-usd";
        var baseRequest = CreateTrustedVisualCostRouteRequest(
            requestId,
            tier: "free",
            monthlyCap: "1");
        var request = baseRequest with
        {
            Budget = baseRequest.Budget! with
            {
                MaxImprovementCandidates = 0,
                BudgetUsd = -0.01m,
                RenderQaPassThreshold = 0.0,
                Metadata = new Dictionary<string, string>
                {
                    ["caller_budget_note"] = "negative-budget-must-not-normalize"
                }
            }
        };
        var events = new List<string>();
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(CreateVerdict(
            "trusted-visual-cost-negative-budget-verdict",
            0.93,
            passed: true));
        var visualJudge = new VisualCostReceiptJudge(0.0600m);
        var service = new DesignConversionInternalService(renderProvider, comparisonProvider, visualJudge, routingFlow: null);
        var evaluator = new RecordingThinClientJobEvaluator(service, events);
        var artifactDb = new TamperingLocalArtifactDb(PersistenceReadbackTamperMode.None);
        var artifactResolver = new CountingArtifactRefResolver();
        var budgetStore = new EventRecordingVisualCostBudgetStore(
            new JsonlDesignConversionVisualCostBudgetStore(
                CreateVisualCostBudgetStoreTempRoot(requestId)),
            events);
        var api = new DesignConversionModelServerApi(
            evaluator,
            () => DateTimeOffset.Parse("2026-07-17T09:00:00Z"),
            artifactDb,
            artifactRefResolver: artifactResolver,
            visualCostLedger: new SeededVisualCostLedger([]),
            visualCostBudgetStore: budgetStore,
            postEvaluationStateStore: TestPostEvaluationStateStore);
        var admission = DesignConversionModelServerRouteAdmission.Create(
            new DesignConversionModelServerRouteAdmissionOptions(
                enabled: true,
                currentToken: TrustedVisualCostRouteToken,
                tenantId: TrustedVisualCostRouteTenant),
            () => DateTimeOffset.Parse("2026-07-17T09:00:00Z"));
        var admissionDecision = admission.AdmitEvaluate(new DesignConversionModelServerHttpRequest(
            Method: "POST",
            Path: DesignConversionModelServerHttpHandler.EndpointPath,
            BodyJson: JsonSerializer.Serialize(request, DesignConversionModelServerHttpHandler.JsonOptions),
            ContentType: "application/json; charset=utf-8",
            Headers: new Dictionary<string, string>
            {
                [DesignConversionModelServerRouteAdmission.AuthorizationHeaderName] =
                    "Bearer " + TrustedVisualCostRouteToken,
                [DesignConversionModelServerRouteAdmission.IdempotencyHeaderName] =
                    request.RequestId + "-idempotency"
            }));
        Assert.True(admissionDecision.IsAccepted, admissionDecision.ErrorMessage);

        var result = await api.SubmitAndEvaluateAsync(request, admissionDecision.AdmissionContext);
        var status = api.AccountingReconciliationRuntime.GetStatus("thin-job-" + request.RequestId);

        Assert.False(result.IsSuccess);
        Assert.Equal(DesignConversionInternalService.NegativeBudgetUsdErrorCode, result.ErrorCode);
        Assert.Equal(
            "DesignConversion improvement-loop budgetUsd must be greater than or equal to zero.",
            result.ErrorMessage);
        Assert.Equal(-0.01m, request.Budget.BudgetUsd);
        Assert.Empty(evaluator.Requests);
        Assert.Empty(renderProvider.Requests);
        Assert.Empty(comparisonProvider.Requests);
        Assert.Equal(0, visualJudge.InvocationCount);
        Assert.Empty(events);
        Assert.Equal(0, artifactResolver.ValidateCalls);
        Assert.Equal(0, artifactDb.AppendCalls);
        Assert.Equal(0, artifactDb.ListCalls);
        Assert.False(status.IsSuccess);
        Assert.Equal("JOB_NOT_FOUND", status.ErrorCode);
    }

    [Theory]
    [InlineData("empty", "")]
    [InlineData("zero", "0")]
    [InlineData("negative", "-1")]
    [InlineData("exponent", "1e2")]
    [InlineData("leading-zero", "01")]
    [InlineData("trailing-zero-noncanonical", "1.0")]
    [InlineData("too-many-decimals", "1.23456")]
    [InlineData("whitespace", " 1")]
    [InlineData("over-max", "1000000.0001")]
    public async Task LocalHttpPostEvaluateAsync_AuthenticatedRouteMalformedMonthlyCapFailsBeforeRuntime(
        string mode,
        string monthlyCap)
    {
        var request = CreateTrustedVisualCostRouteRequest(
            $"trusted-visual-cost-bad-cap-{mode}",
            monthlyCap: monthlyCap);

        var probe = await SubmitTrustedVisualCostRouteAsync(request);

        AssertTrustedVisualCostRejectedBeforeRuntime(
            probe,
            400,
            DesignConversionModelServerApi.TrustedVisualCostEntitlementInvalidErrorCode);
    }

    [Theory]
    [InlineData(DesignConversionModelServerApi.DesignTierAuthorityMetadataKey, "caller_supplied_tier_authority")]
    [InlineData(DesignConversionModelServerApi.MonthlyVisualCostLimitAuthorityMetadataKey, "caller_supplied_monthly_limit_authority")]
    [InlineData(DesignConversionModelServerApi.UserIdentityAuthorityMetadataKey, "caller_supplied_user_authority")]
    public async Task LocalHttpPostEvaluateAsync_AuthenticatedRouteWrongTrustedAuthorityFailsBeforeRuntime(
        string authorityKey,
        string wrongAuthority)
    {
        var request = CreateTrustedVisualCostRouteRequest($"trusted-visual-cost-wrong-authority-{authorityKey}");
        var metadata = new Dictionary<string, string>(request.Metadata!, StringComparer.Ordinal)
        {
            [authorityKey] = wrongAuthority
        };
        request = request with { Metadata = metadata };

        var probe = await SubmitTrustedVisualCostRouteAsync(request);

        AssertTrustedVisualCostRejectedBeforeRuntime(
            probe,
            400,
            DesignConversionModelServerApi.TrustedVisualCostEntitlementInvalidErrorCode,
            wrongAuthority);
    }

    [Theory]
    [InlineData("paid", "125.75", 0.25, 1, 2, true)]
    [InlineData("pro", "1000000", 12.34, 3, 4, true)]
    [InlineData("pro", "1000000.0001", 12.34, 3, 4, false)]
    public async Task LocalHttpPostEvaluateAsync_AuthenticatedRouteMonthlyCapBoundaryPreservesPerJobBudgets(
        string tier,
        string monthlyCap,
        decimal budgetUsd,
        int maxImprovementCandidates,
        int maxRenderQaRuns,
        bool shouldSucceed)
    {
        const string userId = "platform-user-visual-cost-boundary";
        var request = CreateTrustedVisualCostRouteRequest(
            $"trusted-visual-cost-cap-boundary-{monthlyCap.Replace('.', '-')}",
            userId: userId,
            tier: tier,
            monthlyCap: monthlyCap) with
        {
            Budget = CreateTrustedVisualCostRouteRequest(
                $"trusted-visual-cost-cap-boundary-budget-{monthlyCap.Replace('.', '-')}",
                userId: userId,
                tier: tier,
                monthlyCap: monthlyCap).Budget! with
            {
                MaxImprovementCandidates = maxImprovementCandidates,
                MaxRenderQaRuns = maxRenderQaRuns,
                BudgetUsd = budgetUsd,
                Metadata = new Dictionary<string, string>
                {
                    ["caller_budget_note"] = "ordinary"
                }
            }
        };

        var probe = await SubmitTrustedVisualCostRouteAsync(request);

        if (!shouldSucceed)
        {
            AssertTrustedVisualCostRejectedBeforeRuntime(
                probe,
                400,
                DesignConversionModelServerApi.TrustedVisualCostEntitlementInvalidErrorCode,
                monthlyCap);
            return;
        }

        Assert.Equal(200, probe.Response.StatusCode);
        var runtimeRequest = Assert.Single(probe.Evaluator.Requests);
        Assert.NotEqual(userId, runtimeRequest.Budget.UserId);
        Assert.DoesNotContain(userId, runtimeRequest.Budget.UserId, StringComparison.Ordinal);
        Assert.Equal(tier, runtimeRequest.Budget.Tier);
        Assert.Equal(maxImprovementCandidates, runtimeRequest.Budget.MaxImprovementCandidates);
        Assert.Equal(maxRenderQaRuns, runtimeRequest.Budget.MaxRenderQaRuns);
        Assert.Equal(budgetUsd, runtimeRequest.Budget.BudgetUsd);
        Assert.Equal("ordinary", runtimeRequest.Budget.Metadata["caller_budget_note"]);
        Assert.Equal(bool.TrueString, runtimeRequest.Budget.Metadata["trusted_visual_cost_entitlement_bound"]);
        Assert.Equal(tier, runtimeRequest.Budget.Metadata[DesignConversionModelServerApi.ServerAssignedDesignTierMetadataKey]);
        Assert.Equal(monthlyCap, runtimeRequest.Budget.Metadata[DesignConversionModelServerApi.ServerAssignedMonthlyVisualCostLimitUsdMetadataKey]);
        Assert.Equal(
            DesignConversionModelServerApi.PlatformTenantConfigurationAuthority,
            runtimeRequest.Budget.Metadata[DesignConversionModelServerApi.DesignTierAuthorityMetadataKey]);
        Assert.Equal(
            DesignConversionModelServerApi.PlatformTenantConfigurationAuthority,
            runtimeRequest.Budget.Metadata[DesignConversionModelServerApi.MonthlyVisualCostLimitAuthorityMetadataKey]);
        Assert.Equal(
            DesignConversionModelServerApi.VerifiedInboundPrincipalAuthority,
            runtimeRequest.Budget.Metadata[DesignConversionModelServerApi.UserIdentityAuthorityMetadataKey]);
        Assert.Equal(bool.FalseString, runtimeRequest.Budget.Metadata["trusted_visual_cost_raw_user_exposed"]);
        Assert.Equal(bool.TrueString, runtimeRequest.Budget.Metadata["trusted_visual_cost_reservation_store_wired"]);
        Assert.Equal(bool.TrueString, runtimeRequest.Provenance["trusted_visual_cost_entitlement_bound"]);
        Assert.Equal(tier, runtimeRequest.Provenance["trusted_visual_cost_entitlement_design_tier"]);
        Assert.Equal(monthlyCap, runtimeRequest.Provenance["trusted_visual_cost_entitlement_monthly_limit_usd"]);
        Assert.Equal(bool.FalseString, runtimeRequest.Provenance["trusted_visual_cost_entitlement_raw_user_exposed"]);
        Assert.Equal(bool.TrueString, runtimeRequest.Provenance["trusted_visual_cost_reservation_store_wired"]);
        Assert.Equal(["reserve", "evaluate", "settle:0.06"], probe.Events);
        Assert.Equal(1, probe.VisualJudge.InvocationCount);
        Assert.NotNull(probe.BudgetStore);
        var budgetPeriod = await probe.BudgetStore!.ReadPeriodAsync(BuildTrustedRouteBudgetPeriodQuery(runtimeRequest));
        Assert.True(budgetPeriod.IsSuccess, budgetPeriod.ErrorMessage);
        Assert.Equal(1, budgetPeriod.Value!.SettledReservationCount);
        Assert.Equal(0, budgetPeriod.Value.ActiveReservationCount);
        Assert.Equal(0.0600m, budgetPeriod.Value.FinalizedSpendUsd);
        Assert.DoesNotContain(userId, probe.Response.BodyJson, StringComparison.Ordinal);

        using var document = JsonDocument.Parse(probe.Response.BodyJson);
        var boundaryMetadata = document.RootElement.GetProperty("boundaryMetadata");
        Assert.Equal(bool.TrueString, boundaryMetadata.GetProperty("trusted_visual_cost_entitlement_bound").GetString());
        Assert.Equal(tier, boundaryMetadata.GetProperty("trusted_visual_cost_entitlement_design_tier").GetString());
        Assert.Equal(monthlyCap, boundaryMetadata.GetProperty("trusted_visual_cost_entitlement_monthly_limit_usd").GetString());
        Assert.Equal(bool.FalseString, boundaryMetadata.GetProperty("trusted_visual_cost_entitlement_raw_user_exposed").GetString());
        Assert.Equal(bool.TrueString, boundaryMetadata.GetProperty("trusted_visual_cost_reservation_store_wired").GetString());
    }

    [Fact]
    public async Task LocalHttpPostEvaluateAsync_AuthenticatedRouteWithoutBudgetStoreFailsBeforeEvaluator()
    {
        const string privateDesignTreeMarker =
            "PRIVATE_MISSING_BUDGET_STORE_DESIGN_TREE_7F31";
        const string privateHtmlMarker =
            "PRIVATE_MISSING_BUDGET_STORE_HTML_8A42";
        const string privateCssMarker =
            "PRIVATE_MISSING_BUDGET_STORE_CSS_9B53";
        var baseRequest = CreateTrustedVisualCostRouteRequest(
            "trusted-visual-cost-missing-budget-store");
        var request = baseRequest with
        {
            DesignTree = baseRequest.DesignTree with
            {
                Payload = privateDesignTreeMarker
            },
            Html = $"<main>{privateHtmlMarker}</main>",
            Css = $"main::before{{content:'{privateCssMarker}';}}"
        };

        Assert.Contains(privateDesignTreeMarker, request.DesignTree.Payload, StringComparison.Ordinal);
        Assert.Contains(privateHtmlMarker, request.Html, StringComparison.Ordinal);
        Assert.Contains(privateCssMarker, request.Css, StringComparison.Ordinal);

        var probe = await SubmitTrustedVisualCostRouteAsync(request, wireVisualCostBudgetStore: false);

        Assert.Equal(400, probe.Response.StatusCode);
        using var document = JsonDocument.Parse(probe.Response.BodyJson);
        var root = document.RootElement;
        Assert.Equal(
            DesignConversionThinClientJobRuntime.TrustedVisualCostBudgetStoreRequiredErrorCode,
            root.GetProperty("errorCode").GetString());
        Assert.Equal(
            "DesignConversion trusted visual-cost budget store is required before evaluation.",
            root.GetProperty("errorMessage").GetString());
        Assert.Equal(
            bool.FalseString,
            root.GetProperty("boundaryMetadata").GetProperty("runtime_invoked").GetString());
        Assert.Empty(probe.Evaluator.Requests);
        Assert.Empty(probe.Events);
        Assert.Null(probe.BudgetStore);
        Assert.Equal(0, probe.VisualJudge.InvocationCount);
        Assert.DoesNotContain(TrustedVisualCostRouteToken, probe.Response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(request.Budget!.UserId, probe.Response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(TrustedVisualCostRouteTenant, probe.Response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(privateDesignTreeMarker, probe.Response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(privateHtmlMarker, probe.Response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(privateCssMarker, probe.Response.BodyJson, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(DesignConversionModelServerApi.ServerAssignedDesignTierMetadataKey)]
    [InlineData(DesignConversionModelServerApi.ServerAssignedMonthlyVisualCostLimitUsdMetadataKey)]
    [InlineData(DesignConversionModelServerApi.DesignTierAuthorityMetadataKey)]
    [InlineData(DesignConversionModelServerApi.MonthlyVisualCostLimitAuthorityMetadataKey)]
    [InlineData(DesignConversionModelServerApi.UserIdentityAuthorityMetadataKey)]
    public async Task LocalHttpPostEvaluateAsync_AuthenticatedRouteBudgetOnlyCanonicalReservedKeyFailsBeforeRuntime(
        string reservedKey)
    {
        var request = CreateTrustedVisualCostRouteRequest($"trusted-visual-cost-budget-only-{reservedKey}");
        var metadata = new Dictionary<string, string>(request.Metadata!, StringComparer.Ordinal);
        metadata.Remove(reservedKey);
        request = request with
        {
            Metadata = metadata,
            Budget = request.Budget! with
            {
                Metadata = new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    [reservedKey] = TrustedVisualCostCanonicalValue(reservedKey)
                }
            }
        };

        var probe = await SubmitTrustedVisualCostRouteAsync(request);

        AssertTrustedVisualCostRejectedBeforeRuntime(
            probe,
            400,
            DesignConversionModelServerApi.TrustedVisualCostEntitlementInvalidErrorCode,
            TrustedVisualCostCanonicalValue(reservedKey));
    }

    [Theory]
    [InlineData(DesignConversionModelServerApi.ServerAssignedDesignTierMetadataKey)]
    [InlineData(DesignConversionModelServerApi.ServerAssignedMonthlyVisualCostLimitUsdMetadataKey)]
    [InlineData(DesignConversionModelServerApi.DesignTierAuthorityMetadataKey)]
    [InlineData(DesignConversionModelServerApi.MonthlyVisualCostLimitAuthorityMetadataKey)]
    [InlineData(DesignConversionModelServerApi.UserIdentityAuthorityMetadataKey)]
    public async Task LocalHttpPostEvaluateAsync_AuthenticatedRouteWrongCaseOnlyReservedKeyFailsBeforeRuntime(
        string reservedKey)
    {
        var request = CreateTrustedVisualCostRouteRequest($"trusted-visual-cost-wrong-case-{reservedKey}");
        var metadata = new Dictionary<string, string>(request.Metadata!, StringComparer.Ordinal);
        metadata.Remove(reservedKey);
        metadata[reservedKey.ToUpperInvariant()] = TrustedVisualCostCanonicalValue(reservedKey);
        request = request with { Metadata = metadata };

        var probe = await SubmitTrustedVisualCostRouteAsync(request);

        AssertTrustedVisualCostRejectedBeforeRuntime(
            probe,
            400,
            DesignConversionModelServerApi.TrustedVisualCostEntitlementInvalidErrorCode,
            TrustedVisualCostCanonicalValue(reservedKey));
    }

    [Fact]
    public async Task LocalHttpPostEvaluateAsync_AuthenticatedRouteCaseVariantDuplicateReservedMetadataFailsBeforeRuntime()
    {
        var request = CreateTrustedVisualCostRouteRequest("trusted-visual-cost-case-duplicate");
        var metadata = new Dictionary<string, string>(request.Metadata!, StringComparer.Ordinal)
        {
            [DesignConversionModelServerApi.ServerAssignedMonthlyVisualCostLimitUsdMetadataKey.ToUpperInvariant()] = "125.75"
        };
        request = request with { Metadata = metadata };

        var probe = await SubmitTrustedVisualCostRouteAsync(request);

        AssertTrustedVisualCostRejectedBeforeRuntime(
            probe,
            400,
            DesignConversionModelServerApi.TrustedVisualCostEntitlementInvalidErrorCode);
    }

    [Theory]
    [InlineData("budget-tier", "pro", null, null)]
    [InlineData("budget-metadata-tier", null, "design_tier", "pro")]
    [InlineData("budget-metadata-cap", null, "monthly_visual_cost_limit_usd", "999")]
    public async Task LocalHttpPostEvaluateAsync_AuthenticatedRouteCallerBudgetCannotEscalateTrustedContract(
        string mode,
        string? budgetTier,
        string? budgetMetadataKey,
        string? budgetMetadataValue)
    {
        var request = CreateTrustedVisualCostRouteRequest($"trusted-visual-cost-escalation-{mode}");
        var budgetMetadata = new Dictionary<string, string>(StringComparer.Ordinal);
        if (budgetMetadataKey is not null)
            budgetMetadata[budgetMetadataKey] = budgetMetadataValue!;
        request = request with
        {
            Budget = request.Budget! with
            {
                Tier = budgetTier ?? request.Budget.Tier,
                Metadata = budgetMetadata
            }
        };

        var probe = await SubmitTrustedVisualCostRouteAsync(request);

        AssertTrustedVisualCostRejectedBeforeRuntime(
            probe,
            400,
            DesignConversionModelServerApi.TrustedVisualCostEntitlementMismatchErrorCode);
    }

    [Fact]
    public async Task SubmitAndEvaluateAsync_NullRouteContextSpoofedVisualCostMarkersFailClosedBeforeRuntime()
    {
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(CreateVerdict("trusted-visual-cost-direct-spoof", 0.93, passed: true));
        var service = new DesignConversionInternalService(renderProvider, comparisonProvider, routingFlow: null);
        var evaluator = new RecordingThinClientJobEvaluator(service);
        var api = new DesignConversionModelServerApi(
            evaluator,
            () => DateTimeOffset.Parse("2026-07-17T09:00:00Z"),
            visualCostLedger: new SeededVisualCostLedger([]),
            postEvaluationStateStore: TestPostEvaluationStateStore);
        var request = CreateTrustedVisualCostRouteRequest("trusted-visual-cost-direct-spoof");
        var metadata = new Dictionary<string, string>(request.Metadata!, StringComparer.Ordinal)
        {
            ["trusted_visual_cost_reservation_store_wired"] = bool.TrueString
        };
        request = request with
        {
            Metadata = metadata,
            Budget = request.Budget! with
            {
                Metadata = new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["trusted_visual_cost_entitlement_bound"] = bool.TrueString,
                    ["trusted_visual_cost_reservation_store_wired"] = bool.TrueString
                }
            }
        };

        var result = await api.SubmitAndEvaluateAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal(DesignConversionModelServerApi.TrustedVisualCostEntitlementInvalidErrorCode, result.ErrorCode);
        Assert.Contains("tenant-bound route admission", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(evaluator.Requests);
        Assert.Empty(renderProvider.Requests);
        Assert.Empty(comparisonProvider.Requests);
    }

    [Fact]
    public async Task SubmitAndEvaluateAsync_NullRouteContextOrdinaryMetadataStillEvaluates()
    {
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(CreateVerdict("ordinary-direct-visual-cost-metadata", 0.93, passed: true));
        var service = new DesignConversionInternalService(renderProvider, comparisonProvider, routingFlow: null);
        var evaluator = new RecordingThinClientJobEvaluator(service);
        var api = new DesignConversionModelServerApi(
            evaluator,
            () => DateTimeOffset.Parse("2026-07-17T09:00:00Z"),
            visualCostLedger: new SeededVisualCostLedger([]),
            postEvaluationStateStore: TestPostEvaluationStateStore);
        var baseRequest = CreateRequest("ordinary-direct-visual-cost-metadata");
        var request = baseRequest with
        {
            Metadata = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["caller_note"] = "ordinary-direct"
            },
            Budget = baseRequest.Budget! with
            {
                Metadata = new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["caller_budget_note"] = "ordinary-direct"
                }
            }
        };

        var result = await api.SubmitAndEvaluateAsync(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Single(evaluator.Requests);
        Assert.NotEmpty(renderProvider.Requests);
        Assert.NotEmpty(comparisonProvider.Requests);
    }

    [Fact]
    public async Task LocalHttpPostEvaluateAsync_InvalidRequest_ReturnsBadRequestAndDoesNotCallRuntime()
    {
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(CreateVerdict("pass122-http-invalid-verdict", 0.93, passed: true));
        var service = new DesignConversionInternalService(renderProvider, comparisonProvider, routingFlow: null);
        var api = new DesignConversionModelServerApi(service, () => DateTimeOffset.Parse("2026-07-03T09:00:00Z"));
        var handler = new DesignConversionModelServerHttpHandler(api);
        var apiRequest = CreateRequest("pass122-http-invalid-request") with { Html = "" };

        var httpResponse = await handler.HandleAsync(new DesignConversionModelServerHttpRequest(
            Method: "POST",
            Path: DesignConversionModelServerHttpHandler.EndpointPath,
            BodyJson: JsonSerializer.Serialize(apiRequest, DesignConversionModelServerHttpHandler.JsonOptions),
            ContentType: "application/json; charset=utf-8"));

        Assert.Equal(400, httpResponse.StatusCode);
        Assert.Equal("application/json; charset=utf-8", httpResponse.ContentType);
        Assert.Empty(renderProvider.Requests);
        Assert.Empty(comparisonProvider.Requests);

        using var document = JsonDocument.Parse(httpResponse.BodyJson);
        var root = document.RootElement;
        Assert.Equal(DesignConversionModelServerHttpHandler.EndpointPath, root.GetProperty("endpoint").GetString());
        Assert.Equal("POST", root.GetProperty("method").GetString());
        Assert.Equal(DesignConversionModelServerHttpHandler.LocalHttpSeam, root.GetProperty("httpSeam").GetString());
        Assert.Equal("DESIGN_CONVERSION_HTML_REQUIRED", root.GetProperty("errorCode").GetString());
        Assert.Contains("DesignConversion", root.GetProperty("errorMessage").GetString(), StringComparison.Ordinal);

        var metadata = root.GetProperty("boundaryMetadata");
        Assert.Equal(bool.TrueString, metadata.GetProperty("local_http_host_seam_verified").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("runtime_invoked").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("socket_opened").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("public_http_endpoint_verified").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("cross_repo_caller_verified").GetString());
    }

    [Fact]
    public async Task LocalHttpPostEvaluateAsync_OversizedSerializedPayload_ReturnsPayloadTooLargeBadRequestWithoutRuntime()
    {
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(CreateVerdict("payload-too-large-http-verdict", 0.93, passed: true));
        var service = new DesignConversionInternalService(renderProvider, comparisonProvider, routingFlow: null);
        var api = new DesignConversionModelServerApi(service, () => DateTimeOffset.Parse("2026-07-09T10:00:00Z"));
        var handler = new DesignConversionModelServerHttpHandler(api);
        var apiRequest = CreateRequest("payload-too-large-http-request") with
        {
            DesignTree = new DesignConversionModelServerPayloadRef(
                RefId: "payload-too-large-tree-ref",
                Kind: "figma-json",
                Sha256: "sha256:payload-too-large-tree",
                Payload: new string('x', DesignConversionModelServerPayloadSizeGuard.MaxRawPayloadChars + 1),
                Format: "figma-json")
        };

        var httpResponse = await handler.HandleAsync(new DesignConversionModelServerHttpRequest(
            Method: "POST",
            Path: DesignConversionModelServerHttpHandler.EndpointPath,
            BodyJson: JsonSerializer.Serialize(apiRequest, DesignConversionModelServerHttpHandler.JsonOptions),
            ContentType: "application/json; charset=utf-8"));

        Assert.Equal(400, httpResponse.StatusCode);
        Assert.Contains(DesignConversionModelServerPayloadSizeGuard.ErrorCode, httpResponse.BodyJson, StringComparison.Ordinal);
        Assert.Empty(renderProvider.Requests);
        Assert.Empty(comparisonProvider.Requests);

        using var document = JsonDocument.Parse(httpResponse.BodyJson);
        var root = document.RootElement;
        Assert.Equal(DesignConversionModelServerHttpHandler.EndpointPath, root.GetProperty("endpoint").GetString());
        Assert.Equal("POST", root.GetProperty("method").GetString());
        Assert.Equal(DesignConversionModelServerPayloadSizeGuard.ErrorCode, root.GetProperty("errorCode").GetString());
        Assert.Contains("designTree.payload", root.GetProperty("errorMessage").GetString(), StringComparison.OrdinalIgnoreCase);

        var metadata = root.GetProperty("boundaryMetadata");
        Assert.Equal(bool.FalseString, metadata.GetProperty("runtime_invoked").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("socket_opened").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("public_http_endpoint_verified").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("cross_repo_caller_verified").GetString());
    }

    [Fact]
    public async Task LocalHttpPostImprovementLoopAsync_OversizedSerializedPayload_ReturnsPayloadTooLargeBadRequestWithoutRuntime()
    {
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(CreateVerdict("payload-too-large-loop-verdict", 0.93, passed: true));
        var service = new DesignConversionInternalService(renderProvider, comparisonProvider, routingFlow: null);
        var candidateStrategy = new RecordingUnexpectedCandidateStrategy();
        var coordinator = new DesignConversionImprovementLoopCoordinator(service, candidateStrategy);
        var handler = new DesignConversionModelServerImprovementLoopHandler(coordinator);
        var initialPackage = CreateLocalCallerPackage("payload-too-large-loop-request") with
        {
            DesignTree = new DesignConversionModelServerPayloadRef(
                RefId: "payload-too-large-loop-tree-ref",
                Kind: "figma-json",
                Sha256: "sha256:payload-too-large-loop-tree",
                Payload: new string('y', DesignConversionModelServerPayloadSizeGuard.MaxRawPayloadChars + 1),
                Format: "figma-json")
        };
        var loopRequest = new DesignConversionModelServerImprovementLoopRequest(
            LoopId: "payload-too-large-loop",
            InitialPackage: initialPackage,
            MaxAttempts: 2);

        var httpResponse = await handler.HandleAsync(new DesignConversionModelServerHttpRequest(
            Method: "POST",
            Path: DesignConversionModelServerImprovementLoopHandler.EndpointPath,
            BodyJson: JsonSerializer.Serialize(loopRequest, DesignConversionModelServerHttpHandler.JsonOptions),
            ContentType: "application/json; charset=utf-8"));

        Assert.Equal(400, httpResponse.StatusCode);
        Assert.Contains(DesignConversionModelServerPayloadSizeGuard.ErrorCode, httpResponse.BodyJson, StringComparison.Ordinal);
        Assert.Empty(renderProvider.Requests);
        Assert.Empty(comparisonProvider.Requests);
        Assert.Empty(candidateStrategy.Requests);

        using var document = JsonDocument.Parse(httpResponse.BodyJson);
        var root = document.RootElement;
        Assert.Equal(DesignConversionModelServerImprovementLoopHandler.EndpointPath, root.GetProperty("endpoint").GetString());
        Assert.Equal("POST", root.GetProperty("method").GetString());
        Assert.Equal(DesignConversionModelServerPayloadSizeGuard.ErrorCode, root.GetProperty("errorCode").GetString());
        Assert.Contains("designTree.payload", root.GetProperty("errorMessage").GetString(), StringComparison.OrdinalIgnoreCase);

        var metadata = root.GetProperty("boundaryMetadata");
        Assert.Equal(bool.TrueString, metadata.GetProperty("improvement_loop_coordinator_configured").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("runtime_invoked").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("network_call_executed").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("socket_opened").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("public_http_endpoint_verified").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("cross_repo_caller_verified").GetString());
    }

    [Theory]
    [InlineData(false, DesignConversionRenderResolutionFactory.InvalidDeviceScaleFactorErrorCode)]
    [InlineData(true, DesignConversionRenderResolutionFactory.ConflictingDeviceScaleFactorErrorCode)]
    public async Task LocalHttpPostImprovementLoopAsync_DeviceScaleFactorFailureReturnsTypedErrorWithoutRuntime(
        bool conflicting,
        string expectedErrorCode)
    {
        const string privateMalformedDeviceScaleFactor = "PRIVATE_MALFORMED_DSF_MUST_NOT_LEAK";
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(CreateVerdict("dsf-validation-loop-verdict", 0.93, passed: true));
        var service = new DesignConversionInternalService(renderProvider, comparisonProvider, routingFlow: null);
        var candidateStrategy = new RecordingUnexpectedCandidateStrategy();
        var coordinator = new DesignConversionImprovementLoopCoordinator(service, candidateStrategy);
        var handler = new DesignConversionModelServerImprovementLoopHandler(coordinator);
        var initialPackage = CreateLocalCallerPackage("dsf-validation-loop-request");
        var screenshot = initialPackage.Screenshots![0] with
        {
            Metadata = new Dictionary<string, string>
            {
                [DesignConversionRenderResolutionFactory.DeviceScaleFactorMetadataKey] =
                    conflicting ? "2" : privateMalformedDeviceScaleFactor
            }
        };
        var designRender = initialPackage.DesignRenders![0] with
        {
            Metadata = conflicting
                ? new Dictionary<string, string>
                {
                    [DesignConversionRenderResolutionFactory.DeviceScaleFactorMetadataKey] = "3"
                }
                : new Dictionary<string, string>()
        };
        initialPackage = initialPackage with
        {
            Screenshots = [screenshot],
            DesignRenders = [designRender]
        };
        var loopRequest = new DesignConversionModelServerImprovementLoopRequest(
            LoopId: "dsf-validation-loop",
            InitialPackage: initialPackage,
            MaxAttempts: 2);

        var httpResponse = await handler.HandleAsync(new DesignConversionModelServerHttpRequest(
            Method: "POST",
            Path: DesignConversionModelServerImprovementLoopHandler.EndpointPath,
            BodyJson: JsonSerializer.Serialize(loopRequest, DesignConversionModelServerHttpHandler.JsonOptions),
            ContentType: "application/json; charset=utf-8"));

        Assert.Equal(400, httpResponse.StatusCode);
        Assert.Empty(renderProvider.Requests);
        Assert.Empty(comparisonProvider.Requests);
        Assert.Empty(candidateStrategy.Requests);
        Assert.DoesNotContain(privateMalformedDeviceScaleFactor, httpResponse.BodyJson, StringComparison.Ordinal);

        using var document = JsonDocument.Parse(httpResponse.BodyJson);
        var root = document.RootElement;
        Assert.Equal(expectedErrorCode, root.GetProperty("errorCode").GetString());
        Assert.Equal(bool.FalseString, root.GetProperty("boundaryMetadata").GetProperty("runtime_invoked").GetString());
    }

    [Fact]
    public void CreateGrouped_ViewportIdentitySanitizesUnsafeAliasesAndRejectsConflicts()
    {
        const string privateViewport = @"PRIVATE_CALLER_VIEWPORT_C:\tenant\secret\mobile-menu-output.png";
        var unsafeArtifact = new DesignRenderArtifact(
            ArtifactId: "unsafe-viewport",
            Kind: "design-render",
            Width: 390,
            Height: 844,
            Uri: "artifact://identity/unsafe.png",
            Sha256: "sha256:unsafe",
            Metadata: new Dictionary<string, string>
            {
                ["viewport_id"] = privateViewport
            });

        var first = DesignConversionRenderResolutionFactory.CreateGrouped([unsafeArtifact]);
        var second = DesignConversionRenderResolutionFactory.CreateGrouped([unsafeArtifact]);

        Assert.True(first.IsSuccess, first.ErrorMessage);
        Assert.True(second.IsSuccess, second.ErrorMessage);
        var firstId = Assert.Single(first.Value!).ResolutionId;
        var secondId = Assert.Single(second.Value!).ResolutionId;
        Assert.Equal(firstId, secondId);
        Assert.StartsWith("source-390x844~hash-", firstId, StringComparison.Ordinal);
        Assert.DoesNotContain(privateViewport, firstId, StringComparison.Ordinal);
        Assert.DoesNotContain("PRIVATE", firstId, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(@"\tenant\secret", firstId, StringComparison.OrdinalIgnoreCase);

        var canonicalScaleArtifact = unsafeArtifact with
        {
            ArtifactId = "canonical-scale-viewport",
            Metadata = new Dictionary<string, string>
            {
                ["viewportLabel"] = "source-390x844@3~mobile-menu"
            }
        };
        var canonicalScale = DesignConversionRenderResolutionFactory.CreateGrouped([canonicalScaleArtifact]);

        Assert.True(canonicalScale.IsSuccess, canonicalScale.ErrorMessage);
        var inheritedScaleResolution = Assert.Single(canonicalScale.Value!);
        Assert.Equal("source-390x844@3~mobile-menu", inheritedScaleResolution.ResolutionId);
        Assert.Equal(3d, inheritedScaleResolution.DeviceScaleFactor);

        var conflictingCanonicalScaleArtifact = canonicalScaleArtifact with
        {
            Metadata = new Dictionary<string, string>
            {
                ["viewportLabel"] = "source-390x844@3~mobile-menu",
                [DesignConversionRenderResolutionFactory.DeviceScaleFactorMetadataKey] = "2"
            }
        };
        var canonicalScaleConflict = DesignConversionRenderResolutionFactory.CreateGrouped([conflictingCanonicalScaleArtifact]);

        Assert.False(canonicalScaleConflict.IsSuccess);
        Assert.Equal(DesignConversionRenderResolutionFactory.ConflictingDeviceScaleFactorErrorCode, canonicalScaleConflict.ErrorCode);

        var conflictingArtifact = unsafeArtifact with
        {
            Metadata = new Dictionary<string, string>
            {
                ["viewport_id"] = "mobile-menu",
                ["viewportLabel"] = "mobile-cart"
            }
        };
        var conflict = DesignConversionRenderResolutionFactory.CreateGrouped([conflictingArtifact]);

        Assert.False(conflict.IsSuccess);
        Assert.Equal("RENDER_RESOLUTION_VIEWPORT_ID_CONFLICT", conflict.ErrorCode);
        Assert.DoesNotContain("mobile-menu", conflict.ErrorMessage, StringComparison.Ordinal);
        Assert.DoesNotContain("mobile-cart", conflict.ErrorMessage, StringComparison.Ordinal);
    }

    [Fact]
    public async Task EndpointRegistry_RegisterDesignConversionEvaluate_ExposesRouteAndDelegatesToHandler()
    {
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(CreateVerdict("pass123-registry-valid-verdict", 0.94, passed: true));
        var service = new DesignConversionInternalService(renderProvider, comparisonProvider, routingFlow: null);
        var api = new DesignConversionModelServerApi(
            service,
            () => DateTimeOffset.Parse("2026-07-03T11:00:00Z"),
            postEvaluationStateStore: TestPostEvaluationStateStore);
        var handler = new DesignConversionModelServerHttpHandler(api);
        var registry = new DesignConversionModelServerEndpointRegistry();

        var registered = registry.RegisterDesignConversionEvaluate(handler);

        Assert.True(registered.IsSuccess, registered.ErrorMessage);
        Assert.Same(
            api.AccountingReconciliationRuntime,
            handler.AccountingReconciliationRuntime);
        Assert.Same(
            api.AccountingReconciliationRuntime,
            registry.AccountingReconciliationRuntime);
        Assert.Equal("POST", registered.Value!.Method);
        Assert.Equal(DesignConversionModelServerHttpHandler.EndpointPath, registered.Value.Path);
        Assert.Equal(DesignConversionModelServerEndpointRegistry.EndpointRoute, registered.Value.BoundaryMetadata["endpoint_route"]);
        Assert.Equal(
            DesignConversionModelServerEndpointRegistry.HostIntegrationBoundary,
            registered.Value.BoundaryMetadata["host_integration_boundary"]);
        Assert.Equal(nameof(DesignConversionModelServerHttpHandler), registered.Value.BoundaryMetadata["delegates_to_handler"]);
        Assert.False(registered.Value.AspNetHostVerified);
        Assert.False(registered.Value.SocketOpened);
        Assert.False(registered.Value.PublicHttpEndpointVerified);
        Assert.False(registered.Value.CrossRepoCallerVerified);

        var resolved = registry.Resolve("post", DesignConversionModelServerHttpHandler.EndpointPath);
        Assert.True(resolved.IsSuccess, resolved.ErrorMessage);
        Assert.Same(registered.Value, resolved.Value);
        Assert.Same(handler, resolved.Value!.Handler.Target);

        var internalResolved = registry.Resolve(
            "POST",
            DesignConversionModelServerEndpointRegistry.InternalEndpointPath);
        Assert.True(internalResolved.IsSuccess, internalResolved.ErrorMessage);
        Assert.Equal(
            DesignConversionModelServerEndpointRegistry.InternalEndpointPath,
            internalResolved.Value!.Path);
        Assert.Equal(
            DesignConversionModelServerEndpointRegistry.InternalEndpointRoute,
            internalResolved.Value.BoundaryMetadata["endpoint_route"]);
        Assert.Equal(
            "design_conversion_evaluate_internal_alias",
            internalResolved.Value.BoundaryMetadata["endpoint_route_kind"]);
        Assert.Equal(
            nameof(DesignConversionModelServerHttpHandler),
            internalResolved.Value.BoundaryMetadata["delegates_to_handler"]);
        Assert.Same(registered.Value.RouteAdmission, internalResolved.Value.RouteAdmission);
        Assert.Same(handler, internalResolved.Value.Handler.Target);

        var apiRequest = CreateRequest("pass123-registry-valid-request");
        var invoked = await registry.InvokeAsync(new DesignConversionModelServerHttpRequest(
            Method: "POST",
            Path: DesignConversionModelServerHttpHandler.EndpointPath,
            BodyJson: JsonSerializer.Serialize(apiRequest, DesignConversionModelServerHttpHandler.JsonOptions),
            ContentType: "application/json"));

        Assert.True(invoked.IsSuccess, invoked.ErrorMessage);
        var httpResponse = invoked.Value!;
        Assert.Equal(200, httpResponse.StatusCode);
        Assert.Equal("application/json; charset=utf-8", httpResponse.ContentType);
        Assert.NotEmpty(renderProvider.Requests);
        Assert.NotEmpty(comparisonProvider.Requests);

        using var document = JsonDocument.Parse(httpResponse.BodyJson);
        var root = document.RootElement;
        Assert.Equal(DesignConversionModelServerHttpHandler.EndpointPath, root.GetProperty("endpoint").GetString());
        Assert.Equal("POST", root.GetProperty("method").GetString());
        Assert.Equal(DesignConversionModelServerHttpHandler.LocalHttpSeam, root.GetProperty("httpSeam").GetString());
        Assert.False(root.GetProperty("publicHttpEndpointVerified").GetBoolean());
        Assert.False(root.GetProperty("deploymentVerified").GetBoolean());

        var metadata = root.GetProperty("boundaryMetadata");
        Assert.Equal(bool.TrueString, metadata.GetProperty("local_http_host_seam_verified").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("socket_opened").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("public_http_endpoint_verified").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("cross_repo_caller_verified").GetString());

        var renderCountBeforeInternalAlias = renderProvider.Requests.Count;
        var comparisonCountBeforeInternalAlias = comparisonProvider.Requests.Count;
        var internalApiRequest = CreateRequest("pass123-registry-internal-alias-request");
        var internalInvoked = await registry.InvokeAsync(new DesignConversionModelServerHttpRequest(
            Method: "POST",
            Path: DesignConversionModelServerEndpointRegistry.InternalEndpointPath,
            BodyJson: JsonSerializer.Serialize(internalApiRequest, DesignConversionModelServerHttpHandler.JsonOptions),
            ContentType: "application/json"));

        Assert.True(internalInvoked.IsSuccess, internalInvoked.ErrorMessage);
        Assert.Equal(200, internalInvoked.Value!.StatusCode);
        Assert.True(renderProvider.Requests.Count > renderCountBeforeInternalAlias);
        Assert.True(comparisonProvider.Requests.Count > comparisonCountBeforeInternalAlias);
        Assert.Equal(
            DesignConversionModelServerEndpointRegistry.InternalEndpointPath,
            internalInvoked.Value.Headers["x-design-conversion-endpoint"]);
        Assert.Equal(
            DesignConversionModelServerEndpointRegistry.InternalEndpointPath,
            internalInvoked.Value.Headers["x-design-conversion-path"]);

        using var internalDocument = JsonDocument.Parse(internalInvoked.Value.BodyJson);
        var internalRoot = internalDocument.RootElement;
        Assert.Equal(
            DesignConversionModelServerEndpointRegistry.InternalEndpointPath,
            internalRoot.GetProperty("endpoint").GetString());
        Assert.Equal("POST", internalRoot.GetProperty("method").GetString());
        Assert.False(internalRoot.GetProperty("publicHttpEndpointVerified").GetBoolean());
        Assert.False(internalRoot.GetProperty("deploymentVerified").GetBoolean());
        var internalMetadata = internalRoot.GetProperty("boundaryMetadata");
        Assert.Equal(
            "POST " + DesignConversionModelServerEndpointRegistry.InternalEndpointPath,
            internalMetadata.GetProperty("http_endpoint_shape").GetString());
        Assert.Equal(
            DesignConversionModelServerEndpointRegistry.InternalEndpointPath,
            internalMetadata.GetProperty("http_path").GetString());
        Assert.Equal(bool.TrueString, internalMetadata.GetProperty("runtime_invoked").GetString());
        Assert.Equal(bool.FalseString, internalMetadata.GetProperty("public_http_endpoint_verified").GetString());
        Assert.Equal(bool.FalseString, internalMetadata.GetProperty("deployment_verified").GetString());
    }

    [Fact]
    public async Task EndpointRegistry_InvalidPayload_ReturnsBadRequestWithoutRuntimeAndPreservesBoundaryFlags()
    {
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(CreateVerdict("pass123-registry-invalid-verdict", 0.94, passed: true));
        var service = new DesignConversionInternalService(renderProvider, comparisonProvider, routingFlow: null);
        var api = new DesignConversionModelServerApi(service, () => DateTimeOffset.Parse("2026-07-03T11:00:00Z"));
        var handler = new DesignConversionModelServerHttpHandler(api);
        var registry = new DesignConversionModelServerEndpointRegistry();
        var registered = registry.RegisterDesignConversionEvaluate(handler);
        Assert.True(registered.IsSuccess, registered.ErrorMessage);
        var apiRequest = CreateRequest("pass123-registry-invalid-request") with { Html = "" };

        var invoked = await registry.InvokeAsync(new DesignConversionModelServerHttpRequest(
            Method: "POST",
            Path: DesignConversionModelServerHttpHandler.EndpointPath,
            BodyJson: JsonSerializer.Serialize(apiRequest, DesignConversionModelServerHttpHandler.JsonOptions),
            ContentType: "application/json; charset=utf-8"));
        var missingRoute = await registry.InvokeAsync(new DesignConversionModelServerHttpRequest(
            Method: "POST",
            Path: "/api/design-conversion/missing",
            BodyJson: "{}",
            ContentType: "application/json"));

        Assert.True(invoked.IsSuccess, invoked.ErrorMessage);
        Assert.Equal(400, invoked.Value!.StatusCode);
        Assert.Empty(renderProvider.Requests);
        Assert.Empty(comparisonProvider.Requests);
        Assert.False(missingRoute.IsSuccess);
        Assert.Equal("DESIGN_CONVERSION_ENDPOINT_ROUTE_NOT_FOUND", missingRoute.ErrorCode);

        using var document = JsonDocument.Parse(invoked.Value.BodyJson);
        var root = document.RootElement;
        Assert.Equal("DESIGN_CONVERSION_HTML_REQUIRED", root.GetProperty("errorCode").GetString());

        var metadata = root.GetProperty("boundaryMetadata");
        Assert.Equal(bool.FalseString, metadata.GetProperty("runtime_invoked").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("socket_opened").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("public_http_endpoint_verified").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("cross_repo_caller_verified").GetString());
    }

    [Fact]
    public async Task EndpointRegistry_ApiRuntimeException_ReturnsApiFailedClosedWithoutPrivateLeak()
    {
        const string privateExceptionMarker = "PRIVATE_N12_1_RUNTIME_THROW_SECRET";
        const string privateRequestMarker = "PRIVATE_N12_1_REGISTRY_REQUEST_SECRET";
        var rawDesignTree = $"{{\"type\":\"FRAME\",\"name\":\"{privateRequestMarker}\"}}";
        var rawOriginalTree = $"{{\"type\":\"FRAME\",\"name\":\"original-{privateRequestMarker}\"}}";
        var rawHtml = $"<main data-secret=\"{privateRequestMarker}\">Dashboard</main>";
        var rawCss = $".screen{{background-image:url('{privateRequestMarker}');}}";
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(CreateVerdict("n12-1-registry-runtime-throw-verdict", 0.94, passed: true));
        var service = new DesignConversionInternalService(renderProvider, comparisonProvider, routingFlow: null);
        var api = new DesignConversionModelServerApi(
            service,
            () => throw new InvalidOperationException($"Runtime clock failed with private marker {privateExceptionMarker}."));
        var handler = new DesignConversionModelServerHttpHandler(api);
        var registry = new DesignConversionModelServerEndpointRegistry();
        var registered = registry.RegisterDesignConversionEvaluate(handler);
        Assert.True(registered.IsSuccess, registered.ErrorMessage);
        var baseRequest = CreateRequest("n12-1-registry-handler-exception");
        var apiRequest = baseRequest with
        {
            DesignTree = baseRequest.DesignTree with { Payload = rawDesignTree },
            OriginalTree = baseRequest.OriginalTree with { Payload = rawOriginalTree },
            Html = rawHtml,
            Css = rawCss
        };
        var request = new DesignConversionModelServerHttpRequest(
            Method: "POST",
            Path: DesignConversionModelServerHttpHandler.EndpointPath,
            BodyJson: JsonSerializer.Serialize(apiRequest, DesignConversionModelServerHttpHandler.JsonOptions),
            ContentType: "application/json");

        OperationResult<DesignConversionModelServerHttpResponse>? invoked = null;
        var exception = await Record.ExceptionAsync(async () => invoked = await registry.InvokeAsync(request));

        Assert.Null(exception);
        Assert.NotNull(invoked);
        Assert.True(invoked!.IsSuccess, invoked.ErrorMessage);
        var response = Assert.IsType<DesignConversionModelServerHttpResponse>(invoked.Value);
        Assert.Equal("application/json; charset=utf-8", response.ContentType);

        using var document = JsonDocument.Parse(response.BodyJson);
        var root = document.RootElement;
        Assert.Equal(DesignConversionModelServerApi.UnexpectedRequestFailureErrorCode, root.GetProperty("errorCode").GetString());
        Assert.Equal(DesignConversionModelServerApi.UnexpectedRequestFailureErrorMessage, root.GetProperty("errorMessage").GetString());
        var bodyStatusCode = root.GetProperty("statusCode").GetInt32();

        var errorFields = new[]
        {
            root.GetProperty("errorCode").GetString(),
            root.GetProperty("errorMessage").GetString()
        };
        foreach (var errorField in errorFields)
        {
            Assert.DoesNotContain(privateExceptionMarker, errorField, StringComparison.Ordinal);
            Assert.DoesNotContain(privateRequestMarker, errorField, StringComparison.Ordinal);
            Assert.DoesNotContain(rawHtml, errorField, StringComparison.Ordinal);
            Assert.DoesNotContain(rawCss, errorField, StringComparison.Ordinal);
            Assert.DoesNotContain(rawDesignTree, errorField, StringComparison.Ordinal);
            Assert.DoesNotContain(rawOriginalTree, errorField, StringComparison.Ordinal);
            Assert.DoesNotContain(nameof(InvalidOperationException), errorField, StringComparison.Ordinal);
        }

        Assert.DoesNotContain(privateExceptionMarker, response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(privateRequestMarker, response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(rawHtml, response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(rawCss, response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(rawDesignTree, response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(rawOriginalTree, response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(nameof(InvalidOperationException), response.BodyJson, StringComparison.Ordinal);

        var metadata = root.GetProperty("boundaryMetadata");
        var metadataStatusCode = metadata.GetProperty("http_status_code").GetString();
        Assert.True(
            response.StatusCode == 500 &&
            bodyStatusCode == 500 &&
            string.Equals(metadataStatusCode, "500", StringComparison.Ordinal),
            "Expected API failed-closed status 500 in response, body, and metadata; " +
            $"actual response.StatusCode={response.StatusCode}, body.statusCode={bodyStatusCode}, " +
            $"metadata.http_status_code={metadataStatusCode}.");

        foreach (var falseFlag in new[]
                 {
                     "raw_html_included",
                     "raw_css_included",
                     "raw_design_tree_included",
                     "raw_user_private_data_included",
                     "runtime_invoked",
                     "product_ready"
                  })
        {
            Assert.Equal(bool.FalseString, metadata.GetProperty(falseFlag).GetString());
        }

        Assert.Empty(renderProvider.Requests);
        Assert.Empty(comparisonProvider.Requests);
    }

    [Fact]
    public async Task EndpointRegistry_RuntimeOperationCanceledFromSuppliedToken_ReturnsCancelledFailureWithoutPrivateLeak()
    {
        const string privateExceptionMarker = "PRIVATE_N12_1_RUNTIME_CANCEL_SECRET";
        using var cancellationSource = new CancellationTokenSource();
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(CreateVerdict("n12-1-registry-runtime-cancel-verdict", 0.94, passed: true));
        var service = new DesignConversionInternalService(renderProvider, comparisonProvider, routingFlow: null);
        var api = new DesignConversionModelServerApi(
            service,
            () =>
            {
                cancellationSource.Cancel();
                throw new OperationCanceledException(
                    $"Runtime clock cancelled with private marker {privateExceptionMarker}.",
                    cancellationSource.Token);
            });
        var handler = new DesignConversionModelServerHttpHandler(api);
        var registry = new DesignConversionModelServerEndpointRegistry();
        var registered = registry.RegisterDesignConversionEvaluate(handler);
        Assert.True(registered.IsSuccess, registered.ErrorMessage);
        var apiRequest = CreateRequest("n12-1-registry-handler-cancelled");
        var request = new DesignConversionModelServerHttpRequest(
            Method: "POST",
            Path: DesignConversionModelServerHttpHandler.EndpointPath,
            BodyJson: JsonSerializer.Serialize(apiRequest, DesignConversionModelServerHttpHandler.JsonOptions),
            ContentType: "application/json");

        OperationResult<DesignConversionModelServerHttpResponse>? invoked = null;
        var exception = await Record.ExceptionAsync(async () => invoked = await registry.InvokeAsync(request, cancellationSource.Token));

        Assert.Null(exception);
        Assert.NotNull(invoked);
        Assert.True(invoked!.IsSuccess, invoked.ErrorMessage);
        var response = Assert.IsType<DesignConversionModelServerHttpResponse>(invoked.Value);
        using var document = JsonDocument.Parse(response.BodyJson);
        var root = document.RootElement;
        Assert.Equal(400, response.StatusCode);
        Assert.Equal(DesignConversionModelServerEndpointRegistry.EndpointCancelledErrorCode, root.GetProperty("errorCode").GetString());
        Assert.Equal("DesignConversion model-server API request cancelled.", root.GetProperty("errorMessage").GetString());
        Assert.DoesNotContain(privateExceptionMarker, response.BodyJson, StringComparison.Ordinal);
        Assert.Empty(renderProvider.Requests);
        Assert.Empty(comparisonProvider.Requests);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task EndpointRegistry_RegisteredEvaluateAndStatusOrdinaryFailureReturnsSanitizedUnhandled(
        bool statusRoute)
    {
        const string privateMarker = "PRIVATE_REGISTRY_NONFATAL_EXCEPTION_SECRET";
        var (registry, request) = CreateRegisteredRegistryExceptionBoundaryRoute(
            statusRoute,
            (_, _) => Task.FromException<DesignConversionModelServerHttpResponse>(
                new InvalidOperationException(privateMarker)));

        var result = await registry.InvokeAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal(DesignConversionModelServerEndpointRegistry.EndpointUnhandledErrorCode, result.ErrorCode);
        Assert.Equal(DesignConversionModelServerEndpointRegistry.EndpointUnhandledErrorMessage, result.ErrorMessage);
        Assert.DoesNotContain(privateMarker, result.ErrorCode, StringComparison.Ordinal);
        Assert.DoesNotContain(privateMarker, result.ErrorMessage, StringComparison.Ordinal);
        Assert.DoesNotContain(nameof(InvalidOperationException), result.ErrorMessage, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task EndpointRegistry_RegisteredEvaluateAndStatusSuppliedCancellationReturnsSanitizedCancelled(
        bool statusRoute)
    {
        const string privateMarker = "PRIVATE_REGISTRY_SUPPLIED_CANCELLATION_SECRET";
        using var cancellationSource = new CancellationTokenSource();
        cancellationSource.Cancel();
        var (registry, request) = CreateRegisteredRegistryExceptionBoundaryRoute(
            statusRoute,
            (_, token) => Task.FromException<DesignConversionModelServerHttpResponse>(
                new OperationCanceledException(privateMarker, token)));

        var result = await registry.InvokeAsync(request, cancellationSource.Token);

        Assert.False(result.IsSuccess);
        Assert.Equal(DesignConversionModelServerEndpointRegistry.EndpointCancelledErrorCode, result.ErrorCode);
        Assert.Equal(DesignConversionModelServerEndpointRegistry.EndpointCancelledErrorMessage, result.ErrorMessage);
        Assert.DoesNotContain(privateMarker, result.ErrorCode, StringComparison.Ordinal);
        Assert.DoesNotContain(privateMarker, result.ErrorMessage, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task EndpointRegistry_RegisteredEvaluateAndStatusForeignCancellationEscapes(
        bool statusRoute)
    {
        using var foreignCancellationSource = new CancellationTokenSource();
        foreignCancellationSource.Cancel();
        var expected = new OperationCanceledException(
            "PRIVATE_REGISTRY_FOREIGN_CANCELLATION_SECRET",
            foreignCancellationSource.Token);
        var (registry, request) = CreateRegisteredRegistryExceptionBoundaryRoute(
            statusRoute,
            (_, _) => Task.FromException<DesignConversionModelServerHttpResponse>(expected));

        var thrown = await Assert.ThrowsAsync<OperationCanceledException>(
            () => registry.InvokeAsync(request));

        Assert.Same(expected, thrown);
        Assert.Equal(foreignCancellationSource.Token, thrown.CancellationToken);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task EndpointRegistry_RegisteredEvaluateAndStatusNestedFatalGraphEscapes(
        bool statusRoute)
    {
        var expected = new AggregateException(
            "PRIVATE_REGISTRY_FATAL_GRAPH_SECRET",
            new OutOfMemoryException("Constructed fatal node; no memory pressure used."));
        var (registry, request) = CreateRegisteredRegistryExceptionBoundaryRoute(
            statusRoute,
            (_, _) => Task.FromException<DesignConversionModelServerHttpResponse>(expected));

        var thrown = await Assert.ThrowsAsync<AggregateException>(
            () => registry.InvokeAsync(request));

        Assert.Same(expected, thrown);
        Assert.IsType<OutOfMemoryException>(Assert.Single(thrown.InnerExceptions));
    }

    private (
        DesignConversionModelServerEndpointRegistry Registry,
        DesignConversionModelServerHttpRequest Request)
        CreateRegisteredRegistryExceptionBoundaryRoute(
            bool statusRoute,
            Func<DesignConversionModelServerHttpRequest, CancellationToken,
                Task<DesignConversionModelServerHttpResponse>> replacementHandler)
    {
        var api = CreateApi();
        var registry = new DesignConversionModelServerEndpointRegistry();
        OperationResult<DesignConversionModelServerEndpointRegistration> registered;
        DesignConversionModelServerHttpRequest request;

        if (statusRoute)
        {
            registered = registry.RegisterDesignConversionStatusReadback(
                new DesignConversionModelServerStatusHandler(api));
            request = new DesignConversionModelServerHttpRequest(
                Method: DesignConversionModelServerEndpointRegistry.StatusReadbackEndpointMethod,
                Path: DesignConversionModelServerStatusHandler.EndpointPathPrefix +
                      "registry-exception-boundary-job",
                BodyJson: null);
        }
        else
        {
            registered = registry.RegisterDesignConversionEvaluate(
                new DesignConversionModelServerHttpHandler(api));
            request = new DesignConversionModelServerHttpRequest(
                Method: DesignConversionModelServerEndpointRegistry.EndpointMethod,
                Path: DesignConversionModelServerEndpointRegistry.EndpointPath,
                BodyJson: "{}",
                ContentType: "application/json");
        }

        Assert.True(registered.IsSuccess, registered.ErrorMessage);
        ReplaceRegisteredRouteHandlerForBoundaryTest(
            registry,
            registered.Value!,
            replacementHandler);
        return (registry, request);
    }

    private static void ReplaceRegisteredRouteHandlerForBoundaryTest(
        DesignConversionModelServerEndpointRegistry registry,
        DesignConversionModelServerEndpointRegistration registration,
        Func<DesignConversionModelServerHttpRequest, CancellationToken,
            Task<DesignConversionModelServerHttpResponse>> replacementHandler)
    {
        var routesField = typeof(DesignConversionModelServerEndpointRegistry).GetField(
            "_routes",
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(routesField);
        var routes = Assert.IsType<Dictionary<string, DesignConversionModelServerEndpointRegistration>>(
            routesField.GetValue(registry));
        var routeKey = routes.Single(pair => ReferenceEquals(pair.Value, registration)).Key;
        routes[routeKey] = registration with { Handler = replacementHandler };
    }

    private DesignConversionModelServerApi CreateApi(IDesignConversionLocalArtifactDb? artifactDb = null)
    {
        var service = new DesignConversionInternalService(
            new RecordingRenderProvider(),
            new RecordingComparisonProvider(CreateVerdict("pass117-fake-verdict", 0.90, passed: true)),
            routingFlow: null);
        var utcNow = () => DateTimeOffset.Parse("2026-07-02T10:00:00Z");
        if (artifactDb is null)
            return new DesignConversionModelServerApi(
                service,
                utcNow,
                postEvaluationStateStore: TestPostEvaluationStateStore);

        return new DesignConversionModelServerApi(
            new DesignConversionInternalServiceThinClientJobEvaluator(service),
            utcNow,
            artifactDb,
            postEvaluationStateStore: TestPostEvaluationStateStore);
    }

    private static IDesignConversionImprovementCandidateStrategy
        ReadComposedCandidateStrategy(DesignConversionModelServerApi api)
    {
        var captureField = typeof(DesignConversionModelServerApi).GetField(
            "_capture",
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(captureField);
        var capture = captureField.GetValue(api);
        Assert.NotNull(capture);
        var captureInner = capture.GetType().GetField(
            "_inner",
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(captureInner);
        var loopEvaluator = Assert.IsType<
            DesignConversionImprovementLoopThinClientJobEvaluator>(
                captureInner.GetValue(capture));
        var strategyField = typeof(DesignConversionImprovementLoopThinClientJobEvaluator)
            .GetField(
                "_candidateStrategy",
                BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(strategyField);
        return Assert.IsAssignableFrom<IDesignConversionImprovementCandidateStrategy>(
            strategyField.GetValue(loopEvaluator));
    }

    private static IDesignConversionImprovementCandidateStrategy
        ReadWrappedCandidateStrategy(
            DesignConversionDpoTeacherAuthoringCandidateStrategy strategy)
    {
        var field = typeof(DesignConversionDpoTeacherAuthoringCandidateStrategy)
            .GetField("_inner", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(field);
        return Assert.IsAssignableFrom<IDesignConversionImprovementCandidateStrategy>(
            field.GetValue(strategy));
    }

    public static IEnumerable<object[]> PersistenceReadbackTamperCases()
    {
        yield return [PersistenceReadbackTamperMode.EmptyReadback, "fresh_readback_empty"];
        yield return [PersistenceReadbackTamperMode.WrongJobId, "server_owned_record_count_not_one"];
        yield return [PersistenceReadbackTamperMode.WrongRequestId, "server_owned_record_count_not_one"];
        yield return [PersistenceReadbackTamperMode.WrongRecordSource, "server_owned_record_count_not_one"];
        yield return [PersistenceReadbackTamperMode.MissingRefsOnlyFlags, "record_refs_only_flags_missing"];
        yield return [PersistenceReadbackTamperMode.RawHtmlRef, "record_contains_raw_or_local_ref"];
        yield return [PersistenceReadbackTamperMode.LocalRenderPath, "record_contains_raw_or_local_ref"];
        yield return [PersistenceReadbackTamperMode.InvalidCompletedStatusSnapshot, "record_contains_raw_or_local_ref"];
        yield return [PersistenceReadbackTamperMode.MismatchedConversionResult, "conversion_result_not_bound"];
        yield return [PersistenceReadbackTamperMode.MissingRenderRef, "render_refs_not_bound"];
        yield return [PersistenceReadbackTamperMode.MissingIndexMarker, "record_index_not_consistent"];
        yield return [PersistenceReadbackTamperMode.ReadbackFailure, "fresh_readback_failed_fake_readback"];
        yield return [PersistenceReadbackTamperMode.ReadbackException, "fresh_readback_exception_invalidoperationexception"];
    }

    private static void AssertSubmitPathNonReadyVisualDecision(
        DesignConversionModelServerApiResult result,
        string expectedVisualDecision,
        string expectedStopReason)
    {
        Assert.Equal(DesignConversionThinClientStatus.NotReady, result.ContractStatus);
        Assert.Equal(DesignConversionThinClientStatus.NotReady, result.RuntimeStatus.ContractStatus);
        Assert.Equal(DesignConversionThinClientStatus.NotReady, result.RuntimeStatus.Job.Status);
        Assert.False(result.ProductReady);
        Assert.False(result.TrainingReadiness.ProductReady);
        Assert.Equal(expectedVisualDecision, result.RuntimeStatus.Provenance["final_visual_decision"]);
        Assert.Equal(expectedStopReason, result.RuntimeStatus.Provenance["tier_policy_stop_reason"]);
        Assert.Equal(
            DesignConversionThinClientStatus.NotReady,
            result.RuntimeStatus.Provenance["thin_client_terminal_status"]);
        Assert.Contains($"final_visual_decision={expectedVisualDecision}", result.RuntimeStatus.Warnings);
        Assert.Contains($"tier_policy_stop_reason={expectedStopReason}", result.RuntimeStatus.Warnings);
        Assert.Contains(
            $"thin_client_terminal_status={DesignConversionThinClientStatus.NotReady}",
            result.RuntimeStatus.Warnings);
        Assert.True(result.RuntimeStatus.QualityMetrics.Passed.HasValue);
        Assert.False(result.RuntimeStatus.QualityMetrics.Passed.Value);
    }

    private static void AssertPersistenceVerified(
        IReadOnlyDictionary<string, string> metadata,
        string jobId,
        string requestId)
    {
        Assert.True(
            string.Equals(bool.TrueString, metadata["persistence_verified"], StringComparison.Ordinal),
            $"persistence_failure_reason={metadata["persistence_failure_reason"]}; " +
            $"query_job_id={metadata["persistence_fresh_readback_query_job_id"]}; " +
            $"query_request_id={metadata["persistence_fresh_readback_query_request_id"]}");
        Assert.Equal(
            "server_owned_local_artifact_db_append_fresh_readback",
            metadata["persistence_verification_authority"]);
        Assert.Equal(
            "verified_by_server_append_and_fresh_job_request_readback",
            metadata["persistence_verification_state"]);
        Assert.Equal("none", metadata["persistence_failure_reason"]);
        Assert.Equal(bool.TrueString, metadata["persistence_server_append_reported"]);
        Assert.Equal(bool.TrueString, metadata["persistence_fresh_readback_success"]);
        Assert.Equal(jobId, metadata["persistence_fresh_readback_query_job_id"]);
        Assert.Equal(requestId, metadata["persistence_fresh_readback_query_request_id"]);
        Assert.Equal("1", metadata["persistence_fresh_readback_record_count"]);
        Assert.Equal("1", metadata["persistence_server_owned_record_count"]);
        Assert.Equal(nameof(DesignConversionThinClientJobRuntime), metadata["persistence_record_source"]);
        Assert.Equal(bool.TrueString, metadata["persistence_record_job_bound"]);
        Assert.Equal(bool.TrueString, metadata["persistence_record_request_bound"]);
        Assert.Equal(bool.TrueString, metadata["persistence_record_refs_only"]);
        Assert.Equal(bool.TrueString, metadata["persistence_raw_content_free"]);
        Assert.Equal(bool.TrueString, metadata["persistence_no_local_path_public"]);
        Assert.Equal(bool.TrueString, metadata["persistence_conversion_result_bound"]);
        Assert.Equal(bool.TrueString, metadata["persistence_record_index_consistent"]);
        Assert.Equal(metadata["persistence_expected_render_ref_count"], metadata["persistence_record_render_ref_count"]);
        Assert.Equal(metadata["persistence_expected_render_ref_count"], metadata["persistence_bound_render_ref_count"]);
        Assert.NotEqual("0", metadata["persistence_expected_render_ref_count"]);
        Assert.False(string.IsNullOrWhiteSpace(metadata["persistence_record_id"]));
        Assert.Equal(bool.FalseString, metadata["persistence_caller_claim_accepted"]);
        Assert.Equal(bool.TrueString, metadata["persistence_status_spoof_rejected"]);
        Assert.Equal(bool.TrueString, metadata["persistence_public_refs_only"]);
    }

    private static void AssertPersistenceNotVerified(
        IReadOnlyDictionary<string, string> metadata,
        string expectedReason)
    {
        Assert.Equal(bool.FalseString, metadata["persistence_verified"]);
        Assert.Equal(
            "server_owned_local_artifact_db_append_fresh_readback",
            metadata["persistence_verification_authority"]);
        Assert.Equal(
            "not_verified_by_server_append_and_fresh_job_request_readback",
            metadata["persistence_verification_state"]);
        Assert.Equal(expectedReason, metadata["persistence_failure_reason"]);
        Assert.Equal(bool.FalseString, metadata["persistence_caller_claim_accepted"]);
        Assert.Equal(bool.TrueString, metadata["persistence_status_spoof_rejected"]);
        Assert.Equal(bool.TrueString, metadata["persistence_public_refs_only"]);
        Assert.Equal(bool.FalseString, metadata["product_ready"]);
        Assert.Equal(bool.FalseString, metadata["deployment_verified"]);
        Assert.Equal(bool.FalseString, metadata["model_training_verified"]);
        Assert.Equal(bool.FalseString, metadata["model_upload_ready"]);
        Assert.Equal(bool.FalseString, metadata["ModelUploadReady"]);
    }

    private static void AssertReadbackExceptionGraphEscaped(
        Exception? exception,
        PersistenceReadbackTamperMode mode)
    {
        Assert.NotNull(exception);
        if (mode == PersistenceReadbackTamperMode.ReadbackForeignCancellation)
        {
            var cancellation = Assert.IsType<OperationCanceledException>(exception);
            Assert.True(cancellation.CancellationToken.IsCancellationRequested);
            return;
        }

        var aggregate = Assert.IsType<AggregateException>(exception);
        Assert.IsType<OutOfMemoryException>(Assert.Single(aggregate.InnerExceptions));
    }

    private static void AssertNoPersistencePublicLeak(
        string publicJson,
        string privateMarker,
        string rawHtml,
        string rawCss,
        string artifactDbRoot)
    {
        Assert.DoesNotContain(privateMarker, publicJson, StringComparison.Ordinal);
        Assert.DoesNotContain(rawHtml, publicJson, StringComparison.Ordinal);
        Assert.DoesNotContain(rawCss, publicJson, StringComparison.Ordinal);
        Assert.DoesNotContain(Path.GetFullPath(artifactDbRoot), publicJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("file:", publicJson, StringComparison.OrdinalIgnoreCase);
    }

    private static string CreateArtifactDbTempRoot(string scope)
        => Path.Combine(
            Path.GetTempPath(),
            "design-conversion-modelserver-api-persistence-tests",
            SafePathSegment(scope),
            Guid.NewGuid().ToString("N"));

    // Текст платного эталона подаётся в договор ТОЛЬКО хешем — ниже, в
    // личности запроса; в построение сторон обучающей пары он не передаётся
    // вообще, поэтому непопадание текста в тела сторон эта оснастка и не
    // проверяет: проверять нечему было бы упасть.
    // Непопадание обеспечивает продукт, а не оснастка: правило сверки
    // эталонных предпочтений в файле
    // 14.ContinuousLearning/Core/DpoPairReferencePreferenceMatchingRule.cs
    // отклоняет пару состояниями paid_reference_copied_to_chosen_side и
    // paid_reference_copied_to_rejected_side, сравнивая стороны пары С ХЕШЕМ
    // эталонного ответа, а не с его текстом; объявленный признак
    // PaidReferenceOutputNeverDpoPairSide лежит в файле
    // 14.ContinuousLearning/Core/FrozenReferenceCorpus.cs.
    private const string RepositoryOwnedDpoPaidReferenceRawText =
        "repository-owned-dpo-paid-reference-answer";

    // Запечатывает НАСТОЯЩИЙ набор обучающих пар по адресу и личности того же
    // договора, который присылает клиент. Без этого репозиторный маршрут
    // никогда не разрешается, и ветка переиспользования недостижима.
    // Возвращает хеш описи запечатанного набора: ровно это значение продукт
    // обязан выдать в доказательствах завершения, и проверка закрепляет его
    // против запечатанного набора, а не против вписанного руками литерала.
    private static string SealRepositoryOwnedDpoDatasetFor(
        DesignConversionRepositoryOwnedDpoTransactionContext context)
    {
        var identity = context.RequestIdentity!;
        // Паспорт учителя берём из личности договора, иначе запечатанный набор
        // не совпадёт с тем, что ищет разрешение набора.
        var passport = new TrainingTeacherProvenance(
            TrainingTeacherSourceKind.ExternalTeacherOutput,
            OpenWeightTeacherFamily.Qwen,
            TrainingTeacherAccessMode.LocalHostedOpenWeight,
            TrainingTeacherHostingProvider.Ollama,
            identity.OpenTeacher.ModelId,
            identity.OpenTeacher.ModelVersion,
            TrainingTeacherModelLicense.Apache20,
            ProviderTermsChecked: false,
            ProviderTermsAllowTraining: false,
            ClosedProprietaryModel: false,
            TeacherOutputUsedForTraining: true,
            "repository-owned-dpo-submit-path-reuse",
            InvocationCount: 1)
        {
            ModelArtifactDigestKind =
                identity.OpenTeacher.ModelArtifactDigestKind,
            ModelArtifactDigest = identity.OpenTeacher.ModelArtifactSha256
        };
        // Пара: сторона учителя и сторона локальной модели, с принятой
        // лицензией и без карантина.
        // Тему пары берём из адреса договора, а не вписываем словом: страж
        // печатника сверяет тему пары с темой адреса точным сравнением, и
        // вписанное слово молча расходится с адресом при любой его правке —
        // ровно так эта оснастка и падала на «набор обучающих пар не принят».
        var bound = DpoPairTrainingContentAuthority.Bind(new ContinuousLearning.DpoPair(
            "build a responsive card for submit path reuse",
            "open teacher candidate for submit path reuse",
            "local model candidate for submit path reuse",
            context.Address!.DomainSlug,
            DateTimeOffset.UnixEpoch)
        {
            TeacherProvenance = passport,
            GovernanceDecision = new TrainingDataGovernanceDecision(
                licenseAccepted: true,
                isQuarantined: false,
                evidenceRefs:
                [
                    "sha256:" + Sha256Text("governance submit path reuse")
                ]),
            ChosenAuthorship = new DpoPairSideAuthorship(
                DPO.Models.DpoPairSideAuthorKind.OpenTeacher,
                passport),
            RejectedAuthorship = new DpoPairSideAuthorship(
                DPO.Models.DpoPairSideAuthorKind.LocalModel)
        });
        Assert.True(bound.IsSuccess, bound.ErrorMessage);
        var @sealed = RepositoryOwnedDpoDatasetManifestSealer.PersistAndSeal(
            context.Address!,
            identity,
            // Восклицательный знак нужен потому, что успешность привязки
            // доказана утверждением выше, о котором компилятор не знает.
            [bound.Value!]);
        Assert.True(@sealed.IsSuccess, @sealed.ErrorMessage);
        // Отдаём хеш описи наружу: ожидание в проверке должно брать его из
        // того самого набора, который только что лёг на диск.
        return @sealed.Value!.ManifestSha256;
    }

    private static string CreateRepositoryOwnedDpoContextTempRoot(string scope)
    {
        var repositoryRoot = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-modelserver-api-repository-owned-dpo-context",
            SafePathSegment(scope),
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(repositoryRoot);
        File.WriteAllText(
            Path.Combine(repositoryRoot, "LlmFromScratch.sln"),
            "repository-owned DPO context test sentinel");
        File.WriteAllText(
            Path.Combine(repositoryRoot, "llm_mvp_core.sln"),
            "repository-owned DPO context test sentinel");
        File.WriteAllText(
            Path.Combine(repositoryRoot, "AGENTS.md"),
            "repository-owned DPO context test sentinel");
        Directory.CreateDirectory(
            Path.Combine(repositoryRoot, "14.ContinuousLearning"));
        Directory.CreateDirectory(Path.Combine(repositoryRoot, "11.DPO"));
        return repositoryRoot;
    }

    private static DesignConversionRepositoryOwnedDpoTransactionContext
        CreateValidRepositoryOwnedDpoTransactionContext(string repositoryRoot)
    {
        var address = RepositoryOwnedTrainingArtifactLayout
            .CreateDpoDatasetAddress(
                repositoryRoot,
                "html-css",
                "learning-unit",
                "v0001");
        Assert.True(address.IsSuccess, address.ErrorMessage);
        var identity = RepositoryOwnedDpoDatasetManifestSealer
            .CreateRequestIdentity(
                // Хеш платного эталона считаем от единственного источника
                // текста — постоянной выше, чтобы дубля литерала не осталось.
                [Sha256Text(RepositoryOwnedDpoPaidReferenceRawText)],
                new RepositoryOwnedDpoOpenTeacherIdentity(
                    OpenWeightTeacherFamily.Qwen.ToString(),
                    "qwen3.5:9b",
                    MaxTeacherCallsExpectedTeacherManifestDigest,
                    LegalTrainingTeacherAdmissionAuthority
                        .OllamaModelLayerSha256DigestKind,
                    MaxTeacherCallsExpectedTeacherModelLayerDigest),
                new RepositoryOwnedDpoGenerationPolicyIdentity(
                    "design-conversion-submit-path-context-test",
                    "v1",
                    new string('2', 64)),
                new RepositoryOwnedDpoCreatorIdentity(
                    "design-conversion-submit-path-context-test",
                    "v1",
                    new string('3', 64)));
        Assert.True(identity.IsSuccess, identity.ErrorMessage);
        return new DesignConversionRepositoryOwnedDpoTransactionContext(
            Enabled: true,
            Address: address.Value!,
            RequestIdentity: identity.Value!);
    }

    private static string Sha256Text(string value)
        => Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(value)))
            .ToLowerInvariant();

    private static string SafePathSegment(string value)
    {
        var builder = new StringBuilder(value.Length);
        foreach (var ch in value)
            builder.Append(char.IsLetterOrDigit(ch) || ch is '-' or '_' ? ch : '-');

        var segment = builder.ToString().Trim('-');
        return string.IsNullOrWhiteSpace(segment) ? "persistence" : segment;
    }

    private const string PromotionHostDpoProviderTenantId = "tenant-dpo-provider-alpha";
    private const string PromotionHostDpoProviderBearerToken = "promotion-host-dpo-provider-token-123456";
    private const string PromotionHostDpoProviderRepositoryServerBaseUrl =
        "https://models.example/design-conversion/releases/stable";
    private static readonly Uri PromotionHostDpoProviderUploadEndpoint =
        new("https://promotion.example/v1/design-conversion/promotions");

    private static DesignConversionModelServerPromotionHostOptions CreatePromotionHostDpoProviderOptions(
        PromotionHostArtifactDbMode mode,
        string artifactRoot,
        IDesignConversionDpoPolicyBaseProvider provider,
        HttpClient promotionHttpClient)
        => mode switch
        {
            PromotionHostArtifactDbMode.LocalNoRemote => new(
                artifactRoot,
                DpoPolicyBaseProvider: provider),
            PromotionHostArtifactDbMode.RepositoryPromotion => new(
                artifactRoot,
                RepositoryPromotionOptions: new DesignConversionModelServerRepositoryPromotionOptions(
                    new PromotionHostDpoProviderNoNetworkModelRepository(),
                    new PromotionHostDpoProviderNoNetworkRemoteUnitLocator(),
                    PromotionHostDpoProviderRepositoryServerBaseUrl,
                    PromotionHostDpoProviderTenantId),
                DpoPolicyBaseProvider: provider),
            PromotionHostArtifactDbMode.HttpPromotion => new(
                artifactRoot,
                promotionHttpClient,
                CreatePromotionHostDpoProviderHttpUploaderOptions(artifactRoot),
                DpoPolicyBaseProvider: provider),
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unknown promotion host mode.")
        };

    private static ServerOwnedLegalTrainingTeacherAdmissionAuthority CreateServerOwnedQwenOllamaAuthority(
        string modelId,
        string modelVersion,
        int maxTeacherCalls)
        => DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(
            maxTeacherCalls,
            modelId,
            modelVersion,
            LegalTeacherModelLayerDigest);

    private static TrainingTeacherProvenance CreateQwenOllamaTeacherProvenance(
        string requestId,
        string modelId,
        string modelVersion,
        int invocationCount = 1)
        => new(
            TrainingTeacherSourceKind.ExternalTeacherOutput,
            OpenWeightTeacherFamily.Qwen,
            TrainingTeacherAccessMode.LocalHostedOpenWeight,
            TrainingTeacherHostingProvider.Ollama,
            modelId,
            modelVersion,
            TrainingTeacherModelLicense.Apache20,
            ProviderTermsChecked: false,
            ProviderTermsAllowTraining: false,
            ClosedProprietaryModel: false,
            TeacherOutputUsedForTraining: true,
            requestId,
            invocationCount)
        {
            ModelArtifactDigestKind =
                LegalTrainingTeacherAdmissionAuthority.OllamaModelLayerSha256DigestKind,
            ModelArtifactDigest = LegalTeacherModelLayerDigest
        };

    private static DesignConversionModelServerPromotionHttpUploaderOptions
        CreatePromotionHostDpoProviderHttpUploaderOptions(string artifactRoot)
        => new(
            PromotionHostDpoProviderUploadEndpoint,
            PromotionHostDpoProviderTenantId,
            PromotionHostDpoProviderBearerToken,
            artifactRoot)
        {
            Timeout = TimeSpan.FromSeconds(5),
            MaxArtifactBytes = 1024 * 1024,
            MaxTotalArtifactBytes = 3 * 1024 * 1024,
            MaxResponseBodyBytes = 64 * 1024,
            MaxArtifactCount = DesignConversionModelServerPromotionHttpUploaderOptions.HardMaxArtifactCount
        };

    private static object InvokePromotionHostArtifactDbSetup(
        DesignConversionModelServerPromotionHostOptions options,
        ServerOwnedLegalTrainingTeacherAdmissionAuthority?
            legalTeacherAdmissionAuthority = null)
    {
        var method = typeof(DesignConversionModelServerAspNetHostStartup).GetMethod(
            "CreatePromotionHostArtifactDbSetup",
            BindingFlags.Static | BindingFlags.NonPublic);
        Assert.NotNull(method);
        var setup = method.Invoke(
            null,
            [options, null, legalTeacherAdmissionAuthority]);
        Assert.NotNull(setup);
        return setup!;
    }

    private static JsonlDesignConversionLocalArtifactDb ReadPromotionHostSetupArtifactDb(object setup)
        => Assert.IsType<JsonlDesignConversionLocalArtifactDb>(
            ReadPromotionHostSetupProperty(setup, "ArtifactDb"));

    private static bool ReadPromotionHostSetupRemoteConfigured(object setup)
        => Assert.IsType<bool>(
            ReadPromotionHostSetupProperty(setup, "RemotePromotionConfigured"));

    private static string? ReadPromotionHostSetupExpectedTenantId(object setup)
        => (string?)ReadPromotionHostSetupProperty(setup, "ExpectedTenantId");

    private static object? ReadPromotionHostSetupActivatedModelCheckpointLoader(object setup)
        => ReadPromotionHostSetupProperty(setup, "ActivatedModelCheckpointLoader");

    private static object? ReadPromotionHostSetupProperty(object setup, string propertyName)
    {
        var property = setup.GetType().GetProperty(
            propertyName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        Assert.NotNull(property);
        return property.GetValue(setup);
    }

    private static IDesignConversionDpoPolicyBaseProvider? ReadDpoPolicyBaseProvider(
        JsonlDesignConversionLocalArtifactDb artifactDb)
    {
        var providerField = Assert.Single(
            typeof(JsonlDesignConversionLocalArtifactDb).GetFields(
                BindingFlags.Instance | BindingFlags.NonPublic),
            field => field.FieldType == typeof(IDesignConversionDpoPolicyBaseProvider));
        return (IDesignConversionDpoPolicyBaseProvider?)providerField.GetValue(artifactDb);
    }

    private static ServerOwnedLegalTrainingTeacherAdmissionAuthority?
        ReadLegalTeacherAdmissionAuthority(
        JsonlDesignConversionLocalArtifactDb artifactDb)
    {
        var authorityField = Assert.Single(
            typeof(JsonlDesignConversionLocalArtifactDb).GetFields(
                BindingFlags.Instance | BindingFlags.NonPublic),
            field => field.FieldType ==
                typeof(ServerOwnedLegalTrainingTeacherAdmissionAuthority));
        return (ServerOwnedLegalTrainingTeacherAdmissionAuthority?)
            authorityField.GetValue(artifactDb);
    }

    public enum PromotionHostArtifactDbMode
    {
        LocalNoRemote,
        RepositoryPromotion,
        HttpPromotion
    }

    private sealed class PromotionHostDpoProviderNoNetworkModelRepository : IModelRepository
    {
        public OperationResult<string> PreflightStableConfigurationIdentitySha256()
            => OperationResult<string>.Ok("sha256:" + new string('a', 64));

        public Task<OperationResult<Stream>> LoadWeightsAsync(
            string unitId,
            string? serverBaseUrl,
            string localFallbackPath,
            CancellationToken ct = default)
            => throw new InvalidOperationException(
                "Promotion host DPO provider forwarding composition must not load repository weights.");

        public Task<OperationResult<RemoteUnitManifest>> UploadAsync(
            GeneratedCapabilityCheckpointManifest manifest,
            string serverBaseUrl,
            CancellationToken ct = default)
            => throw new InvalidOperationException(
                "Promotion host DPO provider forwarding composition must not upload repository artifacts.");
    }

    private sealed class PromotionHostDpoProviderNoNetworkRemoteUnitLocator : IRemoteUnitLocator
    {
        public OperationResult<string> PreflightStableConfigurationIdentitySha256()
            => OperationResult<string>.Ok("sha256:" + new string('b', 64));

        public Task<OperationResult<RemoteUnitManifest>> LocateAsync(
            string unitId,
            CancellationToken ct = default)
            => throw new InvalidOperationException(
                "Promotion host DPO provider forwarding composition must not locate remote units.");

        public Task<OperationResult<bool>> RegisterAsync(
            RemoteUnitManifest manifest,
            CancellationToken ct = default)
            => throw new InvalidOperationException(
                "Promotion host DPO provider forwarding composition must not register remote units.");
    }

    private sealed class PromotionHostDpoProviderNoNetworkHttpHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
            => throw new InvalidOperationException(
                "Promotion host DPO provider forwarding composition must not send HTTP requests.");
    }

    private static DesignConversionLocalArtifactDbRecord TamperPersistenceRecord(
        DesignConversionLocalArtifactDbRecord record,
        PersistenceReadbackTamperMode mode)
    {
        return mode switch
        {
            PersistenceReadbackTamperMode.WrongJobId => record with
            {
                JobId = $"{record.JobId}-other"
            },
            PersistenceReadbackTamperMode.WrongRequestId => record with
            {
                RequestId = $"{record.RequestId}-other"
            },
            PersistenceReadbackTamperMode.WrongRecordSource => record with
            {
                Metadata = WithMetadata(record.Metadata, "artifact_db_record_source", "caller_spoof")
            },
            PersistenceReadbackTamperMode.MissingRefsOnlyFlags => record with
            {
                ConversionResult = record.ConversionResult with
                {
                    Metadata = WithMetadata(record.ConversionResult.Metadata, "raw_html_included", bool.TrueString)
                }
            },
            PersistenceReadbackTamperMode.RawHtmlRef => record with
            {
                ConversionResult = record.ConversionResult with
                {
                    HtmlRef = "<main data-secret=\"PRIVATE_TAMPERED_HTML\">Raw</main>"
                }
            },
            PersistenceReadbackTamperMode.LocalRenderPath => record with
            {
                RenderArtifactRefs = record.RenderArtifactRefs
                    .Select((render, index) => index == 0
                        ? render with { UriRef = @"C:\private\render-output.png" }
                        : render)
                    .ToArray()
            },
            PersistenceReadbackTamperMode.InvalidCompletedStatusSnapshot => record with
            {
                VisualJudgeStatus = record.VisualJudgeStatus with
                {
                    Metadata = WithMetadata(
                        record.VisualJudgeStatus.Metadata,
                        DesignConversionLocalArtifactDbRecord.CompletedStatusSnapshotMetadataKey,
                        "{\"schema\":\"forged\",\"version\":1,\"PRIVATE\":\"SECRET\"}")
                }
            },
            PersistenceReadbackTamperMode.MismatchedConversionResult => record with
            {
                ConversionResult = record.ConversionResult with
                {
                    ResultId = $"{record.ConversionResult.ResultId}-other"
                }
            },
            PersistenceReadbackTamperMode.MissingRenderRef => record with
            {
                RenderArtifactRefs = []
            },
            PersistenceReadbackTamperMode.MissingIndexMarker => record with
            {
                EvidenceRefs = record.EvidenceRefs
                    .Where(reference => !string.Equals(
                        reference,
                        JsonlDesignConversionLocalArtifactDb.LocalArtifactDbKind,
                        StringComparison.Ordinal))
                    .ToArray(),
                Metadata = WithMetadata(record.Metadata, "training_database_record_count", "0")
            },
            PersistenceReadbackTamperMode.MismatchedDpoCandidateIdentity => record with
            {
                DpoCandidates = record.DpoCandidates
                    .Select((candidate, index) => index == 0
                        ? candidate with { CandidateId = $"{candidate.CandidateId}-other" }
                        : candidate)
                    .ToArray()
            },
            _ => record
        };
    }

    private static IReadOnlyDictionary<string, string> WithMetadata(
        IReadOnlyDictionary<string, string> source,
        string key,
        string value)
    {
        var metadata = new Dictionary<string, string>(source, StringComparer.Ordinal)
        {
            [key] = value
        };
        return metadata;
    }

    public enum PersistenceReadbackTamperMode
    {
        None,
        AppendFailure,
        AppendException,
        AppendCancellation,
        EmptyReadback,
        WrongJobId,
        WrongRequestId,
        WrongRecordSource,
        MissingRefsOnlyFlags,
        RawHtmlRef,
        LocalRenderPath,
        InvalidCompletedStatusSnapshot,
        MismatchedConversionResult,
        MissingRenderRef,
        MissingIndexMarker,
        ReadbackFailure,
        ReadbackException,
        ReadbackForeignCancellation,
        ReadbackFatalGraph,
        MismatchedDpoCandidateIdentity
    }

    private sealed class TamperingLocalArtifactDb :
        IDesignConversionLocalArtifactDb,
        IDesignConversionPostEvaluationStateRootProvider
    {
        private readonly PersistenceReadbackTamperMode _mode;
        private readonly JsonlDesignConversionLocalArtifactDb _inner;

        public TamperingLocalArtifactDb(
            PersistenceReadbackTamperMode mode,
            string? privateExceptionMarker = null)
        {
            _mode = mode;
            PrivateExceptionMarker = privateExceptionMarker;
            RootDirectory = CreateArtifactDbTempRoot($"pt-{(int)mode}");
            _inner = new JsonlDesignConversionLocalArtifactDb(RootDirectory);
        }

        public string RootDirectory { get; }
        public string PostEvaluationStateRootDirectory => Path.Combine(
            RootDirectory,
            JsonDesignConversionPostEvaluationStateStore.DirectoryName);
        public int AppendCalls { get; private set; }
        public int ListCalls { get; private set; }
        public DesignConversionLocalArtifactDbQuery? LastQuery { get; private set; }
        private string? PrivateExceptionMarker { get; }

        public Task<OperationResult<DesignConversionLocalArtifactDbRecord>> AppendRecordAsync(
            DesignConversionLocalArtifactDbRecord record,
            CancellationToken ct = default)
        {
            AppendCalls++;
            if (_mode == PersistenceReadbackTamperMode.AppendFailure)
            {
                return Task.FromResult(OperationResult<DesignConversionLocalArtifactDbRecord>.Fail(
                    "Scripted append failure.",
                    "SCRIPTED_PERSISTENCE_FAILURE"));
            }
            if (_mode == PersistenceReadbackTamperMode.AppendException)
                throw new InvalidOperationException("Fake append exception.");
            if (_mode == PersistenceReadbackTamperMode.AppendCancellation)
            {
                return Task.FromResult(OperationResult<DesignConversionLocalArtifactDbRecord>.Fail(
                    "Scripted append cancellation.",
                    "CANCELLED"));
            }

            return _inner.AppendRecordAsync(record, ct);
        }

        public async Task<OperationResult<IReadOnlyList<DesignConversionLocalArtifactDbRecord>>> ListRecordsAsync(
            DesignConversionLocalArtifactDbQuery? query = null,
            CancellationToken ct = default)
        {
            ListCalls++;
            LastQuery = query;

            if (_mode == PersistenceReadbackTamperMode.ReadbackException)
                throw new InvalidOperationException(
                    string.IsNullOrWhiteSpace(PrivateExceptionMarker)
                        ? "Fake readback exception."
                        : $"Fake readback exception {PrivateExceptionMarker}.");

            if (_mode == PersistenceReadbackTamperMode.ReadbackForeignCancellation)
                throw new OperationCanceledException(
                    "Fake foreign readback cancellation.",
                    new CancellationToken(canceled: true));

            if (_mode == PersistenceReadbackTamperMode.ReadbackFatalGraph)
                throw new AggregateException(
                    "Fake fatal readback exception graph.",
                    new OutOfMemoryException("Fake nested out-of-memory node."));

            if (_mode == PersistenceReadbackTamperMode.ReadbackFailure)
            {
                return OperationResult<IReadOnlyList<DesignConversionLocalArtifactDbRecord>>.Fail(
                    "Fake readback failure.",
                    "FAKE_READBACK");
            }

            var records = await _inner.ListRecordsAsync(query, ct);
            if (!records.IsSuccess)
                return records;

            if (_mode == PersistenceReadbackTamperMode.EmptyReadback)
                return OperationResult<IReadOnlyList<DesignConversionLocalArtifactDbRecord>>.Ok([]);

            return OperationResult<IReadOnlyList<DesignConversionLocalArtifactDbRecord>>.Ok(
                records.Value!
                    .Select(record => TamperPersistenceRecord(record, _mode))
                    .ToArray());
        }

        public Task<OperationResult<IReadOnlyList<DesignConversionDpoTrainingRecord>>> QueryApprovedTrainingCandidatesAsync(
            DesignConversionLocalArtifactDbQuery? query = null,
            CancellationToken ct = default)
            => _inner.QueryApprovedTrainingCandidatesAsync(query, ct);

        public Task<OperationResult<DesignConversionApprovedDpoHandoffArtifact>> ExportApprovedRuntimeDpoCandidatesAsync(
            DesignConversionLocalArtifactDbQuery? query = null,
            DateTimeOffset? exportedAtUtc = null,
            CancellationToken ct = default)
            => _inner.ExportApprovedRuntimeDpoCandidatesAsync(query, exportedAtUtc, ct);

        public Task<OperationResult<DesignConversionTrainingIntakeRecord>> CreateTrainingIntakeAsync(
            DesignConversionApprovedDpoHandoffArtifact handoff,
            string requestedByRef,
            string reason,
            DateTimeOffset? requestedAtUtc = null,
            CancellationToken ct = default,
            IReadOnlyDictionary<string, string>? admissionBoundaryMetadata = null)
            => _inner.CreateTrainingIntakeAsync(
                handoff,
                requestedByRef,
                reason,
                requestedAtUtc,
                ct,
                admissionBoundaryMetadata);

        public Task<OperationResult<IReadOnlyList<DesignConversionTrainingIntakeRecord>>> ListTrainingIntakeRequestsAsync(
            CancellationToken ct = default)
            => _inner.ListTrainingIntakeRequestsAsync(ct);

        public Task<OperationResult<DesignConversionTrainingIntakePreflightResult>> RunTrainingIntakePreflightAsync(
            string intakeId,
            CancellationToken ct = default)
            => _inner.RunTrainingIntakePreflightAsync(intakeId, ct);

        public Task<OperationResult<DesignConversionTrainingIntakeRunResult>> RunTrainingIntakeAsync(
            string intakeId,
            DesignConversionTrainingPipelineOptions? options = null,
            CancellationToken ct = default,
            IReadOnlyDictionary<string, string>? admissionBoundaryMetadata = null)
            => _inner.RunTrainingIntakeAsync(intakeId, options, ct, admissionBoundaryMetadata);

        public Task<OperationResult<DesignConversionTrainingIntakeLocalPromotionResult>> RunTrainingIntakeAndPromoteLocalAsync(
            string intakeId,
            string promotionOutputDirectory,
            DesignConversionTrainingPipelineOptions? options = null,
            CancellationToken ct = default)
            => _inner.RunTrainingIntakeAndPromoteLocalAsync(intakeId, promotionOutputDirectory, options, ct);

        public Task<OperationResult<DesignConversionTrainingIntakeLocalPromotionRollbackResult>> RollbackTrainingIntakeLocalPromotionAsync(
            string promotedLocalModelRef,
            string requestedByRef,
            string reason,
            DateTimeOffset? requestedAtUtc = null,
            CancellationToken ct = default)
            => _inner.RollbackTrainingIntakeLocalPromotionAsync(
                promotedLocalModelRef,
                requestedByRef,
                reason,
                requestedAtUtc,
                ct);

        public Task<OperationResult<DesignConversionDpoCandidateApprovalResult>> ApproveRuntimeCreatedDpoCandidateAsync(
            string localArtifactRecordId,
            string candidateOrTrainingRecordId,
            string reviewerRef,
            string reason,
            DateTimeOffset? approvedAtUtc = null,
            CancellationToken ct = default)
            => _inner.ApproveRuntimeCreatedDpoCandidateAsync(
                localArtifactRecordId,
                candidateOrTrainingRecordId,
                reviewerRef,
                reason,
                approvedAtUtc,
                ct);

        public Task<OperationResult<DesignConversionDpoDisplayResult>> QueryDisplayRowsAsync(
            DesignConversionDpoDisplayQuery? query = null,
            CancellationToken ct = default)
            => _inner.QueryDisplayRowsAsync(query, ct);
    }

    private static async Task<ServerOwnedArtifactFixture> CreateServerOwnedPngArtifactAsync(
        string artifactRoot,
        string artifactId,
        int width = 1440,
        int height = 900)
    {
        var renderer = new DeterministicLocalDesignConversionPngRenderProvider(artifactRoot);
        var resolution = new RenderQaResolution($"server-owned-{width}x{height}", width, height, 1d);
        var render = await renderer.RenderAsync(new DesignConversionRenderRequest(
            RenderRequestId: $"{artifactId}-render",
            RequestId: $"{artifactId}-request",
            Html: "<main class=\"server-owned-proof\"><h1>Server owned proof</h1></main>",
            Css: ".server-owned-proof{display:grid;gap:16px;color:#111827;background:#f9fafb;}",
            Resolution: resolution,
            Metadata: new Dictionary<string, string>
            {
                ["server_owned_artifact_test"] = bool.TrueString
            }));
        Assert.True(render.IsSuccess, render.ErrorMessage);

        var image = render.Value!.Image;
        var png = DesignConversionBrowserPngEvidenceInspector.Inspect(image.Uri);
        Assert.True(png.IsSuccess, png.ErrorMessage);
        Assert.True(png.Value!.HasNonBlankPixels);

        return new ServerOwnedArtifactFixture(
            new DesignConversionModelServerArtifactRef(
                ArtifactId: artifactId,
                Kind: "screenshot",
                Width: png.Value.PngWidth,
                Height: png.Value.PngHeight,
                Uri: image.Uri,
                Sha256: image.Sha256,
                Metadata: image.Metadata),
            png.Value.LocalPath);
    }

    private static DesignConversionModelServerArtifactRef CreateBlankServerOwnedPngArtifact(
        string artifactRoot,
        string artifactId,
        int width = 32,
        int height = 24)
    {
        Directory.CreateDirectory(artifactRoot);
        var bytes = BuildSolidRgbaPng(width, height, r: 255, g: 255, b: 255, a: 255);
        var path = Path.Combine(artifactRoot, $"{SafePathSegment(artifactId)}.png");
        File.WriteAllBytes(path, bytes);

        return new DesignConversionModelServerArtifactRef(
            ArtifactId: artifactId,
            Kind: "screenshot",
            Width: width,
            Height: height,
            Uri: new Uri(path).AbsoluteUri,
            Sha256: "sha256:" + Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant());
    }

    private static byte[] BuildSolidRgbaPng(
        int width,
        int height,
        byte r,
        byte g,
        byte b,
        byte a)
    {
        var raw = new byte[checked(height * ((width * 4) + 1))];
        for (var y = 0; y < height; y++)
        {
            var row = y * ((width * 4) + 1);
            raw[row] = 0;
            for (var x = 0; x < width; x++)
            {
                var offset = row + 1 + (x * 4);
                raw[offset] = r;
                raw[offset + 1] = g;
                raw[offset + 2] = b;
                raw[offset + 3] = a;
            }
        }

        using var png = new MemoryStream();
        png.Write([137, 80, 78, 71, 13, 10, 26, 10]);
        WritePngChunk(png, "IHDR", BuildPngIhdr(width, height));
        WritePngChunk(png, "IDAT", CompressPngScanlines(raw));
        WritePngChunk(png, "IEND", []);
        return png.ToArray();
    }

    private static byte[] BuildPngIhdr(int width, int height)
    {
        var data = new byte[13];
        BinaryPrimitives.WriteInt32BigEndian(data.AsSpan(0, 4), width);
        BinaryPrimitives.WriteInt32BigEndian(data.AsSpan(4, 4), height);
        data[8] = 8;
        data[9] = 6;
        return data;
    }

    private static byte[] CompressPngScanlines(byte[] raw)
    {
        using var compressed = new MemoryStream();
        using (var zlib = new ZLibStream(compressed, CompressionLevel.Fastest, leaveOpen: true))
            zlib.Write(raw);
        return compressed.ToArray();
    }

    private static void WritePngChunk(Stream stream, string type, byte[] data)
    {
        var typeBytes = Encoding.ASCII.GetBytes(type);
        Span<byte> lengthBytes = stackalloc byte[4];
        BinaryPrimitives.WriteInt32BigEndian(lengthBytes, data.Length);
        stream.Write(lengthBytes);
        stream.Write(typeBytes);
        stream.Write(data);

        var crcInput = new byte[typeBytes.Length + data.Length];
        Array.Copy(typeBytes, 0, crcInput, 0, typeBytes.Length);
        Array.Copy(data, 0, crcInput, typeBytes.Length, data.Length);
        Span<byte> crcBytes = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(crcBytes, Crc32(crcInput));
        stream.Write(crcBytes);
    }

    private static uint Crc32(ReadOnlySpan<byte> bytes)
    {
        var crc = 0xffffffffu;
        foreach (var b in bytes)
        {
            crc ^= b;
            for (var bit = 0; bit < 8; bit++)
                crc = (crc & 1) == 1 ? (crc >> 1) ^ 0xedb88320u : crc >> 1;
        }

        return ~crc;
    }

    private async Task<ServerOwnedProofSubmitProbe> SubmitWithServerOwnedProofAsync(
        string artifactRoot,
        DesignConversionModelServerArtifactRef artifactRef,
        long? maxArtifactBytes = null)
    {
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(CreateVerdict("server-owned-proof-verdict", 0.92, passed: true));
        var service = new DesignConversionInternalService(renderProvider, comparisonProvider, routingFlow: null);
        var resolver = maxArtifactBytes.HasValue
            ? new DesignConversionServerOwnedLocalArtifactRefResolver(artifactRoot, maxArtifactBytes.Value)
            : new DesignConversionServerOwnedLocalArtifactRefResolver(artifactRoot);
        var api = new DesignConversionModelServerApi(
            service,
            () => DateTimeOffset.Parse("2026-07-10T10:00:00Z"),
            artifactRefResolver: resolver,
            postEvaluationStateStore: TestPostEvaluationStateStore);
        var result = await api.SubmitAndEvaluateAsync(CreateRequest($"server-owned-proof-{Guid.NewGuid():N}") with
        {
            Screenshots = [artifactRef],
            DesignRenders = []
        });

        return new ServerOwnedProofSubmitProbe(result, renderProvider, comparisonProvider);
    }

    private static void AssertServerOwnedProofRejectedBeforeRuntime(ServerOwnedProofSubmitProbe probe)
    {
        Assert.False(probe.Result.IsSuccess);
        Assert.Equal("DESIGN_CONVERSION_ARTIFACT_REF_PROOF_REQUIRED", probe.Result.ErrorCode);
        Assert.Empty(probe.RenderProvider.Requests);
        Assert.Empty(probe.ComparisonProvider.Requests);
    }

    private static string CreateServerOwnedArtifactTempRoot()
        => Path.Combine(
            Path.GetTempPath(),
            "design-conversion-server-owned-artifact-proof-tests",
            Guid.NewGuid().ToString("N"));

    private static string CreateServerOwnedVisualArtifactRoot()
        => Path.Combine(
            Path.GetTempPath(),
            "design-conversion-modelserver-api-visual-provider-artifacts",
            Guid.NewGuid().ToString("N"));

    private static JsonlDesignConversionVisualCostLedger CreateServerOwnedVisualCostLedger(
        string artifactRoot)
        => new(Path.Combine(artifactRoot, "visual-cost-ledger"));

    private static string DirectoryFileUri(string path)
    {
        var fullPath = Path.GetFullPath(path);
        if (!fullPath.EndsWith(Path.DirectorySeparatorChar))
            fullPath += Path.DirectorySeparatorChar;
        return new Uri(fullPath).AbsoluteUri;
    }

    private static string Sha256FilePrefixed(string path)
        => "sha256:" + Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();

    private static void DeleteDirectoryIfExists(string path)
    {
        if (Directory.Exists(path))
            Directory.Delete(path, recursive: true);
    }

    private sealed record ServerOwnedArtifactFixture(
        DesignConversionModelServerArtifactRef ArtifactRef,
        string LocalPath);

    private sealed record ServerOwnedVisualProviderFixture(
        DesignConversionModelServerApiRequest Request,
        string ArtifactRoot,
        IDesignConversionRenderProvider RenderProvider) : IDisposable
    {
        public void Dispose()
            => DeleteDirectoryIfExists(ArtifactRoot);
    }

    private sealed record ServerOwnedProofSubmitProbe(
        OperationResult<DesignConversionModelServerApiResult> Result,
        RecordingRenderProvider RenderProvider,
        RecordingComparisonProvider ComparisonProvider);

    private static async Task<RuntimeScoreProbe> RunMachineScoreAsync(
        DesignConversionMachine machine,
        DesignConversionRuntimeFlowInput input,
        string runId)
    {
        var orchestrator = await machine.GetOrchestratorAsync();
        Assert.True(orchestrator.IsSuccess, orchestrator.ErrorMessage);

        var result = await orchestrator.Value!.RunAsync(new LogicalOrchestratorRequest(
            RunId: runId,
            ScenarioId: $"{runId}-scenario",
            UserRequest: "convert this design",
            Flags: new LogicalExecutionFlags(),
            Metadata: new Dictionary<string, object>
            {
                ["runtime_flow_input"] = input
            }));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        return new RuntimeScoreProbe(
            Score: result.Value!.FlowResult.Summary.EnhancedPrimary,
            Passed: result.Value.FlowResult.Summary.Passed,
            OverallVerdict: result.Value.OverallVerdict,
            NextAction: result.Value.FlowResult.Summary.NextAction,
            EvidenceRefs: result.Value.FlowResult.EvidenceIds,
            FlowWarnings: result.Value.FlowResult.Warnings,
            OrchestratorWarnings: result.Value.Warnings,
            Metrics: result.Value.FlowResult.Metrics,
            PrimaryMetricName: result.Value.FlowResult.Summary.PrimaryMetricName,
            ScalingSignalNote: result.Value.FlowResult.Summary.ScalingSignalNote,
            Notes: result.Value.FlowResult.Summary.Notes);
    }

    private static void AssertDirectPolicyConfiguredPairUnavailableProbe(
        RuntimeScoreProbe probe,
        RuntimeScoreProbe baseline,
        params string[] forbiddenFragments)
    {
        Assert.Equal(baseline.Score, probe.Score);
        Assert.Equal(baseline.Passed, probe.Passed);
        Assert.Equal(baseline.OverallVerdict, probe.OverallVerdict);
        Assert.Equal(baseline.NextAction, probe.NextAction);
        Assert.Equal(baseline.PrimaryMetricName, probe.PrimaryMetricName);
        Assert.Equal(baseline.ScalingSignalNote, probe.ScalingSignalNote);
        Assert.Equal(baseline.Notes, probe.Notes);

        Assert.Contains(DesignConversionMachine.DpoPolicyDirectInferenceConfiguredEvidence, probe.EvidenceRefs);
        Assert.Contains(DesignConversionMachine.DpoPolicyRuntimePairContentUnavailableEvidence, probe.EvidenceRefs);
        Assert.Contains(DesignConversionMachine.DpoPolicyDirectInferenceNoRawPayloadEvidence, probe.EvidenceRefs);
        Assert.Contains(DesignConversionMachine.DpoTrainerPolicyCheckpointFreshLoadedEvidence, probe.EvidenceRefs);
        Assert.Contains(DesignConversionMachine.DpoTrainerPolicyCheckpointConvertedEvidence, probe.EvidenceRefs);
        Assert.DoesNotContain(DesignConversionMachine.DpoPolicyDirectInferenceScoreEvidence, probe.EvidenceRefs);
        Assert.DoesNotContain(DesignConversionMachine.DpoPolicyDirectInferenceAcceptanceAuthorityScoreEvidence, probe.EvidenceRefs);
        Assert.DoesNotContain(DesignConversionMachine.DpoPolicyDirectInferenceAcceptanceAuthorityConfiguredEvidence, probe.EvidenceRefs);
        Assert.DoesNotContain(DesignConversionMachine.DpoPolicyDirectInferenceAcceptanceAuthorityEvidence, probe.EvidenceRefs);
        Assert.DoesNotContain(DesignConversionMachine.DpoTrainerPolicyCheckpointRuntimeScoringEvidence, probe.EvidenceRefs);
        Assert.DoesNotContain(DesignConversionMachine.LearnedInferenceAppliedEvidence, probe.EvidenceRefs);
        Assert.DoesNotContain(DesignConversionMachine.DpoPolicyContinuationRuntimeEncodingAppliedEvidence, probe.EvidenceRefs);
        Assert.DoesNotContain(probe.EvidenceRefs, evidence => evidence.Contains("direct-dpo-policy-score-source", StringComparison.Ordinal));
        Assert.DoesNotContain(probe.EvidenceRefs, evidence => evidence.Contains("direct-dpo-policy-acceptance-authority", StringComparison.Ordinal));

        Assert.Equal(bool.TrueString, probe.Metrics["direct_dpo_policy_runtime_scorer_configured"]);
        Assert.Equal(bool.FalseString, probe.Metrics["direct_dpo_policy_runtime_pair_content_available"]);
        Assert.Equal(bool.FalseString, probe.Metrics["direct_dpo_policy_inference_applied"]);
        Assert.Equal(bool.FalseString, probe.Metrics["direct_dpo_policy_score_authority"]);
        Assert.Equal(bool.FalseString, probe.Metrics["direct_dpo_policy_acceptance_authority"]);
        Assert.Equal(bool.TrueString, probe.Metrics["baseline_visual_verdict_acceptance_authority"]);
        Assert.Equal(bool.FalseString, probe.Metrics[DesignConversionModelAuthorityBoundary.TrainedModelAuthorityKey]);
        Assert.Equal(bool.TrueString, probe.Metrics[DesignConversionModelAuthorityBoundary.NotTrainedModelAuthorityKey]);
        Assert.Contains(DesignConversionMachine.DpoPolicyRuntimePairContentUnavailableWarning, probe.FlowWarnings);
        Assert.Contains(DesignConversionMachine.DpoPolicyRuntimePairContentUnavailableWarning, probe.OrchestratorWarnings);

        var publicSurface = string.Join(
            "\n",
            probe.EvidenceRefs.Concat(
            [
                JsonSerializer.Serialize(probe.Metrics),
                string.Join("|", probe.FlowWarnings),
                string.Join("|", probe.OrchestratorWarnings),
                probe.PrimaryMetricName,
                probe.ScalingSignalNote,
                probe.Notes
            ]));
        Assert.DoesNotContain("LlmModel.Forward", publicSurface, StringComparison.Ordinal);
        Assert.DoesNotContain("log-prob margin", publicSurface, StringComparison.Ordinal);
        Assert.DoesNotContain("DirectDpoPolicyInferenceSummary", publicSurface, StringComparison.Ordinal);
        foreach (var fragment in forbiddenFragments.Where(fragment => !string.IsNullOrEmpty(fragment)))
            Assert.DoesNotContain(fragment, publicSurface, StringComparison.OrdinalIgnoreCase);
    }

    private sealed record RuntimeScoreProbe(
        double Score,
        bool Passed,
        string OverallVerdict,
        string NextAction,
        IReadOnlyList<string> EvidenceRefs,
        IReadOnlyList<string> FlowWarnings,
        IReadOnlyList<string> OrchestratorWarnings,
        IReadOnlyDictionary<string, string> Metrics,
        string PrimaryMetricName,
        string ScalingSignalNote,
        string Notes);

    private static async Task<DesignConversionTrainingPipelineResult> BuildDpoBridgePromotionReadyPipelineResultAsync()
        => await BuildDpoBridgePromotionPipelineResultAsync(
            BuildDpoBridgePromotionMeasurementGateRequest(
                "dpo-bridge-promotion-test-measurement-gate",
                sourceDisjoint: true));

    private static async Task<DesignConversionTrainingPipelineResult> BuildDpoBridgePromotionPipelineResultAsync(
        DesignConversionMeasurementGateRequest measurementGateRequest)
    {
        var build = new DesignConversionDpoGenerator().BuildTrainReadyRecords(
            BuildDpoBridgePromotionSourceDisjointExamples(100));
        Assert.True(build.IsSuccess, build.ErrorMessage);
        Assert.Empty(build.Value!.QuarantinedRecords);

        var result = await DesignConversionLegalTeacherCapabilityTestFactory
            .RunStrictPipelineAsync(
            new DesignConversionTrainingPipeline(),
            new DesignConversionMachine(),
            build.Value.AcceptedRecords,
            new DesignConversionTrainingPipelineOptions(
                "dpo-bridge-local-promotion-readiness-context",
                BatchSize: 25,
                CallMachineLifecycleHook: true,
                DatasetOptions: DpoBridgePromotionDatasetOptions(),
                MeasurementGateRequest: measurementGateRequest));
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.True(result.Value!.DatasetCountGate.Passed);

        return result.Value;
    }

    private static IReadOnlyList<DesignConversionStoredDpoExample> BuildDpoBridgePromotionSourceDisjointExamples(int count)
        => Enumerable.Range(0, count)
            .Select(index =>
            {
                var sourceId = $"dpo-bridge-promotion-source-{index:000}";
                var unitId = index % 2 == 0
                    ? DesignConversionDpoUnits.CriticUnit
                    : DesignConversionDpoUnits.ImproverUnit;
                var example = DesignConversionDpoGeneratorTests.CreateExample(
                    $"dpo-bridge-promotion-record-{index:000}",
                    unitId,
                    usefulness: 0.80 + (index % 10 * 0.01));

                return example with
                {
                    Provenance = example.Provenance with
                    {
                        SourceRequestId = $"request:{sourceId}",
                        SourceManifestRef = $"manifest://dcm-source/{sourceId}",
                        Metadata = new Dictionary<string, string>
                        {
                            ["source_design_id"] = sourceId
                        }
                    }
                };
            })
            .ToArray();

    private static DesignConversionDpoDatasetOptions DpoBridgePromotionDatasetOptions()
        => new(
            DatasetId: "dcm-dpo-bridge-local-promotion-dataset",
            MinimumTrainRecords: 50,
            MinimumHeldOutRecords: 50,
            Preflight: DesignConversionDpoDatasetPreflight.LocalFixtureTrainingCandidateOnly(
                "dpo_bridge_promotion_local_fixture_structural_preflight"));

    private static DesignConversionMeasurementGateRequest BuildDpoBridgePromotionMeasurementGateRequest(
        string gateId,
        bool sourceDisjoint)
        => new(
            GateId: gateId,
            UnitId: DesignConversionDpoUnits.CriticUnit,
            HeldOutMetrics: new DesignConversionHeldOutMeasurementMetrics(
                HeldOutRecordCount: 50,
                HeldOutQualityScore: 0.94,
                RuntimeMs: 320,
                CostUsd: 0m,
                PeakRamMb: 256,
                EvidenceRefs:
                [
                    $"artifact://caller-measurement/{Uri.EscapeDataString(gateId)}/heldout.json"
                ]),
            IntegrityProof: new DesignConversionMeasurementIntegrityProof(
                SourceDisjoint: sourceDisjoint,
                LeakageDetected: false,
                HiddenDeterminismDetected: false,
                AblationPassed: true,
                EvidenceRefs:
                [
                    $"artifact://caller-measurement/{Uri.EscapeDataString(gateId)}/integrity.json"
                ]),
            Thresholds: new DesignConversionMeasurementGateThresholds(
                RequiredHeldOutQualityScore: 0.90,
                MaxRuntimeMs: 800,
                MaxCostUsd: 1.00m,
                MaxPeakRamMb: 512));

    private static DesignConversionMeasurementGateResult BuildDpoBridgeManualPassedMeasurementGateWithoutReadinessMarker()
        => new(
            GateId: "dpo-bridge-manual-passed-status-without-readiness-marker",
            UnitId: DesignConversionDpoUnits.CriticUnit,
            Status: DesignConversionMeasurementGateStatus.Passed,
            Passed: true,
            RequiredHeldOutQualityScore: 0.90,
            HeldOutQualityScore: 0.94,
            SourceDisjoint: true,
            AblationPassed: true,
            BudgetPassed: true,
            Reasons: ["ALL_MEASUREMENT_GATES_PASSED"],
            EvidenceRefs:
            [
                "DESIGN_CONVERSION_MEASUREMENT_GATE_PASSED",
                "dpo-bridge-manual-heldout-metrics:passed-status-only",
                "dpo-bridge-manual-ablation:passed-status-only"
            ]);

    private static async Task<DesignConversionTrainingManifestArtifacts> WriteDpoBridgePromotionArtifactsAsync(
        string outputDirectory,
        string checkpointPath)
    {
        var tokenizerPath = Path.Combine(outputDirectory, "dpo-bridge-tokenizer.json");
        var policyPath = Path.Combine(outputDirectory, "dpo-bridge-policy.json");
        await File.WriteAllTextAsync(
            tokenizerPath,
            "{\"kind\":\"dpo_bridge_local_promotion_tokenizer_boundary\"}",
            Encoding.UTF8);
        await File.WriteAllTextAsync(
            policyPath,
            "{\"kind\":\"dpo_bridge_local_promotion_policy_boundary\"}",
            Encoding.UTF8);
        return new DesignConversionTrainingManifestArtifacts(
            CheckpointPath: checkpointPath,
            TokenizerPath: tokenizerPath,
            PolicyPath: policyPath);
    }

    private static DesignConversionPromotionGateReport BuildDpoBridgePromotionGate(
        DesignConversionTrainingManifestArtifacts artifacts,
        bool includeSourceDisjointHeldOutReadinessEvidence = true)
    {
        var artifactPaths = new[]
        {
            artifacts.CheckpointPath,
            artifacts.TokenizerPath,
            artifacts.PolicyPath
        };

        var evidenceRefs = new[]
            {
                "DPO_BRIDGE_LOCAL_PROMOTION_GATE_TEST_EVIDENCE"
            }
            .Concat(includeSourceDisjointHeldOutReadinessEvidence
                ? [DesignConversionTrainingPipeline.SourceDisjointHeldOutReadinessGatePassedEvidence]
                : [])
            .ToArray();

        return new DesignConversionPromotionGateReport(
            HeldOutScore: 0.94,
            RequiredHeldOutScore: 0.90,
            HeldOutSourceDisjoint: true,
            DeterministicShortcutDetected: false,
            HiddenShortcutEvidenceDetected: false,
            FreshLoadPassed: true,
            BudgetTierPolicyPassed: true,
            BudgetTier: "bounded-test-tier",
            DpoQuarantineCount: 0,
            MaxDpoQuarantineCount: 1,
            RequiredArtifactPaths: artifactPaths,
            PresentArtifactPaths: artifactPaths,
            EvidenceRefs: evidenceRefs);
    }

    private static MethodInfo[] GetRelevantHandoffMethods(
        Type declaringType,
        BindingFlags bindingFlags)
        => declaringType
            .GetMethods(bindingFlags)
            .Where(method => method.Name is
                nameof(DesignConversionDpoTrainingHandoff.ExecuteAsync) or
                nameof(DesignConversionDpoTrainingHandoff.ConvertPolicyCheckpointToRuntimeCheckpointAsync))
            .ToArray();

    private static MethodInfo FindDeclaredMethod(
        Type declaringType,
        string methodName,
        BindingFlags bindingFlags,
        Type[] parameterTypes)
    {
        var method = declaringType.GetMethod(
            methodName,
            bindingFlags,
            binder: null,
            types: parameterTypes,
            modifiers: null);
        return Assert.IsAssignableFrom<MethodInfo>(method);
    }

    private static void AssertExactPublicMethodSignature(
        IEnumerable<MethodInfo> methods,
        string methodName,
        Type[] parameterTypes)
    {
        var method = Assert.Single(methods, candidate =>
            candidate.Name == methodName &&
            candidate.GetParameters()
                .Select(parameter => parameter.ParameterType)
                .SequenceEqual(parameterTypes));
        Assert.Contains(
            method.GetParameters(),
            parameter => parameter.ParameterType == typeof(IDesignConversionDpoPolicyBaseProvider));
    }

    private static void AssertNonFatalFilters(
        string methodName,
        Type[] parameterTypes,
        int minimumClassifierFilterCount)
    {
        var method = FindDeclaredMethod(
            typeof(DesignConversionDpoTrainingHandoff),
            methodName,
            BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly,
            parameterTypes);
        var stateMachine = Assert.IsType<System.Runtime.CompilerServices.AsyncStateMachineAttribute>(
            method.GetCustomAttribute<System.Runtime.CompilerServices.AsyncStateMachineAttribute>());
        var moveNext = Assert.IsAssignableFrom<MethodInfo>(
            stateMachine.StateMachineType.GetMethod(
                "MoveNext",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
        var body = Assert.IsAssignableFrom<MethodBody>(moveNext.GetMethodBody());
        var filters = body.ExceptionHandlingClauses
            .Where(clause => clause.Flags == ExceptionHandlingClauseOptions.Filter)
            .ToArray();
        Assert.NotEmpty(filters);
        var classifier = Assert.IsAssignableFrom<MethodInfo>(
            typeof(SnapshotExceptionGraphClassifier).GetMethod(
                nameof(SnapshotExceptionGraphClassifier.IsNonFatal),
                BindingFlags.Public | BindingFlags.Static,
                binder: null,
                types: [typeof(Exception)],
                modifiers: null));
        var il = Assert.IsType<byte[]>(body.GetILAsByteArray());
        var classifierCallsByFilter = new List<int>();
        foreach (var filter in filters)
        {
            var classifierCalls = 0;
            for (var offset = filter.FilterOffset;
                 offset <= filter.HandlerOffset - (1 + sizeof(int));
                 offset++)
            {
                if (il[offset] != unchecked((byte)OpCodes.Call.Value))
                    continue;

                var metadataToken = BinaryPrimitives.ReadInt32LittleEndian(
                    il.AsSpan(offset + 1, sizeof(int)));
                MethodBase? resolved;
                try
                {
                    resolved = moveNext.Module.ResolveMethod(
                        metadataToken,
                        moveNext.DeclaringType?.GetGenericArguments(),
                        moveNext.GetGenericArguments());
                }
                catch (ArgumentException)
                {
                    continue;
                }

                if (resolved.DeclaringType == classifier.DeclaringType
                    && resolved.Name == classifier.Name)
                {
                    classifierCalls++;
                }
            }

            classifierCallsByFilter.Add(classifierCalls);
        }

        Assert.True(
            classifierCallsByFilter.Count(classifierCalls => classifierCalls == 1) >=
                minimumClassifierFilterCount,
            $"Expected at least {minimumClassifierFilterCount} non-fatal filters in {methodName}.");
        Assert.All(
            classifierCallsByFilter,
            classifierCalls => Assert.InRange(classifierCalls, 0, 1));
    }

    private static IReadOnlyList<int[]> SnapshotLlmParameterBits(LlmModel model)
        => model.Parameters()
            .Select(parameter => parameter.Value.Data
                .Select(BitConverter.SingleToInt32Bits)
                .ToArray())
            .ToArray();

    private static void AssertLlmParameterBitsEqual(
        IReadOnlyList<int[]> expected,
        LlmModel actual)
    {
        var actualParameters = actual.Parameters().ToArray();
        Assert.Equal(expected.Count, actualParameters.Length);
        for (var index = 0; index < expected.Count; index++)
        {
            Assert.Equal(
                expected[index],
                actualParameters[index].Value.Data
                    .Select(BitConverter.SingleToInt32Bits)
                    .ToArray());
        }
    }

    private static BpeTokenizer BuildDpoHandoffTokenizer()
    {
        var tokenizer = new BpeTokenizer();
        tokenizer.Train("design conversion figma source unit state vector chosen rejected dashboard card layout typography spacing quality accepted failed visual qa", 300);
        return tokenizer;
    }

    private static ModelConfig BuildDpoHandoffTinyConfig()
        => new()
        {
            VocabSize = 512,
            ContextLen = 64,
            HiddenDim = 16,
            NumLayers = 1,
            NumHeads = 4,
            NumKvHeads = 2,
            FfnHiddenDim = 32
        };

    private static DesignConversionDpoTrainingRecord BuildDpoPolicyBridgeSourceEncodedRecord()
    {
        var example = DesignConversionDpoGeneratorTests.CreateExample(
            "dpo-policy-bridge-source-signal",
            DesignConversionDpoUnits.CriticUnit) with
        {
            InternalVectorPayload = [],
            SourceStructurePayload = """
                {
                  "source":"figma",
                  "file_path":"C:/Users/lubao/private/source.fig",
                  "root":{"type":"FRAME","layoutMode":"VERTICAL","width":1440,"height":900},
                  "children":[
                    {"type":"TEXT","name":"headline"},
                    {"type":"COMPONENT","name":"metric-card","constraints":{"horizontal":"CENTER"}},
                    {"type":"IMAGE","name":"hero_asset"}
                  ],
                  "breakpoints":["desktop","mobile"],
                  "layout":{"mode":"AUTO_LAYOUT","gap":16}
                }
                """,
            Provenance = new DesignConversionDpoProvenance(
                SourceRequestId: "dpo-policy-bridge-source-signal-request",
                SourceKind: DesignConversionSourceKind.Figma.ToString(),
                SourceManifestRef: "manifest://design-conversion/dpo-policy-bridge-source-signal",
                ReviewerRef: "teacher:redacted",
                CreatedAtUtc: DateTimeOffset.UnixEpoch,
                Metadata: new Dictionary<string, string> { ["split"] = "train" })
        };
        var build = new DesignConversionDpoGenerator().BuildTrainReadyRecords([example]);

        Assert.True(build.IsSuccess, build.ErrorMessage);
        Assert.Empty(build.Value!.QuarantinedRecords);
        var record = Assert.Single(build.Value.AcceptedRecords);
        Assert.True(record.InternalVectorPayload.Count > 8);
        Assert.Equal("DesignConversionDefaultSourceEncoder", record.Provenance.Metadata["source_encoder"]);
        Assert.Equal("dcm-source-encoder-v1", record.Provenance.Metadata["source_encoder_schema"]);
        Assert.Contains("source_encoder_vector_hash", record.Provenance.Metadata.Keys);
        Assert.Contains("source_structure_signal_summary", record.Provenance.Metadata.Keys);
        Assert.DoesNotContain("C:/Users/lubao", record.SourceStructurePayload, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("private/source.fig", record.SourceStructurePayload, StringComparison.OrdinalIgnoreCase);

        return record;
    }

    private static string ComputeExpectedDpoConfigSha256(ModelConfig config)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        AppendExpectedDpoString(hash, "design-conversion-dpo-policy-config-v1");
        AppendExpectedDpoInt32(hash, config.VocabSize);
        AppendExpectedDpoInt32(hash, config.ContextLen);
        AppendExpectedDpoInt32(hash, config.HiddenDim);
        AppendExpectedDpoInt32(hash, config.NumLayers);
        AppendExpectedDpoInt32(hash, config.NumHeads);
        AppendExpectedDpoInt32(hash, config.NumKvHeads);
        AppendExpectedDpoInt32(hash, config.FfnHiddenDim);
        AppendExpectedDpoInt32(hash, BitConverter.SingleToInt32Bits(config.RopeTheta));
        AppendExpectedDpoInt32(hash, BitConverter.SingleToInt32Bits(config.RmsNormEps));
        return Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();
    }

    private static string ComputeExpectedDpoInitialWeightSha256(LlmModel model)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        AppendExpectedDpoString(hash, "design-conversion-dpo-initial-weights-v1");
        var entries = model.GetCheckpointParameterEntries();
        AppendExpectedDpoInt32(hash, entries.Count);
        foreach (var entry in entries)
        {
            AppendExpectedDpoString(hash, entry.Key);
            AppendExpectedDpoInt32(hash, entry.Parameter.Value.Shape.Rank);
            foreach (var dimension in entry.Parameter.Value.Shape.Dimensions)
                AppendExpectedDpoInt32(hash, dimension);
            AppendExpectedDpoInt32(hash, entry.Parameter.Value.Data.Length);
            foreach (var value in entry.Parameter.Value.Data)
                AppendExpectedDpoInt32(hash, BitConverter.SingleToInt32Bits(value));
        }

        return Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();
    }

    private static void AppendExpectedDpoString(IncrementalHash hash, string value)
    {
        var bytes = new UTF8Encoding(false, true).GetBytes(value);
        AppendExpectedDpoInt32(hash, bytes.Length);
        hash.AppendData(bytes);
    }

    private static void AppendExpectedDpoInt32(IncrementalHash hash, int value)
    {
        Span<byte> bytes = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32BigEndian(bytes, value);
        hash.AppendData(bytes);
    }

    private static void AssertCurrentSeededDpoModelSet(
        DesignConversionDpoPolicyBaseModelSet? modelSet,
        params DesignConversionDpoPolicyModelRole[] roles)
    {
        var value = Assert.IsType<DesignConversionDpoPolicyBaseModelSet>(modelSet);
        var randomContext = DesignConversionDpoPolicyBaseTestProvider
            .CreateCurrentRandomContext();
        Assert.Equal(randomContext, value.RandomContext);
        Assert.NotEmpty(roles);
        foreach (var role in roles)
        {
            var model = role switch
            {
                DesignConversionDpoPolicyModelRole.Policy => value.Policy,
                DesignConversionDpoPolicyModelRole.Reference => value.Reference,
                DesignConversionDpoPolicyModelRole.FreshLoad => value.FreshLoadTarget,
                _ => throw new ArgumentOutOfRangeException(nameof(roles), role, null)
            };
            _ = DesignConversionDpoPolicyBaseMetadata.ValidateSeededCanonicalLoraTopology(
                model,
                randomContext,
                role);
        }
    }

    private static void AssertCurrentRandomContextJson(
        JsonElement json,
        DesignConversionDpoPolicyRandomContext expected)
    {
        Assert.Equal(expected.SchemaVersion, json.GetProperty("schemaVersion").GetInt32());
        Assert.Equal(
            expected.RootInitializationSeed,
            json.GetProperty("rootInitializationSeed").GetUInt64());
        Assert.Equal(
            expected.TrainingConfigurationFingerprint,
            json.GetProperty("trainingConfigurationFingerprint").GetString());
        Assert.Equal(expected.DerivationScheme, json.GetProperty("derivationScheme").GetString());
        Assert.Equal(
            expected.InitializationDomain,
            json.GetProperty("initializationDomain").GetString());
        Assert.Equal(expected.DropoutDomain, json.GetProperty("dropoutDomain").GetString());
        Assert.Equal(
            expected.PolicyDropoutStreamDomain,
            json.GetProperty("policyDropoutStreamDomain").GetString());
        Assert.Equal(
            expected.ReferenceDropoutStreamDomain,
            json.GetProperty("referenceDropoutStreamDomain").GetString());
        Assert.Equal(
            expected.FreshLoadDropoutStreamDomain,
            json.GetProperty("freshLoadDropoutStreamDomain").GetString());
        Assert.Equal(
            expected.ReproducibilityScope,
            json.GetProperty("reproducibilityScope").GetString());
    }

    private static void AssertDpoModelParametersBitwiseEqual(LlmModel expected, LlmModel actual)
    {
        var expectedEntries = expected.GetCheckpointParameterEntries();
        var actualEntries = actual.GetCheckpointParameterEntries();
        Assert.Equal(expectedEntries.Count, actualEntries.Count);
        for (var entryIndex = 0; entryIndex < expectedEntries.Count; entryIndex++)
        {
            Assert.Equal(expectedEntries[entryIndex].Key, actualEntries[entryIndex].Key);
            Assert.Equal(
                expectedEntries[entryIndex].Parameter.Value.Shape.Dimensions,
                actualEntries[entryIndex].Parameter.Value.Shape.Dimensions);
            Assert.Equal(
                expectedEntries[entryIndex].Parameter.Value.Data.Select(BitConverter.SingleToInt32Bits),
                actualEntries[entryIndex].Parameter.Value.Data.Select(BitConverter.SingleToInt32Bits));
        }
    }

    private static IReadOnlyList<int[]> SnapshotDpoModelParameterBits(LlmModel model)
        => model.GetCheckpointParameterEntries()
            .Select(entry => entry.Parameter.Value.Data
                .Select(BitConverter.SingleToInt32Bits)
                .ToArray())
            .ToArray();

    private static IReadOnlyList<int[]> SnapshotDpoBaseParameterBits(LlmModel model)
        => model.BaseParameters
            .Select(parameter => parameter.Value.Data
                .Select(BitConverter.SingleToInt32Bits)
                .ToArray())
            .ToArray();

    private static void AssertDpoBaseParameterBitsEqual(
        IReadOnlyList<int[]> expected,
        LlmModel actual)
    {
        Assert.Equal(expected.Count, actual.BaseParameters.Count);
        for (var index = 0; index < expected.Count; index++)
        {
            Assert.Equal(
                expected[index],
                actual.BaseParameters[index].Value.Data
                    .Select(BitConverter.SingleToInt32Bits)
                    .ToArray());
        }
    }

    private static bool HasDpoTargetAdapters(LlmModel model)
        => model.Blocks.Count > 0 &&
           model.GetAttachedProjectionAdapter(0, "q_proj") is not null &&
           model.GetAttachedProjectionAdapter(0, "v_proj") is not null;

    private static readonly IReadOnlyDictionary<short, OpCode> IlOpCodes =
        typeof(OpCodes)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(field => field.FieldType == typeof(OpCode))
            .Select(field => (OpCode)field.GetValue(null)!)
            .ToDictionary(opCode => opCode.Value);

    private static void AssertIlDoesNotConstruct(MethodInfo method, ConstructorInfo forbiddenConstructor)
        => Assert.False(
            IlConstructs(method, forbiddenConstructor),
            $"{method.DeclaringType?.FullName}.{method.Name} constructs LlmModel.");

    private static bool IlConstructs(MethodInfo method, ConstructorInfo forbiddenConstructor)
    {
        var il = method.GetMethodBody()?.GetILAsByteArray();
        if (il is null)
            return false;

        var offset = 0;
        while (offset < il.Length)
        {
            var instructionOffset = offset;
            var firstByte = il[offset++];
            var opCodeValue = firstByte == 0xfe
                ? ReadTwoByteOpCodeValue(il, ref offset, instructionOffset)
                : (short)firstByte;
            if (!IlOpCodes.TryGetValue(opCodeValue, out var opCode))
            {
                throw new InvalidOperationException(
                    $"Unknown IL opcode 0x{unchecked((ushort)opCodeValue):x4} at offset {instructionOffset} " +
                    $"in {method.DeclaringType?.FullName}.{method.Name}.");
            }

            var operandOffset = offset;
            var operandSize = GetIlOperandSize(opCode, il, operandOffset, method);
            if (operandOffset > il.Length - operandSize)
            {
                throw new InvalidOperationException(
                    $"Truncated IL operand for {opCode.Name} at offset {instructionOffset} " +
                    $"in {method.DeclaringType?.FullName}.{method.Name}.");
            }

            if (opCode == OpCodes.Newobj)
            {
                var metadataToken = BinaryPrimitives.ReadInt32LittleEndian(
                    il.AsSpan(operandOffset, sizeof(int)));
                var resolved = method.Module.ResolveMethod(
                    metadataToken,
                    method.DeclaringType?.GetGenericArguments() ?? Type.EmptyTypes,
                    method.GetGenericArguments());
                if (resolved is ConstructorInfo candidate &&
                    ConstructorsHaveSameSignature(candidate, forbiddenConstructor))
                {
                    return true;
                }
            }

            offset = operandOffset + operandSize;
        }

        return false;
    }

    private static short ReadTwoByteOpCodeValue(byte[] il, ref int offset, int instructionOffset)
    {
        if (offset >= il.Length)
            throw new InvalidOperationException($"Truncated two-byte IL opcode at offset {instructionOffset}.");

        return unchecked((short)(0xfe00 | il[offset++]));
    }

    private static int GetIlOperandSize(
        OpCode opCode,
        byte[] il,
        int operandOffset,
        MethodInfo method)
    {
        switch (opCode.OperandType)
        {
            case OperandType.InlineNone:
                return 0;
            case OperandType.ShortInlineBrTarget:
            case OperandType.ShortInlineI:
            case OperandType.ShortInlineVar:
                return 1;
            case OperandType.InlineVar:
                return 2;
            case OperandType.InlineBrTarget:
            case OperandType.InlineField:
            case OperandType.InlineI:
            case OperandType.InlineMethod:
            case OperandType.InlineSig:
            case OperandType.InlineString:
            case OperandType.InlineTok:
            case OperandType.InlineType:
            case OperandType.ShortInlineR:
                return sizeof(int);
            case OperandType.InlineI8:
            case OperandType.InlineR:
                return sizeof(long);
            case OperandType.InlineSwitch:
                if (operandOffset > il.Length - sizeof(int))
                {
                    throw new InvalidOperationException(
                        $"Truncated IL switch count in {method.DeclaringType?.FullName}.{method.Name}.");
                }

                var targetCount = BinaryPrimitives.ReadInt32LittleEndian(
                    il.AsSpan(operandOffset, sizeof(int)));
                if (targetCount < 0)
                {
                    throw new InvalidOperationException(
                        $"Negative IL switch target count in {method.DeclaringType?.FullName}.{method.Name}.");
                }

                return checked(sizeof(int) + (targetCount * sizeof(int)));
            default:
                throw new InvalidOperationException(
                    $"Unsupported IL operand type {opCode.OperandType} for {opCode.Name} " +
                    $"in {method.DeclaringType?.FullName}.{method.Name}.");
        }
    }

    private static bool ConstructorsHaveSameSignature(
        ConstructorInfo candidate,
        ConstructorInfo forbiddenConstructor)
    {
        if (candidate.Module != forbiddenConstructor.Module ||
            candidate.DeclaringType != forbiddenConstructor.DeclaringType ||
            candidate.CallingConvention != forbiddenConstructor.CallingConvention)
        {
            return false;
        }

        var candidateParameters = candidate.GetParameters();
        var forbiddenParameters = forbiddenConstructor.GetParameters();
        return candidateParameters.Length == forbiddenParameters.Length &&
               candidateParameters
                   .Select(parameter => parameter.ParameterType)
                   .SequenceEqual(forbiddenParameters.Select(parameter => parameter.ParameterType));
    }

    private static LlmModel ConstructLlmModelForIlDecoderPositiveControl(ModelConfig config)
        => new(config);

    private static TrainingTeacherProvenance BuildExternalQwenTeacherProvenance(
        string requestId,
        string modelId,
        string modelVersion,
        bool closedProprietaryModel = false)
        => new(
            TrainingTeacherSourceKind.ExternalTeacherOutput,
            OpenWeightTeacherFamily.Qwen,
            TrainingTeacherAccessMode.LocalHostedOpenWeight,
            TrainingTeacherHostingProvider.Ollama,
            modelId,
            modelVersion,
            TrainingTeacherModelLicense.Apache20,
            ProviderTermsChecked: false,
            ProviderTermsAllowTraining: false,
            closedProprietaryModel,
            TeacherOutputUsedForTraining: true,
            requestId,
            InvocationCount: 1)
        {
            ModelArtifactDigestKind =
                LegalTrainingTeacherAdmissionAuthority.OllamaModelLayerSha256DigestKind,
            ModelArtifactDigest = LegalTeacherModelLayerDigest
        };

    private static byte[] StableTeacherAuditSalt()
        => Enumerable.Range(1, 32).Select(static value => (byte)value).ToArray();

    private static DesignConversionDpoTrainingRecord BuildDpoHandoffRecord()
    {
        var expected = new RenderQaImageRecord(
            ImageId: "pass153-expected",
            Label: "expected",
            Resolution: new RenderQaResolution("desktop", 1440, 900, 1),
            Uri: "artifact://pass153/expected.png",
            Sha256: "sha256:pass153-expected",
            Metadata: new Dictionary<string, string> { ["kind"] = "expected" });
        var actual = expected with
        {
            ImageId = "pass153-actual",
            Label = "actual",
            Uri = "artifact://pass153/actual.png",
            Sha256 = "sha256:pass153-actual"
        };
        var qa = new RenderQaResultRecord(
            ResultId: "pass153-qa-result",
            RequestId: "pass153-request",
            Expected: expected,
            Actual: actual,
            Verdict: CreateVerdict("pass153-qa-verdict", 0.95, passed: true),
            Metrics: new Dictionary<string, double> { ["visual_distance"] = 0.05 },
            Warnings: []);
        var chosen = new DesignConversionImprovementOutput(
            OutputId: "pass153-chosen-output",
            CandidateId: "pass153-chosen-candidate",
            Html: "<main><section class=\"card\">Chosen dashboard</section></main>",
            Css: ".card{display:grid;gap:12px;color:#111827;}",
            Verdict: CreateVerdict("pass153-chosen-verdict", 0.96, passed: true),
            QaResults: [qa],
            EvidenceRefs: ["pass153-chosen-evidence"],
            Accepted: true);
        var rejected = new DesignConversionImprovementOutput(
            OutputId: "pass153-rejected-output",
            CandidateId: "pass153-rejected-candidate",
            Html: "<main><section class=\"card broken\">Rejected dashboard</section></main>",
            Css: ".card{display:block;gap:0;color:#111827;}",
            Verdict: CreateVerdict("pass153-rejected-verdict", 0.21, passed: false),
            QaResults: [],
            EvidenceRefs: ["pass153-rejected-evidence"],
            Accepted: false);

        return new DesignConversionDpoTrainingRecord(
            RecordId: "pass153-dpo-record",
            UnitId: "DesignConversionRuntimeScaffoldCriticUnit",
            InternalUnitStatePayload: "{\"state\":\"compact-local-handoff\",\"slots\":[\"spacing\",\"hierarchy\"]}",
            InternalVectorPayload: [0.25, -0.5, 0.75, 1.0],
            SourceStructurePayload: "{\"type\":\"FRAME\",\"children\":[\"header\",\"metric-card\"]}",
            Chosen: chosen,
            Rejected: rejected,
            TieBreakerPayload: null,
            UsefulnessScore: 0.87,
            NegativeQualityReason: "spacing collapse and hierarchy loss",
            NegativeReasonTaxonomy: "visual_hierarchy",
            Provenance: new DesignConversionDpoProvenance(
                SourceRequestId: "pass153-request",
                SourceKind: "Figma",
                SourceManifestRef: "artifact://pass153/source-manifest.json",
                ReviewerRef: "local-test-reviewer",
                CreatedAtUtc: DateTimeOffset.Parse("2026-07-07T13:00:00Z"),
                Metadata: new Dictionary<string, string> { ["slice"] = "DpoTrainingHandoff" }),
            License: new DesignConversionLicenseScope(
                LicenseId: "pass153-local-training-license",
                RightsScope: "local-test-training",
                AllowsTraining: true,
                ContainsUserPrivateData: false,
                Metadata: new Dictionary<string, string> { ["fixture"] = bool.TrueString }),
            Quarantine: new DesignConversionQuarantineState(
                IsQuarantined: false,
                Reason: string.Empty,
                EvidenceRefs: []),
            TeacherProvenance: TrainingTeacherProvenance.Deterministic(
                "pass153-deterministic-source"))
        {
            ChosenAuthorKind = DpoPairSideAuthorKind.DeterministicGenerator,
            RejectedAuthorKind = DpoPairSideAuthorKind.DeterministicGenerator
        };
    }

    // Общая оснастка выше объявляет обе стороны пары детерминированным генератором:
    // это верно только для записи без внешнего учителя. Как только проверка
    // подменяет паспорт учителя на внешнего, такое авторство становится
    // внутренне противоречивым, и мост законно отвергает запись как пару без
    // действительного паспорта. Этот помощник приводит авторство в соответствие с
    // внешним учителем, чтобы проверка снова доходила до того стража, который она
    // и была написана проверять, и ни одно утверждение защиты не ослаблялось.
    private static DesignConversionDpoTrainingRecord WithOpenTeacherPairAuthorship(
        DesignConversionDpoTrainingRecord record)
    {
        // Прежние ключи источника сохраняем: мост читает признаки направления из
        // тех же метаданных, и терять «slice» оснастки нельзя.
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var entry in record.Provenance.Metadata)
            metadata[entry.Key] = entry.Value;
        // Источник авторства — расписка независимого предпочтения, как в наборе
        // проверок самого коммита-автора договора о паспорте пары.
        metadata[DesignConversionDpoPolicyCheckpointBridge
            .PairAuthorshipSourceMetadataKey] = "independent_preference_receipt";
        // Выбранную сторону пишет открытый учитель.
        metadata[DesignConversionDpoPolicyCheckpointBridge
            .PairChosenSourceSideMetadataKey] =
            DesignConversionDpoPolicyCheckpointBridge.PairSourceSideGenerated;
        // Отклонённую сторону пишет локальная модель.
        metadata[DesignConversionDpoPolicyCheckpointBridge
            .PairRejectedSourceSideMetadataKey] =
            DesignConversionDpoPolicyCheckpointBridge.PairSourceSideIncumbent;
        // Те же два авторства дублируются в метаданных, как требует договор.
        metadata[DesignConversionDpoPolicyCheckpointBridge
            .PairChosenAuthorKindMetadataKey] =
            DpoPairSideAuthorKind.OpenTeacher.ToString();
        metadata[DesignConversionDpoPolicyCheckpointBridge
            .PairRejectedAuthorKindMetadataKey] =
            DpoPairSideAuthorKind.LocalModel.ToString();
        // Ровно одна сторона — открытый учитель, другая — локальная модель: именно
        // этого требует разрешатель авторства для записи с внешним учителем.
        return record with
        {
            Provenance = record.Provenance with { Metadata = metadata },
            ChosenAuthorKind = DpoPairSideAuthorKind.OpenTeacher,
            RejectedAuthorKind = DpoPairSideAuthorKind.LocalModel
        };
    }

    private static DesignConversionDpoResolvedPartition BuildEvaluatedBridgePartitionForApiTest(
        string id,
        int heldOutCount)
    {
        var train = WithEvaluatedBridgeSourceIdentity(
            BuildDpoHandoffRecord() with
            {
                RecordId = $"{id}-train",
                UnitId = DesignConversionDpoUnits.ImproverUnit
            },
            $"{id}-source-train");
        var heldOut = Enumerable.Range(0, heldOutCount)
            .Select(index => WithEvaluatedBridgeSourceIdentity(
                BuildDpoHandoffRecord() with
                {
                    RecordId = $"{id}-heldout-{index}",
                    UnitId = index % 2 == 0
                        ? DesignConversionDpoUnits.CriticUnit
                        : DesignConversionDpoUnits.ImproverUnit,
                    UsefulnessScore = 0.82 + index * 0.01
                },
                $"{id}-source-heldout-{index}"))
            .ToArray();
        var provisional = new DesignConversionDpoResolvedPartition(
            id,
            [train],
            heldOut,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty);
        Assert.True(DesignConversionDpoConsumedPartitionProjectionContract.TryCreate(
            provisional,
            provisional.TrainRecords.Count,
            out var full));
        Assert.NotNull(full);
        return provisional with
        {
            TrainRecordSetSha256 = full!.TrainRecordSetSha256,
            HeldOutRecordSetSha256 = full.HeldOutRecordSetSha256,
            TrainSourceSetSha256 = full.TrainSourceSetSha256,
            HeldOutSourceSetSha256 = full.HeldOutSourceSetSha256,
            PartitionSnapshotSha256 = full.PartitionSnapshotSha256
        };
    }

    private static DesignConversionDpoTrainingRecord WithEvaluatedBridgeSourceIdentity(
        DesignConversionDpoTrainingRecord record,
        string sourceId)
    {
        var metadata = new Dictionary<string, string>(record.Provenance.Metadata, StringComparer.Ordinal)
        {
            ["source_design_id"] = sourceId
        };

        return record with
        {
            Provenance = record.Provenance with
            {
                SourceRequestId = $"request://design-conversion/{sourceId}",
                SourceManifestRef = $"manifest://design-conversion/{sourceId}",
                Metadata = metadata
            },
            TeacherProvenance = TrainingTeacherProvenance.Deterministic(sourceId)
        };
    }

    private static DesignConversionPromotedLocalModel BuildDpoTrainerPromotedLocalModel(
        params string[] extraEvidenceRefs)
        => new(
            ModelId: "dpo-trainer-local-model:test",
            ManifestId: "dpo-trainer-local-manifest",
            ManifestRef: "manifest://dpo-trainer-local",
            CheckpointPath: "local://dpo-trainer/checkpoint.json",
            CheckpointSha256: new string('a', 64),
            MachineId: DesignConversionMachine.MachineId,
            DomainTag: DesignConversionMachine.DomainTag,
            Version: "0.1.0-dpo-trainer",
            LearnedWeightArtifactCount: 1,
            LearnedWeightObservationCount: 1,
            CheckpointFreshLoadVerified: true,
            CheckpointContainsRealTrainedWeights: true,
            PromotionManifestClaimed: true,
            LocalOnly: true,
            EvidenceRefs:
            [
                DesignConversionMachine.DpoTrainerPolicyCheckpointFreshLoadedEvidence,
                DesignConversionMachine.DpoTrainerPolicyCheckpointConvertedEvidence,
                "DESIGN_CONVERSION_DPO_TRAINER_POLICY_CHECKPOINT_SOURCE_11_DPO",
                "dpo_policy_checkpoint_sha256:test-sha",
                "dpo_policy_checkpoint_file:policy-checkpoint-0001.json",
                .. extraEvidenceRefs
            ]);

    private static DesignConversionPromotedLocalModel BuildCallerForgedPromotedLocalModel(
        params string[] extraEvidenceRefs)
        => new(
            ModelId: "caller-forged-local-model",
            ManifestId: "caller-forged-local-manifest",
            ManifestRef: "manifest://caller-forged-local",
            CheckpointPath: @"C:\private\caller-forged-checkpoint.json",
            CheckpointSha256: new string('f', 64),
            MachineId: DesignConversionMachine.MachineId,
            DomainTag: DesignConversionMachine.DomainTag,
            Version: "999.999.999-caller-forged",
            LearnedWeightArtifactCount: 999,
            LearnedWeightObservationCount: 999,
            CheckpointFreshLoadVerified: true,
            CheckpointContainsRealTrainedWeights: true,
            PromotionManifestClaimed: true,
            LocalOnly: true,
            EvidenceRefs:
            [
                "caller_supplied_public_promoted_model",
                .. extraEvidenceRefs
            ]);

    private static void AssertCallerSuppliedModelIsInformationalOnly(
        DesignConversionModelServerApiResult result,
        string caseName)
    {
        Assert.False(result.ModelTrainingVerified);
        Assert.False(result.TrainingReadiness.ModelTrainingVerified);
        Assert.False(result.ModelUploadReady);
        Assert.False(result.TrainingReadiness.ModelUploadReady);
        Assert.False(result.DeploymentVerified);
        Assert.False(result.PublicHttpEndpointVerified);
        Assert.False(result.PlatformIntegrationVerified);
        Assert.False(result.ProductReady);
        Assert.False(result.TrainingReadiness.ProductReady);
        Assert.False(result.TrainedModelAuthority);
        Assert.True(result.NotTrainedModelAuthority);
        Assert.Equal(
            "promoted_local_model_informational_verification_not_proven",
            result.TrainingReadiness.Status);
        Assert.Equal(
            "local_promoted_model_informational_verification_not_proven",
            result.TrainingReadiness.PromotionStatus);
        Assert.Contains("local_promoted_checkpoint_load_not_verified", result.Warnings);
        Assert.Contains("local_model_promotion_verified=False", result.Warnings);
        Assert.Contains("local_model_verification_authority=not_proven", result.Warnings);
        Assert.Equal(bool.FalseString, result.BoundaryMetadata["model_training_verified"]);
        Assert.Equal(bool.FalseString, result.BoundaryMetadata["local_model_promotion_verified"]);
        Assert.Equal(bool.FalseString, result.BoundaryMetadata["local_promoted_checkpoint_loaded"]);
        Assert.Equal(bool.FalseString, result.BoundaryMetadata["local_promoted_checkpoint_real_weights"]);
        Assert.Equal("not_proven", result.BoundaryMetadata["local_model_verification_authority"]);
        Assert.DoesNotContain(
            "caller-forged-checkpoint.json",
            JsonSerializer.Serialize(result),
            StringComparison.OrdinalIgnoreCase);
        var informationalModel = result.PromotedLocalModel;
        Assert.NotNull(informationalModel);
        Assert.True(
            informationalModel.CheckpointFreshLoadVerified,
            $"{caseName}: the public model should remain informational, proving the negative is authority-sensitive rather than input deletion.");
    }

    private static DesignConversionModelServerApiRequest CreateRequest(string requestId)
        => new(
            RequestId: requestId,
            SourceKind: DesignConversionSourceKind.Figma,
            DesignTree: new DesignConversionModelServerPayloadRef(
                RefId: "figma-tree-ref",
                Kind: "figma-json",
                Sha256: "sha256:figma-tree",
                Payload: "{\"type\":\"FRAME\",\"name\":\"safe\"}",
                Format: "figma-json"),
            OriginalTree: new DesignConversionModelServerPayloadRef(
                RefId: "original-tree-ref",
                Kind: "figma-json",
                Sha256: "sha256:original-tree",
                Payload: "{\"type\":\"FRAME\",\"name\":\"original\"}",
                Format: "figma-json"),
            Html: "<main class=\"screen\"><h1>Dashboard</h1></main>",
            Css: ".screen{display:grid;gap:16px;}",
            Screenshots:
            [
                new DesignConversionModelServerArtifactRef(
                    ArtifactId: "screenshot-desktop",
                    Kind: "screenshot",
                    Width: 1440,
                    Height: 900,
                    Uri: "artifact://pass117/screenshot-desktop.png",
                    Sha256: "sha256:screenshot-desktop")
            ],
            DesignRenders:
            [
                new DesignConversionModelServerArtifactRef(
                    ArtifactId: "design-render-desktop",
                    Kind: "design-render",
                    Width: 1440,
                    Height: 900,
                    Uri: "artifact://pass117/design-render-desktop.png",
                    Sha256: "sha256:design-render-desktop")
            ],
            Budget: new DesignConversionModelServerBudget(
                UserId: "user-redacted",
                Tier: "pro",
                MaxImprovementCandidates: 3,
                MaxRenderQaRuns: 2,
                BudgetUsd: 1.25m),
            Metadata: new Dictionary<string, string>
            {
                ["source_manifest_ref"] = "pass117-local-source-manifest",
                ["caller"] = "xiigen-platform-local-contract-test"
            });

    private static IReadOnlyList<DesignConversionModelServerArtifactRef> CreateRequiredBrowserQaArtifacts(
        string idPrefix,
        string kind)
        =>
        [
            new DesignConversionModelServerArtifactRef(
                ArtifactId: $"{idPrefix}-mobile-360",
                Kind: kind,
                Width: 360,
                Height: 800,
                Uri: $"artifact://pass117/{idPrefix}-mobile-360.png",
                Sha256: $"sha256:{idPrefix}-mobile-360"),
            new DesignConversionModelServerArtifactRef(
                ArtifactId: $"{idPrefix}-tablet-768",
                Kind: kind,
                Width: 768,
                Height: 1024,
                Uri: $"artifact://pass117/{idPrefix}-tablet-768.png",
                Sha256: $"sha256:{idPrefix}-tablet-768"),
            new DesignConversionModelServerArtifactRef(
                ArtifactId: $"{idPrefix}-laptop-1280",
                Kind: kind,
                Width: 1280,
                Height: 720,
                Uri: $"artifact://pass117/{idPrefix}-laptop-1280.png",
                Sha256: $"sha256:{idPrefix}-laptop-1280"),
            new DesignConversionModelServerArtifactRef(
                ArtifactId: $"{idPrefix}-desktop-1920",
                Kind: kind,
                Width: 1920,
                Height: 1080,
                Uri: $"artifact://pass117/{idPrefix}-desktop-1920.png",
                Sha256: $"sha256:{idPrefix}-desktop-1920")
        ];

    private const string TrustedVisualCostRouteToken = "trusted-visual-cost-route-token-0123456789";
    private const string TrustedVisualCostRouteTenant = "tenant-visual-cost-route-alpha";

    private static DesignConversionModelServerApiRequest CreateTrustedVisualCostRouteRequest(
        string requestId,
        string userId = "platform-user-visual-cost-alpha",
        string tier = "paid",
        string monthlyCap = "125.75")
    {
        var request = CreateRequest(requestId);
        var metadata = new Dictionary<string, string>(request.Metadata ?? new Dictionary<string, string>(), StringComparer.Ordinal)
        {
            ["tenant_id"] = TrustedVisualCostRouteTenant,
            [DesignConversionModelServerApi.ServerAssignedDesignTierMetadataKey] = tier,
            [DesignConversionModelServerApi.ServerAssignedMonthlyVisualCostLimitUsdMetadataKey] = monthlyCap,
            [DesignConversionModelServerApi.DesignTierAuthorityMetadataKey] =
                DesignConversionModelServerApi.PlatformTenantConfigurationAuthority,
            [DesignConversionModelServerApi.MonthlyVisualCostLimitAuthorityMetadataKey] =
                DesignConversionModelServerApi.PlatformTenantConfigurationAuthority,
            [DesignConversionModelServerApi.UserIdentityAuthorityMetadataKey] =
                DesignConversionModelServerApi.VerifiedInboundPrincipalAuthority
        };

        return request with
        {
            Budget = request.Budget! with
            {
                UserId = userId,
                Tier = tier,
                MaxImprovementCandidates = 1,
                MaxRenderQaRuns = 2,
                BudgetUsd = 0.25m,
                Metadata = new Dictionary<string, string>()
            },
            Metadata = metadata
        };
    }

    private async Task<TrustedVisualCostRouteProbe> SubmitTrustedVisualCostRouteAsync(
        DesignConversionModelServerApiRequest request,
        bool wireVisualCostBudgetStore = true)
    {
        var events = new List<string>();
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(CreateVerdict(
            $"{request.RequestId}-trusted-visual-cost-verdict",
            0.93,
            passed: true));
        var visualJudge = new VisualCostReceiptJudge(0.0600m);
        var service = new DesignConversionInternalService(renderProvider, comparisonProvider, visualJudge, routingFlow: null);
        var evaluator = new RecordingThinClientJobEvaluator(service, events);
        var budgetStore = wireVisualCostBudgetStore
            ? new EventRecordingVisualCostBudgetStore(
                new JsonlDesignConversionVisualCostBudgetStore(CreateVisualCostBudgetStoreTempRoot(request.RequestId)),
                events)
            : null;
        var api = new DesignConversionModelServerApi(
            evaluator,
            () => DateTimeOffset.Parse("2026-07-17T09:00:00Z"),
            visualCostLedger: new SeededVisualCostLedger([]),
            visualCostBudgetStore: budgetStore,
            postEvaluationStateStore: TestPostEvaluationStateStore);
        var admission = DesignConversionModelServerRouteAdmission.Create(
            new DesignConversionModelServerRouteAdmissionOptions(
                enabled: true,
                currentToken: TrustedVisualCostRouteToken,
                tenantId: TrustedVisualCostRouteTenant),
            () => DateTimeOffset.Parse("2026-07-17T09:00:00Z"));
        var handler = new DesignConversionModelServerHttpHandler(api, admission);
        var response = await handler.HandleAsync(new DesignConversionModelServerHttpRequest(
            Method: "POST",
            Path: DesignConversionModelServerHttpHandler.EndpointPath,
            BodyJson: JsonSerializer.Serialize(request, DesignConversionModelServerHttpHandler.JsonOptions),
            ContentType: "application/json; charset=utf-8",
            Headers: new Dictionary<string, string>
            {
                [DesignConversionModelServerRouteAdmission.AuthorizationHeaderName] = "Bearer " + TrustedVisualCostRouteToken,
                [DesignConversionModelServerRouteAdmission.IdempotencyHeaderName] = request.RequestId + "-idempotency"
            }));

        return new TrustedVisualCostRouteProbe(response, evaluator, visualJudge, budgetStore, events);
    }

    private sealed record TrustedVisualCostRouteProbe(
        DesignConversionModelServerHttpResponse Response,
        RecordingThinClientJobEvaluator Evaluator,
        VisualCostReceiptJudge VisualJudge,
        EventRecordingVisualCostBudgetStore? BudgetStore,
        IReadOnlyList<string> Events);

    private sealed class EventRecordingVisualCostBudgetStore(
        JsonlDesignConversionVisualCostBudgetStore inner,
        List<string> events) :
        IDesignConversionVisualCostBudgetStore,
        IDesignConversionPostEvaluationStateRootProvider
    {
        public JsonlDesignConversionVisualCostBudgetStore Inner { get; } = inner;

        public string PostEvaluationStateRootDirectory =>
            Path.Combine(
                Inner.RootDirectory,
                JsonDesignConversionPostEvaluationStateStore.DirectoryName);

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

    private static string CreateVisualCostBudgetStoreTempRoot(string scope)
        => Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            $"model-server-api-visual-cost-budget-{scope}");

    private static DesignConversionVisualCostBudgetPeriodQuery BuildTrustedRouteBudgetPeriodQuery(
        DesignConversionInternalEvaluateRequest request)
    {
        var compositeIdentity = string.Join(
            "|",
            "tenant",
            request.Budget.Metadata["trusted_visual_cost_tenant_id_ref"],
            "user",
            request.Budget.Metadata["trusted_visual_cost_user_ref"]);
        var storeUserRef = JsonlDesignConversionVisualCostLedger.BuildPseudonymousUserRef(compositeIdentity);
        Assert.True(storeUserRef.IsSuccess, storeUserRef.ErrorMessage);
        var period = JsonlDesignConversionVisualCostLedger.BuildUtcCalendarMonthPeriod(
            DateTimeOffset.Parse("2026-07-17T09:00:00Z"));
        return new DesignConversionVisualCostBudgetPeriodQuery(
            storeUserRef.Value!,
            period.StartUtc,
            period.EndExclusiveUtc);
    }

    private static void AssertTrustedVisualCostRejectedBeforeRuntime(
        TrustedVisualCostRouteProbe probe,
        int expectedStatusCode,
        string expectedErrorCode,
        params string[] rawValues)
    {
        Assert.Equal(expectedStatusCode, probe.Response.StatusCode);
        Assert.Contains(expectedErrorCode, probe.Response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(TrustedVisualCostRouteToken, probe.Response.BodyJson, StringComparison.Ordinal);
        foreach (var rawValue in rawValues.Where(value => !string.IsNullOrWhiteSpace(value)))
            Assert.DoesNotContain(rawValue, probe.Response.BodyJson, StringComparison.Ordinal);
        Assert.Empty(probe.Evaluator.Requests);
    }

    private static string TrustedVisualCostCanonicalValue(string reservedKey)
        => reservedKey switch
        {
            DesignConversionModelServerApi.ServerAssignedDesignTierMetadataKey => "paid",
            DesignConversionModelServerApi.ServerAssignedMonthlyVisualCostLimitUsdMetadataKey => "125.75",
            DesignConversionModelServerApi.DesignTierAuthorityMetadataKey => DesignConversionModelServerApi.PlatformTenantConfigurationAuthority,
            DesignConversionModelServerApi.MonthlyVisualCostLimitAuthorityMetadataKey => DesignConversionModelServerApi.PlatformTenantConfigurationAuthority,
            DesignConversionModelServerApi.UserIdentityAuthorityMetadataKey => DesignConversionModelServerApi.VerifiedInboundPrincipalAuthority,
            _ => throw new ArgumentOutOfRangeException(nameof(reservedKey), reservedKey, "Unknown trusted visual-cost key.")
        };

    private static DesignConversionModelServerApiRequest CreateResponsiveSubmitPathRequest(string requestId)
    {
        var request = CreateRequest(requestId);
        return request with
        {
            Screenshots = request.Screenshots!
                .Append(new DesignConversionModelServerArtifactRef(
                    ArtifactId: $"{requestId}-screenshot-mobile",
                    Kind: "screenshot",
                    Width: 390,
                    Height: 844,
                    Uri: $"artifact://{requestId}/screenshot-mobile.png",
                    Sha256: $"sha256:{requestId}:screenshot-mobile"))
                .ToArray(),
            DesignRenders = request.DesignRenders!
                .Append(new DesignConversionModelServerArtifactRef(
                    ArtifactId: $"{requestId}-design-mobile",
                    Kind: "design-render",
                    Width: 390,
                    Height: 844,
                    Uri: $"artifact://{requestId}/design-mobile.png",
                    Sha256: $"sha256:{requestId}:design-mobile"))
                .ToArray()
        };
    }

    private static DesignConversionModelServerApiRequest CreateSingleAttemptRequest(string requestId)
    {
        var request = CreateRequest(requestId);
        return request with
        {
            Budget = request.Budget with { MaxImprovementCandidates = 1 }
        };
    }

    private static DesignConversionModelServerApiRequest CreateTrustedMultiAttemptRequest(string requestId)
    {
        var request = CreateRequest(requestId);
        return request with
        {
            Screenshots =
            [
                new DesignConversionModelServerArtifactRef(
                    ArtifactId: $"{requestId}-screenshot-desktop",
                    Kind: "screenshot",
                    Width: 1440,
                    Height: 900,
                    Uri: $"artifact://{requestId}/screenshot-desktop.png",
                    Sha256: $"sha256:{requestId}:screenshot-desktop"),
                new DesignConversionModelServerArtifactRef(
                    ArtifactId: $"{requestId}-screenshot-mobile",
                    Kind: "screenshot",
                    Width: 390,
                    Height: 844,
                    Uri: $"artifact://{requestId}/screenshot-mobile.png",
                    Sha256: $"sha256:{requestId}:screenshot-mobile")
            ],
            DesignRenders =
            [
                new DesignConversionModelServerArtifactRef(
                    ArtifactId: $"{requestId}-design-desktop",
                    Kind: "design-render",
                    Width: 1440,
                    Height: 900,
                    Uri: $"artifact://{requestId}/design-desktop.png",
                    Sha256: $"sha256:{requestId}:design-desktop"),
                new DesignConversionModelServerArtifactRef(
                    ArtifactId: $"{requestId}-design-mobile",
                    Kind: "design-render",
                    Width: 390,
                    Height: 844,
                    Uri: $"artifact://{requestId}/design-mobile.png",
                    Sha256: $"sha256:{requestId}:design-mobile")
            ],
            Budget = request.Budget! with
            {
                MaxImprovementCandidates = 3,
                MaxRenderQaRuns = 2
            }
        };
    }

    private static async Task<ServerOwnedVisualProviderFixture> CreateServerOwnedVisualProviderFixtureAsync(
        DesignConversionModelServerApiRequest request)
    {
        var artifactRoot = CreateServerOwnedVisualArtifactRoot();
        try
        {
            Directory.CreateDirectory(artifactRoot);
            var renderProvider = new DeterministicLocalDesignConversionPngRenderProvider(artifactRoot);
            var screenshots = await MaterializeServerOwnedVisualArtifactsAsync(
                renderProvider,
                artifactRoot,
                request,
                request.Screenshots,
                "screenshot");
            var designRenders = await MaterializeServerOwnedVisualArtifactsAsync(
                renderProvider,
                artifactRoot,
                request,
                request.DesignRenders,
                "design-render");

            return new ServerOwnedVisualProviderFixture(
                request with
                {
                    Screenshots = screenshots,
                    DesignRenders = designRenders
                },
                artifactRoot,
                renderProvider);
        }
        catch
        {
            DeleteDirectoryIfExists(artifactRoot);
            throw;
        }
    }

    private static async Task<IReadOnlyList<DesignConversionModelServerArtifactRef>?> MaterializeServerOwnedVisualArtifactsAsync(
        IDesignConversionRenderProvider renderProvider,
        string artifactRoot,
        DesignConversionModelServerApiRequest request,
        IReadOnlyList<DesignConversionModelServerArtifactRef>? artifacts,
        string role)
    {
        if (artifacts is null)
            return null;

        var materialized = new List<DesignConversionModelServerArtifactRef>(artifacts.Count);
        for (var index = 0; index < artifacts.Count; index++)
        {
            var artifact = artifacts[index];
            var resolution = new RenderQaResolution(
                ResolutionId: $"server-owned-{artifact.Width}x{artifact.Height}-{index + 1}",
                Width: artifact.Width,
                Height: artifact.Height,
                DeviceScaleFactor: 1d);
            var render = await renderProvider.RenderAsync(new DesignConversionRenderRequest(
                RenderRequestId: $"expected-{SafePathSegment(request.RequestId)}-{index + 1}-{SafePathSegment(artifact.ArtifactId)}",
                RequestId: request.RequestId,
                Html: $"<main data-server-owned-artifact=\"{SafePathSegment(artifact.ArtifactId)}\">{role}</main>",
                Css: ".server-owned-visual-provider-fixture{display:grid;gap:12px;background:#f8fafc;color:#111827;}",
                Resolution: resolution,
                Metadata: new Dictionary<string, string>
                {
                    ["server_owned_visual_provider_fixture"] = bool.TrueString,
                    ["server_owned_visual_provider_fixture_role"] = role
                }));
            Assert.True(render.IsSuccess, render.ErrorMessage);

            var image = render.Value!.Image;
            var inspected = DesignConversionBrowserPngEvidenceInspector.Inspect(image.Uri);
            Assert.True(inspected.IsSuccess, inspected.ErrorMessage);
            Assert.True(inspected.Value!.HasNonBlankPixels);
            Assert.Equal(artifact.Width, inspected.Value.PngWidth);
            Assert.Equal(artifact.Height, inspected.Value.PngHeight);
            Assert.StartsWith("sha256:", image.Sha256, StringComparison.Ordinal);
            Assert.DoesNotContain(artifactRoot, image.Sha256, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(new Uri(image.Uri).LocalPath, image.Sha256, StringComparison.OrdinalIgnoreCase);
            var metadata = (artifact.Metadata ?? new Dictionary<string, string>())
                .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
            metadata["server_owned_visual_provider_fixture"] = bool.TrueString;
            metadata["server_owned_visual_provider_fixture_role"] = role;

            materialized.Add(artifact with
            {
                Uri = image.Sha256,
                Sha256 = image.Sha256,
                Metadata = metadata
            });
        }

        return materialized.ToArray();
    }

    private DesignConversionModelServerApi CreateRuntimeShapedReceiptApi(
        EchoingRealVisualJudgeHttpHandler handler,
        Func<IDesignConversionComparisonProvider, IDesignConversionComparisonProvider>? configureComparisonProvider = null,
        IDesignConversionLocalArtifactDb? artifactDb = null,
        string? serverOwnedImageArtifactRoot = null,
        IDesignConversionRenderProvider? renderProvider = null)
    {
        var artifactRoot = serverOwnedImageArtifactRoot ?? CreateServerOwnedVisualArtifactRoot();
        Directory.CreateDirectory(artifactRoot);
        var input = new DesignConversionExternalVisualProviderConfigInput(
            Endpoint: "https://visual-judge.example/v1/judge",
            ProviderName: "openai-loopback-visual-provider",
            ModelId: "gpt-loopback-visual-verifier-v1",
            ApiKey: "trusted-runtime-shaped-receipt-test-key",
            ApiKeyHeaderName: "X-Visual-Judge-Key",
            ApiKeyScheme: string.Empty,
            ServerOwnedImageArtifactRoot: artifactRoot);
        var comparisonProvider = DesignConversionExternalVisualProviderConfigIntake.TryCreateComparisonProvider(
            input,
            handler);
        var visualJudge = DesignConversionExternalVisualProviderConfigIntake.TryCreateVisualJudge(
            input,
            handler);
        Assert.True(comparisonProvider.IsSuccess, comparisonProvider.ErrorMessage);
        Assert.True(visualJudge.IsSuccess, visualJudge.ErrorMessage);

        var configuredComparisonProvider = configureComparisonProvider?.Invoke(comparisonProvider.Value!) ??
                                           comparisonProvider.Value!;
        var service = new DesignConversionInternalService(
            renderProvider ?? new DeterministicLocalDesignConversionPngRenderProvider(artifactRoot),
            configuredComparisonProvider,
            visualJudge.Value);
        return new DesignConversionModelServerApi(
            new DesignConversionInternalServiceThinClientJobEvaluator(service),
            () => DateTimeOffset.Parse("2026-07-11T00:45:00Z"),
            artifactDb,
            visualCostLedger: CreateServerOwnedVisualCostLedger(artifactRoot),
            postEvaluationStateStore: TestPostEvaluationStateStore);
    }

    private DesignConversionModelServerApi CreateBrowserReceiptRuntimeShapedReceiptApi(
        HttpMessageHandler handler,
        Func<IDesignConversionComparisonProvider, IDesignConversionComparisonProvider>? configureComparisonProvider = null,
        Func<IDesignConversionVisualJudge, IDesignConversionVisualJudge>? configureVisualJudge = null,
        IDesignConversionLocalArtifactDb? artifactDb = null,
        string? serverOwnedCostPerRequestUsd = null,
        string? serverOwnedImageArtifactRoot = null,
        string? postEvaluationStateRoot = null,
        IDesignConversionLocalArtifactDb? teacherAuthoringArtifactDb = null)
    {
        var artifactRoot = serverOwnedImageArtifactRoot ?? CreateServerOwnedVisualArtifactRoot();
        Directory.CreateDirectory(artifactRoot);
        var input = new DesignConversionExternalVisualProviderConfigInput(
            Endpoint: "https://visual-judge.example/v1/judge",
            ProviderName: "openai-loopback-visual-provider",
            ModelId: "gpt-loopback-visual-verifier-v1",
            ApiKey: "trusted-runtime-shaped-receipt-test-key",
            ApiKeyHeaderName: "X-Visual-Judge-Key",
            ApiKeyScheme: string.Empty,
            ServerOwnedCostPerRequestUsd: serverOwnedCostPerRequestUsd,
            ServerOwnedImageArtifactRoot: artifactRoot);
        var comparisonProvider = DesignConversionExternalVisualProviderConfigIntake.TryCreateComparisonProvider(
            input,
            handler);
        var visualJudge = DesignConversionExternalVisualProviderConfigIntake.TryCreateVisualJudge(
            input,
            handler);
        Assert.True(comparisonProvider.IsSuccess, comparisonProvider.ErrorMessage);
        Assert.True(visualJudge.IsSuccess, visualJudge.ErrorMessage);

        var configuredComparisonProvider = configureComparisonProvider?.Invoke(comparisonProvider.Value!) ??
                                           comparisonProvider.Value!;
        var configuredVisualJudge = configureVisualJudge?.Invoke(visualJudge.Value!) ?? visualJudge.Value!;
        var service = new DesignConversionInternalService(
            new BrowserBackedDesignConversionRenderProvider(new TrustedBrowserReceiptBrowserAdapter(artifactRoot)),
            configuredComparisonProvider,
            configuredVisualJudge);
        var visualCostLedger = CreateServerOwnedVisualCostLedger(artifactRoot);
        var visualCostBudgetStore = string.IsNullOrWhiteSpace(postEvaluationStateRoot)
            ? null
            : new JsonlDesignConversionVisualCostBudgetStore(postEvaluationStateRoot);
        if (teacherAuthoringArtifactDb is not null)
        {
            var composition = DesignConversionImprovementLoopRuntimeCompositionFactory.Create(
                new DesignConversionInternalServiceThinClientJobEvaluator(service),
                new DesignConversionDpoTeacherAuthoringCandidateStrategy(
                    new FailingUnexpectedTeacherAuthoringFallbackCandidateStrategy(),
                    teacherAuthoringArtifactDb));
            var runtime = new DesignConversionThinClientJobRuntime(
                composition.Evaluator,
                () => DateTimeOffset.Parse("2026-07-11T00:50:00Z"),
                artifactDb,
                visualCostLedger,
                visualCostBudgetStore,
                postEvaluationStateStore: TestPostEvaluationStateStore,
                rejectedEvaluationSource: composition.RejectedEvaluationSource);
            return new DesignConversionModelServerApi(runtime);
        }

        return new DesignConversionModelServerApi(
            new DesignConversionInternalServiceThinClientJobEvaluator(service),
            () => DateTimeOffset.Parse("2026-07-11T00:50:00Z"),
            artifactDb,
            visualCostLedger: visualCostLedger,
            visualCostBudgetStore: visualCostBudgetStore,
            postEvaluationStateStore: TestPostEvaluationStateStore);
    }

    private static DesignConversionModelServerApiRequest AddUnrenderedMobileViewport(
        DesignConversionModelServerApiRequest request)
        => request with
        {
            Screenshots = request.Screenshots!.Append(new DesignConversionModelServerArtifactRef(
                ArtifactId: "screenshot-mobile",
                Kind: "screenshot",
                Width: 390,
                Height: 844,
                Uri: "artifact://pass117/screenshot-mobile.png",
                Sha256: "sha256:screenshot-mobile")).ToArray(),
            DesignRenders = request.DesignRenders!.Append(new DesignConversionModelServerArtifactRef(
                ArtifactId: "design-render-mobile",
                Kind: "design-render",
                Width: 390,
                Height: 844,
                Uri: "artifact://pass117/design-render-mobile.png",
                Sha256: "sha256:design-render-mobile")).ToArray(),
            Budget = request.Budget with { MaxRenderQaRuns = 1 }
        };

    private static void AssertTrustedReceiptNetworkBoundary(
        IReadOnlyDictionary<string, string> metadata,
        string expectedJobId,
        string expectedRootRequestId,
        string expectedProvider,
        string expectedModel)
    {
        Assert.Equal(bool.TrueString, metadata["external_visual_ai_executed"]);
        AssertTrustedVisualProviderVerdictMetadata(metadata);
        Assert.Equal(bool.TrueString, metadata["visual_ai_executed"]);
        Assert.Equal(bool.FalseString, metadata["visual_ai_not_executed"]);
        Assert.Equal(bool.TrueString, metadata["network_call_executed"]);
        Assert.Equal(bool.TrueString, metadata["external_network_call_executed"]);
        Assert.Equal(bool.TrueString, metadata["external_provider_called"]);
        Assert.Equal(bool.TrueString, metadata["real_external_provider_executed"]);
        Assert.Equal(bool.TrueString, metadata["provider_response_identity_verified"]);
        Assert.Equal(bool.TrueString, metadata["provider_response_schema_verified"]);
        Assert.Equal(
            bool.TrueString,
            metadata[DesignConversionModelServerNetworkBoundary.ServerOwnedVisualProviderNetworkCallVerifiedKey]);
        Assert.Equal(
            "comparison_artifact_provider_identity_schema_and_network_evidence_verified",
            metadata[DesignConversionModelServerNetworkBoundary.ServerOwnedVisualProviderNetworkCallVerificationKey]);
        Assert.Equal(bool.TrueString, metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptKey]);
        Assert.Equal(
            ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVersion,
            metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVersionKey]);
        Assert.StartsWith(
            $"{expectedJobId}:comparison-request:",
            metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptIdKey],
            StringComparison.Ordinal);
        Assert.EndsWith(
            ":server-visual-provider-call-receipt",
            metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptIdKey],
            StringComparison.Ordinal);
        Assert.Equal(expectedJobId, metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptJobIdKey]);
        Assert.Equal(expectedRootRequestId, metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptRequestIdKey]);
        Assert.Equal(expectedRootRequestId, metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptRootRequestIdKey]);
        Assert.StartsWith(
            $"{expectedJobId}:comparison-request:",
            metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptJudgeRequestIdKey],
            StringComparison.Ordinal);
        Assert.StartsWith(
            $"{expectedJobId}:comparison-request:",
            metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptComparisonRequestIdKey],
            StringComparison.Ordinal);
        Assert.StartsWith(
            $"{expectedJobId}:visual-judge-request:",
            metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVisualJudgeRequestIdKey],
            StringComparison.Ordinal);
        Assert.Contains(
            $"{expectedJobId}:comparison-request:",
            metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptProviderRequestIdKey],
            StringComparison.Ordinal);
        Assert.Contains(
            $"{expectedJobId}:visual-judge-request:",
            metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptProviderRequestIdKey],
            StringComparison.Ordinal);
        var comparisonArtifactId =
            metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptComparisonArtifactIdKey];
        Assert.StartsWith($"{expectedJobId}:comparison-request:", comparisonArtifactId, StringComparison.Ordinal);
        Assert.Contains(
            $":{ExternalHttpDesignConversionVisualJudge.ServerOwnedVisualResultSegment}:comparison-artifact",
            comparisonArtifactId,
            StringComparison.Ordinal);
        Assert.NotEqual("-result:comparison-artifact", comparisonArtifactId);
        var visualResultId =
            metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVisualResultIdKey];
        Assert.StartsWith($"{expectedJobId}:comparison-request:", visualResultId, StringComparison.Ordinal);
        Assert.Contains(
            $":{ExternalHttpDesignConversionVisualJudge.ServerOwnedVisualResultSegment}",
            visualResultId,
            StringComparison.Ordinal);
        Assert.NotEqual("-result", visualResultId);
        Assert.Equal(expectedProvider, metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptProviderNameKey]);
        Assert.Equal(expectedModel, metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptProviderModelKey]);
        Assert.Equal(bool.TrueString, metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptExternalCallExecutedKey]);
        Assert.Equal(bool.TrueString, metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseIdentityVerifiedKey]);
        Assert.Equal(bool.TrueString, metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseSchemaVerifiedKey]);
    }

    private static void AssertRuntimeDeferredTrustedVisualFidelityMetadata(IReadOnlyDictionary<string, string> metadata)
    {
        Assert.Equal(DesignConversionThinClientStatus.NotReady, metadata["runtime_contract_status"]);
        Assert.Equal(bool.FalseString, metadata["browser_e2e_verified"]);
        Assert.Equal("runtime_not_completed", metadata["browser_e2e_failure_reason"]);
        AssertVisualFidelityFalse(metadata, "runtime_not_completed");
        Assert.Equal(
            "server_owned_browser_render_and_visual_provider_receipts",
            metadata["visual_fidelity_verification_authority"]);
        Assert.Equal("0.973", metadata["visual_fidelity_score"]);
        Assert.Equal("0.9", metadata["visual_fidelity_acceptance_threshold"]);
        Assert.Equal("0.917", metadata["visual_fidelity_confidence"]);
        Assert.Equal("1", metadata["visual_fidelity_issue_count"]);
        Assert.Equal("0", metadata["visual_fidelity_blocking_issue_count"]);
        Assert.Equal("1", metadata["visual_fidelity_canonical_result_count"]);
        Assert.Equal("1", metadata["visual_fidelity_bound_artifact_count"]);
        Assert.Equal(bool.TrueString, metadata["visual_fidelity_score_threshold_met"]);
        Assert.Equal(bool.TrueString, metadata["visual_fidelity_passed"]);
        Assert.Equal(bool.TrueString, metadata["visual_fidelity_provider_receipt_verified"]);
        Assert.Equal(bool.TrueString, metadata["visual_fidelity_response_identity_verified"]);
        Assert.Equal(bool.TrueString, metadata["visual_fidelity_response_schema_verified"]);
        Assert.Equal(
            "server_owned_model_server_browser_render_receipts",
            metadata["browser_e2e_verification_authority"]);
        Assert.Equal(
            "not_verified_by_server_owned_browser_render_receipts",
            metadata["browser_e2e_verification_state"]);
        Assert.Equal("2", metadata["browser_e2e_required_viewport_count"]);
        Assert.Equal("2", metadata["browser_e2e_observed_viewport_count"]);
        Assert.Equal("2", metadata["browser_e2e_verified_render_artifact_count"]);
        Assert.Equal("2", metadata["browser_e2e_png_signature_verified_count"]);
        Assert.Equal("2", metadata["browser_e2e_sha256_verified_count"]);
        Assert.Equal("2", metadata["browser_e2e_nonblank_verified_count"]);
        Assert.Equal("2", metadata["browser_e2e_dimension_verified_count"]);
        Assert.Equal("2", metadata["browser_e2e_browser_provenance_count"]);
    }

    private static void AssertNoFalseBoundaryMarkers(IEnumerable<string> values, params string[] markers)
    {
        foreach (var marker in markers)
            Assert.DoesNotContain($"{marker}=false", values);
    }

    private static void AssertContainsFalseBoundaryMarker(IEnumerable<string> values, string marker)
        => Assert.Contains($"{marker}=false", values);

    private static string FormatBoundaryMetadata(IReadOnlyDictionary<string, string> metadata)
        => string.Join(
            Environment.NewLine,
            metadata
                .OrderBy(pair => pair.Key, StringComparer.Ordinal)
                .Select(pair => $"{pair.Key}={pair.Value}"));

    private static void AssertVisualFidelityFalse(
        IReadOnlyDictionary<string, string> metadata,
        string expectedReason)
    {
        Assert.Equal(bool.FalseString, metadata["visual_fidelity_verified"]);
        Assert.Equal(expectedReason, metadata["visual_fidelity_failure_reason"]);
        Assert.Equal(
            "not_verified_by_server_owned_browser_and_visual_provider_receipts",
            metadata["visual_fidelity_verification_state"]);
    }

    private static void AssertVisualFidelityNegativeMatrixDiagnostics(
        IReadOnlyDictionary<string, string> metadata,
        VisualFidelityNegativeMode mode)
    {
        Assert.Equal(ExpectedVisualFidelityNegativeRuntimeStatus(mode), metadata["runtime_contract_status"]);
        Assert.Equal(bool.FalseString, metadata["browser_e2e_verified"]);
        Assert.Equal("runtime_not_completed", metadata["browser_e2e_failure_reason"]);

        switch (mode)
        {
            case VisualFidelityNegativeMode.ReceiptMismatch:
                Assert.Equal(bool.FalseString, metadata["visual_fidelity_provider_receipt_verified"]);
                break;
            case VisualFidelityNegativeMode.ResponseIdentityMismatch:
                Assert.Equal(bool.FalseString, metadata["visual_fidelity_response_identity_verified"]);
                break;
            case VisualFidelityNegativeMode.ResponseSchemaMismatch:
                Assert.Equal(bool.FalseString, metadata["visual_fidelity_response_schema_verified"]);
                break;
            case VisualFidelityNegativeMode.NaNScore:
                Assert.Equal("none", metadata["visual_fidelity_score"]);
                Assert.Equal(bool.FalseString, metadata["visual_fidelity_score_threshold_met"]);
                break;
            case VisualFidelityNegativeMode.BelowThreshold:
                Assert.Equal("0.82", metadata["visual_fidelity_score"]);
                Assert.Equal(bool.FalseString, metadata["visual_fidelity_score_threshold_met"]);
                Assert.Equal(bool.FalseString, metadata["visual_fidelity_passed"]);
                break;
            case VisualFidelityNegativeMode.MissingConfidence:
                Assert.Equal("none", metadata["visual_fidelity_confidence"]);
                break;
            case VisualFidelityNegativeMode.BlockingIssue:
                Assert.Equal("1", metadata["visual_fidelity_blocking_issue_count"]);
                break;
            case VisualFidelityNegativeMode.IncompleteViewport:
                Assert.Equal(bool.FalseString, metadata["multi_viewport_complete"]);
                break;
        }
    }

    private static string ExpectedVisualFidelityNegativeRuntimeStatus(VisualFidelityNegativeMode mode)
        => mode == VisualFidelityNegativeMode.IncompleteViewport
            ? DesignConversionThinClientStatus.Quarantined
            : DesignConversionThinClientStatus.NotReady;

    private static void AssertTrustedReceiptNetworkBoundary(
        JsonElement metadata,
        string expectedJobId,
        string expectedRootRequestId)
    {
        Assert.Equal(bool.TrueString, metadata.GetProperty("network_call_executed").GetString());
        AssertTrustedVisualProviderVerdictMetadata(metadata);
        Assert.Equal(bool.TrueString, metadata.GetProperty("external_network_call_executed").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("external_provider_called").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("real_external_provider_executed").GetString());
        Assert.Equal(
            bool.TrueString,
            metadata.GetProperty(DesignConversionModelServerNetworkBoundary.ServerOwnedVisualProviderNetworkCallVerifiedKey).GetString());
        Assert.Equal(
            bool.TrueString,
            metadata.GetProperty(ExternalHttpDesignConversionVisualJudge.ServerCallReceiptKey).GetString());
        Assert.StartsWith(
            $"{expectedJobId}:comparison-request:",
            metadata.GetProperty(ExternalHttpDesignConversionVisualJudge.ServerCallReceiptIdKey).GetString(),
            StringComparison.Ordinal);
        Assert.EndsWith(
            ":server-visual-provider-call-receipt",
            metadata.GetProperty(ExternalHttpDesignConversionVisualJudge.ServerCallReceiptIdKey).GetString(),
            StringComparison.Ordinal);
        Assert.Equal(
            expectedJobId,
            metadata.GetProperty(ExternalHttpDesignConversionVisualJudge.ServerCallReceiptJobIdKey).GetString());
        Assert.Equal(
            expectedRootRequestId,
            metadata.GetProperty(ExternalHttpDesignConversionVisualJudge.ServerCallReceiptRequestIdKey).GetString());
        Assert.Equal(
            expectedRootRequestId,
            metadata.GetProperty(ExternalHttpDesignConversionVisualJudge.ServerCallReceiptRootRequestIdKey).GetString());
        if (metadata.TryGetProperty(
                ExternalHttpDesignConversionVisualJudge.ServerCallReceiptProviderRequestIdKey,
                out var providerRequestId))
        {
            Assert.Contains(
                $"{expectedJobId}:comparison-request:",
                providerRequestId.GetString(),
                StringComparison.Ordinal);
            Assert.Contains(
                $"{expectedJobId}:visual-judge-request:",
                providerRequestId.GetString(),
                StringComparison.Ordinal);
        }

        Assert.StartsWith(
            $"{expectedJobId}:comparison-request:",
            metadata.GetProperty(ExternalHttpDesignConversionVisualJudge.ServerCallReceiptComparisonRequestIdKey).GetString(),
            StringComparison.Ordinal);
        Assert.StartsWith(
            $"{expectedJobId}:visual-judge-request:",
            metadata.GetProperty(ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVisualJudgeRequestIdKey).GetString(),
            StringComparison.Ordinal);
        Assert.StartsWith(
            $"{expectedJobId}:comparison-request:",
            metadata.GetProperty(ExternalHttpDesignConversionVisualJudge.ServerCallReceiptComparisonArtifactIdKey).GetString(),
            StringComparison.Ordinal);
        Assert.Contains(
            $":{ExternalHttpDesignConversionVisualJudge.ServerOwnedVisualResultSegment}:comparison-artifact",
            metadata.GetProperty(ExternalHttpDesignConversionVisualJudge.ServerCallReceiptComparisonArtifactIdKey).GetString(),
            StringComparison.Ordinal);
        Assert.StartsWith(
            $"{expectedJobId}:comparison-request:",
            metadata.GetProperty(ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVisualResultIdKey).GetString(),
            StringComparison.Ordinal);
        Assert.Contains(
            $":{ExternalHttpDesignConversionVisualJudge.ServerOwnedVisualResultSegment}",
            metadata.GetProperty(ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVisualResultIdKey).GetString(),
            StringComparison.Ordinal);
        Assert.Equal(
            bool.TrueString,
            metadata.GetProperty(ExternalHttpDesignConversionVisualJudge.ServerCallReceiptExternalCallExecutedKey).GetString());
        Assert.Equal(
            bool.TrueString,
            metadata.GetProperty(ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseIdentityVerifiedKey).GetString());
        Assert.Equal(
            bool.TrueString,
            metadata.GetProperty(ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseSchemaVerifiedKey).GetString());
    }

    private static void AssertUntrustedReceiptNetworkBoundary(IReadOnlyDictionary<string, string> metadata)
    {
        AssertUntrustedReceiptNetworkAuthority(metadata);
        Assert.Equal(bool.FalseString, metadata["visual_fidelity_verified"]);
    }

    private static IReadOnlyDictionary<string, string> InvokeBuildVisualProviderMetadata(
        IReadOnlyList<DesignConversionQaComparisonArtifact> artifacts,
        IReadOnlyList<DesignConversionVisualJudgeResult> visualJudgeResults,
        string? currentJobId,
        string? currentRootRequestId)
    {
        var method = typeof(DesignConversionModelServerApi).GetMethod(
            "BuildVisualProviderMetadata",
            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        Assert.NotNull(method);

        var result = method!.Invoke(
            obj: null,
            parameters: [artifacts, visualJudgeResults, currentJobId, currentRootRequestId]);
        return Assert.IsAssignableFrom<IReadOnlyDictionary<string, string>>(result);
    }

    private static void AssertUntrustedReceiptNetworkAuthority(IReadOnlyDictionary<string, string> metadata)
    {
        Assert.Equal(bool.FalseString, metadata["network_call_executed"]);
        Assert.Equal(bool.FalseString, metadata["external_network_call_executed"]);
        Assert.Equal(bool.FalseString, metadata["external_provider_called"]);
        Assert.Equal(bool.FalseString, metadata["real_external_provider_executed"]);
        AssertNoTrustedVisualProviderVerdictMetadata(metadata);
        Assert.Equal(bool.FalseString, metadata["provider_response_identity_verified"]);
        Assert.Equal(bool.FalseString, metadata["provider_response_schema_verified"]);
        Assert.Equal(
            bool.FalseString,
            metadata[DesignConversionModelServerNetworkBoundary.ServerOwnedVisualProviderNetworkCallVerifiedKey]);
        Assert.Equal(
            "not_verified",
            metadata[DesignConversionModelServerNetworkBoundary.ServerOwnedVisualProviderNetworkCallVerificationKey]);
        Assert.Equal(bool.FalseString, metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptKey]);
        Assert.Equal(bool.FalseString, metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptExternalCallExecutedKey]);
        Assert.Equal(bool.FalseString, metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseIdentityVerifiedKey]);
        Assert.Equal(bool.FalseString, metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseSchemaVerifiedKey]);
    }

    private static void AssertNoAuthorityReceiptTuple(IReadOnlyDictionary<string, string> metadata)
    {
        Assert.All(
            new[]
            {
                ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVersionKey,
                ExternalHttpDesignConversionVisualJudge.ServerCallReceiptIdKey,
                ExternalHttpDesignConversionVisualJudge.ServerCallReceiptJobIdKey,
                ExternalHttpDesignConversionVisualJudge.ServerCallReceiptJudgeRequestIdKey,
                ExternalHttpDesignConversionVisualJudge.ServerCallReceiptRequestIdKey,
                ExternalHttpDesignConversionVisualJudge.ServerCallReceiptRootRequestIdKey,
                ExternalHttpDesignConversionVisualJudge.ServerCallReceiptProviderRequestIdKey,
                ExternalHttpDesignConversionVisualJudge.ServerCallReceiptComparisonRequestIdKey,
                ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVisualJudgeRequestIdKey,
                ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVisualResultIdKey,
                ExternalHttpDesignConversionVisualJudge.ServerCallReceiptComparisonArtifactIdKey,
                ExternalHttpDesignConversionVisualJudge.ServerCallReceiptProviderNameKey,
                ExternalHttpDesignConversionVisualJudge.ServerCallReceiptProviderModelKey,
                ExternalHttpDesignConversionVisualJudge.ServerCallReceiptEndpointHostKey
            },
            key => Assert.Equal("none", metadata[key]));
    }

    private static void AssertTrustedVisualProviderVerdictMetadata(IReadOnlyDictionary<string, string> metadata)
    {
        Assert.Equal(
            DesignConversionVisualAiProviderSelectionPolicy.VerifiedCloudExecutionPolicyName,
            metadata[DesignConversionVisualAiProviderSelectionPolicy.ProviderSelectionPolicyKey]);
        Assert.Equal(
            bool.TrueString,
            metadata[DesignConversionVisualAiProviderSelectionPolicy.RealCloudProviderClaimedKey]);
        Assert.Equal(bool.TrueString, metadata["external_ai_visual_judge_executed"]);
        Assert.Equal("0.973", metadata["visual_provider_visual_judge_score"]);
        Assert.Equal(bool.TrueString, metadata["visual_provider_visual_judge_passed"]);
        Assert.Equal("0.917", metadata["visual_provider_visual_judge_confidence"]);
        Assert.Equal("1", metadata["visual_provider_visual_judge_issue_count"]);
    }

    private static void AssertTrustedVisualProviderVerdictMetadata(JsonElement metadata)
    {
        Assert.Equal(
            DesignConversionVisualAiProviderSelectionPolicy.VerifiedCloudExecutionPolicyName,
            metadata.GetProperty(DesignConversionVisualAiProviderSelectionPolicy.ProviderSelectionPolicyKey).GetString());
        Assert.Equal(
            bool.TrueString,
            metadata.GetProperty(DesignConversionVisualAiProviderSelectionPolicy.RealCloudProviderClaimedKey).GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("external_ai_visual_judge_executed").GetString());
        Assert.Equal("0.973", metadata.GetProperty("visual_provider_visual_judge_score").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("visual_provider_visual_judge_passed").GetString());
        Assert.Equal("0.917", metadata.GetProperty("visual_provider_visual_judge_confidence").GetString());
        Assert.Equal("1", metadata.GetProperty("visual_provider_visual_judge_issue_count").GetString());
    }

    private static void AssertNoTrustedVisualProviderVerdictMetadata(IReadOnlyDictionary<string, string> metadata)
    {
        Assert.NotEqual(
            DesignConversionVisualAiProviderSelectionPolicy.VerifiedCloudExecutionPolicyName,
            metadata[DesignConversionVisualAiProviderSelectionPolicy.ProviderSelectionPolicyKey]);
        Assert.Equal(
            bool.FalseString,
            metadata[DesignConversionVisualAiProviderSelectionPolicy.RealCloudProviderClaimedKey]);
        Assert.Equal(bool.FalseString, metadata["external_ai_visual_judge_executed"]);
        Assert.DoesNotContain("visual_provider_visual_judge_score", metadata.Keys);
        Assert.DoesNotContain("visual_provider_visual_judge_passed", metadata.Keys);
        Assert.DoesNotContain("visual_provider_visual_judge_confidence", metadata.Keys);
        Assert.DoesNotContain("visual_provider_visual_judge_issue_count", metadata.Keys);
    }

    public static IEnumerable<object[]> ForgedLearnedStatusProvenanceCases()
    {
        yield return
        [
            "forged-learned-label",
            new Dictionary<string, string>
            {
                ["LearnedInferenceApplied"] = bool.TrueString,
                ["learned_inference_applied"] = bool.TrueString,
                ["server_trained"] = bool.TrueString,
                ["product_ready"] = bool.TrueString
            }
        ];

        yield return
        [
            "cross-tenant-replay-learned-label",
            new Dictionary<string, string>
            {
                ["learned_inference_applied"] = bool.TrueString,
                ["model_activation_tenant_id_ref"] = "tenant:other",
                ["runtime_tenant_id_ref"] = "tenant:current",
                ["local_promoted_model_id"] = "tenant-other-model",
                ["product_ready"] = bool.TrueString
            }
        ];

        yield return
        [
            "changed-checkpoint-artifact-digest-label",
            new Dictionary<string, string>
            {
                ["learned_inference_applied"] = bool.TrueString,
                ["local_promoted_checkpoint_sha256"] = new string('f', 64),
                ["source_artifact_digest"] = "sha256:" + new string('e', 64),
                ["runtime_artifact_digest"] = "sha256:" + new string('d', 64),
                ["promotion_ready"] = bool.TrueString
            }
        ];

        yield return
        [
            "status-only-promotion-ready-claim",
            new Dictionary<string, string>
            {
                ["LearnedInferenceApplied"] = bool.TrueString,
                ["model_training_verified"] = bool.TrueString,
                ["local_promotion_ready"] = bool.TrueString,
                ["promotion_status"] = "promotion_ready_status_only",
                ["product_ready"] = bool.TrueString
            }
        ];

        yield return
        [
            "trained-model-authority-status-claim",
            new Dictionary<string, string>
            {
                ["trained_model_authority"] = bool.TrueString
            }
        ];

        yield return
        [
            "learned-evidence-status-claim",
            new Dictionary<string, string>
            {
                ["learned_evidence"] = bool.TrueString
            }
        ];

        yield return
        [
            "checkpoint-promoted-status-claim",
            new Dictionary<string, string>
            {
                ["checkpoint_promoted"] = bool.TrueString
            }
        ];

        yield return
        [
            "unknown-future-authority-status-claim",
            new Dictionary<string, string>
            {
                ["future_authority_claim"] = bool.TrueString
            }
        ];
    }

    private static SubmittedDesignConversionRequest CreateSubmittedRuntimeRequest(
        string requestId,
        IReadOnlyDictionary<string, string> provenance)
        => CreateSubmittedRuntimeRequest(CreateRequest(requestId), provenance);

    private static SubmittedDesignConversionRequest CreateSubmittedRuntimeRequest(
        DesignConversionModelServerApiRequest request,
        IReadOnlyDictionary<string, string> provenance)
    {
        var budget = request.Budget!;
        var mergedProvenance = new Dictionary<string, string>(provenance, StringComparer.Ordinal)
        {
            ["api_shape"] = DesignConversionModelServerApi.ApiShape,
            ["runtime_surface"] = DesignConversionModelServerApi.RuntimeSurface,
            ["submission_mapper"] = nameof(DesignConversionModelServerApi),
            ["product_ready"] = provenance.TryGetValue("product_ready", out var productReady)
                ? productReady
                : bool.FalseString
        };

        return new SubmittedDesignConversionRequest(
            RequestId: request.RequestId,
            SourceKind: request.SourceKind,
            Html: request.Html,
            Css: request.Css,
            DesignTreePayload: ToSubmittedTreePayload(request.DesignTree),
            OriginalTreePayload: ToSubmittedTreePayload(request.OriginalTree),
            Screenshots: request.Screenshots!.Select(ToSubmittedArtifact).ToArray(),
            DesignRenders: request.DesignRenders!.Select(ToSubmittedArtifact).ToArray(),
            Budget: new DesignConversionBudgetMetadata(
                UserId: budget.UserId,
                Tier: budget.Tier,
                MaxImprovementCandidates: budget.MaxImprovementCandidates,
                MaxRenderQaRuns: budget.MaxRenderQaRuns,
                BudgetUsd: budget.BudgetUsd,
                Metadata: budget.Metadata ?? new Dictionary<string, string>(),
                RenderQaPassThreshold: budget.RenderQaPassThreshold,
                MaxImprovementIterations: budget.MaxImprovementIterations,
                MaxTeacherCalls: budget.MaxTeacherCalls),
            Provenance: mergedProvenance);
    }

    private static DesignConversionThinClientJobRuntime CreateTeacherAuthoredDirectRuntime(
        IDesignConversionLocalArtifactDb artifactDb)
    {
        var composition = DesignConversionImprovementLoopRuntimeCompositionFactory.Create(
            new CanonicalTeacherAuthoredMeasuredEvaluator(),
            new DesignConversionDpoTeacherAuthoringCandidateStrategy(
                new FailingUnexpectedTeacherAuthoringFallbackCandidateStrategy(),
                artifactDb));
        return new DesignConversionThinClientJobRuntime(
            composition.Evaluator,
            () => DateTimeOffset.Parse("2026-07-02T10:00:00Z"),
            artifactDb,
            rejectedEvaluationSource: composition.RejectedEvaluationSource);
    }

    private static string TruthyProvenanceKeys(IReadOnlyDictionary<string, string> provenance)
        => string.Join(
            ",",
            provenance
                .Where(pair => bool.TryParse(pair.Value, out var parsed) && parsed)
                .Select(pair => pair.Key)
                .Order(StringComparer.Ordinal));

    private static DesignTreePayload ToSubmittedTreePayload(DesignConversionModelServerPayloadRef payload)
        => new(
            PayloadId: payload.RefId,
            Format: payload.Format ?? payload.Kind,
            Content: payload.Payload ?? payload.RefId,
            Sha256: payload.Sha256 ?? "sha256:missing",
            Metadata: payload.Metadata ?? new Dictionary<string, string>());

    private static DesignRenderArtifact ToSubmittedArtifact(DesignConversionModelServerArtifactRef artifact)
        => new(
            ArtifactId: artifact.ArtifactId,
            Kind: artifact.Kind,
            Width: artifact.Width,
            Height: artifact.Height,
            Uri: artifact.Uri,
            Sha256: artifact.Sha256,
            Metadata: artifact.Metadata ?? new Dictionary<string, string>());

    private static DesignConversionModelServerLocalCallerPackage CreateLocalCallerPackage(string requestId)
    {
        var request = CreateRequest(requestId);
        return new DesignConversionModelServerLocalCallerPackage(
            RequestId: request.RequestId,
            SourceKind: request.SourceKind.ToString(),
            DesignTree: request.DesignTree,
            OriginalTree: request.OriginalTree,
            Html: request.Html,
            Css: request.Css,
            Screenshots: request.Screenshots,
            DesignRenders: request.DesignRenders,
            Budget: request.Budget,
            Metadata: request.Metadata);
    }

    private static VisualVerdict CreateVerdict(string verdictId, double score, bool passed)
        => new(
            VerdictId: verdictId,
            Score: score,
            Defects: passed
                ? []
                :
                [
                    new VisualDefect(
                        DefectId: $"{verdictId}:spacing",
                        Category: "spacing",
                        Severity: "major",
                        Description: "Spacing differs from expected render.",
                        X: 16,
                        Y: 24,
                        Width: 80,
                        Height: 20,
                        EvidenceImageIds: ["expected", "actual"])
                ],
            EvidenceRefs: [$"verdict:{verdictId}"],
            Passed: passed);

    private static DesignConversionInternalEvaluationResult CreateServerOwnedDirectDpoAuthorityEvaluation(
        DesignConversionInternalEvaluateRequest request)
    {
        var evaluation = DesignConversionThinClientJobRuntimeTests.CreateMeasuredRuntimeDpoEvaluation(
            canPersistTrainingCandidate: false,
            jobId: request.JobId,
            requestId: request.RequestId,
            outputId: $"{request.JobId}-direct-dpo-authority-output",
            candidateId: $"{request.JobId}-direct-dpo-authority-candidate",
            verdictId: $"{request.JobId}-direct-dpo-authority-verdict",
            score: 0.97,
            passed: true,
            htmlSuffix: "direct-dpo-authority");
        var authorityEvidenceRefs = new[]
        {
            DesignConversionMachine.LearnedInferenceAppliedEvidence,
            DesignConversionMachine.DpoTrainerPolicyCheckpointRuntimeScoringEvidence,
            DesignConversionMachine.DpoPolicyDirectInferenceAcceptanceAuthorityEvidence,
            DesignConversionMachine.DpoPolicyDirectInferenceAcceptanceAuthorityScoreEvidence,
            DesignConversionMachine.DpoPolicyDirectInferenceAcceptanceAuthorityConfiguredEvidence,
            DesignConversionMachine.DpoRuntimeCheckpointAccuracyFromHeldOutEvaluationEvidence,
            DesignConversionMachine.DpoRuntimeCheckpointAccuracyCurrentCycleHeldOutEvaluationScopeEvidence
        };
        var candidateMetadata = new Dictionary<string, string>(
            evaluation.RuntimeFlowResult.Candidate.Metadata,
            StringComparer.Ordinal)
        {
            ["direct_dpo_policy_inference_applied"] = bool.TrueString,
            ["direct_dpo_policy_score_authority"] = bool.TrueString,
            ["direct_dpo_policy_acceptance_authority"] = bool.TrueString,
            ["baseline_visual_verdict_acceptance_authority"] = bool.FalseString
        };
        DesignConversionModelAuthorityBoundary.ApplyTrustedSnapshot(
            candidateMetadata,
            DesignConversionModelAuthorityBoundary.TrustedDirectDpoRuntimeFlowAuthority);
        var candidate = evaluation.RuntimeFlowResult.Candidate with
        {
            EvidenceRefs = evaluation.RuntimeFlowResult.Candidate.EvidenceRefs
                .Concat(authorityEvidenceRefs)
                .Distinct(StringComparer.Ordinal)
                .ToArray(),
            Metadata = candidateMetadata
        };
        var output = evaluation.RuntimeFlowResult.Output with
        {
            EvidenceRefs = evaluation.RuntimeFlowResult.Output.EvidenceRefs
                .Concat(authorityEvidenceRefs)
                .Distinct(StringComparer.Ordinal)
                .ToArray(),
            Verdict = evaluation.RuntimeFlowResult.Output.Verdict with
            {
                EvidenceRefs = evaluation.RuntimeFlowResult.Output.Verdict.EvidenceRefs
                    .Concat(authorityEvidenceRefs)
                    .Distinct(StringComparer.Ordinal)
                    .ToArray()
            }
        };
        var runtimeFlow = evaluation.RuntimeFlowResult with
        {
            Candidate = candidate,
            Output = output,
            ImproverOutput = evaluation.RuntimeFlowResult.ImproverOutput with
            {
                Candidate = candidate,
                Output = output
            },
            EvidenceRefs = evaluation.RuntimeFlowResult.EvidenceRefs
                .Concat(authorityEvidenceRefs)
                .Distinct(StringComparer.Ordinal)
                .ToArray()
        };

        return evaluation with
        {
            RuntimeFlowResult = runtimeFlow,
            Verdict = output.Verdict,
            EvidenceRefs = evaluation.EvidenceRefs
                .Concat(authorityEvidenceRefs)
                .Distinct(StringComparer.Ordinal)
                .ToArray(),
            Warnings = evaluation.Warnings
                .Append("DESIGN_CONVERSION_RUNTIME_DIRECT_DPO_POLICY_SCORE_ACCEPTANCE_AUTHORITY_ACTIVE")
                .Distinct(StringComparer.Ordinal)
                .ToArray()
        };
    }

    private static DesignConversionVisualCostLedgerRecord CreateVisualCostLedgerRecordForApiTest(
        string jobId,
        string requestId,
        decimal totalCostUsd,
        bool measurementComplete,
        int missingMeasurementCount,
        int evaluationAttemptCount,
        string costSource,
        DateTimeOffset recordedAtUtc)
    {
        var evaluationJobRef = JsonlDesignConversionVisualCostLedger.BuildCanonicalEvaluationJobRef(jobId);
        var submittedRequestRef = JsonlDesignConversionVisualCostLedger.BuildCanonicalSubmittedRequestRef(requestId);
        var period = JsonlDesignConversionVisualCostLedger.BuildUtcCalendarMonthPeriod(recordedAtUtc);
        var accounting = new DesignConversionVisualCostAccounting(
            TotalCostUsd: totalCostUsd,
            MeasurementComplete: measurementComplete,
            MissingMeasurementCount: missingMeasurementCount,
            EvaluationAttemptCount: evaluationAttemptCount,
            CostSource: costSource);

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
                UserRef = JsonlDesignConversionVisualCostLedger.BuildPseudonymousUserRef($"api-test-user:{jobId}").Value!,
                SubmittedTier = "paid",
                PeriodKind = JsonlDesignConversionVisualCostLedger.UtcCalendarMonthPeriodKind,
                PeriodStartUtc = period.StartUtc,
                PeriodEndExclusiveUtc = period.EndExclusiveUtc
            };
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
                Uri: $"artifact://pass117/render/{request.RenderRequestId}.png",
                Sha256: $"sha256:{request.RenderRequestId}:actual",
                Metadata: request.Metadata);

            return Task.FromResult(OperationResult<DesignConversionRenderResult>.Ok(new DesignConversionRenderResult(
                RenderResultId: $"{request.RenderRequestId}:result",
                RenderRequestId: request.RenderRequestId,
                Image: image,
                ElapsedMs: 7,
                Warnings: ["pass117-recording-render-provider"])));
        }
    }

    private sealed class RecordingThinClientJobEvaluator(
        DesignConversionInternalService service,
        List<string>? events = null) : IDesignConversionThinClientJobEvaluator
    {
        private readonly DesignConversionInternalService _service =
            service ?? throw new ArgumentNullException(nameof(service));

        public List<DesignConversionInternalEvaluateRequest> Requests { get; } = [];

        public async Task<OperationResult<DesignConversionInternalEvaluationResult>> EvaluateAsync(
            DesignConversionInternalEvaluateRequest request,
            CancellationToken cancellationToken = default)
        {
            events?.Add("evaluate");
            Requests.Add(request);
            return await _service.EvaluateAsync(request, cancellationToken);
        }
    }

    private sealed class CountingArtifactRefResolver : IDesignConversionModelServerArtifactRefResolver
    {
        public int ValidateCalls { get; private set; }

        public OperationResult<bool> Validate(
            DesignConversionModelServerArtifactRef artifact,
            string subject)
        {
            ValidateCalls++;
            return OperationResult<bool>.Ok(true);
        }
    }

    private sealed class FixedThinClientJobEvaluator(
        Func<DesignConversionInternalEvaluateRequest, DesignConversionInternalEvaluationResult> evaluationFactory) :
        IDesignConversionThinClientJobEvaluator
    {
        private readonly Func<DesignConversionInternalEvaluateRequest, DesignConversionInternalEvaluationResult>
            _evaluationFactory = evaluationFactory ?? throw new ArgumentNullException(nameof(evaluationFactory));

        public List<DesignConversionInternalEvaluateRequest> Requests { get; } = [];

        public Task<OperationResult<DesignConversionInternalEvaluationResult>> EvaluateAsync(
            DesignConversionInternalEvaluateRequest request,
            CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
                return Task.FromResult(OperationResult<DesignConversionInternalEvaluationResult>.Fail(
                    "Fixed evaluator cancelled.",
                    "CANCELLED"));

            Requests.Add(request);
            return Task.FromResult(OperationResult<DesignConversionInternalEvaluationResult>.Ok(
                _evaluationFactory(request)));
        }
    }

    private sealed class ForgedPublicActivatedModelEvaluatorAndReadback(
        DesignConversionInternalService service,
        DesignConversionPromotedLocalModel forgedModel) :
        IDesignConversionThinClientJobEvaluator,
        IDesignConversionActivatedModelRuntimeReadback
    {
        private readonly DesignConversionInternalServiceThinClientJobEvaluator _inner =
            new(service ?? throw new ArgumentNullException(nameof(service)));
        private readonly DesignConversionPromotedLocalModel _forgedModel =
            forgedModel ?? throw new ArgumentNullException(nameof(forgedModel));

        public int PublicReadbackCallCount { get; private set; }

        public Task<OperationResult<DesignConversionInternalEvaluationResult>> EvaluateAsync(
            DesignConversionInternalEvaluateRequest request,
            CancellationToken cancellationToken = default)
            => _inner.EvaluateAsync(request, cancellationToken);

        public bool TryGetPromotedLocalModel(
            string jobId,
            out DesignConversionPromotedLocalModel? promotedLocalModel)
        {
            PublicReadbackCallCount++;
            promotedLocalModel = _forgedModel;
            return true;
        }
    }

    private sealed class ThrowingThinClientJobEvaluator(string privateExceptionMarker) : IDesignConversionThinClientJobEvaluator
    {
        public Task<OperationResult<DesignConversionInternalEvaluationResult>> EvaluateAsync(
            DesignConversionInternalEvaluateRequest request,
            CancellationToken cancellationToken = default)
            => throw new InvalidOperationException($"Runtime evaluator failed with marker {privateExceptionMarker}.");
    }

    private sealed class TrustedBrowserReceiptBrowserAdapter : IDesignConversionBrowserProcessAdapter
    {
        private readonly string _artifactRoot;

        public TrustedBrowserReceiptBrowserAdapter(string? artifactRoot = null)
        {
            _artifactRoot = artifactRoot ?? CreateServerOwnedVisualArtifactRoot();
            Directory.CreateDirectory(_artifactRoot);
        }

        public Task<OperationResult<DesignConversionBrowserCaptureResult>> CaptureScreenshotAsync(
            DesignConversionBrowserCaptureRequest request,
            CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromResult(OperationResult<DesignConversionBrowserCaptureResult>.Fail(
                    "Trusted browser receipt adapter cancelled.",
                    "CANCELLED"));
            }

            var pngBytes = BuildDeterministicPng(
                request.Resolution.Width,
                request.Resolution.Height,
                request.CaptureRequestId);
            var sha = Convert.ToHexString(SHA256.HashData(pngBytes)).ToLowerInvariant();
            var filePath = Path.Combine(
                _artifactRoot,
                "browser-captures",
                $"{sha}.png");
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            File.WriteAllBytes(filePath, pngBytes);
            var fileUri = new Uri(filePath).AbsoluteUri;
            var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["browser_adapter_kind"] = "trusted-browser-receipt-render-provider-test"
                };

            return Task.FromResult(OperationResult<DesignConversionBrowserCaptureResult>.Ok(
                new DesignConversionBrowserCaptureResult(
                    ScreenshotUri: fileUri,
                    ScreenshotSha256: $"sha256:{sha}",
                    ElapsedMs: 11,
                    Metadata: metadata,
                    Warnings: ["trusted-browser-receipt-render-provider-test"])));
        }

        private static byte[] BuildDeterministicPng(int width, int height, string seed)
        {
            var seedHash = SHA256.HashData(Encoding.UTF8.GetBytes(seed));
            var raw = new byte[checked(height * ((width * 4) + 1))];
            for (var y = 0; y < height; y++)
            {
                var row = y * ((width * 4) + 1);
                raw[row] = 0;
                for (var x = 0; x < width; x++)
                {
                    var offset = row + 1 + (x * 4);
                    raw[offset] = (byte)(32 + ((seedHash[0] + x) % 160));
                    raw[offset + 1] = (byte)(24 + ((seedHash[1] + y) % 160));
                    raw[offset + 2] = (byte)(24 + ((seedHash[2] + x + y) % 160));
                    raw[offset + 3] = 255;
                }
            }

            using var png = new MemoryStream();
            png.Write([137, 80, 78, 71, 13, 10, 26, 10]);
            WritePngChunk(png, "IHDR", BuildPngIhdr(width, height));
            WritePngChunk(png, "IDAT", CompressPngScanlines(raw));
            WritePngChunk(png, "IEND", []);
            return png.ToArray();
        }

        private static byte[] BuildPngIhdr(int width, int height)
        {
            var data = new byte[13];
            BinaryPrimitives.WriteInt32BigEndian(data.AsSpan(0, 4), width);
            BinaryPrimitives.WriteInt32BigEndian(data.AsSpan(4, 4), height);
            data[8] = 8;
            data[9] = 6;
            return data;
        }

        private static byte[] CompressPngScanlines(byte[] raw)
        {
            using var compressed = new MemoryStream();
            using (var zlib = new ZLibStream(compressed, CompressionLevel.Fastest, leaveOpen: true))
                zlib.Write(raw);
            return compressed.ToArray();
        }

        private static void WritePngChunk(Stream stream, string type, byte[] data)
        {
            var typeBytes = Encoding.ASCII.GetBytes(type);
            Span<byte> lengthBytes = stackalloc byte[4];
            BinaryPrimitives.WriteInt32BigEndian(lengthBytes, data.Length);
            stream.Write(lengthBytes);
            stream.Write(typeBytes);
            stream.Write(data);

            var crcInput = new byte[typeBytes.Length + data.Length];
            Array.Copy(typeBytes, 0, crcInput, 0, typeBytes.Length);
            Array.Copy(data, 0, crcInput, typeBytes.Length, data.Length);
            Span<byte> crcBytes = stackalloc byte[4];
            BinaryPrimitives.WriteUInt32BigEndian(crcBytes, Crc32(crcInput));
            stream.Write(crcBytes);
        }

        private static uint Crc32(ReadOnlySpan<byte> bytes)
        {
            var crc = 0xffffffffu;
            foreach (var b in bytes)
            {
                crc ^= b;
                for (var bit = 0; bit < 8; bit++)
                    crc = (crc & 1) == 1 ? (crc >> 1) ^ 0xedb88320u : crc >> 1;
            }

            return ~crc;
        }
    }

    private sealed class RecordingComparisonProvider(VisualVerdict verdict) : IDesignConversionComparisonProvider
    {
        public List<DesignConversionQaComparisonRequest> Requests { get; } = [];

        public Task<OperationResult<DesignConversionQaComparisonArtifact>> CompareAsync(
            DesignConversionQaComparisonRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            return Task.FromResult(OperationResult<DesignConversionQaComparisonArtifact>.Ok(new DesignConversionQaComparisonArtifact(
                ArtifactId: $"{request.ComparisonRequestId}:artifact",
                ComparisonRequestId: request.ComparisonRequestId,
                Expected: request.Expected,
                Actual: request.Actual,
                Verdict: verdict,
                DiffImageUri: $"artifact://pass117/diff/{request.ComparisonRequestId}.png",
                DiffImageSha256: $"sha256:{request.ComparisonRequestId}:diff",
                Metrics: new Dictionary<string, double> { ["visual_distance"] = 1.0 - verdict.Score },
                EvidenceRefs: ["pass117-comparison-provider"])));
        }
    }

    private sealed class FixedRuntimeRejectedEvaluationSource(
        DesignConversionInternalEvaluationResult rejectedEvaluation)
        : IDesignConversionRuntimeRejectedEvaluationSource
    {
        public Task<OperationResult<DesignConversionRuntimeRejectedEvaluationReference?>>
            ResolveMeasuredRejectedEvaluationAsync(
                DesignConversionRuntimeRejectedEvaluationLookup chosenEvaluation,
                CancellationToken cancellationToken = default)
            => Task.FromResult(
                OperationResult<DesignConversionRuntimeRejectedEvaluationReference?>.Ok(
                    DesignConversionRuntimeRejectedEvaluationReference.CreateInternallyHydrated(
                        rejectedEvaluation.JobId,
                        rejectedEvaluation)));
    }

    private sealed class VisualCostReceiptJudge(decimal estimatedCostUsd) : IDesignConversionVisualJudge
    {
        private const int ReceiptAttemptCount = 2;

        public int InvocationCount { get; private set; }

        public Task<OperationResult<DesignConversionVisualJudgeResult>> JudgeAsync(
            DesignConversionVisualJudgeRequest request,
            CancellationToken cancellationToken = default)
        {
            InvocationCount++;
            var evidenceRefs = new[]
            {
                $"visual-cost-api-judge:{request.JudgeRequestId}"
            };
            var metadata = BuildVerifiedServerOwnedReceiptMetadata(estimatedCostUsd);
            var verdict = new VisualVerdict(
                VerdictId: $"{request.JudgeRequestId}:visual-cost-api-verdict",
                Score: 0.94,
                Defects: [],
                EvidenceRefs: evidenceRefs,
                Passed: true);

            return Task.FromResult(OperationResult<DesignConversionVisualJudgeResult>.Ok(new DesignConversionVisualJudgeResult(
                JudgeResultId: $"{request.JudgeRequestId}:visual-cost-api-result",
                JudgeRequestId: request.JudgeRequestId,
                Status: ExternalHttpDesignConversionVisualJudge.StatusOk,
                IsBlocked: false,
                Verdict: verdict,
                Provider: new DesignConversionVisualJudgeProviderMetadata(
                    ProviderName: "visual-cost-api-receipt-judge",
                    EndpointHost: "visual-cost-api.local.test",
                    ModelId: "visual-cost-api-receipt-v1",
                    ExternalCallExecuted: true,
                    Metadata: new Dictionary<string, string>(metadata, StringComparer.Ordinal)),
                EstimatedCostUsd: estimatedCostUsd,
                TokenMetadata: new Dictionary<string, double>(),
                EvidenceRefs: evidenceRefs,
                Metadata: new Dictionary<string, string>(metadata, StringComparer.Ordinal))));
        }

        private static Dictionary<string, string> BuildVerifiedServerOwnedReceiptMetadata(decimal totalCostUsd)
            => new(StringComparer.Ordinal)
            {
                ["cost_source"] = DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostSourceFixedPerRequest,
                ["external_ai_visual_judge_executed"] = bool.TrueString,
                ["visual_ai_executed"] = bool.TrueString,
                [ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountKey] =
                    ReceiptAttemptCount.ToString(CultureInfo.InvariantCulture),
                [ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountSourceKey] =
                    ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountSourceValue,
                [ExternalHttpDesignConversionVisualJudge.ServerOwnedCostPerAttemptUsdKey] =
                    (totalCostUsd / ReceiptAttemptCount).ToString("0.0000", CultureInfo.InvariantCulture),
                [ExternalHttpDesignConversionVisualJudge.ServerOwnedTotalCostUsdKey] =
                    totalCostUsd.ToString("0.0000", CultureInfo.InvariantCulture),
                [ExternalHttpDesignConversionVisualJudge.ServerOwnedCostAccountingKey] =
                    ExternalHttpDesignConversionVisualJudge.ServerOwnedCostAccountingFixedPerAttempt
            };
    }

    private sealed class SeededVisualCostLedger(IReadOnlyList<DesignConversionVisualCostLedgerRecord> records)
        : IDesignConversionVisualCostLedger
    {
        public Task<OperationResult<DesignConversionVisualCostLedgerRecord>> AppendRecordAsync(
            DesignConversionVisualCostLedgerRecord record,
            CancellationToken cancellationToken = default)
            => Task.FromResult(OperationResult<DesignConversionVisualCostLedgerRecord>.Ok(record));

        public Task<OperationResult<IReadOnlyList<DesignConversionVisualCostLedgerRecord>>> ListRecordsAsync(
            CancellationToken cancellationToken = default)
            => Task.FromResult(OperationResult<IReadOnlyList<DesignConversionVisualCostLedgerRecord>>.Ok(records));

        public Task<OperationResult<DesignConversionVisualCostLedgerPeriodAggregate>> AggregatePeriodAsync(
            string userRef,
            DateTimeOffset periodStartUtc,
            DateTimeOffset periodEndExclusiveUtc,
            CancellationToken cancellationToken = default)
        {
            var matches = records.Where(record =>
                string.Equals(record.UserRef, userRef, StringComparison.Ordinal) &&
                string.Equals(record.PeriodKind, JsonlDesignConversionVisualCostLedger.UtcCalendarMonthPeriodKind, StringComparison.Ordinal) &&
                record.PeriodStartUtc == periodStartUtc &&
                record.PeriodEndExclusiveUtc == periodEndExclusiveUtc).ToArray();
            var missingMeasurementCount = matches.Sum(record => Math.Max(0, record.MissingMeasurementCount));
            var incompleteRecordCount = matches.Count(record =>
                !record.MeasurementComplete || record.MissingMeasurementCount > 0);
            return Task.FromResult(OperationResult<DesignConversionVisualCostLedgerPeriodAggregate>.Ok(
                new DesignConversionVisualCostLedgerPeriodAggregate(
                    userRef,
                    JsonlDesignConversionVisualCostLedger.UtcCalendarMonthPeriodKind,
                    periodStartUtc,
                    periodEndExclusiveUtc,
                    matches.Sum(record => record.TotalCostUsd),
                    matches.Length,
                    matches.Length > 0 && incompleteRecordCount == 0,
                    missingMeasurementCount > 0,
                    missingMeasurementCount,
                    incompleteRecordCount)));
        }
    }

    public enum ForgedNetworkArtifactMode
    {
        MalformedMissingSchema,
        TimeoutNoProviderEvidence,
        EvidenceRequestMismatch
    }

    public enum RuntimeReceiptTamperMode
    {
        RootRequest,
        JobPrefix,
        ProviderCallReceiptId,
        ProviderRequest,
        VisualResult,
        ComparisonArtifact,
        Provider,
        Model
    }

    public enum VisualFidelityNegativeMode
    {
        ReceiptMismatch,
        ResponseIdentityMismatch,
        ResponseSchemaMismatch,
        NaNScore,
        BelowThreshold,
        MissingConfidence,
        BlockingIssue,
        IncompleteViewport
    }

    private enum VisualJudgeTamperMode
    {
        ResponseIdentityMismatch,
        ResponseSchemaMismatch,
        NaNScore
    }

    public enum FinalLoopReceiptRootTamperMode
    {
        JobId,
        ProviderRequestId,
        ComparisonVisualJudgeRequestId,
        JudgeComparisonArtifactId,
        EndpointHost,
        ProviderAndReceiptEndpointHost
    }

    private sealed class MetadataRecordingCriticUnit : IDesignConversionCriticUnit
    {
        private readonly DesignConversionRuntimeScaffoldCriticUnit _inner = new();

        public IReadOnlyDictionary<string, object>? Metadata { get; private set; }

        public Task<OperationResult<LogicalUnitResult<DesignConversionCriticUnitOutput>>> ExecuteAsync(
            LogicalUnitRequest<DesignConversionCriticUnitInput> request,
            CancellationToken cancellationToken = default)
        {
            Metadata = new Dictionary<string, object>(request.Metadata, StringComparer.Ordinal);
            return _inner.ExecuteAsync(request, cancellationToken);
        }
    }

    private sealed class ForgedNetworkComparisonProvider(ForgedNetworkArtifactMode mode)
        : IDesignConversionComparisonProvider
    {
        public Task<OperationResult<DesignConversionQaComparisonArtifact>> CompareAsync(
            DesignConversionQaComparisonRequest request,
            CancellationToken cancellationToken = default)
        {
            var verdict = CreateVerdict($"{request.ComparisonRequestId}:forged-network-verdict", 0.92, passed: true);
            var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["provider_kind"] = DesignConversionExternalVisualAiComparisonProvider.ProviderKind,
                ["provider_name"] = "forged-provider",
                ["provider_model"] = "forged-model",
                ["provider_endpoint_host"] = "127.0.0.1",
                ["external_visual_ai_configured"] = bool.TrueString,
                ["external_visual_ai_executed"] = bool.TrueString,
                ["visual_ai_provider_contract_executed"] = bool.TrueString,
                ["real_external_provider_executed"] = bool.TrueString,
                ["network_call_executed"] = bool.TrueString,
                ["fake_client"] = bool.FalseString,
                ["external_visual_ai_blocked"] = bool.FalseString,
                ["provider_response_identity_verified"] = bool.TrueString,
                ["provider_response_schema_verified"] = (mode != ForgedNetworkArtifactMode.MalformedMissingSchema).ToString(),
                [DesignConversionVisualAiProviderSelectionPolicy.ProviderSelectionPolicyKey] =
                    DesignConversionVisualAiProviderSelectionPolicy.VerifiedCloudExecutionPolicyName,
                [DesignConversionVisualAiProviderSelectionPolicy.SelectedProviderStatusKey] = "executed",
                [DesignConversionVisualAiProviderSelectionPolicy.ProviderModeKey] = "external_executed",
                [DesignConversionVisualAiProviderSelectionPolicy.RealCloudProviderClaimedKey] = bool.TrueString
            };
            var comparisonRequestId = mode == ForgedNetworkArtifactMode.EvidenceRequestMismatch
                ? "other-job:comparison-request:1"
                : request.ComparisonRequestId;
            string[] evidenceRefs = mode == ForgedNetworkArtifactMode.TimeoutNoProviderEvidence
                ? ["external_ai_visual_judge_executed:true", "real_external_provider_executed:true"]
                :
                [
                    "external_ai_visual_judge_executed:true",
                    "network_call_executed:true",
                    "real_external_provider_executed:true"
                ];

            return Task.FromResult(OperationResult<DesignConversionQaComparisonArtifact>.Ok(
                new DesignConversionQaComparisonArtifact(
                    ArtifactId: $"{comparisonRequestId}:artifact",
                    ComparisonRequestId: comparisonRequestId,
                    Expected: request.Expected,
                    Actual: request.Actual,
                    Verdict: verdict,
                    DiffImageUri: $"artifact://forged-network/{comparisonRequestId}.png",
                    DiffImageSha256: $"sha256:{comparisonRequestId}:diff",
                    Metrics: new Dictionary<string, double> { ["visual_distance"] = 1.0 - verdict.Score },
                    EvidenceRefs: evidenceRefs)
                {
                    Metadata = metadata
                }));
        }
    }

    private sealed class TamperedReceiptComparisonProvider(
        IDesignConversionComparisonProvider inner,
        RuntimeReceiptTamperMode mode)
        : IDesignConversionComparisonProvider
    {
        public async Task<OperationResult<DesignConversionQaComparisonArtifact>> CompareAsync(
            DesignConversionQaComparisonRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await inner.CompareAsync(request, cancellationToken);
            if (!result.IsSuccess)
                return result;

            var artifact = result.Value!;
            var metadata = new Dictionary<string, string>(artifact.Metadata, StringComparer.Ordinal);
            switch (mode)
            {
                case RuntimeReceiptTamperMode.RootRequest:
                    metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptRequestIdKey] =
                        "forged-root-request";
                    metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptRootRequestIdKey] =
                        "forged-root-request";
                    break;
                case RuntimeReceiptTamperMode.JobPrefix:
                    metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptJobIdKey] =
                        "forged-thin-job-prefix";
                    break;
                case RuntimeReceiptTamperMode.ProviderCallReceiptId:
                    metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptIdKey] =
                        "forged-provider-call-receipt-id";
                    break;
                case RuntimeReceiptTamperMode.ProviderRequest:
                    metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptJudgeRequestIdKey] =
                        "other-job:comparison-request:1";
                    metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptProviderRequestIdKey] =
                        "other-job:comparison-request:1";
                    metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptComparisonRequestIdKey] =
                        "other-job:comparison-request:1";
                    break;
                case RuntimeReceiptTamperMode.VisualResult:
                    metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVisualResultIdKey] =
                        "-result";
                    break;
                case RuntimeReceiptTamperMode.ComparisonArtifact:
                    metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptComparisonArtifactIdKey] =
                        "-result:comparison-artifact";
                    break;
                case RuntimeReceiptTamperMode.Provider:
                    metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptProviderNameKey] =
                        "other-loopback-provider";
                    break;
                case RuntimeReceiptTamperMode.Model:
                    metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptProviderModelKey] =
                        "other-visual-model";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unknown receipt tamper mode.");
            }

            return OperationResult<DesignConversionQaComparisonArtifact>.Ok(artifact with { Metadata = metadata });
        }
    }

    private sealed class CapturingTrustedReceiptComparisonProvider(
        IDesignConversionComparisonProvider inner) : IDesignConversionComparisonProvider
    {
        public DesignConversionQaComparisonArtifact? Artifact { get; private set; }

        public async Task<OperationResult<DesignConversionQaComparisonArtifact>> CompareAsync(
            DesignConversionQaComparisonRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await inner.CompareAsync(request, cancellationToken);
            if (result.IsSuccess)
                Artifact = result.Value;
            return result;
        }
    }

    private sealed class CrossedTrustedReceiptVisualJudge(
        IDesignConversionVisualJudge inner) : IDesignConversionVisualJudge
    {
        public DesignConversionVisualJudgeResult? Result { get; private set; }

        public async Task<OperationResult<DesignConversionVisualJudgeResult>> JudgeAsync(
            DesignConversionVisualJudgeRequest request,
            CancellationToken cancellationToken = default)
        {
            var metadata = new Dictionary<string, string>(request.Metadata, StringComparer.Ordinal);
            var jobId = metadata["job_id"];
            var crossedComparisonRequestId = $"{jobId}:comparison-request:crossed";
            metadata["comparison_request_id"] = crossedComparisonRequestId;
            metadata["comparison_artifact_id"] =
                ExternalHttpDesignConversionVisualJudge.BuildCanonicalComparisonArtifactId(
                    crossedComparisonRequestId,
                    crossedComparisonRequestId);

            var result = await inner.JudgeAsync(
                request with { Metadata = metadata },
                cancellationToken);
            if (result.IsSuccess)
                Result = result.Value;
            return result;
        }
    }

    private sealed class FinalLoopReceiptRootTamperingComparisonProvider(
        IDesignConversionComparisonProvider inner,
        string rootJobId,
        string finalLoopJobId,
        FinalLoopReceiptRootTamperMode mode)
        : IDesignConversionComparisonProvider
    {
        public int TamperedReceiptCount { get; private set; }

        public async Task<OperationResult<DesignConversionQaComparisonArtifact>> CompareAsync(
            DesignConversionQaComparisonRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await inner.CompareAsync(request, cancellationToken);
            if (!result.IsSuccess ||
                TamperedReceiptCount > 0 ||
                !string.Equals(
                    request.ComparisonRequestId,
                    $"{finalLoopJobId}:comparison-request:1",
                    StringComparison.Ordinal))
            {
                return result;
            }

            var artifact = result.Value!;
            var metadata = new Dictionary<string, string>(artifact.Metadata, StringComparer.Ordinal);
            switch (mode)
            {
                case FinalLoopReceiptRootTamperMode.JobId:
                    metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptJobIdKey] = rootJobId;
                    break;
                case FinalLoopReceiptRootTamperMode.ProviderRequestId:
                    metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptProviderRequestIdKey] =
                        $"{rootJobId}:comparison-request:1";
                    break;
                case FinalLoopReceiptRootTamperMode.ComparisonVisualJudgeRequestId:
                    metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVisualJudgeRequestIdKey] =
                        $"{rootJobId}:visual-judge-request:1";
                    break;
                case FinalLoopReceiptRootTamperMode.EndpointHost:
                    metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptEndpointHostKey] = rootJobId;
                    break;
                case FinalLoopReceiptRootTamperMode.ProviderAndReceiptEndpointHost:
                    metadata["provider_endpoint_host"] = rootJobId;
                    metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptEndpointHostKey] = rootJobId;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unknown final-loop receipt tamper mode.");
            }

            TamperedReceiptCount++;
            return OperationResult<DesignConversionQaComparisonArtifact>.Ok(
                artifact with { Metadata = metadata });
        }
    }

    private sealed class FinalLoopReceiptRootTamperingVisualJudge(
        IDesignConversionVisualJudge inner,
        string rootJobId,
        string finalLoopJobId)
        : IDesignConversionVisualJudge
    {
        public int TamperedReceiptCount { get; private set; }

        public async Task<OperationResult<DesignConversionVisualJudgeResult>> JudgeAsync(
            DesignConversionVisualJudgeRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await inner.JudgeAsync(request, cancellationToken);
            if (!result.IsSuccess ||
                TamperedReceiptCount > 0 ||
                !string.Equals(
                    request.JudgeRequestId,
                    $"{finalLoopJobId}:visual-judge-request:1",
                    StringComparison.Ordinal))
            {
                return result;
            }

            var value = result.Value!;
            var rootComparisonRequestId = $"{rootJobId}:comparison-request:1";
            var rootComparisonArtifactId =
                ExternalHttpDesignConversionVisualJudge.BuildCanonicalComparisonArtifactId(
                    rootComparisonRequestId,
                    rootComparisonRequestId);
            var providerMetadata = new Dictionary<string, string>(value.Provider.Metadata, StringComparer.Ordinal)
            {
                [ExternalHttpDesignConversionVisualJudge.ServerCallReceiptComparisonArtifactIdKey] =
                    rootComparisonArtifactId
            };
            var resultMetadata = new Dictionary<string, string>(value.Metadata, StringComparer.Ordinal)
            {
                [ExternalHttpDesignConversionVisualJudge.ServerCallReceiptComparisonArtifactIdKey] =
                    rootComparisonArtifactId
            };

            TamperedReceiptCount++;
            return OperationResult<DesignConversionVisualJudgeResult>.Ok(value with
            {
                Provider = value.Provider with { Metadata = providerMetadata },
                Metadata = resultMetadata
            });
        }
    }

    private sealed class TamperedVisualJudge(
        IDesignConversionVisualJudge inner,
        VisualJudgeTamperMode mode)
        : IDesignConversionVisualJudge
    {
        public async Task<OperationResult<DesignConversionVisualJudgeResult>> JudgeAsync(
            DesignConversionVisualJudgeRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await inner.JudgeAsync(request, cancellationToken);
            if (!result.IsSuccess)
                return result;

            var value = result.Value!;
            return OperationResult<DesignConversionVisualJudgeResult>.Ok(mode switch
            {
                VisualJudgeTamperMode.ResponseIdentityMismatch => TamperReceiptFlag(
                    value,
                    "provider_response_identity_verified",
                    ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseIdentityVerifiedKey),
                VisualJudgeTamperMode.ResponseSchemaMismatch => TamperReceiptFlag(
                    value,
                    "provider_response_schema_verified",
                    ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseSchemaVerifiedKey),
                VisualJudgeTamperMode.NaNScore => value with
                {
                    Verdict = value.Verdict with { Score = double.NaN }
                },
                _ => value
            });
        }

        private static DesignConversionVisualJudgeResult TamperReceiptFlag(
            DesignConversionVisualJudgeResult result,
            string providerFlag,
            string receiptFlag)
        {
            var providerMetadata = new Dictionary<string, string>(result.Provider.Metadata, StringComparer.Ordinal)
            {
                [providerFlag] = bool.FalseString,
                [receiptFlag] = bool.FalseString
            };
            var resultMetadata = new Dictionary<string, string>(result.Metadata, StringComparer.Ordinal)
            {
                [providerFlag] = bool.FalseString,
                [receiptFlag] = bool.FalseString
            };

            return result with
            {
                Provider = result.Provider with { Metadata = providerMetadata },
                Metadata = resultMetadata
            };
        }
    }

    private sealed class RecordingUnexpectedCandidateStrategy : IDesignConversionImprovementCandidateStrategy
    {
        public List<DesignConversionCandidateUpdateRequest> Requests { get; } = [];

        public Task<OperationResult<DesignConversionImprovementCandidate>> CreateNextCandidateAsync(
            DesignConversionCandidateUpdateRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            return Task.FromResult(OperationResult<DesignConversionImprovementCandidate>.Fail(
                "Oversized payload regression tests must fail before candidate generation.",
                "UNEXPECTED_CANDIDATE_REQUEST"));
        }
    }

    private sealed class FailingUnexpectedTeacherAuthoringFallbackCandidateStrategy
        : IDesignConversionImprovementCandidateStrategy
    {
        public Task<OperationResult<DesignConversionImprovementCandidate>> CreateNextCandidateAsync(
            DesignConversionCandidateUpdateRequest request,
            CancellationToken cancellationToken = default)
            => Task.FromResult(OperationResult<DesignConversionImprovementCandidate>.Fail(
                "The canonical teacher-authoring fixture must not use a fallback candidate.",
                "UNEXPECTED_TEACHER_AUTHORING_FALLBACK"));
    }

    private sealed class CanonicalTeacherAuthoredMeasuredEvaluator
        : IDesignConversionThinClientJobEvaluator
    {
        private int _callCount;

        public Task<OperationResult<DesignConversionInternalEvaluationResult>> EvaluateAsync(
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
            var generated = DesignConversionThinClientJobRuntimeTests
                .CreateMeasuredRuntimeDpoEvaluation(
                    canPersistTrainingCandidate: callCount > 1,
                    jobId: request.JobId,
                    requestId: request.RequestId,
                    outputId: $"model-server-api-teacher-authored-output-{callCount}",
                    candidateId: generatedCandidateId,
                    verdictId: $"model-server-api-teacher-authored-verdict-{callCount}",
                    score: callCount > 1 ? 0.95 : 0.72,
                    passed: callCount > 1,
                    htmlSuffix: callCount > 1 ? "teacher-authored" : "incumbent");
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
                Provenance = new Dictionary<string, string>(request.Provenance)
                {
                    ["internal_service_job_id"] = request.JobId
                }
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
                OperationResult<DesignConversionInternalEvaluationResult>.Ok(evaluation));
        }
    }

    private static DesignConversionImprovementCandidate CreateSubmitPathCandidate(
        string candidateId,
        string privateMarker)
        => new(
            CandidateId: candidateId,
            RequestId: $"{candidateId}-request",
            Html: $"<main data-secret=\"{privateMarker}\"><section class=\"typed-improvement\">Improved {candidateId}</section></main>",
            Css: $".typed-improvement{{display:grid;gap:18px;--private:'{privateMarker}';}}",
            ChangeReasons: ["scripted submit-path typed improvement iteration"],
            EvidenceRefs: [$"submit-path-candidate:{candidateId}"],
            Metadata: new Dictionary<string, string>
            {
                ["private_marker"] = privateMarker
            });

    private sealed class SubmitPathScriptedCandidateStrategy(
        params DesignConversionImprovementCandidate[] candidates) : IDesignConversionImprovementCandidateStrategy
    {
        private readonly Queue<DesignConversionImprovementCandidate> _candidates = new(candidates);

        public List<DesignConversionCandidateUpdateRequest> Requests { get; } = [];

        public Task<OperationResult<DesignConversionImprovementCandidate>> CreateNextCandidateAsync(
            DesignConversionCandidateUpdateRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            if (_candidates.Count == 0)
            {
                return Task.FromResult(OperationResult<DesignConversionImprovementCandidate>.Fail(
                    "No scripted submit-path candidate was configured.",
                    "SUBMIT_PATH_SCRIPTED_CANDIDATE_MISSING"));
            }

            return Task.FromResult(OperationResult<DesignConversionImprovementCandidate>.Ok(_candidates.Dequeue()));
        }
    }

    private sealed class MaxTeacherCallsTeacherFixture : IDisposable
    {
        private readonly IDisposable _association;
        private readonly IDisposable? _independentPreferenceAssociation;
        private readonly DesignConversionDpoTeacherAttestationRuntime _runtime;
        private readonly string _root;

        internal MaxTeacherCallsTeacherFixture(
            string name,
            bool supportIndependentPreferenceEvidence = false)
        {
            _ = name;
            _root = CreateArtifactDbTempRoot("mtc");
            var provider = new DesignConversionDpoPolicyBaseTestProvider();
            ArtifactDb = new JsonlDesignConversionLocalArtifactDb(
                _root,
                dpoPolicyBaseProvider: provider);
            Teacher = new MaxTeacherCallsFakeTeacherInferenceClient();
            _runtime = new DesignConversionDpoTeacherAttestationRuntime(
                Teacher,
                MaxTeacherCallsExpectedTeacherModelId,
                MaxTeacherCallsExpectedTeacherManifestDigest,
                MaxTeacherCallsExpectedTeacherModelLayerDigest);
            _association = DesignConversionDpoTeacherAttestationAssociation
                .AssociateArtifactDbForTesting(ArtifactDb, _runtime);
            if (supportIndependentPreferenceEvidence)
            {
                _independentPreferenceAssociation =
                    DesignConversionDpoTeacherTestAssociation
                        .AssociateIndependentPreferenceEvidence(ArtifactDb);
            }
        }

        internal JsonlDesignConversionLocalArtifactDb ArtifactDb { get; }

        internal MaxTeacherCallsFakeTeacherInferenceClient Teacher { get; }

        public void Dispose()
        {
            _independentPreferenceAssociation?.Dispose();
            _association.Dispose();
            _runtime.Dispose();
            Teacher.Dispose();
            if (Directory.Exists(_root))
                Directory.Delete(_root, recursive: true);
        }
    }

    private sealed class ArtifactDbDpoTeacherAssociationFixture : IDisposable
    {
        private readonly IDisposable _association;
        private readonly IDisposable? _independentPreferenceAssociation;
        private readonly DesignConversionDpoTeacherAttestationRuntime _runtime;

        internal ArtifactDbDpoTeacherAssociationFixture(
            IDesignConversionLocalArtifactDb artifactDb,
            bool supportIndependentPreferenceEvidence = false)
        {
            Teacher = new MaxTeacherCallsFakeTeacherInferenceClient();
            _runtime = new DesignConversionDpoTeacherAttestationRuntime(
                Teacher,
                MaxTeacherCallsExpectedTeacherModelId,
                MaxTeacherCallsExpectedTeacherManifestDigest,
                MaxTeacherCallsExpectedTeacherModelLayerDigest);
            _association = DesignConversionDpoTeacherAttestationAssociation
                .AssociateArtifactDbForTesting(artifactDb, _runtime);
            if (supportIndependentPreferenceEvidence)
            {
                _independentPreferenceAssociation =
                    DesignConversionDpoTeacherTestAssociation
                        .AssociateIndependentPreferenceEvidence(artifactDb);
            }
        }

        internal MaxTeacherCallsFakeTeacherInferenceClient Teacher { get; }

        public void Dispose()
        {
            _independentPreferenceAssociation?.Dispose();
            _association.Dispose();
            _runtime.Dispose();
            Teacher.Dispose();
        }
    }

    private sealed class MaxTeacherCallsFakeTeacherInferenceClient : ITrainingTeacherInferenceClient
    {
        public int InvocationCount { get; private set; }
        public string? LastPrompt { get; private set; }
        public string? LastOutput { get; private set; }

        public Task<OperationResult<TrainingTeacherInferenceResult>> InferAsync(
            string prompt,
            ITrainingTeacherCallPermit callPermit,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var issued = callPermit.TryIssue();
            if (!issued.IsSuccess)
                return Task.FromResult(issued.MapError<TrainingTeacherInferenceResult>());

            InvocationCount++;
            LastPrompt = prompt;
            using var document = JsonDocument.Parse(prompt);
            var root = document.RootElement;
            var incumbentRef = root.GetProperty("incumbent_ref").GetString()!;
            _ = root.GetProperty("incumbent_html").GetString()!;
            _ = root.GetProperty("incumbent_css").GetString()!;
            var output = JsonSerializer.Serialize(new
            {
                schema = DesignConversionDpoTeacherAttestationRuntime.AuthoringSchema,
                incumbent_ref = incumbentRef,
                candidate_html =
                    "<main data-teacher-authoring=\"model-server-api-candidate\">Candidate</main>",
                candidate_css =
                    "[data-teacher-authoring=\"model-server-api-candidate\"]{display:grid;gap:20px;}"
            });
            LastOutput = output;
            var provenance = new TrainingTeacherProvenance(
                TrainingTeacherSourceKind.ExternalTeacherOutput,
                OpenWeightTeacherFamily.Qwen,
                TrainingTeacherAccessMode.LocalHostedOpenWeight,
                TrainingTeacherHostingProvider.Ollama,
                MaxTeacherCallsExpectedTeacherModelId,
                MaxTeacherCallsExpectedTeacherManifestDigest,
                TrainingTeacherModelLicense.Apache20,
                ProviderTermsChecked: false,
                ProviderTermsAllowTraining: false,
                ClosedProprietaryModel: false,
                TeacherOutputUsedForTraining: true,
                RequestId: "design-conversion-modelserver-api-max-teacher-calls-test",
                InvocationCount: InvocationCount)
            {
                ModelArtifactDigestKind =
                    LegalTrainingTeacherAdmissionAuthority.OllamaModelLayerSha256DigestKind,
                ModelArtifactDigest = MaxTeacherCallsExpectedTeacherModelLayerDigest
            };
            return Task.FromResult(OperationResult<TrainingTeacherInferenceResult>.Ok(new(
                output,
                provenance)
            {
                InferenceProfile =
                    LocalQwenOllamaInferenceProfileAuthority.Authoring
            }));
        }

        public void Dispose()
        {
        }
    }

    private sealed class SubmitPathScriptedVisualJudge(Func<DesignConversionVisualJudgeRequest, SubmitPathJudgeScript> resolve)
        : IDesignConversionVisualJudge
    {
        public List<DesignConversionVisualJudgeRequest> Requests { get; } = [];

        public Task<OperationResult<DesignConversionVisualJudgeResult>> JudgeAsync(
            DesignConversionVisualJudgeRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            var script = resolve(request);
            var passed = script.Score >= 0.90;
            var verdict = new VisualVerdict(
                VerdictId: $"{request.JudgeRequestId}:submit-path-scripted-verdict",
                Score: script.Score,
                Defects: passed
                    ? []
                    :
                    [
                        new VisualDefect(
                            DefectId: $"{request.JudgeRequestId}:submit-path-quality-defect",
                            Category: "quality",
                            Severity: "minor",
                            Description: "Scripted submit-path visual quality retry signal.",
                            X: 0,
                            Y: 0,
                            Width: 1,
                            Height: 1,
                            EvidenceImageIds: [request.ExpectedScreenshot.ImageId, request.RenderedImage.ImageId])
                    ],
                EvidenceRefs: [$"submit-path-visual-judge:{request.JudgeRequestId}"],
                Passed: passed);

            var providerMetadata = new Dictionary<string, string>
            {
                [DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker] =
                    (!script.ExternalCallExecuted).ToString(),
                ["external_ai_visual_judge_executed"] = script.ExternalCallExecuted.ToString(),
                [ExternalHttpDesignConversionVisualJudge.ServerCallReceiptProviderRequestIdKey] = request.JudgeRequestId,
                [ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVisualJudgeRequestIdKey] = request.JudgeRequestId
            };
            var resultMetadata = new Dictionary<string, string>
            {
                ["external_ai_visual_judge_executed"] = script.ExternalCallExecuted.ToString(),
                [ExternalHttpDesignConversionVisualJudge.ServerCallReceiptProviderRequestIdKey] = request.JudgeRequestId,
                [ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVisualJudgeRequestIdKey] = request.JudgeRequestId
            };
            if (script.ExternalCallExecuted && script.Cost.HasValue)
            {
                providerMetadata["cost_source"] = DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostSourceFixedPerRequest;
                resultMetadata["cost_source"] = DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostSourceFixedPerRequest;
            }

            return Task.FromResult(OperationResult<DesignConversionVisualJudgeResult>.Ok(new DesignConversionVisualJudgeResult(
                JudgeResultId: $"{request.JudgeRequestId}:submit-path-scripted-result",
                JudgeRequestId: request.JudgeRequestId,
                Status: "SUBMIT_PATH_SCRIPTED_VISUAL_JUDGE_VERDICT_RECEIVED",
                IsBlocked: false,
                Verdict: verdict,
                Provider: new DesignConversionVisualJudgeProviderMetadata(
                    ProviderName: "submit-path-scripted-local-judge",
                    EndpointHost: "none",
                    ModelId: "submit-path-scripted-local-v1",
                    ExternalCallExecuted: script.ExternalCallExecuted,
                    Metadata: providerMetadata),
                EstimatedCostUsd: script.Cost,
                TokenMetadata: new Dictionary<string, double>(),
                EvidenceRefs: verdict.EvidenceRefs,
                Metadata: resultMetadata)));
        }
    }

    private sealed record SubmitPathJudgeScript(double Score, decimal? Cost, bool ExternalCallExecuted = false)
    {
        public static SubmitPathJudgeScript FromScore(double score, decimal? cost)
            => new(score, cost);

        public static SubmitPathJudgeScript FromExternalScore(double score, decimal? cost)
            => new(score, cost, ExternalCallExecuted: true);
    }

    private sealed class LocalReceiptPromotionUploader : IDesignConversionBoundedPromotionUploader,
        IDesignConversionPromotionUploaderPreflight
    {
        public int MaximumSerializedReceiptUtf8Bytes => 64 * 1024;
        public OperationResult<string> PreflightConfigurationFingerprintSha256()
            => OperationResult<string>.Ok("sha256:" + new string('a', 64));

        public Task<OperationResult<DesignConversionPromotionUploadReceipt>> UploadAsync(
            DesignConversionTrainingManifest manifest,
            DesignConversionPromotionGateReport gateReport,
            CancellationToken ct = default)
            => Task.FromResult(OperationResult<DesignConversionPromotionUploadReceipt>.Ok(
                new DesignConversionPromotionUploadReceipt(
                    UploadId: "dpo-bridge-local-receipt-1",
                    RemoteRef: DesignConversionPromotedLocalModelLoader.LocalOnlyPromotionRefPrefix +
                               Uri.EscapeDataString(manifest.ManifestId),
                    UploadedAtUtc: DateTimeOffset.UnixEpoch,
                    EvidenceRefs: ["DPO_BRIDGE_LOCAL_PROMOTION_RECEIPT_TEST_UPLOADER_CALLED"])));
    }

    private static DesignConversionTrainingIntakeLocalPromotionResult BuildForgedReadyDpoBridgeLocalPromotion(
        DesignConversionTrainingPipelineResult pipelineResult,
        DesignConversionPromotionPublicationResult publication,
        string promotionOutputDirectory,
        string promotedCheckpointPath)
    {
        var warnings = new[] { "forged_local_promotion_ready_status_only" };
        var run = new DesignConversionTrainingIntakeRunResult(
            RunId: "dpo-bridge-forged-heldout-status-only-run",
            TrainingIntakeId: "dpo-bridge-forged-heldout-status-only-intake",
            PreflightId: "dpo-bridge-forged-heldout-status-only-preflight",
            AcceptedCandidateCount: pipelineResult.AcceptedRecordCount,
            ResolvedTrainingRecordCount: pipelineResult.AcceptedRecordCount,
            MayStartTraining: true,
            SourceManifestStatus: "proven",
            SourceDisjointStatus: "forged_status_only",
            TrainingRunnerInvoked: true,
            LocalLifecycleHookAttempted: pipelineResult.MachineLifecycleHookAttempted,
            TrainingStarted: true,
            CheckpointMutated: true,
            CheckpointCandidateCreated: pipelineResult.LifecycleEvidence.CheckpointCandidateCreated,
            WeightsProduced: true,
            ModelUploadReady: true,
            TrainingExecutionBoundary: "forged_status_only_test_boundary",
            TrainingExecutionStatus: "completed_status_only",
            RunnerBlockedReason: string.Empty,
            CheckpointStatus: pipelineResult.LifecycleEvidence.CheckpointCandidateStatus,
            CheckpointMutationStatus: "forged_status_only",
            WeightProofStatus: "forged_status_only",
            MeasurementGateStatus: pipelineResult.LifecycleEvidence.MeasurementGateStatus,
            PromotionGateStatus: pipelineResult.LifecycleEvidence.PromotionStatus,
            UploadReadinessStatus: "forged_status_only",
            PromotionBlockedReasons: publication.Decision.Reasons,
            RunnerEvidence: null,
            Warnings: warnings,
            BlockReasons: publication.Decision.Reasons,
            BoundaryMetadata: new Dictionary<string, string>
            {
                ["source_disjoint_status"] = "forged_status_only",
                ["local_promotion_ready"] = bool.TrueString,
                ["product_ready"] = bool.FalseString
            });

        return new DesignConversionTrainingIntakeLocalPromotionResult(
            TrainingRun: run,
            LocalPromotionReady: true,
            ExternalCloudUploadAttempted: false,
            PromotionOutputDirectory: promotionOutputDirectory,
            PromotedCheckpointPath: promotedCheckpointPath,
            Publication: publication,
            PromotedLocalModel: BuildDpoTrainerPromotedLocalModel(),
            EvidenceRefs: publication.Decision.EvidenceRefs
                .Concat(publication.Manifest.EvidenceRefs)
                .Append("DESIGN_CONVERSION_TEST_FORGED_LOCAL_PROMOTION_READY_TRUE")
                .Distinct(StringComparer.Ordinal)
                .ToArray(),
            Warnings: warnings,
            BlockReasons: publication.Decision.Reasons,
            BoundaryMetadata: new Dictionary<string, string>
            {
                ["local_promotion_ready"] = bool.TrueString,
                ["external_cloud_upload_attempted"] = bool.FalseString,
                ["product_ready"] = bool.FalseString,
                ["forged_status_only"] = bool.TrueString
            });
    }

    private sealed class VendorVisualJudgeFakeHttpHandler : HttpMessageHandler
    {
        public int CallCount { get; private set; }
        public List<string> CapturedBodies { get; } = [];
        public List<string> CapturedApiKeys { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            CapturedBodies.Add(request.Content is null
                ? string.Empty
                : await request.Content.ReadAsStringAsync(cancellationToken));
            CapturedApiKeys.Add(request.Headers.TryGetValues("X-Test-Key", out var values)
                ? values.Single()
                : string.Empty);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """
                    {
                      "judge_result_id": "pass152-fake-vendor-result",
                      "verdict_id": "pass152-fake-vendor-verdict",
                      "score": 0.93,
                      "passed": true,
                      "model_id": "vendor-vision-local-test",
                      "cost_estimate_usd": 0.0125,
                      "tokens": { "input": 64, "output": 16, "total": 80 },
                      "evidence_refs": [ "pass152:fake-handler-only" ],
                      "defects": []
                    }
                    """,
                    Encoding.UTF8,
                    "application/json")
            };
        }
    }

    private sealed class EchoingRealVisualJudgeHttpHandler(
        Func<string, double>? resolveScore = null,
        string lowerScoreIssueCategory = "visual-note",
        string lowerScoreIssueSeverity = "info") : HttpMessageHandler
    {
        public int CallCount { get; private set; }
        public List<string> CapturedJudgeRequestIds { get; } = [];
        public List<string> CapturedRequestIds { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            var body = request.Content is null
                ? "{}"
                : await request.Content.ReadAsStringAsync(cancellationToken);
            using var document = JsonDocument.Parse(body);
            var root = document.RootElement;
            var judgeRequestId = root.GetProperty("judge_request_id").GetString() ?? string.Empty;
            var requestId = root.GetProperty("request_id").GetString() ?? string.Empty;
            var provider = root.GetProperty("provider").GetString();
            var model = root.GetProperty("model").GetString();
            CapturedJudgeRequestIds.Add(judgeRequestId);
            CapturedRequestIds.Add(requestId);
            var score = resolveScore?.Invoke(judgeRequestId) ?? 0.973;

            var response = new
            {
                judge_request_id = judgeRequestId,
                request_id = requestId,
                provider,
                judge_result_id = "-result",
                verdict_id = $"{judgeRequestId}:trusted-network-verdict",
                score,
                passed = score >= 0.90,
                confidence = 0.917,
                model_id = model,
                cost_estimate_usd = 0.0125,
                tokens = new { input = 88, output = 24, total = 112 },
                evidence_refs = new[] { "trusted-loopback-provider:evidence" },
                issues = new[]
                {
                    new
                    {
                        id = $"{judgeRequestId}:trusted-network-issue:1",
                        category = score < 0.90 ? lowerScoreIssueCategory : "visual-note",
                        severity = score < 0.90 ? lowerScoreIssueSeverity : "info",
                        description = "Loopback provider returned one bounded visual note.",
                        x = 0,
                        y = 0,
                        width = 1,
                        height = 1
                    }
                }
            };

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(response),
                    Encoding.UTF8,
                    "application/json")
            };
        }
    }

    private sealed class ScriptedRealVisualJudgeHttpHandler(
        double score,
        bool passed,
        double? confidence,
        string issueSeverity) : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var body = request.Content is null
                ? "{}"
                : await request.Content.ReadAsStringAsync(cancellationToken);
            using var document = JsonDocument.Parse(body);
            var root = document.RootElement;
            var judgeRequestId = root.GetProperty("judge_request_id").GetString();
            var requestId = root.GetProperty("request_id").GetString();
            var provider = root.GetProperty("provider").GetString();
            var model = root.GetProperty("model").GetString();
            Dictionary<string, object?>[] issues = string.IsNullOrWhiteSpace(issueSeverity)
                ? []
                :
                [
                    new Dictionary<string, object?>
                    {
                        ["id"] = $"{judgeRequestId}:scripted-issue:1",
                        ["category"] = "visual-note",
                        ["severity"] = issueSeverity,
                        ["description"] = "Scripted visual fidelity predicate issue.",
                        ["x"] = 0,
                        ["y"] = 0,
                        ["width"] = 1,
                        ["height"] = 1
                    }
                ];
            var response = new Dictionary<string, object?>
            {
                ["judge_request_id"] = judgeRequestId,
                ["request_id"] = requestId,
                ["provider"] = provider,
                ["judge_result_id"] = "-result",
                ["verdict_id"] = $"{judgeRequestId}:scripted-verdict",
                ["score"] = score,
                ["passed"] = passed,
                ["confidence"] = confidence,
                ["model_id"] = model,
                ["cost_estimate_usd"] = 0.0125,
                ["tokens"] = new Dictionary<string, int>
                {
                    ["input"] = 88,
                    ["output"] = 24,
                    ["total"] = 112
                },
                ["evidence_refs"] = new[] { "scripted-loopback-provider:evidence" },
                ["issues"] = issues
            };

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(response),
                    Encoding.UTF8,
                    "application/json")
            };
        }
    }
}

file sealed class BlockingDpoPolicyBaseProvider : IDesignConversionDpoPolicyBaseProvider
{
    private readonly TaskCompletionSource<object?> _entered = new(
        TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource<object?> _release = new(
        TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly DesignConversionDpoPolicyBaseTestProvider _inner = new();

    public Task Entered => _entered.Task;
    public int InvocationCount { get; private set; }
    public DesignConversionDpoPolicyBaseModelSet? LastModelSet => _inner.LastModelSet;

    public void Release() => _release.TrySetResult(null);

    public async Task<OperationResult<DesignConversionDpoPolicyBaseModelSet>>
        CreateModelSetAsync(CancellationToken cancellationToken)
    {
        InvocationCount++;
        _entered.TrySetResult(null);
        await _release.Task.ConfigureAwait(false);
        return await _inner.CreateModelSetAsync(cancellationToken).ConfigureAwait(false);
    }
}

file sealed class DesignConversionReparsePointFactAttribute : FactAttribute
{
    public DesignConversionReparsePointFactAttribute()
        => Skip = DesignConversionReparsePointTestSupport.GetCapabilitySkipReason();
}

file sealed class DesignConversionReparsePointTheoryAttribute : TheoryAttribute
{
    public DesignConversionReparsePointTheoryAttribute()
        => Skip = DesignConversionReparsePointTestSupport.GetCapabilitySkipReason();
}

file static class DesignConversionReparsePointTestSupport
{
    // HRESULT_FROM_WIN32(ERROR_PRIVILEGE_NOT_HELD).
    private const int HResultPrivilegeNotHeld = unchecked((int)0x80070522);

    public static string? GetCapabilitySkipReason()
    {
        if (!OperatingSystem.IsWindows())
            return null;

        var root = Path.Combine(
            Path.GetTempPath(),
            $"design-conversion-reparse-capability-{Guid.NewGuid():N}");
        var targetPath = Path.Combine(root, "target.png");
        var linkPath = Path.Combine(root, "link.png");
        try
        {
            Directory.CreateDirectory(root);
            File.WriteAllBytes(targetPath, [1]);
            try
            {
                File.CreateSymbolicLink(linkPath, targetPath);
                return null;
            }
            catch (Exception exception)
                when (exception is IOException or UnauthorizedAccessException
                    && exception.HResult == HResultPrivilegeNotHeld)
            {
                return "REPARSE_TEST_PLATFORM_LIMITATION: " +
                    $"{exception.GetType().FullName}, HResult=0x{exception.HResult:X8}: " +
                    exception.Message;
            }
        }
        finally
        {
            if (File.Exists(linkPath))
                File.Delete(linkPath);
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }
}

file sealed class ModelServerMoveNextProbeEnumerable<T> : IEnumerable<T>
{
    private readonly T[] _items;
    private readonly int? _throwOnMoveNextCall;
    private readonly Exception? _exception;

    public ModelServerMoveNextProbeEnumerable(
        IEnumerable<T> items,
        int? throwOnMoveNextCall = null,
        Exception? exception = null)
    {
        _items = items.ToArray();
        _throwOnMoveNextCall = throwOnMoveNextCall;
        _exception = exception;
    }

    public int MoveNextCallCount { get; private set; }
    public int YieldedCount { get; private set; }

    public IEnumerator<T> GetEnumerator() => new Enumerator(this);

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        => GetEnumerator();

    private sealed class Enumerator : IEnumerator<T>
    {
        private readonly ModelServerMoveNextProbeEnumerable<T> _owner;
        private int _index = -1;

        public Enumerator(ModelServerMoveNextProbeEnumerable<T> owner)
            => _owner = owner;

        public T Current => _owner._items[_index];
        object? System.Collections.IEnumerator.Current => Current;

        public bool MoveNext()
        {
            _owner.MoveNextCallCount++;
            if (_owner._throwOnMoveNextCall == _owner.MoveNextCallCount)
                throw _owner._exception ?? new InvalidOperationException("Configured enumeration probe failure.");

            var next = _index + 1;
            if (next >= _owner._items.Length)
                return false;

            _index = next;
            _owner.YieldedCount++;
            return true;
        }

        public void Reset() => throw new NotSupportedException();
        public void Dispose() { }
    }
}

file sealed class ThrowOnAccessDpoTrainingRecordList :
    IReadOnlyList<DesignConversionDpoTrainingRecord>
{
    public int Count => throw new InvalidOperationException(
        "Training records must not be observed before legal authority admission.");

    public DesignConversionDpoTrainingRecord this[int index] =>
        throw new InvalidOperationException(
            "Training records must not be observed before legal authority admission.");

    public IEnumerator<DesignConversionDpoTrainingRecord> GetEnumerator()
        => throw new InvalidOperationException(
            "Training records must not be observed before legal authority admission.");

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        => GetEnumerator();
}
