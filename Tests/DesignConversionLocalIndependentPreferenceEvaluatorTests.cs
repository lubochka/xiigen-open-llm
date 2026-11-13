// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Reflection;
using System.Text.Json;
using AgentInfrastructure;
using ContinuousLearning;
using DesignConversionDomain;

public sealed class DesignConversionLocalIndependentPreferenceEvaluatorTests
{
    private const string ModelId = "qwen-test:9b";
    private static readonly string ManifestDigest = new('c', 64);
    private static readonly string ModelLayerDigest = new('d', 64);
    private static readonly string RuntimeInstanceRef =
        DesignConversionDpoGenerator.Sha256Ref("local-independent-evaluator-runtime");

    [Fact]
    public async Task IssueAsync_ValidEvaluationOnlyResultBuildsCanonicalReviewOnlyReceipt()
    {
        var pair = CreatePair("valid");
        var root = DesignConversionDpoGenerator.Sha256Ref("local-evaluator-valid-root");
        var observedAtUtc = DateTimeOffset.UnixEpoch.AddDays(30);
        using var client = new EvaluationClient(BuildValidOutput);
        using var evaluator = CreateEvaluator(client);

        var issued = await evaluator.IssueAsync(root, pair, observedAtUtc);

        Assert.True(issued.IsSuccess, issued.ErrorMessage);
        var receipt = Assert.IsType<DesignConversionIndependentPreferenceEvidence>(
            issued.Value);
        Assert.Equal(
            DesignConversionIndependentPreferenceEvidenceContract.Schema,
            receipt.Schema);
        Assert.Equal(
            LocalQwenOllamaInferenceProfileAuthority.Evaluation.ProfileId,
            receipt.EvaluationProfileId);
        Assert.NotEqual(
            LocalQwenOllamaInferenceProfileAuthority.Authoring.ProfileId,
            receipt.EvaluationProfileId);
        Assert.Equal(
            LocalQwenOllamaInferenceProfileAuthority.Evaluation.ConfigurationSha256,
            receipt.EvaluationProfileConfigurationSha256);
        Assert.Equal("sha256:" + ModelLayerDigest, receipt.EvaluationModelSha256);
        Assert.Equal(root, receipt.ArtifactRootBinding);
        Assert.Equal(pair.AuthoringEvidence.GeneratedRef, receipt.GeneratedRef);
        Assert.Equal(pair.AuthoringEvidence.IncumbentRef, receipt.IncumbentRef);
        Assert.Equal(
            DesignConversionIndependentPreferenceEvidenceContract.GeneratedSide,
            receipt.PreferredDirection);
        Assert.NotEqual(
            pair.AuthoringEvidence.Provenance.RequestId,
            receipt.ScorerProvenance.RequestId);
        Assert.NotEqual(
            pair.AuthoringEvidence.OutputHash,
            receipt.ScorerProvenance.OutputId);
        Assert.Equal(RuntimeInstanceRef, receipt.ScorerProvenance.RuntimeInstanceRef);
        Assert.Equal(receipt.EvaluationProfileId,
            receipt.ScorerProvenance.EvaluationProfileId);
        Assert.Equal(receipt.EvaluationProfileVersion,
            receipt.ScorerProvenance.EvaluationProfileVersion);
        Assert.Equal(receipt.EvaluationProfileConfigurationSha256,
            receipt.ScorerProvenance.EvaluationProfileConfigurationSha256);
        Assert.Equal(receipt.EvaluationModelSha256,
            receipt.ScorerProvenance.EvaluationModelSha256);
        Assert.Equal(
            ValidEvaluationProvenance().RequestId,
            receipt.ScorerProvenance.InferenceRequestId);
        Assert.Equal(
            DesignConversionDpoGenerator.BuildIndependentPreferenceScorerOutputId(
                receipt.ScorerOutputSha256,
                receipt.ScorerProvenance.RequestId,
                receipt.ScorerProvenance.InferenceRequestId,
                receipt.ScorerProvenance.RuntimeInstanceRef),
            receipt.ScorerProvenance.OutputId);
        Assert.Equal(
            DesignConversionDpoGenerator.BuildIndependentPreferenceReceiptSha256(
                receipt),
            receipt.ReceiptSha256);
        Assert.NotNull(receipt.IssuerAuthorityCapability);
        Assert.True(
            DesignConversionLocalIndependentPreferenceEvaluator
                .HasIssuedAuthority(receipt));

        var build = new DesignConversionDpoGenerator()
            .BuildRuntimeTeacherAuthoredTrainingCandidates(
                [pair],
                [receipt],
                root,
                observedAtUtc);
        Assert.True(build.IsSuccess, build.ErrorMessage);
        Assert.Equal(2, build.Value!.AcceptedRecords.Count);
        Assert.Contains("training_grade=false", build.Value.Warnings);
        Assert.Contains("approved_for_training=false", build.Value.Warnings);
        Assert.Contains("production_dpo=false", build.Value.Warnings);
        Assert.Contains("product_not_ready", build.Value.Warnings);
        Assert.DoesNotContain("training_grade=true", build.Value.Warnings);
        Assert.All(build.Value.AcceptedRecords, record =>
        {
            Assert.Equal(
                bool.FalseString,
                record.Provenance.Metadata["training_grade"]);
            Assert.Equal(
                receipt.ReceiptSha256,
                record.Provenance.Metadata[
                    "independent_preference_receipt_sha256"]);
        });
    }

    [Fact]
    public async Task IssueAsync_IncumbentDirectionIsPreservedByCanonicalReceipt()
    {
        var pair = CreatePair("incumbent-direction");
        var root = DesignConversionDpoGenerator.Sha256Ref(
            "local-evaluator-incumbent-direction-root");
        var observedAtUtc = DateTimeOffset.UnixEpoch.AddDays(30).AddHours(1);
        using var client = new EvaluationClient(prompt => BuildOutput(
            prompt,
            DesignConversionIndependentPreferenceEvidenceContract.IncumbentSide));
        using var evaluator = CreateEvaluator(client);

        var issued = await evaluator.IssueAsync(root, pair, observedAtUtc);

        Assert.True(issued.IsSuccess, issued.ErrorMessage);
        var receipt = Assert.IsType<DesignConversionIndependentPreferenceEvidence>(
            issued.Value);
        Assert.Equal(
            DesignConversionIndependentPreferenceEvidenceContract.IncumbentSide,
            receipt.PreferredDirection);
        Assert.Equal(
            DesignConversionDpoGenerator.BuildIndependentPreferenceReceiptSha256(
                receipt),
            receipt.ReceiptSha256);
    }

