// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using ContinuousLearning;
using DesignConversionDomain;
using System.Globalization;

public sealed class DesignConversionDpoTeacherAuthoringTests
{
    private const string ModelId = "qwen-test:9b";
    private static readonly string ManifestDigest = new('c', 64);
    private static readonly string ModelLayerDigest = new('d', 64);

    [Fact]
    public async Task AuthorCandidateAsync_AuthorsOneServerIdentifiedCandidate()
    {
        var incumbent = DesignConversionDpoGeneratorTests.CreateRuntimeEvaluation(
            canPersistTrainingCandidate: false,
            score: 0.46,
            passed: false,
            htmlSuffix: "incumbent");
        using var client = new AuthoringTeacherClient(prompt =>
            BuildOutput(prompt));
        using var runtime = CreateRuntime(client);

        var authored = await runtime.AuthorCandidateAsync(
            CreateUpdateRequest(incumbent),
            CreateReservation());

        Assert.True(authored.IsSuccess, authored.ErrorMessage);
        Assert.Equal(1, client.InvocationCount);
        Assert.StartsWith("teacher-authored:", authored.Value!.Candidate.CandidateId);
        Assert.StartsWith("teacher-authored-request:", authored.Value.Candidate.RequestId);
        var authoringEvidence = Assert.IsType<
            DesignConversionDpoTeacherCandidateAuthoringEvidence>(
                authored.Value.AuthoringEvidence);
        Assert.Equal(
            LocalQwenOllamaInferenceProfileAuthority.Authoring.ProfileId,
            authoringEvidence.InferenceProfileId);
        Assert.Equal(
            LocalQwenOllamaInferenceProfileAuthority.Authoring.ConfigurationSha256,
            authoringEvidence.InferenceProfileConfigurationSha256);
        Assert.Equal(
            LocalQwenOllamaInferenceProfileAuthority.Authoring.ProfileId,
            authoringEvidence.Metadata["teacher_inference_profile_id"]);
        Assert.Contains(
            $"teacher-inference-profile:configuration_sha256={LocalQwenOllamaInferenceProfileAuthority.Authoring.ConfigurationSha256}",
            authoringEvidence.EvidenceRefs);
        Assert.Equal(
            authoringEvidence.GeneratedRef,
            DesignConversionDpoTeacherAttestationRuntime
                .BuildGeneratedRefFromCandidateId(
                    authoringEvidence.GeneratedCandidateId));
        Assert.Equal(
            DesignConversionDpoTeacherAttestationRuntime.BuildIncumbentBinding(
                CreateUpdateRequest(incumbent).CurrentRequest),
            authoringEvidence.IncumbentBinding);
        Assert.DoesNotContain(
            client.LastOutput!,
            authoringEvidence.Metadata.Values);
        Assert.Equal(bool.TrueString, authoringEvidence.Metadata[
            "candidate_authoring_only"]);
        Assert.False(authoringEvidence.Metadata.ContainsKey(
            "runtime_teacher_selected_preference"));
        Assert.Equal(
            bool.FalseString,
            authored.Value.Candidate.Metadata["raw_teacher_output_persisted"]);
        Assert.Equal(
            bool.FalseString,
            authored.Value.Candidate.Metadata["trained_model_authority"]);
    }

    [Fact]
    public void AuthorCandidateAsync_StrictlyRejectsMalformedDuplicateOversizedAndNoOpOutputs()
    {
        var incumbent = DesignConversionDpoGeneratorTests.CreateRuntimeEvaluation(
            canPersistTrainingCandidate: false,
            score: 0.45,
            passed: false,
            htmlSuffix: "incumbent");
        var request = CreateUpdateRequest(incumbent);
        var valid = BuildOutputForRequest(
            request,
            "generated");
        var unsafeControl = BuildOutputForRequest(
            request,
            "generated",
            candidateHtml: "<main>unsafe\u0001</main>");
        var invalid = new (string Name, string Output, string ExpectedErrorCode)[]
        {
            (
                "markdown fence",
                "```json\n" + valid + "\n```",
                DesignConversionDpoTeacherAttestationRuntime
                    .AuthoringOutputInvalidErrorCode),
            (
                "unknown property",
                valid.Insert(1, "\"unknown\":\"value\","),
                DesignConversionDpoTeacherAttestationRuntime
                    .AuthoringOutputInvalidErrorCode),
            (
                "obsolete preferred side",
                valid.Insert(1, "\"preferred_side\":\"generated\","),
                DesignConversionDpoTeacherAttestationRuntime
                    .AuthoringOutputInvalidErrorCode),
            (
                "duplicate schema property",
                valid.Insert(1, "\"schema\":\"duplicate\","),
                DesignConversionDpoTeacherAttestationRuntime
                    .AuthoringOutputInvalidErrorCode),
            (
                "oversized output",
                new string(
                    'x',
                    DesignConversionDpoTeacherAttestationRuntime
                        .MaximumAuthoringOutputUtf8Bytes + 1),
                DesignConversionDpoTeacherAttestationRuntime
                    .AuthoringOutputInvalidErrorCode),
            (
                "no-op candidate",
                BuildOutputForRequest(
                    request,
                    "generated",
                    request.CurrentRequest.Html,
                    request.CurrentRequest.Css),
                DesignConversionDpoTeacherAttestationRuntime
                    .AuthoringCandidateNoOpErrorCode),
            (
                "unsafe control",
                unsafeControl,
                DesignConversionDpoTeacherAttestationRuntime
                    .AuthoringOutputInvalidErrorCode)
        };

        var parsedValid = DesignConversionDpoTeacherAttestationRuntime
            .ParseAuthoringOutputForTesting(valid, request, ValidProvenance());
        Assert.True(parsedValid.IsSuccess, parsedValid.ErrorMessage);
        using (var unsafeDocument = System.Text.Json.JsonDocument.Parse(unsafeControl))
        {
            Assert.Contains(
                '\u0001',
                unsafeDocument.RootElement.GetProperty("candidate_html").GetString()!);
        }

        Assert.All(invalid, testCase =>
        {
            Assert.False(
                string.Equals(valid, testCase.Output, StringComparison.Ordinal),
                $"Malformed fixture '{testCase.Name}' must differ from the valid baseline.");
            var parsed = DesignConversionDpoTeacherAttestationRuntime
                .ParseAuthoringOutputForTesting(
                    testCase.Output,
                    request,
                    ValidProvenance());
            Assert.False(parsed.IsSuccess, testCase.Name);
            Assert.Null(parsed.Value);
            Assert.Equal(testCase.ExpectedErrorCode, parsed.ErrorCode);
            Assert.Equal(
                DesignConversionDpoTeacherAttestationRuntime.FailureSafeMessage,
                parsed.ErrorMessage);
        });
    }

    [Fact]
    public async Task AuthorCandidateAsync_UsesTransportSizedAuthoringLimitsWithoutTruncation()
    {
        Assert.Equal(
            65_536,
            DesignConversionDpoTeacherAttestationRuntime
                .MaximumAuthoringPromptUtf8Bytes);
        Assert.Equal(
            262_144,
            DesignConversionDpoTeacherAttestationRuntime
                .MaximumAuthoringOutputUtf8Bytes);
        Assert.Equal(
            12 * 1024,
            DesignConversionDpoTeacherAttestationRuntime.MaximumPromptUtf8Bytes);
        Assert.Equal(
            8 * 1024,
            DesignConversionDpoTeacherAttestationRuntime.MaximumOutputUtf8Bytes);

        var incumbent = DesignConversionDpoGeneratorTests.CreateRuntimeEvaluation(
            canPersistTrainingCandidate: false,
            score: 0.45,
            passed: false,
            htmlSuffix: "authoring-limit");
        var request = CreateUpdateRequest(incumbent);
        using var acceptsFortyKiB = new AuthoringTeacherClient(prompt =>
            BuildOutput(
                prompt,
                "generated",
                candidateHtml: "<main>" + new string('h', 40 * 1024) + "</main>"));
        using var runtime = CreateRuntime(acceptsFortyKiB);

        var accepted = await runtime.AuthorCandidateAsync(
            request,
            CreateReservation());

        Assert.True(accepted.IsSuccess, accepted.ErrorMessage);
        Assert.Equal(1, acceptsFortyKiB.InvocationCount);
        Assert.Equal(
            "<main>" + new string('h', 40 * 1024) + "</main>",
            accepted.Value!.Candidate.Html);

        var overPromptRequest = request with
        {
            CurrentRequest = request.CurrentRequest with
            {
                Html = new string(
                    'p',
                    DesignConversionDpoTeacherAttestationRuntime
                        .MaximumAuthoringPromptUtf8Bytes)
            }
        };
        using var promptClient = new AuthoringTeacherClient(prompt =>
            BuildOutput(
                prompt,
                "generated"));
        using var promptRuntime = CreateRuntime(promptClient);
        var promptRejected = await promptRuntime.AuthorCandidateAsync(
            overPromptRequest,
            CreateReservation());

        Assert.False(promptRejected.IsSuccess);
        Assert.Equal(0, promptClient.InvocationCount);
        Assert.Equal(
            DesignConversionDpoTeacherAttestationRuntime
                .AuthoringOutputInvalidErrorCode,
            promptRejected.ErrorCode);

        using var outputClient = new AuthoringTeacherClient(prompt =>
            BuildOutput(
                prompt,
                "generated",
                candidateHtml: new string(
                    'o',
                    DesignConversionDpoTeacherAttestationRuntime
                        .MaximumAuthoringOutputUtf8Bytes)));
        using var outputRuntime = CreateRuntime(outputClient);
        var outputRejected = await outputRuntime.AuthorCandidateAsync(
            request,
            CreateReservation());

        Assert.False(outputRejected.IsSuccess);
        Assert.Equal(1, outputClient.InvocationCount);
        Assert.NotNull(outputClient.LastOutput);
        Assert.True(
            System.Text.Encoding.UTF8.GetByteCount(outputClient.LastOutput) >
            DesignConversionDpoTeacherAttestationRuntime
                .MaximumAuthoringOutputUtf8Bytes);
        Assert.Equal(
            DesignConversionDpoTeacherAttestationRuntime
                .AuthoringOutputInvalidErrorCode,
            outputRejected.ErrorCode);
    }

    [Fact]
    public async Task AuthorCandidateAsync_SanitizesOrdinaryFailureButPreservesCancellationAndFatalGraphs()
    {
        const string rawException = "RAW_AUTHORING_EXCEPTION_MUST_NOT_ESCAPE";
        var incumbent = DesignConversionDpoGeneratorTests.CreateRuntimeEvaluation(
            canPersistTrainingCandidate: false,
            score: 0.45,
            passed: false,
            htmlSuffix: "authoring-exception");
        var request = CreateUpdateRequest(incumbent);
        using var ordinaryClient = new ThrowingTeacherClient(
            new InvalidOperationException(rawException));
        using var ordinaryRuntime = CreateRuntime(ordinaryClient);

        var ordinary = await ordinaryRuntime.AuthorCandidateAsync(
            request,
            CreateReservation());

        Assert.False(ordinary.IsSuccess);
        Assert.Equal(
            DesignConversionDpoTeacherAttestationRuntime.RuntimeUnavailableErrorCode,
            ordinary.ErrorCode);
        Assert.DoesNotContain(rawException, ordinary.ErrorMessage, StringComparison.Ordinal);
        Assert.DoesNotContain(rawException, ordinary.ErrorCode, StringComparison.Ordinal);

        using var suppliedCancellation = new CancellationTokenSource();
        using var suppliedClient = new CancellingTeacherClient(suppliedCancellation);
        using var suppliedRuntime = CreateRuntime(suppliedClient);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            suppliedRuntime.AuthorCandidateAsync(
                request,
                CreateReservation(),
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
            using var client = new ThrowingTeacherClient(exception);
            using var candidateRuntime = CreateRuntime(client);
            var thrown = await Record.ExceptionAsync(() =>
                candidateRuntime.AuthorCandidateAsync(
                    request,
                    CreateReservation()));
            Assert.Same(exception, thrown);
        }
    }

    [Fact]
    public async Task AuthorCandidateAsync_RejectsMismatchedCanonicalTeacherProvenance()
    {
        var incumbent = DesignConversionDpoGeneratorTests.CreateRuntimeEvaluation(
            canPersistTrainingCandidate: false,
            score: 0.45,
            passed: false,
            htmlSuffix: "authoring-provenance");
        var request = CreateUpdateRequest(incumbent);
        var invalidProvenance = ValidProvenance() with
        {
            ModelArtifactDigest = new string('e', 64)
        };
        using var client = new AuthoringTeacherClient(
            prompt => BuildOutput(
                prompt,
                "generated"),
            () => invalidProvenance);
        using var runtime = CreateRuntime(client);

        var result = await runtime.AuthorCandidateAsync(
            request,
            CreateReservation());

        Assert.False(result.IsSuccess);
        Assert.Equal(
            DesignConversionDpoTeacherAttestationRuntime.ProvenanceInvalidErrorCode,
            result.ErrorCode);
        Assert.Equal(1, client.InvocationCount);
    }

