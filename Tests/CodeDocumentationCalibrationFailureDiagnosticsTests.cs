// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

using System.Collections;
using System.Globalization;
using System.Text.Json;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ContinuousLearning;
using LoRA;
using TransformerModel;
using H = LlmFromScratch.Tests.CodeDocumentationPreparedBaselineTests;

namespace LlmFromScratch.Tests;

[Collection("RepoArtifactSnapshot")]
public sealed class CodeDocumentationCalibrationFailureDiagnosticsTests
{
    private const string PrivateText = "private-closed-exam-text-must-not-escape";

    [Fact]
    public async Task CalibrationExceptionBeforeEvaluator_PreservesSafeCauseAndRestoredAssessment()
    {
        using var h = HealthyHarness();
        var request = LoadGenuineRequest(h);
        var files = h.Files();
        var before = ModelState(h, request.Model);
        var inner = new FileLoadException(PrivateText, PrivateText + ".dll");
        var failure = new InvalidOperationException(PrivateText, inner);
        var invalid = request with { EvidenceRefs = new ThrowingEvidenceRefs(failure) };

        var rejected = await new CodeDocumentationSavedModelAssessmentCalibrationService()
            .CalibrateAsync(invalid, h.Context.NoisePolicyConfig, h.Context.MaximumDuration);

        Assert.False(rejected.IsSuccess);
        Assert.Null(rejected.Value);
        Assert.Equal(CodeDocumentationSavedModelAssessmentCalibrationService.CalibrationConditionMismatchErrorCode, rejected.ErrorCode);
        Assert.Contains("failureStage=assessment-repeat-001", rejected.ErrorMessage);
        Assert.Contains("exception[0]=System.InvalidOperationException; HResult=0x" + failure.HResult.ToString("X8", CultureInfo.InvariantCulture), rejected.ErrorMessage);
        Assert.Contains("exception[1]=System.IO.FileLoadException; HResult=0x" + inner.HResult.ToString("X8", CultureInfo.InvariantCulture), rejected.ErrorMessage);
        Assert.DoesNotContain(PrivateText, rejected.ErrorMessage);
        Assert.DoesNotContain(" at ", rejected.ErrorMessage);
        Assert.Equal(0, h.Client.Calls);
        Assert.False(File.Exists(h.Fixture.OrdinaryClaim));
        Assert.False(File.Exists(h.Fixture.OrdinaryBaseline));
        Assert.Equal(before, ModelState(h, request.Model));
        h.AssertFiles(files);

        // A deep private exception chain must remain finite and must never expose its messages or filename.
        Exception deepFailure = inner;
        for (var depth = 0; depth < 10; depth++)
            deepFailure = new InvalidOperationException(PrivateText, deepFailure);
        var bounded = await new CodeDocumentationSavedModelAssessmentCalibrationService()
            .CalibrateAsync(request with { EvidenceRefs = new ThrowingEvidenceRefs(deepFailure) },
                h.Context.NoisePolicyConfig, h.Context.MaximumDuration);
        Assert.False(bounded.IsSuccess);
        Assert.Equal(CodeDocumentationSavedModelAssessmentCalibrationService.CalibrationConditionMismatchErrorCode, bounded.ErrorCode);
        Assert.Contains("failureStage=assessment-repeat-001", bounded.ErrorMessage);
        Assert.Contains("exception[7]=System.InvalidOperationException; HResult=0x80131509; inner-cause-chain-truncated", bounded.ErrorMessage);
        Assert.DoesNotContain("exception[8]", bounded.ErrorMessage);
        Assert.DoesNotContain(PrivateText, bounded.ErrorMessage);
        Assert.Equal(0, h.Client.Calls);
        Assert.Equal(before, ModelState(h, request.Model));
        h.AssertFiles(files);

        // Restore the original genuine request, rather than altering settings or the guard.
        var restored = await new CodeDocumentationSavedModelAssessmentCalibrationService()
            .CalibrateAsync(request, h.Context.NoisePolicyConfig, h.Context.MaximumDuration);
        Assert.True(restored.IsSuccess, restored.ErrorCode + ": " + restored.ErrorMessage);
        Assert.Equal(10, restored.Value!.Calibration.ObservedEvaluationCallCount);
        Assert.Equal(10, h.Client.Calls);
        Assert.Equal(before, ModelState(h, request.Model));
        h.AssertFiles(files);
    }