    [Fact]
    public async Task IssueAsync_PromptNamesAllowedDirectionsWithoutCopyablePlaceholder()
    {
        var pair = CreatePair("prompt-contract");
        string? capturedPrompt = null;
        using var client = new EvaluationClient(prompt =>
        {
            capturedPrompt = prompt;
            return BuildValidOutput(prompt);
        });
        using var evaluator = CreateEvaluator(client);

        var issued = await evaluator.IssueAsync(
            DesignConversionDpoGenerator.Sha256Ref("prompt-contract-root"),
            pair,
            DateTimeOffset.UnixEpoch.AddDays(30).AddHours(2));

        Assert.True(issued.IsSuccess, issued.ErrorMessage);
        Assert.NotNull(capturedPrompt);
        Assert.DoesNotContain(
            "exactly generated or incumbent",
            capturedPrompt,
            StringComparison.Ordinal);
        using var document = JsonDocument.Parse(capturedPrompt);
        var root = document.RootElement;
        Assert.Contains(
            "better candidate",
            root.GetProperty("decision_instruction").GetString(),
            StringComparison.Ordinal);
        Assert.Contains(
            "Never copy",
            root.GetProperty("decision_instruction").GetString(),
            StringComparison.Ordinal);
        Assert.Equal(
            new[] { "generated", "incumbent" },
            root.GetProperty("allowed_preferred_direction_values")
                .EnumerateArray()
                .Select(static value => value.GetString())
                .ToArray());
        var required = root.GetProperty("required_output");
        Assert.Equal(
            "preferred_direction",
            required.GetProperty("preferred_direction_property_name").GetString());
        Assert.False(required.TryGetProperty("preferred_direction", out _));
    }

    [Fact]
    public async Task IssueAsync_AuthoringProfileCannotIssuePreferenceAuthority()
    {
        var pair = CreatePair("authoring-profile");
        using var client = new EvaluationClient(
            BuildValidOutput,
            inferenceProfile: LocalQwenOllamaInferenceProfileAuthority.Authoring);
        using var evaluator = CreateEvaluator(client);

        var issued = await evaluator.IssueAsync(
            DesignConversionDpoGenerator.Sha256Ref("authoring-profile-root"),
            pair,
            DateTimeOffset.UnixEpoch.AddDays(31));

        Assert.False(issued.IsSuccess);
        Assert.Equal(
            DesignConversionLocalIndependentPreferenceEvaluator
                .EvaluationProfileInvalidErrorCode,
            issued.ErrorCode);
    }

    [Fact]
    public async Task IssueAsync_AuthoringResponseCannotSelfApprove()
    {
        var pair = CreatePair("authoring-response");
        var authoringResponse = JsonSerializer.Serialize(new
        {
            schema = DesignConversionDpoTeacherAttestationRuntime.AuthoringSchema,
            incumbent_ref = pair.AuthoringEvidence.IncumbentRef,
            candidate_html = pair.GeneratedEvaluation.SubmittedRequest.Html,
            candidate_css = pair.GeneratedEvaluation.SubmittedRequest.Css
        });
        using var client = new EvaluationClient(_ => authoringResponse);
        using var evaluator = CreateEvaluator(client);

        var issued = await evaluator.IssueAsync(
            DesignConversionDpoGenerator.Sha256Ref("authoring-response-root"),
            pair,
            DateTimeOffset.UnixEpoch.AddDays(31));

        Assert.False(issued.IsSuccess);
        Assert.Equal(
            DesignConversionLocalIndependentPreferenceEvaluator
                .EvaluationOutputInvalidErrorCode,
            issued.ErrorCode);
    }

    [Theory]
    [InlineData("authoring_request_identity")]
    [InlineData("training_authoring_provenance")]
    [InlineData("wrong_model_identity")]
    public async Task IssueAsync_AuthoringOrMismatchedProvenanceFailsClosed(
        string scenario)
    {
        var pair = CreatePair("provenance-" + scenario);
        var provenance = ValidEvaluationProvenance();
        provenance = scenario switch
        {
            "authoring_request_identity" => provenance with
            {
                RequestId = pair.AuthoringEvidence.Provenance.RequestId
            },
            "training_authoring_provenance" => provenance with
            {
                TeacherOutputUsedForTraining = true
            },
            "wrong_model_identity" => provenance with
            {
                ModelArtifactDigest = new string('e', 64)
            },
            _ => throw new ArgumentOutOfRangeException(nameof(scenario))
        };
        using var client = new EvaluationClient(
            BuildValidOutput,
            provenance: provenance);
        using var evaluator = CreateEvaluator(client);

        var issued = await evaluator.IssueAsync(
            DesignConversionDpoGenerator.Sha256Ref("provenance-root-" + scenario),
            pair,
            DateTimeOffset.UnixEpoch.AddDays(32));

        Assert.False(issued.IsSuccess);
        Assert.Equal(
            DesignConversionLocalIndependentPreferenceEvaluator
                .EvaluationProvenanceInvalidErrorCode,
            issued.ErrorCode);
    }

    [Theory]
    [InlineData("mismatched_request_hash")]
    [InlineData("swapped_evaluation_identities")]
    [InlineData("unknown_field")]
    [InlineData("invalid_direction")]
    public async Task IssueAsync_MismatchedOrNonCanonicalOutputFailsClosed(
        string scenario)
    {
        var pair = CreatePair("output-" + scenario);
        using var client = new EvaluationClient(prompt =>
            BuildInvalidOutput(prompt, scenario));
        using var evaluator = CreateEvaluator(client);

        var issued = await evaluator.IssueAsync(
            DesignConversionDpoGenerator.Sha256Ref("output-root-" + scenario),
            pair,
            DateTimeOffset.UnixEpoch.AddDays(33));

        Assert.False(issued.IsSuccess);
        Assert.Equal(
            DesignConversionLocalIndependentPreferenceEvaluator
                .EvaluationOutputInvalidErrorCode,
            issued.ErrorCode);
    }