    [Fact]
    public async Task AuthorCandidateAsync_RejectsMissingEvaluationAndMalformedInferenceProfiles()
    {
        var incumbent = DesignConversionDpoGeneratorTests.CreateRuntimeEvaluation(
            canPersistTrainingCandidate: false,
            score: 0.45,
            passed: false,
            htmlSuffix: "authoring-profile");
        var authoring = LocalQwenOllamaInferenceProfileAuthority.Authoring;
        TrainingTeacherInferenceProfileEvidence?[] rejectedProfiles =
        [
            null,
            LocalQwenOllamaInferenceProfileAuthority.Evaluation,
            new("unknown-authoring-profile", authoring.ConfigurationSha256),
            new(authoring.ProfileId, new string('a', 64)),
            new(authoring.ProfileId, authoring.ConfigurationSha256.ToUpperInvariant()),
            new(authoring.ProfileId, $"sha256:{authoring.ConfigurationSha256}")
        ];

        foreach (var profile in rejectedProfiles)
        {
            using var client = new AuthoringTeacherClient(
                prompt => BuildOutput(
                    prompt,
                    "generated"),
                inferenceProfileFactory: () => profile);
            using var runtime = CreateRuntime(client);

            var result = await runtime.AuthorCandidateAsync(
                CreateUpdateRequest(incumbent),
                CreateReservation());

            Assert.False(result.IsSuccess);
            Assert.Equal(
                DesignConversionDpoTeacherAttestationRuntime
                    .InferenceProfileInvalidErrorCode,
                result.ErrorCode);
            Assert.Equal(1, client.InvocationCount);
        }
    }

    [Fact]
    public async Task AttestAsync_RejectsMissingEvaluationAndMalformedInferenceProfiles()
    {
        var chosen = DesignConversionDpoGeneratorTests.CreateRuntimeEvaluation(
            canPersistTrainingCandidate: true,
            htmlSuffix: "attestation-profile-chosen");
        var rejected = DesignConversionDpoGeneratorTests.CreateRuntimeEvaluation(
            canPersistTrainingCandidate: false,
            jobId: "attestation-profile-rejected-job",
            requestId: "attestation-profile-rejected-request",
            outputId: "attestation-profile-rejected-output",
            candidateId: "attestation-profile-rejected-candidate",
            verdictId: "attestation-profile-rejected-verdict",
            score: 0.4,
            passed: false,
            htmlSuffix: "attestation-profile-rejected");
        var authoring = LocalQwenOllamaInferenceProfileAuthority.Authoring;
        TrainingTeacherInferenceProfileEvidence?[] rejectedProfiles =
        [
            null,
            LocalQwenOllamaInferenceProfileAuthority.Evaluation,
            new("unknown-authoring-profile", authoring.ConfigurationSha256),
            new(authoring.ProfileId, new string('a', 64)),
            new(authoring.ProfileId, authoring.ConfigurationSha256.ToUpperInvariant()),
            new(authoring.ProfileId, $"sha256:{authoring.ConfigurationSha256}")
        ];

        foreach (var profile in rejectedProfiles)
        {
            using var client = new AuthoringTeacherClient(
                BuildAttestationOutput,
                inferenceProfileFactory: () => profile);
            using var runtime = CreateRuntime(client);

            var result = await runtime.AttestAsync(
                chosen,
                rejected,
                CreateReservation());

            Assert.False(result.IsSuccess);
            Assert.Equal(
                DesignConversionDpoTeacherAttestationRuntime
                    .InferenceProfileInvalidErrorCode,
                result.ErrorCode);
            Assert.Equal(1, client.InvocationCount);
        }
    }