    [Fact]
    public async Task OrdinaryBaselineFailure_ForwardsLowerDetailAndKeepsStartedHistoryWithoutRetry()
    {
        using (var h = HealthyHarness())
        using (var client = new RefusingEvaluationClient())
        {
            var owner = new CodeDocumentationRepositoryOwnedPreparedBaselineJournal();
            var failedContext = h.Context with { EvaluationClient = client };
            var rejected = await ProduceOrdinary(owner, h, failedContext);
            Assert.False(rejected.IsSuccess);
            Assert.Null(rejected.Value);
            Assert.Equal(CodeDocumentationRepositoryOwnedPreparedBaselineJournal.RecoveryRequiredErrorCode, rejected.ErrorCode);
            Assert.Contains(CodeDocumentationSavedModelAssessmentService.EvaluationFailedErrorCode, rejected.ErrorMessage);
            Assert.Contains("Проверяющий не вернул допустимый ответ для конкретного отложенного задания.", rejected.ErrorMessage);
            Assert.DoesNotContain(PrivateText, rejected.ErrorMessage);
            Assert.DoesNotContain("failureStage=", rejected.ErrorMessage); // A returned evaluator refusal is distinct from the calibration exception branch.
            Assert.Equal(1, client.Attempts);
            Assert.Equal(0, h.Client.Calls);
            Assert.True(File.Exists(h.Fixture.OrdinaryClaim));
            Assert.False(File.Exists(h.Fixture.OrdinaryBaseline));
            using var started = JsonDocument.Parse(Assert.Single(File.ReadAllLines(h.Fixture.OrdinaryClaim)));
            Assert.Equal("Started", started.RootElement.GetProperty("Kind").GetString());
            var files = h.Files();

            // Restoring the healthy client cannot reopen Started as permission to measure again.
            var retry = await ProduceOrdinary(owner, h, h.Context);
            Assert.False(retry.IsSuccess);
            Assert.Equal(CodeDocumentationRepositoryOwnedPreparedBaselineJournal.RecoveryRequiredErrorCode, retry.ErrorCode);
            Assert.Equal(1, client.Attempts);
            Assert.Equal(0, h.Client.Calls);
            h.AssertFiles(files);
        }

        // An independent healthy fixture proves production and receiving-owner reopening remain reachable.
        using var healthy = HealthyHarness();
        var complete = await ProduceOrdinary(new(), healthy, healthy.Context);
        Assert.True(complete.IsSuccess, complete.ErrorCode + ": " + complete.ErrorMessage);
        Assert.Equal(10, healthy.Client.Calls);
        var committed = healthy.Files();
        var reopened = await new CodeDocumentationRepositoryOwnedPreparedBaselineJournal()
            .OpenExistingOrdinaryAsync(healthy.Catalog, healthy.Fixture.OrdinarySelector,
                healthy.Start.RepositoryRoot, healthy.Start.DomainSlug, healthy.Start.DesignPatternSlug,
                healthy.Start.ModelIdSlug, healthy.Start.ModelVersion, healthy.Start.ModelConfig,
                healthy.Start.RuntimeCompatibility, healthy.Context, healthy.Start.LoraConfig,
                healthy.Start.AdapterSeedAuthority, CancellationToken.None);
        Assert.True(reopened.IsSuccess, reopened.ErrorCode + ": " + reopened.ErrorMessage);
        Assert.Equal(complete.Value!.Readback.Sha256, reopened.Value!.Readback.Sha256);
        Assert.Equal(10, healthy.Client.Calls);
        healthy.AssertFiles(committed);
    }