    [Theory]
    [InlineData("swapped_pair")]
    [InlineData("binding")]
    [InlineData("root")]
    [InlineData("request_hash")]
    [InlineData("output_hash")]
    [InlineData("joint_output_commitment")]
    [InlineData("profile")]
    [InlineData("model")]
    [InlineData("provenance")]
    [InlineData("provenance_model")]
    [InlineData("inference_request")]
    [InlineData("authoring_scorer_identity")]
    [InlineData("reconstructed_without_capability")]
    [InlineData("forged_capability")]
    [InlineData("stale")]
    public async Task IssuedReceipt_TamperSwapAndStaleEvidenceRemainZeroDpo(
        string tamper)
    {
        var pair = CreatePair("tamper-" + tamper);
        var root = DesignConversionDpoGenerator.Sha256Ref("tamper-root-" + tamper);
        var observedAtUtc = DateTimeOffset.UnixEpoch.AddDays(34);
        using var client = new EvaluationClient(BuildValidOutput);
        using var evaluator = CreateEvaluator(client);
        var issued = await evaluator.IssueAsync(root, pair, observedAtUtc);
        Assert.True(issued.IsSuccess, issued.ErrorMessage);
        var receipt = issued.Value!;
        var buildObservedAtUtc = observedAtUtc;

        var tampered = tamper switch
        {
            "swapped_pair" => receipt with
            {
                GeneratedRef = receipt.IncumbentRef,
                IncumbentRef = receipt.GeneratedRef,
                ReceiptSha256 = string.Empty
            },
            "binding" => receipt with
            {
                GeneratedBinding = DesignConversionDpoGenerator.Sha256Ref(
                    "tampered-binding"),
                ReceiptSha256 = string.Empty
            },
            "root" => receipt with
            {
                ArtifactRootBinding = DesignConversionDpoGenerator.Sha256Ref(
                    "different-root"),
                ReceiptSha256 = string.Empty
            },
            "request_hash" => receipt with
            {
                ScorerRequestSha256 = DesignConversionDpoGenerator.Sha256Ref(
                    "tampered-request"),
                ReceiptSha256 = string.Empty
            },
            "output_hash" => receipt with
            {
                ScorerOutputSha256 = DesignConversionDpoGenerator.Sha256Ref(
                    "tampered-output"),
                ReceiptSha256 = string.Empty
            },
            "joint_output_commitment" => WithJointOutputCommitment(
                receipt,
                DesignConversionDpoGenerator.Sha256Ref(
                    "tampered-joint-output")),
            "profile" => receipt with
            {
                EvaluationProfileId =
                    LocalQwenOllamaInferenceProfileAuthority.Authoring.ProfileId,
                ReceiptSha256 = string.Empty
            },
            "model" => receipt with
            {
                EvaluationModelSha256 = DesignConversionDpoGenerator.Sha256Ref(
                    "tampered-model"),
                ReceiptSha256 = string.Empty
            },
            "provenance" => receipt with
            {
                ScorerProvenance = receipt.ScorerProvenance with
                {
                    RequestId = pair.AuthoringEvidence.Provenance.RequestId
                },
                ReceiptSha256 = string.Empty
            },
            "provenance_model" => receipt with
            {
                ScorerProvenance = receipt.ScorerProvenance with
                {
                    EvaluationModelSha256 =
                        DesignConversionDpoGenerator.Sha256Ref(
                            "tampered-provenance-model")
                },
                ReceiptSha256 = string.Empty
            },
            "inference_request" => WithInferenceRequest(
                receipt,
                pair.AuthoringEvidence.Provenance.RequestId),
            "authoring_scorer_identity" => receipt with
            {
                ScorerProvenance = receipt.ScorerProvenance with
                {
                    ScorerId = pair.AuthoringEvidence.Provenance.ModelId
                },
                ReceiptSha256 = string.Empty
            },
            "reconstructed_without_capability" => receipt with
            {
                IssuerAuthorityCapability = null
            },
            "forged_capability" => receipt with
            {
                IssuerAuthorityCapability = new object()
            },
            "stale" => receipt,
            _ => throw new ArgumentOutOfRangeException(nameof(tamper))
        };
        if (tamper is "profile" or "model" or "provenance" or
            "provenance_model" or "authoring_scorer_identity")
        {
            tampered = tampered with
            {
                ScorerRequestSha256 = DesignConversionDpoGenerator
                    .BuildIndependentPreferenceScorerRequestSha256(
                        pair,
                        tampered.ArtifactRootBinding,
                        tampered.ScorerProvenance,
                        tampered.EvaluationProfileId,
                        tampered.EvaluationProfileVersion,
                        tampered.EvaluationProfileConfigurationSha256,
                        tampered.EvaluationModelSha256)
            };
        }
        if (tamper != "stale")
        {
            tampered = tampered with
            {
                ReceiptSha256 = DesignConversionDpoGenerator
                    .BuildIndependentPreferenceReceiptSha256(tampered)
            };
        }
        else
        {
            buildObservedAtUtc = observedAtUtc.Add(
                DesignConversionIndependentPreferenceEvidenceContract
                    .MaximumFreshness).AddTicks(1);
        }
        if (tamper == "joint_output_commitment")
        {
            Assert.Equal(
                DesignConversionDpoGenerator.BuildIndependentPreferenceScorerOutputId(
                    tampered.ScorerOutputSha256,
                    tampered.ScorerProvenance.RequestId,
                    tampered.ScorerProvenance.InferenceRequestId,
                    tampered.ScorerProvenance.RuntimeInstanceRef),
                tampered.ScorerProvenance.OutputId);
            Assert.Equal(
                DesignConversionDpoGenerator.BuildIndependentPreferenceReceiptSha256(
                    tampered),
                tampered.ReceiptSha256);
            Assert.NotEqual(
                DesignConversionDpoGenerator
                    .BuildIndependentPreferenceScorerOutputCommitmentSha256(receipt),
                DesignConversionDpoGenerator
                    .BuildIndependentPreferenceScorerOutputCommitmentSha256(tampered));
        }

        var build = new DesignConversionDpoGenerator()
            .BuildRuntimeTeacherAuthoredTrainingCandidates(
                [pair],
                [tampered],
                root,
                buildObservedAtUtc);

        Assert.True(build.IsSuccess, build.ErrorMessage);
        Assert.Empty(build.Value!.AcceptedRecords);
        Assert.Contains("independent_preference_not_proven", build.Value.Warnings);
        Assert.Contains("training_grade=false", build.Value.Warnings);
        Assert.DoesNotContain("training_grade=true", build.Value.Warnings);
    }