    [Fact]
    public async Task TeacherAuthoredGenerator_CandidateAuthoringCannotMintPreferenceRows()
    {
        var incumbent = DesignConversionDpoGeneratorTests.CreateRuntimeEvaluation(
            canPersistTrainingCandidate: false,
            jobId: "incumbent-job",
            requestId: "incumbent-request",
            outputId: "incumbent-output",
            candidateId: "incumbent-candidate",
            verdictId: "incumbent-verdict",
            score: 0.46,
            passed: false,
            htmlSuffix: "incumbent");
        using var client = new AuthoringTeacherClient(prompt => BuildOutput(
            prompt,
            "generated"));
        using var runtime = CreateRuntime(client);
        var authored = await runtime.AuthorCandidateAsync(
            CreateUpdateRequest(incumbent),
            CreateReservation());

        Assert.True(authored.IsSuccess, authored.ErrorMessage);
        Assert.Equal(1, client.InvocationCount);
        var authoredValue = Assert.IsType<DesignConversionTeacherAuthoredCandidateResult>(
            authored.Value);
        var preference = Assert.IsType<
            DesignConversionDpoTeacherCandidateAuthoringEvidence>(
                authoredValue.AuthoringEvidence);
        Assert.Equal(
            DesignConversionDpoTeacherAttestationRuntime.AuthoringSchema,
            preference.Schema);
        Assert.Equal(authoredValue.Candidate.RequestId, preference.GeneratedRequestId);
        Assert.Equal(authoredValue.Candidate.CandidateId, preference.GeneratedCandidateId);
        Assert.Equal(bool.TrueString, preference.Metadata[
            "candidate_authoring_only"]);
        Assert.False(preference.Metadata.ContainsKey(
            "runtime_teacher_selected_preference"));
        Assert.Equal(bool.FalseString, preference.Metadata[
            DesignConversionModelAuthorityBoundary.TrainedModelAuthorityKey]);
        Assert.NotEqual(
            incumbent.SubmittedRequest.Html,
            authoredValue.Candidate.Html);
        Assert.NotEqual(
            incumbent.SubmittedRequest.Css,
            authoredValue.Candidate.Css);

        var generated = AsGeneratedEvaluation(
            authoredValue,
            score: 0.88,
            passed: true,
            canPersist: true);
        Assert.True(
            generated.VisualQaBoundaryTrace.VisualDecision.QualityScore >
            incumbent.VisualQaBoundaryTrace.VisualDecision.QualityScore);
        var build = new DesignConversionDpoGenerator()
            .BuildRuntimeTeacherAuthoredTrainingCandidates(
                generated,
                incumbent,
                preference);

        Assert.True(build.IsSuccess, build.ErrorMessage);
        Assert.Empty(build.Value!.AcceptedRecords);
        Assert.Empty(build.Value.QuarantinedRecords);
        Assert.Contains("independent_preference_not_proven", build.Value.Warnings);
        Assert.Contains(
            "runtime_rejected_independently_evaluated=false",
            build.Value.Warnings);
        Assert.Contains(
            "high_quality_negative_pair_not_proven",
            build.Value.Warnings);
        Assert.DoesNotContain(
            "runtime_rejected_independently_evaluated=true",
            build.Value.Warnings);
        Assert.DoesNotContain(
            "high_quality_negative_pair_proven",
            build.Value.Warnings);
        // Exact dual-target custody/atomicity remains covered by the manual,
        // non-self-labelled H2 aggregate tests; this live producer never reaches it.
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task TeacherAuthoredGenerator_MeasuredScoreDirectionCannotMintTrainingRows(
        bool generatedScoresHigher)
    {
        var incumbent = DesignConversionDpoGeneratorTests.CreateRuntimeEvaluation(
            canPersistTrainingCandidate: !generatedScoresHigher,
            jobId: $"candidate-only-{generatedScoresHigher}-incumbent-job",
            requestId: $"candidate-only-{generatedScoresHigher}-incumbent-request",
            outputId: $"candidate-only-{generatedScoresHigher}-incumbent-output",
            candidateId: $"candidate-only-{generatedScoresHigher}-incumbent-candidate",
            verdictId: $"candidate-only-{generatedScoresHigher}-incumbent-verdict",
            score: generatedScoresHigher
                ? 0.41
                : 0.89,
            passed: !generatedScoresHigher,
            htmlSuffix: $"candidate-only-{generatedScoresHigher}-incumbent");
        var authored = await AuthorAsync(incumbent);
        Assert.True(authored.IsSuccess, authored.ErrorMessage);
        Assert.NotNull(authored.Value!.AuthoringEvidence);
        Assert.False(string.IsNullOrWhiteSpace(authored.Value.Candidate.Html));
        Assert.False(string.IsNullOrWhiteSpace(authored.Value.Candidate.Css));
        var generated = AsGeneratedEvaluation(
            authored.Value,
            score: generatedScoresHigher
                ? 0.89
                : 0.31,
            passed: generatedScoresHigher,
            canPersist: generatedScoresHigher);

        var build = new DesignConversionDpoGenerator()
            .BuildRuntimeTeacherAuthoredTrainingCandidates(
                generated,
                incumbent,
                authored.Value.AuthoringEvidence!);

        Assert.True(build.IsSuccess, build.ErrorMessage);
        Assert.Empty(build.Value!.AcceptedRecords);
        Assert.Empty(build.Value.QuarantinedRecords);
        Assert.Contains("independent_preference_not_proven", build.Value.Warnings);
        Assert.Contains(
            "runtime_rejected_independently_evaluated=false",
            build.Value.Warnings);
        Assert.Contains(
            "high_quality_negative_pair_not_proven",
            build.Value.Warnings);
        Assert.Contains("candidate_authoring_only=true", build.Value.Warnings);
        Assert.DoesNotContain(
            "runtime_rejected_independently_evaluated=true",
            build.Value.Warnings);
        Assert.DoesNotContain(
            "high_quality_negative_pair_proven",
            build.Value.Warnings);
    }

    [Fact]
    public async Task TeacherAuthoredGenerator_CandidateOnlyGuardPrecedesProjectionConsistency()
    {
        var pair = await CreateMeasuredPairAsync("atomic-second-target", 1);
        Assert.Equal(
            bool.TrueString,
            pair.AuthoringEvidence.Metadata["candidate_authoring_only"]);
        Assert.True(
            pair.GeneratedEvaluation.VisualQaBoundaryTrace.VisualDecision.QualityScore >
            pair.IncumbentEvaluation.VisualQaBoundaryTrace.VisualDecision.QualityScore);

        var canonical = new DesignConversionDpoGenerator()
            .BuildRuntimeTeacherAuthoredTrainingCandidates([pair]);
        var mismatchedEncoder = new DesignConversionDpoGenerator(
                new MismatchedImproverSourceEncoder())
            .BuildRuntimeTeacherAuthoredTrainingCandidates([pair]);

        Assert.True(canonical.IsSuccess, canonical.ErrorMessage);
        Assert.True(mismatchedEncoder.IsSuccess, mismatchedEncoder.ErrorMessage);
        Assert.Empty(canonical.Value!.AcceptedRecords);
        Assert.Empty(mismatchedEncoder.Value!.AcceptedRecords);
        Assert.Empty(canonical.Value.QuarantinedRecords);
        Assert.Empty(mismatchedEncoder.Value.QuarantinedRecords);
        Assert.Equal(canonical.Value.Warnings, mismatchedEncoder.Value.Warnings);
        Assert.Contains(
            "independent_preference_not_proven",
            mismatchedEncoder.Value.Warnings);
        Assert.DoesNotContain(
            "teacher_payload_custody_exact_validation_failed",
            mismatchedEncoder.Value.Warnings);
        // Manual H2 fixtures remain separate; candidate authoring never reaches
        // a positive projection body.
    }

    [Fact]
    public async Task TeacherAuthoredGenerator_CandidateEnvelopeReportsNoPreferenceAuthority()
    {
        var incumbent = CreateEnvelopeTestIncumbent("internal-state-authority");
        var authored = await AuthorAsync(
            incumbent,
            "generated");

        Assert.True(authored.IsSuccess, authored.ErrorMessage);
        var preference = Assert.IsType<
            DesignConversionDpoTeacherCandidateAuthoringEvidence>(
                authored.Value!.AuthoringEvidence);
        Assert.Equal(
            bool.TrueString,
            preference.Metadata["runtime_teacher_authored_candidate"]);
        Assert.Equal(
            bool.TrueString,
            preference.Metadata["candidate_authoring_only"]);
        Assert.False(preference.Metadata.ContainsKey(
            "runtime_teacher_selected_preference"));
        Assert.Equal(
            bool.FalseString,
            preference.Metadata[
                DesignConversionModelAuthorityBoundary.TrainedModelAuthorityKey]);
        Assert.Equal(
            bool.TrueString,
            preference.Metadata[
                "deterministic_measurement_used_as_admission_veto"]);

        var generated = AsGeneratedEvaluation(
            authored.Value,
            score: 0.88,
            passed: true,
            canPersist: true);
        var build = new DesignConversionDpoGenerator()
            .BuildRuntimeTeacherAuthoredTrainingCandidates(
                generated,
                incumbent,
                preference);

        Assert.True(build.IsSuccess, build.ErrorMessage);
        Assert.Empty(build.Value!.AcceptedRecords);
        Assert.Empty(build.Value.QuarantinedRecords);
        Assert.Contains("independent_preference_not_proven", build.Value.Warnings);
        Assert.Contains(
            "runtime_teacher_authored_candidate=True",
            build.Value.Warnings);
        Assert.Contains("candidate_authoring_only=true", build.Value.Warnings);
        Assert.Contains("trained_model_authority=False", build.Value.Warnings);
    }

    [Fact]
    public async Task TeacherAuthoredGenerator_ScoreChangesNeverCreateDirectionality()
    {
        var incumbent = DesignConversionDpoGeneratorTests.CreateRuntimeEvaluation(
            canPersistTrainingCandidate: true,
            jobId: "preferred-incumbent-job",
            requestId: "preferred-incumbent-request",
            outputId: "preferred-incumbent-output",
            candidateId: "preferred-incumbent-candidate",
            verdictId: "preferred-incumbent-verdict",
            score: 0.89,
            passed: true,
            htmlSuffix: "preferred-incumbent");
        var authored = await AuthorAsync(
            incumbent,
            "incumbent");
        Assert.True(authored.IsSuccess, authored.ErrorMessage);
        var preference = Assert.IsType<
            DesignConversionDpoTeacherCandidateAuthoringEvidence>(
                authored.Value!.AuthoringEvidence);
        var generated = AsGeneratedEvaluation(
            authored.Value,
            score: 0.31,
            passed: false,
            canPersist: false);
        Assert.True(
            incumbent.VisualQaBoundaryTrace.VisualDecision.QualityScore >
            generated.VisualQaBoundaryTrace.VisualDecision.QualityScore);

        var incumbentHigher = new DesignConversionDpoGenerator()
            .BuildRuntimeTeacherAuthoredTrainingCandidates(
                generated,
                incumbent,
                preference);
        var generatedHigher = new DesignConversionDpoGenerator()
            .BuildRuntimeTeacherAuthoredTrainingCandidates(
                generated with
                {
                    VisualQaBoundaryTrace = generated.VisualQaBoundaryTrace with
                    {
                        VisualDecision = generated.VisualQaBoundaryTrace.VisualDecision with
                        {
                            QualityScore = 0.95,
                            CanPersistTrainingCandidate = true
                        }
                    }
                },
                incumbent,
                preference);

        Assert.True(incumbentHigher.IsSuccess, incumbentHigher.ErrorMessage);
        Assert.Empty(incumbentHigher.Value!.AcceptedRecords);
        Assert.Contains(
            "independent_preference_not_proven",
            incumbentHigher.Value.Warnings);
        Assert.Contains(
            "runtime_rejected_independently_evaluated=false",
            incumbentHigher.Value.Warnings);
        Assert.True(generatedHigher.IsSuccess, generatedHigher.ErrorMessage);
        Assert.Empty(generatedHigher.Value!.AcceptedRecords);
        Assert.Contains(
            "independent_preference_not_proven",
            generatedHigher.Value.Warnings);
        Assert.Contains(
            "high_quality_negative_pair_not_proven",
            generatedHigher.Value.Warnings);
        Assert.DoesNotContain(
            "runtime_rejected_independently_evaluated=true",
            generatedHigher.Value.Warnings);
    }

    [Fact]
    public async Task TeacherAuthoredGenerator_PluralBatchOrderIsDeterministicAtCandidateOnlyBoundary()
    {
        var second = await CreateMeasuredPairAsync(
            "plural-second",
            1,
            "incumbent");
        var third = await CreateMeasuredPairAsync(
            "plural-third",
            2,
            "incumbent");
        var fourth = await CreateMeasuredPairAsync(
            "plural-fourth",
            3,
            "incumbent");
        Assert.Equal(
            new[] { 2, 3, 4 },
            new[] { second, third, fourth }.Select(pair => pair.AttemptNumber));

        var generator = new DesignConversionDpoGenerator();
        var scrambled = generator.BuildRuntimeTeacherAuthoredTrainingCandidates(
            [fourth, second, third]);
        var ordered = generator.BuildRuntimeTeacherAuthoredTrainingCandidates(
            [second, third, fourth]);

        Assert.True(scrambled.IsSuccess, scrambled.ErrorMessage);
        Assert.True(ordered.IsSuccess, ordered.ErrorMessage);
        Assert.Empty(scrambled.Value!.AcceptedRecords);
        Assert.Empty(ordered.Value!.AcceptedRecords);
        Assert.Empty(scrambled.Value.QuarantinedRecords);
        Assert.Empty(ordered.Value.QuarantinedRecords);
        Assert.Equal(ordered.Value.Warnings, scrambled.Value.Warnings);
        Assert.Contains("independent_preference_not_proven", scrambled.Value.Warnings);
        Assert.Contains(
            "runtime_teacher_authored_candidate=True",
            scrambled.Value.Warnings);
        Assert.Contains("candidate_authoring_only=true", scrambled.Value.Warnings);
        Assert.Contains(
            "runtime_rejected_independently_evaluated=false",
            scrambled.Value.Warnings);
        Assert.DoesNotContain(
            scrambled.Value.Warnings,
            warning => warning.StartsWith(
                "teacher_authoring_output_hash=",
                StringComparison.Ordinal));
    }

    [Fact]
    public async Task TeacherAuthoredGenerator_MeasuredScoreNormalizationIsEquivalentBeforeCandidateOnlyBlock()
    {
        var second = WithStaleRuntimeFlowScore(
            await CreateMeasuredPairAsync("measured-score-second", 1),
            chosenScore: 0.01,
            rejectedScore: 0.99);
        var third = WithStaleRuntimeFlowScore(
            await CreateMeasuredPairAsync("measured-score-third", 2),
            chosenScore: 0.02,
            rejectedScore: 0.98);
        var normalizedSecond = second with
        {
            GeneratedEvaluation = DesignConversionDpoGenerator
                .NormalizeMeasuredVisualScoreForDpo(second.GeneratedEvaluation),
            IncumbentEvaluation = DesignConversionDpoGenerator
                .NormalizeMeasuredVisualScoreForDpo(second.IncumbentEvaluation)
        };
        var normalizedThird = third with
        {
            GeneratedEvaluation = DesignConversionDpoGenerator
                .NormalizeMeasuredVisualScoreForDpo(third.GeneratedEvaluation),
            IncumbentEvaluation = DesignConversionDpoGenerator
                .NormalizeMeasuredVisualScoreForDpo(third.IncumbentEvaluation)
        };
        foreach (var pair in new[] { normalizedSecond, normalizedThird })
        {
            Assert.Equal(
                pair.GeneratedEvaluation.VisualQaBoundaryTrace.VisualDecision.QualityScore,
                pair.GeneratedEvaluation.RuntimeFlowResult.Output.Verdict.Score);
            Assert.Equal(
                pair.IncumbentEvaluation.VisualQaBoundaryTrace.VisualDecision.QualityScore,
                pair.IncumbentEvaluation.RuntimeFlowResult.Output.Verdict.Score);
        }

        var generator = new DesignConversionDpoGenerator();
        var createdAtUtc = DateTimeOffset.UnixEpoch;
        var direct = generator.BuildRuntimeTeacherAuthoredTrainingCandidates(
            [third, second],
            createdAtUtc);
        var explicitlyNormalized = generator.BuildRuntimeTeacherAuthoredTrainingCandidates(
            [normalizedThird, normalizedSecond],
            createdAtUtc);

        Assert.True(direct.IsSuccess, direct.ErrorMessage);
        Assert.True(explicitlyNormalized.IsSuccess, explicitlyNormalized.ErrorMessage);
        Assert.Empty(direct.Value!.AcceptedRecords);
        Assert.Empty(explicitlyNormalized.Value!.AcceptedRecords);
        Assert.Equal(direct.Value.Warnings, explicitlyNormalized.Value.Warnings);
        Assert.Contains("independent_preference_not_proven", direct.Value.Warnings);
    }

    [Fact]
    public async Task TeacherAuthoredGenerator_MixedScoreBatchCannotCreateDirectionality()
    {
        var second = await CreateMeasuredPairAsync("drop-second", 1);
        var disagreement = await CreateMeasuredPairAsync(
            "drop-third",
            2,
            "incumbent");
        var fourth = await CreateMeasuredPairAsync("drop-fourth", 3);

        var build = new DesignConversionDpoGenerator()
            .BuildRuntimeTeacherAuthoredTrainingCandidates(
                [second, disagreement, fourth]);

        Assert.True(build.IsSuccess, build.ErrorMessage);
        Assert.Empty(build.Value!.AcceptedRecords);
        Assert.Empty(build.Value.QuarantinedRecords);
        Assert.DoesNotContain(
            build.Value.Warnings,
            warning => warning.Contains(
                disagreement.AuthoringEvidence.GeneratedRef,
                StringComparison.Ordinal));
        Assert.Contains("independent_preference_not_proven", build.Value.Warnings);
        Assert.Contains(
            "runtime_rejected_independently_evaluated=false",
            build.Value.Warnings);
        Assert.Contains(
            "high_quality_negative_pair_not_proven",
            build.Value.Warnings);
        Assert.DoesNotContain(
            "runtime_rejected_independently_evaluated=true",
            build.Value.Warnings);
        Assert.DoesNotContain(
            "high_quality_negative_pair_proven",
            build.Value.Warnings);
    }

    [Fact]
    public async Task TeacherAuthoredGenerator_AllIncumbentHigherScoresProduceHonestZeroRows()
    {
        var second = await CreateMeasuredPairAsync(
            "all-disagreement-second",
            1,
            "incumbent");
        var third = await CreateMeasuredPairAsync(
            "all-disagreement-third",
            2,
            "incumbent");

        var build = new DesignConversionDpoGenerator()
            .BuildRuntimeTeacherAuthoredTrainingCandidates([third, second]);

        Assert.True(build.IsSuccess, build.ErrorMessage);
        Assert.Empty(build.Value!.AcceptedRecords);
        Assert.Empty(build.Value.QuarantinedRecords);
        Assert.Contains(
            "chosen_rejected_pair_or_negative_quality_not_proven",
            build.Value.Warnings);
        Assert.Contains("independent_preference_not_proven", build.Value.Warnings);
        Assert.Contains(
            "runtime_teacher_authored_candidate=True",
            build.Value.Warnings);
        Assert.Contains("candidate_authoring_only=true", build.Value.Warnings);
        Assert.Contains(
            "runtime_rejected_independently_evaluated=false",
            build.Value.Warnings);
        Assert.Contains("high_quality_negative_pair_not_proven", build.Value.Warnings);
        Assert.DoesNotContain("chosen_rejected_pair_proven", build.Value.Warnings);
        Assert.DoesNotContain("high_quality_negative_pair_proven", build.Value.Warnings);
        Assert.DoesNotContain(
            "runtime_rejected_independently_evaluated=true",
            build.Value.Warnings);
    }

    [Fact]
    public async Task TeacherAuthoredGenerator_PluralBatchRejectsTamperAndDuplicateIdentityAsWholeBatch()
    {
        var second = await CreateMeasuredPairAsync("reject-second", 1);
        var third = await CreateMeasuredPairAsync("reject-third", 2);
        var distinctSecondAttempt = await CreateMeasuredPairAsync(
            "reject-distinct-second-attempt",
            1);
        var generator = new DesignConversionDpoGenerator();

        Assert.Equal(second.AttemptNumber, distinctSecondAttempt.AttemptNumber);
        Assert.NotEqual(
            second.AuthoringEvidence.GeneratedBinding,
            distinctSecondAttempt.AuthoringEvidence.GeneratedBinding);
        Assert.NotEqual(
            second.AuthoringEvidence.GeneratedRequestId,
            distinctSecondAttempt.AuthoringEvidence.GeneratedRequestId);
        Assert.NotEqual(
            second.AuthoringEvidence.GeneratedCandidateId,
            distinctSecondAttempt.AuthoringEvidence.GeneratedCandidateId);
        Assert.NotEqual(
            second.AuthoringEvidence.Provenance.RequestId,
            distinctSecondAttempt.AuthoringEvidence.Provenance.RequestId);
        Assert.NotEqual(
            second.AuthoringEvidence.IncumbentBinding,
            distinctSecondAttempt.AuthoringEvidence.IncumbentBinding);
        Assert.NotEqual(
            second.GeneratedEvaluation.JobId,
            distinctSecondAttempt.GeneratedEvaluation.JobId);
        Assert.NotEqual(
            second.IncumbentEvaluation.SubmittedRequest.RequestId,
            distinctSecondAttempt.IncumbentEvaluation.SubmittedRequest.RequestId);

        var tampered = generator.BuildRuntimeTeacherAuthoredTrainingCandidates(
        [
            second,
            third with
            {
                AuthoringEvidence = third.AuthoringEvidence with
                {
                    GeneratedBinding = new string('a', 64)
                }
            }
        ]);
        var duplicateRequest = generator.BuildRuntimeTeacherAuthoredTrainingCandidates(
        [
            second,
            third with
            {
                AuthoringEvidence = third.AuthoringEvidence with
                {
                    Provenance = third.AuthoringEvidence.Provenance with
                    {
                        RequestId = second.AuthoringEvidence.Provenance.RequestId
                    }
                }
            }
        ]);
        var duplicateGeneratedBinding = generator
            .BuildRuntimeTeacherAuthoredTrainingCandidates(
            [
                second,
                third with
                {
                    GeneratedEvaluation = third.GeneratedEvaluation with
                    {
                        SubmittedRequest = third.GeneratedEvaluation.SubmittedRequest with
                        {
                            Html = second.GeneratedEvaluation.SubmittedRequest.Html,
                            Css = second.GeneratedEvaluation.SubmittedRequest.Css
                        }
                    },
                    AuthoringEvidence = third.AuthoringEvidence with
                    {
                        GeneratedBinding = second.AuthoringEvidence.GeneratedBinding
                    }
                }
            ]);
        var duplicateGeneratedRequestPair = third with
        {
            GeneratedCandidateId = second.AuthoringEvidence.GeneratedCandidateId,
            GeneratedEvaluation = third.GeneratedEvaluation with
            {
                SubmittedRequest = third.GeneratedEvaluation.SubmittedRequest with
                {
                    RequestId = second.AuthoringEvidence.GeneratedRequestId
                }
            },
            AuthoringEvidence = third.AuthoringEvidence with
            {
                GeneratedCandidateId = second.AuthoringEvidence.GeneratedCandidateId,
                GeneratedRequestId = second.AuthoringEvidence.GeneratedRequestId,
                GeneratedRef = second.AuthoringEvidence.GeneratedRef,
                Metadata = new Dictionary<string, string>(
                    third.AuthoringEvidence.Metadata,
                    StringComparer.Ordinal)
                {
                    ["teacher_authoring_generated_ref"] =
                        second.AuthoringEvidence.GeneratedRef
                },
                EvidenceRefs = third.AuthoringEvidence.EvidenceRefs
                    .Select(reference => reference.StartsWith(
                        "teacher-authoring:generated_ref=",
                        StringComparison.Ordinal)
                            ? $"teacher-authoring:generated_ref={second.AuthoringEvidence.GeneratedRef}"
                            : reference)
                    .ToArray()
            }
        };
        var duplicateGeneratedRequestSingle = generator
            .BuildRuntimeTeacherAuthoredTrainingCandidates(
                [duplicateGeneratedRequestPair]);
        var duplicateGeneratedRequest = generator
            .BuildRuntimeTeacherAuthoredTrainingCandidates(
            [
                second,
                duplicateGeneratedRequestPair
            ]);
        var nullChosen = generator.BuildRuntimeTeacherAuthoredTrainingCandidates(
            [second with { GeneratedEvaluation = null! }]);
        var nullRejected = generator.BuildRuntimeTeacherAuthoredTrainingCandidates(
            [second with { IncumbentEvaluation = null! }]);
        var firstDuplicateAttemptSingle = generator
            .BuildRuntimeTeacherAuthoredTrainingCandidates([second]);
        var secondDuplicateAttemptSingle = generator
            .BuildRuntimeTeacherAuthoredTrainingCandidates([distinctSecondAttempt]);
        var duplicateAttempt = generator.BuildRuntimeTeacherAuthoredTrainingCandidates(
            [second, distinctSecondAttempt]);

        Assert.True(duplicateGeneratedRequestSingle.IsSuccess,
            duplicateGeneratedRequestSingle.ErrorMessage);
        Assert.Empty(duplicateGeneratedRequestSingle.Value!.AcceptedRecords);
        Assert.Contains(
            "independent_preference_not_proven",
            duplicateGeneratedRequestSingle.Value.Warnings);
        Assert.True(firstDuplicateAttemptSingle.IsSuccess,
            firstDuplicateAttemptSingle.ErrorMessage);
        Assert.Empty(firstDuplicateAttemptSingle.Value!.AcceptedRecords);
        Assert.Contains(
            "independent_preference_not_proven",
            firstDuplicateAttemptSingle.Value.Warnings);
        Assert.True(secondDuplicateAttemptSingle.IsSuccess,
            secondDuplicateAttemptSingle.ErrorMessage);
        Assert.Empty(secondDuplicateAttemptSingle.Value!.AcceptedRecords);
        Assert.Contains(
            "independent_preference_not_proven",
            secondDuplicateAttemptSingle.Value.Warnings);
        AssertStructuralBatchRejected(tampered);
        AssertStructuralBatchRejected(duplicateRequest);
        AssertStructuralBatchRejected(duplicateGeneratedBinding);
        AssertStructuralBatchRejected(duplicateGeneratedRequest);
        AssertStructuralBatchRejected(nullChosen);
        AssertStructuralBatchRejected(nullRejected);
        AssertStructuralBatchRejected(duplicateAttempt);
        Assert.Contains(
            "runtime_teacher_authoring_duplicate_binding_or_request_id",
            duplicateAttempt.Value!.Warnings);
        Assert.Contains(
            "runtime_rejected_independently_evaluated=false",
            duplicateAttempt.Value.Warnings);
        Assert.DoesNotContain(
            "runtime_rejected_independently_evaluated=true",
            duplicateAttempt.Value.Warnings);
        Assert.DoesNotContain(
            "runtime_teacher_authored_candidate=False",
            duplicateAttempt.Value.Warnings);
        Assert.DoesNotContain(
            "runtime_teacher_authored_candidate=True",
            duplicateAttempt.Value.Warnings);
        Assert.DoesNotContain(
            "runtime_teacher_selected_preference=False",
            duplicateAttempt.Value.Warnings);
        Assert.DoesNotContain(
            "runtime_teacher_selected_preference=True",
            duplicateAttempt.Value.Warnings);
    }

    [Fact]
    public async Task IndependentPreferenceReceipt_ValidBoundEvidenceCreatesReviewOnlyRows()
    {
        var pair = await CreateMeasuredPairAsync("independent-receipt-valid", 1);
        var observedAtUtc = DateTimeOffset.UnixEpoch.AddDays(20);
        var artifactRootBinding = DesignConversionDpoGenerator.Sha256Ref(
            "independent-receipt-valid-root");
        var receipt = BuildIndependentPreferenceReceipt(
            pair,
            artifactRootBinding,
            observedAtUtc);

        var build = new DesignConversionDpoGenerator()
            .BuildRuntimeTeacherAuthoredTrainingCandidates(
                [pair],
                [receipt],
                artifactRootBinding,
                observedAtUtc);

        Assert.True(build.IsSuccess, build.ErrorMessage);
        var records = build.Value!.AcceptedRecords.ToArray();
        Assert.Equal(2, records.Length);
        Assert.Equal(
            [
                DesignConversionDpoUnits.CriticUnit,
                DesignConversionDpoUnits.ImproverUnit
            ],
            records.Select(static record => record.UnitId).ToArray());
        Assert.Empty(build.Value.QuarantinedRecords);
        Assert.Contains("independent_preference_proven=true", build.Value.Warnings);
        Assert.Contains(
            "runtime_rejected_independently_evaluated=true",
            build.Value.Warnings);
        Assert.Contains("high_quality_negative_pair_proven", build.Value.Warnings);
        Assert.Contains("training_grade=false", build.Value.Warnings);
        Assert.DoesNotContain("training_grade=true", build.Value.Warnings);
        Assert.All(records, record =>
        {
            Assert.False(record.Provenance.Metadata.ContainsKey(
                "runtime_teacher_selected_preference"));
            Assert.Equal(bool.TrueString,
                record.Provenance.Metadata["independent_preference_proven"]);
            Assert.Equal(bool.FalseString,
                record.Provenance.Metadata["training_grade"]);
            Assert.Equal(receipt.ReceiptSha256,
                record.Provenance.Metadata[
                    "independent_preference_receipt_sha256"]);
            Assert.NotNull(record.PayloadCustodySecret);
        });
    }

    [Fact]
    public async Task IndependentPreferenceReceipt_ExplicitCanonicalUnboundRouteSentinelCreatesDualTargetRecords()
    {
        var pair = WithRouteAdmissionProvenance(
            await CreateMeasuredPairAsync("independent-receipt-unbound-sentinel", 1),
            BuildExplicitUnboundRouteAdmissionSentinel());
        var observedAtUtc = DateTimeOffset.UnixEpoch.AddDays(20);
        var artifactRootBinding = DesignConversionDpoGenerator.Sha256Ref(
            "independent-receipt-unbound-sentinel-root");
        var receipt = BuildIndependentPreferenceReceipt(
            pair,
            artifactRootBinding,
            observedAtUtc);

        var build = new DesignConversionDpoGenerator()
            .BuildRuntimeTeacherAuthoredTrainingCandidates(
                [pair],
                [receipt],
                artifactRootBinding,
                observedAtUtc);

        Assert.True(build.IsSuccess, build.ErrorMessage);
        var records = build.Value!.AcceptedRecords.ToArray();
        Assert.Equal(2, records.Length);
        Assert.Equal(
            [
                DesignConversionDpoUnits.CriticUnit,
                DesignConversionDpoUnits.ImproverUnit
            ],
            records.Select(static record => record.UnitId).ToArray());
        Assert.Empty(build.Value.QuarantinedRecords);
        Assert.Contains("independent_preference_proven=true", build.Value.Warnings);
        Assert.Contains(
            "runtime_rejected_independently_evaluated=true",
            build.Value.Warnings);
        Assert.All(records, record =>
        {
            var metadata = record.Provenance.Metadata;
            Assert.False(metadata.ContainsKey(
                DesignConversionThinClientJobRuntime
                    .ServerOwnedRouteAdmissionTenantBoundMetadataKey));
            Assert.False(metadata.ContainsKey(
                DesignConversionThinClientJobRuntime
                    .ServerOwnedRouteAdmissionTenantIdRefMetadataKey));
            Assert.False(metadata.ContainsKey(
                DesignConversionThinClientJobRuntime
                    .ServerOwnedRouteAdmissionScopeRefMetadataKey));
            Assert.Equal(bool.TrueString, metadata["independent_preference_proven"]);
            Assert.Equal(bool.FalseString, metadata["training_grade"]);
            Assert.Equal(receipt.ReceiptSha256,
                metadata["independent_preference_receipt_sha256"]);
            Assert.NotNull(record.PayloadCustodySecret);
        });
    }

    [Theory]
    [InlineData("partial_bound_only")]
    [InlineData("partial_tenant_scope_without_bound")]
    [InlineData("false_nonempty_tenant")]
    [InlineData("false_nonempty_scope")]
    [InlineData("true_empty_refs")]
    [InlineData("true_malformed_tenant")]
    [InlineData("true_malformed_scope")]
    public async Task IndependentPreferenceReceipt_MalformedExplicitUnboundRouteSentinelFailsClosed(
        string mutation)
    {
        var pair = WithRouteAdmissionProvenance(
            await CreateMeasuredPairAsync(
                "independent-receipt-unbound-malformed-" + mutation,
                1),
            BuildMalformedRouteAdmissionSentinel(mutation));
        var observedAtUtc = DateTimeOffset.UnixEpoch.AddDays(20);
        var artifactRootBinding = DesignConversionDpoGenerator.Sha256Ref(
            "independent-receipt-unbound-malformed-root-" + mutation);
        var receipt = BuildIndependentPreferenceReceipt(
            pair,
            artifactRootBinding,
            observedAtUtc);

        var build = new DesignConversionDpoGenerator()
            .BuildRuntimeTeacherAuthoredTrainingCandidates(
                [pair],
                [receipt],
                artifactRootBinding,
                observedAtUtc);

        AssertStructuralBatchRejected(build);
        Assert.Contains("route_admission_binding_malformed", build.Value!.Warnings);
        Assert.Contains(
            "runtime_created_dpo_candidate_quarantine_reason=route_admission_binding_malformed",
            build.Value.Warnings);
    }

    [Fact]
    public async Task IndependentPreferenceReceipt_IncumbentDirectionControlsChosenPayload()
    {
        var generatedPair = await CreateReceiptDirectionPairAsync(
            "generated-baseline",
            incumbentScore: 0.46,
            incumbentCanPersist: false,
            incumbentPassed: false,
            generatedScore: 0.88,
            generatedCanPersist: true,
            generatedPassed: true);
        var incumbentPair = await CreateReceiptDirectionPairAsync(
            "incumbent-baseline",
            incumbentScore: 0.89,
            incumbentCanPersist: true,
            incumbentPassed: true,
            generatedScore: 0.31,
            generatedCanPersist: false,
            generatedPassed: false);
        Assert.Equal(
            generatedPair.GeneratedEvaluation.SubmittedRequest.Html,
            incumbentPair.GeneratedEvaluation.SubmittedRequest.Html);
        Assert.Equal(
            generatedPair.GeneratedEvaluation.SubmittedRequest.Css,
            incumbentPair.GeneratedEvaluation.SubmittedRequest.Css);
        Assert.Equal(
            generatedPair.IncumbentEvaluation.SubmittedRequest.Html,
            incumbentPair.IncumbentEvaluation.SubmittedRequest.Html);
        Assert.Equal(
            generatedPair.IncumbentEvaluation.SubmittedRequest.Css,
            incumbentPair.IncumbentEvaluation.SubmittedRequest.Css);
        var observedAtUtc = DateTimeOffset.UnixEpoch.AddDays(20);
        var generatedArtifactRootBinding = DesignConversionDpoGenerator.Sha256Ref(
            "independent-receipt-generated-root");
        var incumbentArtifactRootBinding = DesignConversionDpoGenerator.Sha256Ref(
            "independent-receipt-incumbent-root");
        var generatedReceipt = BuildIndependentPreferenceReceipt(
            generatedPair,
            generatedArtifactRootBinding,
            observedAtUtc);
        var incumbentReceipt = BuildIndependentPreferenceReceipt(
            incumbentPair,
            incumbentArtifactRootBinding,
            observedAtUtc,
            DesignConversionIndependentPreferenceEvidenceContract
                .IncumbentSide);

        var generator = new DesignConversionDpoGenerator();
        var generatedBuild = generator
            .BuildRuntimeTeacherAuthoredTrainingCandidates(
                [generatedPair],
                [generatedReceipt],
                generatedArtifactRootBinding,
                observedAtUtc);
        var incumbentBuild = generator
            .BuildRuntimeTeacherAuthoredTrainingCandidates(
                [incumbentPair],
                [incumbentReceipt],
                incumbentArtifactRootBinding,
                observedAtUtc);

        Assert.True(generatedBuild.IsSuccess, generatedBuild.ErrorMessage);
        Assert.True(incumbentBuild.IsSuccess, incumbentBuild.ErrorMessage);
        var generatedByUnit = AssertReceiptDirectionUnitSet(generatedBuild.Value!);
        var incumbentByUnit = AssertReceiptDirectionUnitSet(incumbentBuild.Value!);
        var generatedSourceRequestId = BuildIndependentSha256ShortRef(
            "dcm-source-request",
            generatedPair.GeneratedEvaluation.SubmittedRequest.RequestId);
        var incumbentSourceRequestId = BuildIndependentSha256ShortRef(
            "dcm-source-request",
            incumbentPair.IncumbentEvaluation.SubmittedRequest.RequestId);
        Assert.NotEqual(generatedSourceRequestId, incumbentSourceRequestId);

        foreach (var unitId in new[]
                 {
                     DesignConversionDpoUnits.CriticUnit,
                     DesignConversionDpoUnits.ImproverUnit
                 })
        {
            var generatedRecord = generatedByUnit[unitId];
            var incumbentRecord = incumbentByUnit[unitId];
            Assert.Equal(
                generatedSourceRequestId,
                generatedRecord.Provenance.SourceRequestId);
            Assert.Equal(
                incumbentSourceRequestId,
                incumbentRecord.Provenance.SourceRequestId);
            Assert.NotEqual(
                generatedRecord.Provenance.SourceRequestId,
                incumbentRecord.Provenance.SourceRequestId);

            var generatedSecret = Assert.IsType<
                DesignConversionDpoPayloadCustodySecret>(
                    generatedRecord.PayloadCustodySecret);
            var incumbentSecret = Assert.IsType<
                DesignConversionDpoPayloadCustodySecret>(
                    incumbentRecord.PayloadCustodySecret);
            AssertCustodyResponse(
                generatedSecret.ChosenResponse,
                generatedPair.GeneratedEvaluation.SubmittedRequest.Html,
                generatedPair.GeneratedEvaluation.SubmittedRequest.Css);
            AssertCustodyResponse(
                generatedSecret.RejectedResponse,
                generatedPair.IncumbentEvaluation.SubmittedRequest.Html,
                generatedPair.IncumbentEvaluation.SubmittedRequest.Css);
            AssertCustodyResponse(
                incumbentSecret.ChosenResponse,
                incumbentPair.IncumbentEvaluation.SubmittedRequest.Html,
                incumbentPair.IncumbentEvaluation.SubmittedRequest.Css);
            AssertCustodyResponse(
                incumbentSecret.RejectedResponse,
                incumbentPair.GeneratedEvaluation.SubmittedRequest.Html,
                incumbentPair.GeneratedEvaluation.SubmittedRequest.Css);
            Assert.Equal(
                BuildIndependentSha256Ref(generatedSecret.ChosenResponse),
                generatedSecret.ChosenResponseSha256);
            Assert.Equal(
                BuildIndependentSha256Ref(generatedSecret.RejectedResponse),
                generatedSecret.RejectedResponseSha256);
            Assert.Equal(
                BuildIndependentSha256Ref(incumbentSecret.ChosenResponse),
                incumbentSecret.ChosenResponseSha256);
            Assert.Equal(
                BuildIndependentSha256Ref(incumbentSecret.RejectedResponse),
                incumbentSecret.RejectedResponseSha256);
            Assert.Equal(
                generatedSecret.ChosenResponse,
                incumbentSecret.RejectedResponse);
            Assert.Equal(
                generatedSecret.RejectedResponse,
                incumbentSecret.ChosenResponse);
            Assert.Equal(
                generatedSecret.ChosenResponseSha256,
                incumbentSecret.RejectedResponseSha256);
            Assert.Equal(
                generatedSecret.RejectedResponseSha256,
                incumbentSecret.ChosenResponseSha256);

            Assert.Equal(
                BuildIndependentSha256ShortRef(
                    "dcm-runtime-independent-scorer-chosen-html",
                    generatedPair.GeneratedEvaluation.SubmittedRequest.Html),
                generatedRecord.Chosen.Html);
            Assert.Equal(
                BuildIndependentSha256ShortRef(
                    "dcm-runtime-independent-scorer-chosen-css",
                    generatedPair.GeneratedEvaluation.SubmittedRequest.Css),
                generatedRecord.Chosen.Css);
            Assert.Equal(
                BuildIndependentSha256ShortRef(
                    "dcm-runtime-independent-scorer-rejected-html",
                    generatedPair.IncumbentEvaluation.SubmittedRequest.Html),
                generatedRecord.Rejected.Html);
            Assert.Equal(
                BuildIndependentSha256ShortRef(
                    "dcm-runtime-independent-scorer-rejected-css",
                    generatedPair.IncumbentEvaluation.SubmittedRequest.Css),
                generatedRecord.Rejected.Css);
            Assert.Equal(
                BuildIndependentSha256ShortRef(
                    "dcm-runtime-independent-scorer-chosen-html",
                    incumbentPair.IncumbentEvaluation.SubmittedRequest.Html),
                incumbentRecord.Chosen.Html);
            Assert.Equal(
                BuildIndependentSha256ShortRef(
                    "dcm-runtime-independent-scorer-chosen-css",
                    incumbentPair.IncumbentEvaluation.SubmittedRequest.Css),
                incumbentRecord.Chosen.Css);
            Assert.Equal(
                BuildIndependentSha256ShortRef(
                    "dcm-runtime-independent-scorer-rejected-html",
                    incumbentPair.GeneratedEvaluation.SubmittedRequest.Html),
                incumbentRecord.Rejected.Html);
            Assert.Equal(
                BuildIndependentSha256ShortRef(
                    "dcm-runtime-independent-scorer-rejected-css",
                    incumbentPair.GeneratedEvaluation.SubmittedRequest.Css),
                incumbentRecord.Rejected.Css);
        }
    }

    private static IReadOnlyDictionary<string, DesignConversionDpoTrainingRecord>
        AssertReceiptDirectionUnitSet(DesignConversionDpoBuildResult build)
    {
        Assert.Empty(build.QuarantinedRecords);
        Assert.Equal(2, build.AcceptedRecords.Count);
        var byUnit = build.AcceptedRecords.ToDictionary(
            record => record.UnitId,
            StringComparer.Ordinal);
        Assert.Equal(
            new[]
            {
                DesignConversionDpoUnits.CriticUnit,
                DesignConversionDpoUnits.ImproverUnit
            },
            byUnit.Keys.Order(StringComparer.Ordinal));
        return byUnit;
    }

    private static void AssertCustodyResponse(
        string response,
        string expectedHtml,
        string expectedCss)
    {
        using var document = System.Text.Json.JsonDocument.Parse(response);
        Assert.Equal(
            expectedHtml,
            document.RootElement.GetProperty("html").GetString());
        Assert.Equal(
            expectedCss,
            document.RootElement.GetProperty("css").GetString());
    }

    private static string BuildIndependentSha256Ref(string value)
        => "sha256:" + Convert.ToHexString(
            System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    private static string BuildIndependentSha256ShortRef(
        string prefix,
        string value)
        => prefix + "-" + Convert.ToHexString(
            System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(value))).ToLowerInvariant()[..16];

    [Theory]
    [InlineData("attempt")]
    [InlineData("generated_ref")]
    [InlineData("incumbent_ref")]
    [InlineData("generated_binding")]
    [InlineData("incumbent_binding")]
    [InlineData("generated_evaluation_identity")]
    [InlineData("incumbent_evaluation_identity")]
    [InlineData("direction")]
    [InlineData("scorer_request_hash")]
    [InlineData("scorer_output_hash")]
    [InlineData("profile_id")]
    [InlineData("profile_version")]
    [InlineData("profile_configuration_hash")]
    [InlineData("model_hash")]
    [InlineData("artifact_root")]
    [InlineData("provenance_kind")]
    [InlineData("scorer_id")]
    [InlineData("scorer_version")]
    [InlineData("scorer_request_id")]
    [InlineData("scorer_output_id")]
    [InlineData("scorer_runtime")]
    [InlineData("issued_future")]
    [InlineData("expired")]
    [InlineData("replay_nonce")]
    [InlineData("receipt_hash")]
    public async Task IndependentPreferenceReceipt_TamperFailsClosed(
        string tamper)
    {
        var pair = await CreateMeasuredPairAsync(
            "independent-receipt-tamper-" + tamper,
            1);
        var observedAtUtc = DateTimeOffset.UnixEpoch.AddDays(21);
        var artifactRootBinding = DesignConversionDpoGenerator.Sha256Ref(
            "independent-receipt-tamper-root");
        var receipt = BuildIndependentPreferenceReceipt(
            pair,
            artifactRootBinding,
            observedAtUtc);
        var tampered = tamper switch
        {
            "attempt" => receipt with
            {
                AttemptNumber = receipt.AttemptNumber + 1
            },
            "generated_ref" => receipt with { GeneratedRef = "tampered" },
            "incumbent_ref" => receipt with { IncumbentRef = "tampered" },
            "generated_binding" => receipt with
            {
                GeneratedBinding = "tampered"
            },
            "incumbent_binding" => receipt with
            {
                IncumbentBinding = "tampered"
            },
            "generated_evaluation_identity" => receipt with
            {
                GeneratedEvaluationIdentity =
                    DesignConversionDpoGenerator.Sha256Ref("tampered-generated")
            },
            "incumbent_evaluation_identity" => receipt with
            {
                IncumbentEvaluationIdentity =
                    DesignConversionDpoGenerator.Sha256Ref("tampered-incumbent")
            },
            "direction" => receipt with { PreferredDirection = "authoring" },
            "scorer_request_hash" => receipt with
            {
                ScorerRequestSha256 =
                    DesignConversionDpoGenerator.Sha256Ref("tampered-request")
            },
            "scorer_output_hash" => receipt with
            {
                ScorerOutputSha256 =
                    DesignConversionDpoGenerator.Sha256Ref("tampered-output")
            },
            "profile_id" => receipt with { EvaluationProfileId = "tampered" },
            "profile_version" => receipt with
            {
                EvaluationProfileVersion = "tampered"
            },
            "profile_configuration_hash" => receipt with
            {
                EvaluationProfileConfigurationSha256 =
                    DesignConversionDpoGenerator.Sha256Ref("tampered-profile")
            },
            "model_hash" => receipt with
            {
                EvaluationModelSha256 =
                    DesignConversionDpoGenerator.Sha256Ref("tampered-model")
            },
            "artifact_root" => receipt with
            {
                ArtifactRootBinding =
                    DesignConversionDpoGenerator.Sha256Ref("tampered-root")
            },
            "provenance_kind" => receipt with
            {
                ScorerProvenance = receipt.ScorerProvenance with
                {
                    Kind = "candidate_authoring"
                }
            },
            "scorer_id" => receipt with
            {
                ScorerProvenance = receipt.ScorerProvenance with
                {
                    ScorerId = pair.AuthoringEvidence.Provenance.ModelId
                }
            },
            "scorer_version" => receipt with
            {
                ScorerProvenance = receipt.ScorerProvenance with
                {
                    ScorerVersion = string.Empty
                }
            },
            "scorer_request_id" => receipt with
            {
                ScorerProvenance = receipt.ScorerProvenance with
                {
                    RequestId = pair.AuthoringEvidence.Provenance.RequestId
                }
            },
            "scorer_output_id" => receipt with
            {
                ScorerProvenance = receipt.ScorerProvenance with
                {
                    OutputId = pair.AuthoringEvidence.OutputHash
                }
            },
            "scorer_runtime" => receipt with
            {
                ScorerProvenance = receipt.ScorerProvenance with
                {
                    RuntimeInstanceRef = string.Empty
                }
            },
            "issued_future" => receipt with
            {
                IssuedAtUtc = observedAtUtc.AddSeconds(1)
            },
            "expired" => receipt with
            {
                ExpiresAtUtc = observedAtUtc
            },
            "replay_nonce" => receipt with { ReplayNonce = string.Empty },
            "receipt_hash" => receipt with
            {
                ReceiptSha256 =
                    DesignConversionDpoGenerator.Sha256Ref("tampered-receipt")
            },
            _ => throw new ArgumentOutOfRangeException(nameof(tamper))
        };
        if (tamper is "provenance_kind" or "scorer_id" or
            "scorer_version" or "scorer_request_id" or "scorer_runtime")
        {
            tampered = tampered with
            {
                ScorerRequestSha256 = DesignConversionDpoGenerator
                    .BuildIndependentPreferenceScorerRequestSha256(
                        pair,
                        artifactRootBinding,
                        tampered.ScorerProvenance)
            };
        }
        if (tamper is not "receipt_hash" and not "scorer_output_hash")
        {
            tampered = tampered with
            {
                ReceiptSha256 = DesignConversionDpoGenerator
                    .BuildIndependentPreferenceReceiptSha256(tampered)
            };
        }

        var build = new DesignConversionDpoGenerator()
            .BuildRuntimeTeacherAuthoredTrainingCandidates(
                [pair],
                [tampered],
                artifactRootBinding,
                observedAtUtc);

        AssertStructuralBatchRejected(build);
        Assert.Contains("independent_preference_not_proven", build.Value!.Warnings);
        Assert.Contains(
            "runtime_rejected_independently_evaluated=false",
            build.Value.Warnings);
        Assert.Contains("training_grade=false", build.Value.Warnings);
        Assert.DoesNotContain("training_grade=true", build.Value.Warnings);
        Assert.DoesNotContain(
            "high_quality_negative_pair_proven",
            build.Value.Warnings);
    }

    [Fact]
    public async Task IndependentPreferenceEvidenceAssociation_ConsumesRootAttemptOnce()
    {
        var pair = await CreateMeasuredPairAsync(
            "independent-receipt-replay",
            1);
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "independent-preference-replay"));
        using var association =
            DesignConversionIndependentPreferenceEvidenceAssociation
                .AssociateArtifactDbForTesting(
                    artifactDb,
                    new ReplayReceiptSource());
        var registration = Assert.IsType<
            DesignConversionIndependentPreferenceEvidenceAssociation
                .EvidenceSourceRegistration>(
                    DesignConversionIndependentPreferenceEvidenceAssociation
                        .Resolve(artifactDb));
        var observedAtUtc = DateTimeOffset.UnixEpoch.AddDays(22);

