// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using AgentInfrastructure;
using DesignConversionDomain;

public sealed class DesignConversionLocalFullLoopE2eTests
{
    [Fact]
    public async Task LocalNonProviderLoop_SavesConversionDisplaysUsefulnessAndGatesTrainingRows()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "pass87-local-full-loop");
        var db = new JsonlDesignConversionLocalArtifactDb(root);

        var localEvaluation = await new DesignConversionInternalService(
            new Pass87RenderProvider(),
            new Pass87ComparisonProvider(),
            new Pass87TestProviderOnlyVisualJudge()).EvaluateAsync(CreateRequest("pass87-local", "pass87-local-request"));
        Assert.True(localEvaluation.IsSuccess, localEvaluation.ErrorMessage);
        Assert.False(localEvaluation.Value!.VisualQaBoundaryTrace.ExternalAiVisualJudgeExecuted);
        Assert.False(localEvaluation.Value.VisualQaBoundaryTrace.ProductReady);
        Assert.Contains(DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker, localEvaluation.Value.EvidenceRefs);
        Assert.Contains(DesignConversionVisualQaBoundaryPolicy.ImprovementLoopMarker, localEvaluation.Value.EvidenceRefs);

        var safeA = BuildTrainingRecord("pass87-safe-a", "pass87-source-a", 0.94);
        var safeB = BuildTrainingRecord("pass87-safe-b", "pass87-source-b", 0.89);
        var privateUnsafe = BuildTrainingRecord("pass87-private-unsafe", "pass87-source-private", 0.71) with
        {
            License = DesignConversionDpoGeneratorTests.CreateLicense(
                allowsTraining: true,
                containsPrivateData: true)
        };

        var localAppend = await db.AppendRecordAsync(DesignConversionLocalArtifactDbRecord.FromEvaluation(
            localEvaluation.Value,
            [safeA, safeB, privateUnsafe],
            DateTimeOffset.UnixEpoch,
            new Dictionary<string, string>
            {
                ["pass87_local_full_loop_e2e"] = bool.TrueString,
                ["storage_to_dpo_to_display"] = bool.TrueString
            }));
        Assert.True(localAppend.IsSuccess, localAppend.ErrorMessage);

        var defaultLocalJudgeEvaluation = await new DesignConversionInternalService(
            new Pass87RenderProvider(),
            new Pass87ComparisonProvider()).EvaluateAsync(CreateRequest("pass87-default-local-judge", "pass87-default-local-judge-request"));
        Assert.True(defaultLocalJudgeEvaluation.IsSuccess, defaultLocalJudgeEvaluation.ErrorMessage);
        Assert.Equal(
            DefaultLocalDesignConversionVisualJudge.StatusOk,
            defaultLocalJudgeEvaluation.Value!.VisualQaBoundaryTrace.VisualJudgeStatus);
        Assert.Equal(
            DesignConversionVisualQaBoundaryPolicy.LocalVisualLabelerMarker,
            defaultLocalJudgeEvaluation.Value.VisualQaBoundaryTrace.VisualJudgeProviderBoundary);
        Assert.False(defaultLocalJudgeEvaluation.Value.VisualQaBoundaryTrace.ExternalAiVisualJudgeExecuted);
        Assert.Contains(DesignConversionVisualQaBoundaryPolicy.LocalVisualLabelerMarker, defaultLocalJudgeEvaluation.Value.EvidenceRefs);
        Assert.Contains(DefaultLocalDesignConversionVisualJudge.ExternalProviderNotUsedMarker, defaultLocalJudgeEvaluation.Value.EvidenceRefs);
        Assert.DoesNotContain(DesignConversionVisualQaBoundaryPolicy.VisualJudgeProviderRequiredMarker, defaultLocalJudgeEvaluation.Value.EvidenceRefs);

        var defaultLocalJudge = BuildTrainingRecord("pass87-default-local-judge", "pass87-source-default-local-judge", 0.98);
        var defaultLocalAppend = await db.AppendRecordAsync(DesignConversionLocalArtifactDbRecord.FromEvaluation(
            defaultLocalJudgeEvaluation.Value,
            [defaultLocalJudge],
            DateTimeOffset.UnixEpoch.AddSeconds(1),
            new Dictionary<string, string>
            {
                ["pass87_local_full_loop_e2e"] = bool.TrueString,
                ["training_gate_boundary"] = bool.TrueString
            }));
        Assert.True(defaultLocalAppend.IsSuccess, defaultLocalAppend.ErrorMessage);

        var stored = await db.ListRecordsAsync();
        Assert.True(stored.IsSuccess, stored.ErrorMessage);
        Assert.Equal(2, stored.Value!.Count);
        Assert.All(stored.Value, record =>
        {
            Assert.Equal(JsonlDesignConversionLocalArtifactDb.LocalArtifactDbKind, record.DatabaseKind);
            Assert.StartsWith("dcm-html_result-", record.ConversionResult.HtmlRef, StringComparison.Ordinal);
            Assert.StartsWith("dcm-css_result-", record.ConversionResult.CssRef, StringComparison.Ordinal);
            Assert.StartsWith("sha256:", record.ConversionResult.HtmlSha256, StringComparison.Ordinal);
            Assert.StartsWith("sha256:", record.ConversionResult.CssSha256, StringComparison.Ordinal);
            Assert.NotEmpty(record.RenderArtifactRefs);
            Assert.Contains(JsonlDesignConversionLocalArtifactDb.StorageTrainingArtifactDbMarker, record.EvidenceRefs);
            Assert.Contains(JsonlDesignConversionLocalArtifactDb.ConversionResultMarker, record.EvidenceRefs);
            Assert.Contains(JsonlDesignConversionLocalArtifactDb.DpoCandidateMarker, record.EvidenceRefs);
            Assert.Contains(JsonlDesignConversionLocalArtifactDb.ProductNotReadyMarker, record.EvidenceRefs);
        });

        var approved = await db.QueryApprovedTrainingCandidatesAsync();
        Assert.True(approved.IsSuccess, approved.ErrorMessage);
        Assert.Equal(3, approved.Value!.Count);
        Assert.Equal(
            ["pass87-default-local-judge", "pass87-safe-a", "pass87-safe-b"],
            approved.Value.Select(record => record.RecordId).Order(StringComparer.Ordinal).ToArray());

        var api = await new DesignConversionDpoUsefulnessApi(db).QueryAsync(
            new DesignConversionDpoUsefulnessApiRequest(IncludeBlocked: true));
        Assert.True(api.IsSuccess, api.ErrorMessage);
        Assert.False(api.Value!.ProductReady);
        Assert.Equal(bool.FalseString, api.Value.BoundaryMetadata["trained_model_ready"]);
        Assert.Equal(4, api.Value.Rows.Count);
        Assert.Equal(3, api.Value.TrainReadyCount);
        Assert.Equal(0, api.Value.ProviderMissingCount);
        Assert.Equal(1, api.Value.BlockedCount);
        Assert.Contains(api.Value.Rows, row =>
            row.ReviewStatus == "train-ready"
            && row.TrainReady
            && row.UsefulnessStatus == "high_usefulness"
            && row.ProductNotReady);
        Assert.DoesNotContain(api.Value.Rows, row => row.ProviderMissing);
        Assert.Contains(api.Value.Rows, row =>
            row.ReviewStatus == "blocked"
            && row.ContainsUserPrivateData
            && row.BlockReasons.Contains("private_data_requires_quarantine"));

        var gate = await new DesignConversionTrainingMeasurementGate().EvaluateLocalArtifactDbAsync(
            db,
            null,
            LocalTrainingOptions());
        Assert.True(gate.IsSuccess, gate.ErrorMessage);
        Assert.Equal(DesignConversionTrainingMeasurementGateStatus.TrainingCandidateOnly, gate.Value!.Status);
        Assert.False(gate.Value.ReadyForTraining);
        Assert.True(gate.Value.TrainingCandidateOnly);
        Assert.True(gate.Value.NotRealModelReady);
        Assert.NotNull(gate.Value.MeasurementGate);
        Assert.True(gate.Value.MeasurementGate!.Passed);
        Assert.Contains("training_candidate_only", gate.Value.EvidenceRefs);
        Assert.Contains("not_real_model_ready", gate.Value.EvidenceRefs);
        Assert.Contains("promotion_blocked", gate.Value.EvidenceRefs);
        Assert.Contains("product_not_ready", gate.Value.EvidenceRefs);
        Assert.Contains(gate.Value.CandidateDecisions, decision =>
            decision.RecordId == "pass87-default-local-judge"
            && decision.Accepted);
        Assert.Contains(gate.Value.CandidateDecisions, decision =>
            decision.RecordId == "pass87-private-unsafe"
            && !decision.Accepted
            && decision.Reasons.Contains("quarantine_blocked"));
        Assert.NotNull(gate.Value.TrainingBatchArtifact);
        var trainedIds = gate.Value.TrainingBatchArtifact!.Batches
            .SelectMany(batch => batch.Records)
            .Select(record => record.RecordId)
            .ToArray();
        Assert.NotEmpty(trainedIds);
        Assert.All(trainedIds, id => Assert.True(
            id.StartsWith("pass87-safe-", StringComparison.Ordinal)
            || id == "pass87-default-local-judge"));
        Assert.DoesNotContain("pass87-provider-missing", trainedIds);
        Assert.DoesNotContain("pass87-private-unsafe", trainedIds);

        var persistedText = await File.ReadAllTextAsync(Path.Combine(root, "conversion-results.jsonl"));
        Assert.Contains("pass87_local_full_loop_e2e", persistedText);
        Assert.Contains("storage_training_artifact_db", persistedText);
        Assert.Contains("conversion_result", persistedText);
        Assert.Contains("visual_judge_status", persistedText);
        Assert.Contains("dpo_candidate", persistedText);
        Assert.Contains("training_candidate_only", persistedText);
        Assert.Contains("product_not_ready", persistedText);
        Assert.DoesNotContain("<main", persistedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(".pass87", persistedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("C:/Users/", persistedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("exact_prompt", persistedText, StringComparison.OrdinalIgnoreCase);
    }

    private static DesignConversionTrainingPipelineOptions LocalTrainingOptions()
        => new(
            "pass87-local-full-loop",
            BatchSize: 2,
            RequireDatasetCountGate: false,
            DatasetOptions: new DesignConversionDpoDatasetOptions(
                DatasetId: "pass87-local-full-loop",
                MinimumTrainRecords: 1,
                MinimumHeldOutRecords: 1,
                Preflight: DesignConversionDpoDatasetPreflight.LocalFixtureTrainingCandidateOnly(
                    "pass87_local_fixture_structural_preflight")),
            MeasurementGateRequest: new DesignConversionMeasurementGateRequest(
                GateId: "pass87-local-measurement",
                UnitId: DesignConversionDpoUnits.CriticUnit,
                HeldOutMetrics: new DesignConversionHeldOutMeasurementMetrics(
                    HeldOutRecordCount: 1,
                    HeldOutQualityScore: 0.93,
                    RuntimeMs: 42,
                    CostUsd: 0m,
                    PeakRamMb: 128,
                    EvidenceRefs: ["pass87-local-heldout-measurement"]),
                IntegrityProof: new DesignConversionMeasurementIntegrityProof(
                    SourceDisjoint: true,
                    LeakageDetected: false,
                    HiddenDeterminismDetected: false,
                    AblationPassed: true,
                    EvidenceRefs: ["pass87-source-disjoint", "pass87-ablation-local-fixture"]),
                Thresholds: new DesignConversionMeasurementGateThresholds(
                    RequiredHeldOutQualityScore: 0.90,
                    MaxRuntimeMs: 250,
                    MaxCostUsd: 0.01m,
                    MaxPeakRamMb: 512)));

    private static DesignConversionDpoTrainingRecord BuildTrainingRecord(
        string id,
        string sourceId,
        double usefulness)
    {
        var example = DesignConversionDpoGeneratorTests.CreateExample(
            id,
            DesignConversionDpoUnits.CriticUnit,
            usefulness) with
            {
                Provenance = new DesignConversionDpoProvenance(
                    SourceRequestId: $"request:{sourceId}",
                    SourceKind: DesignConversionSourceKind.Figma.ToString(),
                    SourceManifestRef: $"manifest://pass87/{sourceId}",
                    ReviewerRef: "teacher:redacted",
                    CreatedAtUtc: DateTimeOffset.UnixEpoch,
                    Metadata: new Dictionary<string, string>
                    {
                        ["source_design_id"] = sourceId,
                        ["pass87_local_full_loop_e2e"] = bool.TrueString
                    })
            };

        var build = new DesignConversionDpoGenerator().BuildTrainReadyRecords([example]);
        Assert.True(build.IsSuccess, build.ErrorMessage);
        Assert.Empty(build.Value!.QuarantinedRecords);
        return Assert.Single(build.Value.AcceptedRecords);
    }

    private static DesignConversionInternalEvaluateRequest CreateRequest(string jobId, string requestId)
        => new(
            JobId: jobId,
            RequestId: requestId,
            SourceKind: DesignConversionSourceKind.Figma,
            Html: "<main class=\"pass87-card\">Hello</main>",
            Css: ".pass87-card{display:grid;gap:12px;}",
            DesignTreePayload: new DesignTreePayload(
                PayloadId: $"{requestId}:tree-current",
                Format: "figma-json",
                Content: "{\"type\":\"FRAME\",\"name\":\"pass87\"}",
                Sha256: "sha256:pass87-tree-current",
                Metadata: new Dictionary<string, string>()),
            OriginalTreePayload: new DesignTreePayload(
                PayloadId: $"{requestId}:tree-original",
                Format: "figma-json",
                Content: "{\"type\":\"FRAME\",\"name\":\"original\"}",
                Sha256: "sha256:pass87-tree-original",
                Metadata: new Dictionary<string, string>()),
            Screenshots:
            [
                new DesignRenderArtifact(
                    ArtifactId: $"{requestId}:source-shot",
                    Kind: "screenshot",
                    Width: 1440,
                    Height: 900,
                    Uri: $"artifact://{requestId}/source-shot.png",
                    Sha256: "sha256:pass87-source-shot",
                    Metadata: new Dictionary<string, string>())
            ],
            DesignRenders:
            [
                new DesignRenderArtifact(
                    ArtifactId: $"{requestId}:design-render",
                    Kind: "design-render",
                    Width: 1440,
                    Height: 900,
                    Uri: $"artifact://{requestId}/design-render.png",
                    Sha256: "sha256:pass87-design-render",
                    Metadata: new Dictionary<string, string>())
            ],
            Budget: new DesignConversionBudgetMetadata(
                UserId: "user-redacted",
                Tier: "local-fixture",
                MaxImprovementCandidates: 1,
                MaxRenderQaRuns: 1,
                BudgetUsd: 0.01m,
                Metadata: new Dictionary<string, string>()),
            RenderResolutions: [new RenderQaResolution("desktop", 1440, 900, 1.0)],
            AiTeacherEvidenceRefs: ["pass87-local-teacher:none"],
            DeterministicEvidenceRefs: new Dictionary<string, string>
            {
                ["pass87_local_full_loop_e2e"] = bool.TrueString
            },
            Provenance: new Dictionary<string, string>
            {
                ["source_manifest_ref"] = "manifest://pass87/local-non-provider"
            });

    private sealed class Pass87RenderProvider : IDesignConversionRenderProvider
    {
        public Task<OperationResult<DesignConversionRenderResult>> RenderAsync(
            DesignConversionRenderRequest request,
            CancellationToken cancellationToken = default)
        {
            var image = new RenderQaImageRecord(
                ImageId: $"{request.RenderRequestId}:actual",
                Label: "actual-render",
                Resolution: request.Resolution,
                Uri: $"artifact://pass87/{request.RenderRequestId}/actual.png",
                Sha256: $"sha256:{request.RenderRequestId}:actual",
                Metadata: new Dictionary<string, string>(request.Metadata, StringComparer.Ordinal)
                {
                    ["pass87_local_full_loop_e2e"] = bool.TrueString
                });

            return Task.FromResult(OperationResult<DesignConversionRenderResult>.Ok(new DesignConversionRenderResult(
                RenderResultId: $"{request.RenderRequestId}:result",
                RenderRequestId: request.RenderRequestId,
                Image: image,
                ElapsedMs: 7,
                Warnings: ["pass87-local-render-provider"])));
        }
    }

    private sealed class Pass87ComparisonProvider : IDesignConversionComparisonProvider
    {
        public Task<OperationResult<DesignConversionQaComparisonArtifact>> CompareAsync(
            DesignConversionQaComparisonRequest request,
            CancellationToken cancellationToken = default)
        {
            var verdict = CreateVerdict($"{request.ComparisonRequestId}:comparison-verdict", 0.91, passed: true);
            return Task.FromResult(OperationResult<DesignConversionQaComparisonArtifact>.Ok(
                new DesignConversionQaComparisonArtifact(
                    ArtifactId: $"{request.ComparisonRequestId}:artifact",
                    ComparisonRequestId: request.ComparisonRequestId,
                    Expected: request.Expected,
                    Actual: request.Actual,
                    Verdict: verdict,
                    DiffImageUri: $"artifact://pass87/{request.ComparisonRequestId}/diff.png",
                    DiffImageSha256: $"sha256:{request.ComparisonRequestId}:diff",
                    Metrics: new Dictionary<string, double>
                    {
                        ["visual_distance"] = 0.09
                    },
                    EvidenceRefs: ["pass87-local-comparison"])));
        }
    }

    private sealed class Pass87TestProviderOnlyVisualJudge : IDesignConversionVisualJudge
    {
        public Task<OperationResult<DesignConversionVisualJudgeResult>> JudgeAsync(
            DesignConversionVisualJudgeRequest request,
            CancellationToken cancellationToken = default)
        {
            var verdict = CreateVerdict($"{request.JudgeRequestId}:test-provider-verdict", 0.92, passed: true);
            return Task.FromResult(OperationResult<DesignConversionVisualJudgeResult>.Ok(
                new DesignConversionVisualJudgeResult(
                    JudgeResultId: $"{request.JudgeRequestId}:test-provider-result",
                    JudgeRequestId: request.JudgeRequestId,
                    Status: DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
                    IsBlocked: false,
                    Verdict: verdict,
                    Provider: new DesignConversionVisualJudgeProviderMetadata(
                        ProviderName: "pass87-test-provider-only",
                        EndpointHost: "none",
                        ModelId: "local-test-double",
                        ExternalCallExecuted: false,
                        Metadata: new Dictionary<string, string>
                        {
                            [DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker] = bool.TrueString,
                            ["external_provider_call"] = bool.FalseString
                        }),
                    EstimatedCostUsd: 0m,
                    TokenMetadata: new Dictionary<string, double>(),
                    EvidenceRefs:
                    [
                        DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
                        "pass87-local-visual-judge-test-double",
                        .. verdict.EvidenceRefs
                    ],
                    Metadata: new Dictionary<string, string>
                    {
                        ["pass87_local_full_loop_e2e"] = bool.TrueString,
                        ["external_ai_visual_judge_executed"] = bool.FalseString
                    })));
        }
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
                        Description: "Spacing differs from the expected render.",
                        X: 12,
                        Y: 24,
                        Width: 80,
                        Height: 20,
                        EvidenceImageIds: ["expected", "actual"])
                ],
            EvidenceRefs: [$"pass87-verdict:{verdictId}"],
            Passed: passed);
}