    [Fact]
    public async Task IssuedReceipt_JsonRoundTripLosesAuthorityAndRemainsZeroDpo()
    {
        var pair = CreatePair("json-roundtrip");
        var root = DesignConversionDpoGenerator.Sha256Ref(
            "json-roundtrip-root");
        var observedAtUtc = DateTimeOffset.UnixEpoch.AddDays(35);
        using var client = new EvaluationClient(BuildValidOutput);
        using var evaluator = CreateEvaluator(client);
        var issued = await evaluator.IssueAsync(root, pair, observedAtUtc);
        Assert.True(issued.IsSuccess, issued.ErrorMessage);
        var receipt = issued.Value!;

        var jsonOptions = CreateNonPublicContractRoundTripOptions();
        var json = JsonSerializer.Serialize(receipt, jsonOptions);
        var roundTripped = JsonSerializer.Deserialize<
            DesignConversionIndependentPreferenceEvidence>(json, jsonOptions);

        Assert.NotNull(roundTripped);
        Assert.Null(roundTripped.IssuerAuthorityCapability);
        Assert.Equal(receipt.ReceiptSha256, roundTripped.ReceiptSha256);
        Assert.Equal(
            receipt.ReceiptSha256,
            DesignConversionDpoGenerator.BuildIndependentPreferenceReceiptSha256(
                roundTripped));
        Assert.Equal(
            DesignConversionDpoGenerator
                .BuildIndependentPreferenceScorerOutputCommitmentSha256(receipt),
            DesignConversionDpoGenerator
                .BuildIndependentPreferenceScorerOutputCommitmentSha256(
                    roundTripped));
        var build = new DesignConversionDpoGenerator()
            .BuildRuntimeTeacherAuthoredTrainingCandidates(
                [pair],
                [roundTripped],
                root,
                observedAtUtc);

        Assert.True(build.IsSuccess, build.ErrorMessage);
        Assert.Empty(build.Value!.AcceptedRecords);
        Assert.Contains("independent_preference_not_proven", build.Value.Warnings);
        Assert.Contains(
            "independent_preference_evidence_authority_missing_or_tampered",
            build.Value.Warnings);
        Assert.Contains("training_grade=false", build.Value.Warnings);
        Assert.False(
            DesignConversionLocalIndependentPreferenceEvaluator
                .HasIssuedAuthority(roundTripped));
        Assert.False(
            DesignConversionLocalIndependentPreferenceEvaluator
                .TryConsumeIssuedAuthority(roundTripped));
    }

    [Fact]
    public async Task IssuedReceipt_GenuineFutureIssueFailsClosedAtEarlierObservation()
    {
        var pair = CreatePair("future-issued");
        var root = DesignConversionDpoGenerator.Sha256Ref("future-issued-root");
        var observedAtUtc = DateTimeOffset.UnixEpoch.AddDays(36);
        var futureIssuedAtUtc = observedAtUtc.AddSeconds(30);
        using var client = new EvaluationClient(BuildValidOutput);
        using var evaluator = CreateEvaluator(client);
        var issued = await evaluator.IssueAsync(root, pair, futureIssuedAtUtc);
        Assert.True(issued.IsSuccess, issued.ErrorMessage);
        Assert.True(
            DesignConversionLocalIndependentPreferenceEvaluator
                .HasIssuedAuthority(issued.Value!));

        var build = new DesignConversionDpoGenerator()
            .BuildRuntimeTeacherAuthoredTrainingCandidates(
                [pair],
                [issued.Value!],
                root,
                observedAtUtc);

        Assert.True(build.IsSuccess, build.ErrorMessage);
        Assert.Empty(build.Value!.AcceptedRecords);
        Assert.Contains("independent_preference_not_proven", build.Value.Warnings);
        Assert.Contains("training_grade=false", build.Value.Warnings);
    }

    [Fact]
    public async Task IssuedEvidence_SwappedLookupAndReplayFailClosed()
    {
        var pair = CreatePair("replay");
        var other = CreatePair("replay-other");
        var root = DesignConversionDpoGenerator.Sha256Ref("replay-root");
        var observedAtUtc = DateTimeOffset.UnixEpoch.AddDays(35);
        using var client = new EvaluationClient(BuildValidOutput);
        using var evaluator = CreateEvaluator(client);
        var issued = await evaluator.IssueAsync(root, pair, observedAtUtc);
        Assert.True(issued.IsSuccess, issued.ErrorMessage);

        Assert.False(evaluator.TryConsumeEvidence(
            root,
            other,
            observedAtUtc,
            out _));
        Assert.False(evaluator.TryConsumeEvidence(
            DesignConversionDpoGenerator.Sha256Ref("wrong-root"),
            pair,
            observedAtUtc,
            out _));
        Assert.True(evaluator.TryConsumeEvidence(
            root,
            pair,
            observedAtUtc,
            out var first));
        Assert.Equal(issued.Value!.ReceiptSha256, first.ReceiptSha256);
        Assert.False(evaluator.TryConsumeEvidence(
            root,
            pair,
            observedAtUtc,
            out _));
    }

