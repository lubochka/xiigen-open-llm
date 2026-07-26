// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Net;
using System.Buffers.Binary;
using System.Globalization;
using System.IO.Compression;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using DesignConversionDomain;
using DPO;
using LoRA;
using ModelServer.DesignConversion;
using Tokenizer;
using TransformerModel;
using TransformerModel.Config;

public sealed class DesignConversionModelServerApiTests
{
    [Fact]
    public async Task SubmitAndEvaluateAsync_ValidRequest_CallsRuntimeServiceAndReturnsEvaluationEvidence()
    {
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(CreateVerdict("pass117-valid-verdict", 0.91, passed: true));
        var service = new DesignConversionInternalService(renderProvider, comparisonProvider, routingFlow: null);
        var api = new DesignConversionModelServerApi(service, () => DateTimeOffset.Parse("2026-07-02T10:00:00Z"));

        var result = await api.SubmitAndEvaluateAsync(CreateRequest("pass117-valid-api-request"));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionThinClientStatus.Completed, result.Value!.ContractStatus);
        Assert.False(result.Value.ProductReady);
        Assert.False(result.Value.TrainingReadiness.ProductReady);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["product_ready"]);
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
            "runtime_evaluation_completed_local_only",
            result.Value.TrainingReadiness.MeasurementStatus);
        Assert.False(result.Value.DeterministicLogicClaimedAsLearning);
        Assert.Equal("1", result.Value.BoundaryMetadata["render_qa_result_count"]);
        Assert.Equal("1", result.Value.BoundaryMetadata["comparison_artifact_count"]);
        Assert.Equal("2", result.Value.BoundaryMetadata["multi_viewport_required_count"]);
        Assert.Equal("2", result.Value.BoundaryMetadata["multi_viewport_observed_count"]);
        Assert.Equal(bool.TrueString, result.Value.BoundaryMetadata["multi_viewport_complete"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["browser_render_executed"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["browser_e2e_verified"]);
        Assert.Equal(
            "server_owned_browser_render_artifact_receipts_incomplete",
            result.Value.BoundaryMetadata["browser_e2e_failure_reason"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["runtime_ready_verified"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["host_readiness_verified"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["visual_fidelity_verified"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["persistence_verified"]);
        Assert.Equal(bool.TrueString, result.Value.BoundaryMetadata["browser_render_not_executed"]);
        Assert.Equal(bool.TrueString, result.Value.BoundaryMetadata["visual_ai_not_executed"]);
        Assert.Equal(DefaultLocalDesignConversionVisualJudge.StatusOk, result.Value.BoundaryMetadata["visual_judge_status"]);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.LocalVisualLabelerMarker, result.Value.BoundaryMetadata["visual_judge_provider_boundary"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["external_visual_ai_executed"]);
        Assert.Equal("0", result.Value.BoundaryMetadata["estimated_visual_judge_cost_usd"]);
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
        Assert.False(status.Value!.ProductReady);
        Assert.Equal(bool.FalseString, status.Value.BoundaryMetadata["product_ready"]);
        Assert.Equal("2", status.Value.BoundaryMetadata["multi_viewport_required_count"]);
        Assert.Equal("2", status.Value.BoundaryMetadata["multi_viewport_observed_count"]);
        Assert.Equal(bool.TrueString, status.Value.BoundaryMetadata["multi_viewport_complete"]);
        Assert.Equal(bool.FalseString, status.Value.BoundaryMetadata["browser_e2e_verified"]);
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
        var ledgerRoot = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "model-server-api-visual-cost-ledger");
        var ledger = new JsonlDesignConversionVisualCostLedger(ledgerRoot);
        var visualJudge = new VisualCostReceiptJudge(0.0600m);
        var service = new DesignConversionInternalService(
            new RecordingRenderProvider(),
            new RecordingComparisonProvider(CreateVerdict("visual-cost-api-exact-verdict", 0.94, passed: true)),
            visualJudge);
        var api = new DesignConversionModelServerApi(
            service,
            () => DateTimeOffset.Parse("2026-07-15T09:30:00Z"),
            visualCostLedger: ledger);
        var baseRequest = CreateRequest("visual-cost-api-exact-request");
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
                MaxRenderQaRuns = 1,
                BudgetUsd = 0.25m
            }
        };

        var submitted = await api.SubmitAndEvaluateAsync(request);
        Assert.True(submitted.IsSuccess, submitted.ErrorMessage);
        Assert.Equal(DesignConversionThinClientStatus.Completed, submitted.Value.ContractStatus);
        var readback = await api.GetVisualCostRecordsAsync(submitted.Value.JobId);
        var secondReadback = await api.GetVisualCostRecordsAsync(submitted.Value.JobId);

        Assert.Equal(1, visualJudge.InvocationCount);
        Assert.True(readback.IsSuccess, readback.ErrorMessage);
        Assert.True(secondReadback.IsSuccess, secondReadback.ErrorMessage);
        Assert.Single(readback.Value!.Records);
        Assert.Single(secondReadback.Value!.Records);
        var record = Assert.Single(readback.Value.Records);
        Assert.Equal(0.0600m, record.TotalCostUsd);
        Assert.True(record.MeasurementComplete);
        Assert.Equal(0, record.MissingMeasurementCount);
        Assert.Equal(1, record.EvaluationAttemptCount);
        Assert.Equal(
            DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostSourceFixedPerRequest,
            record.CostSource);
        Assert.Equal(1, readback.Value.Summary.RecordCount);
        Assert.Equal(0.0600m, readback.Value.Summary.TotalCostUsd);
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
        Assert.DoesNotContain(submitted.Value.JobId, serialized, StringComparison.Ordinal);
        Assert.DoesNotContain(request.RequestId, serialized, StringComparison.Ordinal);
        Assert.DoesNotContain(request.Html, serialized, StringComparison.Ordinal);
        Assert.DoesNotContain(request.Css, serialized, StringComparison.Ordinal);
        Assert.DoesNotContain(privateMarker, serialized, StringComparison.Ordinal);
        Assert.DoesNotContain(ledgerRoot, serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("billing", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("tenant", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("account", serialized, StringComparison.OrdinalIgnoreCase);
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
    [InlineData(PersistenceReadbackTamperMode.AppendFailure, "scripted_persistence_failure")]
    [InlineData(PersistenceReadbackTamperMode.AppendException, "local_artifact_db_persistence_exception")]
    public async Task RunAsync_ArtifactDbFailurePreservesCompletedNonDurableResult(
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
        Assert.Equal(DesignConversionThinClientStatus.Completed, run.Value!.ContractStatus);
        Assert.NotEmpty(run.Value.Artifacts);
        Assert.Equal(bool.FalseString, run.Value.Job.Metadata["local_artifact_db_persistence_verified"]);
        Assert.Equal(bool.TrueString, run.Value.Job.Metadata["local_artifact_db_persistence_failed"]);
        Assert.Equal("non_durable", run.Value.Job.Metadata["local_artifact_db_persistence_status"]);
        Assert.Equal(expectedErrorCode, run.Value.Job.Metadata["local_artifact_db_persistence_error_code"]);
        Assert.Equal(bool.FalseString, run.Value.Job.Metadata["local_artifact_db_autowrite"]);
        Assert.Equal(bool.TrueString, run.Value.Job.Metadata["n8_12_non_durable_result_preserved"]);
        Assert.Equal(bool.FalseString, run.Value.Job.Metadata["storage_training_artifact_db"]);
        Assert.Equal(bool.FalseString, run.Value.Job.Metadata["local_artifact_db"]);
        Assert.Contains("local_artifact_db_persistence_failed=true", run.Value.Warnings);
        Assert.Contains("local_artifact_db_persistence_status=non_durable", run.Value.Warnings);
        Assert.Contains("local_artifact_db_autowrite=false", run.Value.Warnings);
        Assert.Contains("n8_12_non_durable_result_preserved=true", run.Value.Warnings);
        Assert.Equal(1, artifactDb.AppendCalls);

        var status = await api.GetJobResultAsync(submit.Value.JobId);
        Assert.True(status.IsSuccess, status.ErrorMessage);
        Assert.Equal(DesignConversionThinClientStatus.Completed, status.Value!.ContractStatus);
        AssertPersistenceNotVerified(status.Value!.BoundaryMetadata, "runtime_append_success_not_reported");
        Assert.Equal(bool.FalseString, status.Value.BoundaryMetadata["persistence_server_append_reported"]);

        var records = await artifactDb.ListRecordsAsync();
        Assert.True(records.IsSuccess, records.ErrorMessage);
        Assert.Empty(records.Value!);
        Assert.Equal(1, artifactDb.AppendCalls);
    }

    [Fact]
    public async Task RunAsync_ArtifactDbCancelledAppendPropagatesCancellation()
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

        await Assert.ThrowsAsync<OperationCanceledException>(() => runtime.RunAsync(submit.Value!.JobId));

        Assert.Equal(1, artifactDb.AppendCalls);
        var records = await artifactDb.ListRecordsAsync();
        Assert.True(records.IsSuccess, records.ErrorMessage);
        Assert.Empty(records.Value!);
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

    [Fact]
    public async Task SubmitAndEvaluateAsync_NullBudgetFallbackRendersDesktopAndMobileViewports()
    {
        const string privateMarker = "PRIVATE_N6_3_NULL_BUDGET_SECRET_TOKEN";
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(CreateVerdict("n6-3-null-budget-two-viewports-verdict", 0.91, passed: true));
        var service = new DesignConversionInternalService(renderProvider, comparisonProvider, routingFlow: null);
        var api = new DesignConversionModelServerApi(service, () => DateTimeOffset.Parse("2026-07-11T00:00:00Z"));
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
                ["private_note"] = privateMarker
            }
        };

        var result = await api.SubmitAndEvaluateAsync(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(
            ["source-1440x900", "source-390x844@3"],
            renderProvider.Requests.Select(renderRequest => renderRequest.Resolution.ResolutionId).ToArray());
        Assert.Equal(3.0, renderProvider.Requests[1].Resolution.DeviceScaleFactor);
        Assert.Equal("2", result.Value!.BoundaryMetadata["render_qa_result_count"]);
        Assert.Equal("2", result.Value.BoundaryMetadata["multi_viewport_required_count"]);
        Assert.Equal("2", result.Value.BoundaryMetadata["multi_viewport_observed_count"]);
        Assert.Equal(bool.TrueString, result.Value.BoundaryMetadata["multi_viewport_complete"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["browser_e2e_verified"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["product_ready"]);

        var resultJson = JsonSerializer.Serialize(result.Value);
        Assert.DoesNotContain(privateMarker, resultJson, StringComparison.Ordinal);
        Assert.DoesNotContain(request.Html, resultJson, StringComparison.Ordinal);
        Assert.DoesNotContain(request.Css, resultJson, StringComparison.Ordinal);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["raw_html_included"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["raw_design_tree_included"]);

        var status = api.GetJobResult(result.Value.JobId);
        Assert.True(status.IsSuccess, status.ErrorMessage);
        Assert.Equal("2", status.Value!.BoundaryMetadata["multi_viewport_required_count"]);
        Assert.Equal("2", status.Value.BoundaryMetadata["multi_viewport_observed_count"]);
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
        var api = new DesignConversionModelServerApi(service, () => DateTimeOffset.Parse("2026-07-21T00:00:00Z"));
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
        var api = new DesignConversionModelServerApi(service, () => DateTimeOffset.Parse("2026-07-11T00:00:00Z"));
        var baseRequest = CreateRequest("pass144-missing-viewport-api-request");
        var mobileScreenshot = new DesignConversionModelServerArtifactRef(
            ArtifactId: "screenshot-mobile",
            Kind: "screenshot",
            Width: 390,
            Height: 844,
            Uri: "artifact://pass144/screenshot-mobile.png",
            Sha256: "sha256:screenshot-mobile");
        var mobileDesignRender = new DesignConversionModelServerArtifactRef(
            ArtifactId: "design-render-mobile",
            Kind: "design-render",
            Width: 390,
            Height: 844,
            Uri: "artifact://pass144/design-render-mobile.png",
            Sha256: "sha256:design-render-mobile");
        var request = baseRequest with
        {
            Screenshots = baseRequest.Screenshots!.Append(mobileScreenshot).ToArray(),
            DesignRenders = baseRequest.DesignRenders!.Append(mobileDesignRender).ToArray(),
            Budget = baseRequest.Budget with { MaxRenderQaRuns = 1 }
        };

        var result = await api.SubmitAndEvaluateAsync(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal("2", result.Value!.BoundaryMetadata["multi_viewport_required_count"]);
        Assert.Equal("1", result.Value.BoundaryMetadata["multi_viewport_observed_count"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["multi_viewport_complete"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["browser_e2e_verified"]);
        Assert.Equal(
            "required_viewport_coverage_incomplete",
            result.Value.BoundaryMetadata["browser_e2e_failure_reason"]);
        Assert.Contains(
            $"tier_policy:stop_reason={DesignConversionVisualQaBoundaryPolicy.RenderManifestIncomplete}",
            result.Value.EvidenceRefs);

        var status = api.GetJobResult(result.Value.JobId);
        Assert.True(status.IsSuccess, status.ErrorMessage);
        Assert.Equal("2", status.Value!.BoundaryMetadata["multi_viewport_required_count"]);
        Assert.Equal("1", status.Value.BoundaryMetadata["multi_viewport_observed_count"]);
        Assert.Equal(bool.FalseString, status.Value.BoundaryMetadata["multi_viewport_complete"]);
        Assert.Equal(bool.FalseString, status.Value.BoundaryMetadata["browser_e2e_verified"]);
        Assert.Equal(
            "required_viewport_coverage_incomplete",
            status.Value.BoundaryMetadata["browser_e2e_failure_reason"]);
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
        Assert.Equal(bool.FalseString, result.Value!.BoundaryMetadata["browser_render_executed"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["browser_e2e_verified"]);
        Assert.Equal(
            "not_verified_by_server_owned_browser_render_receipts",
            result.Value.BoundaryMetadata["browser_e2e_verification_state"]);
        Assert.Equal(
            "server_owned_browser_render_artifact_receipts_incomplete",
            result.Value.BoundaryMetadata["browser_e2e_failure_reason"]);
        AssertVisualFidelityFalse(result.Value.BoundaryMetadata, "browser_e2e_not_verified");
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
        AssertContainsFalseBoundaryMarker(status.Value!.EvidenceRefs, "browser_e2e_verified");
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

    [Fact]
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

    [Fact]
    public async Task SubmitAndEvaluateAsync_LearnedRuntimeFlow_IsInjectableAndSurfacesLearnedEvidence()
    {
        var machine = new DesignConversionMachine();
        var training = await machine.TrainBatchAsync(new LearningBatch<DesignConversionDomain.DesignConversionDomain>(
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
                        ["source_structure_hash"] = "pass117-source-structure",
                        ["usefulness_score"] = 0.92,
                        ["pair_family"] = "visual"
                    }),
                    ChosenResponse: JsonSerializer.Serialize(new Dictionary<string, object>
                    {
                        ["visual_score"] = 0.96,
                        ["accepted"] = true
                    }),
                    RejectedResponse: JsonSerializer.Serialize(new Dictionary<string, object>
                    {
                        ["visual_score"] = 0.12,
                        ["accepted"] = false
                    }))
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
        var api = new DesignConversionModelServerApi(service, () => DateTimeOffset.Parse("2026-07-02T10:00:00Z"));

        var result = await api.SubmitAndEvaluateAsync(CreateRequest("pass117-learned-api-request"));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.True(result.Value!.LearnedInferenceApplied);
        Assert.False(result.Value.TrainedModelAuthority);
        Assert.True(result.Value.NotTrainedModelAuthority);
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
                ["local_artifact_db_status_restored"] = bool.TrueString,
                ["local_artifact_db_persistence_verified"] = bool.TrueString,
                ["storage_training_artifact_db"] = bool.TrueString,
                ["local_artifact_db"] = bool.TrueString,
                ["external_ai_visual_judge_executed"] = bool.TrueString,
                ["runtime_created_dpo_candidate"] = bool.TrueString,
                ["review_required"] = bool.TrueString,
                ["approval_required"] = bool.TrueString,
                ["training_candidate_only"] = bool.TrueString,
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
            () => DateTimeOffset.Parse("2026-07-07T09:59:00Z"));

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
        var training = await machine.TrainBatchAsync(new LearningBatch<DesignConversionDomain.DesignConversionDomain>(
            BatchId: "pass151-machine-backed-api-batch",
            DomainTag: DesignConversionMachine.DomainTag,
            TrainingItems:
            [
                new LearningTrainingItem(
                    ItemId: "pass151-machine-backed-critic",
                    Prompt: JsonSerializer.Serialize(new Dictionary<string, object>
                    {
                        ["unit_id"] = DesignConversionRuntimeScaffoldCriticUnit.UnitId,
                        ["source_manifest_hash"] = "pass151-source-manifest",
                        ["source_identity_hash"] = "pass151-source-identity",
                        ["source_structure_hash"] = "pass151-source-structure",
                        ["pair_family"] = "visual"
                    }),
                    ChosenResponse: JsonSerializer.Serialize(new Dictionary<string, object>
                    {
                        ["visual_score"] = 0.97,
                        ["accepted"] = true
                    }),
                    RejectedResponse: JsonSerializer.Serialize(new Dictionary<string, object>
                    {
                        ["visual_score"] = 0.11,
                        ["accepted"] = false
                    }))
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
            utcNow: () => DateTimeOffset.Parse("2026-07-07T10:00:00Z"));
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
                visualCostLedger: visualCostLedger);

            var result = await api.SubmitAndEvaluateAsync(request);

            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.Equal(0, handler.CallCount);
            Assert.Empty(handler.CapturedApiKeys);
            Assert.Empty(handler.CapturedBodies);

            Assert.Equal(DesignConversionThinClientStatus.Completed, result.Value!.ContractStatus);
            Assert.False(result.Value.ProductReady);
            Assert.False(result.Value.PublicHttpEndpointVerified);
            Assert.False(result.Value.ModelUploadReady);
            Assert.False(result.Value.BoundaryMetadata["product_ready"] is "True");
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
            ProviderName: "codex-loopback-visual-provider",
            ModelId: "codex-non-dpo-visual-verifier-v1",
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
            visualCostLedger: CreateServerOwnedVisualCostLedger(fixture.ArtifactRoot));

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
            "codex-loopback-visual-provider",
            "codex-non-dpo-visual-verifier-v1");

        var status = api.GetJobResult(result.Value.JobId);
        Assert.True(status.IsSuccess, status.ErrorMessage);
        AssertTrustedReceiptNetworkBoundary(
            status.Value!.BoundaryMetadata,
            result.Value.JobId,
            result.Value.RequestId,
            "codex-loopback-visual-provider",
            "codex-non-dpo-visual-verifier-v1");
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
            Interlocked.Increment(ref providerCallOrdinal) <= callsPerAttempt ? 0.72 : 0.973);
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
            "codex-loopback-visual-provider",
            result.Value.BoundaryMetadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptProviderNameKey]);
        Assert.Equal(
            "codex-non-dpo-visual-verifier-v1",
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
            Interlocked.Increment(ref providerCallOrdinal) <= callsPerAttempt ? 0.72 : 0.973);
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
    public async Task SubmitAndEvaluateAsync_BrowserBackedTrustedReceiptVerifiesVisualFidelityInEvaluateAndStatus()
    {
        var handler = new EchoingRealVisualJudgeHttpHandler();
        using var fixture = await CreateServerOwnedVisualProviderFixtureAsync(
            CreateSingleAttemptRequest("browser-backed-visual-fidelity-trusted-receipt"));
        var api = CreateBrowserReceiptRuntimeShapedReceiptApi(
            handler,
            serverOwnedImageArtifactRoot: fixture.ArtifactRoot);

        var result = await api.SubmitAndEvaluateAsync(fixture.Request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(2, handler.CallCount);
        AssertTrustedReceiptNetworkBoundary(
            result.Value!.BoundaryMetadata,
            result.Value.JobId,
            result.Value.RequestId,
            "codex-loopback-visual-provider",
            "codex-non-dpo-visual-verifier-v1");
        AssertTrustedVisualFidelityMetadata(result.Value.BoundaryMetadata);
        AssertNoFalseBoundaryMarkers(result.Value.Warnings, "browser_e2e_verified", "visual_fidelity_verified");
        AssertNoFalseBoundaryMarkers(result.Value.EvidenceRefs, "browser_e2e_verified", "visual_fidelity_verified");
        AssertNoFalseBoundaryMarkers(result.Value.RuntimeStatus.Warnings, "browser_e2e_verified", "visual_fidelity_verified");

        var status = api.GetJobResult(result.Value.JobId);
        Assert.True(status.IsSuccess, status.ErrorMessage);
        AssertTrustedVisualFidelityMetadata(status.Value!.BoundaryMetadata);
        Assert.Equal(bool.FalseString, status.Value.BoundaryMetadata["product_ready"]);
        AssertNoFalseBoundaryMarkers(status.Value.EvidenceRefs, "browser_e2e_verified", "visual_fidelity_verified");
        AssertNoFalseBoundaryMarkers(status.Value.RuntimeStatus.Warnings, "browser_e2e_verified", "visual_fidelity_verified");
    }

    [Fact]
    public async Task SubmitAndEvaluateAsync_BrowserBackedLocalProviderKeepsVisualFidelityFalse()
    {
        var api = new DesignConversionModelServerApi(
            new DesignConversionInternalService(
                new BrowserBackedDesignConversionRenderProvider(new TrustedBrowserReceiptBrowserAdapter()),
                new RecordingComparisonProvider(CreateVerdict("browser-backed-local-provider-verdict", 0.973, passed: true)),
                routingFlow: null),
            () => DateTimeOffset.Parse("2026-07-11T00:52:00Z"));

        var result = await api.SubmitAndEvaluateAsync(CreateSingleAttemptRequest("browser-backed-local-provider-visual-fidelity"));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(bool.TrueString, result.Value!.BoundaryMetadata["browser_e2e_verified"]);
        AssertVisualFidelityFalse(result.Value.BoundaryMetadata, "trusted_external_provider_network_receipt_missing");
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["product_ready"]);
        AssertNoFalseBoundaryMarkers(result.Value.Warnings, "browser_e2e_verified");
        AssertNoFalseBoundaryMarkers(result.Value.EvidenceRefs, "browser_e2e_verified");
        AssertNoFalseBoundaryMarkers(result.Value.RuntimeStatus.Warnings, "browser_e2e_verified");
        AssertContainsFalseBoundaryMarker(result.Value.Warnings, "visual_fidelity_verified");
        AssertContainsFalseBoundaryMarker(result.Value.EvidenceRefs, "visual_fidelity_verified");
        AssertContainsFalseBoundaryMarker(result.Value.RuntimeStatus.Warnings, "visual_fidelity_verified");

        var status = api.GetJobResult(result.Value.JobId);
        Assert.True(status.IsSuccess, status.ErrorMessage);
        Assert.Equal(bool.TrueString, status.Value!.BoundaryMetadata["browser_e2e_verified"]);
        AssertVisualFidelityFalse(status.Value.BoundaryMetadata, "trusted_external_provider_network_receipt_missing");
        Assert.Equal(bool.FalseString, status.Value.BoundaryMetadata["product_ready"]);
        AssertNoFalseBoundaryMarkers(status.Value.EvidenceRefs, "browser_e2e_verified");
        AssertNoFalseBoundaryMarkers(status.Value.RuntimeStatus.Warnings, "browser_e2e_verified");
        AssertContainsFalseBoundaryMarker(status.Value.EvidenceRefs, "visual_fidelity_verified");
        AssertContainsFalseBoundaryMarker(status.Value.RuntimeStatus.Warnings, "visual_fidelity_verified");
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
        Assert.False(result.Value!.ProductReady);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["product_ready"]);
        Assert.Equal("2", result.Value.BoundaryMetadata["multi_viewport_required_count"]);
        Assert.Equal("1", result.Value.BoundaryMetadata["multi_viewport_observed_count"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["multi_viewport_complete"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["browser_e2e_verified"]);
        Assert.Equal(
            "required_viewport_coverage_incomplete",
            result.Value.BoundaryMetadata["browser_e2e_failure_reason"]);
        AssertVisualFidelityFalse(result.Value.BoundaryMetadata, "required_viewport_coverage_incomplete");
        Assert.Contains(
            $"tier_policy:stop_reason={DesignConversionVisualQaBoundaryPolicy.RenderManifestIncomplete}",
            result.Value.EvidenceRefs);

        var status = api.GetJobResult(result.Value.JobId);
        Assert.True(status.IsSuccess, status.ErrorMessage);
        Assert.False(status.Value!.ProductReady);
        Assert.Equal(bool.FalseString, status.Value.BoundaryMetadata["product_ready"]);
        Assert.Equal("2", status.Value.BoundaryMetadata["multi_viewport_required_count"]);
        Assert.Equal("1", status.Value.BoundaryMetadata["multi_viewport_observed_count"]);
        Assert.Equal(bool.FalseString, status.Value.BoundaryMetadata["multi_viewport_complete"]);
        Assert.Equal(bool.FalseString, status.Value.BoundaryMetadata["browser_e2e_verified"]);
        Assert.Equal(
            "required_viewport_coverage_incomplete",
            status.Value.BoundaryMetadata["browser_e2e_failure_reason"]);
        AssertVisualFidelityFalse(status.Value.BoundaryMetadata, "required_viewport_coverage_incomplete");
    }

    [Theory]
    [InlineData(VisualFidelityNegativeMode.ReceiptMismatch, "trusted_external_provider_network_receipt_missing")]
    [InlineData(VisualFidelityNegativeMode.ResponseIdentityMismatch, "trusted_external_provider_network_receipt_missing")]
    [InlineData(VisualFidelityNegativeMode.ResponseSchemaMismatch, "trusted_external_provider_network_receipt_missing")]
    [InlineData(VisualFidelityNegativeMode.NaNScore, "canonical_visual_result_score_invalid")]
    [InlineData(VisualFidelityNegativeMode.BelowThreshold, "canonical_visual_result_score_below_threshold")]
    [InlineData(VisualFidelityNegativeMode.MissingConfidence, "canonical_visual_result_confidence_invalid")]
    [InlineData(VisualFidelityNegativeMode.BlockingIssue, "canonical_visual_result_blocking_issue_present")]
    [InlineData(VisualFidelityNegativeMode.IncompleteViewport, "required_viewport_coverage_incomplete")]
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
        AssertVisualFidelityFalse(result.Value!.BoundaryMetadata, expectedReason);
        AssertVisualFidelityNegativeMatrixDiagnostics(result.Value.BoundaryMetadata, mode);

        var status = api.GetJobResult(result.Value.JobId);
        Assert.True(status.IsSuccess, status.ErrorMessage);
        AssertVisualFidelityFalse(status.Value!.BoundaryMetadata, expectedReason);
        AssertVisualFidelityNegativeMatrixDiagnostics(status.Value.BoundaryMetadata, mode);
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
        AssertTrustedReceiptNetworkBoundary(
            result.Value!.BoundaryMetadata,
            result.Value.JobId,
            result.Value.RequestId,
            "codex-loopback-visual-provider",
            "codex-non-dpo-visual-verifier-v1");
        AssertTrustedVisualFidelityMetadata(result.Value.BoundaryMetadata);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["visual_provider_reported_passed"]);
        Assert.Equal(bool.TrueString, result.Value.BoundaryMetadata["visual_provider_server_passed_mismatch"]);

        var status = api.GetJobResult(result.Value.JobId);
        Assert.True(status.IsSuccess, status.ErrorMessage);
        AssertTrustedVisualFidelityMetadata(status.Value!.BoundaryMetadata);
        Assert.Equal(bool.FalseString, status.Value.BoundaryMetadata["visual_provider_reported_passed"]);
        Assert.Equal(bool.TrueString, status.Value.BoundaryMetadata["visual_provider_server_passed_mismatch"]);
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
        Assert.Equal("codex-loopback-visual-provider", crossedPairMetadata["provider_name"]);
        Assert.Equal("codex-non-dpo-visual-verifier-v1", crossedPairMetadata["provider_model"]);
        AssertNoAuthorityReceiptTuple(crossedPairMetadata);
        AssertUntrustedReceiptNetworkBoundary(result.Value.BoundaryMetadata);
        Assert.Equal(bool.TrueString, result.Value.BoundaryMetadata["visual_provider_output_observed"]);
        Assert.Equal("codex-loopback-visual-provider", result.Value.BoundaryMetadata["provider_name"]);
        Assert.Equal("codex-non-dpo-visual-verifier-v1", result.Value.BoundaryMetadata["provider_model"]);
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
            visualCostLedger: CreateServerOwnedVisualCostLedger(artifactRoot));

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
    public async Task DesignConversionDpoTrainingHandoff_ExecutesLogicalDpoUnitThroughStepVector()
    {
        var tokenizer = BuildDpoHandoffTokenizer();
        var policy = new LlmModel(BuildDpoHandoffTinyConfig());
        var reference = new LlmModel(BuildDpoHandoffTinyConfig());
        var checkpointDir = Path.Combine(Path.GetTempPath(), $"design-conversion-dpo-handoff-{Guid.NewGuid():N}");

        try
        {
            var record = BuildDpoHandoffRecord();

            var result = await DesignConversionDpoTrainingHandoff.ExecuteAsync(
                [record],
                policy,
                reference,
                tokenizer,
                checkpointDir,
                new LoraConfig { Rank = 2, Alpha = 4, Dropout = 0, TargetModules = ["q_proj", "v_proj"] },
                maxStepCount: 1);

            Assert.True(result.IsSuccess, result.ErrorMessage);
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
            Assert.Contains("11.DPO:DpoTrainer.StepVector", result.Value.IngredientIds.Concat(result.Value.EvidenceRefs));
            Assert.Contains("DESIGN_CONVERSION_DPO_HANDOFF_LOCAL_ABLATION_GATE_PASSED", result.Value.EvidenceRefs);
            Assert.Contains("DESIGN_CONVERSION_DPO_HANDOFF_NO_REAL_TRAINED_WEIGHTS_CLAIM", result.Value.QualityGate.EvidenceRefs);
            Assert.Single(result.Value.MetricFilePaths);
            Assert.True(File.Exists(result.Value.MetricFilePaths[0]));
            Assert.Single(result.Value.PolicyCheckpointFilePaths);
            var policyCheckpointPath = Path.Combine(checkpointDir, DpoTrainer.BuildPolicyCheckpointFileName(1));
            Assert.Equal(Path.GetFullPath(policyCheckpointPath), Path.GetFullPath(result.Value.PolicyCheckpointFilePaths[0]));
            Assert.True(File.Exists(policyCheckpointPath), "DPO handoff must leave a loadable trained policy checkpoint, not metrics only.");
            Assert.False(File.Exists(policyCheckpointPath + ".tmp"), "DPO handoff must not leave a partial policy checkpoint temp file.");

            var freshLoadedPolicy = new LlmModel(BuildDpoHandoffTinyConfig());
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
    public async Task DpoTrainerPolicyCheckpointBridge_FreshLoadsConvertedCheckpointIntoRuntimeScoring()
    {
        var tokenizer = BuildDpoHandoffTokenizer();
        var policy = new LlmModel(BuildDpoHandoffTinyConfig());
        var reference = new LlmModel(BuildDpoHandoffTinyConfig());
        var checkpointDir = Path.Combine(Path.GetTempPath(), $"design-conversion-dpo-policy-bridge-{Guid.NewGuid():N}");
        var runtimeCheckpointPath = Path.Combine(checkpointDir, "design-conversion-runtime-from-dpo-policy.json");

        try
        {
            var record = BuildDpoHandoffRecord();
            var runtimeInput = DesignConversionTestFixtures.CreateInput(comparisonScore: 0.40, comparisonPassed: false);
            var baselineProbe = await RunMachineScoreAsync(
                new DesignConversionMachine(),
                runtimeInput,
                "dpo-policy-bridge-baseline-runtime");
            Assert.Equal(0.40, baselineProbe.Score, precision: 2);
            Assert.False(baselineProbe.Passed);
            var oldEchoCap = baselineProbe.Score;

            var handoff = await DesignConversionDpoTrainingHandoff.ExecuteAsync(
                [record],
                policy,
                reference,
                tokenizer,
                checkpointDir,
                new LoraConfig { Rank = 2, Alpha = 4, Dropout = 0, TargetModules = ["q_proj", "v_proj"] },
                maxStepCount: 1);
            Assert.True(handoff.IsSuccess, handoff.ErrorMessage);
            var policyCheckpointPath = Assert.Single(handoff.Value!.PolicyCheckpointFilePaths);
            Assert.True(File.Exists(policyCheckpointPath));

            var bridge = await DesignConversionDpoTrainingHandoff.ConvertPolicyCheckpointToRuntimeCheckpointAsync(
                record,
                policyCheckpointPath,
                new LlmModel(BuildDpoHandoffTinyConfig()),
                runtimeCheckpointPath);
            Assert.True(bridge.IsSuccess, bridge.ErrorMessage);
            Assert.True(bridge.Value!.PolicyCheckpointFreshLoadVerified);
            Assert.True(File.Exists(runtimeCheckpointPath));
            Assert.Contains(DesignConversionMachine.DpoTrainerPolicyCheckpointFreshLoadedEvidence, bridge.Value.EvidenceRefs);
            Assert.Contains(DesignConversionMachine.DpoTrainerPolicyCheckpointConvertedEvidence, bridge.Value.EvidenceRefs);
            Assert.Contains(bridge.Value.RuntimeCheckpoint.CriticUnit.LearnedWeights ?? [], weight =>
                weight.EvidenceRefs.Contains(DesignConversionMachine.DpoTrainerPolicyCheckpointFreshLoadedEvidence, StringComparer.Ordinal) &&
                weight.EvidenceRefs.Contains(DesignConversionMachine.DpoTrainerPolicyCheckpointConvertedEvidence, StringComparer.Ordinal) &&
                weight.EvidenceRefs.Contains($"dpo_policy_checkpoint_sha256:{bridge.Value.PolicyCheckpointSha256}", StringComparer.Ordinal));

            var loaded = await DesignConversionMachine.LoadCheckpointAsync(runtimeCheckpointPath);
            Assert.True(loaded.IsSuccess, loaded.ErrorMessage);
            var freshLoadedProbe = await RunMachineScoreAsync(
                loaded.Value!.Machine,
                runtimeInput,
                "dpo-policy-bridge-fresh-loaded-runtime");

            Assert.Contains(DesignConversionMachine.LearnedInferenceAppliedEvidence, freshLoadedProbe.EvidenceRefs);
            Assert.Contains(DesignConversionMachine.DpoTrainerPolicyCheckpointFreshLoadedEvidence, freshLoadedProbe.EvidenceRefs);
            Assert.Contains(DesignConversionMachine.DpoTrainerPolicyCheckpointConvertedEvidence, freshLoadedProbe.EvidenceRefs);
            Assert.Contains(DesignConversionMachine.DpoTrainerPolicyCheckpointRuntimeScoringEvidence, freshLoadedProbe.EvidenceRefs);
            Assert.Contains(freshLoadedProbe.EvidenceRefs, reference =>
                reference.StartsWith("dpo_policy_checkpoint_sha256:", StringComparison.Ordinal));
            Assert.True(
                freshLoadedProbe.Score > oldEchoCap + 0.20,
                $"fresh loaded DpoTrainer bridge score {freshLoadedProbe.Score:0.######} stayed within old echo+0.20 cap {oldEchoCap + 0.20:0.######}");
            Assert.True(freshLoadedProbe.Score >= 0.85, $"fresh loaded DpoTrainer bridge score did not reach runtime threshold: {freshLoadedProbe.Score:0.######}");
            Assert.True(freshLoadedProbe.Passed);
            Assert.Equal("11.DPO/DpoTrainer:dpo_policy_checkpoint_converted", freshLoadedProbe.Metrics["local_checkpoint_weights_source"]);
            Assert.Equal(bool.TrueString, freshLoadedProbe.Metrics["dpo_trainer_policy_checkpoint_converted"]);
            Assert.Equal(bool.TrueString, freshLoadedProbe.Metrics["dpo_trainer_policy_checkpoint_fresh_loaded"]);
            Assert.Equal(bool.FalseString, freshLoadedProbe.Metrics["trained_model_authority"]);
            Assert.Equal(bool.TrueString, freshLoadedProbe.Metrics["not_trained_model_authority"]);
            Assert.Contains("fresh-loaded 11.DPO/DpoTrainer policy checkpoint", freshLoadedProbe.Notes, StringComparison.Ordinal);
        }
        finally
        {
            if (Directory.Exists(checkpointDir))
                Directory.Delete(checkpointDir, recursive: true);
        }
    }

    [Fact]
    public async Task DpoTrainerPolicyCheckpointBridge_PromotedLocalModelReadinessMetadataPreservesSourceEvidence()
    {
        var tokenizer = BuildDpoHandoffTokenizer();
        var policy = new LlmModel(BuildDpoHandoffTinyConfig());
        var reference = new LlmModel(BuildDpoHandoffTinyConfig());
        var checkpointDir = Path.Combine(Path.GetTempPath(), $"design-conversion-dpo-policy-promotion-{Guid.NewGuid():N}");
        var runtimeCheckpointPath = Path.Combine(checkpointDir, "design-conversion-runtime-from-dpo-policy.json");

        try
        {
            Directory.CreateDirectory(checkpointDir);
            var record = BuildDpoHandoffRecord();
            var handoff = await DesignConversionDpoTrainingHandoff.ExecuteAsync(
                [record],
                policy,
                reference,
                tokenizer,
                checkpointDir,
                new LoraConfig { Rank = 2, Alpha = 4, Dropout = 0, TargetModules = ["q_proj", "v_proj"] },
                maxStepCount: 1);
            Assert.True(handoff.IsSuccess, handoff.ErrorMessage);
            var policyCheckpointPath = Assert.Single(handoff.Value!.PolicyCheckpointFilePaths);

            var bridge = await DesignConversionDpoTrainingHandoff.ConvertPolicyCheckpointToRuntimeCheckpointAsync(
                record,
                policyCheckpointPath,
                new LlmModel(BuildDpoHandoffTinyConfig()),
                runtimeCheckpointPath);
            Assert.True(bridge.IsSuccess, bridge.ErrorMessage);

            var loaded = await DesignConversionMachine.LoadCheckpointAsync(runtimeCheckpointPath);
            Assert.True(loaded.IsSuccess, loaded.ErrorMessage);
            var artifacts = await WriteDpoBridgePromotionArtifactsAsync(checkpointDir, runtimeCheckpointPath);
            var pipelineResult = await BuildDpoBridgePromotionReadyPipelineResultAsync();
            Assert.Contains(
                DesignConversionTrainingPipeline.SourceDisjointHeldOutReadinessGatePassedEvidence,
                pipelineResult.LifecycleEvidence.EvidenceRefs);
            Assert.Contains(
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
                BuildDpoBridgePromotionGate(artifacts));
            Assert.True(publication.IsSuccess, publication.ErrorMessage);
            Assert.Equal(DesignConversionPromotionDecisionKind.Promote, publication.Value!.Decision.Kind);
            Assert.True(publication.Value.Manifest.LocalPromotionReceiptWritten);
            Assert.False(publication.Value.Manifest.ModelServerUploadAttempted);
            Assert.False(publication.Value.Manifest.ModelServerPromotionClaimed);
            Assert.Contains(
                DesignConversionTrainingPipeline.SourceDisjointHeldOutReadinessGatePassedEvidence,
                publication.Value.Manifest.EvidenceRefs);

            var promoted = await DesignConversionPromotedLocalModelLoader.LoadAsync(
                publication.Value,
                runtimeCheckpointPath);
            Assert.True(promoted.IsSuccess, promoted.ErrorMessage);
            Assert.True(promoted.Value!.Model.CheckpointFreshLoadVerified);
            Assert.True(promoted.Value.Model.CheckpointContainsRealTrainedWeights);
            Assert.True(promoted.Value.Model.LocalOnly);
            Assert.Contains(DesignConversionMachine.DpoTrainerPolicyCheckpointFreshLoadedEvidence, promoted.Value.Model.EvidenceRefs);
            Assert.Contains(DesignConversionMachine.DpoTrainerPolicyCheckpointConvertedEvidence, promoted.Value.Model.EvidenceRefs);
            Assert.Contains($"dpo_policy_checkpoint_sha256:{bridge.Value!.PolicyCheckpointSha256}", promoted.Value.Model.EvidenceRefs);
            Assert.True(promoted.Value.Model.DpoTrainerPolicyCheckpointConverted);
            Assert.True(promoted.Value.Model.DpoTrainerPolicyCheckpointFreshLoaded);
            Assert.True(promoted.Value.Model.SourceDisjointHeldOutReadinessGatePassed);
            Assert.True(promoted.Value.Model.ReadinessEvaluable);
            Assert.Equal("11.DPO/DpoTrainer:dpo_policy_checkpoint_converted", promoted.Value.Model.LocalCheckpointWeightsSource);
            Assert.Equal("11.DPO/DpoTrainer", promoted.Value.Model.SourceProvenanceRef);
            Assert.Equal($"dpo_policy_checkpoint_sha256:{bridge.Value.PolicyCheckpointSha256}", promoted.Value.Model.DpoPolicyCheckpointSha256Ref);

            var service = DesignConversionPromotedLocalModelLoader.CreateInternalService(
                promoted.Value,
                new RecordingRenderProvider(),
                new RecordingComparisonProvider(CreateVerdict("dpo-bridge-promoted-local-verdict", 0.40, passed: false)));
            var api = new DesignConversionModelServerApi(
                service,
                () => DateTimeOffset.Parse("2026-07-08T10:00:00Z"),
                promotedLocalModel: promoted.Value.Model);

            var response = await api.SubmitAndEvaluateAsync(CreateRequest("dpo-bridge-promoted-local-modelserver-request"));
            Assert.True(response.IsSuccess, response.ErrorMessage);
            Assert.True(response.Value!.LearnedInferenceApplied);
            Assert.True(response.Value.ModelTrainingVerified);
            Assert.False(response.Value.ModelUploadReady);
            Assert.False(response.Value.DeploymentVerified);
            Assert.False(response.Value.ProductReady);
            Assert.False(response.Value.TrainedModelAuthority);
            Assert.True(response.Value.NotTrainedModelAuthority);
            Assert.Equal("source_disjoint_heldout_gate_passed", response.Value.TrainingReadiness.SourceDisjointStatus);
            Assert.Equal(bool.FalseString, response.Value.BoundaryMetadata["product_ready"]);
            Assert.Equal(bool.FalseString, response.Value.BoundaryMetadata["trained_model_authority"]);
            Assert.Equal(bool.TrueString, response.Value.BoundaryMetadata["not_trained_model_authority"]);
            Assert.Equal("source_disjoint_heldout_gate_passed",
                response.Value.BoundaryMetadata["source_disjoint_status"]);
            Assert.Equal("source_disjoint_heldout_gate_passed",
                response.Value.BoundaryMetadata["local_promoted_source_disjoint_heldout_readiness_status"]);
            Assert.Equal(bool.TrueString,
                response.Value.BoundaryMetadata["local_promoted_source_disjoint_heldout_readiness_gate_passed"]);
            Assert.False(response.Value.TrainingReadiness.ModelUploadReady);
            Assert.False(response.Value.TrainingReadiness.ProductReady);
            Assert.Contains(
                DesignConversionTrainingPipeline.SourceDisjointHeldOutReadinessGatePassedEvidence,
                response.Value.EvidenceRefs);
            Assert.Equal("11.DPO/DpoTrainer:dpo_policy_checkpoint_converted", response.Value.BoundaryMetadata["local_promoted_checkpoint_weights_source"]);
            Assert.Equal("11.DPO/DpoTrainer", response.Value.BoundaryMetadata["local_promoted_source_provenance_ref"]);
            Assert.Equal(bool.TrueString, response.Value.BoundaryMetadata["local_promoted_dpo_trainer_policy_checkpoint_converted"]);
            Assert.Equal(bool.TrueString, response.Value.BoundaryMetadata["local_promoted_dpo_trainer_policy_checkpoint_fresh_loaded"]);
            Assert.Equal(
                $"dpo_policy_checkpoint_sha256:{bridge.Value.PolicyCheckpointSha256}",
                response.Value.BoundaryMetadata["local_promoted_dpo_policy_checkpoint_sha256_ref"]);
            Assert.Contains(DesignConversionMachine.DpoTrainerPolicyCheckpointRuntimeScoringEvidence, response.Value.EvidenceRefs);

            var status = api.GetJobResult(response.Value.JobId);
            Assert.True(status.IsSuccess, status.ErrorMessage);
            Assert.Equal("11.DPO/DpoTrainer:dpo_policy_checkpoint_converted", status.Value!.BoundaryMetadata["local_promoted_checkpoint_weights_source"]);
            Assert.Equal(bool.FalseString, status.Value.BoundaryMetadata["product_ready"]);
            Assert.Equal(bool.FalseString, status.Value.BoundaryMetadata["trained_model_authority"]);
            Assert.Equal(bool.TrueString, status.Value.BoundaryMetadata["not_trained_model_authority"]);
        }
        finally
        {
            if (Directory.Exists(checkpointDir))
                Directory.Delete(checkpointDir, recursive: true);
        }
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
            Assert.Equal("PROMOTION_BLOCKED_MEASUREMENT_GATE_NOT_PASSED", sameSourcePipeline.LifecycleEvidence.PromotionStatus);
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

            var publication = await new DesignConversionPromotionPublisher(new LocalReceiptPromotionUploader()).PublishAsync(
                new DesignConversionMachine(),
                passedStatusOnlyPipeline,
                "dpo-bridge-source-disjoint-heldout-required",
                artifacts,
                BuildDpoBridgePromotionGate(artifacts));

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
            promotedLocalModel: BuildDpoTrainerPromotedLocalModel());

        var response = await api.SubmitAndEvaluateAsync(CreateRequest("dpo-bridge-missing-heldout-gate-request"));

        Assert.True(response.IsSuccess, response.ErrorMessage);
        Assert.False(response.Value!.ModelTrainingVerified);
        Assert.False(response.Value.ModelUploadReady);
        Assert.False(response.Value.DeploymentVerified);
        Assert.False(response.Value.ProductReady);
        Assert.False(response.Value.TrainedModelAuthority);
        Assert.True(response.Value.NotTrainedModelAuthority);
        Assert.Equal(
            "promoted_local_checkpoint_loaded_source_disjoint_heldout_not_proven",
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
    public async Task DesignConversionDpoTrainingHandoff_BlocksLowDeltaLocalAblationGateWithoutDpoStep()
    {
        var tokenizer = BuildDpoHandoffTokenizer();
        var policy = new LlmModel(BuildDpoHandoffTinyConfig());
        var reference = new LlmModel(BuildDpoHandoffTinyConfig());
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
                [lowDeltaRecord],
                policy,
                reference,
                tokenizer,
                checkpointDir,
                new LoraConfig { Rank = 2, Alpha = 4, Dropout = 0, TargetModules = ["q_proj", "v_proj"] },
                maxStepCount: 1);

            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.Equal(1, result.Value!.RecordCount);
            Assert.Equal(1, result.Value.PreferenceCount);
            Assert.Equal(0, result.Value.DpoStepCount);
            Assert.Empty(result.Value.MetricFilePaths);
            Assert.Empty(result.Value.IngredientIds);
            Assert.False(result.Value.QualityGate.PassesQualityGate);
            Assert.Equal(0.52, result.Value.QualityGate.PreferredScore, precision: 2);
            Assert.Equal(0.49, result.Value.QualityGate.BaselineScore, precision: 2);
            Assert.Equal(0.03, result.Value.QualityGate.ScoreDelta, precision: 2);
            Assert.Equal(0.10, result.Value.QualityGate.Threshold, precision: 2);
            Assert.Contains("score_delta_below_threshold:0.03<0.1", result.Value.QualityGate.GateReason);
            Assert.Contains("DESIGN_CONVERSION_DPO_HANDOFF_LOCAL_ABLATION_GATE_FAILED", result.Value.EvidenceRefs);
            Assert.Contains("passes_quality_gate:false", result.Value.QualityGate.EvidenceRefs);

            var serialized = JsonSerializer.Serialize(result.Value);
            Assert.DoesNotContain("ProductReady", serialized, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("public_http_endpoint", serialized, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("deploy", serialized, StringComparison.OrdinalIgnoreCase);
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
        var api = new DesignConversionModelServerApi(service, () => DateTimeOffset.Parse("2026-07-09T10:00:00Z"));
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
    [InlineData(" free ", "free", 1)]
    [InlineData("PAID", "PAID", 2)]
    [InlineData("pro", "pro", 4)]
    public async Task SubmitAndEvaluateAsync_ServerOwnedTierZeroMaxImprovementCandidatesUsesEntitlementCap(
        string tier,
        string publishedTier,
        int expectedMaxAttempts)
    {
        var normalizedTier = tier.Trim().ToLowerInvariant();
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(CreateVerdict(
            $"submit-path-{normalizedTier}-tier-zero-budget-comparison-verdict",
            0.70,
            passed: false));
        var visualJudge = new SubmitPathScriptedVisualJudge(_ => SubmitPathJudgeScript.FromScore(0.70, cost: 0.01m));
        var service = new DesignConversionInternalService(renderProvider, comparisonProvider, visualJudge);
        var api = new DesignConversionModelServerApi(service, () => DateTimeOffset.Parse("2026-07-09T10:00:00Z"));
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
        Assert.Equal(DesignConversionThinClientStatus.Completed, result.Value!.ContractStatus);
        var observedAttempts = renderProvider.Requests.Count / 2;
        Assert.InRange(observedAttempts, 1, expectedMaxAttempts);
        Assert.Equal(observedAttempts * 2, renderProvider.Requests.Count);
        Assert.Equal(observedAttempts * 2, comparisonProvider.Requests.Count);
        Assert.Equal(observedAttempts * 2, visualJudge.Requests.Count);
        Assert.Contains($"tier_policy:tier={publishedTier}", result.Value.EvidenceRefs);
        Assert.Contains("tier_policy:submitted_max_improvement_candidates=0", result.Value.EvidenceRefs);
        Assert.Contains(
            $"tier_policy:effective_max_improvement_candidates={expectedMaxAttempts}",
            result.Value.EvidenceRefs);
        Assert.Contains(DesignConversionVisualQaBoundaryPolicy.TierBudgetEntitlementDefaultAppliedMarker, result.Value.EvidenceRefs);
        Assert.Contains(DesignConversionVisualQaBoundaryPolicy.TierIterationEntitlementDefaultAppliedMarker, result.Value.EvidenceRefs);
        Assert.Contains("tier_policy:iteration_entitlement_default_applied=true", result.Value.EvidenceRefs);
        if (expectedMaxAttempts == 1)
        {
            Assert.DoesNotContain(
                DesignConversionImprovementLoopThinClientJobEvaluator.SubmitPathLoopExecutedMarker,
                result.Value.EvidenceRefs);
            Assert.Contains("tier_policy:iteration_cap_reached", result.Value.EvidenceRefs);
        }
        else
        {
            Assert.Contains(
                DesignConversionImprovementLoopThinClientJobEvaluator.SubmitPathLoopExecutedMarker,
                result.Value.EvidenceRefs);
            Assert.Contains($"submit_path_improvement_loop_attempt_count={observedAttempts}", result.Value.Warnings);
            Assert.Contains($"submit_path_improvement_loop_max_attempts={expectedMaxAttempts}", result.Value.Warnings);
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
        var api = new DesignConversionModelServerApi(service, () => DateTimeOffset.Parse("2026-07-09T10:00:00Z"));
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
        Assert.Equal(DesignConversionThinClientStatus.Completed, result.Value!.ContractStatus);
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
        var api = new DesignConversionModelServerApi(service, () => DateTimeOffset.Parse("2026-07-03T09:00:00Z"));
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
        Assert.Equal(DesignConversionThinClientStatus.Completed, root.GetProperty("contractStatus").GetString());
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
        Assert.Equal(bool.FalseString, metadata.GetProperty("ModelUploadReady").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("trained_model_authority").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("not_trained_model_authority").GetString());
        Assert.Equal(DefaultLocalDesignConversionVisualJudge.StatusOk, metadata.GetProperty("visual_judge_status").GetString());
        Assert.Equal(
            DesignConversionVisualQaBoundaryPolicy.LocalVisualLabelerMarker,
            metadata.GetProperty("visual_judge_provider_boundary").GetString());
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
        var request = CreateTrustedVisualCostRouteRequest("trusted-visual-cost-missing-budget-store");

        var probe = await SubmitTrustedVisualCostRouteAsync(request, wireVisualCostBudgetStore: false);

        Assert.Equal(400, probe.Response.StatusCode);
        Assert.Contains(
            DesignConversionThinClientJobRuntime.TrustedVisualCostBudgetStoreRequiredErrorCode,
            probe.Response.BodyJson,
            StringComparison.Ordinal);
        Assert.Empty(probe.Evaluator.Requests);
        Assert.Empty(probe.Events);
        Assert.Null(probe.BudgetStore);
        Assert.Equal(0, probe.VisualJudge.InvocationCount);
        Assert.DoesNotContain(TrustedVisualCostRouteToken, probe.Response.BodyJson, StringComparison.Ordinal);
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
    public async Task SubmitAndEvaluateAsync_NullRouteContextSpoofedVisualCostMarkersRemainNonAuthoritative()
    {
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(CreateVerdict("trusted-visual-cost-direct-spoof", 0.93, passed: true));
        var service = new DesignConversionInternalService(renderProvider, comparisonProvider, routingFlow: null);
        var api = new DesignConversionModelServerApi(
            service,
            () => DateTimeOffset.Parse("2026-07-17T09:00:00Z"),
            visualCostLedger: new SeededVisualCostLedger([]));
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

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(bool.FalseString, result.Value!.RuntimeStatus.Provenance["trusted_visual_cost_entitlement_bound"]);
        Assert.Equal(bool.FalseString, result.Value.RuntimeStatus.Provenance["trusted_visual_cost_reservation_store_wired"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["trusted_visual_cost_entitlement_bound"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["trusted_visual_cost_reservation_store_wired"]);
        Assert.False(result.Value.RuntimeStatus.Provenance.ContainsKey("trusted_visual_cost_entitlement_monthly_limit_usd"));
        Assert.Contains(
            "request_metadata:" + DesignConversionModelServerApi.ServerAssignedMonthlyVisualCostLimitUsdMetadataKey,
            result.Value.RuntimeStatus.Provenance.Keys);
        Assert.Contains(
            "budget_metadata:trusted_visual_cost_reservation_store_wired",
            result.Value.RuntimeStatus.Provenance.Keys);
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
        var api = new DesignConversionModelServerApi(service, () => DateTimeOffset.Parse("2026-07-03T11:00:00Z"));
        var handler = new DesignConversionModelServerHttpHandler(api);
        var registry = new DesignConversionModelServerEndpointRegistry();

        var registered = registry.RegisterDesignConversionEvaluate(handler);

        Assert.True(registered.IsSuccess, registered.ErrorMessage);
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
    public async Task EndpointRegistry_RuntimeException_ReturnsGenericUnhandledFailureWithoutPrivateLeak()
    {
        const string privateExceptionMarker = "PRIVATE_N12_1_RUNTIME_THROW_SECRET";
        const string privateRequestMarker = "PRIVATE_N12_1_REGISTRY_REQUEST_SECRET";
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
            DesignTree = baseRequest.DesignTree with { Payload = $"{{\"type\":\"FRAME\",\"name\":\"{privateRequestMarker}\"}}" },
            OriginalTree = baseRequest.OriginalTree with { Payload = $"{{\"type\":\"FRAME\",\"name\":\"original-{privateRequestMarker}\"}}" },
            Html = $"<main data-secret=\"{privateRequestMarker}\">Dashboard</main>",
            Css = $".screen{{background-image:url('{privateRequestMarker}');}}"
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
        Assert.False(invoked!.IsSuccess);
        Assert.Equal(DesignConversionModelServerEndpointRegistry.EndpointUnhandledErrorCode, invoked.ErrorCode);
        Assert.Equal(DesignConversionModelServerEndpointRegistry.EndpointUnhandledErrorMessage, invoked.ErrorMessage);
        Assert.DoesNotContain(privateExceptionMarker, invoked.ErrorMessage, StringComparison.Ordinal);
        Assert.DoesNotContain(privateRequestMarker, invoked.ErrorMessage, StringComparison.Ordinal);
        Assert.DoesNotContain(nameof(InvalidOperationException), invoked.ErrorMessage, StringComparison.Ordinal);
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

        var invoked = await registry.InvokeAsync(request, cancellationSource.Token);

        Assert.False(invoked.IsSuccess);
        Assert.Equal(DesignConversionModelServerEndpointRegistry.EndpointCancelledErrorCode, invoked.ErrorCode);
        Assert.Equal(DesignConversionModelServerEndpointRegistry.EndpointCancelledErrorMessage, invoked.ErrorMessage);
        Assert.DoesNotContain(privateExceptionMarker, invoked.ErrorMessage, StringComparison.Ordinal);
        Assert.Empty(renderProvider.Requests);
        Assert.Empty(comparisonProvider.Requests);
    }

    private static DesignConversionModelServerApi CreateApi(IDesignConversionLocalArtifactDb? artifactDb = null)
    {
        var service = new DesignConversionInternalService(
            new RecordingRenderProvider(),
            new RecordingComparisonProvider(CreateVerdict("pass117-fake-verdict", 0.90, passed: true)),
            routingFlow: null);
        var utcNow = () => DateTimeOffset.Parse("2026-07-02T10:00:00Z");
        if (artifactDb is null)
            return new DesignConversionModelServerApi(service, utcNow);

        return new DesignConversionModelServerApi(
            new DesignConversionInternalServiceThinClientJobEvaluator(service),
            utcNow,
            artifactDb);
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
        yield return [PersistenceReadbackTamperMode.MismatchedConversionResult, "conversion_result_not_bound"];
        yield return [PersistenceReadbackTamperMode.MissingRenderRef, "render_refs_not_bound"];
        yield return [PersistenceReadbackTamperMode.MissingIndexMarker, "record_index_not_consistent"];
        yield return [PersistenceReadbackTamperMode.ReadbackFailure, "fresh_readback_failed_fake_readback"];
        yield return [PersistenceReadbackTamperMode.ReadbackException, "fresh_readback_exception_invalidoperationexception"];
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

    private static string SafePathSegment(string value)
    {
        var builder = new StringBuilder(value.Length);
        foreach (var ch in value)
            builder.Append(char.IsLetterOrDigit(ch) || ch is '-' or '_' ? ch : '-');

        var segment = builder.ToString().Trim('-');
        return string.IsNullOrWhiteSpace(segment) ? "persistence" : segment;
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
        MismatchedConversionResult,
        MissingRenderRef,
        MissingIndexMarker,
        ReadbackFailure,
        ReadbackException
    }

    private sealed class TamperingLocalArtifactDb : IDesignConversionLocalArtifactDb
    {
        private readonly PersistenceReadbackTamperMode _mode;
        private readonly JsonlDesignConversionLocalArtifactDb _inner;

        public TamperingLocalArtifactDb(PersistenceReadbackTamperMode mode)
        {
            _mode = mode;
            RootDirectory = CreateArtifactDbTempRoot($"persistence-tamper-{mode}");
            _inner = new JsonlDesignConversionLocalArtifactDb(RootDirectory);
        }

        public string RootDirectory { get; }
        public int AppendCalls { get; private set; }
        public DesignConversionLocalArtifactDbQuery? LastQuery { get; private set; }

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
            LastQuery = query;

            if (_mode == PersistenceReadbackTamperMode.ReadbackException)
                throw new InvalidOperationException("Fake readback exception.");

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

    private static async Task<ServerOwnedProofSubmitProbe> SubmitWithServerOwnedProofAsync(
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
            artifactRefResolver: resolver);
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
            EvidenceRefs: result.Value.FlowResult.EvidenceIds,
            Metrics: result.Value.FlowResult.Metrics,
            Notes: result.Value.FlowResult.Summary.Notes);
    }

    private sealed record RuntimeScoreProbe(
        double Score,
        bool Passed,
        IReadOnlyList<string> EvidenceRefs,
        IReadOnlyDictionary<string, string> Metrics,
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

        var result = await new DesignConversionTrainingPipeline().RunAsync(
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
                EvidenceRefs: [$"{gateId}:source-disjoint-heldout-metrics"]),
            IntegrityProof: new DesignConversionMeasurementIntegrityProof(
                SourceDisjoint: sourceDisjoint,
                LeakageDetected: false,
                HiddenDeterminismDetected: false,
                AblationPassed: true,
                EvidenceRefs: [$"{gateId}:anti-leakage-ablation"]),
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
        DesignConversionTrainingManifestArtifacts artifacts)
    {
        var artifactPaths = new[]
        {
            artifacts.CheckpointPath,
            artifacts.TokenizerPath,
            artifacts.PolicyPath
        };

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
            EvidenceRefs: ["DPO_BRIDGE_LOCAL_PROMOTION_GATE_TEST_EVIDENCE"]);
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
                EvidenceRefs: []));
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

    private static async Task<TrustedVisualCostRouteProbe> SubmitTrustedVisualCostRouteAsync(
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
            visualCostBudgetStore: budgetStore);
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

    private static DesignConversionModelServerApi CreateRuntimeShapedReceiptApi(
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
            ProviderName: "codex-loopback-visual-provider",
            ModelId: "codex-non-dpo-visual-verifier-v1",
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
            visualCostLedger: CreateServerOwnedVisualCostLedger(artifactRoot));
    }

    private static DesignConversionModelServerApi CreateBrowserReceiptRuntimeShapedReceiptApi(
        HttpMessageHandler handler,
        Func<IDesignConversionComparisonProvider, IDesignConversionComparisonProvider>? configureComparisonProvider = null,
        Func<IDesignConversionVisualJudge, IDesignConversionVisualJudge>? configureVisualJudge = null,
        IDesignConversionLocalArtifactDb? artifactDb = null,
        string? serverOwnedCostPerRequestUsd = null,
        string? serverOwnedImageArtifactRoot = null)
    {
        var artifactRoot = serverOwnedImageArtifactRoot ?? CreateServerOwnedVisualArtifactRoot();
        Directory.CreateDirectory(artifactRoot);
        var input = new DesignConversionExternalVisualProviderConfigInput(
            Endpoint: "https://visual-judge.example/v1/judge",
            ProviderName: "codex-loopback-visual-provider",
            ModelId: "codex-non-dpo-visual-verifier-v1",
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
        return new DesignConversionModelServerApi(
            new DesignConversionInternalServiceThinClientJobEvaluator(service),
            () => DateTimeOffset.Parse("2026-07-11T00:50:00Z"),
            artifactDb,
            visualCostLedger: CreateServerOwnedVisualCostLedger(artifactRoot));
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

    private static void AssertTrustedVisualFidelityMetadata(IReadOnlyDictionary<string, string> metadata)
    {
        Assert.True(
            string.Equals(bool.TrueString, metadata["visual_fidelity_verified"], StringComparison.Ordinal),
            FormatBoundaryMetadata(metadata));
        Assert.Equal(
            "server_owned_browser_render_and_visual_provider_receipts",
            metadata["visual_fidelity_verification_authority"]);
        Assert.Equal(
            "verified_by_server_owned_browser_and_visual_provider_receipts",
            metadata["visual_fidelity_verification_state"]);
        Assert.Equal("none", metadata["visual_fidelity_failure_reason"]);
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
        Assert.Equal(bool.TrueString, metadata["browser_e2e_verified"]);
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
        switch (mode)
        {
            case VisualFidelityNegativeMode.BelowThreshold:
                Assert.Equal("0.82", metadata["visual_fidelity_score"]);
                Assert.Equal(bool.FalseString, metadata["visual_fidelity_score_threshold_met"]);
                Assert.Equal(bool.FalseString, metadata["visual_fidelity_passed"]);
                break;
        }
    }

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
    {
        var request = CreateRequest(requestId);
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
                Metadata: budget.Metadata ?? new Dictionary<string, string>()),
            Provenance: mergedProvenance);
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
                    ExternalCallExecuted: false,
                    Metadata: new Dictionary<string, string>
                    {
                        [DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker] = bool.TrueString,
                        ["external_ai_visual_judge_executed"] = bool.FalseString,
                        [ExternalHttpDesignConversionVisualJudge.ServerCallReceiptProviderRequestIdKey] = request.JudgeRequestId,
                        [ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVisualJudgeRequestIdKey] = request.JudgeRequestId
                    }),
                EstimatedCostUsd: script.Cost,
                TokenMetadata: new Dictionary<string, double>(),
                EvidenceRefs: verdict.EvidenceRefs,
                Metadata: new Dictionary<string, string>
                {
                    ["external_ai_visual_judge_executed"] = bool.FalseString,
                    [ExternalHttpDesignConversionVisualJudge.ServerCallReceiptProviderRequestIdKey] = request.JudgeRequestId,
                    [ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVisualJudgeRequestIdKey] = request.JudgeRequestId
                })));
        }
    }

    private sealed record SubmitPathJudgeScript(double Score, decimal? Cost)
    {
        public static SubmitPathJudgeScript FromScore(double score, decimal? cost)
            => new(score, cost);
    }

    private sealed class LocalReceiptPromotionUploader : IDesignConversionPromotionUploader
    {
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
        Func<string, double>? resolveScore = null) : HttpMessageHandler
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
                        category = "visual-note",
                        severity = "info",
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
