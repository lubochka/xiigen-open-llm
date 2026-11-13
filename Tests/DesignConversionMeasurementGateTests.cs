// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using ContinuousLearning;
using DesignConversionDomain;

public sealed class DesignConversionMeasurementGateTests
{
    [Fact]
    public void Evaluate_PassesBoundedCallerAdmissionAdvisoryWithRequiredProofs()
    {
        var result = new DesignConversionMeasurementGateEvaluator().Evaluate(BuildPassingRequest());

        Assert.True(result.Passed);
        Assert.Equal(DesignConversionMeasurementGateStatus.Passed, result.Status);
        Assert.Equal("ALL_MEASUREMENT_GATES_PASSED", Assert.Single(result.Reasons));
        Assert.True(result.SourceDisjoint);
        Assert.True(result.AblationPassed);
        Assert.True(result.BudgetPassed);
        Assert.Contains("artifact://caller-measurement/fixture-held-out-metrics/pass", result.EvidenceRefs);
        Assert.Contains("artifact://caller-measurement/fixture-anti-leakage-ablation/pass", result.EvidenceRefs);
        Assert.Contains("DESIGN_CONVERSION_MEASUREMENT_GATE_PASSED", result.EvidenceRefs);
        Assert.StartsWith("sha256:", result.GateId, StringComparison.Ordinal);
        Assert.StartsWith("sha256:", result.UnitId, StringComparison.Ordinal);
        Assert.DoesNotContain(BuildPassingRequest().GateId, result.GateId, StringComparison.Ordinal);
    }

    [Fact]
    public void Evaluate_FailsWhenHeldOutEvidenceIsMissing()
    {
        var request = BuildPassingRequest() with
        {
            HeldOutMetrics = BuildPassingMetrics() with
            {
                EvidenceRefs = []
            }
        };

        var result = new DesignConversionMeasurementGateEvaluator().Evaluate(request);

        Assert.False(result.Passed);
        Assert.Equal(DesignConversionMeasurementGateStatus.Failed, result.Status);
        Assert.Contains("HELD_OUT_MEASUREMENT_EVIDENCE_MISSING", result.Reasons);
        Assert.Contains("DESIGN_CONVERSION_MEASUREMENT_GATE_FAILED", result.EvidenceRefs);
    }

    [Fact]
    public void Capture_DeepSnapshotsAndHashesCallerAdmissionContract()
    {
        var metricRefs = new[] { "artifact://measurement/heldout.json" };
        var proofRefs = new[] { "sha256:" + new string('a', 64) };
        var request = BuildPassingRequest() with
        {
            HeldOutMetrics = BuildPassingMetrics() with { EvidenceRefs = metricRefs },
            IntegrityProof = BuildPassingProof() with { EvidenceRefs = proofRefs }
        };

        var captured = DesignConversionMeasurementAdmission.Capture(request);
        var hashBeforeMutation = DesignConversionMeasurementAdmission
            .BuildCanonicalSha256(captured.Value);
        metricRefs[0] = "raw:private-measurement";
        proofRefs[0] = "private:changed";
        var hashAfterMutation = DesignConversionMeasurementAdmission
            .BuildCanonicalSha256(captured.Value);

        Assert.True(captured.IsSuccess, captured.ErrorMessage);
        Assert.True(hashBeforeMutation.IsSuccess, hashBeforeMutation.ErrorMessage);
        Assert.Equal(hashBeforeMutation.Value, hashAfterMutation.Value);
        Assert.Equal("artifact://measurement/heldout.json", captured.Value!.HeldOutMetrics!.EvidenceRefs[0]);
        Assert.StartsWith("sha256:", hashBeforeMutation.Value, StringComparison.Ordinal);
    }

    [Fact]
    public void BuildCanonicalSha256_LegacyRequestPreservesExactHistoricalV1Bytes()
    {
        var request = BuildPassingRequest();
        var actual = DesignConversionMeasurementAdmission.BuildCanonicalSha256(request);

        // Этот эталон собирается тестом без production-помощников и фиксирует
        // старые байты: у v1 нет даже маркеров отсутствующего паспорта шума.
        var independentCanonical = BuildIndependentMeasurementCanonical(
            request,
            "design-conversion-caller-admission-measurement-v1",
            includeMeasurementNoiseBinding: false);
        var independentHash = BuildIndependentMeasurementSha256(independentCanonical);

        Assert.True(actual.IsSuccess, actual.ErrorMessage);
        Assert.DoesNotContain("measurement_noise_", independentCanonical, StringComparison.Ordinal);
        Assert.Equal(
            "sha256:0bbe395a279ebcd6f07073c75135d69e87c049388804931f474ac62d21aa52f4",
            independentHash);
        Assert.Equal(independentHash, actual.Value);
    }

    [Fact]
    public void BuildCanonicalSha256_PassportUsesExactIndependentV2AndRejectsInterimV1Shape()
    {
        var legacyRequest = BuildPassingRequest();
        var request = legacyRequest with
        {
            MeasurementNoiseScorerIdentity = MeasurementNoiseIdentity,
            MeasurementNoisePassport = BuildPassportForGate(legacyRequest)
        };
        var actual = DesignConversionMeasurementAdmission.BuildCanonicalSha256(request);

        // Вторая версия включает ожидаемую личность и полный паспорт. Тот же
        // хвост с прежним именем схемы был промежуточной ошибкой, не alias.
        var independentV2 = BuildIndependentMeasurementCanonical(
            request,
            "design-conversion-caller-admission-measurement-v2",
            includeMeasurementNoiseBinding: true);
        var interimV1WithNoiseFields = BuildIndependentMeasurementCanonical(
            request,
            "design-conversion-caller-admission-measurement-v1",
            includeMeasurementNoiseBinding: true);

        Assert.True(actual.IsSuccess, actual.ErrorMessage);
        Assert.Equal(BuildIndependentMeasurementSha256(independentV2), actual.Value);
        Assert.NotEqual(
            BuildIndependentMeasurementSha256(interimV1WithNoiseFields),
            actual.Value);
    }