    [Fact]
    public async Task IssuedReceipt_DirectGeneratorReplayFailsClosed()
    {
        var pair = CreatePair("direct-generator-replay");
        var root = DesignConversionDpoGenerator.Sha256Ref(
            "direct-generator-replay-root");
        var observedAtUtc = DateTimeOffset.UnixEpoch.AddDays(35).AddHours(1);
        using var client = new EvaluationClient(BuildValidOutput);
        using var evaluator = CreateEvaluator(client);
        var issued = await evaluator.IssueAsync(root, pair, observedAtUtc);
        Assert.True(issued.IsSuccess, issued.ErrorMessage);
        var generator = new DesignConversionDpoGenerator();

        var first = generator.BuildRuntimeTeacherAuthoredTrainingCandidates(
            [pair],
            [issued.Value!],
            root,
            observedAtUtc);
        var replay = generator.BuildRuntimeTeacherAuthoredTrainingCandidates(
            [pair],
            [issued.Value!],
            root,
            observedAtUtc);

        Assert.True(first.IsSuccess, first.ErrorMessage);
        Assert.Equal(2, first.Value!.AcceptedRecords.Count);
        Assert.True(replay.IsSuccess, replay.ErrorMessage);
        Assert.Empty(replay.Value!.AcceptedRecords);
        Assert.Contains("independent_preference_not_proven", replay.Value.Warnings);
    }

    [Fact]
    public async Task IssuedReceipt_ConcurrentDirectReplayAllowsExactlyOneConsumer()
    {
        var pair = CreatePair("concurrent-direct-replay");
        var root = DesignConversionDpoGenerator.Sha256Ref(
            "concurrent-direct-replay-root");
        var observedAtUtc = DateTimeOffset.UnixEpoch.AddDays(38);
        using var client = new EvaluationClient(BuildValidOutput);
        using var evaluator = CreateEvaluator(client);
        var issued = await evaluator.IssueAsync(root, pair, observedAtUtc);
        Assert.True(issued.IsSuccess, issued.ErrorMessage);
        var receipt = issued.Value!;
        using var ready = new CountdownEvent(2);
        using var release = new ManualResetEventSlim(false);

        Task<OperationResult<DesignConversionDpoBuildResult>> StartConsumer()
            => Task.Run(() =>
            {
                ready.Signal();
                release.Wait();
                return new DesignConversionDpoGenerator()
                    .BuildRuntimeTeacherAuthoredTrainingCandidates(
                        [pair],
                        [receipt],
                        root,
                        observedAtUtc);
            });

        var first = StartConsumer();
        var second = StartConsumer();
        var bothReady = ready.Wait(TimeSpan.FromSeconds(10));
        release.Set();
        Assert.True(bothReady);
        var results = await Task.WhenAll(first, second);
        Assert.All(results, result =>
            Assert.True(result.IsSuccess, result.ErrorMessage));

        var accepted = Assert.Single(results, result =>
            result.Value!.AcceptedRecords.Count == 2);
        Assert.Equal(
            [DesignConversionDpoUnits.CriticUnit,
             DesignConversionDpoUnits.ImproverUnit],
            accepted.Value!.AcceptedRecords
                .Select(static record => record.UnitId)
                .Order(StringComparer.Ordinal)
                .ToArray());
        var replay = Assert.Single(results, result =>
            result.Value!.AcceptedRecords.Count == 0);
        Assert.Contains(
            "independent_preference_not_proven",
            replay.Value!.Warnings);
        Assert.Contains("training_grade=false", replay.Value.Warnings);
    }

    [Fact]
    public async Task MissingLocalEvaluatorResponseKeepsZeroDpoAndHasNoDefaultIssuer()
    {
        var pair = CreatePair("missing-response");
        using var client = new EvaluationClient(
            _ => string.Empty,
            failureCode: "LOCAL_EVALUATOR_UNAVAILABLE");
        using var evaluator = CreateEvaluator(client);

        var issued = await evaluator.IssueAsync(
            DesignConversionDpoGenerator.Sha256Ref("missing-response-root"),
            pair,
            DateTimeOffset.UnixEpoch.AddDays(36));
        var noReceipt = new DesignConversionDpoGenerator()
            .BuildRuntimeTeacherAuthoredTrainingCandidates(
                [pair],
                DateTimeOffset.UnixEpoch.AddDays(36));

        Assert.False(issued.IsSuccess);
        Assert.True(noReceipt.IsSuccess, noReceipt.ErrorMessage);
        Assert.Empty(noReceipt.Value!.AcceptedRecords);
        Assert.Contains("independent_preference_not_proven", noReceipt.Value.Warnings);
        Assert.Contains("training_grade=false", noReceipt.Value.Warnings);

        AssertNoDefaultIssuer(typeof(ContinuousLearningModelServerStartup));
        AssertNoDefaultIssuer(typeof(DesignConversionLocalServiceComposition));
        Assert.DoesNotContain(
            typeof(ExternalHttpDesignConversionVisualJudge),
            typeof(DesignConversionLocalIndependentPreferenceEvaluator)
                .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                .Select(static field => field.FieldType));
    }

    [Theory]
    [InlineData(LocalQwenOllamaTeacherInferenceClient.TimeoutErrorCode)]
    [InlineData(LocalQwenOllamaTeacherInferenceClient.TransportFailedErrorCode)]
    [InlineData(LocalQwenOllamaTeacherInferenceClient.ResponseInvalidErrorCode)]
    public async Task EvaluationTimeoutErrorOrMalformedResponseLeavesNoReceiptOrPair(
        string failureCode)
    {
        var pair = CreatePair("closed-failure-" + failureCode);
        var root = DesignConversionDpoGenerator.Sha256Ref(
            "closed-failure-root-" + failureCode);
        using var client = new EvaluationClient(
            _ => string.Empty,
            failureCode: failureCode);
        using var evaluator = CreateEvaluator(client);

        var issued = await evaluator.IssueAsync(
            root,
            pair,
            DateTimeOffset.UnixEpoch.AddDays(36).AddHours(1));
        var candidates = new DesignConversionDpoGenerator()
            .BuildRuntimeTeacherAuthoredTrainingCandidates(
                [pair],
                DateTimeOffset.UnixEpoch.AddDays(36).AddHours(1));

        Assert.False(issued.IsSuccess);
        Assert.Equal(failureCode, issued.ErrorCode);
        Assert.False(evaluator.TryConsumeEvidence(
            root,
            pair,
            DateTimeOffset.UnixEpoch.AddDays(36).AddHours(1),
            out _));
        Assert.True(candidates.IsSuccess, candidates.ErrorMessage);
        Assert.Empty(candidates.Value!.AcceptedRecords);
        Assert.Contains(
            "independent_preference_not_proven",
            candidates.Value.Warnings);
    }