        Assert.True(registration.TryConsumeEvidence(
            pair,
            observedAtUtc,
            out var first));
        Assert.False(registration.TryConsumeEvidence(
            pair,
            observedAtUtc,
            out _));
        Assert.Equal(registration.ArtifactRootBinding,
            first.ArtifactRootBinding);
    }

    [Fact]
    public async Task TeacherAuthoredGenerator_RejectsWellFormedAlternateScalarOutputHash()
    {
        var incumbent = CreateEnvelopeTestIncumbent("scalar-output-hash");
        var authored = await AuthorAsync(
            incumbent,
            "generated");
        var generated = AsGeneratedEvaluation(
            authored.Value!,
            score: 0.88,
            passed: true,
            canPersist: true);
        var canonical = authored.Value!.AuthoringEvidence!;
        var alternateHash = string.Equals(
            canonical.OutputHash,
            new string('a', 64),
            StringComparison.Ordinal)
                ? new string('b', 64)
                : new string('a', 64);
        var tampered = canonical with
        {
            OutputHash = alternateHash,
            Metadata = new Dictionary<string, string>(
                canonical.Metadata,
                StringComparer.Ordinal),
            EvidenceRefs = canonical.EvidenceRefs.ToArray()
        };

        AssertTeacherAuthoredPairRejected(generated, incumbent, tampered);
    }

    [Fact]
    public async Task TeacherAuthoredGenerator_RejectsTamperedInferenceProfileFields()
    {
        var incumbent = CreateEnvelopeTestIncumbent("profile-field-tamper");
        var authored = await AuthorAsync(
            incumbent,
            "generated");
        var generated = AsGeneratedEvaluation(
            authored.Value!,
            score: 0.88,
            passed: true,
            canPersist: true);
        var canonical = authored.Value!.AuthoringEvidence!;

        AssertTeacherAuthoredPairRejected(
            generated,
            incumbent,
            canonical with { InferenceProfileId = "unknown-authoring-profile" });
        AssertTeacherAuthoredPairRejected(
            generated,
            incumbent,
            canonical with
            {
                InferenceProfileConfigurationSha256 = new string('a', 64)
            });
    }

    [Fact]
    public async Task TeacherAuthoredGenerator_RejectsEveryRequiredMetadataValueMutationAndRemoval()
    {
        var incumbent = CreateEnvelopeTestIncumbent("metadata-envelope");
        var authored = await AuthorAsync(
            incumbent,
            "generated");
        var generated = AsGeneratedEvaluation(
            authored.Value!,
            score: 0.88,
            passed: true,
            canPersist: true);
        var canonical = authored.Value!.AuthoringEvidence!;

        foreach (var key in canonical.Metadata.Keys)
        {
            var valueMutation = new Dictionary<string, string>(
                canonical.Metadata,
                StringComparer.Ordinal)
            {
                [key] = canonical.Metadata[key] + "-tampered"
            };
            Assert.NotSame(canonical.Metadata, valueMutation);
            AssertTeacherAuthoredPairRejected(
                generated,
                incumbent,
                canonical with
                {
                    Metadata = valueMutation,
                    EvidenceRefs = canonical.EvidenceRefs.ToArray()
                });

            var removal = new Dictionary<string, string>(
                canonical.Metadata,
                StringComparer.Ordinal);
            Assert.True(removal.Remove(key));
            Assert.NotSame(canonical.Metadata, removal);
            AssertTeacherAuthoredPairRejected(
                generated,
                incumbent,
                canonical with
                {
                    Metadata = removal,
                    EvidenceRefs = canonical.EvidenceRefs.ToArray()
                });
        }
    }

    [Fact]
    public async Task TeacherAuthoredGenerator_RejectsEveryRequiredEvidenceBindingMutationAndRemoval()
    {
        var incumbent = CreateEnvelopeTestIncumbent("evidence-envelope");
        var authored = await AuthorAsync(
            incumbent,
            "generated");
        var generated = AsGeneratedEvaluation(
            authored.Value!,
            score: 0.88,
            passed: true,
            canPersist: true);
        var canonical = authored.Value!.AuthoringEvidence!;

        for (var index = 0; index < canonical.EvidenceRefs.Count; index++)
        {
            var valueMutation = canonical.EvidenceRefs.ToArray();
            valueMutation[index] += "-tampered";
            Assert.NotSame(canonical.EvidenceRefs, valueMutation);
            AssertTeacherAuthoredPairRejected(
                generated,
                incumbent,
                canonical with
                {
                    Metadata = new Dictionary<string, string>(
                        canonical.Metadata,
                        StringComparer.Ordinal),
                    EvidenceRefs = valueMutation
                });

            var removal = canonical.EvidenceRefs
                .Where((_, evidenceIndex) => evidenceIndex != index)
                .ToArray();
            Assert.NotSame(canonical.EvidenceRefs, removal);
            AssertTeacherAuthoredPairRejected(
                generated,
                incumbent,
                canonical with
                {
                    Metadata = new Dictionary<string, string>(
                        canonical.Metadata,
                        StringComparer.Ordinal),
                    EvidenceRefs = removal
                });
        }
    }

    [Fact]
    public async Task TeacherAuthoredGenerator_FailClosedWarningsDoNotLeakPrivateQaPaths()
    {
        const string privateBoundaryPath =
            "C:/Users/lubao/private-qa/raw-capture.png";
        var boundaryWarnings = new[]
        {
            "DETERMINISTIC_RENDER_SCAFFOLD_DOES_NOT_CLAIM_VISUAL_FIDELITY",
            $"BROWSER_RENDER_PROOF_RECEIPT_REQUIRED raw_probe={privateBoundaryPath}",
            "DETERMINISTIC_RENDER_SCAFFOLD_NOT_BROWSER_SCREENSHOT",
            "SERVER_OWNED_SCAFFOLD_RENDER_PROVENANCE_CANONICALIZED",
            "SERVER_OWNED_BROWSER_RENDER_PROVENANCE_CANONICALIZED",
            "BROWSER_RENDER_PROOF_RECEIPT_REQUIRED",
            $"PRIVATE_CHROMIUM_BROWSER_CAPTURE raw_probe={privateBoundaryPath}",
            $"BROWSER_CAPTURE_RESOLUTION_REQUIRED_PRIVATE_SUFFIX raw_probe={privateBoundaryPath}",
            $"SERVER_OWNED_BROWSER_RENDER_PROVENANCE_CANONICALIZED_EXTRA raw_probe={privateBoundaryPath}",
            "browser_render_proof_receipt_required",
            "unrelated_private_warning C:/Users/lubao/private-qa/not-boundary.txt",
            "DETERMINISTIC_RENDER_SCAFFOLD_NOT_BROWSER_SCREENSHOT"
        };
        var incumbent = DesignConversionDpoGeneratorTests.CreateRuntimeEvaluation(
            canPersistTrainingCandidate: false,
            jobId: "warning-incumbent-job",
            requestId: "warning-incumbent-request",
            outputId: "warning-incumbent-output",
            candidateId: "warning-incumbent-candidate",
            verdictId: "warning-incumbent-verdict",
            score: 0.46,
            passed: false,
            htmlSuffix: "warning-incumbent",
            warnings: boundaryWarnings);
        var authored = await AuthorAsync(
            incumbent,
            "generated");
        Assert.True(authored.IsSuccess, authored.ErrorMessage);
        var preference = Assert.IsType<
            DesignConversionDpoTeacherCandidateAuthoringEvidence>(
                authored.Value!.AuthoringEvidence);
        var generated = AsGeneratedEvaluation(
            authored.Value,
            score: 0.88,
            passed: true,
            canPersist: true,
            warnings: boundaryWarnings);

        var result = new DesignConversionDpoGenerator()
            .BuildRuntimeTeacherAuthoredTrainingCandidates(
                generated,
                incumbent,
                preference);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Empty(result.Value!.AcceptedRecords);
        Assert.Empty(result.Value.QuarantinedRecords);
        Assert.Contains("independent_preference_not_proven", result.Value.Warnings);
        Assert.Contains(
            "runtime_rejected_independently_evaluated=false",
            result.Value.Warnings);
        var persistedBoundary = System.Text.Json.JsonSerializer.Serialize(new
        {
            authored.Value.Candidate,
            AuthoringEvidence = preference,
            result.Value.Warnings
        });
        Assert.DoesNotContain(
            privateBoundaryPath,
            persistedBoundary,
            StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(
            "raw_probe=",
            persistedBoundary,
            StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(
            "C:/Users/lubao",
            persistedBoundary,
            StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(
            "private-qa",
            persistedBoundary,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AttestationOnlyGeneratorOverload_IsNonAuthoritative()
    {
        var chosen = DesignConversionDpoGeneratorTests.CreateRuntimeEvaluation(
            canPersistTrainingCandidate: true);
        var rejected = DesignConversionDpoGeneratorTests.CreateRuntimeEvaluation(
            canPersistTrainingCandidate: false,
            jobId: "legacy-rejected-job",
            requestId: "legacy-rejected-request",
            outputId: "legacy-rejected-output",
            candidateId: "legacy-rejected-candidate",
            verdictId: "legacy-rejected-verdict",
            score: 0.4,
            passed: false,
            htmlSuffix: "legacy-rejected");
        using var client = new AuthoringTeacherClient(prompt =>
        {
            using var document = System.Text.Json.JsonDocument.Parse(prompt);
            var root = document.RootElement;
            var chosenRef = root.GetProperty("chosen").GetProperty("ref").GetString()!;
            var rejectedRef = root.GetProperty("rejected").GetProperty("ref").GetString()!;
            var evidenceRef = root.GetProperty("evidence_refs")[0].GetString()!;
            return System.Text.Json.JsonSerializer.Serialize(new
            {
                schema = DesignConversionDpoTeacherAttestationRuntime.Schema,
                chosen_ref = chosenRef,
                rejected_ref = rejectedRef,
                preferred_ref = chosenRef,
                supports_preference = true,
                evidence_refs = new[] { evidenceRef }
            });
        });
        using var runtime = CreateRuntime(client);
        var attestation = await runtime.AttestAsync(
            chosen,
            rejected,
            CreateReservation());

        var build = new DesignConversionDpoGenerator()
            .BuildRuntimeCreatedTrainingCandidates(
                chosen,
                rejected,
                attestation.Value!);

        Assert.True(build.IsSuccess, build.ErrorMessage);
        Assert.Empty(build.Value!.AcceptedRecords);
        Assert.Contains(
            "runtime_teacher_attestation_only_is_non_authoritative",
            build.Value.Warnings);
    }

    private static DesignConversionInternalEvaluationResult
        CreateEnvelopeTestIncumbent(string suffix)
        => DesignConversionDpoGeneratorTests.CreateRuntimeEvaluation(
            canPersistTrainingCandidate: false,
            jobId: $"envelope-{suffix}-job",
            requestId: $"envelope-{suffix}-request",
            outputId: $"envelope-{suffix}-output",
            candidateId: $"envelope-{suffix}-candidate",
            verdictId: $"envelope-{suffix}-verdict",
            score: 0.46,
            passed: false,
            htmlSuffix: suffix);

    private static void AssertTeacherAuthoredPairRejected(
        DesignConversionInternalEvaluationResult generated,
        DesignConversionInternalEvaluationResult incumbent,
        DesignConversionDpoTeacherCandidateAuthoringEvidence preference)
    {
        var build = new DesignConversionDpoGenerator()
            .BuildRuntimeTeacherAuthoredTrainingCandidates(
                generated,
                incumbent,
                preference);

        Assert.True(build.IsSuccess, build.ErrorMessage);
        Assert.Empty(build.Value!.AcceptedRecords);
        Assert.Empty(build.Value.QuarantinedRecords);
    }

    private static DesignConversionIndependentPreferenceEvidence
        BuildIndependentPreferenceReceipt(
            DesignConversionTeacherAuthoredEvaluationPair pair,
            string artifactRootBinding,
            DateTimeOffset observedAtUtc,
            string preferredDirection =
                DesignConversionIndependentPreferenceEvidenceContract.GeneratedSide)
        => DesignConversionIndependentPreferenceEvidenceTestIssuer.Issue(
            pair,
            artifactRootBinding,
            observedAtUtc,
            preferredDirection);

    private static IReadOnlyDictionary<string, string>
        BuildExplicitUnboundRouteAdmissionSentinel()
        => new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [DesignConversionThinClientJobRuntime
                .ServerOwnedRouteAdmissionTenantBoundMetadataKey] =
                bool.FalseString,
            [DesignConversionThinClientJobRuntime
                .ServerOwnedRouteAdmissionTenantIdRefMetadataKey] =
                string.Empty,
            [DesignConversionThinClientJobRuntime
                .ServerOwnedRouteAdmissionScopeRefMetadataKey] =
                string.Empty
        };

    private static IReadOnlyDictionary<string, string>
        BuildMalformedRouteAdmissionSentinel(string mutation)
    {
        var tenantIdRef = DesignConversionModelActivationRegistry
            .BuildTenantIdRef("w053-dpo-teacher-authoring-tenant");
        var scopeRef = BuildRouteAdmissionScopeRefForTest(
            "w053-dpo-teacher-authoring-scope");
        return mutation switch
        {
            "partial_bound_only" => new Dictionary<string, string>(
                StringComparer.Ordinal)
            {
                [DesignConversionThinClientJobRuntime
                    .ServerOwnedRouteAdmissionTenantBoundMetadataKey] =
                    bool.FalseString
            },
            "partial_tenant_scope_without_bound" => new Dictionary<string, string>(
                StringComparer.Ordinal)
            {
                [DesignConversionThinClientJobRuntime
                    .ServerOwnedRouteAdmissionTenantIdRefMetadataKey] =
                    string.Empty,
                [DesignConversionThinClientJobRuntime
                    .ServerOwnedRouteAdmissionScopeRefMetadataKey] =
                    string.Empty
            },
            "false_nonempty_tenant" => new Dictionary<string, string>(
                StringComparer.Ordinal)
            {
                [DesignConversionThinClientJobRuntime
                    .ServerOwnedRouteAdmissionTenantBoundMetadataKey] =
                    bool.FalseString,
                [DesignConversionThinClientJobRuntime
                    .ServerOwnedRouteAdmissionTenantIdRefMetadataKey] =
                    tenantIdRef,
                [DesignConversionThinClientJobRuntime
                    .ServerOwnedRouteAdmissionScopeRefMetadataKey] =
                    string.Empty
            },
            "false_nonempty_scope" => new Dictionary<string, string>(
                StringComparer.Ordinal)
            {
                [DesignConversionThinClientJobRuntime
                    .ServerOwnedRouteAdmissionTenantBoundMetadataKey] =
                    bool.FalseString,
                [DesignConversionThinClientJobRuntime
                    .ServerOwnedRouteAdmissionTenantIdRefMetadataKey] =
                    string.Empty,
                [DesignConversionThinClientJobRuntime
                    .ServerOwnedRouteAdmissionScopeRefMetadataKey] =
                    scopeRef
            },
            "true_empty_refs" => new Dictionary<string, string>(
                StringComparer.Ordinal)
            {
                [DesignConversionThinClientJobRuntime
                    .ServerOwnedRouteAdmissionTenantBoundMetadataKey] =
                    bool.TrueString,
                [DesignConversionThinClientJobRuntime
                    .ServerOwnedRouteAdmissionTenantIdRefMetadataKey] =
                    string.Empty,
                [DesignConversionThinClientJobRuntime
                    .ServerOwnedRouteAdmissionScopeRefMetadataKey] =
                    string.Empty
            },
            "true_malformed_tenant" => new Dictionary<string, string>(
                StringComparer.Ordinal)
            {
                [DesignConversionThinClientJobRuntime
                    .ServerOwnedRouteAdmissionTenantBoundMetadataKey] =
                    bool.TrueString,
                [DesignConversionThinClientJobRuntime
                    .ServerOwnedRouteAdmissionTenantIdRefMetadataKey] =
                    "sha256:design-conversion-model-activation-tenant:not64",
                [DesignConversionThinClientJobRuntime
                    .ServerOwnedRouteAdmissionScopeRefMetadataKey] =
                    scopeRef
            },
            "true_malformed_scope" => new Dictionary<string, string>(
                StringComparer.Ordinal)
            {
                [DesignConversionThinClientJobRuntime
                    .ServerOwnedRouteAdmissionTenantBoundMetadataKey] =
                    bool.TrueString,
                [DesignConversionThinClientJobRuntime
                    .ServerOwnedRouteAdmissionTenantIdRefMetadataKey] =
                    tenantIdRef,
                [DesignConversionThinClientJobRuntime
                    .ServerOwnedRouteAdmissionScopeRefMetadataKey] =
                    "sha256:design-conversion-route-admission-scope:not64"
            },
            _ => throw new ArgumentOutOfRangeException(nameof(mutation), mutation, null)
        };
    }

    private static DesignConversionTeacherAuthoredEvaluationPair
        WithRouteAdmissionProvenance(
            DesignConversionTeacherAuthoredEvaluationPair pair,
            IReadOnlyDictionary<string, string> routeAdmission)
        => pair with
        {
            GeneratedEvaluation = WithRouteAdmissionProvenance(
                pair.GeneratedEvaluation,
                routeAdmission),
            IncumbentEvaluation = WithRouteAdmissionProvenance(
                pair.IncumbentEvaluation,
                routeAdmission)
        };

    private static DesignConversionInternalEvaluationResult
        WithRouteAdmissionProvenance(
            DesignConversionInternalEvaluationResult evaluation,
            IReadOnlyDictionary<string, string> routeAdmission)
    {
        var provenance = new Dictionary<string, string>(
            evaluation.SubmittedRequest.Provenance,
            StringComparer.Ordinal);
        foreach (var item in routeAdmission)
            provenance[item.Key] = item.Value;
        var submitted = evaluation.SubmittedRequest with
        {
            Provenance = provenance
        };
        return evaluation with
        {
            SubmittedRequest = submitted,
            RuntimeFlowResult = evaluation.RuntimeFlowResult is null
                ? null
                : evaluation.RuntimeFlowResult with
                {
                    Request = submitted
                }
        };
    }

    private static string BuildRouteAdmissionScopeRefForTest(string source)
        => "sha256:design-conversion-route-admission-scope:" +
           Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
                   System.Text.Encoding.UTF8.GetBytes(source)))
               .ToLowerInvariant();

    private static async Task<AgentInfrastructure.OperationResult<
        DesignConversionTeacherAuthoredCandidateResult>> AuthorAsync(
        DesignConversionInternalEvaluationResult incumbent,
            string obsoleteDirectionFixture = "generated")
    {
        using var client = new AuthoringTeacherClient(prompt =>
            BuildOutput(prompt, obsoleteDirectionFixture));
        using var runtime = CreateRuntime(client);
        return await runtime.AuthorCandidateAsync(
            CreateUpdateRequest(incumbent),
            CreateReservation());
    }

    private static async Task<DesignConversionTeacherAuthoredEvaluationPair>
        CreateMeasuredPairAsync(
            string suffix,
            int completedAttemptNumber,
            string candidateFixture = "generated")
    {
        var incumbent = CreateEnvelopeTestIncumbent(suffix);
        using var client = new AuthoringTeacherClient(
            prompt => BuildOutput(
                prompt,
                candidateFixture,
                candidateHtml: $"<main data-teacher=\"{suffix}\">Candidate</main>",
                candidateCss: $"[data-teacher=\"{suffix}\"]{{display:grid;gap:18px;}}"),
            () => ValidProvenance($"teacher-authoring-{suffix}"));
        using var runtime = CreateRuntime(client);
        var authored = await runtime.AuthorCandidateAsync(
            CreateUpdateRequest(incumbent, completedAttemptNumber),
            CreateReservation());
        Assert.True(authored.IsSuccess, authored.ErrorMessage);
        var generated = AsGeneratedEvaluation(
            authored.Value!,
            score: 0.88,
            passed: true,
            canPersist: true) with
        {
            JobId = $"teacher-generated-{suffix}-job"
        };
        return new DesignConversionTeacherAuthoredEvaluationPair(
            completedAttemptNumber + 1,
            generated,
            incumbent,
            authored.Value!.AuthoringEvidence!,
            authored.Value.Candidate.CandidateId);
    }

    private static async Task<DesignConversionTeacherAuthoredEvaluationPair>
        CreateReceiptDirectionPairAsync(
            string suffix,
            double incumbentScore,
            bool incumbentCanPersist,
            bool incumbentPassed,
            double generatedScore,
            bool generatedCanPersist,
            bool generatedPassed)
    {
        const string sharedIncumbentSuffix =
            "independent-receipt-direction-incumbent";
        const string sharedGeneratedHtml =
            "<main data-teacher=\"independent-receipt-direction\">Candidate</main>";
        const string sharedGeneratedCss =
            "[data-teacher=\"independent-receipt-direction\"]{display:grid;gap:18px;}";
        var incumbent = DesignConversionDpoGeneratorTests.CreateRuntimeEvaluation(
            canPersistTrainingCandidate: incumbentCanPersist,
            jobId: $"receipt-direction-{suffix}-incumbent-job",
            requestId: $"receipt-direction-{suffix}-incumbent-request",
            outputId: $"receipt-direction-{suffix}-incumbent-output",
            candidateId: $"receipt-direction-{suffix}-incumbent-candidate",
            verdictId: $"receipt-direction-{suffix}-incumbent-verdict",
            score: incumbentScore,
            passed: incumbentPassed,
            htmlSuffix: sharedIncumbentSuffix);
        using var client = new AuthoringTeacherClient(
            prompt => BuildOutput(
                prompt,
                candidateHtml: sharedGeneratedHtml,
                candidateCss: sharedGeneratedCss),
            () => ValidProvenance($"receipt-direction-{suffix}-teacher"));
        using var runtime = CreateRuntime(client);
        var authored = await runtime.AuthorCandidateAsync(
            CreateUpdateRequest(incumbent),
            CreateReservation());
        Assert.True(authored.IsSuccess, authored.ErrorMessage);
        var generated = AsGeneratedEvaluation(
            authored.Value!,
            score: generatedScore,
            passed: generatedPassed,
            canPersist: generatedCanPersist) with
        {
            JobId = $"receipt-direction-{suffix}-generated-job"
        };
        return new DesignConversionTeacherAuthoredEvaluationPair(
            2,
            generated,
            incumbent,
            authored.Value!.AuthoringEvidence!,
            authored.Value.Candidate.CandidateId);
    }

    private static DesignConversionTeacherAuthoredEvaluationPair
        WithStaleRuntimeFlowScore(
            DesignConversionTeacherAuthoredEvaluationPair pair,
            double chosenScore,
            double rejectedScore)
        => pair with
        {
            GeneratedEvaluation = pair.GeneratedEvaluation with
            {
                RuntimeFlowResult = pair.GeneratedEvaluation.RuntimeFlowResult with
                {
                    Output = pair.GeneratedEvaluation.RuntimeFlowResult.Output with
                    {
                        Verdict = pair.GeneratedEvaluation.RuntimeFlowResult.Output.Verdict with
                        {
                            Score = chosenScore
                        }
                    }
                }
            },
            IncumbentEvaluation = pair.IncumbentEvaluation with
            {
                RuntimeFlowResult = pair.IncumbentEvaluation.RuntimeFlowResult with
                {
                    Output = pair.IncumbentEvaluation.RuntimeFlowResult.Output with
                    {
                        Verdict = pair.IncumbentEvaluation.RuntimeFlowResult.Output.Verdict with
                        {
                            Score = rejectedScore
                        }
                    }
                }
            }
        };

    private static void AssertStructuralBatchRejected(
        AgentInfrastructure.OperationResult<DesignConversionDpoBuildResult> build)
    {
        Assert.True(build.IsSuccess, build.ErrorMessage);
        Assert.Empty(build.Value!.AcceptedRecords);
        Assert.Empty(build.Value.QuarantinedRecords);
        Assert.Contains("runtime_created_dpo_candidate=false", build.Value.Warnings);
        Assert.Contains(
            "chosen_rejected_pair_or_negative_quality_not_proven",
            build.Value.Warnings);
        Assert.Contains("high_quality_negative_pair_not_proven", build.Value.Warnings);
        Assert.DoesNotContain("chosen_rejected_pair_proven", build.Value.Warnings);
        Assert.DoesNotContain("high_quality_negative_pair_proven", build.Value.Warnings);
        Assert.DoesNotContain(
            "runtime_rejected_independently_evaluated=true",
            build.Value.Warnings);
    }

    private static DesignConversionInternalEvaluationResult AsGeneratedEvaluation(
        DesignConversionTeacherAuthoredCandidateResult authored,
        double score,
        bool passed,
        bool canPersist,
        IReadOnlyList<string>? warnings = null)
    {
        var generated = DesignConversionDpoGeneratorTests.CreateRuntimeEvaluation(
            canPersistTrainingCandidate: canPersist,
            jobId: "teacher-generated-job",
            requestId: authored.Candidate.RequestId,
            outputId: "teacher-generated-output",
            candidateId: authored.Candidate.CandidateId,
            verdictId: "teacher-generated-verdict",
            score: score,
            passed: passed,
            htmlSuffix: "teacher-generated",
            warnings: warnings);
        var submitted = generated.SubmittedRequest with
        {
            RequestId = authored.Candidate.RequestId,
            Html = authored.Candidate.Html,
            Css = authored.Candidate.Css
        };
        return generated with
        {
            SubmittedRequest = submitted,
            RuntimeFlowResult = generated.RuntimeFlowResult with
            {
                Request = submitted,
                Candidate = authored.Candidate,
                Output = generated.RuntimeFlowResult.Output with
                {
                    CandidateId = authored.Candidate.CandidateId
                }
            }
        };
    }

    private static DesignConversionCandidateUpdateRequest CreateUpdateRequest(
        DesignConversionInternalEvaluationResult incumbent,
        int completedAttemptNumber = 1)
    {
        var submitted = incumbent.SubmittedRequest;
        return new DesignConversionCandidateUpdateRequest(
            LoopId: "teacher-authoring-loop",
            CompletedAttemptNumber: completedAttemptNumber,
            CurrentRequest: new DesignConversionInternalEvaluateRequest(
                incumbent.JobId,
                submitted.RequestId,
                submitted.SourceKind,
                submitted.Html,
                submitted.Css,
                submitted.DesignTreePayload,
                submitted.OriginalTreePayload,
                submitted.Screenshots,
                submitted.DesignRenders,
                submitted.Budget,
                RenderResolutions: [],
                AiTeacherEvidenceRefs: [],
                DeterministicEvidenceRefs: new Dictionary<string, string>(),
                submitted.Provenance),
            CurrentEvaluation: incumbent,
            TierPolicy: incumbent.VisualQaBoundaryTrace.TierPolicy,
            AttemptHistory: []);
    }

    private static string BuildOutput(
        string prompt,
        string? obsoleteDirectionFixture = null,
        string candidateHtml = "<main data-teacher=\"generated\">Candidate</main>",
        string candidateCss = "[data-teacher=\"generated\"]{display:grid;gap:18px;}")
    {
        using var document = System.Text.Json.JsonDocument.Parse(prompt);
        return System.Text.Json.JsonSerializer.Serialize(new
        {
            schema = DesignConversionDpoTeacherAttestationRuntime.AuthoringSchema,
            incumbent_ref = document.RootElement
                .GetProperty("incumbent_ref")
                .GetString(),
            candidate_html = candidateHtml,
            candidate_css = candidateCss
        });
    }

    private static string BuildOutputForRequest(
        DesignConversionCandidateUpdateRequest request,
        string? obsoleteDirectionFixture = null,
        string candidateHtml = "<main data-teacher=\"generated\">Candidate</main>",
        string candidateCss = "[data-teacher=\"generated\"]{display:grid;gap:18px;}")
        => System.Text.Json.JsonSerializer.Serialize(new
        {
            schema = DesignConversionDpoTeacherAttestationRuntime.AuthoringSchema,
            incumbent_ref = DesignConversionDpoLeakageGuard.HashRef(
                "dcm-dpo-teacher-incumbent",
                DesignConversionDpoTeacherAttestationRuntime.BuildIncumbentBinding(
                    request.CurrentRequest)),
            candidate_html = candidateHtml,
            candidate_css = candidateCss
        });

    private static string BuildAttestationOutput(string prompt)
    {
        using var document = System.Text.Json.JsonDocument.Parse(prompt);
        var root = document.RootElement;
        var chosenRef = root.GetProperty("chosen").GetProperty("ref").GetString()!;
        var rejectedRef = root.GetProperty("rejected").GetProperty("ref").GetString()!;
        var evidenceRef = root.GetProperty("evidence_refs")[0].GetString()!;
        return System.Text.Json.JsonSerializer.Serialize(new
        {
            schema = DesignConversionDpoTeacherAttestationRuntime.Schema,
            chosen_ref = chosenRef,
            rejected_ref = rejectedRef,
            preferred_ref = chosenRef,
            supports_preference = true,
            evidence_refs = new[] { evidenceRef }
        });
    }

    private static DesignConversionDpoTeacherCallReservation CreateReservation()
    {
        var reservation = DesignConversionDpoTeacherCallReservation.Create(1, 0);
        Assert.True(reservation.IsSuccess, reservation.ErrorMessage);
        return reservation.Value!;
    }

    private static DesignConversionDpoTeacherAttestationRuntime CreateRuntime(
        ITrainingTeacherInferenceClient client)
        => new(client, ModelId, ManifestDigest, ModelLayerDigest);

    private static TrainingTeacherProvenance ValidProvenance(
        string requestId = "teacher-authoring-request")
        => new(
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
            TeacherOutputUsedForTraining: true,
            RequestId: requestId,
            InvocationCount: 1)
        {
            ModelArtifactDigestKind =
                LegalTrainingTeacherAdmissionAuthority
                    .OllamaModelLayerSha256DigestKind,
            ModelArtifactDigest = ModelLayerDigest
        };

    private sealed class AuthoringTeacherClient(
        Func<string, string> outputFactory,
        Func<TrainingTeacherProvenance>? provenanceFactory = null,
        Func<TrainingTeacherInferenceProfileEvidence?>? inferenceProfileFactory = null)
        : ITrainingTeacherInferenceClient
    {
        public int InvocationCount { get; private set; }
        public string? LastOutput { get; private set; }

        public Task<AgentInfrastructure.OperationResult<TrainingTeacherInferenceResult>>
            InferAsync(
                string prompt,
                ITrainingTeacherCallPermit callPermit,
                CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var issued = callPermit.TryIssue();
            if (!issued.IsSuccess)
                return Task.FromResult(issued.MapError<TrainingTeacherInferenceResult>());
            InvocationCount++;
            LastOutput = outputFactory(prompt);
            return Task.FromResult(
                AgentInfrastructure.OperationResult<TrainingTeacherInferenceResult>.Ok(
                    new TrainingTeacherInferenceResult(
                        LastOutput,
                        provenanceFactory is null
                            ? ValidProvenance()
                            : provenanceFactory())
                    {
                        InferenceProfile = inferenceProfileFactory is null
                            ? LocalQwenOllamaInferenceProfileAuthority.Authoring
                            : inferenceProfileFactory()
                    }));
        }

        public void Dispose()
        {
        }
    }

    private sealed class ReplayReceiptSource
        : IDesignConversionIndependentPreferenceEvidenceSource
    {
        public bool TryConsumeEvidence(
            string artifactRootBinding,
            DesignConversionTeacherAuthoredEvaluationPair pair,
            DateTimeOffset observedAtUtc,
            out DesignConversionIndependentPreferenceEvidence evidence)
        {
            evidence = BuildIndependentPreferenceReceipt(
                pair,
                artifactRootBinding,
                observedAtUtc);
            return true;
        }
    }

    private sealed class MismatchedImproverSourceEncoder
        : IDesignConversionSourceEncoder
    {
        private readonly DesignConversionDefaultSourceEncoder _inner = new();

        public AgentInfrastructure.OperationResult<DesignConversionSourceEncoding> Encode(
            DesignConversionStoredDpoExample example,
            string normalizedUnitId)
        {
            var encoded = _inner.Encode(example, normalizedUnitId);
            if (!encoded.IsSuccess || encoded.Value is null ||
                !string.Equals(
                    normalizedUnitId,
                    DesignConversionDpoUnits.ImproverUnit,
                    StringComparison.Ordinal))
            {
                return encoded;
            }

            using var document = System.Text.Json.JsonDocument.Parse(
                encoded.Value.InternalUnitStatePayload);
            var state = System.Text.Json.Nodes.JsonNode.Parse(
                document.RootElement.GetRawText())!.AsObject();
            state["unit_id"] = DesignConversionDpoUnits.CriticUnit;
            return AgentInfrastructure.OperationResult<DesignConversionSourceEncoding>.Ok(
                encoded.Value with
                {
                    InternalUnitStatePayload = state.ToJsonString()
                });
        }
    }

    private sealed class ThrowingTeacherClient(Exception exception)
        : ITrainingTeacherInferenceClient
    {
        public Task<AgentInfrastructure.OperationResult<TrainingTeacherInferenceResult>>
            InferAsync(
                string prompt,
                ITrainingTeacherCallPermit callPermit,
                CancellationToken cancellationToken = default)
        {
            var issued = callPermit.TryIssue();
            if (!issued.IsSuccess)
                return Task.FromResult(
                    issued.MapError<TrainingTeacherInferenceResult>());
            throw exception;
        }

        public void Dispose()
        {
        }
    }

    private sealed class CancellingTeacherClient(
        CancellationTokenSource cancellation)
        : ITrainingTeacherInferenceClient
    {
        public Task<AgentInfrastructure.OperationResult<TrainingTeacherInferenceResult>>
            InferAsync(
                string prompt,
                ITrainingTeacherCallPermit callPermit,
                CancellationToken cancellationToken = default)
        {
            var issued = callPermit.TryIssue();
            if (!issued.IsSuccess)
                return Task.FromResult(
                    issued.MapError<TrainingTeacherInferenceResult>());
            cancellation.Cancel();
            throw new OperationCanceledException(cancellationToken);
        }

        public void Dispose()
        {
        }
    }
}