    [Fact]
    public void BuildCanonicalSha256_IdentityOnlyUsesV2AndIdentityInfluencesHash()
    {
        var request = BuildPassingRequest() with
        {
            MeasurementNoiseScorerIdentity = MeasurementNoiseIdentity
        };
        var changedIdentityRequest = request with
        {
            MeasurementNoiseScorerIdentity = MeasurementNoiseIdentity with
            {
                ScorerVersion = "v2"
            }
        };

        var actual = DesignConversionMeasurementAdmission.BuildCanonicalSha256(request);
        var changedIdentity = DesignConversionMeasurementAdmission
            .BuildCanonicalSha256(changedIdentityRequest);
        var legacy = DesignConversionMeasurementAdmission
            .BuildCanonicalSha256(BuildPassingRequest());
        var independentV2 = BuildIndependentMeasurementCanonical(
            request,
            "design-conversion-caller-admission-measurement-v2",
            includeMeasurementNoiseBinding: true);

        Assert.True(actual.IsSuccess, actual.ErrorMessage);
        Assert.True(changedIdentity.IsSuccess, changedIdentity.ErrorMessage);
        Assert.True(legacy.IsSuccess, legacy.ErrorMessage);
        Assert.Equal(BuildIndependentMeasurementSha256(independentV2), actual.Value);
        Assert.NotEqual(legacy.Value, actual.Value);
        Assert.NotEqual(changedIdentity.Value, actual.Value);
    }

    [Theory]
    [MemberData(nameof(InvalidIdentityOnlyCases))]
    public void Capture_IdentityOnlyRejectsEveryInvalidScorerIdentityField(
        string field,
        string invalidKind,
        string? invalidValue)
    {
        var invalidIdentity = field switch
        {
            nameof(HeldOutScorerIdentity.DomainTag) => MeasurementNoiseIdentity with
            {
                DomainTag = invalidValue!
            },
            nameof(HeldOutScorerIdentity.ScorerId) => MeasurementNoiseIdentity with
            {
                ScorerId = invalidValue!
            },
            nameof(HeldOutScorerIdentity.ScorerVersion) => MeasurementNoiseIdentity with
            {
                ScorerVersion = invalidValue!
            },
            nameof(HeldOutScorerIdentity.NormalizationProfileId) => MeasurementNoiseIdentity with
            {
                NormalizationProfileId = invalidValue!
            },
            nameof(HeldOutScorerIdentity.MetricProfileId) => MeasurementNoiseIdentity with
            {
                MetricProfileId = invalidValue!
            },
            _ => throw new ArgumentOutOfRangeException(nameof(field), field, null)
        };
        var request = BuildPassingRequest() with
        {
            MeasurementNoiseScorerIdentity = invalidIdentity
        };

        var captured = DesignConversionMeasurementAdmission.Capture(request);
        var hash = DesignConversionMeasurementAdmission.BuildCanonicalSha256(request);

        Assert.False(
            captured.IsSuccess,
            $"Field {field} accepted invalid case {invalidKind}.");
        Assert.Equal(DesignConversionMeasurementAdmission.InvalidErrorCode, captured.ErrorCode);
        Assert.False(hash.IsSuccess);
        Assert.Equal(DesignConversionMeasurementAdmission.InvalidErrorCode, hash.ErrorCode);
        Assert.Equal(
            "Caller admission measurement must be bounded, refs-only, and contain no raw or private evidence.",
            captured.ErrorMessage);
    }

    [Fact]
    public void Capture_PassportWithoutExpectedIdentityFailsClosed()
    {
        var legacyRequest = BuildPassingRequest();
        var request = legacyRequest with
        {
            MeasurementNoisePassport = BuildPassportForGate(legacyRequest)
        };

        var captured = DesignConversionMeasurementAdmission.Capture(request);
        var hash = DesignConversionMeasurementAdmission.BuildCanonicalSha256(request);

        Assert.False(captured.IsSuccess);
        Assert.Equal(
            DesignConversionMeasurementAdmission.MeasurementNoisePassportInvalidErrorCode,
            captured.ErrorCode);
        Assert.False(hash.IsSuccess);
        Assert.Equal(
            DesignConversionMeasurementAdmission.MeasurementNoisePassportInvalidErrorCode,
            hash.ErrorCode);
    }

    [Fact]
    public void Evaluate_RejectsRawPrivateOrOversizedEvidenceWithoutThrowing()
    {
        var invalid = new DesignConversionMeasurementGateRequest?[]
        {
            BuildPassingRequest() with
            {
                HeldOutMetrics = BuildPassingMetrics() with
                {
                    EvidenceRefs = ["raw:<html>private payload</html>"]
                }
            },
            BuildPassingRequest() with
            {
                IntegrityProof = BuildPassingProof() with
                {
                    EvidenceRefs = [new string('x', DesignConversionMeasurementAdmission.MaxEvidenceRefChars + 1)]
                }
            },
            BuildPassingRequest() with
            {
                IntegrityProof = BuildPassingProof() with { EvidenceRefs = null! }
            }
        };

        foreach (var request in invalid)
        {
            var result = new DesignConversionMeasurementGateEvaluator().Evaluate(request);
            Assert.False(result.Passed);
            Assert.Equal(DesignConversionMeasurementGateStatus.Failed, result.Status);
            Assert.Contains(
                DesignConversionMeasurementAdmission.InvalidErrorCode,
                result.Reasons);
            Assert.DoesNotContain(result.EvidenceRefs, value => value.Contains("private", StringComparison.OrdinalIgnoreCase));
        }
    }