    [Fact]
    public async Task RuntimeDisposalPreventsHeldRegistrationFromIssuingOrConsuming()
    {
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(
            Path.Combine(
                Path.GetTempPath(),
                "local-independent-evaluator-disposal-" +
                Guid.NewGuid().ToString("N")));
        var pair = CreatePair("runtime-disposal");
        var observedAtUtc = DateTimeOffset.UnixEpoch.AddDays(37);
        var client = new EvaluationClient(BuildValidOutput);
        var evaluator = CreateEvaluator(client);
        var owned =
            DesignConversionLocalIndependentPreferenceEvaluatorRuntime
                .OwnForTesting(artifactDb, evaluator);
        Assert.True(owned.IsSuccess, owned.ErrorMessage);
        var runtime = owned.Value!;
        var heldRegistration =
            DesignConversionIndependentPreferenceEvidenceAssociation.Resolve(
                artifactDb)!;
        var issued = await heldRegistration.IssueEvidenceAsync(
            pair,
            observedAtUtc,
            CancellationToken.None);
        Assert.True(issued.IsSuccess, issued.ErrorMessage);

        runtime.Dispose();

        var afterDispose = await heldRegistration.IssueEvidenceAsync(
            CreatePair("runtime-disposal-after"),
            observedAtUtc,
            CancellationToken.None);
        Assert.False(afterDispose.IsSuccess);
        Assert.Equal(
            DesignConversionLocalIndependentPreferenceEvaluator
                .EvaluationUnavailableErrorCode,
            afterDispose.ErrorCode);
        Assert.False(heldRegistration.TryConsumeEvidence(
            pair,
            observedAtUtc,
            out _));
        Assert.Null(
            DesignConversionIndependentPreferenceEvidenceAssociation.Resolve(
                artifactDb));
    }

    private static DesignConversionLocalIndependentPreferenceEvaluator
        CreateEvaluator(ILegalTeacherEvaluationInferenceClient client)
        => DesignConversionLocalIndependentPreferenceEvaluator.OwnForTesting(
            client,
            ModelId,
            ManifestDigest,
            ModelLayerDigest,
            RuntimeInstanceRef);

    private static JsonSerializerOptions
        CreateNonPublicContractRoundTripOptions()
    {
        var resolver = new System.Text.Json.Serialization.Metadata
            .DefaultJsonTypeInfoResolver();
        resolver.Modifiers.Add(typeInfo =>
        {
            var existingNames = typeInfo.Properties
                .Select(static property => property.Name)
                .ToHashSet(StringComparer.Ordinal);
            foreach (var property in typeInfo.Type.GetProperties(
                         BindingFlags.Instance | BindingFlags.NonPublic))
            {
                if (!property.CanRead || !property.CanWrite ||
                    property.GetIndexParameters().Length != 0 ||
                    property.GetCustomAttribute<
                        System.Text.Json.Serialization.JsonIgnoreAttribute>()
                        is not null ||
                    !existingNames.Add(property.Name))
                {
                    continue;
                }

                var jsonProperty = typeInfo.CreateJsonPropertyInfo(
                    property.PropertyType,
                    property.Name);
                jsonProperty.Get = target => property.GetValue(target);
                jsonProperty.Set = (target, value) =>
                    property.SetValue(target, value);
                typeInfo.Properties.Add(jsonProperty);
            }
        });
        return new JsonSerializerOptions { TypeInfoResolver = resolver };
    }

    private static void AssertNoDefaultIssuer(Type compositionType)
    {
        var methods = compositionType.GetMethods(
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static |
            BindingFlags.Instance);
        Assert.DoesNotContain(methods, method =>
            method.ReturnType ==
                typeof(DesignConversionLocalIndependentPreferenceEvaluator) ||
            method.GetParameters().Any(parameter =>
                parameter.ParameterType ==
                    typeof(DesignConversionLocalIndependentPreferenceEvaluator) ||
                parameter.ParameterType ==
                    typeof(IDesignConversionIndependentPreferenceEvidenceSource)));
    }

    private static string BuildValidOutput(string prompt)
        => BuildOutput(
            prompt,
            DesignConversionIndependentPreferenceEvidenceContract.GeneratedSide);

    private static string BuildOutput(string prompt, string preferredDirection)
    {
        using var document = JsonDocument.Parse(prompt);
        var root = document.RootElement;
        return JsonSerializer.Serialize(new
        {
            schema = DesignConversionLocalIndependentPreferenceEvaluator
                .ResultSchema,
            scorer_request_sha256 = root.GetProperty("scorer_request_sha256")
                .GetString(),
            generated_evaluation_identity = root
                .GetProperty("generated_evaluation_identity").GetString(),
            incumbent_evaluation_identity = root
                .GetProperty("incumbent_evaluation_identity").GetString(),
            preferred_direction = preferredDirection
        });
    }

    private static DesignConversionIndependentPreferenceEvidence
        WithInferenceRequest(
            DesignConversionIndependentPreferenceEvidence receipt,
            string inferenceRequestId)
    {
        var provenance = receipt.ScorerProvenance with
        {
            InferenceRequestId = inferenceRequestId
        };
        provenance = provenance with
        {
            OutputId = DesignConversionDpoGenerator
                .BuildIndependentPreferenceScorerOutputId(
                    receipt.ScorerOutputSha256,
                    provenance.RequestId,
                    provenance.InferenceRequestId,
                    provenance.RuntimeInstanceRef)
        };
        return receipt with
        {
            ScorerProvenance = provenance,
            ReceiptSha256 = string.Empty
        };
    }