    private static H.OrdinaryDiagnosticHarness HealthyHarness()
        => new((_, _) => (false, CodeDocumentationSavedModelAssessmentService.MissingEssentialBehaviorReasonCode));

    private static Task<OperationResult<CodeDocumentationRepositoryOwnedPreparedBaselineJournal.OrdinaryOriginal>> ProduceOrdinary(
        CodeDocumentationRepositoryOwnedPreparedBaselineJournal owner, H.OrdinaryDiagnosticHarness h,
        CodeDocumentationRepositoryOwnedDpoTrainingQualityGateContext context)
        => owner.ProduceOrOpenOrdinaryAsync(h.Catalog, h.Fixture.OrdinarySelector,
            h.Start.RepositoryRoot, h.Start.DomainSlug, h.Start.DesignPatternSlug,
            h.Start.ModelIdSlug, h.Start.ModelVersion, h.Start.ModelConfig,
            h.Start.RuntimeCompatibility, context, h.Start.LoraConfig, h.Start.AdapterSeedAuthority, CancellationToken.None);

    private static CodeDocumentationSavedModelAssessmentRequest LoadGenuineRequest(H.OrdinaryDiagnosticHarness h)
    {
        var loaded = new RepositoryOwnedModelCheckpointLoader().LoadLatestCompatible(h.Start.RepositoryRoot,
            h.Start.DomainSlug, h.Start.DesignPatternSlug, h.Start.ModelIdSlug, h.Start.ModelVersion, h.Start.RuntimeCompatibility);
        Assert.True(loaded.IsSuccess, loaded.ErrorCode + ": " + loaded.ErrorMessage);
        using var checkpoint = loaded.Value!;
        var model = new LlmModel(h.Start.ModelConfig);
        var seeds = h.Start.AdapterSeedAuthority!;
        LoraApplicator.ApplySeeded(model, h.Start.LoraConfig!, seeds.RootInitializationSeed, seeds.PolicyDropoutStreamDomain);
        checkpoint.Weights.Position = 0;
        model.LoadCheckpoint(checkpoint.Weights);
        var parent = checkpoint.Manifest;
        var passport = new CodeDocumentationSavedModelAssessmentModelPassport(parent.Domain, parent.DesignPattern,
            parent.ModelId, parent.Version, parent.Step, parent.RepositoryRelativeManifestPath, parent.ManifestSha256,
            parent.RepositoryRelativeWeightsPath, checkpoint.WeightsSha256, checkpoint.WeightsByteLength,
            checkpoint.WeightsByteLength, RepositoryRelativeModelIdentityPath: parent.RepositoryRelativeManifestPath,
            ModelIdentitySha256: parent.ManifestSha256);
        return CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService.BuildAssessmentRequest(h.Context,
            model, passport, "calibration-failure-diagnostics");
    }

    private static byte[] ModelState(H.OrdinaryDiagnosticHarness h, LlmModel model)
        => CodeDocumentationRepositoryOwnedPreparedBaselineJournal.CaptureState(model,
            h.Start.RuntimeCompatibility, h.Start.LoraConfig, h.Start.AdapterSeedAuthority);

    private sealed class ThrowingEvidenceRefs(Exception failure) : IReadOnlyList<string>
    {
        public int Count => 1;
        public string this[int index] => throw failure;
        public IEnumerator<string> GetEnumerator() => throw failure;
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    private sealed class RefusingEvaluationClient : ILegalTeacherEvaluationInferenceClient
    {
        internal int Attempts;
        public Task<OperationResult<TeacherEvaluationInferenceResult>> EvaluateAsync(string prompt,
            ILegalTeacherCallPermit permit, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Attempts++;
            // This controlled unavailable dependency issues no permit and exposes no exam input.
            return Task.FromResult(OperationResult<TeacherEvaluationInferenceResult>.Fail(PrivateText,
                CodeDocumentationSavedModelAssessmentService.EvaluationFailedErrorCode));
        }
        public void Dispose() { }
    }
}