    [Fact]
    public void Capture_RejectsBarePathsMalformedHashesAndUnrecognizedSchemes()
    {
        var invalidRefs = new[]
        {
            "bare-secret-token",
            "C:/private/measurement.json",
            "sha256:1234",
            "https://caller.example/private-measurement",
            "secret:opaque-value"
        };

        foreach (var invalidRef in invalidRefs)
        {
            var captured = DesignConversionMeasurementAdmission.Capture(
                BuildPassingRequest() with
                {
                    HeldOutMetrics = BuildPassingMetrics() with
                    {
                        EvidenceRefs = [invalidRef]
                    }
                });

            Assert.False(captured.IsSuccess);
            Assert.Equal(
                DesignConversionMeasurementAdmission.InvalidErrorCode,
                captured.ErrorCode);
            Assert.DoesNotContain(invalidRef, captured.ErrorMessage ?? string.Empty, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void Capture_RejectsRawOrPrivateIdentifiersWithoutEchoingThem()
    {
        var invalidIdentifiers = new[]
        {
            "raw:<private-gate-payload>",
            "private:caller-secret",
            "payload:{\"secret\":true}",
            "data:text/plain,secret"
        };

        foreach (var invalidIdentifier in invalidIdentifiers)
        {
            var invalidGate = DesignConversionMeasurementAdmission.Capture(
                BuildPassingRequest() with { GateId = invalidIdentifier });
            var invalidUnit = DesignConversionMeasurementAdmission.Capture(
                BuildPassingRequest() with { UnitId = invalidIdentifier });

            Assert.False(invalidGate.IsSuccess);
            Assert.False(invalidUnit.IsSuccess);
            Assert.Equal(DesignConversionMeasurementAdmission.InvalidErrorCode, invalidGate.ErrorCode);
            Assert.Equal(DesignConversionMeasurementAdmission.InvalidErrorCode, invalidUnit.ErrorCode);
            Assert.DoesNotContain(
                invalidIdentifier,
                invalidGate.ErrorMessage ?? string.Empty,
                StringComparison.Ordinal);
            Assert.DoesNotContain(
                invalidIdentifier,
                invalidUnit.ErrorMessage ?? string.Empty,
                StringComparison.Ordinal);
        }
    }

    [Fact]
    public void Evaluate_FailsWhenThresholdsAreMissing()
    {
        var request = BuildPassingRequest() with
        {
            Thresholds = null!
        };

        var result = new DesignConversionMeasurementGateEvaluator().Evaluate(request);

        Assert.False(result.Passed);
        Assert.Equal(DesignConversionMeasurementGateStatus.Failed, result.Status);
        Assert.Contains(
            DesignConversionMeasurementAdmission.InvalidErrorCode,
            result.Reasons);
        Assert.False(result.BudgetPassed);
        Assert.Contains("DESIGN_CONVERSION_MEASUREMENT_GATE_FAILED", result.EvidenceRefs);
    }

    [Fact]
    public void Evaluate_FailsWhenQualityScoreIsNotFinite()
    {
        var invalidCases = new[]
        {
            double.NaN,
            double.PositiveInfinity,
            double.NegativeInfinity
        };

        foreach (var invalidCase in invalidCases)
        {
            var request = BuildPassingRequest() with
            {
                HeldOutMetrics = BuildPassingMetrics() with
                {
                    HeldOutQualityScore = invalidCase
                }
            };

            var result = new DesignConversionMeasurementGateEvaluator().Evaluate(request);

            Assert.False(result.Passed);
            Assert.Equal(DesignConversionMeasurementGateStatus.Failed, result.Status);
            Assert.Contains(
                DesignConversionMeasurementAdmission.InvalidErrorCode,
                result.Reasons);
        }
    }

    [Fact]
    public void Evaluate_FailsWhenBudgetMetricsOrThresholdsAreInvalid()
    {
        var request = BuildPassingRequest() with
        {
            HeldOutMetrics = BuildPassingMetrics() with
            {
                RuntimeMs = -1,
                CostUsd = -0.01m,
                PeakRamMb = -1
            },
            Thresholds = new DesignConversionMeasurementGateThresholds(
                RequiredHeldOutQualityScore: double.PositiveInfinity,
                MaxRuntimeMs: 0,
                MaxCostUsd: -0.01m,
                MaxPeakRamMb: 0)
        };

        var result = new DesignConversionMeasurementGateEvaluator().Evaluate(request);

        Assert.False(result.Passed);
        Assert.False(result.BudgetPassed);
        Assert.Contains(
            DesignConversionMeasurementAdmission.InvalidErrorCode,
            result.Reasons);
    }

    [Fact]
    public void Evaluate_FailsWhenLeakageOrAblationShortcutIsFlagged()
    {
        var request = BuildPassingRequest() with
        {
            IntegrityProof = BuildPassingProof() with
            {
                SourceDisjoint = false,
                LeakageDetected = true,
                HiddenDeterminismDetected = true,
                AblationPassed = false
            }
        };

        var result = new DesignConversionMeasurementGateEvaluator().Evaluate(request);

        Assert.False(result.Passed);
        Assert.Contains("SOURCE_DISJOINT_PROOF_FAILED", result.Reasons);
        Assert.Contains("LEAKAGE_DETECTED", result.Reasons);
        Assert.Contains("HIDDEN_DETERMINISM_DETECTED", result.Reasons);
        Assert.Contains("ABLATION_PROOF_FAILED", result.Reasons);
    }

    [Fact]
    public void Evaluate_FailsWhenQualityIsLowOrBudgetIsOverrun()
    {
        var request = BuildPassingRequest() with
        {
            HeldOutMetrics = BuildPassingMetrics() with
            {
                HeldOutQualityScore = 0.81,
                RuntimeMs = 901,
                CostUsd = 2.50m,
                PeakRamMb = 1024
            }
        };

        var result = new DesignConversionMeasurementGateEvaluator().Evaluate(request);

        Assert.False(result.Passed);
        Assert.False(result.BudgetPassed);
        Assert.Contains("HELD_OUT_QUALITY_SCORE_BELOW_THRESHOLD:0.81<0.9", result.Reasons);
        Assert.Contains("RUNTIME_MS_BUDGET_EXCEEDED:901>800", result.Reasons);
        Assert.Contains("COST_USD_BUDGET_EXCEEDED:2.50>1.00", result.Reasons);
        Assert.Contains("PEAK_RAM_MB_BUDGET_EXCEEDED:1024>512", result.Reasons);
    }

    [Fact]
    public void Evaluate_RejectsHeldOutScoreInsideMeasurementNoiseBand()
    {
        var request = BuildPassingRequest() with
        {
            MeasurementNoiseScorerIdentity = MeasurementNoiseIdentity,
            MeasurementNoisePassport = BuildPassportForGate(BuildPassingRequest())
        };

        var result = new DesignConversionMeasurementGateEvaluator().Evaluate(request);

        Assert.False(result.Passed);
        Assert.Equal(MeasurementNoisePolicy.IndistinguishableStatus, result.MeasurementNoiseStatus);
        Assert.NotNull(result.MeasurementNoiseFloor);
        Assert.Contains(result.Reasons, static reason => reason.StartsWith(
            "HELD_OUT_QUALITY_SCORE_WITHIN_MEASUREMENT_NOISE:0.94-0.9<=",
            StringComparison.Ordinal));
    }

    [Fact]
    public void Evaluate_RejectsBelowThresholdScoreInsideMeasurementNoiseBandAsIndistinguishable()
    {
        var request = BuildPassingRequest() with
        {
            HeldOutMetrics = BuildPassingMetrics() with
            {
                HeldOutQualityScore = 0.88d
            }
        };
        request = request with
        {
            MeasurementNoiseScorerIdentity = MeasurementNoiseIdentity,
            MeasurementNoisePassport = BuildPassportForGate(request)
        };

        var result = new DesignConversionMeasurementGateEvaluator().Evaluate(request);

        Assert.False(result.Passed);
        Assert.Equal(MeasurementNoisePolicy.IndistinguishableStatus, result.MeasurementNoiseStatus);
        Assert.Contains(result.Reasons, static reason => reason.StartsWith(
            "HELD_OUT_QUALITY_SCORE_WITHIN_MEASUREMENT_NOISE:0.88-0.9<=",
            StringComparison.Ordinal));
        Assert.DoesNotContain("HELD_OUT_QUALITY_SCORE_BELOW_THRESHOLD:0.88<0.9",
            result.Reasons);
    }

    [Fact]
    public void Evaluate_StrictlyAboveMeasurementNoiseBandCanPass()
    {
        var request = BuildPassingRequest() with
        {
            HeldOutMetrics = BuildPassingMetrics() with
            {
                HeldOutQualityScore = 0.99d
            }
        };
        request = request with
        {
            MeasurementNoiseScorerIdentity = MeasurementNoiseIdentity,
            MeasurementNoisePassport = BuildPassportForGate(request)
        };

        var result = new DesignConversionMeasurementGateEvaluator().Evaluate(request);

        Assert.True(result.Passed);
        Assert.Equal(MeasurementNoisePolicy.ImprovedStatus, result.MeasurementNoiseStatus);
        Assert.True(
            result.HeldOutQualityScore -
            result.RequiredHeldOutQualityScore >
            result.MeasurementNoiseFloor);
    }

    [Fact]
    public void Capture_ForeignMeasurementNoisePassportFailsClosed()
    {
        var metrics = BuildPassingMetrics();
        var request = BuildPassingRequest() with
        {
            MeasurementNoiseScorerIdentity = MeasurementNoiseIdentity,
            MeasurementNoisePassport = MeasurementNoisePolicy.BuildPassport(
                MeasurementNoiseIdentity,
                "foreign-gate",
                metrics.HeldOutRecordCount!.Value,
                Enumerable.Repeat(metrics.HeldOutQualityScore!.Value,
                    MeasurementNoisePolicyConfig.DefaultRepeatCount).ToArray(),
                MeasurementNoisePolicyConfig.Canonical,
                DateTimeOffset.UnixEpoch)
        };

        var captured = DesignConversionMeasurementAdmission.Capture(request);
        var result = new DesignConversionMeasurementGateEvaluator().Evaluate(request);

        Assert.False(captured.IsSuccess);
        Assert.Equal(DesignConversionMeasurementAdmission.MeasurementNoisePassportInvalidErrorCode,
            captured.ErrorCode);
        Assert.False(result.Passed);
        Assert.Equal(MeasurementNoisePolicy.InvalidPassportStatus, result.MeasurementNoiseStatus);
        Assert.Contains(
            DesignConversionMeasurementAdmission.MeasurementNoisePassportInvalidErrorCode,
            result.Reasons);
    }

    [Theory]
    [InlineData("scorer-id")]
    [InlineData("scorer-version")]
    [InlineData("normalization-profile")]
    [InlineData("metric-profile")]
    public void Capture_ForeignMeasurementNoisePassportScorerIdentityFailsClosed(
        string mutation)
    {
        var passportIdentity = mutation switch
        {
            "scorer-id" => MeasurementNoiseIdentity with { ScorerId = "foreign-gate-scorer" },
            "scorer-version" => MeasurementNoiseIdentity with { ScorerVersion = "v2" },
            "normalization-profile" => MeasurementNoiseIdentity with { NormalizationProfileId = "gate-normalization-v2" },
            _ => MeasurementNoiseIdentity with { MetricProfileId = "held-out-quality-score-v2" }
        };
        var request = BuildPassingRequest() with
        {
            MeasurementNoiseScorerIdentity = MeasurementNoiseIdentity,
            MeasurementNoisePassport = BuildPassportForGateWithScorer(BuildPassingRequest(), passportIdentity)
        };

        var captured = DesignConversionMeasurementAdmission.Capture(request);
        var result = new DesignConversionMeasurementGateEvaluator().Evaluate(request);

        Assert.False(captured.IsSuccess);
        Assert.Equal(
            DesignConversionMeasurementAdmission.MeasurementNoisePassportInvalidErrorCode,
            captured.ErrorCode);
        Assert.False(result.Passed);
        Assert.Equal(MeasurementNoisePolicy.InvalidPassportStatus, result.MeasurementNoiseStatus);
    }

    [Fact]
    public async Task RunAsync_CallerAdmissionAllowsMutationButCannotAuthorizePostTrainingPromotion()
    {
        var tempRoot = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-measurement-gate-tests",
            Guid.NewGuid().ToString("N"));

        try
        {
            var machine = new DesignConversionMachine();
            var records = BuildSyntheticSourceDisjointRecords(100);
            var pipeline = new DesignConversionTrainingPipeline();
            var pipelineResult = await DesignConversionLegalTeacherCapabilityTestFactory
                .RunStrictPipelineAsync(
                pipeline,
                machine,
                records,
                new DesignConversionTrainingPipelineOptions(
                    "s3-local-measurement-gate-pass",
                    BatchSize: 25,
                    MeasurementGateRequest: BuildPassingRequest(),
                    DatasetOptions: LocalDatasetOptions()));

            Assert.True(pipelineResult.IsSuccess, pipelineResult.ErrorMessage);
            var lifecycle = pipelineResult.Value!.LifecycleEvidence;
            Assert.Equal(DesignConversionLocalContinueTrainingProbe.VerifiedStatus, lifecycle.ContinueTrainingStatus);
            Assert.Equal(DesignConversionMeasurementGateStatus.Failed, lifecycle.MeasurementGateStatus);
            Assert.NotNull(lifecycle.MeasurementGate);
            Assert.False(lifecycle.MeasurementGate!.Passed);
            Assert.True(lifecycle.CheckpointContainsRealTrainedWeights);
            Assert.False(lifecycle.PromotionUploadAllowed);
            Assert.Contains("held_out_measurement_gate", lifecycle.NotProvenFields);
            Assert.DoesNotContain("real_trained_weights", lifecycle.NotProvenFields);
            Assert.Contains("promotion_upload", lifecycle.NotProvenFields);
            Assert.Null(lifecycle.MeasurementGate.HeldOutQualityScore);
            Assert.Equal(
                DesignConversionMachineEvaluationOptions
                    .DefaultHeldOutEvaluationAccuracyGateThreshold,
                lifecycle.MeasurementGate.RequiredHeldOutQualityScore);
            Assert.Contains(
                "DESIGN_CONVERSION_CHILD_DPO_MEASUREMENT_NOT_AVAILABLE_IN_TRAINING_PIPELINE",
                lifecycle.MeasurementGate.Reasons);

            var checkpointPath = Path.Combine(tempRoot, "trained-machine-checkpoint.json");
            var savedCheckpoint = await machine.SaveCheckpointAsync(checkpointPath);

            Assert.True(savedCheckpoint.IsSuccess, savedCheckpoint.ErrorMessage);
            Assert.Equal(Path.GetFullPath(checkpointPath), savedCheckpoint.Value!.CheckpointRef);
            Assert.True(File.Exists(savedCheckpoint.Value.CheckpointRef));

            var uploader = new FakePromotionUploader();
            var publication = await new DesignConversionPromotionPublisher(uploader).PublishAsync(
                machine,
                pipelineResult.Value,
                "s3-local-measurement-promotion-allowed",
                BuildArtifacts(),
                BuildPromotionGate());

            Assert.True(publication.IsSuccess, publication.ErrorMessage);
            Assert.Equal(DesignConversionPromotionDecisionKind.Reject, publication.Value!.Decision.Kind);
            Assert.Equal(
                "DESIGN_CONVERSION_MEASUREMENT_GATE_NOT_PASSED:failed",
                Assert.Single(publication.Value.Decision.Reasons));
            Assert.Contains(
                "DESIGN_CONVERSION_PROMOTION_UPLOAD_BLOCKED_WITHOUT_MEASUREMENT_GATE",
                publication.Value.Decision.EvidenceRefs);
            Assert.Contains("MODEL_SERVER_UPLOAD_NOT_ATTEMPTED", publication.Value.Decision.EvidenceRefs);
            Assert.Equal(DesignConversionPromotionPublicationKind.Rejected, publication.Value.PublicationKind);
            Assert.Equal(DesignConversionPromotionPublicationStatus.Rejected, publication.Value.PublicationStatus);
            Assert.Equal(0, uploader.UploadCallCount);
            Assert.Null(publication.Value.UploadReceipt);
            Assert.False(publication.Value.Manifest.ModelServerUploadAttempted);
            Assert.False(publication.Value.Manifest.ModelServerPromotionClaimed);
            Assert.False(publication.Value.Manifest.LocalPromotionReceiptWritten);
            Assert.Equal("none", publication.Value.Manifest.PromotionReceiptKind);
            Assert.Null(publication.Value.Manifest.PromotionUploadRef);
            Assert.Null(publication.Value.Manifest.PromotionUploadedAtUtc);
            Assert.Equal(
                $"not-exported://{DesignConversionMachine.MachineId}/checkpoint-path/{DesignConversionDpoLeakageGuard.HashRef("design-conversion-promotion-checkpoint-path", BuildArtifacts().CheckpointPath)}",
                publication.Value.Manifest.CheckpointRef);
            Assert.NotEqual(savedCheckpoint.Value.CheckpointRef, publication.Value.Manifest.CheckpointRef);
            Assert.True(File.Exists(savedCheckpoint.Value.CheckpointRef));
        }
        finally
        {
            if (Directory.Exists(tempRoot))
                Directory.Delete(tempRoot, recursive: true);
        }
    }

    private static string BuildIndependentMeasurementCanonical(
        DesignConversionMeasurementGateRequest request,
        string schema,
        bool includeMeasurementNoiseBinding)
    {
        var canonical = new StringBuilder();
        AppendIndependentMeasurementField(canonical, "schema", schema);
        AppendIndependentMeasurementField(canonical, "gate_id", request.GateId);
        AppendIndependentMeasurementField(canonical, "unit_id", request.UnitId);
        AppendIndependentMeasurementField(
            canonical,
            "held_out_record_count",
            request.HeldOutMetrics?.HeldOutRecordCount?.ToString(CultureInfo.InvariantCulture));
        AppendIndependentMeasurementField(
            canonical,
            "held_out_quality_score",
            request.HeldOutMetrics?.HeldOutQualityScore?.ToString("R", CultureInfo.InvariantCulture));

        if (includeMeasurementNoiseBinding)
        {
            AppendIndependentMeasurementScorerIdentity(
                canonical,
                request.MeasurementNoiseScorerIdentity);
            AppendIndependentMeasurementPassport(
                canonical,
                request.MeasurementNoisePassport);
        }

        AppendIndependentMeasurementField(
            canonical,
            "runtime_ms",
            request.HeldOutMetrics?.RuntimeMs?.ToString(CultureInfo.InvariantCulture));
        AppendIndependentMeasurementField(
            canonical,
            "cost_usd",
            request.HeldOutMetrics?.CostUsd?.ToString("G29", CultureInfo.InvariantCulture));
        AppendIndependentMeasurementField(
            canonical,
            "peak_ram_mb",
            request.HeldOutMetrics?.PeakRamMb?.ToString(CultureInfo.InvariantCulture));
        AppendIndependentMeasurementRefs(
            canonical,
            "held_out_evidence",
            request.HeldOutMetrics?.EvidenceRefs);
        AppendIndependentMeasurementField(
            canonical,
            "source_disjoint",
            FormatIndependentMeasurementBoolean(request.IntegrityProof?.SourceDisjoint));
        AppendIndependentMeasurementField(
            canonical,
            "leakage_detected",
            FormatIndependentMeasurementBoolean(request.IntegrityProof?.LeakageDetected));
        AppendIndependentMeasurementField(
            canonical,
            "hidden_determinism_detected",
            FormatIndependentMeasurementBoolean(request.IntegrityProof?.HiddenDeterminismDetected));
        AppendIndependentMeasurementField(
            canonical,
            "ablation_passed",
            FormatIndependentMeasurementBoolean(request.IntegrityProof?.AblationPassed));
        AppendIndependentMeasurementRefs(
            canonical,
            "integrity_evidence",
            request.IntegrityProof?.EvidenceRefs);
        AppendIndependentMeasurementField(
            canonical,
            "required_quality",
            request.Thresholds.RequiredHeldOutQualityScore.ToString("R", CultureInfo.InvariantCulture));
        AppendIndependentMeasurementField(
            canonical,
            "max_runtime_ms",
            request.Thresholds.MaxRuntimeMs.ToString(CultureInfo.InvariantCulture));
        AppendIndependentMeasurementField(
            canonical,
            "max_cost_usd",
            request.Thresholds.MaxCostUsd.ToString("G29", CultureInfo.InvariantCulture));
        AppendIndependentMeasurementField(
            canonical,
            "max_peak_ram_mb",
            request.Thresholds.MaxPeakRamMb.ToString(CultureInfo.InvariantCulture));
        return canonical.ToString();
    }

    private static void AppendIndependentMeasurementScorerIdentity(
        StringBuilder canonical,
        HeldOutScorerIdentity? scorerIdentity)
    {
        AppendIndependentMeasurementField(
            canonical,
            "measurement_noise_expected_scorer_present",
            scorerIdentity is null ? "false" : "true");
        if (scorerIdentity is null)
            return;

        AppendIndependentMeasurementField(canonical, "measurement_noise_expected_scorer_domain", scorerIdentity.DomainTag);
        AppendIndependentMeasurementField(canonical, "measurement_noise_expected_scorer_id", scorerIdentity.ScorerId);
        AppendIndependentMeasurementField(canonical, "measurement_noise_expected_scorer_version", scorerIdentity.ScorerVersion);
        AppendIndependentMeasurementField(canonical, "measurement_noise_expected_normalization_profile", scorerIdentity.NormalizationProfileId);
        AppendIndependentMeasurementField(canonical, "measurement_noise_expected_metric_profile", scorerIdentity.MetricProfileId);
    }

    private static void AppendIndependentMeasurementPassport(
        StringBuilder canonical,
        MeasurementNoisePassport? passport)
    {
        AppendIndependentMeasurementField(
            canonical,
            "measurement_noise_passport_present",
            passport is null ? "false" : "true");
        if (passport is null)
            return;

        AppendIndependentMeasurementField(canonical, "measurement_noise_scorer_domain", passport.ScorerIdentity.DomainTag);
        AppendIndependentMeasurementField(canonical, "measurement_noise_scorer_id", passport.ScorerIdentity.ScorerId);
        AppendIndependentMeasurementField(canonical, "measurement_noise_scorer_version", passport.ScorerIdentity.ScorerVersion);
        AppendIndependentMeasurementField(canonical, "measurement_noise_normalization_profile", passport.ScorerIdentity.NormalizationProfileId);
        AppendIndependentMeasurementField(canonical, "measurement_noise_metric_profile", passport.ScorerIdentity.MetricProfileId);
        AppendIndependentMeasurementField(canonical, "measurement_noise_evaluation_set", passport.EvaluationSetId);
        AppendIndependentMeasurementField(canonical, "measurement_noise_scored_rows", passport.ScoredRows.ToString(CultureInfo.InvariantCulture));
        AppendIndependentMeasurementField(canonical, "measurement_noise_repeat_count", passport.RepeatCount.ToString(CultureInfo.InvariantCulture));
        AppendIndependentMeasurementField(canonical, "measurement_noise_multiplier", passport.StandardDeviationMultiplier.ToString("R", CultureInfo.InvariantCulture));
        AppendIndependentMeasurementField(canonical, "measurement_noise_mean_score", passport.MeanScore.ToString("R", CultureInfo.InvariantCulture));
        AppendIndependentMeasurementField(canonical, "measurement_noise_sample_stddev", passport.SampleStandardDeviation.ToString("R", CultureInfo.InvariantCulture));
        AppendIndependentMeasurementField(canonical, "measurement_noise_binomial_lower_bound", passport.BinomialLowerBound.ToString("R", CultureInfo.InvariantCulture));
        AppendIndependentMeasurementField(canonical, "measurement_noise_floor", passport.NoiseFloor.ToString("R", CultureInfo.InvariantCulture));
        AppendIndependentMeasurementField(canonical, "measurement_noise_calibrated_at_utc", passport.CalibratedAtUtc.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture));
        AppendIndependentMeasurementField(canonical, "measurement_noise_repeated_score_count", passport.RepeatedScores.Count.ToString(CultureInfo.InvariantCulture));
        for (var index = 0; index < passport.RepeatedScores.Count; index++)
        {
            AppendIndependentMeasurementField(
                canonical,
                "measurement_noise_repeated_score." + index.ToString(CultureInfo.InvariantCulture),
                passport.RepeatedScores[index].ToString("R", CultureInfo.InvariantCulture));
        }
    }

    private static void AppendIndependentMeasurementRefs(
        StringBuilder canonical,
        string name,
        IReadOnlyList<string>? values)
    {
        var ordered = values?.Order(StringComparer.Ordinal).ToArray();
        AppendIndependentMeasurementField(
            canonical,
            name + ".count",
            ordered?.Length.ToString(CultureInfo.InvariantCulture));
        if (ordered is null)
            return;

        for (var index = 0; index < ordered.Length; index++)
        {
            AppendIndependentMeasurementField(
                canonical,
                name + "." + index.ToString(CultureInfo.InvariantCulture),
                ordered[index]);
        }
    }

    private static void AppendIndependentMeasurementField(
        StringBuilder canonical,
        string name,
        string? value)
    {
        canonical.Append(name.Length.ToString(CultureInfo.InvariantCulture))
            .Append(':').Append(name).Append('=');
        if (value is null)
            canonical.Append("N;");
        else
            canonical.Append('V').Append(value.Length.ToString(CultureInfo.InvariantCulture))
                .Append(':').Append(value).Append(';');
    }

    private static string? FormatIndependentMeasurementBoolean(bool? value)
        => value switch { true => "true", false => "false", null => null };

    private static string BuildIndependentMeasurementSha256(string canonical)
        => "sha256:" + Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes(canonical)))
            .ToLowerInvariant();

    public static IEnumerable<object[]> InvalidIdentityOnlyCases()
    {
        var fields = new[]
        {
            nameof(HeldOutScorerIdentity.DomainTag),
            nameof(HeldOutScorerIdentity.ScorerId),
            nameof(HeldOutScorerIdentity.ScorerVersion),
            nameof(HeldOutScorerIdentity.NormalizationProfileId),
            nameof(HeldOutScorerIdentity.MetricProfileId)
        };
        var invalidValues = new (string Kind, string? Value)[]
        {
            ("null", null),
            ("empty", string.Empty),
            ("whitespace", " "),
            ("control", "invalid\u0001identity"),
            ("oversized", new string(
                'x',
                DesignConversionMeasurementAdmission.MaxIdentifierChars + 1))
        };

        // Пять видов повреждения применяются к каждому из пяти полей, поэтому
        // manifest обязан увидеть ровно двадцать пять физических строк.
        foreach (var field in fields)
        foreach (var invalidValue in invalidValues)
            yield return [field, invalidValue.Kind, invalidValue.Value!];
    }

    private static DesignConversionMeasurementGateRequest BuildPassingRequest()
        => new(
            GateId: "s3-local-measurement-gate-fixture",
            UnitId: DesignConversionDpoUnits.CriticUnit,
            HeldOutMetrics: BuildPassingMetrics(),
            IntegrityProof: BuildPassingProof(),
            Thresholds: new DesignConversionMeasurementGateThresholds(
                RequiredHeldOutQualityScore: 0.90,
                MaxRuntimeMs: 800,
                MaxCostUsd: 1.00m,
                MaxPeakRamMb: 512));

    private static DesignConversionHeldOutMeasurementMetrics BuildPassingMetrics()
        => new(
            HeldOutRecordCount: 50,
            HeldOutQualityScore: 0.94,
            RuntimeMs: 320,
            CostUsd: 0.25m,
            PeakRamMb: 256,
            EvidenceRefs: ["artifact://caller-measurement/fixture-held-out-metrics/pass"]);

    private static DesignConversionMeasurementIntegrityProof BuildPassingProof()
        => new(
            SourceDisjoint: true,
            LeakageDetected: false,
            HiddenDeterminismDetected: false,
            AblationPassed: true,
            EvidenceRefs: ["artifact://caller-measurement/fixture-anti-leakage-ablation/pass"]);

    private static MeasurementNoisePassport BuildPassportForGate(
        DesignConversionMeasurementGateRequest request)
        => BuildPassportForGateWithScorer(request, MeasurementNoiseIdentity);

    private static MeasurementNoisePassport BuildPassportForGateWithScorer(
        DesignConversionMeasurementGateRequest request,
        HeldOutScorerIdentity scorerIdentity)
        => MeasurementNoisePolicy.BuildPassport(
            scorerIdentity,
            request.GateId,
            request.HeldOutMetrics!.HeldOutRecordCount!.Value,
            Enumerable.Repeat(
                request.HeldOutMetrics.HeldOutQualityScore!.Value,
                MeasurementNoisePolicyConfig.DefaultRepeatCount).ToArray(),
            MeasurementNoisePolicyConfig.Canonical,
            DateTimeOffset.UnixEpoch);

    private static HeldOutScorerIdentity MeasurementNoiseIdentity { get; } = new(
        DomainTag: "design-conversion-measurement-gate",
        ScorerId: "held-out-gate-scorer",
        ScorerVersion: "v1",
        NormalizationProfileId: "gate-normalization",
        MetricProfileId: "held-out-quality-score");

    private static IReadOnlyList<DesignConversionDpoTrainingRecord> BuildSyntheticSourceDisjointRecords(int count)
    {
        var result = new DesignConversionDpoGenerator().BuildTrainReadyRecords(
            Enumerable.Range(0, count)
                .Select(index =>
                {
                    var unitId = index % 2 == 0
                        ? DesignConversionDpoUnits.CriticUnit
                        : DesignConversionDpoUnits.ImproverUnit;
                    var example = DesignConversionDpoGeneratorTests.CreateExample(
                        $"s3-measurement-source-disjoint-{index:000}",
                        unitId,
                        usefulness: 0.8 + (index % 10 * 0.01));
                    return example with
                    {
                        Provenance = example.Provenance with
                        {
                            SourceRequestId = $"request:s3-measurement-source-{index:000}",
                            SourceManifestRef = $"manifest://dcm-source/s3-measurement-source-{index:000}",
                            Metadata = new Dictionary<string, string>
                            {
                                ["source_design_id"] = $"s3-measurement-source-{index:000}"
                            }
                        }
                    };
                })
                .ToArray());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Empty(result.Value!.QuarantinedRecords);
        Assert.Equal(count, result.Value.AcceptedRecords.Count);
        return result.Value.AcceptedRecords;
    }

    private static DesignConversionDpoDatasetOptions LocalDatasetOptions(
        string DatasetId = "dcm-dpo-measurement-dataset",
        int MinimumTrainRecords = 50,
        int MinimumHeldOutRecords = 50)
        => new(
            DatasetId: DatasetId,
            MinimumTrainRecords: MinimumTrainRecords,
            MinimumHeldOutRecords: MinimumHeldOutRecords,
            Preflight: DesignConversionDpoDatasetPreflight.LocalFixtureTrainingCandidateOnly());

    private static DesignConversionTrainingManifestArtifacts BuildArtifacts()
        => new(
            CheckpointPath: "artifacts/design-conversion/checkpoint.json",
            TokenizerPath: "artifacts/design-conversion/tokenizer.json",
            PolicyPath: "artifacts/design-conversion/policy.bin");

    private static DesignConversionPromotionGateReport BuildPromotionGate()
    {
        var artifacts = BuildArtifacts();
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
            EvidenceRefs: ["synthetic-promotion-gate:pass"]);
    }

    private sealed class FakePromotionUploader : IDesignConversionBoundedPromotionUploader,
        IDesignConversionPromotionUploaderPreflight
    {
        public int UploadCallCount { get; private set; }
        public int MaximumSerializedReceiptUtf8Bytes => 64 * 1024;
        public AgentInfrastructure.OperationResult<string> PreflightConfigurationFingerprintSha256()
            => AgentInfrastructure.OperationResult<string>.Ok(
                "sha256:" + new string('a', 64));

        public Task<AgentInfrastructure.OperationResult<DesignConversionPromotionUploadReceipt>> UploadAsync(
            DesignConversionTrainingManifest manifest,
            DesignConversionPromotionGateReport gateReport,
            CancellationToken ct = default)
        {
            UploadCallCount++;
            return Task.FromResult(AgentInfrastructure.OperationResult<DesignConversionPromotionUploadReceipt>.Ok(
                new DesignConversionPromotionUploadReceipt(
                    UploadId: "local-only-upload-receipt",
                    RemoteRef: DesignConversionPromotedLocalModelLoader.LocalOnlyPromotionRefPrefix +
                               Uri.EscapeDataString(manifest.ManifestId),
                    UploadedAtUtc: DateTimeOffset.UnixEpoch,
                    EvidenceRefs: ["LOCAL_ONLY_PROMOTION_UPLOADER_CALLED"])));
        }
    }
}