    private static DesignConversionIndependentPreferenceEvidence
        WithJointOutputCommitment(
            DesignConversionIndependentPreferenceEvidence receipt,
            string scorerOutputSha256)
    {
        var provenance = receipt.ScorerProvenance with
        {
            OutputId = DesignConversionDpoGenerator
                .BuildIndependentPreferenceScorerOutputId(
                    scorerOutputSha256,
                    receipt.ScorerProvenance.RequestId,
                    receipt.ScorerProvenance.InferenceRequestId,
                    receipt.ScorerProvenance.RuntimeInstanceRef)
        };
        return receipt with
        {
            ScorerOutputSha256 = scorerOutputSha256,
            ScorerProvenance = provenance,
            ReceiptSha256 = string.Empty
        };
    }

    private static string BuildInvalidOutput(string prompt, string scenario)
    {
        using var document = JsonDocument.Parse(prompt);
        var root = document.RootElement;
        var requestSha = root.GetProperty("scorer_request_sha256").GetString();
        var generatedIdentity = root.GetProperty("generated_evaluation_identity")
            .GetString();
        var incumbentIdentity = root.GetProperty("incumbent_evaluation_identity")
            .GetString();
        return scenario switch
        {
            "mismatched_request_hash" => JsonSerializer.Serialize(new
            {
                schema = DesignConversionLocalIndependentPreferenceEvaluator
                    .ResultSchema,
                scorer_request_sha256 = DesignConversionDpoGenerator.Sha256Ref(
                    "wrong-request"),
                generated_evaluation_identity = generatedIdentity,
                incumbent_evaluation_identity = incumbentIdentity,
                preferred_direction = "generated"
            }),
            "swapped_evaluation_identities" => JsonSerializer.Serialize(new
            {
                schema = DesignConversionLocalIndependentPreferenceEvaluator
                    .ResultSchema,
                scorer_request_sha256 = requestSha,
                generated_evaluation_identity = incumbentIdentity,
                incumbent_evaluation_identity = generatedIdentity,
                preferred_direction = "generated"
            }),
            "unknown_field" => JsonSerializer.Serialize(new
            {
                schema = DesignConversionLocalIndependentPreferenceEvaluator
                    .ResultSchema,
                scorer_request_sha256 = requestSha,
                generated_evaluation_identity = generatedIdentity,
                incumbent_evaluation_identity = incumbentIdentity,
                preferred_direction = "generated",
                confidence = 1.0
            }),
            "invalid_direction" => JsonSerializer.Serialize(new
            {
                schema = DesignConversionLocalIndependentPreferenceEvaluator
                    .ResultSchema,
                scorer_request_sha256 = requestSha,
                generated_evaluation_identity = generatedIdentity,
                incumbent_evaluation_identity = incumbentIdentity,
                preferred_direction = "authoring_selected"
            }),
            _ => throw new ArgumentOutOfRangeException(nameof(scenario))
        };
    }

    internal static DesignConversionTeacherAuthoredEvaluationPair CreatePair(
        string suffix,
        bool generatedIsGood = true,
        bool neutralizeQualityMetadata = false)
    {
        const string goodHtml =
            "<section class=\"product-card\" aria-labelledby=\"product-title\">" +
            "<h1 id=\"product-title\">Team workspace</h1>" +
            "<p>Plan projects and share progress with your team.</p>" +
            "<button type=\"button\">Start free trial</button></section>";
        const string goodCss =
            ".product-card{display:grid;gap:16px;max-width:36rem;padding:24px;" +
            "color:#172033;background:#fff;border:1px solid #d8deea;" +
            "border-radius:12px}.product-card button{min-height:44px}";
        const string poorHtml =
            "<div><h1>Team workspace</h1><p>Plan projects and share progress" +
            "<button>Start free trial</div>";
        const string poorCss =
            "div{width:1px;height:1px;overflow:hidden;color:#fff;background:#fff;" +
            "font-size:2px}";
        var incumbentIsGood = !generatedIsGood;
        var incumbent = DesignConversionDpoGeneratorTests.CreateRuntimeEvaluation(
            canPersistTrainingCandidate: neutralizeQualityMetadata || incumbentIsGood,
            jobId: $"local-eval-{suffix}-incumbent-job",
            requestId: $"local-eval-{suffix}-incumbent-request",
            outputId: $"local-eval-{suffix}-incumbent-output",
            candidateId: $"local-eval-{suffix}-incumbent-candidate",
            verdictId: $"local-eval-{suffix}-incumbent-verdict",
            score: neutralizeQualityMetadata ? 0.5 : incumbentIsGood ? 0.91 : 0.42,
            passed: neutralizeQualityMetadata || incumbentIsGood,
            htmlSuffix: "incumbent-" + suffix);
        incumbent = WithSubmittedContent(
            incumbent,
            incumbentIsGood ? goodHtml : poorHtml,
            incumbentIsGood ? goodCss : poorCss);
        var request = CreateUpdateRequest(incumbent);
        var candidateHtml = generatedIsGood ? goodHtml : poorHtml;
        var candidateCss = generatedIsGood ? goodCss : poorCss;
        var output = JsonSerializer.Serialize(new
        {
            schema = DesignConversionDpoTeacherAttestationRuntime.AuthoringSchema,
            incumbent_ref = DesignConversionDpoLeakageGuard.HashRef(
                "dcm-dpo-teacher-incumbent",
                DesignConversionDpoTeacherAttestationRuntime.BuildIncumbentBinding(
                    request.CurrentRequest)),
            candidate_html = candidateHtml,
            candidate_css = candidateCss
        });
        var authored = DesignConversionDpoTeacherAttestationRuntime
            .ParseAuthoringOutputForTesting(
                output,
                request,
                ValidAuthoringProvenance("authoring-request-" + suffix));
        Assert.True(authored.IsSuccess, authored.ErrorMessage);

        var generated = DesignConversionDpoGeneratorTests.CreateRuntimeEvaluation(
            canPersistTrainingCandidate: neutralizeQualityMetadata || generatedIsGood,
            jobId: $"local-eval-{suffix}-generated-job",
            requestId: authored.Value!.Candidate.RequestId,
            outputId: $"local-eval-{suffix}-generated-output",
            candidateId: authored.Value.Candidate.CandidateId,
            verdictId: $"local-eval-{suffix}-generated-verdict",
            score: neutralizeQualityMetadata ? 0.5 : generatedIsGood ? 0.91 : 0.42,
            passed: neutralizeQualityMetadata || generatedIsGood,
            htmlSuffix: "generated-" + suffix);
        var submitted = generated.SubmittedRequest with
        {
            RequestId = authored.Value.Candidate.RequestId,
            Html = authored.Value.Candidate.Html,
            Css = authored.Value.Candidate.Css
        };
        generated = generated with
        {
            SubmittedRequest = submitted,
            RuntimeFlowResult = generated.RuntimeFlowResult with
            {
                Request = submitted,
                Candidate = authored.Value.Candidate,
                Output = generated.RuntimeFlowResult.Output with
                {
                    CandidateId = authored.Value.Candidate.CandidateId
                }
            }
        };
        return new DesignConversionTeacherAuthoredEvaluationPair(
            2,
            generated,
            incumbent,
            authored.Value.AuthoringEvidence!,
            authored.Value.Candidate.CandidateId);
    }

    private static DesignConversionInternalEvaluationResult WithSubmittedContent(
        DesignConversionInternalEvaluationResult evaluation,
        string html,
        string css)
    {
        var submitted = evaluation.SubmittedRequest with
        {
            Html = html,
            Css = css
        };
        return evaluation with
        {
            SubmittedRequest = submitted,
            RuntimeFlowResult = evaluation.RuntimeFlowResult with
            {
                Request = submitted
            }
        };
    }

    private static DesignConversionCandidateUpdateRequest CreateUpdateRequest(
        DesignConversionInternalEvaluationResult incumbent)
    {
        var submitted = incumbent.SubmittedRequest;
        return new DesignConversionCandidateUpdateRequest(
            LoopId: "local-independent-evaluator-authoring-loop",
            CompletedAttemptNumber: 1,
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

    private static TrainingTeacherProvenance ValidAuthoringProvenance(
        string requestId)
        => ValidEvaluationProvenance() with
        {
            TeacherOutputUsedForTraining = true,
            RequestId = requestId
        };

    private static TrainingTeacherProvenance ValidEvaluationProvenance()
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
            TeacherOutputUsedForTraining: false,
            RequestId: "local-evaluation-transport-request",
            InvocationCount: 1)
        {
            ModelArtifactDigestKind =
                LegalTrainingTeacherAdmissionAuthority
                    .OllamaModelLayerSha256DigestKind,
            ModelArtifactDigest = ModelLayerDigest
        };

    private sealed class EvaluationClient(
        Func<string, string> outputFactory,
        TrainingTeacherInferenceProfileEvidence? inferenceProfile = null,
        TrainingTeacherProvenance? provenance = null,
        string? failureCode = null)
        : ILegalTeacherEvaluationInferenceClient
    {
        public Task<OperationResult<TeacherEvaluationInferenceResult>>
            EvaluateAsync(
                string prompt,
                ILegalTeacherCallPermit callPermit,
                CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!string.IsNullOrWhiteSpace(failureCode))
            {
                return Task.FromResult(
                    OperationResult<TeacherEvaluationInferenceResult>.Fail(
                        "The local evaluation response is unavailable.",
                        failureCode));
            }

            var permit = callPermit.TryIssue();
            if (!permit.IsSuccess)
                return Task.FromResult(
                    permit.MapError<TeacherEvaluationInferenceResult>());
            return Task.FromResult(
                OperationResult<TeacherEvaluationInferenceResult>.Ok(
                    new TeacherEvaluationInferenceResult(
                        outputFactory(prompt),
                        provenance ?? ValidEvaluationProvenance())
                    {
                        InferenceProfile = inferenceProfile ??
                            LocalQwenOllamaInferenceProfileAuthority.Evaluation
                    }));
        }

        public void Dispose()
        {
        }
    }
}

internal static class DesignConversionIndependentPreferenceEvidenceTestIssuer
{
    private const string ModelId = "qwen-test:9b";
    private static readonly string ManifestDigest = new('c', 64);
    private static readonly string ModelLayerDigest = new('d', 64);

    internal static DesignConversionIndependentPreferenceEvidence Issue(
        DesignConversionTeacherAuthoredEvaluationPair pair,
        string artifactRootBinding,
        DateTimeOffset observedAtUtc,
        string preferredDirection =
            DesignConversionIndependentPreferenceEvidenceContract.GeneratedSide)
    {
        using var client = new Client(preferredDirection);
        using var evaluator =
            DesignConversionLocalIndependentPreferenceEvaluator.OwnForTesting(
                client,
                ModelId,
                ManifestDigest,
                ModelLayerDigest,
                DesignConversionDpoGenerator.Sha256Ref(
                    "test-independent-evaluator-runtime|" +
                    Guid.NewGuid().ToString("N")));
        var issued = evaluator.IssueAsync(
                artifactRootBinding,
                pair,
                observedAtUtc)
            .GetAwaiter()
            .GetResult();
        if (!issued.IsSuccess || issued.Value is null)
        {
            throw new InvalidOperationException(
                issued.ErrorCode ??
                "TEST_INDEPENDENT_PREFERENCE_EVIDENCE_NOT_ISSUED");
        }

        return issued.Value;
    }

    private sealed class Client(string preferredDirection)
        : ILegalTeacherEvaluationInferenceClient
    {
        private readonly string _requestId =
            "test-independent-evaluation-transport:" +
            Guid.NewGuid().ToString("N");

        public Task<OperationResult<TeacherEvaluationInferenceResult>>
            EvaluateAsync(
                string prompt,
                ILegalTeacherCallPermit callPermit,
                CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var issued = callPermit.TryIssue();
            if (!issued.IsSuccess)
            {
                return Task.FromResult(
                    issued.MapError<TeacherEvaluationInferenceResult>());
            }

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
                preferred_direction = preferredDirection
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
                _requestId,
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

        public void Dispose()
        {
        }
    }
}
