// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Security.Cryptography;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ContinuousLearning;
using DesignConversionDomain;
using DPO;
using ModelServer.DesignConversion;
using TransformerModel;
using TransformerModel.Config;
using DesignDomain = DesignConversionDomain.DesignConversionDomain;

public sealed class DesignConversionModelActivationRouteRuntimeTests
{
    private const string TenantA = "tenant-design-conversion-route-alpha";
    private const string TenantB = "tenant-design-conversion-route-beta";
    private const string LocalToken = "route-runtime-local-token";
    private const string PreviousLocalToken = "route-runtime-previous-token";
    private const string UnknownLocalToken = "route-runtime-unknown-token";
    private const string RouteAdmissionCurrentToken = "route-runtime-admission-current-token";
    private const string RouteAdmissionPreviousToken = "route-runtime-admission-previous-token";
    private const string EvaluatedDpoPromotionEvidence =
        "DESIGN_CONVERSION_PROMOTION_GATE_FROM_VALIDATED_EVALUATED_DPO_CONTRACT";
    private static readonly Encoding StrictUtf8 = new UTF8Encoding(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);
    private static readonly JsonSerializerOptions PromotionJournalJsonOptions = new(
        DesignConversionCheckpointJsonOptions.Standard)
    {
        WriteIndented = false
    };

    [Fact]
    public async Task ActivatedPairAssociation_ExactSelectedInnerForwardsOnceAndReplayIsUnavailable()
    {
        var fixture = await BuildActivatedPairAssociationFixtureAsync(
            "exact-selected-inner");
        var association =
            new DesignConversionActivatedDpoPolicyPairRuntimeAssociation();
        var selected = new RecordingPairRuntimeEvaluator("selected-exact");
        association.Register(fixture.Evaluation, fixture.Request, selected);

        var applied = await association.ApplyAsync(
            fixture.Evaluation,
            fixture.Pair);
        Assert.True(applied.IsSuccess, applied.ErrorMessage);
        Assert.Equal(1, selected.CallCount);
        Assert.Same(fixture.Evaluation, selected.Evaluations[0]);
        Assert.Same(fixture.Pair, selected.Pairs[0]);

        var replay = await association.ApplyAsync(
            fixture.Evaluation,
            fixture.Pair);
        AssertPairAssociationUnavailable(replay);
        Assert.Equal(1, selected.CallCount);
    }

    [Fact]
    public async Task ActivatedPairAssociation_GatedCrossedCompletionSameJobKeepsExactSelectedRuntime()
    {
        var first = await BuildActivatedPairAssociationFixtureAsync(
            "crossed-same-job");
        var secondEvaluation = await EvaluatePairAssociationRequestAsync(
            first.Request);
        Assert.NotSame(first.Evaluation, secondEvaluation);
        Assert.Equal(first.Evaluation.JobId, secondEvaluation.JobId);
        var second = first with
        {
            Evaluation = secondEvaluation,
            Pair = BuildActivatedPairAssociationPair(
                secondEvaluation,
                first.RootJobId,
                first.RootRequestId,
                first.RootOwnershipBinding)
        };
        var association =
            new DesignConversionActivatedDpoPolicyPairRuntimeAssociation();
        var selectedFirst = new RecordingPairRuntimeEvaluator("selected-first");
        var selectedSecond = new RecordingPairRuntimeEvaluator("selected-second");
        var firstCompletionWaiting = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseFirstCompletion = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);

        var crossedFirstCompletion = Task.Run(async () =>
        {
            firstCompletionWaiting.SetResult();
            await releaseFirstCompletion.Task;
            association.Register(
                first.Evaluation,
                first.Request,
                selectedFirst);
        });
        await firstCompletionWaiting.Task;
        association.Register(
            second.Evaluation,
            second.Request,
            selectedSecond);
        releaseFirstCompletion.SetResult();
        await crossedFirstCompletion;

        var secondApplied = await association.ApplyAsync(
            second.Evaluation,
            second.Pair);
        Assert.True(secondApplied.IsSuccess, secondApplied.ErrorMessage);
        Assert.Equal(0, selectedFirst.CallCount);
        Assert.Equal(1, selectedSecond.CallCount);

        var firstApplied = await association.ApplyAsync(
            first.Evaluation,
            first.Pair);
        Assert.True(firstApplied.IsSuccess, firstApplied.ErrorMessage);
        Assert.Equal(1, selectedFirst.CallCount);
        Assert.Equal(1, selectedSecond.CallCount);
    }

    [Fact]
    public async Task ActivatedPairAssociation_CloneAndJsonReconstructionCannotBorrowExactResultCapability()
    {
        var fixture = await BuildActivatedPairAssociationFixtureAsync(
            "clone-json-reconstruction");
        var association =
            new DesignConversionActivatedDpoPolicyPairRuntimeAssociation();
        var selected = new RecordingPairRuntimeEvaluator("selected-original");
        association.Register(fixture.Evaluation, fixture.Request, selected);
        var clone = fixture.Evaluation with
        {
            Warnings = fixture.Evaluation.Warnings.ToArray()
        };
        var reconstructed = JsonSerializer.Deserialize<
            DesignConversionInternalEvaluationResult>(
                JsonSerializer.Serialize(fixture.Evaluation),
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        Assert.NotNull(reconstructed);
        Assert.NotSame(fixture.Evaluation, clone);
        Assert.NotSame(fixture.Evaluation, reconstructed);
        Assert.Equal(fixture.Evaluation.JobId, clone.JobId);
        Assert.Equal(fixture.Evaluation.JobId, reconstructed!.JobId);

        AssertPairAssociationUnavailable(
            await association.ApplyAsync(clone, fixture.Pair));
        AssertPairAssociationUnavailable(
            await association.ApplyAsync(reconstructed, fixture.Pair));
        Assert.Equal(0, selected.CallCount);

        var original = await association.ApplyAsync(
            fixture.Evaluation,
            fixture.Pair);
        Assert.True(original.IsSuccess, original.ErrorMessage);
        Assert.Equal(1, selected.CallCount);
    }

    [Fact]
    public async Task ActivatedPairAssociation_AbsentNonPairAndWrongRootNeverForwardAndWrongRootBurnsEntry()
    {
        var fixture = await BuildActivatedPairAssociationFixtureAsync(
            "absent-non-pair-wrong-root");
        var association =
            new DesignConversionActivatedDpoPolicyPairRuntimeAssociation();
        var neverSelected = new RecordingPairRuntimeEvaluator("never-selected");

        AssertPairAssociationUnavailable(
            await association.ApplyAsync(
                fixture.Evaluation,
                fixture.Pair));
        association.Register(fixture.Evaluation, fixture.Request, evaluator: null);
        AssertPairAssociationUnavailable(
            await association.ApplyAsync(
                fixture.Evaluation,
                fixture.Pair));
        Assert.Equal(0, neverSelected.CallCount);

        association.Register(
            fixture.Evaluation,
            fixture.Request,
            neverSelected);
        var wrongRoot = fixture.Pair with
        {
            RootOwnershipBinding = DesignConversionDpoLeakageGuard.HashRef(
                "dcm-runtime-rejected-root-ownership",
                "wrong-root|wrong-request")
        };
        AssertPairAssociationUnavailable(
            await association.ApplyAsync(
                fixture.Evaluation,
                wrongRoot));
        AssertPairAssociationUnavailable(
            await association.ApplyAsync(
                fixture.Evaluation,
                fixture.Pair));
        Assert.Equal(0, neverSelected.CallCount);
    }

    [Fact]
    public async Task ActivatedPairAssociation_WeakEntryDoesNotRetainUnconsumedResultOrEvictedEvaluator()
    {
        var fixture = await BuildActivatedPairAssociationFixtureAsync(
            "weak-cleanup");
        var association =
            new DesignConversionActivatedDpoPolicyPairRuntimeAssociation();
        var references = RegisterEphemeralPairAssociation(
            association,
            fixture);

        Assert.True(references.Evaluation.IsAlive);
        Assert.True(references.Evaluator.IsAlive);

        for (var attempt = 0;
             attempt < 5 &&
             (references.Evaluation.IsAlive || references.Evaluator.IsAlive);
             attempt++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        Assert.False(references.Evaluation.IsAlive);
        Assert.False(references.Evaluator.IsAlive);
    }

    [Fact]
    public async Task ActivatedRuntimeOuterPairWiring_ExactAnnotatedResultSelectsOneOfTwoDirectDpoRuntimesAndForwardsOnce()
    {
        var root = NewRoot();
        var modelA = await BuildActivationRecordAsync(
            root,
            TenantA,
            "outer-pair-direct-a",
            batchCount: 1,
            directDpoWeightSeed: 67);
        var modelB = await BuildActivationRecordAsync(
            root,
            TenantA,
            "outer-pair-direct-b",
            batchCount: 2,
            directDpoWeightSeed: 149);
        var loadA = await DesignConversionMachine.LoadCheckpointAsync(
            modelA.Model.CheckpointPath);
        var loadB = await DesignConversionMachine.LoadCheckpointAsync(
            modelB.Model.CheckpointPath);
        Assert.True(loadA.IsSuccess, loadA.ErrorMessage);
        Assert.True(loadB.IsSuccess, loadB.ErrorMessage);

        var artifactDbRoot = ArtifactDbRoot(root);
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(
            artifactDbRoot);
        var activationRegistry = new DesignConversionModelActivationRegistry(
            Path.Combine(root, "activation-state"),
            () => DateTimeOffset.Parse("2026-07-10T10:00:00Z"),
            allowedArtifactRootDirectory: artifactDbRoot);
        var activationApi = new DesignConversionModelServerModelActivationApi(
            activationRegistry,
            LocalToken,
            artifactDb);
        await ActivatePublishedModelAsync(
            activationApi,
            modelA,
            expectedGeneration: 0,
            "outer-pair-activate-a");

        var composition = DesignConversionLocalServiceComposition
            .CreateDeterministicLocalForLiveHostTesting();
        var loader = new RoutedCheckpointLoader(
            [loadA.Value!, loadB.Value!]);
        var activatedEvaluator =
            new DesignConversionActivatedModelRuntimeEvaluator(
                activationRegistry,
                composition.RenderProvider,
                composition.ComparisonProvider,
                composition.VisualJudge,
                loader);

        var evaluatedA = await activatedEvaluator.EvaluateAsync(
            CreateInternalEvaluateRequest(
                TenantA,
                "outer-pair-cache-a",
                BuildTrustedRouteAdmissionProvenance(TenantA)));
        Assert.True(evaluatedA.IsSuccess, evaluatedA.ErrorMessage);
        Assert.Equal(
            modelA.Model.ModelId,
            evaluatedA.Value!.ScaffoldMarkers["active_model_id"]);
        var recordingA = WrapCachedPairRuntimeEvaluatorForModel(
            activatedEvaluator,
            modelA.Model.ModelId);

        await ActivatePublishedModelAsync(
            activationApi,
            modelB,
            expectedGeneration: 1,
            "outer-pair-activate-b");
        var warmedB = await activatedEvaluator.EvaluateAsync(
            CreateInternalEvaluateRequest(
                TenantA,
                "outer-pair-cache-b",
                BuildTrustedRouteAdmissionProvenance(TenantA)));
        Assert.True(warmedB.IsSuccess, warmedB.ErrorMessage);
        Assert.Equal(
            modelB.Model.ModelId,
            warmedB.Value!.ScaffoldMarkers["active_model_id"]);
        var recordingB = WrapCachedPairRuntimeEvaluatorForModel(
            activatedEvaluator,
            modelB.Model.ModelId);
        var generatedRequest = CreateOuterPairGeneratedRequest(
            "b7824ce4f6f9ff3a8df5ecb9964c457b809f96822295c34b80fa690709f890b5");
        var evaluatedB = await activatedEvaluator.EvaluateAsync(
            generatedRequest);
        Assert.True(evaluatedB.IsSuccess, evaluatedB.ErrorMessage);
        var generated = evaluatedB.Value!;
        Assert.Equal(
            modelB.Model.ModelId,
            generated.ScaffoldMarkers["active_model_id"]);
        Assert.Equal(2, loader.Calls.Count);

        var pair = await BuildExactOuterRuntimePairAsync(
            generatedRequest,
            generated,
            preferredDirection:
                DesignConversionIndependentPreferenceEvidenceContract
                    .IncumbentSide);
        var policyPathA = DirectDpoPolicyPath(modelA);
        var policyPathB = DirectDpoPolicyPath(modelB);
        var expectedA = ComputeExpectedDirectPairScore(policyPathA, pair);
        var expectedB = ComputeExpectedDirectPairScore(policyPathB, pair);
        Assert.NotEqual(expectedA, expectedB);

        var pairRuntime = Assert.IsAssignableFrom<
            IDesignConversionDpoPolicyPairRuntimeEvaluator>(
                activatedEvaluator);
        var clone = generated with
        {
            Warnings = generated.Warnings.ToArray()
        };
        var reconstructed = JsonSerializer.Deserialize<
            DesignConversionInternalEvaluationResult>(
                JsonSerializer.Serialize(generated),
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        Assert.NotNull(reconstructed);
        Assert.NotSame(generated, clone);
        Assert.NotSame(generated, reconstructed);
        AssertPairAssociationUnavailable(
            await pairRuntime.ApplyDpoPolicyRuntimePairAsync(clone, pair));
        AssertPairAssociationUnavailable(
            await pairRuntime.ApplyDpoPolicyRuntimePairAsync(
                reconstructed!,
                pair));
        Assert.Equal(0, recordingA.PairCallCount);
        Assert.Equal(0, recordingB.PairCallCount);

        var applied = await pairRuntime.ApplyDpoPolicyRuntimePairAsync(
            generated,
            pair);
        Assert.True(applied.IsSuccess, applied.ErrorMessage);
        Assert.Equal(0, recordingA.PairCallCount);
        Assert.Equal(1, recordingB.PairCallCount);
        Assert.Same(generated, recordingB.PairEvaluations[0]);
        Assert.Same(pair, recordingB.Pairs[0]);
        Assert.Equal(
            expectedB,
            applied.Value!.RuntimeFlowResult.CriticOutput.Verdict.Score,
            precision: 6);

        var policyShaA = await ReadSha256Async(policyPathA);
        var policyShaB = await ReadSha256Async(policyPathB);
        Assert.Contains(
            $"dpo_policy_checkpoint_sha256:{policyShaB}",
            applied.Value.EvidenceRefs);
        Assert.DoesNotContain(
            $"dpo_policy_checkpoint_sha256:{policyShaA}",
            applied.Value.EvidenceRefs);
        Assert.Contains(
            "dpo_policy_pair_score_direction:generated_minus_incumbent",
            applied.Value.EvidenceRefs);
        Assert.Equal(
            DesignConversionIndependentPreferenceEvidenceContract
                .IncumbentSide,
            pair.IndependentEvaluation.PreferredDirection);
        Assert.Same(generated, pair.SourcePair.GeneratedEvaluation);
        Assert.Equal(
            pair.SourcePair.GeneratedEvaluation.SubmittedRequest.Html,
            pair.Generated.Html);
        Assert.Equal(
            pair.SourcePair.GeneratedEvaluation.SubmittedRequest.Css,
            pair.Generated.Css);
        Assert.Equal(
            pair.SourcePair.IncumbentEvaluation.SubmittedRequest.Html,
            pair.Incumbent.Html);
        Assert.Equal(
            pair.SourcePair.IncumbentEvaluation.SubmittedRequest.Css,
            pair.Incumbent.Css);
        Assert.Equal(
            DesignConversionDpoGenerator.Sha256Ref(
                DesignConversionDpoPolicyCheckpointBridge
                    .SerializePolicySideText(
                        pair.Generated.Html,
                        pair.Generated.Css)),
            pair.Generated.ContentSha256);
        Assert.Equal(
            DesignConversionDpoGenerator.Sha256Ref(
                DesignConversionDpoPolicyCheckpointBridge
                    .SerializePolicySideText(
                        pair.Incumbent.Html,
                        pair.Incumbent.Css)),
            pair.Incumbent.ContentSha256);
        Assert.Equal(
            DesignConversionDpoGenerator
                .BuildIndependentPreferenceEvaluationIdentity(generated),
            pair.Generated.EvaluationIdentity);
        Assert.Equal(
            DesignConversionDpoGenerator
                .BuildIndependentPreferenceEvaluationIdentity(
                    pair.SourcePair.IncumbentEvaluation),
            pair.Incumbent.EvaluationIdentity);

        AssertPairAssociationUnavailable(
            await pairRuntime.ApplyDpoPolicyRuntimePairAsync(
                generated,
                pair));
        Assert.Equal(0, recordingA.PairCallCount);
        Assert.Equal(1, recordingB.PairCallCount);
    }

    [Fact]
    public async Task ActivatedRuntimeOuterPairWiring_FailedUnregisteredAndNonPairSelectedEvaluatorsNeverForwardPair()
    {
        var root = NewRoot();
        var model = await BuildActivationRecordAsync(
            root,
            TenantA,
            "outer-pair-negative",
            batchCount: 1);
        var artifactDbRoot = ArtifactDbRoot(root);
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(
            artifactDbRoot);
        var activationRegistry = new DesignConversionModelActivationRegistry(
            Path.Combine(root, "activation-state"),
            () => DateTimeOffset.Parse("2026-07-10T10:00:00Z"),
            allowedArtifactRootDirectory: artifactDbRoot);
        var activationApi = new DesignConversionModelServerModelActivationApi(
            activationRegistry,
            LocalToken,
            artifactDb);
        await ActivatePublishedModelAsync(
            activationApi,
            model,
            expectedGeneration: 0,
            "outer-pair-negative-activate");

        var loaded = await DesignConversionMachine.LoadCheckpointAsync(
            model.Model.CheckpointPath);
        Assert.True(loaded.IsSuccess, loaded.ErrorMessage);
        var composition = DesignConversionLocalServiceComposition
            .CreateDeterministicLocalForLiveHostTesting();
        var loader = new RoutedCheckpointLoader([loaded.Value!]);
        var activatedEvaluator =
            new DesignConversionActivatedModelRuntimeEvaluator(
                activationRegistry,
                composition.RenderProvider,
                composition.ComparisonProvider,
                composition.VisualJudge,
                loader);
        var warmed = await activatedEvaluator.EvaluateAsync(
            CreateInternalEvaluateRequest(
                TenantA,
                "outer-pair-negative-warm",
                BuildTrustedRouteAdmissionProvenance(TenantA)));
        Assert.True(warmed.IsSuccess, warmed.ErrorMessage);

        var nonPairComposition = DesignConversionLocalServiceComposition
            .CreateDeterministicLocalForLiveHostTesting();
        var nonPair = new NonPairThinRuntimeEvaluator(
            nonPairComposition.Service);
        ReplaceOnlyCachedRuntimeEvaluator(activatedEvaluator, nonPair);
        var generatedRequest = CreateOuterPairGeneratedRequest(
            "b5f7b1d5c1ebec60c09e76cb825658710759fa40900284b0f723646c4945d782");
        var generated = await activatedEvaluator.EvaluateAsync(
            generatedRequest);
        Assert.True(generated.IsSuccess, generated.ErrorMessage);
        Assert.Equal(1, nonPair.EvaluateCallCount);
        Assert.False(typeof(IDesignConversionDpoPolicyPairRuntimeEvaluator)
            .IsAssignableFrom(nonPair.GetType()));
        var pair = await BuildExactOuterRuntimePairAsync(
            generatedRequest,
            generated.Value!);
        var outerPairRuntime = Assert.IsAssignableFrom<
            IDesignConversionDpoPolicyPairRuntimeEvaluator>(
                activatedEvaluator);
        AssertPairAssociationUnavailable(
            await outerPairRuntime.ApplyDpoPolicyRuntimePairAsync(
                generated.Value!,
                pair));
        Assert.Equal(1, nonPair.EvaluateCallCount);

        var failingLoader = new FailingCheckpointLoader(
            "INJECTED_OUTER_PAIR_RUNTIME_LOAD_FAILED");
        var failedEvaluator =
            new DesignConversionActivatedModelRuntimeEvaluator(
                activationRegistry,
                composition.RenderProvider,
                composition.ComparisonProvider,
                composition.VisualJudge,
                failingLoader);
        var failed = await failedEvaluator.EvaluateAsync(
            CreateOuterPairGeneratedRequest(
                "c5df4279407342a31ec03c701d2f982b75033fbcbf83d7e10fe69e443eca38cc"));
        Assert.False(failed.IsSuccess);
        Assert.Single(failingLoader.Calls);
        var unregistered = await BuildActivatedPairAssociationFixtureAsync(
            "outer-failed-unregistered");
        var failedPairRuntime = Assert.IsAssignableFrom<
            IDesignConversionDpoPolicyPairRuntimeEvaluator>(failedEvaluator);
        AssertPairAssociationUnavailable(
            await failedPairRuntime.ApplyDpoPolicyRuntimePairAsync(
                unregistered.Evaluation,
                unregistered.Pair));
    }

    [Fact]
    public async Task ModelActivationRoute_UnauthorizedMissingStartupConfigAndTokenRotationReturnBoundedResponses()
    {
        var root = NewRoot();
        var candidate = await BuildActivationRecordAsync(root, TenantA, "unauthorized", batchCount: 1);
        var readyRegistry = CreateRegistry(root);

        var unauthorized = await InvokeActivationAsync(
            readyRegistry,
            ActivateRequest(TenantA, "activate:unauthorized", 0, candidate),
            token: null);

        Assert.Equal(401, unauthorized.StatusCode);
        AssertErrorCode(unauthorized.BodyJson, "DESIGN_CONVERSION_MODEL_ACTIVATION_AUTH_TOKEN_REQUIRED");
        Assert.DoesNotContain(LocalToken, unauthorized.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(root, unauthorized.BodyJson, StringComparison.OrdinalIgnoreCase);

        var nonReadyRegistry = CreateRegistry(root, configureActivation: false);
        var nonReady = await InvokeActivationAsync(
            nonReadyRegistry,
            ActivateRequest(TenantA, "activate:not-ready", 0, candidate),
            token: LocalToken);

        Assert.Equal(503, nonReady.StatusCode);
        AssertErrorCode(nonReady.BodyJson, "DESIGN_CONVERSION_MODEL_ACTIVATION_STATE_ROOT_NOT_CONFIGURED");
        using var document = JsonDocument.Parse(nonReady.BodyJson);
        var metadata = document.RootElement.GetProperty("boundaryMetadata");
        Assert.Equal("bounded_non_ready", metadata.GetProperty("model_activation_route_startup_configuration_status").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("private_file_paths_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("secret_values_included").GetString());
        Assert.DoesNotContain(LocalToken, nonReady.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(root, nonReady.BodyJson, StringComparison.OrdinalIgnoreCase);

        var rotationNow = DateTimeOffset.Parse("2026-07-10T10:00:00Z");
        var previousExpiresAt = rotationNow.AddMinutes(5);
        var rotationOptions = new DesignConversionModelServerModelLifecycleTokenKeyRingOptions(
            LocalToken,
            PreviousLocalToken,
            previousExpiresAt);
        var rotationRegistry = CreateRegistry(
            root,
            utcNow: () => rotationNow,
            modelLifecycleTokenKeyRing: rotationOptions);

        var currentAccepted = await InvokeActivationAsync(
            rotationRegistry,
            ActivateRequest(TenantA, "activate:rotation-current", 0, candidate),
            token: LocalToken);
        AssertSuccessActivation(currentAccepted.BodyJson, candidate.Model.ModelId, expectedGeneration: 1, replay: false);
        AssertDoesNotExposeLifecycleTokens(currentAccepted.BodyJson);

        var previousAcceptedBeforeExpiry = await InvokeActivationAsync(
            rotationRegistry,
            ReadActiveRequest(TenantA),
            token: PreviousLocalToken);
        Assert.Equal(200, previousAcceptedBeforeExpiry.StatusCode);
        AssertDoesNotExposeLifecycleTokens(previousAcceptedBeforeExpiry.BodyJson);

        var unknownRejected = await InvokeActivationAsync(
            rotationRegistry,
            ReadActiveRequest(TenantA),
            token: UnknownLocalToken);
        Assert.Equal(403, unknownRejected.StatusCode);
        AssertErrorCode(unknownRejected.BodyJson, "DESIGN_CONVERSION_MODEL_ACTIVATION_AUTH_TOKEN_INVALID");
        AssertDoesNotExposeLifecycleTokens(unknownRejected.BodyJson);

        var expiryRegistry = CreateRegistry(
            root,
            utcNow: () => previousExpiresAt,
            modelLifecycleTokenKeyRing: rotationOptions);
        var previousRejectedAtExpiry = await InvokeActivationAsync(
            expiryRegistry,
            ReadActiveRequest(TenantA),
            token: PreviousLocalToken);
        Assert.Equal(403, previousRejectedAtExpiry.StatusCode);
        AssertErrorCode(previousRejectedAtExpiry.BodyJson, "DESIGN_CONVERSION_MODEL_ACTIVATION_AUTH_TOKEN_INVALID");
        AssertDoesNotExposeLifecycleTokens(previousRejectedAtExpiry.BodyJson);

        var afterExpiryRegistry = CreateRegistry(
            root,
            utcNow: () => previousExpiresAt.AddTicks(1),
            modelLifecycleTokenKeyRing: rotationOptions);
        var previousRejectedAfterExpiry = await InvokeActivationAsync(
            afterExpiryRegistry,
            ReadActiveRequest(TenantA),
            token: PreviousLocalToken);
        Assert.Equal(403, previousRejectedAfterExpiry.StatusCode);
        AssertErrorCode(previousRejectedAfterExpiry.BodyJson, "DESIGN_CONVERSION_MODEL_ACTIVATION_AUTH_TOKEN_INVALID");
        AssertDoesNotExposeLifecycleTokens(previousRejectedAfterExpiry.BodyJson);

        await AssertInvalidKeyRingConfigReturnsNonReadyAsync(
            root,
            new DesignConversionModelServerModelLifecycleTokenKeyRingOptions(
                LocalToken,
                PreviousLocalToken),
            "DESIGN_CONVERSION_MODEL_ACTIVATION_AUTH_TOKEN_PREVIOUS_EXPIRATION_REQUIRED",
            LocalToken,
            PreviousLocalToken);
        await AssertInvalidKeyRingConfigReturnsNonReadyAsync(
            root,
            new DesignConversionModelServerModelLifecycleTokenKeyRingOptions(
                LocalToken,
                previousTokenExpiresAtUtc: previousExpiresAt),
            "DESIGN_CONVERSION_MODEL_ACTIVATION_AUTH_TOKEN_PREVIOUS_REQUIRED",
            LocalToken);
        await AssertInvalidKeyRingConfigReturnsNonReadyAsync(
            root,
            new DesignConversionModelServerModelLifecycleTokenKeyRingOptions(string.Empty),
            "DESIGN_CONVERSION_MODEL_ACTIVATION_AUTH_TOKEN_CURRENT_REQUIRED");
        await AssertInvalidKeyRingConfigReturnsNonReadyAsync(
            root,
            new DesignConversionModelServerModelLifecycleTokenKeyRingOptions("short"),
            "DESIGN_CONVERSION_MODEL_ACTIVATION_AUTH_TOKEN_CURRENT_INVALID",
            "short");
        await AssertInvalidKeyRingConfigReturnsNonReadyAsync(
            root,
            new DesignConversionModelServerModelLifecycleTokenKeyRingOptions(
                LocalToken,
                LocalToken,
                previousExpiresAt),
            "DESIGN_CONVERSION_MODEL_ACTIVATION_AUTH_TOKEN_CURRENT_PREVIOUS_MATCH",
            LocalToken);
        await AssertInvalidKeyRingConfigReturnsNonReadyAsync(
            root,
            new DesignConversionModelServerModelLifecycleTokenKeyRingOptions(
                LocalToken,
                "route-runtime-\u0001previous-token",
                previousExpiresAt),
            "DESIGN_CONVERSION_MODEL_ACTIVATION_AUTH_TOKEN_PREVIOUS_INVALID",
            LocalToken,
            "route-runtime-\u0001previous-token");
    }

    [Fact]
    public async Task ModelActivationRoute_CrossTenantCandidateReturnsConflict()
    {
        var root = NewRoot();
        var tenantARecord = await BuildActivationRecordAsync(root, TenantA, "tenant-a", batchCount: 1);
        var registry = CreateRegistry(root);

        var response = await InvokeActivationAsync(
            registry,
            ActivateRequest(TenantB, "activate:cross-tenant", 0, tenantARecord));

        Assert.Equal(409, response.StatusCode);
        AssertErrorCode(response.BodyJson, "DESIGN_CONVERSION_MODEL_ACTIVATION_TENANT_SCOPE_MISMATCH");
        Assert.DoesNotContain(root, response.BodyJson, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ModelActivationHostStartup_EvaluateWithoutActivationConfigurationFailsClosedWith503()
    {
        var root = NewRoot();
        var requestId = "host-startup-no-activation";
        var registry = CreateRegistry(
            root,
            configureActivation: false,
            routeAdmissionTenantId: TenantA);

        var evaluated = await InvokeEvaluateAsync(registry, TenantA, requestId);

        AssertActiveModelNotReadyFailedClosed(
            evaluated,
            root,
            TenantA,
            requestId,
            "<main><h1>Route runtime activation</h1></main>",
            "body{margin:0;color:#123456;background:#f8fafc}",
            "{\"kind\":\"route-runtime-design\"}",
            "{\"kind\":\"route-runtime-original\"}",
            "activation-route-runtime-user");
    }

    [Fact]
    public async Task ModelActivationRuntime_BodyTenantWithoutTrustedAdmissionFailsClosedWithoutFallbackResult()
    {
        var root = NewRoot();
        var modelB = await BuildActivationRecordAsync(root, TenantB, "body-spoof-model-b", batchCount: 2);
        var registry = CreateRegistry(root);

        var activated = await InvokeActivationAsync(
            registry,
            ActivateRequest(TenantB, "activate:body-spoof-b", 0, modelB));
        AssertSuccessActivation(activated.BodyJson, modelB.Model.ModelId, expectedGeneration: 1, replay: false);

        var evaluated = await InvokeEvaluateAsync(
            registry,
            TenantB,
            "evaluate-body-only-spoof",
            routeAdmissionToken: null,
            includeTrustedVisualCostMetadata: false);

        AssertActiveModelNotReadyFailedClosed(
            evaluated,
            root,
            TenantB,
            modelB.Model.ModelId,
            modelB.Model.CheckpointPath,
            "<main><h1>Route runtime activation</h1></main>",
            "body{margin:0;color:#123456;background:#f8fafc}");
        Assert.DoesNotContain(TenantB, evaluated.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(modelB.Model.CheckpointPath, evaluated.BodyJson, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ModelActivationRuntime_DirectApiCallerTenantMetadataSpoofFailsClosedWithoutSemanticResult()
    {
        var root = NewRoot();
        var modelB = await BuildActivationRecordAsync(root, TenantB, "direct-api-spoof-model-b", batchCount: 2);
        var composition = DesignConversionLocalServiceComposition.CreateDeterministicLocalForLiveHostTesting();
        var artifactDbRoot = ArtifactDbRoot(root);
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(artifactDbRoot);
        var visualCostLedger = new JsonlDesignConversionVisualCostLedger(
            Path.Combine(root, "visual-cost-ledger"));
        var activationRegistry = new DesignConversionModelActivationRegistry(
            Path.Combine(root, "activation-state"),
            () => DateTimeOffset.Parse("2026-07-10T10:00:00Z"),
            allowedArtifactRootDirectory: artifactDbRoot);
        var activationApi = new DesignConversionModelServerModelActivationApi(
            activationRegistry,
            LocalToken,
            artifactDb);

        var activated = await activationApi.ActivatePublishedModelAsync(
            new DesignConversionModelServerModelActivationAuthorization(
                TenantId: TenantB,
                RequestedByRef: "architect:design-conversion-model-activation-route-runtime-test",
                Reason: "direct API tenant metadata spoof setup",
                IdempotencyKey: "activate:direct-api-spoof-b",
                ExpectedGeneration: 0,
                LocalAuthorizationToken: LocalToken),
            modelB,
            expectedModelVersion: modelB.Model.Version,
            requestedAtUtc: DateTimeOffset.Parse("2026-07-10T10:00:00Z"));
        Assert.True(activated.IsSuccess, activated.ErrorMessage);

        var activatedEvaluator = new DesignConversionActivatedModelRuntimeEvaluator(
            activationRegistry,
            composition.RenderProvider,
            composition.ComparisonProvider,
            composition.VisualJudge);
        var api = new DesignConversionModelServerApi(
            activatedEvaluator,
            () => DateTimeOffset.Parse("2026-07-10T10:00:00Z"),
            artifactDb,
            trainingReadbackApi: new DesignConversionDpoUsefulnessApi(artifactDb),
            activatedModelRuntimeReadback: activatedEvaluator,
            visualCostLedger: visualCostLedger);

        var result = await api.SubmitAndEvaluateAsync(
            CreateEvaluateRequest(
                TenantB,
                "direct-api-body-spoof-b",
                includeTrustedVisualCostMetadata: false),
            routeAdmissionContext: null);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            DesignConversionActivatedModelRuntimeEvaluator.ActiveModelNotReadyErrorCode,
            result.ErrorCode);
        Assert.DoesNotContain(TenantB, result.ErrorMessage ?? string.Empty, StringComparison.Ordinal);
        Assert.DoesNotContain(modelB.Model.ModelId, result.ErrorMessage ?? string.Empty, StringComparison.Ordinal);
        Assert.DoesNotContain(modelB.Model.CheckpointPath, result.ErrorMessage ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ModelActivationRuntime_AdmittedTenantWithoutActiveVersionFailsClosedWith503()
    {
        var root = NewRoot();
        var registry = CreateRegistry(root, routeAdmissionTenantId: TenantA);

        var evaluated = await InvokeEvaluateAsync(
            registry,
            TenantA,
            "evaluate-admitted-a-no-active-version");

        AssertActiveModelNotReadyFailedClosed(
            evaluated,
            root,
            TenantA,
            "<main><h1>Route runtime activation</h1></main>",
            "body{margin:0;color:#123456;background:#f8fafc}");
    }

    [Fact]
    public async Task ModelActivationRuntime_ActiveVersionNotFoundFailsClosed()
    {
        var root = NewRoot();
        var composition = DesignConversionLocalServiceComposition.CreateDeterministicLocalForLiveHostTesting();
        var artifactDbRoot = ArtifactDbRoot(root);
        var activationRegistry = new DesignConversionModelActivationRegistry(
            Path.Combine(root, "activation-state"),
            () => DateTimeOffset.Parse("2026-07-10T10:00:00Z"),
            allowedArtifactRootDirectory: artifactDbRoot);
        var activatedEvaluator = new DesignConversionActivatedModelRuntimeEvaluator(
            activationRegistry,
            composition.RenderProvider,
            composition.ComparisonProvider,
            composition.VisualJudge);

        var evaluated = await activatedEvaluator.EvaluateAsync(CreateInternalEvaluateRequest(
            TenantA,
            "direct-admitted-no-active-version",
            BuildTrustedRouteAdmissionProvenance(TenantA)));

        Assert.False(evaluated.IsSuccess);
        Assert.Equal(
            DesignConversionActivatedModelRuntimeEvaluator.ActiveModelNotReadyErrorCode,
            evaluated.ErrorCode);
    }

    [Fact]
    public async Task ModelActivationRuntime_AdmittedTenantMismatchRejectsBeforeRuntimeOrPersistence()
    {
        var root = NewRoot();
        var modelA = await BuildActivationRecordAsync(root, TenantA, "mismatch-model-a", batchCount: 1);
        var registry = CreateRegistry(root, routeAdmissionTenantId: TenantA);

        var activated = await InvokeActivationAsync(
            registry,
            ActivateRequest(TenantA, "activate:mismatch-a", 0, modelA));
        AssertSuccessActivation(activated.BodyJson, modelA.Model.ModelId, expectedGeneration: 1, replay: false);

        var evaluated = await InvokeEvaluateAsync(
            registry,
            TenantB,
            "evaluate-admitted-a-body-b");

        Assert.Equal(403, evaluated.StatusCode);
        AssertErrorCode(evaluated.BodyJson, "DESIGN_CONVERSION_ACTIVE_MODEL_TENANT_CONTEXT_MISMATCH");
        Assert.DoesNotContain(TenantA, evaluated.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(TenantB, evaluated.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(modelA.Model.ModelId, evaluated.BodyJson, StringComparison.Ordinal);
        using var document = JsonDocument.Parse(evaluated.BodyJson);
        var metadata = document.RootElement.GetProperty("boundaryMetadata");
        Assert.Equal(bool.FalseString, metadata.GetProperty("runtime_invoked").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("persistence_invoked").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("route_admission_tenant_bound").GetString());
        Assert.Equal(
            DesignConversionModelActivationRegistry.BuildTenantIdRef(TenantA),
            metadata.GetProperty("route_admission_tenant_id_ref").GetString());
    }

    [Fact]
    public async Task ModelActivationRuntime_ActivateBThenInferenceAndReadbackUseB()
    {
        var root = NewRoot();
        var modelB = await BuildActivationRecordAsync(root, TenantB, "model-b", batchCount: 2);
        var registry = CreateRegistry(root, routeAdmissionTenantId: TenantB);

        var activated = await InvokeActivationAsync(
            registry,
            ActivateRequest(TenantB, "activate:b", 0, modelB));
        AssertSuccessActivation(
            activated.BodyJson,
            modelB.Model.ModelId,
            expectedGeneration: 1,
            replay: false,
            modelB.Model.LocalCheckpointWeightsSource);

        var evaluated = await InvokeEvaluateAsync(registry, TenantB, "evaluate-tenant-b");
        Assert.Equal(200, evaluated.StatusCode);
        AssertEvaluateSelectedModel(evaluated.BodyJson, modelB.Model.ModelId, modelB.Model.LocalCheckpointWeightsSource);
        Assert.DoesNotContain(TenantB, evaluated.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(modelB.Model.CheckpointPath, evaluated.BodyJson, StringComparison.OrdinalIgnoreCase);

        var readback = await InvokeActivationAsync(
            registry,
            new DesignConversionModelServerModelActivationRequest(
                Action: DesignConversionModelServerModelActivationHandler.ActionReadActive,
                TenantId: TenantB));
        AssertSuccessActivation(
            readback.BodyJson,
            modelB.Model.ModelId,
            expectedGeneration: 1,
            replay: false,
            modelB.Model.LocalCheckpointWeightsSource);
        Assert.DoesNotContain(root, readback.BodyJson, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ModelActivationRoute_DpoCheckpointWeightsSourceFlowsThroughActivationAndRuntimeMetadata()
    {
        var root = NewRoot();
        var dpoModel = await BuildActivationRecordAsync(
            root,
            TenantA,
            "dpo-weights-source",
            batchCount: 1,
            directDpoWeightSeed: 211);
        var registry = CreateRegistry(root, routeAdmissionTenantId: TenantA);

        Assert.Equal(
            "11.DPO/DpoTrainer:dpo_policy_checkpoint_converted",
            dpoModel.Model.LocalCheckpointWeightsSource);
        Assert.Matches(
            "^dpo_policy_checkpoint_sha256:[0-9a-f]{64}$",
            dpoModel.Model.DpoPolicyCheckpointSha256Ref);
        Assert.Equal(
            "dpo_policy_checkpoint_file:dpo_policy_step_000001.bin",
            dpoModel.Model.DpoPolicyCheckpointFileRef);

        var activated = await InvokeActivationAsync(
            registry,
            ActivateRequest(TenantA, "activate:dpo-weights-source", 0, dpoModel));

        AssertSuccessActivation(
            activated.BodyJson,
            dpoModel.Model.ModelId,
            expectedGeneration: 1,
            replay: false,
            dpoModel.Model.LocalCheckpointWeightsSource);
        Assert.DoesNotContain(dpoModel.Model.CheckpointPath, activated.BodyJson, StringComparison.OrdinalIgnoreCase);

        var evaluated = await InvokeEvaluateAsync(registry, TenantA, "evaluate-dpo-weights-source");
        Assert.Equal(200, evaluated.StatusCode);
        AssertEvaluateSelectedDpoCheckpointModel(
            evaluated.BodyJson,
            dpoModel.Model.ModelId,
            dpoModel.Model.LocalCheckpointWeightsSource);
        AssertRuntimeSelection(evaluated.BodyJson, loadedFresh: true, preservedPrevious: false);
        Assert.DoesNotContain(dpoModel.Model.CheckpointPath, evaluated.BodyJson, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ModelActivationRuntime_UsesInjectedCheckpointLoaderForFreshActiveVersionLoad()
    {
        var root = NewRoot();
        var modelA = await BuildActivationRecordAsync(root, TenantA, "injected-loader-model-a", batchCount: 1);
        var composition = DesignConversionLocalServiceComposition.CreateDeterministicLocalForLiveHostTesting();
        var artifactDbRoot = ArtifactDbRoot(root);
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(artifactDbRoot);
        var activationRegistry = new DesignConversionModelActivationRegistry(
            Path.Combine(root, "activation-state"),
            () => DateTimeOffset.Parse("2026-07-10T10:00:00Z"),
            allowedArtifactRootDirectory: artifactDbRoot);
        var activationApi = new DesignConversionModelServerModelActivationApi(
            activationRegistry,
            LocalToken,
            artifactDb);

        var realLoad = await DesignConversionMachine.LoadCheckpointAsync(modelA.Model.CheckpointPath);
        Assert.True(realLoad.IsSuccess, realLoad.ErrorMessage);

        var activated = await activationApi.ActivatePublishedModelAsync(
            new DesignConversionModelServerModelActivationAuthorization(
                TenantId: TenantA,
                RequestedByRef: "architect:design-conversion-model-activation-route-runtime-test",
                Reason: "injected checkpoint loader route runtime setup",
                IdempotencyKey: "activate:injected-loader-a",
                ExpectedGeneration: 0,
                LocalAuthorizationToken: LocalToken),
            modelA,
            expectedModelVersion: modelA.Model.Version,
            requestedAtUtc: DateTimeOffset.Parse("2026-07-10T10:00:00Z"));
        Assert.True(activated.IsSuccess, activated.ErrorMessage);

        var active = await activationRegistry.ReadActiveAsync(TenantA);
        Assert.True(active.IsSuccess, active.ErrorMessage);
        var expectedActiveVersion = active.Value!.ActiveVersion;
        Assert.Equal(modelA.Model.ModelId, expectedActiveVersion.ModelId);

        await File.WriteAllTextAsync(
            modelA.Model.CheckpointPath,
            "{\"checkpoint\":\"corrupt-local-path-after-activation\"",
            StrictUtf8);

        var loader = new RecordingCheckpointLoader(realLoad.Value!);
        var activatedEvaluator = new DesignConversionActivatedModelRuntimeEvaluator(
            activationRegistry,
            composition.RenderProvider,
            composition.ComparisonProvider,
            composition.VisualJudge,
            loader);
        var requestId = "evaluate-injected-loader-a";
        var internalRequest = CreateInternalEvaluateRequest(
            TenantA,
            requestId,
            BuildTrustedRouteAdmissionProvenance(TenantA));
        using var evaluateCts = new CancellationTokenSource();
        var directLocalReadCount = 0;
        OperationResult<DesignConversionInternalEvaluationResult> evaluated;
        using (DesignConversionMachine.UseCheckpointByteReaderForCurrentAsyncFlow(
                   async (requestedPath, cancellationToken) =>
                   {
                       directLocalReadCount++;
                       Assert.Equal(
                           Path.GetFullPath(modelA.Model.CheckpointPath),
                           Path.GetFullPath(requestedPath));
                       return await File.ReadAllBytesAsync(requestedPath, cancellationToken);
                   }))
        {
            evaluated = await activatedEvaluator.EvaluateAsync(internalRequest, evaluateCts.Token);
        }

        Assert.True(evaluated.IsSuccess, evaluated.ErrorMessage);
        var result = evaluated.Value!;
        Assert.Equal(modelA.Model.ModelId, result.ScaffoldMarkers["active_model_id"]);
        Assert.Equal(modelA.Model.CheckpointSha256, result.ScaffoldMarkers["active_model_checkpoint_sha256"]);
        Assert.Equal(
            modelA.Model.LocalCheckpointWeightsSource,
            result.ScaffoldMarkers["active_model_checkpoint_weights_source"]);
        Assert.Equal(bool.TrueString, result.ScaffoldMarkers["active_model_runtime_fresh_load_succeeded"]);
        Assert.Equal(bool.FalseString, result.ScaffoldMarkers["active_model_runtime_previous_preserved"]);
        Assert.Contains(DesignConversionActivatedModelRuntimeEvaluator.ActiveSelectionEvidence, result.EvidenceRefs);
        Assert.Contains(DesignConversionActivatedModelRuntimeEvaluator.FreshLoadEvidence, result.EvidenceRefs);
        Assert.DoesNotContain(
            DesignConversionRepositoryActivatedModelCheckpointLoader.RepositoryFreshLoadEvidence,
            result.EvidenceRefs);
        Assert.Equal(0, directLocalReadCount);

        var call = Assert.Single(loader.Calls);
        Assert.Equal(expectedActiveVersion, call.ActiveVersion);
        Assert.Equal(evaluateCts.Token, call.CancellationToken);
    }

    [Theory]
    [InlineData(DesignConversionRepositoryActivatedModelCheckpointLoader.RepositoryFreshLoadEvidence)]
    [InlineData("INJECTED_LOADER_FAILURE:" + DesignConversionRepositoryActivatedModelCheckpointLoader.RepositoryFreshLoadEvidence + ":COMPOSITE")]
    public async Task ModelActivationRuntime_InjectedLoaderCannotExposeReservedRepositoryEvidenceThroughErrorCode(
        string injectedErrorCode)
    {
        var root = NewRoot();
        var modelA = await BuildActivationRecordAsync(root, TenantA, "injected-error-code-model-a", batchCount: 1);
        var composition = DesignConversionLocalServiceComposition.CreateDeterministicLocalForLiveHostTesting();
        var artifactDbRoot = ArtifactDbRoot(root);
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(artifactDbRoot);
        var activationRegistry = new DesignConversionModelActivationRegistry(
            Path.Combine(root, "activation-state"),
            () => DateTimeOffset.Parse("2026-07-10T10:00:00Z"),
            allowedArtifactRootDirectory: artifactDbRoot);
        var activationApi = new DesignConversionModelServerModelActivationApi(
            activationRegistry,
            LocalToken,
            artifactDb);
        var activated = await activationApi.ActivatePublishedModelAsync(
            new DesignConversionModelServerModelActivationAuthorization(
                TenantId: TenantA,
                RequestedByRef: "architect:design-conversion-model-activation-route-runtime-test",
                Reason: "adversarial injected checkpoint loader error-code setup",
                IdempotencyKey: "activate:injected-error-code-a",
                ExpectedGeneration: 0,
                LocalAuthorizationToken: LocalToken),
            modelA,
            expectedModelVersion: modelA.Model.Version,
            requestedAtUtc: DateTimeOffset.Parse("2026-07-10T10:00:00Z"));
        Assert.True(activated.IsSuccess, activated.ErrorMessage);

        var loader = new FailingCheckpointLoader(injectedErrorCode);
        var evaluator = new DesignConversionActivatedModelRuntimeEvaluator(
            activationRegistry,
            composition.RenderProvider,
            composition.ComparisonProvider,
            composition.VisualJudge,
            loader);
        var registry = CreateEvaluateRegistry(root, artifactDb, evaluator, routeAdmissionTenantId: TenantA);

        var response = await InvokeEvaluateAsync(registry, TenantA, "evaluate-injected-error-code-a");

        AssertActiveRuntimeLoadFailedClosed(
            response,
            "DESIGN_CONVERSION_ACTIVE_MODEL_CHECKPOINT_LOAD_FAILED",
            expectedRuntimeInvoked: false,
            DesignConversionRepositoryActivatedModelCheckpointLoader.RepositoryFreshLoadEvidence,
            injectedErrorCode,
            modelA.Model.CheckpointPath,
            TenantA);
        Assert.DoesNotContain(
            DesignConversionRepositoryActivatedModelCheckpointLoader.RepositoryFreshLoadEvidence,
            response.BodyJson,
            StringComparison.Ordinal);
        Assert.Single(loader.Calls);
    }

    [Fact]
    public async Task ModelActivationRuntime_FreshLoadedCheckpointWithoutRealWeightsFailsClosedBeforeRuntime()
    {
        var root = NewRoot();
        var modelA = await BuildActivationRecordAsync(root, TenantA, "missing-real-weights-model-a", batchCount: 1);
        var composition = DesignConversionLocalServiceComposition.CreateDeterministicLocalForLiveHostTesting();
        var artifactDbRoot = ArtifactDbRoot(root);
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(artifactDbRoot);
        var activationRegistry = new DesignConversionModelActivationRegistry(
            Path.Combine(root, "activation-state"),
            () => DateTimeOffset.Parse("2026-07-10T10:00:00Z"),
            allowedArtifactRootDirectory: artifactDbRoot);
        var activationApi = new DesignConversionModelServerModelActivationApi(
            activationRegistry,
            LocalToken,
            artifactDb);
        var realLoad = await DesignConversionMachine.LoadCheckpointAsync(modelA.Model.CheckpointPath);
        Assert.True(realLoad.IsSuccess, realLoad.ErrorMessage);
        Assert.True(DesignConversionMachine.CheckpointHasRealLearnedWeights(realLoad.Value!.Checkpoint));

        var activated = await activationApi.ActivatePublishedModelAsync(
            new DesignConversionModelServerModelActivationAuthorization(
                TenantId: TenantA,
                RequestedByRef: "architect:design-conversion-model-activation-route-runtime-test",
                Reason: "fresh loaded checkpoint without real weights route runtime setup",
                IdempotencyKey: "activate:missing-real-weights-a",
                ExpectedGeneration: 0,
                LocalAuthorizationToken: LocalToken),
            modelA,
            expectedModelVersion: modelA.Model.Version,
            requestedAtUtc: DateTimeOffset.Parse("2026-07-10T10:00:00Z"));
        Assert.True(activated.IsSuccess, activated.ErrorMessage);

        await TamperActiveCheckpointWeightsSourceAsync(root, "none");

        var active = await activationRegistry.ReadActiveAsync(TenantA);
        Assert.True(active.IsSuccess, active.ErrorMessage);
        var expectedActiveVersion = active.Value!.ActiveVersion;
        Assert.Equal(modelA.Model.ModelId, expectedActiveVersion.ModelId);
        Assert.True(expectedActiveVersion.CheckpointContainsRealTrainedWeights);
        Assert.Equal(modelA.Model.CheckpointSha256, expectedActiveVersion.CheckpointSha256);
        Assert.Equal("none", expectedActiveVersion.LocalCheckpointWeightsSource);

        var noRealWeightsLoad = realLoad.Value with
        {
            Checkpoint = RemoveRealLearnedWeights(realLoad.Value.Checkpoint)
        };
        Assert.Equal(modelA.Model.CheckpointSha256, noRealWeightsLoad.CheckpointSha256);
        Assert.False(DesignConversionMachine.CheckpointHasRealLearnedWeights(noRealWeightsLoad.Checkpoint));

        var loader = new RecordingCheckpointLoader(noRealWeightsLoad);
        var activatedEvaluator = new DesignConversionActivatedModelRuntimeEvaluator(
            activationRegistry,
            composition.RenderProvider,
            composition.ComparisonProvider,
            composition.VisualJudge,
            loader);
        var registry = CreateEvaluateRegistry(
            root,
            artifactDb,
            activatedEvaluator,
            routeAdmissionTenantId: TenantA);
        var requestId = "evaluate-missing-real-weights-a";

        var evaluated = await InvokeEvaluateAsync(registry, TenantA, requestId);

        AssertActiveRuntimeLoadFailedClosed(
            evaluated,
            DesignConversionActivatedModelRuntimeEvaluator.ActiveModelCheckpointRealWeightsRequiredErrorCode,
            expectedRuntimeInvoked: false,
            modelA.Model.ModelId,
            modelA.Model.CheckpointPath,
            modelA.Model.CheckpointSha256,
            TenantA);
        Assert.DoesNotContain(
            DesignConversionActivatedModelRuntimeEvaluator.PreviousRuntimePreservedEvidence,
            evaluated.BodyJson,
            StringComparison.Ordinal);
        Assert.DoesNotContain("active_model_runtime_previous_preserved", evaluated.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(
            DesignConversionRepositoryActivatedModelCheckpointLoader.RepositoryFreshLoadEvidence,
            evaluated.BodyJson,
            StringComparison.Ordinal);
        Assert.False(activatedEvaluator.TryGetPromotedLocalModel($"job:{requestId}", out var promotedLocalModel));
        Assert.Null(promotedLocalModel);
        var call = Assert.Single(loader.Calls);
        Assert.Equal(expectedActiveVersion, call.ActiveVersion);
    }

    [Fact]
    public async Task ModelActivationRuntime_SameActiveSelectionUsesKnownGoodCacheWithoutReloadingCheckpoint()
    {
        var root = NewRoot();
        var modelA = await BuildActivationRecordAsync(root, TenantA, "snapshot-model-a", batchCount: 1);
        var modelB = await BuildActivationRecordAsync(root, TenantA, "snapshot-model-b", batchCount: 2);
        var registry = CreateRegistry(root, routeAdmissionTenantId: TenantA);

        var activated = await InvokeActivationAsync(
            registry,
            ActivateRequest(TenantA, "activate:snapshot-a", 0, modelA));
        AssertSuccessActivation(activated.BodyJson, modelA.Model.ModelId, expectedGeneration: 1, replay: false);
        var activationARef = ReadActiveActivationRef(activated.BodyJson);

        var cached = await InvokeEvaluateAsync(registry, TenantA, "evaluate-snapshot-cache-a");
        Assert.Equal(200, cached.StatusCode);
        AssertEvaluateSelectedModel(cached.BodyJson, modelA.Model.ModelId);
        AssertRuntimeSelection(cached.BodyJson, loadedFresh: true, preservedPrevious: false);
        AssertEvaluationPromotedModelIdentity(cached.BodyJson, modelA.Model.ModelId, modelA.Model.CheckpointSha256);

        var replacementBytes = await File.ReadAllBytesAsync(modelB.Model.CheckpointPath);
        Assert.NotEqual(modelA.Model.ModelId, modelB.Model.ModelId);
        Assert.NotEqual(modelA.Model.CheckpointSha256, modelB.Model.CheckpointSha256);
        var readCount = 0;
        using (DesignConversionMachine.UseCheckpointByteReaderForCurrentAsyncFlow(
                   async (requestedPath, cancellationToken) =>
                    {
                        readCount++;
                        Assert.Equal(Path.GetFullPath(modelA.Model.CheckpointPath), Path.GetFullPath(requestedPath));
                        await File.WriteAllBytesAsync(requestedPath, replacementBytes, cancellationToken);
                        return replacementBytes;
                    }))
        {
            var preservedWithoutReload = await InvokeEvaluateAsync(registry, TenantA, "evaluate-snapshot-cache-a-no-reload");
            Assert.Equal(200, preservedWithoutReload.StatusCode);
            AssertEvaluateSelectedModel(preservedWithoutReload.BodyJson, modelA.Model.ModelId);
            AssertRuntimeSelection(
                preservedWithoutReload.BodyJson,
                loadedFresh: false,
                preservedPrevious: true,
                freshLoadAttempted: false);
            AssertEvaluationPromotedModelIdentity(
                preservedWithoutReload.BodyJson,
                modelA.Model.ModelId,
                modelA.Model.CheckpointSha256);
            Assert.DoesNotContain(modelB.Model.ModelId, preservedWithoutReload.BodyJson, StringComparison.Ordinal);
        }

        Assert.Equal(0, readCount);
        Assert.Equal(modelA.Model.CheckpointSha256, await ReadSha256Async(modelA.Model.CheckpointPath));

        var activeAfterReplacement = await InvokeActivationAsync(registry, ReadActiveRequest(TenantA));
        AssertSuccessActivation(activeAfterReplacement.BodyJson, modelA.Model.ModelId, expectedGeneration: 1, replay: false);
        Assert.Equal(activationARef, ReadActiveActivationRef(activeAfterReplacement.BodyJson));

        var preserved = await InvokeEvaluateAsync(registry, TenantA, "evaluate-snapshot-mismatch-preserves-cached-a");
        Assert.Equal(200, preserved.StatusCode);
        AssertEvaluateSelectedModel(preserved.BodyJson, modelA.Model.ModelId);
        AssertRuntimeSelection(
            preserved.BodyJson,
            loadedFresh: false,
            preservedPrevious: true,
            freshLoadAttempted: false);
        AssertEvaluationPromotedModelIdentity(preserved.BodyJson, modelA.Model.ModelId, modelA.Model.CheckpointSha256);
        Assert.Contains(
            DesignConversionActivatedModelRuntimeEvaluator.CachedRuntimeReusedEvidence,
            preserved.BodyJson,
            StringComparison.Ordinal);
        Assert.DoesNotContain(modelB.Model.ModelId, preserved.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(TenantA, preserved.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(modelA.Model.CheckpointPath, preserved.BodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(modelB.Model.CheckpointPath, preserved.BodyJson, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ModelActivationRuntime_FailedReloadPreservesConcurrentKnownGoodRuntimeWithoutRepositoryEvidence()
    {
        var root = NewRoot();
        var modelA = await BuildActivationRecordAsync(root, TenantA, "failed-reload-preserved-model-a", batchCount: 1);
        var registry = CreateRegistry(root, routeAdmissionTenantId: TenantA);
        var activated = await InvokeActivationAsync(
            registry,
            ActivateRequest(TenantA, "activate:failed-reload-preserved-a", 0, modelA));
        Assert.Equal(200, activated.StatusCode);

        var composition = DesignConversionLocalServiceComposition.CreateDeterministicLocalForLiveHostTesting();
        var activationRegistry = new DesignConversionModelActivationRegistry(
            Path.Combine(root, "activation-state"),
            () => DateTimeOffset.Parse("2026-07-10T10:00:00Z"),
            allowedArtifactRootDirectory: ArtifactDbRoot(root));
        var evaluator = new DesignConversionActivatedModelRuntimeEvaluator(
            activationRegistry,
            composition.RenderProvider,
            composition.ComparisonProvider,
            composition.VisualJudge);
        var firstReadEntered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseFailedRead = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        async Task<OperationResult<DesignConversionInternalEvaluationResult>> EvaluateWithFailedReadAsync()
        {
            using var checkpointReader = DesignConversionMachine.UseCheckpointByteReaderForCurrentAsyncFlow(
                async (_, cancellationToken) =>
                {
                    firstReadEntered.SetResult();
                    await releaseFailedRead.Task.WaitAsync(cancellationToken);
                    return StrictUtf8.GetBytes("{\"checkpoint\":\"corrupt-concurrent-load\"");
                });
            return await evaluator.EvaluateAsync(CreateInternalEvaluateRequest(
                TenantA,
                "evaluate-failed-reload-preserved-a",
                BuildTrustedRouteAdmissionProvenance(TenantA)));
        }

        var failedLoadEvaluation = EvaluateWithFailedReadAsync();
        await firstReadEntered.Task.WaitAsync(TimeSpan.FromSeconds(10));
        var successfulEvaluation = await evaluator.EvaluateAsync(CreateInternalEvaluateRequest(
            TenantA,
            "evaluate-concurrent-known-good-a",
            BuildTrustedRouteAdmissionProvenance(TenantA)));
        Assert.True(successfulEvaluation.IsSuccess, successfulEvaluation.ErrorMessage);
        releaseFailedRead.SetResult();

        var preserved = await failedLoadEvaluation;
        Assert.True(preserved.IsSuccess, preserved.ErrorMessage);
        Assert.Contains(
            DesignConversionActivatedModelRuntimeEvaluator.PreviousRuntimePreservedEvidence,
            preserved.Value!.EvidenceRefs);
        Assert.DoesNotContain(
            DesignConversionActivatedModelRuntimeEvaluator.CachedRuntimeReusedEvidence,
            preserved.Value.EvidenceRefs);
        Assert.Contains("active_model_runtime_fresh_load_attempted=True", preserved.Value.Warnings);
        Assert.Contains("active_model_runtime_fresh_load_succeeded=False", preserved.Value.Warnings);
        Assert.Contains("active_model_runtime_previous_preserved=True", preserved.Value.Warnings);
        Assert.DoesNotContain(
            DesignConversionRepositoryActivatedModelCheckpointLoader.RepositoryFreshLoadEvidence,
            preserved.Value.EvidenceRefs);
    }

    [Fact]
    public async Task ModelActivationRuntime_SourceMismatchRejectsFreshLoadAndDoesNotReuseCachedRuntime()
    {
        var root = NewRoot();
        var modelA = await BuildActivationRecordAsync(root, TenantA, "weights-source-cache-a", batchCount: 1);
        var registry = CreateRegistry(root, routeAdmissionTenantId: TenantA);

        var activated = await InvokeActivationAsync(
            registry,
            ActivateRequest(TenantA, "activate:weights-source-cache-a", 0, modelA));
        AssertSuccessActivation(
            activated.BodyJson,
            modelA.Model.ModelId,
            expectedGeneration: 1,
            replay: false,
            modelA.Model.LocalCheckpointWeightsSource);

        var cached = await InvokeEvaluateAsync(registry, TenantA, "evaluate-weights-source-cache-a");
        Assert.Equal(200, cached.StatusCode);
        AssertEvaluateSelectedModel(cached.BodyJson, modelA.Model.ModelId, modelA.Model.LocalCheckpointWeightsSource);
        AssertRuntimeSelection(cached.BodyJson, loadedFresh: true, preservedPrevious: false);

        await TamperActiveCheckpointWeightsSourceAsync(
            root,
            "11.DPO/DpoTrainer:dpo_policy_checkpoint_converted");

        var rejected = await InvokeEvaluateAsync(registry, TenantA, "evaluate-weights-source-mismatch");

        AssertActiveRuntimeLoadFailedClosed(
            rejected,
            "DESIGN_CONVERSION_ACTIVE_MODEL_CHECKPOINT_WEIGHTS_SOURCE_MISMATCH",
            expectedRuntimeInvoked: false,
            modelA.Model.ModelId,
            modelA.Model.CheckpointPath,
            modelA.Model.CheckpointSha256,
            TenantA);
        Assert.DoesNotContain(
            DesignConversionActivatedModelRuntimeEvaluator.PreviousRuntimePreservedEvidence,
            rejected.BodyJson,
            StringComparison.Ordinal);
        Assert.DoesNotContain("active_model_runtime_previous_preserved", rejected.BodyJson, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ModelActivationRuntime_RollbackThenSubsequentInferenceUsesA()
    {
        var root = NewRoot();
        var modelA = await BuildActivationRecordAsync(root, TenantA, "model-a", batchCount: 1);
        var modelB = await BuildActivationRecordAsync(root, TenantA, "model-b", batchCount: 2);
        var registry = CreateRegistry(root, routeAdmissionTenantId: TenantA);

        Assert.Equal(200, (await InvokeActivationAsync(registry, ActivateRequest(TenantA, "activate:a", 0, modelA))).StatusCode);
        Assert.Equal(200, (await InvokeActivationAsync(registry, ActivateRequest(TenantA, "activate:b", 1, modelB))).StatusCode);

        var beforeRollback = await InvokeEvaluateAsync(registry, TenantA, "evaluate-before-rollback");
        AssertEvaluateSelectedModel(beforeRollback.BodyJson, modelB.Model.ModelId);

        var rollback = await InvokeActivationAsync(
            registry,
            RollbackRequest(TenantA, "rollback:b-to-a", expectedGeneration: 2));
        AssertSuccessActivation(rollback.BodyJson, modelA.Model.ModelId, expectedGeneration: 3, replay: false);

        var afterRollback = await InvokeEvaluateAsync(registry, TenantA, "evaluate-after-rollback");
        Assert.Equal(200, afterRollback.StatusCode);
        AssertEvaluateSelectedModel(afterRollback.BodyJson, modelA.Model.ModelId);
    }

    [Fact]
    public async Task ModelActivationRuntime_RollbackToAThenCorruptAFailsClosedWithoutCachedB()
    {
        var root = NewRoot();
        var modelA = await BuildActivationRecordAsync(root, TenantA, "rollback-corrupt-model-a", batchCount: 1);
        var modelB = await BuildActivationRecordAsync(root, TenantA, "rollback-corrupt-model-b", batchCount: 2);
        var registry = CreateRegistry(root, routeAdmissionTenantId: TenantA);

        var activatedA = await InvokeActivationAsync(registry, ActivateRequest(TenantA, "activate:rollback-corrupt-a", 0, modelA));
        Assert.Equal(200, activatedA.StatusCode);
        var activationARef = ReadActiveActivationRef(activatedA.BodyJson);
        var activatedB = await InvokeActivationAsync(registry, ActivateRequest(TenantA, "activate:rollback-corrupt-b", 1, modelB));
        Assert.Equal(200, activatedB.StatusCode);
        var cachedBActivationRef = ReadActiveActivationRef(activatedB.BodyJson);
        Assert.NotEqual(activationARef, cachedBActivationRef);
        Assert.NotEqual(modelA.Model.ModelId, modelB.Model.ModelId);
        Assert.NotEqual(modelA.Model.CheckpointSha256, modelB.Model.CheckpointSha256);

        var cachedB = await InvokeEvaluateAsync(registry, TenantA, "evaluate-rollback-corrupt-cache-b");
        Assert.Equal(200, cachedB.StatusCode);
        AssertEvaluateSelectedModel(cachedB.BodyJson, modelB.Model.ModelId);
        AssertRuntimeSelection(cachedB.BodyJson, loadedFresh: true, preservedPrevious: false);
        AssertEvaluationPromotedModelIdentity(cachedB.BodyJson, modelB.Model.ModelId, modelB.Model.CheckpointSha256);

        var rollback = await InvokeActivationAsync(
            registry,
            RollbackRequest(TenantA, "rollback:corrupt-b-to-a", expectedGeneration: 2));
        AssertSuccessActivation(rollback.BodyJson, modelA.Model.ModelId, expectedGeneration: 3, replay: false);
        var rollbackAActivationRef = ReadActiveActivationRef(rollback.BodyJson);
        Assert.Equal(activationARef, rollbackAActivationRef);
        Assert.NotEqual(cachedBActivationRef, rollbackAActivationRef);

        var replacementBytes = await File.ReadAllBytesAsync(modelB.Model.CheckpointPath);
        await File.WriteAllBytesAsync(modelA.Model.CheckpointPath, replacementBytes);

        var failed = await InvokeEvaluateAsync(registry, TenantA, "evaluate-rollback-corrupt-a");
        AssertActiveRuntimeLoadFailedClosed(
            failed,
            "DESIGN_CONVERSION_ACTIVE_MODEL_CHECKPOINT_SHA_MISMATCH",
            false,
            root,
            TenantA,
            activationARef,
            cachedBActivationRef,
            rollbackAActivationRef,
            modelA.Model.ModelId,
            modelB.Model.ModelId,
            modelA.Model.CheckpointPath,
            modelB.Model.CheckpointPath);
    }

    [Fact]
    public async Task ModelActivationRuntime_RollbackToPreviouslyCachedAThenCorruptAUsesExactCachedA()
    {
        var root = NewRoot();
        var modelA = await BuildActivationRecordAsync(root, TenantA, "rollback-cached-corrupt-model-a", batchCount: 1);
        var modelB = await BuildActivationRecordAsync(root, TenantA, "rollback-cached-corrupt-model-b", batchCount: 2);
        var registry = CreateRegistry(root, routeAdmissionTenantId: TenantA);

        var activatedA = await InvokeActivationAsync(registry, ActivateRequest(TenantA, "activate:rollback-cached-corrupt-a", 0, modelA));
        Assert.Equal(200, activatedA.StatusCode);
        var activationARef = ReadActiveActivationRef(activatedA.BodyJson);

        var cachedA = await InvokeEvaluateAsync(registry, TenantA, "evaluate-rollback-cached-corrupt-cache-a");
        Assert.Equal(200, cachedA.StatusCode);
        AssertEvaluateSelectedModel(cachedA.BodyJson, modelA.Model.ModelId);
        AssertRuntimeSelection(cachedA.BodyJson, loadedFresh: true, preservedPrevious: false);
        AssertEvaluationPromotedModelIdentity(cachedA.BodyJson, modelA.Model.ModelId, modelA.Model.CheckpointSha256);

        var activatedB = await InvokeActivationAsync(registry, ActivateRequest(TenantA, "activate:rollback-cached-corrupt-b", 1, modelB));
        Assert.Equal(200, activatedB.StatusCode);
        var activationBRef = ReadActiveActivationRef(activatedB.BodyJson);
        Assert.NotEqual(activationARef, activationBRef);
        Assert.NotEqual(modelA.Model.ModelId, modelB.Model.ModelId);
        Assert.NotEqual(modelA.Model.CheckpointSha256, modelB.Model.CheckpointSha256);

        var cachedB = await InvokeEvaluateAsync(registry, TenantA, "evaluate-rollback-cached-corrupt-cache-b");
        Assert.Equal(200, cachedB.StatusCode);
        AssertEvaluateSelectedModel(cachedB.BodyJson, modelB.Model.ModelId);
        AssertRuntimeSelection(cachedB.BodyJson, loadedFresh: true, preservedPrevious: false);
        AssertEvaluationPromotedModelIdentity(cachedB.BodyJson, modelB.Model.ModelId, modelB.Model.CheckpointSha256);

        var rollback = await InvokeActivationAsync(
            registry,
            RollbackRequest(TenantA, "rollback:cached-corrupt-b-to-a", expectedGeneration: 2));
        AssertSuccessActivation(rollback.BodyJson, modelA.Model.ModelId, expectedGeneration: 3, replay: false);
        Assert.Equal(activationARef, ReadActiveActivationRef(rollback.BodyJson));

        var replacementBytes = await File.ReadAllBytesAsync(modelB.Model.CheckpointPath);
        await File.WriteAllBytesAsync(modelA.Model.CheckpointPath, replacementBytes);

        var recovered = await InvokeEvaluateAsync(registry, TenantA, "evaluate-rollback-cached-corrupt-a");
        Assert.True(recovered.StatusCode == 200, recovered.BodyJson);
        AssertEvaluateSelectedModel(recovered.BodyJson, modelA.Model.ModelId);
        AssertRuntimeSelection(
            recovered.BodyJson,
            loadedFresh: false,
            preservedPrevious: true,
            freshLoadAttempted: false);
        AssertEvaluationPromotedModelIdentity(recovered.BodyJson, modelA.Model.ModelId, modelA.Model.CheckpointSha256);
        Assert.Contains(
            DesignConversionActivatedModelRuntimeEvaluator.CachedRuntimeReusedEvidence,
            recovered.BodyJson,
            StringComparison.Ordinal);
        Assert.DoesNotContain(modelB.Model.ModelId, recovered.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(activationBRef, recovered.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(TenantA, recovered.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(modelA.Model.CheckpointPath, recovered.BodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(modelB.Model.CheckpointPath, recovered.BodyJson, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ModelActivationRuntime_RuntimeCacheDoesNotCrossTenantForSamePromotedModelIdentity()
    {
        var root = NewRoot();
        var tenantAModel = await BuildActivationRecordAsync(root, TenantA, "tenant-cache-shared-model", batchCount: 1);
        var tenantBPromotionRef = DesignConversionPromotedLocalModelLoader.LocalOnlyPromotionRefPrefix +
                                  Uri.EscapeDataString("tenant-cache-shared-model-b-manifest");
        var tenantBModel = tenantAModel with
        {
            TenantIdRef = DesignConversionModelActivationRegistry.BuildTenantIdRef(TenantB),
            Publication = tenantAModel.Publication with
            {
                Manifest = tenantAModel.Publication.Manifest with
                {
                    PromotionUploadRef = tenantBPromotionRef
                },
                UploadReceipt = tenantAModel.Publication.UploadReceipt! with
                {
                    RemoteRef = tenantBPromotionRef
                }
            }
        };
        await PersistPromotionAuthorityRecordAsync(root, TenantB, "tenant-cache-shared-model", tenantBModel, batchCount: 1);
        var replacement = await BuildActivationRecordAsync(root, TenantA, "tenant-cache-replacement", batchCount: 2);
        var composition = DesignConversionLocalServiceComposition.CreateDeterministicLocalForLiveHostTesting();
        var artifactDbRoot = ArtifactDbRoot(root);
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(artifactDbRoot);
        var activationRegistry = new DesignConversionModelActivationRegistry(
            Path.Combine(root, "activation-state"),
            () => DateTimeOffset.Parse("2026-07-10T10:00:00Z"),
            allowedArtifactRootDirectory: artifactDbRoot);
        var activationApi = new DesignConversionModelServerModelActivationApi(
            activationRegistry,
            LocalToken,
            artifactDb);
        var activatedEvaluator = new DesignConversionActivatedModelRuntimeEvaluator(
            activationRegistry,
            composition.RenderProvider,
            composition.ComparisonProvider,
            composition.VisualJudge);
        var tenantARegistry = CreateEvaluateRegistry(
            root,
            artifactDb,
            activatedEvaluator,
            routeAdmissionTenantId: TenantA);
        var tenantBRegistry = CreateEvaluateRegistry(
            root,
            artifactDb,
            activatedEvaluator,
            routeAdmissionTenantId: TenantB);

        Assert.Equal(tenantAModel.Model.ModelId, tenantBModel.Model.ModelId);
        Assert.Equal(tenantAModel.Model.CheckpointSha256, tenantBModel.Model.CheckpointSha256);
        Assert.NotEqual(tenantAModel.TenantIdRef, tenantBModel.TenantIdRef);
        await ActivateAsync(TenantA, "activate:tenant-cache-shared-a", 0, tenantAModel);
        await ActivateAsync(TenantB, "activate:tenant-cache-shared-b", 0, tenantBModel);

        var cachedA = await InvokeEvaluateAsync(tenantARegistry, TenantA, "evaluate-tenant-cache-shared-a");
        Assert.Equal(200, cachedA.StatusCode);
        AssertEvaluateSelectedModel(cachedA.BodyJson, tenantAModel.Model.ModelId);
        AssertRuntimeSelection(cachedA.BodyJson, loadedFresh: true, preservedPrevious: false, freshLoadAttempted: true);
        AssertEvaluationPromotedModelIdentity(cachedA.BodyJson, tenantAModel.Model.ModelId, tenantAModel.Model.CheckpointSha256);

        Assert.NotEqual(tenantAModel.Model.CheckpointSha256, replacement.Model.CheckpointSha256);
        var replacementBytes = await File.ReadAllBytesAsync(replacement.Model.CheckpointPath);
        await File.WriteAllBytesAsync(tenantAModel.Model.CheckpointPath, replacementBytes);

        var rejectedB = await InvokeEvaluateAsync(tenantBRegistry, TenantB, "evaluate-tenant-cache-shared-b");
        AssertActiveRuntimeLoadFailedClosed(
            rejectedB,
            "DESIGN_CONVERSION_ACTIVE_MODEL_CHECKPOINT_SHA_MISMATCH",
            expectedRuntimeInvoked: false,
            root,
            TenantA,
            TenantB,
            tenantAModel.Model.ModelId,
            tenantAModel.Model.CheckpointPath,
            replacement.Model.CheckpointPath);
        Assert.DoesNotContain(
            DesignConversionActivatedModelRuntimeEvaluator.PreviousRuntimePreservedEvidence,
            rejectedB.BodyJson,
            StringComparison.Ordinal);

        async Task ActivateAsync(
            string tenantId,
            string idempotencyKey,
            long expectedGeneration,
            DesignConversionPromotedModelActivationRecord candidate)
        {
            var activated = await activationApi.ActivatePublishedModelAsync(
                new DesignConversionModelServerModelActivationAuthorization(
                    TenantId: tenantId,
                    RequestedByRef: "architect:design-conversion-model-activation-route-runtime-test",
                    Reason: $"runtime cache tenant isolation setup {idempotencyKey}",
                    IdempotencyKey: idempotencyKey,
                    ExpectedGeneration: expectedGeneration,
                    LocalAuthorizationToken: LocalToken),
                candidate,
                expectedModelVersion: candidate.Model.Version,
                requestedAtUtc: DateTimeOffset.Parse("2026-07-10T10:00:00Z"));
            Assert.True(activated.IsSuccess, activated.ErrorMessage);
        }
    }

    [Fact]
    public void ModelActivationVersion_LocalCheckpointWeightsSourceBackCompatInfersAbsentOrBlankSource()
    {
        var dpoVersion = BuildActivationVersionForSourceInference(
            checkpointContainsRealTrainedWeights: true,
            evidenceRefs:
            [
                DesignConversionMachine.DpoTrainerPolicyCheckpointFreshLoadedEvidence
            ]);
        Assert.Equal("11.DPO/DpoTrainer:dpo_policy_checkpoint_converted", dpoVersion.LocalCheckpointWeightsSource);

        var localFeatureVersion = BuildActivationVersionForSourceInference(
            checkpointContainsRealTrainedWeights: true,
            evidenceRefs: []);
        Assert.Equal("local_checkpoint_feature_weights", localFeatureVersion.LocalCheckpointWeightsSource);

        var noWeightsVersion = BuildActivationVersionForSourceInference(
            checkpointContainsRealTrainedWeights: false,
            evidenceRefs: []);
        Assert.Equal("none", noWeightsVersion.LocalCheckpointWeightsSource);

        var blankSourceVersion = BuildActivationVersionForSourceInference(
            checkpointContainsRealTrainedWeights: true,
            evidenceRefs: [])
            with
            {
                LocalCheckpointWeightsSource = " "
            };
        Assert.Equal("local_checkpoint_feature_weights", blankSourceVersion.LocalCheckpointWeightsSource);
    }

    [Fact]
    public async Task ModelActivationRuntime_PerTenantRuntimeCacheRetainsTwoAndEvictsOldestExactIdentity()
    {
        var root = NewRoot();
        var modelA = await BuildActivationRecordAsync(root, TenantA, "runtime-cache-evict-model-a", batchCount: 1);
        var registry = CreateRegistry(root, routeAdmissionTenantId: TenantA);

        Assert.Equal(200, (await InvokeActivationAsync(registry, ActivateRequest(TenantA, "activate:runtime-cache-evict-a", 0, modelA))).StatusCode);
        var cachedA = await InvokeEvaluateAsync(registry, TenantA, "evaluate-runtime-cache-evict-a");
        Assert.Equal(200, cachedA.StatusCode);
        AssertEvaluateSelectedModel(cachedA.BodyJson, modelA.Model.ModelId);
        AssertRuntimeSelection(cachedA.BodyJson, loadedFresh: true, preservedPrevious: false);

        var modelB = await BuildActivationRecordAsync(root, TenantA, "runtime-cache-evict-model-b", batchCount: 2);
        Assert.Equal(200, (await InvokeActivationAsync(registry, ActivateRequest(TenantA, "activate:runtime-cache-evict-b", 1, modelB))).StatusCode);
        var cachedB = await InvokeEvaluateAsync(registry, TenantA, "evaluate-runtime-cache-evict-b");
        Assert.Equal(200, cachedB.StatusCode);
        AssertEvaluateSelectedModel(cachedB.BodyJson, modelB.Model.ModelId);
        AssertRuntimeSelection(cachedB.BodyJson, loadedFresh: true, preservedPrevious: false);

        var modelC = await BuildActivationRecordAsync(root, TenantA, "runtime-cache-evict-model-c", batchCount: 3);
        Assert.Equal(200, (await InvokeActivationAsync(registry, ActivateRequest(TenantA, "activate:runtime-cache-evict-c", 2, modelC))).StatusCode);
        var cachedC = await InvokeEvaluateAsync(registry, TenantA, "evaluate-runtime-cache-evict-c");
        Assert.Equal(200, cachedC.StatusCode);
        AssertEvaluateSelectedModel(cachedC.BodyJson, modelC.Model.ModelId);
        AssertRuntimeSelection(cachedC.BodyJson, loadedFresh: true, preservedPrevious: false);

        await AppendPromotionRollbackAuthorityRecordAsync(
            root,
            TenantA,
            "runtime-cache-restore-b",
            modelC,
            modelB,
            batchCount: 3);
        Assert.Equal(200, (await InvokeActivationAsync(registry, ActivateRequest(TenantA, "activate:runtime-cache-retained-b", 3, modelB))).StatusCode);
        var originalB = await File.ReadAllBytesAsync(modelB.Model.CheckpointPath);
        var replacementForB = await File.ReadAllBytesAsync(modelC.Model.CheckpointPath);
        await File.WriteAllBytesAsync(modelB.Model.CheckpointPath, replacementForB);

        var retainedB = await InvokeEvaluateAsync(registry, TenantA, "evaluate-runtime-cache-retained-b");
        Assert.Equal(200, retainedB.StatusCode);
        AssertEvaluateSelectedModel(retainedB.BodyJson, modelB.Model.ModelId);
        AssertRuntimeSelection(
            retainedB.BodyJson,
            loadedFresh: false,
            preservedPrevious: true,
            freshLoadAttempted: false);
        AssertEvaluationPromotedModelIdentity(retainedB.BodyJson, modelB.Model.ModelId, modelB.Model.CheckpointSha256);
        Assert.DoesNotContain(modelA.Model.ModelId, retainedB.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(modelC.Model.ModelId, retainedB.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(TenantA, retainedB.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(modelA.Model.CheckpointPath, retainedB.BodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(modelB.Model.CheckpointPath, retainedB.BodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(modelC.Model.CheckpointPath, retainedB.BodyJson, StringComparison.OrdinalIgnoreCase);
        await File.WriteAllBytesAsync(modelB.Model.CheckpointPath, originalB);

        await AppendPromotionRollbackAuthorityRecordAsync(
            root,
            TenantA,
            "runtime-cache-restore-a",
            modelB,
            modelA,
            batchCount: 2);
        Assert.Equal(200, (await InvokeActivationAsync(registry, ActivateRequest(TenantA, "activate:runtime-cache-evicted-a", 4, modelA))).StatusCode);
        var replacementForA = await File.ReadAllBytesAsync(modelC.Model.CheckpointPath);
        await File.WriteAllBytesAsync(modelA.Model.CheckpointPath, replacementForA);

        var evictedA = await InvokeEvaluateAsync(registry, TenantA, "evaluate-runtime-cache-evicted-a");
        AssertActiveRuntimeLoadFailedClosed(
            evictedA,
            "DESIGN_CONVERSION_ACTIVE_MODEL_CHECKPOINT_SHA_MISMATCH",
            false,
            root,
            TenantA,
            modelA.Model.ModelId,
            modelB.Model.ModelId,
            modelC.Model.ModelId,
            modelA.Model.CheckpointPath,
            modelB.Model.CheckpointPath,
            modelC.Model.CheckpointPath);
    }

    [Fact]
    public async Task ModelActivationRuntime_FreshHostRegistryRetainsDurableState()
    {
        var root = NewRoot();
        var modelB = await BuildActivationRecordAsync(root, TenantB, "fresh-host-b", batchCount: 2);
        var firstHostRegistry = CreateRegistry(root, routeAdmissionTenantId: TenantB);

        var activated = await InvokeActivationAsync(
            firstHostRegistry,
            ActivateRequest(TenantB, "activate:fresh-host-b", 0, modelB));
        AssertSuccessActivation(activated.BodyJson, modelB.Model.ModelId, expectedGeneration: 1, replay: false);

        var freshHostRegistry = CreateRegistry(root, routeAdmissionTenantId: TenantB);
        var readback = await InvokeActivationAsync(
            freshHostRegistry,
            new DesignConversionModelServerModelActivationRequest(
                Action: DesignConversionModelServerModelActivationHandler.ActionReadActive,
                TenantId: TenantB));
        AssertSuccessActivation(readback.BodyJson, modelB.Model.ModelId, expectedGeneration: 1, replay: false);

        var evaluated = await InvokeEvaluateAsync(freshHostRegistry, TenantB, "evaluate-fresh-host-b");
        Assert.Equal(200, evaluated.StatusCode);
        AssertEvaluateSelectedModel(evaluated.BodyJson, modelB.Model.ModelId);
    }

    [Fact]
    public async Task ModelActivationRuntime_AdmittedTenantSelectsAWithMatchingOrCaseVariantBodyAndFailsClosedWhenBodyTenantAbsent()
    {
        var root = NewRoot();
        var modelA = await BuildActivationRecordAsync(root, TenantA, "admitted-a-model", batchCount: 1);
        var modelB = await BuildActivationRecordAsync(root, TenantB, "admitted-b-model", batchCount: 2);
        var registry = CreateRegistry(root, routeAdmissionTenantId: TenantA);

        Assert.Equal(200, (await InvokeActivationAsync(registry, ActivateRequest(TenantA, "activate:admitted-a", 0, modelA))).StatusCode);
        Assert.Equal(200, (await InvokeActivationAsync(registry, ActivateRequest(TenantB, "activate:admitted-b", 0, modelB))).StatusCode);

        var matchingBody = await InvokeEvaluateAsync(
            registry,
            TenantA,
            "evaluate-admitted-a-matching-body",
            routeAdmissionToken: RouteAdmissionCurrentToken);
        Assert.Equal(200, matchingBody.StatusCode);
        AssertEvaluateSelectedModel(matchingBody.BodyJson, modelA.Model.ModelId);
        Assert.DoesNotContain(modelB.Model.ModelId, matchingBody.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(TenantA, matchingBody.BodyJson, StringComparison.Ordinal);

        var caseVariantBody = await InvokeEvaluateAsync(
            registry,
            TenantA.ToUpperInvariant(),
            "evaluate-admitted-a-case-variant-body",
            routeAdmissionToken: RouteAdmissionCurrentToken);
        Assert.Equal(200, caseVariantBody.StatusCode);
        AssertEvaluateSelectedModel(caseVariantBody.BodyJson, modelA.Model.ModelId);
        Assert.DoesNotContain(modelB.Model.ModelId, caseVariantBody.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(TenantA.ToUpperInvariant(), caseVariantBody.BodyJson, StringComparison.Ordinal);

        var absentBody = await InvokeEvaluateAsync(
            registry,
            tenantId: null,
            requestId: "evaluate-admitted-a-absent-body",
            routeAdmissionToken: RouteAdmissionPreviousToken,
            includeTenantMetadata: false);
        Assert.Equal(403, absentBody.StatusCode);
        AssertErrorCode(absentBody.BodyJson, "DESIGN_CONVERSION_ACTIVE_MODEL_TRUSTED_TENANT_CONTEXT_INVALID");
        Assert.DoesNotContain(modelA.Model.ModelId, absentBody.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(modelB.Model.ModelId, absentBody.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(TenantA, absentBody.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(TenantB, absentBody.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(modelA.Model.CheckpointPath, absentBody.BodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(modelB.Model.CheckpointPath, absentBody.BodyJson, StringComparison.OrdinalIgnoreCase);
        using var absentDocument = JsonDocument.Parse(absentBody.BodyJson);
        var metadata = absentDocument.RootElement.GetProperty("boundaryMetadata");
        Assert.Equal(bool.FalseString, metadata.GetProperty("runtime_invoked").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("persistence_invoked").GetString());
    }

    [Fact]
    public async Task ModelActivationRuntime_InvalidBodyTenantRejectsBeforeRuntimeOrPersistence()
    {
        var root = NewRoot();
        var modelA = await BuildActivationRecordAsync(root, TenantA, "invalid-body-tenant-model-a", batchCount: 1);
        var registry = CreateRegistry(root, routeAdmissionTenantId: TenantA);

        var activated = await InvokeActivationAsync(
            registry,
            ActivateRequest(TenantA, "activate:invalid-body-tenant-a", 0, modelA));
        AssertSuccessActivation(activated.BodyJson, modelA.Model.ModelId, expectedGeneration: 1, replay: false);

        var evaluated = await InvokeEvaluateAsync(
            registry,
            "tenant:design-conversion:route-alpha",
            "evaluate-invalid-body-tenant");

        Assert.Equal(403, evaluated.StatusCode);
        AssertErrorCode(evaluated.BodyJson, "DESIGN_CONVERSION_ACTIVE_MODEL_TRUSTED_TENANT_CONTEXT_INVALID");
        Assert.DoesNotContain("tenant:design-conversion:route-alpha", evaluated.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(modelA.Model.ModelId, evaluated.BodyJson, StringComparison.Ordinal);
        using var document = JsonDocument.Parse(evaluated.BodyJson);
        var metadata = document.RootElement.GetProperty("boundaryMetadata");
        Assert.Equal(bool.FalseString, metadata.GetProperty("runtime_invoked").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("persistence_invoked").GetString());
    }

    [Fact]
    public async Task ModelActivationRoute_StaleGenerationReturnsConflict409()
    {
        var root = NewRoot();
        var first = await BuildActivationRecordAsync(root, TenantA, "stale-first", batchCount: 1);
        var stale = await BuildActivationRecordAsync(root, TenantA, "stale-second", batchCount: 2);
        var registry = CreateRegistry(root);

        var activated = await InvokeActivationAsync(
            registry,
            ActivateRequest(TenantA, "activate:stale-first", 0, first));
        Assert.Equal(200, activated.StatusCode);

        var response = await InvokeActivationAsync(
            registry,
            ActivateRequest(TenantA, "activate:stale-second", 0, stale));

        Assert.Equal(409, response.StatusCode);
        AssertErrorCode(response.BodyJson, "DESIGN_CONVERSION_MODEL_ACTIVATION_GENERATION_CONFLICT");
        Assert.DoesNotContain(root, response.BodyJson, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ModelActivationRoute_DuplicateActivateOperationIsIdempotentReplay()
    {
        var root = NewRoot();
        var model = await BuildActivationRecordAsync(root, TenantA, "duplicate", batchCount: 1);
        var registry = CreateRegistry(root);
        var request = ActivateRequest(TenantA, "activate:duplicate", 0, model);

        var first = await InvokeActivationAsync(registry, request);
        AssertSuccessActivation(first.BodyJson, model.Model.ModelId, expectedGeneration: 1, replay: false);

        var replay = await InvokeActivationAsync(registry, request);
        AssertSuccessActivation(replay.BodyJson, model.Model.ModelId, expectedGeneration: 1, replay: true);
        using var document = JsonDocument.Parse(replay.BodyJson);
        Assert.False(document.RootElement.GetProperty("applied").GetBoolean());
        Assert.True(document.RootElement.GetProperty("replay").GetBoolean());
    }

    [Fact]
    public async Task ModelActivationRoute_ForgedCallerOnlyPromotionReceiptFailsWithoutStateMutation()
    {
        var root = NewRoot();
        var forged = await BuildActivationRecordAsync(
            root,
            TenantA,
            "forged-caller-only",
            batchCount: 1,
            persistDurablePromotion: false);
        var registry = CreateRegistry(root);

        var response = await InvokeActivationAsync(
            registry,
            ActivateRequest(TenantA, "activate:forged-caller-only", 0, forged));

        Assert.Equal(400, response.StatusCode);
        AssertErrorCode(response.BodyJson, "DESIGN_CONVERSION_MODEL_ACTIVATION_PROMOTION_RECORD_NOT_FOUND");

        var readback = await InvokeActivationAsync(registry, ReadActiveRequest(TenantA));
        Assert.Equal(404, readback.StatusCode);
        AssertErrorCode(readback.BodyJson, "DESIGN_CONVERSION_MODEL_ACTIVATION_ACTIVE_VERSION_NOT_FOUND");
    }

    [Fact]
    public async Task ModelActivationRoute_CorruptPromotionAuthorityJournalFailsClosedWithoutStateMutation()
    {
        var root = NewRoot();
        var candidate = await BuildActivationRecordAsync(
            root,
            TenantA,
            "corrupt-authority-journal",
            batchCount: 1,
            persistDurablePromotion: false);
        Directory.CreateDirectory(Path.GetDirectoryName(PromotionJournalPath(root))!);
        await File.WriteAllTextAsync(PromotionJournalPath(root), "{not-json", StrictUtf8);
        var registry = CreateRegistry(root);

        var response = await InvokeActivationAsync(
            registry,
            ActivateRequest(TenantA, "activate:corrupt-authority-journal", 0, candidate));

        Assert.Equal(400, response.StatusCode);
        AssertErrorCode(response.BodyJson, "DESIGN_CONVERSION_MODEL_ACTIVATION_PROMOTION_AUTHORITY_READ_FAILED");

        var readback = await InvokeActivationAsync(registry, ReadActiveRequest(TenantA));
        Assert.Equal(404, readback.StatusCode);
        AssertErrorCode(readback.BodyJson, "DESIGN_CONVERSION_MODEL_ACTIVATION_ACTIVE_VERSION_NOT_FOUND");
    }

    [Fact]
    public async Task ModelActivationRoute_RollbackAuthorityRejectsFormerBAndAcceptsRestoredA()
    {
        var root = NewRoot();
        var modelA = await BuildActivationRecordAsync(
            root,
            TenantA,
            "authority-current-a",
            batchCount: 1);
        var modelB = await BuildActivationRecordAsync(
            root,
            TenantA,
            "authority-rolled-back-b",
            batchCount: 2);
        var rollbackCarrier = BuildPromotionAuthorityRecord(
            TenantA,
            "authority-rollback-b-to-a",
            modelB,
            batchCount: 2);
        var rollbackMetadata = new Dictionary<string, string>(
            rollbackCarrier.BoundaryMetadata,
            StringComparer.Ordinal)
        {
            ["schema_version"] =
                JsonlDesignConversionLocalArtifactDb.TrainingIntakeLocalPromotionRollbackSchemaVersion,
            ["local_promotion_record_kind"] = "rollback",
            ["local_promotion_rollback_id"] = "rollback-authority-b-to-a",
            ["local_promotion_rollback_applied"] = bool.TrueString,
            ["local_promotion_prior_state_restored"] = bool.TrueString,
            ["local_promotion_rollback_target_local_model_ref"] =
                modelB.Publication.Manifest.PromotionUploadRef!,
            ["local_promotion_rollback_previous_active_local_model_ref"] =
                modelB.Publication.Manifest.PromotionUploadRef!,
            ["local_promotion_rollback_restored_active_local_model_ref"] =
                modelA.Publication.Manifest.PromotionUploadRef!,
            ["local_promotion_rollback_restored_active_model_id"] =
                modelA.Model.ModelId,
            ["local_promotion_rollback_restored_active_checkpoint_sha256"] =
                modelA.Model.CheckpointSha256,
            ["local_promotion_active_local_model_ref"] =
                modelA.Publication.Manifest.PromotionUploadRef!,
            ["local_promotion_active_model_id"] = modelA.Model.ModelId,
            ["local_promotion_active_checkpoint_sha256"] =
                modelA.Model.CheckpointSha256
        };
        await File.AppendAllTextAsync(
            PromotionJournalPath(root),
            JsonSerializer.Serialize(
                rollbackCarrier with { BoundaryMetadata = rollbackMetadata },
                PromotionJournalJsonOptions) + Environment.NewLine,
            StrictUtf8);
        var registry = CreateRegistry(root);

        var rejectedB = await InvokeActivationAsync(
            registry,
            ActivateRequest(TenantA, "activate:rolled-back-b", 0, modelB));
        Assert.Equal(400, rejectedB.StatusCode);
        AssertErrorCode(
            rejectedB.BodyJson,
            "DESIGN_CONVERSION_MODEL_ACTIVATION_PROMOTION_NOT_CURRENT_ACTIVE");

        var acceptedA = await InvokeActivationAsync(
            registry,
            ActivateRequest(TenantA, "activate:restored-a", 0, modelA));
        AssertSuccessActivation(
            acceptedA.BodyJson,
            modelA.Model.ModelId,
            expectedGeneration: 1,
            replay: false);
    }

    [Fact]
    public async Task ModelActivationRoute_DurableAuthorityRootConfinesCheckpointArtifacts()
    {
        var root = NewRoot();
        var candidate = await BuildActivationRecordAsync(
            root,
            TenantA,
            "authority-root-confined",
            batchCount: 1,
            persistDurablePromotion: false);
        var outsideRoot = ArtifactDbRoot(root) + "-sibling";
        Directory.CreateDirectory(outsideRoot);
        var outsideCheckpoint = Path.Combine(outsideRoot, "authority-root-confined-checkpoint.json");
        File.Copy(candidate.Model.CheckpointPath, outsideCheckpoint);
        var forged = WithArtifactPaths(candidate, checkpointPath: outsideCheckpoint);
        var promotion = BuildPromotionAuthorityRecord(
            TenantA,
            "authority-root-confined",
            forged,
            batchCount: 1);
        var db = new JsonlDesignConversionLocalArtifactDb(ArtifactDbRoot(root));
        var persisted = await db.PersistServerOwnedLocalPromotionAuthorityForTestingAsync(
            promotion,
            "idempotency:authority-root-confined-authority",
            CancellationToken.None);
        Assert.False(persisted.IsSuccess);
        var registry = CreateRegistry(root);

        var response = await InvokeActivationAsync(
            registry,
            ActivateRequest(TenantA, "activate:authority-root-confined", 0, forged));

        Assert.Equal(400, response.StatusCode);
        AssertErrorCode(
            response.BodyJson,
            "DESIGN_CONVERSION_MODEL_ACTIVATION_PROMOTION_RECORD_NOT_FOUND");
        Assert.DoesNotContain(outsideRoot, response.BodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(outsideCheckpoint, response.BodyJson, StringComparison.OrdinalIgnoreCase);

        var readback = await InvokeActivationAsync(registry, ReadActiveRequest(TenantA));
        Assert.Equal(404, readback.StatusCode);
        AssertErrorCode(readback.BodyJson, "DESIGN_CONVERSION_MODEL_ACTIVATION_ACTIVE_VERSION_NOT_FOUND");
    }

    [Fact]
    public async Task ModelActivationRoute_NullPublicationManifestFailsClosedWithoutStateMutation()
    {
        var root = NewRoot();
        var candidate = await BuildActivationRecordAsync(root, TenantA, "null-manifest-shape", batchCount: 1);
        var malformed = candidate with
        {
            Publication = candidate.Publication with { Manifest = null! }
        };
        var registry = CreateRegistry(root);

        var response = await InvokeActivationAsync(
            registry,
            ActivateRequest(TenantA, "activate:null-manifest-shape", 0, malformed));

        Assert.Equal(400, response.StatusCode);
        AssertErrorCode(response.BodyJson, "DESIGN_CONVERSION_MODEL_ACTIVATION_MANIFEST_REQUIRED");

        var readback = await InvokeActivationAsync(registry, ReadActiveRequest(TenantA));
        Assert.Equal(404, readback.StatusCode);
        AssertErrorCode(readback.BodyJson, "DESIGN_CONVERSION_MODEL_ACTIVATION_ACTIVE_VERSION_NOT_FOUND");
    }

    [Fact]
    public async Task ModelActivationRoute_DurablePromotionNullEvidenceCollectionFailsClosedWithoutStateMutation()
    {
        var root = NewRoot();
        var candidate = await BuildActivationRecordAsync(
            root,
            TenantA,
            "durable-null-evidence",
            batchCount: 1,
            persistDurablePromotion: false);
        var malformedDurable = candidate with
        {
            Publication = candidate.Publication with
            {
                Decision = candidate.Publication.Decision with { EvidenceRefs = null! }
            }
        };
        await PersistPromotionAuthorityRecordAsync(root, TenantA, "durable-null-evidence", malformedDurable, batchCount: 1);
        var registry = CreateRegistry(root);

        var response = await InvokeActivationAsync(
            registry,
            ActivateRequest(TenantA, "activate:durable-null-evidence", 0, candidate));

        Assert.Equal(400, response.StatusCode);
        AssertErrorCode(response.BodyJson, "DESIGN_CONVERSION_MODEL_ACTIVATION_PROMOTION_RECORD_NOT_FOUND");

        var readback = await InvokeActivationAsync(registry, ReadActiveRequest(TenantA));
        Assert.Equal(404, readback.StatusCode);
        AssertErrorCode(readback.BodyJson, "DESIGN_CONVERSION_MODEL_ACTIVATION_ACTIVE_VERSION_NOT_FOUND");
    }

    [Fact]
    public async Task ModelActivationRoute_DurablePromotionBindingMismatchFailsAndPreservesActiveModel()
    {
        var root = NewRoot();
        var modelA = await BuildActivationRecordAsync(root, TenantA, "binding-active-a", batchCount: 1);
        var modelB = await BuildActivationRecordAsync(root, TenantA, "binding-candidate-b", batchCount: 2);
        var registry = CreateRegistry(root);

        var activated = await InvokeActivationAsync(
            registry,
            ActivateRequest(TenantA, "activate:binding-active-a", 0, modelA));
        AssertSuccessActivation(activated.BodyJson, modelA.Model.ModelId, expectedGeneration: 1, replay: false);

        var tamperedModel = modelB with
        {
            Model = modelB.Model with { ModelId = modelB.Model.ModelId + "-forged" }
        };
        var tamperedCheckpoint = modelB with
        {
            Model = modelB.Model with { CheckpointSha256 = Sha256Text("forged-checkpoint-sha") }
        };
        var tamperedPublication = modelB with
        {
            Publication = modelB.Publication with
            {
                Manifest = modelB.Publication.Manifest with
                {
                    ManifestRef = modelB.Publication.Manifest.ManifestRef + "-forged"
                }
            }
        };

        foreach (var (candidate, key) in new[]
                 {
                     (tamperedModel, "model"),
                     (tamperedCheckpoint, "checkpoint"),
                     (tamperedPublication, "publication")
                 })
        {
            var response = await InvokeActivationAsync(
                registry,
                ActivateRequest(TenantA, $"activate:binding-mismatch-{key}", 1, candidate));

            Assert.Equal(400, response.StatusCode);
            AssertErrorCode(response.BodyJson, "DESIGN_CONVERSION_MODEL_ACTIVATION_DURABLE_PROMOTION_BINDING_MISMATCH");
        }

        var readback = await InvokeActivationAsync(registry, ReadActiveRequest(TenantA));
        AssertSuccessActivation(readback.BodyJson, modelA.Model.ModelId, expectedGeneration: 1, replay: false);
    }

    [Fact]
    public async Task ModelActivationRoute_EvaluatedSourceEncoderDurableBindingMismatchFailsAndPreservesActiveModel()
    {
        var root = NewRoot();
        var modelA = await BuildActivationRecordAsync(root, TenantA, "source-binding-active-a", batchCount: 1);
        var modelB = await BuildActivationRecordAsync(
            root,
            TenantA,
            "source-binding-candidate-b",
            batchCount: 2,
            evaluatedDpoPromotion: true);
        var registry = CreateRegistry(root);
        var activated = await InvokeActivationAsync(
            registry,
            ActivateRequest(TenantA, "activate:source-binding-active-a", 0, modelA));
        AssertSuccessActivation(activated.BodyJson, modelA.Model.ModelId, expectedGeneration: 1, replay: false);

        var forgedSourcePath = Path.Combine(
            Path.GetDirectoryName(modelB.Model.CheckpointPath)!,
            "forged-source-encoder.json");
        await File.WriteAllTextAsync(forgedSourcePath, "{\"kind\":\"forged-source-encoder\"}", StrictUtf8);
        var forged = WithArtifactPaths(modelB, sourceEncoderPath: Path.GetFullPath(forgedSourcePath));

        var response = await InvokeActivationAsync(
            registry,
            ActivateRequest(TenantA, "activate:source-binding-mismatch", 1, forged));
        var readback = await InvokeActivationAsync(registry, ReadActiveRequest(TenantA));

        Assert.Equal(400, response.StatusCode);
        AssertErrorCode(response.BodyJson, "DESIGN_CONVERSION_MODEL_ACTIVATION_DURABLE_PROMOTION_BINDING_MISMATCH");
        Assert.DoesNotContain(forgedSourcePath, response.BodyJson, StringComparison.OrdinalIgnoreCase);
        AssertSuccessActivation(readback.BodyJson, modelA.Model.ModelId, expectedGeneration: 1, replay: false);
    }

    [Fact]
    public async Task ModelActivationRoute_MultiplePartialAuthorityMatchesAcceptsExactDurableBinding()
    {
        var root = NewRoot();
        var exact = await BuildActivationRecordAsync(root, TenantA, "partial-match-exact", batchCount: 2);
        var laterPartial = await BuildActivationRecordAsync(
            root,
            TenantA,
            "partial-match-later-mismatch",
            batchCount: 3,
            persistDurablePromotion: false);
        var laterPartialWithSharedCheckpointSha = laterPartial with
        {
            Model = laterPartial.Model with { CheckpointSha256 = exact.Model.CheckpointSha256 }
        };
        await PersistPromotionAuthorityRecordAsync(
            root,
            TenantA,
            "partial-match-later-mismatch",
            laterPartialWithSharedCheckpointSha,
            batchCount: 3);
        await AppendPromotionRollbackAuthorityRecordAsync(
            root,
            TenantA,
            "partial-match-restore-exact",
            laterPartialWithSharedCheckpointSha,
            exact,
            batchCount: 3);
        var registry = CreateRegistry(root);

        var activated = await InvokeActivationAsync(
            registry,
            ActivateRequest(TenantA, "activate:partial-match-exact", 0, exact));

        AssertSuccessActivation(activated.BodyJson, exact.Model.ModelId, expectedGeneration: 1, replay: false);
    }

    [Fact]
    public async Task ModelActivationRoute_ActivateLocalPromotionRefsOnlySelectorActivatesServerOwnedDurableRecord()
    {
        var root = NewRoot();
        var model = await BuildActivationRecordAsync(root, TenantA, "refs-only-local-success", batchCount: 1);
        var registry = CreateRegistry(root);

        var response = await InvokeActivationAsync(
            registry,
            ActivateLocalPromotionRequest(TenantA, "activate-local:refs-only-success", 0, model));

        AssertSuccessActivation(response.BodyJson, model.Model.ModelId, expectedGeneration: 1, replay: false);
        AssertActivatedLocalPromotionIdentity(response.BodyJson, model);
        AssertRefsOnlyActivationResponse(response.BodyJson, model);
    }

    [Fact]
    public async Task ModelActivationRoute_ActivateLocalPromotionDpoCheckpointSelectorActivatesServerOwnedDurableRecord()
    {
        var root = NewRoot();
        var dpoModel = await BuildActivationRecordAsync(
            root,
            TenantA,
            "refs-only-local-dpo-success",
            batchCount: 1,
            directDpoWeightSeed: 311);
        var registry = CreateRegistry(root);

        var response = await InvokeActivationAsync(
            registry,
            ActivateLocalPromotionRequest(TenantA, "activate-local:refs-only-dpo-success", 0, dpoModel));

        AssertSuccessActivation(
            response.BodyJson,
            dpoModel.Model.ModelId,
            expectedGeneration: 1,
            replay: false,
            dpoModel.Model.LocalCheckpointWeightsSource);
        AssertActivatedLocalPromotionIdentity(response.BodyJson, dpoModel);
        AssertRefsOnlyActivationResponse(response.BodyJson, dpoModel);
    }

    [Fact]
    public async Task ModelActivationRoute_ActivateLocalPromotionMissingSelectorFailsBeforeMutation()
    {
        var root = NewRoot();
        var registry = CreateRegistry(root);

        var response = await InvokeActivationAsync(
            registry,
            new DesignConversionModelServerModelActivationRequest(
                Action: DesignConversionModelServerModelActivationHandler.ActionActivateLocalPromotion,
                TenantId: TenantA,
                RequestedByRef: "architect:design-conversion-model-activation-route-runtime-test",
                Reason: "activation route runtime missing refs-only selector",
                IdempotencyKey: "activate-local:missing-selector",
                ExpectedGeneration: 0,
                ExpectedModelVersion: "missing-selector-version",
                RequestedAtUtc: DateTimeOffset.Parse("2026-07-10T10:00:00Z")));

        Assert.Equal(400, response.StatusCode);
        AssertErrorCode(response.BodyJson, "MODEL_ACTIVATION_LOCAL_PROMOTION_SELECTOR_REQUIRED");
        await AssertNoActiveActivationAsync(registry, TenantA);
    }

    [Fact]
    public async Task ModelActivationRoute_ActivateLocalPromotionPathLikeSelectorFailsBeforeMutation()
    {
        var root = NewRoot();
        var registry = CreateRegistry(root);
        var checkpointSha = new string('a', 64);
        var safePromotionRef =
            DesignConversionPromotedLocalModelLoader.LocalOnlyPromotionRefPrefix +
            "refs-only-local-safe-selector";
        var invalidSelectors = new[]
        {
            (
                CaseId: "drive-model-id",
                Selector: new DesignConversionModelServerLocalPromotionActivationSelector(
                    safePromotionRef,
                    "D:\\private\\model",
                    "selector-version",
                    checkpointSha),
                Forbidden: "D:\\"),
            (
                CaseId: "embedded-drive-promotion-ref",
                Selector: new DesignConversionModelServerLocalPromotionActivationSelector(
                    DesignConversionPromotedLocalModelLoader.LocalOnlyPromotionRefPrefix +
                    "E:/private/model",
                    "selector-model",
                    "selector-version",
                    checkpointSha),
                Forbidden: "E:/"),
            (
                CaseId: "encoded-drive-promotion-ref",
                Selector: new DesignConversionModelServerLocalPromotionActivationSelector(
                    DesignConversionPromotedLocalModelLoader.LocalOnlyPromotionRefPrefix +
                    "E:%2Fprivate%2Fmodel",
                    "selector-model",
                    "selector-version",
                    checkpointSha),
                Forbidden: "E:%2F"),
            (
                CaseId: "traversal-version",
                Selector: new DesignConversionModelServerLocalPromotionActivationSelector(
                    safePromotionRef,
                    "selector-model",
                    "../selector-version",
                    checkpointSha),
                Forbidden: "../")
        };

        foreach (var invalid in invalidSelectors)
        {
            var response = await InvokeActivationAsync(
                registry,
                ActivateLocalPromotionSelectorRequest(
                    TenantA,
                    $"activate-local:path-like-{invalid.CaseId}",
                    0,
                    invalid.Selector));

            Assert.Equal(400, response.StatusCode);
            AssertErrorCode(
                response.BodyJson,
                "DESIGN_CONVERSION_MODEL_ACTIVATION_LOCAL_PROMOTION_SELECTOR_INVALID");
            Assert.DoesNotContain(
                invalid.Forbidden,
                response.BodyJson,
                StringComparison.OrdinalIgnoreCase);
            await AssertNoActiveActivationAsync(registry, TenantA);
        }
    }

    [Fact]
    public async Task ModelActivationRoute_ActivateLocalPromotionRejectsCallerSerializedCandidateBeforeMutation()
    {
        var root = NewRoot();
        var model = await BuildActivationRecordAsync(root, TenantA, "refs-only-local-candidate-forbidden", batchCount: 1);
        var registry = CreateRegistry(root);
        var request = ActivateLocalPromotionRequest(
            TenantA,
            "activate-local:candidate-forbidden",
            0,
            model) with
        {
            Candidate = model
        };

        var response = await InvokeActivationAsync(registry, request);

        Assert.Equal(400, response.StatusCode);
        AssertErrorCode(response.BodyJson, "MODEL_ACTIVATION_LOCAL_PROMOTION_CANDIDATE_FORBIDDEN");
        await AssertNoActiveActivationAsync(registry, TenantA);
        AssertRefsOnlyActivationResponse(response.BodyJson, model);
    }

    [Fact]
    public async Task ModelActivationRoute_ActivateLocalPromotionForgedSelectorWithoutDurableMatchFailsBeforeMutation()
    {
        var root = NewRoot();
        var forged = await BuildActivationRecordAsync(
            root,
            TenantA,
            "refs-only-local-forged",
            batchCount: 1,
            persistDurablePromotion: false);
        var registry = CreateRegistry(root);

        var response = await InvokeActivationAsync(
            registry,
            ActivateLocalPromotionRequest(TenantA, "activate-local:forged-no-match", 0, forged));

        Assert.Equal(404, response.StatusCode);
        AssertErrorCode(response.BodyJson, "DESIGN_CONVERSION_MODEL_ACTIVATION_PROMOTION_RECORD_NOT_FOUND");
        await AssertNoActiveActivationAsync(registry, TenantA);
        AssertRefsOnlyActivationResponse(response.BodyJson, forged);
    }

    [Fact]
    public async Task ModelActivationRoute_ActivateLocalPromotionCorruptAuthorityJournalFailsClosedBeforeMutation()
    {
        var root = NewRoot();
        Directory.CreateDirectory(Path.GetDirectoryName(PromotionJournalPath(root))!);
        await File.WriteAllTextAsync(PromotionJournalPath(root), "{not-json", StrictUtf8);
        var registry = CreateRegistry(root);
        var selector = new DesignConversionModelServerLocalPromotionActivationSelector(
            DesignConversionPromotedLocalModelLoader.LocalOnlyPromotionRefPrefix +
            "refs-only-local-corrupt-journal",
            "refs-only-local-corrupt-journal-model",
            "refs-only-local-corrupt-journal-version",
            new string('b', 64));

        var response = await InvokeActivationAsync(
            registry,
            ActivateLocalPromotionSelectorRequest(
                TenantA,
                "activate-local:corrupt-authority-journal",
                0,
                selector));

        Assert.Equal(400, response.StatusCode);
        AssertErrorCode(
            response.BodyJson,
            "DESIGN_CONVERSION_MODEL_ACTIVATION_PROMOTION_AUTHORITY_READ_FAILED");
        await AssertNoActiveActivationAsync(registry, TenantA);
    }

    [Fact]
    public async Task ModelActivationRoute_ActivateLocalPromotionAmbiguousSelectorFailsClosedBeforeMutation()
    {
        var root = NewRoot();
        var model = await BuildActivationRecordAsync(root, TenantA, "refs-only-local-ambiguous", batchCount: 1);
        var duplicate = BuildPromotionAuthorityRecord(
            TenantA,
            "refs-only-local-ambiguous-duplicate",
            model,
            batchCount: 1);
        await File.AppendAllTextAsync(
            PromotionJournalPath(root),
            JsonSerializer.Serialize(duplicate, PromotionJournalJsonOptions) + Environment.NewLine,
            StrictUtf8);
        var registry = CreateRegistry(root);

        var response = await InvokeActivationAsync(
            registry,
            ActivateLocalPromotionRequest(TenantA, "activate-local:ambiguous-selector", 0, model));

        Assert.Equal(409, response.StatusCode);
        AssertErrorCode(
            response.BodyJson,
            "DESIGN_CONVERSION_MODEL_ACTIVATION_LOCAL_PROMOTION_SELECTOR_AMBIGUOUS");
        await AssertNoActiveActivationAsync(registry, TenantA);
        AssertRefsOnlyActivationResponse(response.BodyJson, model);
    }

    [Fact]
    public async Task ModelActivationRoute_ActivateLocalPromotionStaleSelectorFailsClosedBeforeMutation()
    {
        var root = NewRoot();
        var stale = await BuildActivationRecordAsync(root, TenantA, "refs-only-local-stale-a", batchCount: 1);
        await BuildActivationRecordAsync(root, TenantA, "refs-only-local-current-b", batchCount: 2);
        var registry = CreateRegistry(root);

        var response = await InvokeActivationAsync(
            registry,
            ActivateLocalPromotionRequest(TenantA, "activate-local:stale-selector", 0, stale));

        Assert.Equal(409, response.StatusCode);
        AssertErrorCode(response.BodyJson, "DESIGN_CONVERSION_MODEL_ACTIVATION_PROMOTION_NOT_CURRENT_ACTIVE");
        await AssertNoActiveActivationAsync(registry, TenantA);
        AssertRefsOnlyActivationResponse(response.BodyJson, stale);
    }

    [Fact]
    public async Task ModelActivationRoute_ActivateLocalPromotionRolledBackSelectorFailsClosedBeforeMutation()
    {
        var root = NewRoot();
        var restored = await BuildActivationRecordAsync(root, TenantA, "refs-only-local-restored-a", batchCount: 1);
        var rolledBack = await BuildActivationRecordAsync(root, TenantA, "refs-only-local-rolled-back-b", batchCount: 2);
        await AppendPromotionRollbackAuthorityRecordAsync(
            root,
            TenantA,
            "refs-only-local-rollback-b-to-a",
            rolledBack,
            restored,
            batchCount: 2);
        var registry = CreateRegistry(root);

        var response = await InvokeActivationAsync(
            registry,
            ActivateLocalPromotionRequest(TenantA, "activate-local:rolled-back-selector", 0, rolledBack));

        Assert.Equal(409, response.StatusCode);
        AssertErrorCode(response.BodyJson, "DESIGN_CONVERSION_MODEL_ACTIVATION_PROMOTION_NOT_CURRENT_ACTIVE");
        await AssertNoActiveActivationAsync(registry, TenantA);
        AssertRefsOnlyActivationResponse(response.BodyJson, rolledBack);
    }

    [Fact]
    public async Task ModelActivationRoute_ActivateLocalPromotionExactReplayDoesNotIncreaseGeneration()
    {
        var root = NewRoot();
        var model = await BuildActivationRecordAsync(root, TenantA, "refs-only-local-replay", batchCount: 1);
        var registry = CreateRegistry(root);
        var request = ActivateLocalPromotionRequest(TenantA, "activate-local:replay", 0, model);

        var first = await InvokeActivationAsync(registry, request);
        var replay = await InvokeActivationAsync(registry, request);

        AssertSuccessActivation(first.BodyJson, model.Model.ModelId, expectedGeneration: 1, replay: false);
        AssertSuccessActivation(replay.BodyJson, model.Model.ModelId, expectedGeneration: 1, replay: true);
        AssertActivatedLocalPromotionIdentity(replay.BodyJson, model);
        using var document = JsonDocument.Parse(replay.BodyJson);
        Assert.False(document.RootElement.GetProperty("applied").GetBoolean());
        Assert.True(document.RootElement.GetProperty("replay").GetBoolean());
        AssertRefsOnlyActivationResponse(replay.BodyJson, model);
    }

    private static DesignConversionModelServerEndpointRegistry CreateRegistry(
        string root,
        bool configureActivation = true,
        Func<DateTimeOffset>? utcNow = null,
        DesignConversionModelServerModelLifecycleTokenKeyRingOptions? modelLifecycleTokenKeyRing = null,
        string? routeAdmissionTenantId = null)
    {
        var composition = DesignConversionLocalServiceComposition.CreateDeterministicLocalForLiveHostTesting();
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(ArtifactDbRoot(root));
        var previousVisualCostBudgetStoreRoot = Environment.GetEnvironmentVariable(
            JsonlDesignConversionVisualCostBudgetStore.VisualCostBudgetStoreRootEnvironmentVariable);
        var previousVisualCostLedgerRoot = Environment.GetEnvironmentVariable(
            JsonlDesignConversionVisualCostLedger.VisualCostLedgerRootEnvironmentVariable);
        try
        {
            Environment.SetEnvironmentVariable(
                JsonlDesignConversionVisualCostBudgetStore.VisualCostBudgetStoreRootEnvironmentVariable,
                Path.Combine(root, "visual-cost-budget-store"));
            Environment.SetEnvironmentVariable(
                JsonlDesignConversionVisualCostLedger.VisualCostLedgerRootEnvironmentVariable,
                Path.Combine(root, "visual-cost-ledger"));
            return DesignConversionModelServerAspNetHostStartup.CreateLocalRouteRegistry(
                composition,
                new UnexpectedCandidateStrategy(),
                artifactDb,
                utcNow ?? (() => DateTimeOffset.Parse("2026-07-10T10:00:00Z")),
                activationStateRootDirectory: configureActivation ? Path.Combine(root, "activation-state") : null,
                modelLifecycleAuthToken: configureActivation ? LocalToken : null,
                modelLifecycleTokenKeyRing: configureActivation ? modelLifecycleTokenKeyRing : null,
                routeAdmissionOptions: string.IsNullOrWhiteSpace(routeAdmissionTenantId)
                    ? null
                    : new DesignConversionModelServerRouteAdmissionOptions(
                        enabled: true,
                        currentToken: RouteAdmissionCurrentToken,
                        previousToken: RouteAdmissionPreviousToken,
                        previousTokenExpiresAtUtc: DateTimeOffset.Parse("2026-07-10T10:05:00Z"),
                        tenantId: routeAdmissionTenantId));
        }
        finally
        {
            Environment.SetEnvironmentVariable(
                JsonlDesignConversionVisualCostBudgetStore.VisualCostBudgetStoreRootEnvironmentVariable,
                previousVisualCostBudgetStoreRoot);
            Environment.SetEnvironmentVariable(
                JsonlDesignConversionVisualCostLedger.VisualCostLedgerRootEnvironmentVariable,
                previousVisualCostLedgerRoot);
        }
    }

    private static DesignConversionModelServerEndpointRegistry CreateEvaluateRegistry(
        string root,
        IDesignConversionLocalArtifactDb artifactDb,
        DesignConversionActivatedModelRuntimeEvaluator activatedEvaluator,
        string routeAdmissionTenantId)
    {
        var productionFactory = typeof(DesignConversionModelServerApi).GetMethod(
            "CreateForProductionActivatedRuntime",
            BindingFlags.Static | BindingFlags.NonPublic);
        Assert.NotNull(productionFactory);
        var api = Assert.IsType<DesignConversionModelServerApi>(productionFactory.Invoke(
            null,
            [
                activatedEvaluator,
                (Func<DateTimeOffset>)(() => DateTimeOffset.Parse("2026-07-10T10:00:00Z")),
                artifactDb,
                null,
                null,
                null,
                null,
                new JsonlDesignConversionVisualCostLedger(Path.Combine(root, "visual-cost-ledger")),
                new JsonlDesignConversionVisualCostBudgetStore(Path.Combine(root, "visual-cost-budget-store"))
            ]));
        var routeAdmission = DesignConversionModelServerRouteAdmission.Create(
            new DesignConversionModelServerRouteAdmissionOptions(
                enabled: true,
                currentToken: RouteAdmissionCurrentToken,
                previousToken: RouteAdmissionPreviousToken,
                previousTokenExpiresAtUtc: DateTimeOffset.Parse("2026-07-10T10:05:00Z"),
                tenantId: routeAdmissionTenantId),
            () => DateTimeOffset.Parse("2026-07-10T10:00:00Z"));
        var registry = new DesignConversionModelServerEndpointRegistry();
        var registered = registry.RegisterDesignConversionEvaluate(new DesignConversionModelServerHttpHandler(api, routeAdmission));
        Assert.True(registered.IsSuccess, registered.ErrorMessage);
        return registry;
    }

    private static async Task<DesignConversionModelServerHttpResponse> InvokeActivationAsync(
        DesignConversionModelServerEndpointRegistry registry,
        DesignConversionModelServerModelActivationRequest request,
        string? token = LocalToken)
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (token is not null)
            headers[DesignConversionModelServerModelActivationApi.LocalAuthorizationHeaderName] = token;

        var invoked = await registry.InvokeAsync(new DesignConversionModelServerHttpRequest(
            Method: DesignConversionModelServerEndpointRegistry.ModelActivationEndpointMethod,
            Path: DesignConversionModelServerEndpointRegistry.ModelActivationEndpointPath,
            BodyJson: JsonSerializer.Serialize(request, DesignConversionModelServerHttpHandler.JsonOptions),
            ContentType: "application/json",
            Headers: headers));

        Assert.True(invoked.IsSuccess, invoked.ErrorMessage);
        return invoked.Value!;
    }

    private static async Task<DesignConversionModelServerHttpResponse> InvokeEvaluateAsync(
        DesignConversionModelServerEndpointRegistry registry,
        string? tenantId,
        string requestId,
        string? routeAdmissionToken = RouteAdmissionCurrentToken,
        bool includeTenantMetadata = true,
        bool includeTrustedVisualCostMetadata = true)
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["x-design-conversion-model-activation-test"] = "route-runtime"
        };
        if (routeAdmissionToken is not null)
        {
            headers[DesignConversionModelServerRouteAdmission.AuthorizationHeaderName] = "Bearer " + routeAdmissionToken;
            headers[DesignConversionModelServerRouteAdmission.IdempotencyHeaderName] = requestId + "-idempotency";
        }

        var invoked = await registry.InvokeAsync(new DesignConversionModelServerHttpRequest(
            Method: DesignConversionModelServerEndpointRegistry.EndpointMethod,
            Path: DesignConversionModelServerEndpointRegistry.EndpointPath,
            BodyJson: JsonSerializer.Serialize(
                CreateEvaluateRequest(
                    tenantId,
                    requestId,
                    includeTenantMetadata,
                    includeTrustedVisualCostMetadata),
                DesignConversionModelServerHttpHandler.JsonOptions),
            ContentType: "application/json",
            Headers: headers));

        Assert.True(invoked.IsSuccess, invoked.ErrorMessage);
        return invoked.Value!;
    }

    private static DesignConversionModelServerModelActivationRequest ActivateRequest(
        string tenantId,
        string idempotencyKey,
        long expectedGeneration,
        DesignConversionPromotedModelActivationRecord candidate)
        => new(
            Action: DesignConversionModelServerModelActivationHandler.ActionActivate,
            TenantId: tenantId,
            RequestedByRef: "architect:design-conversion-model-activation-route-runtime-test",
            Reason: $"activation route runtime test {idempotencyKey}",
            IdempotencyKey: idempotencyKey,
            ExpectedGeneration: expectedGeneration,
            Candidate: candidate,
            ExpectedModelVersion: candidate.Model.Version,
            RequestedAtUtc: DateTimeOffset.Parse("2026-07-10T10:00:00Z"));

    private static DesignConversionModelServerModelActivationRequest ActivateLocalPromotionRequest(
        string tenantId,
        string idempotencyKey,
        long expectedGeneration,
        DesignConversionPromotedModelActivationRecord durableRecord,
        DesignConversionModelServerLocalPromotionActivationSelector? selector = null)
    {
        var resolvedSelector = selector ?? LocalPromotionSelector(durableRecord);
        return ActivateLocalPromotionSelectorRequest(
            tenantId,
            idempotencyKey,
            expectedGeneration,
            resolvedSelector);
    }

    private static DesignConversionModelServerModelActivationRequest ActivateLocalPromotionSelectorRequest(
        string tenantId,
        string idempotencyKey,
        long expectedGeneration,
        DesignConversionModelServerLocalPromotionActivationSelector selector,
        string? expectedModelVersion = null)
        => new(
            Action: DesignConversionModelServerModelActivationHandler.ActionActivateLocalPromotion,
            TenantId: tenantId,
            RequestedByRef: "architect:design-conversion-model-activation-route-runtime-test",
            Reason: $"activation route runtime refs-only local promotion test {idempotencyKey}",
            IdempotencyKey: idempotencyKey,
            ExpectedGeneration: expectedGeneration,
            Candidate: null,
            ExpectedModelVersion: expectedModelVersion ?? selector.Version,
            RequestedAtUtc: DateTimeOffset.Parse("2026-07-10T10:00:00Z"),
            LocalPromotionSelector: selector);

    private static DesignConversionModelServerLocalPromotionActivationSelector LocalPromotionSelector(
        DesignConversionPromotedModelActivationRecord record)
        => new(
            PromotionUploadRef: record.Publication.Manifest.PromotionUploadRef!,
            PromotedModelId: record.Model.ModelId,
            Version: record.Model.Version,
            CheckpointSha256: record.Model.CheckpointSha256);

    private static DesignConversionModelServerModelActivationRequest RollbackRequest(
        string tenantId,
        string idempotencyKey,
        long expectedGeneration)
        => new(
            Action: DesignConversionModelServerModelActivationHandler.ActionRollback,
            TenantId: tenantId,
            RequestedByRef: "architect:design-conversion-model-activation-route-runtime-test",
            Reason: $"activation route runtime rollback test {idempotencyKey}",
            IdempotencyKey: idempotencyKey,
            ExpectedGeneration: expectedGeneration,
            RequestedAtUtc: DateTimeOffset.Parse("2026-07-10T10:00:00Z"));

    private static DesignConversionModelServerModelActivationRequest ReadActiveRequest(string tenantId)
        => new(
            Action: DesignConversionModelServerModelActivationHandler.ActionReadActive,
            TenantId: tenantId);

    private static DesignConversionModelServerApiRequest CreateEvaluateRequest(
        string? tenantId,
        string requestId,
        bool includeTenantMetadata = true,
        bool includeTrustedVisualCostMetadata = true)
    {
        var metadata = new Dictionary<string, string>
        {
            ["activation_route_runtime_test"] = bool.TrueString
        };
        if (includeTrustedVisualCostMetadata)
        {
            metadata[DesignConversionModelServerApi.ServerAssignedDesignTierMetadataKey] = "paid";
            metadata[DesignConversionModelServerApi.ServerAssignedMonthlyVisualCostLimitUsdMetadataKey] = "125.75";
            metadata[DesignConversionModelServerApi.DesignTierAuthorityMetadataKey] =
                DesignConversionModelServerApi.PlatformTenantConfigurationAuthority;
            metadata[DesignConversionModelServerApi.MonthlyVisualCostLimitAuthorityMetadataKey] =
                DesignConversionModelServerApi.PlatformTenantConfigurationAuthority;
            metadata[DesignConversionModelServerApi.UserIdentityAuthorityMetadataKey] =
                DesignConversionModelServerApi.VerifiedInboundPrincipalAuthority;
        }

        if (includeTenantMetadata && !string.IsNullOrWhiteSpace(tenantId))
            metadata["tenant_id"] = tenantId;

        return new DesignConversionModelServerApiRequest(
            RequestId: requestId,
            SourceKind: DesignConversionSourceKind.Figma,
            DesignTree: new DesignConversionModelServerPayloadRef(
                RefId: $"{requestId}:design-tree",
                Kind: "figma-json",
                Sha256: $"sha256:{Sha256Text(requestId + ":design-tree")}",
                Payload: "{\"kind\":\"route-runtime-design\"}",
                Format: "figma-json"),
            OriginalTree: new DesignConversionModelServerPayloadRef(
                RefId: $"{requestId}:original-tree",
                Kind: "figma-json",
                Sha256: $"sha256:{Sha256Text(requestId + ":original-tree")}",
                Payload: "{\"kind\":\"route-runtime-original\"}",
                Format: "figma-json"),
            Html: "<main><h1>Route runtime activation</h1></main>",
            Css: "body{margin:0;color:#123456;background:#f8fafc}",
            Screenshots:
            [
                new DesignConversionModelServerArtifactRef(
                    ArtifactId: $"{requestId}:source",
                    Kind: "screenshot",
                    Width: 800,
                    Height: 600,
                    Uri: $"artifact://activation-route/{requestId}/source.png",
                    Sha256: $"sha256:{Sha256Text(requestId + ":source")}")
            ],
            DesignRenders:
            [
                new DesignConversionModelServerArtifactRef(
                    ArtifactId: $"{requestId}:render",
                    Kind: "design-render",
                    Width: 800,
                    Height: 600,
                    Uri: $"artifact://activation-route/{requestId}/render.png",
                    Sha256: $"sha256:{Sha256Text(requestId + ":render")}")
            ],
            Budget: new DesignConversionModelServerBudget(
                UserId: "activation-route-runtime-user",
                Tier: "paid",
                MaxImprovementCandidates: 1,
                MaxRenderQaRuns: 1,
                BudgetUsd: 0.25m,
                Metadata: includeTrustedVisualCostMetadata
                    ? new Dictionary<string, string>(StringComparer.Ordinal)
                    {
                        ["design_tier"] = "paid",
                        ["monthly_visual_cost_limit_usd"] = "125.75"
                    }
                    : new Dictionary<string, string>(StringComparer.Ordinal)),
            Metadata: metadata);
    }

    private static DesignConversionModelServerApiRequest CreateEvaluateRequestLegacyUnused(string tenantId, string requestId)
        => new(
            RequestId: requestId,
            SourceKind: DesignConversionSourceKind.Figma,
            DesignTree: new DesignConversionModelServerPayloadRef(
                RefId: $"{requestId}:design-tree",
                Kind: "figma-json",
                Sha256: $"sha256:{Sha256Text(requestId + ":design-tree")}",
                Payload: "{\"kind\":\"route-runtime-design\"}",
                Format: "figma-json"),
            OriginalTree: new DesignConversionModelServerPayloadRef(
                RefId: $"{requestId}:original-tree",
                Kind: "figma-json",
                Sha256: $"sha256:{Sha256Text(requestId + ":original-tree")}",
                Payload: "{\"kind\":\"route-runtime-original\"}",
                Format: "figma-json"),
            Html: "<main><h1>Route runtime activation</h1></main>",
            Css: "body{margin:0;color:#123456;background:#f8fafc}",
            Screenshots:
            [
                new DesignConversionModelServerArtifactRef(
                    ArtifactId: $"{requestId}:source",
                    Kind: "screenshot",
                    Width: 800,
                    Height: 600,
                    Uri: $"artifact://activation-route/{requestId}/source.png",
                    Sha256: $"sha256:{Sha256Text(requestId + ":source")}")
            ],
            DesignRenders:
            [
                new DesignConversionModelServerArtifactRef(
                    ArtifactId: $"{requestId}:render",
                    Kind: "design-render",
                    Width: 800,
                    Height: 600,
                    Uri: $"artifact://activation-route/{requestId}/render.png",
                    Sha256: $"sha256:{Sha256Text(requestId + ":render")}")
            ],
            Budget: new DesignConversionModelServerBudget(
                UserId: "activation-route-runtime-user",
                Tier: "local",
                MaxImprovementCandidates: 1,
                MaxRenderQaRuns: 1,
                BudgetUsd: 0m),
            Metadata: new Dictionary<string, string>
            {
                ["tenant_id"] = tenantId,
                ["activation_route_runtime_test"] = bool.TrueString
            });

    private static void AssertSuccessActivation(
        string bodyJson,
        string expectedModelId,
        long expectedGeneration,
        bool replay,
        string expectedWeightsSource = "local_checkpoint_feature_weights")
    {
        using var document = JsonDocument.Parse(bodyJson);
        var root = document.RootElement;
        Assert.Equal(200, root.GetProperty("statusCode").GetInt32());
        Assert.Equal("model-activation-completed", root.GetProperty("contractStatus").GetString());
        Assert.Equal(expectedGeneration, root.GetProperty("generation").GetInt64());
        Assert.Equal(replay, root.GetProperty("replay").GetBoolean());
        Assert.Equal(expectedModelId, root.GetProperty("activeVersion").GetProperty("modelId").GetString());
        Assert.False(root.GetProperty("productReady").GetBoolean());

        var active = root.GetProperty("activeVersion");
        Assert.True(active.TryGetProperty("checkpointPathRef", out _));
        Assert.False(active.TryGetProperty("checkpointPath", out _));
        Assert.Equal(expectedWeightsSource, active.GetProperty("localCheckpointWeightsSource").GetString());
        var metadata = root.GetProperty("boundaryMetadata");
        Assert.Equal(bool.TrueString, metadata.GetProperty("model_activation_internal_token_required").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("model_activation_public_anonymous_control").GetString());
        Assert.Equal(expectedWeightsSource, metadata.GetProperty("model_activation_checkpoint_weights_source").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("model_activation_checkpoints_deleted").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("product_ready").GetString());
    }

    private static void AssertActivatedLocalPromotionIdentity(
        string bodyJson,
        DesignConversionPromotedModelActivationRecord expected)
    {
        using var document = JsonDocument.Parse(bodyJson);
        var active = document.RootElement.GetProperty("activeVersion");
        Assert.Equal(expected.Model.ModelId, active.GetProperty("modelId").GetString());
        Assert.Equal(expected.Model.Version, active.GetProperty("version").GetString());
        Assert.Equal(expected.Model.CheckpointSha256, active.GetProperty("checkpointSha256").GetString());
        Assert.Equal(
            expected.Publication.Manifest.PromotionUploadRef,
            active.GetProperty("promotionUploadRef").GetString());
        Assert.False(document.RootElement.GetProperty("productReady").GetBoolean());

        var metadata = document.RootElement.GetProperty("boundaryMetadata");
        Assert.Equal(
            bool.TrueString,
            metadata.GetProperty("modelserver_model_lifecycle_refs_only_local_promotion_selector_verified").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("product_ready").GetString());
    }

    private static void AssertRefsOnlyActivationResponse(
        string bodyJson,
        DesignConversionPromotedModelActivationRecord record)
    {
        AssertDoesNotExposeLifecycleTokens(bodyJson);
        Assert.DoesNotContain("\"candidate\"", bodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("\"publication\"", bodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("\"promotedLocalModel\"", bodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain("\"promotionOutputDirectory\"", bodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain("\"promotedCheckpointPath\"", bodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain("\"checkpointPath\"", bodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(record.Model.CheckpointPath, bodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(
            record.Publication.Manifest.PromotionArtifacts?.CheckpointPath ?? string.Empty,
            bodyJson,
            StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(
            record.Publication.Manifest.PromotionArtifacts?.TokenizerPath ?? string.Empty,
            bodyJson,
            StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(
            record.Publication.Manifest.PromotionArtifacts?.PolicyPath ?? string.Empty,
            bodyJson,
            StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(
            record.Publication.Manifest.PromotionArtifacts?.SourceEncoderPath ?? string.Empty,
            bodyJson,
            StringComparison.OrdinalIgnoreCase);
    }

    private static async Task AssertNoActiveActivationAsync(
        DesignConversionModelServerEndpointRegistry registry,
        string tenantId)
    {
        var readback = await InvokeActivationAsync(registry, ReadActiveRequest(tenantId));
        Assert.Equal(404, readback.StatusCode);
        AssertErrorCode(
            readback.BodyJson,
            "DESIGN_CONVERSION_MODEL_ACTIVATION_ACTIVE_VERSION_NOT_FOUND");
    }

    private static void AssertEvaluateSelectedModel(
        string bodyJson,
        string expectedModelId,
        string expectedWeightsSource = "local_checkpoint_feature_weights")
    {
        using var document = JsonDocument.Parse(bodyJson);
        var root = document.RootElement;
        Assert.Equal(200, root.GetProperty("statusCode").GetInt32());
        var evidenceRefs = root.GetProperty("evidenceRefs")
            .EnumerateArray()
            .Select(item => item.GetString())
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .ToArray();
        var warnings = root.GetProperty("warnings")
            .EnumerateArray()
            .Select(item => item.GetString())
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .ToArray();
        var metadata = root.GetProperty("boundaryMetadata");
        Assert.False(root.GetProperty("trainedModelAuthority").GetBoolean());
        Assert.True(root.GetProperty("notTrainedModelAuthority").GetBoolean());
        Assert.Equal(expectedModelId, metadata.GetProperty("local_promoted_model_id").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("local_model_promotion_verified").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("local_promoted_checkpoint_loaded").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("local_promoted_checkpoint_real_weights").GetString());
        Assert.Equal(expectedWeightsSource, metadata.GetProperty("local_promoted_checkpoint_weights_source").GetString());
        Assert.Equal("critic_local_checkpoint_feature_adjusted_score", metadata.GetProperty("logical_flow_primary_metric_name").GetString());
        Assert.True(
            int.Parse(metadata.GetProperty("LocalCheckpointFeatureContributionCount").GetString() ?? "0", System.Globalization.CultureInfo.InvariantCulture) > 0);
        Assert.Equal(bool.TrueString, metadata.GetProperty("local_checkpoint_weights_configured").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("trained_model_authority").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("not_trained_model_authority").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("product_ready").GetString());
        Assert.Contains(DesignConversionMachine.LearnedInferenceAppliedEvidence, evidenceRefs);
        Assert.Contains(DesignConversionMachine.LocalCheckpointFeatureAdjustmentEvidence, evidenceRefs);
        Assert.Contains("logical-flow:design-conversion-routing-flow", evidenceRefs);
        Assert.Contains("logical-orchestrator-selected-flow:design-conversion-routing-flow", evidenceRefs);
        Assert.Contains(DesignConversionMachine.RuntimeScaffoldNotTrainedModelWarning, warnings);
    }

    private static void AssertEvaluateSelectedDpoCheckpointModel(
        string bodyJson,
        string expectedModelId,
        string expectedWeightsSource)
    {
        using var document = JsonDocument.Parse(bodyJson);
        var root = document.RootElement;
        Assert.Equal(200, root.GetProperty("statusCode").GetInt32());
        var evidenceRefs = root.GetProperty("evidenceRefs")
            .EnumerateArray()
            .Select(item => item.GetString())
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .ToArray();
        var warnings = root.GetProperty("warnings")
            .EnumerateArray()
            .Select(item => item.GetString())
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .ToArray();
        var metadata = root.GetProperty("boundaryMetadata");
        Assert.False(root.GetProperty("trainedModelAuthority").GetBoolean());
        Assert.True(root.GetProperty("notTrainedModelAuthority").GetBoolean());
        Assert.Equal(expectedModelId, metadata.GetProperty("local_promoted_model_id").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("local_model_promotion_verified").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("local_promoted_checkpoint_loaded").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("local_promoted_checkpoint_real_weights").GetString());
        Assert.Equal(expectedWeightsSource, metadata.GetProperty("local_promoted_checkpoint_weights_source").GetString());
        Assert.Equal("critic_input_evidence_score", metadata.GetProperty("logical_flow_primary_metric_name").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("local_checkpoint_weights_configured").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("direct_dpo_policy_inference_applied").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("direct_dpo_policy_score_authority").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("direct_dpo_policy_acceptance_authority").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("baseline_visual_verdict_acceptance_authority").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("trained_model_authority").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("not_trained_model_authority").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("product_ready").GetString());
        Assert.Contains(DesignConversionMachine.DpoPolicyDirectInferenceConfiguredEvidence, evidenceRefs);
        Assert.Contains(DesignConversionMachine.DpoPolicyRuntimePairContentUnavailableEvidence, evidenceRefs);
        Assert.Contains(DesignConversionMachine.DpoPolicyDirectInferenceNoRawPayloadEvidence, evidenceRefs);
        Assert.Contains(DesignConversionMachine.DpoTrainerPolicyCheckpointFreshLoadedEvidence, evidenceRefs);
        Assert.Contains(DesignConversionMachine.DpoTrainerPolicyCheckpointConvertedEvidence, evidenceRefs);
        Assert.Contains("logical-flow:design-conversion-routing-flow", evidenceRefs);
        Assert.Contains("logical-orchestrator-selected-flow:design-conversion-routing-flow", evidenceRefs);
        Assert.DoesNotContain(DesignConversionMachine.LearnedInferenceAppliedEvidence, evidenceRefs);
        Assert.DoesNotContain(DesignConversionMachine.LocalCheckpointFeatureAdjustmentEvidence, evidenceRefs);
        Assert.Contains(DesignConversionMachine.DpoPolicyRuntimePairContentUnavailableWarning, warnings);
    }

    private static void AssertRuntimeSelection(
        string bodyJson,
        bool loadedFresh,
        bool preservedPrevious,
        bool? freshLoadAttempted = null)
    {
        using var document = JsonDocument.Parse(bodyJson);
        var root = document.RootElement;
        var evidenceRefs = root.GetProperty("evidenceRefs")
            .EnumerateArray()
            .Select(item => item.GetString())
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .ToArray();
        var warnings = root.GetProperty("warnings")
            .EnumerateArray()
            .Select(item => item.GetString())
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .ToArray();

        if (freshLoadAttempted is not null)
            Assert.Contains($"active_model_runtime_fresh_load_attempted={freshLoadAttempted}", warnings);

        Assert.Contains($"active_model_runtime_fresh_load_succeeded={loadedFresh}", warnings);
        Assert.Contains($"active_model_runtime_previous_preserved={preservedPrevious}", warnings);
        var expectedRuntimeEvidence = loadedFresh
            ? DesignConversionActivatedModelRuntimeEvaluator.FreshLoadEvidence
            : freshLoadAttempted == false
                ? DesignConversionActivatedModelRuntimeEvaluator.CachedRuntimeReusedEvidence
                : DesignConversionActivatedModelRuntimeEvaluator.PreviousRuntimePreservedEvidence;
        Assert.Contains(expectedRuntimeEvidence, evidenceRefs);
        if (loadedFresh || freshLoadAttempted == false)
        {
            Assert.DoesNotContain(
                DesignConversionActivatedModelRuntimeEvaluator.PreviousRuntimePreservedEvidence,
                evidenceRefs);
        }

        if (loadedFresh || freshLoadAttempted == true)
        {
            Assert.DoesNotContain(
                DesignConversionActivatedModelRuntimeEvaluator.CachedRuntimeReusedEvidence,
                evidenceRefs);
        }
    }

    private static void AssertEvaluationPromotedModelIdentity(
        string bodyJson,
        string expectedModelId,
        string expectedCheckpointSha256)
    {
        using var document = JsonDocument.Parse(bodyJson);
        var metadata = document.RootElement.GetProperty("boundaryMetadata");
        Assert.Equal(expectedModelId, metadata.GetProperty("local_promoted_model_id").GetString());
        Assert.Equal(expectedCheckpointSha256, metadata.GetProperty("local_promoted_checkpoint_sha256").GetString());
    }

    private static void AssertActiveModelNotReadyFailedClosed(
        DesignConversionModelServerHttpResponse response,
        params string[] forbiddenValues)
    {
        Assert.Equal(503, response.StatusCode);
        AssertActiveRuntimeLoadFailedClosed(
            response,
            DesignConversionActivatedModelRuntimeEvaluator.ActiveModelNotReadyErrorCode,
            true,
            forbiddenValues);
    }

    private static void AssertActiveRuntimeLoadFailedClosed(
        DesignConversionModelServerHttpResponse response,
        string expectedErrorCode,
        bool expectedRuntimeInvoked,
        params string[] forbiddenValues)
    {
        Assert.False(response.StatusCode is >= 200 and < 300, response.BodyJson);
        using var document = JsonDocument.Parse(response.BodyJson);
        var root = document.RootElement;
        Assert.Equal(response.StatusCode, root.GetProperty("statusCode").GetInt32());
        Assert.Equal(expectedErrorCode, root.GetProperty("errorCode").GetString());
        Assert.False(root.TryGetProperty("evidenceRefs", out _));
        Assert.False(root.TryGetProperty("runtimeStatus", out _));
        Assert.False(root.TryGetProperty("trainingReadiness", out _));
        Assert.False(root.TryGetProperty("contractStatus", out _));
        Assert.False(root.TryGetProperty("jobId", out _));
        Assert.False(root.TryGetProperty("requestId", out _));
        Assert.False(root.TryGetProperty("learnedInferenceApplied", out _));
        Assert.False(root.TryGetProperty("modelTrainingVerified", out _));
        Assert.False(root.TryGetProperty("modelUploadReady", out _));
        Assert.False(root.TryGetProperty("warnings", out _));
        Assert.False(root.TryGetProperty("html", out _));
        Assert.False(root.TryGetProperty("css", out _));
        Assert.False(root.TryGetProperty("designTree", out _));
        Assert.False(root.TryGetProperty("originalTree", out _));
        Assert.False(root.TryGetProperty("promotedLocalModel", out _));

        var metadata = root.GetProperty("boundaryMetadata");
        Assert.Equal(expectedRuntimeInvoked.ToString(), metadata.GetProperty("runtime_invoked").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_html_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_css_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_design_tree_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_user_private_data_included").GetString());

        Assert.DoesNotContain(
            DesignConversionActivatedModelRuntimeEvaluator.PreviousRuntimePreservedEvidence,
            response.BodyJson,
            StringComparison.Ordinal);
        Assert.DoesNotContain("active_model_runtime_previous_preserved", response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain("local_promoted_model_id", response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain("active_model_id", response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain("active_model_checkpoint", response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain("CHECKPOINT_PATH:", response.BodyJson, StringComparison.Ordinal);

        foreach (var forbiddenValue in forbiddenValues.Where(value => !string.IsNullOrWhiteSpace(value)).Distinct(StringComparer.Ordinal))
            Assert.DoesNotContain(forbiddenValue, response.BodyJson, StringComparison.OrdinalIgnoreCase);
    }

    private static DesignConversionInternalEvaluateRequest CreateInternalEvaluateRequest(
        string? tenantId,
        string requestId,
        IReadOnlyDictionary<string, string> provenance)
        => new(
            JobId: $"job:{requestId}",
            RequestId: requestId,
            SourceKind: DesignConversionSourceKind.Figma,
            Html: "<main><h1>Route runtime activation</h1></main>",
            Css: "body{margin:0;color:#123456;background:#f8fafc}",
            DesignTreePayload: new DesignTreePayload(
                PayloadId: $"{requestId}:design-tree",
                Format: "figma-json",
                Content: "{\"kind\":\"route-runtime-design\"}",
                Sha256: $"sha256:{Sha256Text(requestId + ":design-tree")}",
                Metadata: new Dictionary<string, string>(StringComparer.Ordinal)),
            OriginalTreePayload: new DesignTreePayload(
                PayloadId: $"{requestId}:original-tree",
                Format: "figma-json",
                Content: "{\"kind\":\"route-runtime-original\"}",
                Sha256: $"sha256:{Sha256Text(requestId + ":original-tree")}",
                Metadata: new Dictionary<string, string>(StringComparer.Ordinal)),
            Screenshots:
            [
                new DesignRenderArtifact(
                    ArtifactId: $"{requestId}:source",
                    Kind: "screenshot",
                    Width: 800,
                    Height: 600,
                    Uri: $"artifact://activation-route/{requestId}/source.png",
                    Sha256: $"sha256:{Sha256Text(requestId + ":source")}",
                    Metadata: new Dictionary<string, string>(StringComparer.Ordinal))
            ],
            DesignRenders:
            [
                new DesignRenderArtifact(
                    ArtifactId: $"{requestId}:render",
                    Kind: "design-render",
                    Width: 800,
                    Height: 600,
                    Uri: $"artifact://activation-route/{requestId}/render.png",
                    Sha256: $"sha256:{Sha256Text(requestId + ":render")}",
                    Metadata: new Dictionary<string, string>(StringComparer.Ordinal))
            ],
            Budget: new DesignConversionBudgetMetadata(
                UserId: "activation-route-runtime-user",
                Tier: "paid",
                MaxImprovementCandidates: 1,
                MaxRenderQaRuns: 1,
                BudgetUsd: 0m,
                Metadata: string.IsNullOrWhiteSpace(tenantId)
                    ? new Dictionary<string, string>(StringComparer.Ordinal)
                    : new Dictionary<string, string>(StringComparer.Ordinal)
                    {
                        ["tenant_id"] = tenantId
                    }),
            RenderResolutions:
            [
                new RenderQaResolution(
                    ResolutionId: "route-runtime-800x600",
                    Width: 800,
                    Height: 600,
                    DeviceScaleFactor: 1d)
            ],
            AiTeacherEvidenceRefs: [],
            DeterministicEvidenceRefs: new Dictionary<string, string>(StringComparer.Ordinal),
            Provenance: provenance);

    private static IReadOnlyDictionary<string, string> BuildTrustedRouteAdmissionProvenance(string tenantId)
    {
        var tenant = DesignConversionModelActivationRegistry.TryBuildTenantIdentity(tenantId);
        Assert.True(tenant.IsSuccess, tenant.ErrorMessage);

        return new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [DesignConversionModelServerApi.ServerOwnedRouteAdmissionTenantBoundProvenanceKey] = bool.TrueString,
            [DesignConversionModelServerApi.ServerOwnedRouteAdmissionTenantIdProvenanceKey] = tenant.Value!.TenantId,
            [DesignConversionModelServerApi.ServerOwnedRouteAdmissionTenantIdRefProvenanceKey] = tenant.Value.TenantIdRef,
            [DesignConversionModelServerApi.ServerOwnedRouteAdmissionScopeRefProvenanceKey] = "route-runtime-direct-scope-ref",
            ["request_metadata:tenant_id"] = tenant.Value.TenantId,
            ["active_model_runtime_tenant_context_source"] = "server_owned_route_admission"
        };
    }

    private static void AssertErrorCode(string bodyJson, string expectedErrorCode)
    {
        using var document = JsonDocument.Parse(bodyJson);
        Assert.Equal(expectedErrorCode, document.RootElement.GetProperty("errorCode").GetString());
    }

    private static string ReadActiveActivationRef(string bodyJson)
    {
        using var document = JsonDocument.Parse(bodyJson);
        return document.RootElement.GetProperty("activeVersion").GetProperty("activationRef").GetString() ?? string.Empty;
    }

    private static void AssertDoesNotExposeLifecycleTokens(string bodyJson)
    {
        Assert.DoesNotContain(LocalToken, bodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(PreviousLocalToken, bodyJson, StringComparison.Ordinal);
    }

    private static async Task AssertInvalidKeyRingConfigReturnsNonReadyAsync(
        string root,
        DesignConversionModelServerModelLifecycleTokenKeyRingOptions options,
        string expectedErrorCode,
        params string[] forbiddenTokens)
    {
        var registry = CreateRegistry(root, modelLifecycleTokenKeyRing: options);
        var response = await InvokeActivationAsync(
            registry,
            ReadActiveRequest(TenantA),
            token: LocalToken);

        Assert.Equal(503, response.StatusCode);
        AssertErrorCode(response.BodyJson, expectedErrorCode);
        foreach (var forbiddenToken in forbiddenTokens)
        {
            if (!string.IsNullOrEmpty(forbiddenToken))
                Assert.DoesNotContain(forbiddenToken, response.BodyJson, StringComparison.Ordinal);
        }
    }

    internal static async Task<DesignConversionPromotedModelActivationRecord> BuildActivationRecordAsync(
        string root,
        string tenantId,
        string name,
        int batchCount,
        bool persistDurablePromotion = true,
        bool evaluatedDpoPromotion = false,
        int? directDpoWeightSeed = null)
    {
        var artifactDbRoot = ArtifactDbRoot(root);
        var runId = $"{name}-authority-run";
        var outputDirectory = Path.Combine(artifactDbRoot, "models", name);
        Directory.CreateDirectory(artifactDbRoot);
        var resolvedPaths =
            DesignConversionDpoPromotionArtifactContractCodec.ResolvePaths(
                artifactDbRoot,
                outputDirectory,
                runId);
        Assert.True(resolvedPaths.IsSuccess, resolvedPaths.ErrorMessage);
        var paths = resolvedPaths.Value!;
        Directory.CreateDirectory(paths.OutputDirectory);
        var machine = new DesignConversionMachine();
        for (var index = 0; index < batchCount; index++)
        {
            var trained = await DesignConversionLegalTeacherCapabilityTestFactory
                .TrainMachineStrictAsync(machine, new LearningBatch<DesignDomain>(
                BatchId: $"{name}-batch-{index}",
                DomainTag: DesignConversionMachine.DomainTag,
                TrainingItems:
                [
                    new LearningTrainingItem(
                        ItemId: $"{name}-item-{index}",
                        Prompt: JsonSerializer.Serialize(new Dictionary<string, object>
                        {
                            ["unit_id"] = DesignConversionRuntimeScaffoldCriticUnit.UnitId,
                            ["source_kind"] = "Figma",
                            ["source_manifest_hash"] = $"{name}-manifest-source-{index}",
                            ["source_identity_hash"] = $"{name}-identity-{index}",
                            ["source_structure_hash"] = $"{name}-structure-{index}"
                        }),
                        ChosenResponse: "{\"html\":\"<main class=\\\"layout-a\\\"></main>\",\"css\":\".layout-a{display:grid;gap:16px}\"}",
                        RejectedResponse: "{\"html\":\"<main class=\\\"layout-b\\\"></main>\",\"css\":\".layout-b{display:block;gap:2px}\"}")
                ],
                SourceIds: [$"{name}-source-{index}"]));
            Assert.True(trained.IsSuccess, trained.ErrorMessage);
        }

        var checkpointPath = paths.RuntimeCheckpointPath;
        if (directDpoWeightSeed is null)
        {
            var save = await machine.SaveCheckpointAsync(checkpointPath);
            Assert.True(save.IsSuccess, save.ErrorMessage);
        }
        else
        {
            await SaveActivatedDirectDpoCheckpointAsync(
                machine,
                checkpointPath,
                directDpoWeightSeed.Value);
        }
        var load = await DesignConversionMachine.LoadCheckpointAsync(checkpointPath);
        Assert.True(load.IsSuccess, load.ErrorMessage);
        Assert.True(DesignConversionMachine.CheckpointHasRealLearnedWeights(load.Value!.Checkpoint));

        var tokenizerPath = paths.TokenizerArtifactPath;
        var policyPath = paths.PolicyCheckpointContractPath;
        var sourceEncoderPath = paths.SourceEncoderContractPath;
        await File.WriteAllTextAsync(
            tokenizerPath,
            "{\"kind\":\"activation-route-tokenizer\"}",
            StrictUtf8);
        await File.WriteAllTextAsync(
            policyPath,
            "{\"kind\":\"activation-route-policy-contract\"}",
            StrictUtf8);
        await File.WriteAllTextAsync(
            sourceEncoderPath,
            "{\"kind\":\"activation-route-source-encoder-contract\"}",
            StrictUtf8);

        var checkpointFullPath = Path.GetFullPath(checkpointPath);
        var checkpointSha = await ReadSha256Async(checkpointFullPath);
        var policyCheckpointFileName =
            load.Value.Checkpoint.DpoPolicyCheckpointRef?.PolicyCheckpointFileName ??
            $"{paths.SafeRunId}-policy-checkpoint.json";
        var policyCheckpointPath = Path.Combine(
            paths.OutputDirectory,
            policyCheckpointFileName);
        if (!File.Exists(policyCheckpointPath))
        {
            await File.WriteAllTextAsync(
                policyCheckpointPath,
                "{\"kind\":\"activation-route-policy-checkpoint\"}",
                StrictUtf8);
        }

        var manifestId = $"{name}-manifest";
        var manifestRef = $"manifest://design-conversion/{manifestId}";
        var promotionRef = DesignConversionPromotedLocalModelLoader.LocalOnlyPromotionRefPrefix +
                           Uri.EscapeDataString(manifestId);
        var artifactSetId = $"dpo-promotion-set-{checkpointSha}";
        var artifacts = new DesignConversionTrainingManifestArtifacts(
            checkpointFullPath,
            Path.GetFullPath(tokenizerPath),
            Path.GetFullPath(policyPath))
        {
            SourceEncoderPath = Path.GetFullPath(sourceEncoderPath),
            RuntimeCheckpointArtifact =
                await BuildActivationArtifactDescriptorAsync(
                    "design_conversion_dpo_runtime_checkpoint",
                    checkpointFullPath,
                    artifactSetId,
                    checkpointSha),
            TokenizerArtifact =
                await BuildActivationArtifactDescriptorAsync(
                    "design_conversion_dpo_utf8_byte_tokenizer_artifact",
                    tokenizerPath,
                    artifactSetId,
                    checkpointSha),
            SourceEncoderContractArtifact =
                await BuildActivationArtifactDescriptorAsync(
                    "design_conversion_dpo_source_encoder_contract",
                    sourceEncoderPath,
                    artifactSetId,
                    checkpointSha),
            PolicyCheckpointContractArtifact =
                await BuildActivationArtifactDescriptorAsync(
                    "design_conversion_dpo_policy_checkpoint_contract",
                    policyPath,
                    artifactSetId,
                    checkpointSha),
            PolicyCheckpointArtifact =
                await BuildActivationArtifactDescriptorAsync(
                    DesignConversionDpoTrainerRouteBGate.PolicyCheckpointArtifactKind,
                    policyCheckpointPath,
                    artifactSetId,
                    checkpointSha)
        };
        var manifest = new DesignConversionTrainingManifest(
            ManifestId: manifestId,
            MachineId: load.Value.Checkpoint.MachineId,
            DomainTag: load.Value.Checkpoint.DomainTag,
            Version: load.Value.Checkpoint.Version,
            CheckpointRef: checkpointFullPath,
            ManifestRef: manifestRef,
            CreatedAtUtc: DateTimeOffset.Parse("2026-07-10T10:00:00Z"),
            AcceptedRecordCount: batchCount,
            QuarantinedRecordCount: 0,
            UnitIds: [DesignConversionRuntimeScaffoldCriticUnit.UnitId],
            Batches:
            [
                new DesignConversionTrainingManifestBatch(
                    BatchId: $"{name}-manifest-batch",
                    UnitId: DesignConversionRuntimeScaffoldCriticUnit.UnitId,
                    RecordCount: batchCount,
                    RecordIds: Enumerable.Range(0, batchCount).Select(index => $"{name}-record-{index}").ToArray(),
                    SourceIds: Enumerable.Range(0, batchCount).Select(index => $"{name}-source-{index}").ToArray())
            ],
            EvidenceRefs: BuildManifestEvidenceRefs(checkpointFullPath, evaluatedDpoPromotion),
            ModelServerUploadAttempted: false,
            ModelServerPromotionClaimed: false,
            DeterministicLogicClaimedAsLearning: false)
        {
            PromotionArtifacts = artifacts,
            PromotionUploadRef = promotionRef,
            LocalPromotionReceiptWritten = true,
            PromotionReceiptKind = "local_receipt_only"
        };
        var decision = new DesignConversionPromotionDecision(
            DesignConversionPromotionDecisionKind.Promote,
            ["ALL_PROMOTION_GATES_PASSED"],
            ["DESIGN_CONVERSION_PROMOTION_GATES_PASSED"]);
        var receipt = new DesignConversionPromotionUploadReceipt(
            UploadId: $"{name}-local-receipt",
            RemoteRef: promotionRef,
            UploadedAtUtc: DateTimeOffset.Parse("2026-07-10T10:00:00Z"),
            EvidenceRefs: ["DESIGN_CONVERSION_LOCAL_PROMOTION_RECEIPT_WRITTEN"]);
        var publication = new DesignConversionPromotionPublicationResult(decision, manifest, receipt);
        var modelId = $"{manifestId}:{checkpointSha[..12]}";
        var modelEvidenceRefs = new List<string>
        {
            DesignConversionPromotedLocalModelLoader.LocalPromotionBoundary,
            "DESIGN_CONVERSION_LOCAL_PROMOTION_RECEIPT_WRITTEN",
            "DESIGN_CONVERSION_PROMOTED_LOCAL_CHECKPOINT_FRESH_LOADED",
            "DESIGN_CONVERSION_PROMOTED_LOCAL_CHECKPOINT_REAL_WEIGHTS_VERIFIED"
        };
        if (load.Value.Checkpoint.DpoPolicyCheckpointRef is { } policyRef)
        {
            modelEvidenceRefs.Add(DesignConversionMachine.DpoTrainerPolicyCheckpointConvertedEvidence);
            modelEvidenceRefs.Add(DesignConversionMachine.DpoTrainerPolicyCheckpointFreshLoadedEvidence);
            modelEvidenceRefs.Add($"dpo_policy_checkpoint_sha256:{policyRef.Sha256}");
            modelEvidenceRefs.Add($"dpo_policy_checkpoint_file:{policyRef.PolicyCheckpointFileName}");
        }

        var model = new DesignConversionPromotedLocalModel(
            ModelId: modelId,
            ManifestId: manifest.ManifestId,
            ManifestRef: manifest.ManifestRef,
            CheckpointPath: checkpointFullPath,
            CheckpointSha256: checkpointSha,
            MachineId: load.Value.Checkpoint.MachineId,
            DomainTag: load.Value.Checkpoint.DomainTag,
            Version: load.Value.Checkpoint.Version,
            LearnedWeightArtifactCount: DesignConversionMachine.CountRealLearnedWeightArtifacts(load.Value.Checkpoint),
            LearnedWeightObservationCount: DesignConversionMachine.CountRealLearnedWeightObservations(load.Value.Checkpoint),
            CheckpointFreshLoadVerified: true,
            CheckpointContainsRealTrainedWeights: true,
            PromotionManifestClaimed: true,
            LocalOnly: true,
            EvidenceRefs: modelEvidenceRefs
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray());

        var record = new DesignConversionPromotedModelActivationRecord(
            DesignConversionModelActivationRegistry.BuildTenantIdRef(tenantId),
            publication,
            model);
        if (persistDurablePromotion)
            await PersistPromotionAuthorityRecordAsync(root, tenantId, name, record, batchCount);

        return record;
    }

    private static string[] BuildManifestEvidenceRefs(
        string checkpointFullPath,
        bool evaluatedDpoPromotion)
    {
        var evidence = new List<string>
        {
            "DESIGN_CONVERSION_LOCAL_PROMOTION_RECEIPT_WRITTEN",
            "MODEL_SERVER_UPLOAD_NOT_ATTEMPTED_LOCAL_RECEIPT",
            $"CHECKPOINT_PATH:{checkpointFullPath}"
        };
        if (evaluatedDpoPromotion)
            evidence.Add(EvaluatedDpoPromotionEvidence);
        return evidence.ToArray();
    }

    private static async Task PersistPromotionAuthorityRecordAsync(
        string root,
        string tenantId,
        string name,
        DesignConversionPromotedModelActivationRecord record,
        int batchCount)
    {
        var promotion = BuildPromotionAuthorityRecord(tenantId, name, record, batchCount);
        var db = new JsonlDesignConversionLocalArtifactDb(ArtifactDbRoot(root));
        var persisted =
            await db.PersistServerOwnedLocalPromotionAuthorityForTestingAsync(
                promotion,
                $"idempotency:{name}-authority",
                CancellationToken.None);
        Assert.True(persisted.IsSuccess, persisted.ErrorMessage);
        var records = await db.ListTrainingIntakeLocalPromotionRecordsAsync();
        Assert.True(records.IsSuccess, records.ErrorMessage);
        Assert.Contains(
            records.Value!,
            candidate => string.Equals(
                candidate.PromotedLocalModel?.ModelId,
                record.Model.ModelId,
                StringComparison.Ordinal));
    }

    private static async Task AppendPromotionRollbackAuthorityRecordAsync(
        string root,
        string tenantId,
        string name,
        DesignConversionPromotedModelActivationRecord target,
        DesignConversionPromotedModelActivationRecord restored,
        int batchCount)
    {
        _ = tenantId;
        var db = new JsonlDesignConversionLocalArtifactDb(ArtifactDbRoot(root));
        var records = await db.ListTrainingIntakeLocalPromotionRecordsAsync();
        Assert.True(records.IsSuccess, records.ErrorMessage);
        var active = JsonlDesignConversionLocalArtifactDb
            .ResolveCurrentLocalPromotionActiveState(records.Value!);
        Assert.Equal(DesignConversionLocalPromotionActiveStateKind.Active, active.Kind);
        Assert.NotNull(active.Active);
        Assert.Equal(
            target.Publication.Manifest.PromotionUploadRef,
            active.Active!.ActiveLocalModelRef);
        var rollback = await db.RollbackTrainingIntakeLocalPromotionAsync(
            target.Publication.Manifest.PromotionUploadRef!,
            $"reviewer:{name}",
            $"restore:{restored.Model.ModelId}",
            requestedAtUtc: null,
            ct: CancellationToken.None);
        Assert.True(
            rollback.IsSuccess,
            $"{rollback.ErrorCode}: {rollback.ErrorMessage}");
        Assert.True(rollback.Value!.RollbackApplied);
        Assert.True(rollback.Value.PriorStateRestored);
        Assert.Equal(
            restored.Publication.Manifest.PromotionUploadRef,
            rollback.Value.RestoredActiveLocalModelRef);
        Assert.Equal(
            restored.Model.CheckpointSha256,
            rollback.Value.RestoredActiveCheckpointSha256);
    }

    private static string PromotionJournalPath(string root)
        => Path.Combine(
            ArtifactDbRoot(root),
            JsonlDesignConversionLocalArtifactDb.TrainingIntakeLocalPromotionFileName);

    private static string ArtifactDbRoot(string root)
        => Path.Combine(root, "artifact-db");

    private static DesignConversionPromotedModelActivationRecord WithArtifactPaths(
        DesignConversionPromotedModelActivationRecord record,
        string? checkpointPath = null,
        string? tokenizerPath = null,
        string? policyPath = null,
        string? sourceEncoderPath = null)
    {
        Assert.NotNull(record.Publication.Manifest.PromotionArtifacts);
        var artifacts = record.Publication.Manifest.PromotionArtifacts!;
        var nextCheckpointPath = checkpointPath ?? artifacts.CheckpointPath;
        var nextArtifacts = artifacts with
        {
            CheckpointPath = nextCheckpointPath,
            TokenizerPath = tokenizerPath ?? artifacts.TokenizerPath,
            PolicyPath = policyPath ?? artifacts.PolicyPath,
            SourceEncoderPath = sourceEncoderPath ?? artifacts.SourceEncoderPath
        };
        var nextManifest = record.Publication.Manifest with
        {
            CheckpointRef = nextCheckpointPath,
            PromotionArtifacts = nextArtifacts
        };

        return record with
        {
            Publication = record.Publication with { Manifest = nextManifest },
            Model = checkpointPath is null
                ? record.Model
                : record.Model with { CheckpointPath = checkpointPath }
        };
    }

    private static DesignConversionMachineCheckpoint RemoveRealLearnedWeights(
        DesignConversionMachineCheckpoint checkpoint)
        => checkpoint with
        {
            CriticUnit = RemoveRealLearnedWeights(checkpoint.CriticUnit),
            ImproverUnit = RemoveRealLearnedWeights(checkpoint.ImproverUnit)
        };

    private static DesignConversionRuntimeUnitCheckpoint RemoveRealLearnedWeights(
        DesignConversionRuntimeUnitCheckpoint unit)
        => unit with
        {
            ImplementationStatus = "runtime_scaffold_without_real_learned_weights",
            LearnedWeightsPresent = false,
            LearnedWeights = []
        };

    private static async Task TamperActiveCheckpointWeightsSourceAsync(
        string root,
        string localCheckpointWeightsSource)
    {
        var statePath = Path.Combine(
            root,
            "activation-state",
            "design-conversion-model-activation-state.json");
        var node = JsonNode.Parse(await File.ReadAllTextAsync(statePath, StrictUtf8));
        Assert.NotNull(node);
        var tenants = node!["tenants"]?.AsArray();
        Assert.NotNull(tenants);
        Assert.NotEmpty(tenants!);
        var activeVersion = tenants![0]?["activeVersion"]?.AsObject();
        Assert.NotNull(activeVersion);
        activeVersion!["localCheckpointWeightsSource"] = localCheckpointWeightsSource;
        await File.WriteAllTextAsync(
            statePath,
            node.ToJsonString(new JsonSerializerOptions(JsonSerializerDefaults.Web) { WriteIndented = true }),
            StrictUtf8);
    }

    private static DesignConversionActivatedModelVersion BuildActivationVersionForSourceInference(
        bool checkpointContainsRealTrainedWeights,
        IReadOnlyList<string> evidenceRefs)
        => new(
            ActivationRef: "sha256:activation-ref:source-inference",
            ModelId: "source-inference-model",
            ManifestId: "source-inference-manifest",
            ManifestRef: "manifest://design-conversion/source-inference",
            PromotionUploadRef: "local://design-conversion/promoted/source-inference",
            Version: "source-inference-version",
            MachineId: "source-inference-machine",
            DomainTag: DesignConversionMachine.DomainTag,
            CheckpointPath: "C:\\activation-source-inference\\checkpoint.json",
            CheckpointPathRef: "sha256:checkpoint-path:source-inference",
            CheckpointSha256: Sha256Text("source-inference-checkpoint"),
            CheckpointFreshLoadVerified: checkpointContainsRealTrainedWeights,
            CheckpointContainsRealTrainedWeights: checkpointContainsRealTrainedWeights,
            LocalOnly: true,
            EvidenceRefs: evidenceRefs);

    private static DesignConversionTrainingIntakeLocalPromotionResult BuildPromotionAuthorityRecord(
        string tenantId,
        string name,
        DesignConversionPromotedModelActivationRecord record,
        int batchCount)
    {
        var run = new DesignConversionTrainingIntakeRunResult(
            RunId: $"{name}-authority-run",
            TrainingIntakeId: $"{name}-authority-intake",
            PreflightId: $"{name}-authority-preflight",
            AcceptedCandidateCount: batchCount,
            ResolvedTrainingRecordCount: batchCount,
            MayStartTraining: true,
            SourceManifestStatus: "proven",
            SourceDisjointStatus: "proven",
            TrainingRunnerInvoked: true,
            LocalLifecycleHookAttempted: true,
            TrainingStarted: true,
            CheckpointMutated: true,
            CheckpointCandidateCreated: true,
            WeightsProduced: true,
            ModelUploadReady: true,
            TrainingExecutionBoundary: "model_activation_route_test_server_owned_promotion_authority",
            TrainingExecutionStatus: "completed",
            RunnerBlockedReason: string.Empty,
            CheckpointStatus: "created",
            CheckpointMutationStatus: "mutated",
            WeightProofStatus: "real_weights_proven",
            MeasurementGateStatus: DesignConversionMeasurementGateStatus.Passed,
            PromotionGateStatus: "PROMOTION_ALLOWED_BY_MEASUREMENT_GATE_AND_REAL_WEIGHTS",
            UploadReadinessStatus: "upload_ready_local_promotion_gate_passed_no_external_upload",
            PromotionBlockedReasons: [],
            RunnerEvidence: null,
            Warnings: [],
            BlockReasons: [],
            BoundaryMetadata: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["product_ready"] = bool.FalseString
            });

        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["schema_version"] = JsonlDesignConversionLocalArtifactDb.TrainingIntakeLocalPromotionSchemaVersion,
            ["training_intake_tenant_id_ref"] = DesignConversionModelActivationRegistry.BuildTenantIdRef(tenantId),
            ["local_promotion_record_kind"] = "promotion",
            ["local_promotion_ready"] = bool.TrueString,
            ["local_promotion_not_ready"] = bool.FalseString,
            ["local_promotion_active_local_model_ref"] = record.Publication.Manifest.PromotionUploadRef ?? string.Empty,
            ["local_promotion_active_model_id"] = record.Model.ModelId,
            ["local_promotion_active_checkpoint_sha256"] = record.Model.CheckpointSha256,
            ["local_promotion_prior_state_recorded"] = bool.TrueString,
            ["local_promotion_prior_state_kind"] = "none",
            ["local_promotion_prior_active_local_model_ref"] = string.Empty,
            ["local_promotion_prior_active_model_id"] = string.Empty,
            ["local_promotion_prior_active_checkpoint_sha256"] = string.Empty,
            ["promotion_upload_ref"] = record.Publication.Manifest.PromotionUploadRef ?? string.Empty,
            ["local_promoted_model_id"] = record.Model.ModelId,
            ["local_promoted_checkpoint_sha256"] = record.Model.CheckpointSha256,
            ["deployment_verified"] = bool.FalseString,
            ["product_ready"] = bool.FalseString
        };
        const string policyCheckpointFileRefPrefix = "dpo_policy_checkpoint_file:";
        var promotionArtifacts = record.Publication.Manifest.PromotionArtifacts;
        if (record.Model.DpoPolicyCheckpointFileRef.StartsWith(
                policyCheckpointFileRefPrefix,
                StringComparison.Ordinal) &&
            promotionArtifacts?.TokenizerArtifact is not null &&
            promotionArtifacts.SourceEncoderContractArtifact is not null &&
            promotionArtifacts.PolicyCheckpointContractArtifact is not null)
        {
            var policyCheckpointFileName =
                record.Model.DpoPolicyCheckpointFileRef[policyCheckpointFileRefPrefix.Length..];
            metadata = new Dictionary<string, string>(
                DesignConversionDpoPromotionArtifactContractCodec.AddTrustedAnchorMetadata(
                    metadata,
                    new DesignConversionDpoPromotionArtifactAnchors(
                        promotionArtifacts.TokenizerArtifact.ArtifactSetId,
                        promotionArtifacts.TokenizerArtifact.FileName,
                        promotionArtifacts.TokenizerArtifact.Sha256,
                        promotionArtifacts.TokenizerArtifact.ByteLength,
                        promotionArtifacts.SourceEncoderContractArtifact.Sha256,
                        promotionArtifacts.SourceEncoderContractArtifact.ByteLength,
                        promotionArtifacts.PolicyCheckpointContractArtifact.Sha256,
                        promotionArtifacts.PolicyCheckpointContractArtifact.ByteLength,
                        policyCheckpointFileName)),
                StringComparer.Ordinal);
        }

        return new DesignConversionTrainingIntakeLocalPromotionResult(
            TrainingRun: run,
            LocalPromotionReady: true,
            ExternalCloudUploadAttempted: false,
            PromotionOutputDirectory: Path.GetDirectoryName(record.Model.CheckpointPath) ?? string.Empty,
            PromotedCheckpointPath: record.Model.CheckpointPath,
            Publication: record.Publication,
            PromotedLocalModel: record.Model,
            EvidenceRefs:
            [
                "DESIGN_CONVERSION_TRAINING_INTAKE_LOCAL_PROMOTION_READY",
                "DESIGN_CONVERSION_LOCAL_PROMOTION_ACTIVE_STATE_RECORDED",
                "DESIGN_CONVERSION_MODEL_ACTIVATION_TEST_DURABLE_PROMOTION_RECORD"
            ],
            Warnings: ["local_promotion_ready"],
            BlockReasons: [],
            BoundaryMetadata: metadata);
    }

    private static string NewRoot()
        => Path.Combine(Path.GetTempPath(), "llm_mvp_core", "design-conversion-model-activation-route", Guid.NewGuid().ToString("N"));

    private static async Task<DesignConversionTrainingManifestArtifactDescriptor>
        BuildActivationArtifactDescriptorAsync(
            string artifactKind,
            string path,
            string artifactSetId,
            string runtimeCheckpointSha256)
    {
        var canonicalPath = Path.GetFullPath(path);
        var file = new FileInfo(canonicalPath);
        Assert.True(file.Exists, $"Expected activation fixture artifact at {canonicalPath}.");
        return new DesignConversionTrainingManifestArtifactDescriptor(
            artifactKind,
            canonicalPath,
            Path.GetFileName(canonicalPath),
            await ReadSha256Async(canonicalPath),
            file.Length,
            artifactSetId,
            runtimeCheckpointSha256);
    }

    private static async Task<string> ReadSha256Async(string path)
    {
        var bytes = await File.ReadAllBytesAsync(path);
        return Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    }

    private static string Sha256Bytes(byte[] bytes)
        => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static string Sha256Text(string value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    private static async Task ActivatePublishedModelAsync(
        DesignConversionModelServerModelActivationApi activationApi,
        DesignConversionPromotedModelActivationRecord record,
        long expectedGeneration,
        string idempotencyKey)
    {
        var activated = await activationApi.ActivatePublishedModelAsync(
            new DesignConversionModelServerModelActivationAuthorization(
                TenantId: TenantA,
                RequestedByRef:
                    "architect:activated-runtime-outer-pair-wiring-test",
                Reason: "exercise exact activated runtime pair custody",
                IdempotencyKey: idempotencyKey,
                ExpectedGeneration: expectedGeneration,
                LocalAuthorizationToken: LocalToken),
            record,
            expectedModelVersion: record.Model.Version,
            requestedAtUtc:
                DateTimeOffset.Parse("2026-07-10T10:00:00Z"));
        Assert.True(activated.IsSuccess, activated.ErrorMessage);
        Assert.Equal(expectedGeneration + 1, activated.Value!.Generation);
        Assert.Equal(record.Model.ModelId,
            activated.Value.ActiveVersion.ModelId);
    }

    private static DesignConversionInternalEvaluateRequest
        CreateOuterPairGeneratedRequest(string candidateIdentity)
    {
        var generatedCandidateId = "teacher-authored:" + candidateIdentity;
        var requestId = DesignConversionDpoTeacherAttestationRuntime
            .BuildGeneratedRequestIdFromCandidateId(generatedCandidateId);
        var jobId = $"job:{requestId}";
        var rootOwnershipBinding = DesignConversionDpoLeakageGuard.HashRef(
            "dcm-runtime-rejected-root-ownership",
            string.Concat(jobId, "|", requestId));
        var provenance = BuildTrustedRouteAdmissionProvenance(TenantA)
            .ToDictionary(pair => pair.Key, pair => pair.Value,
                StringComparer.Ordinal);
        provenance[DesignConversionDpoPolicyRuntimePairContract
            .RootOwnershipBindingProvenanceKey] = rootOwnershipBinding;
        return CreateInternalEvaluateRequest(
            TenantA,
            requestId,
            provenance);
    }

    private static async Task<DesignConversionDpoPolicyRuntimePair>
        BuildExactOuterRuntimePairAsync(
            DesignConversionInternalEvaluateRequest generatedRequest,
            DesignConversionInternalEvaluationResult generatedEvaluation,
            string preferredDirection =
                DesignConversionIndependentPreferenceEvidenceContract
                    .GeneratedSide)
    {
        Assert.Equal(generatedRequest.JobId, generatedEvaluation.JobId);
        Assert.Equal(
            generatedRequest.RequestId,
            generatedEvaluation.SubmittedRequest.RequestId);
        Assert.True(generatedRequest.Provenance.TryGetValue(
            DesignConversionDpoPolicyRuntimePairContract
                .RootOwnershipBindingProvenanceKey,
            out var rootOwnershipBinding));

        var incumbentProvenance = BuildTrustedRouteAdmissionProvenance(TenantA)
            .ToDictionary(pair => pair.Key, pair => pair.Value,
                StringComparer.Ordinal);
        incumbentProvenance[DesignConversionDpoPolicyRuntimePairContract
            .RootOwnershipBindingProvenanceKey] = rootOwnershipBinding!;
        var incumbentRequest = CreateInternalEvaluateRequest(
                TenantA,
                generatedRequest.RequestId + ":incumbent",
                incumbentProvenance) with
            {
                JobId = generatedRequest.JobId + ":incumbent",
                Html =
                    "<main class=\"incumbent\"><p>Measured incumbent payload</p></main>",
                Css = ".incumbent{display:block;gap:2px}"
            };
        var incumbentComposition = DesignConversionLocalServiceComposition
            .CreateDeterministicLocalForLiveHostTesting();
        var incumbent = await incumbentComposition.Service.EvaluateAsync(
            incumbentRequest);
        Assert.True(incumbent.IsSuccess, incumbent.ErrorMessage);

        const string requestPrefix = "teacher-authored-request:";
        Assert.StartsWith(
            requestPrefix,
            generatedRequest.RequestId,
            StringComparison.Ordinal);
        var generatedCandidateId = "teacher-authored:" +
            generatedRequest.RequestId[requestPrefix.Length..];
        var generatedBinding =
            DesignConversionDpoTeacherAttestationRuntime
                .BuildGeneratedBinding(
                    generatedEvaluation.SubmittedRequest.Html,
                    generatedEvaluation.SubmittedRequest.Css);
        var incumbentBinding =
            DesignConversionDpoTeacherAttestationRuntime
                .BuildIncumbentBinding(
                    incumbent.Value!.SubmittedRequest);
        var generatedRef =
            DesignConversionDpoTeacherAttestationRuntime
                .BuildGeneratedRefFromCandidateId(generatedCandidateId);
        var incumbentRef = DesignConversionDpoLeakageGuard.HashRef(
            "dcm-dpo-teacher-incumbent",
            incumbentBinding);
        var authoring =
            new DesignConversionDpoTeacherCandidateAuthoringEvidence(
                DesignConversionDpoTeacherAttestationRuntime.AuthoringSchema,
                incumbentRef,
                generatedRef,
                incumbentBinding,
                generatedBinding,
                generatedCandidateId,
                generatedRequest.RequestId,
                DesignConversionDpoGenerator.Sha256Ref(
                    "outer-activated-runtime-pair-authoring-output"),
                TrainingTeacherProvenance.Deterministic(
                    "outer-activated-runtime-pair-authoring-request"),
                ["outer-activated-runtime-pair-authoring:evidence"],
                new Dictionary<string, string>(StringComparer.Ordinal),
                AttemptNumber: 2)
            {
                InferenceProfileId =
                    LocalQwenOllamaInferenceProfileAuthority.Authoring
                        .ProfileId,
                InferenceProfileConfigurationSha256 =
                    LocalQwenOllamaInferenceProfileAuthority.Authoring
                        .ConfigurationSha256
            };
        var sourcePair = new DesignConversionTeacherAuthoredEvaluationPair(
            2,
            generatedEvaluation,
            incumbent.Value,
            authoring,
            generatedCandidateId);
        var artifactRootBinding = DesignConversionDpoGenerator.Sha256Ref(
            "outer-activated-runtime-pair-artifact-root|" +
            generatedRequest.RequestId);
        var observedAtUtc = DateTimeOffset.UtcNow;
        var independent = await IssueOuterPreferenceEvidenceAsync(
            sourcePair,
            artifactRootBinding,
            observedAtUtc,
            preferredDirection);
        var created = DesignConversionDpoPolicyRuntimePairFactory.Create(
            generatedRequest,
            sourcePair,
            independent,
            artifactRootBinding,
            observedAtUtc);
        Assert.True(created.IsSuccess, created.ErrorMessage);
        return created.Value!;
    }

    private static async Task<DesignConversionIndependentPreferenceEvidence>
        IssueOuterPreferenceEvidenceAsync(
            DesignConversionTeacherAuthoredEvaluationPair pair,
            string artifactRootBinding,
            DateTimeOffset observedAtUtc,
            string preferredDirection)
    {
        using var client = new OuterPreferenceEvaluationClient(
            preferredDirection);
        using var evaluator =
            DesignConversionLocalIndependentPreferenceEvaluator.OwnForTesting(
                client,
                OuterPreferenceEvaluationClient.ModelId,
                OuterPreferenceEvaluationClient.ManifestDigest,
                OuterPreferenceEvaluationClient.ModelLayerDigest,
                DesignConversionDpoGenerator.Sha256Ref(
                    "outer-activated-runtime-independent-evaluator|" +
                    Guid.NewGuid().ToString("N")));
        var issued = await evaluator.IssueAsync(
            artifactRootBinding,
            pair,
            observedAtUtc);
        Assert.True(issued.IsSuccess, issued.ErrorMessage);
        return issued.Value!;
    }

    private static string DirectDpoPolicyPath(
        DesignConversionPromotedModelActivationRecord record)
        => Path.Combine(
            Path.GetDirectoryName(record.Model.CheckpointPath)!,
            "dpo_policy_step_000001.bin");

    private static double ComputeExpectedDirectPairScore(
        string policyCheckpointPath,
        DesignConversionDpoPolicyRuntimePair pair)
    {
        var config = BuildActivatedDirectPolicyConfig();
        var model = new LlmModel(config);
        using (var stream = File.OpenRead(policyCheckpointPath))
            model.LoadCheckpoint(stream);
        var binding = DesignConversionDpoPolicyCheckpointBridge
            .BuildRuntimeInferenceEncodingBindingPreviewForTests(
                config,
                evaluated: true);
        Assert.True(binding.IsSuccess, binding.ErrorMessage);
        var batch = DesignConversionDpoPolicyCheckpointBridge
            .BuildRuntimePolicyPairBatchForInference(
                pair.BuildPolicyPrompt(),
                DesignConversionDpoPolicyCheckpointBridge
                    .SerializePolicySideText(
                        pair.Generated.Html,
                        pair.Generated.Css),
                DesignConversionDpoPolicyCheckpointBridge
                    .SerializePolicySideText(
                        pair.Incumbent.Html,
                        pair.Incumbent.Css),
                binding.Value!,
                config);
        Assert.True(batch.IsSuccess, batch.ErrorMessage);
        var generatedLogProbability =
            DpoTrainer.EvaluateAverageNextTokenLogProbability(
                model,
                batch.Value!.ChosenIds,
                batch.Value.ChosenMask);
        var incumbentLogProbability =
            DpoTrainer.EvaluateAverageNextTokenLogProbability(
                model,
                batch.Value.RejectedIds,
                batch.Value.RejectedMask);
        var margin = generatedLogProbability - incumbentLogProbability;
        return Math.Round(
            1d / (1d + Math.Exp(-Math.Clamp(margin, -60d, 60d))),
            6);
    }

    private static async Task SaveActivatedDirectDpoCheckpointAsync(
        DesignConversionMachine trainedMachine,
        string runtimeCheckpointPath,
        int weightSeed)
    {
        var config = BuildActivatedDirectPolicyConfig();
        var policyModel = new LlmModel(config);
        FillActivatedDirectPolicyModelWeights(policyModel, weightSeed);
        var policyPath = Path.Combine(
            Path.GetDirectoryName(runtimeCheckpointPath)!,
            "dpo_policy_step_000001.bin");
        policyModel.SaveCheckpoint(policyPath);
        var policyBytes = await File.ReadAllBytesAsync(policyPath);
        var policyRef = BuildActivatedDirectPolicyCheckpointRef(
            policyBytes,
            config);
        var exported = trainedMachine.ExportCheckpoint();
        Assert.True(exported.IsSuccess, exported.ErrorMessage);
        var runtimeCheckpoint = exported.Value! with
        {
            DpoPolicyCheckpointRef = policyRef,
            AccuracyAfterLastBatch = 1d,
            EvidenceRefs = exported.Value.EvidenceRefs
                .Where(evidence => !string.Equals(
                    evidence,
                    DesignConversionMachine.HeldOutAccuracyNotMeasuredEvidence,
                    StringComparison.Ordinal) &&
                    !string.Equals(
                        evidence,
                        DesignConversionMachine
                            .LocalPreferenceFitSignalOnlyEvidence,
                        StringComparison.Ordinal))
                .Concat(BuildActivatedDirectPolicyHeldOutEvidence())
                .Distinct(StringComparer.Ordinal)
                .ToArray()
        };
        await File.WriteAllBytesAsync(
            runtimeCheckpointPath,
            JsonSerializer.SerializeToUtf8Bytes(
                runtimeCheckpoint,
                DesignConversionCheckpointJsonOptions.Standard));
    }

    private static ModelConfig BuildActivatedDirectPolicyConfig()
        => new()
        {
            VocabSize = 256,
            ContextLen = 128,
            HiddenDim = 16,
            NumLayers = 1,
            NumHeads = 4,
            NumKvHeads = 1,
            FfnHiddenDim = 32,
            RopeTheta = 10_000f,
            RmsNormEps = 1e-5f
        };

    private static void FillActivatedDirectPolicyModelWeights(
        LlmModel model,
        int seed)
    {
        var entries = model.GetCheckpointParameterEntries();
        for (var entryIndex = 0;
             entryIndex < entries.Count;
             entryIndex++)
        {
            var data = entries[entryIndex].Parameter.Value.Data;
            for (var valueIndex = 0;
                 valueIndex < data.Length;
                 valueIndex++)
            {
                var angle =
                    (seed + (entryIndex * 17) + valueIndex + 1) * 0.137f;
                data[valueIndex] = MathF.Sin(angle) * 0.045f +
                    MathF.Cos(angle * 0.37f) * 0.015f;
            }
        }
    }

    private static DesignConversionDpoPolicyCheckpointRef
        BuildActivatedDirectPolicyCheckpointRef(
            byte[] policyBytes,
            ModelConfig config)
    {
        const string policyFileName = "dpo_policy_step_000001.bin";
        const string providerKind = "activated-outer-pair-test-provider";
        const string configKind = "activated-outer-pair-test-config";
        const string baseModelId = "activated-outer-pair-test-model";
        const string baseRevision = "unit-test-v1";
        const int sourceRecordCount = 51;
        const int bridgePreferenceCount = 1;
        var policySha256 = Sha256Bytes(policyBytes);
        var configSha256 = DesignConversionDpoPolicyBaseMetadata
            .ComputeConfigSha256(config);
        var initialWeightSha256 = DesignConversionDpoPolicyBaseMetadata
            .ComputeInitialWeightSha256(new LlmModel(config));
        var baseCheckpointSha256 = Sha256Bytes(
            JsonSerializer.SerializeToUtf8Bytes(new
            {
                provider = providerKind,
                model = baseModelId,
                revision = baseRevision,
                configSha256
            }));
        if (string.Equals(
                baseCheckpointSha256,
                initialWeightSha256,
                StringComparison.Ordinal))
        {
            baseCheckpointSha256 = Sha256Bytes(
                JsonSerializer.SerializeToUtf8Bytes(new
                {
                    alternate = baseCheckpointSha256
                }));
        }

        var binding = DesignConversionDpoPolicyCheckpointBridge
            .BuildRuntimeInferenceEncodingBindingPreviewForTests(
                config,
                evaluated: true);
        Assert.True(binding.IsSuccess, binding.ErrorMessage);
        var bindingValue = binding.Value!;
        var randomContext = DesignConversionDpoPolicyRandomContextContract
            .CreateCurrent(0UL, configSha256);
        var evidenceRefs = new[]
            {
                DesignConversionMachine
                    .DpoTrainerPolicyCheckpointFreshLoadedEvidence,
                DesignConversionMachine
                    .DpoTrainerPolicyCheckpointConvertedEvidence,
                "DESIGN_CONVERSION_DPO_TRAINER_POLICY_CHECKPOINT_SOURCE_11_DPO",
                "DESIGN_CONVERSION_DPO_POLICY_BRIDGE_PRODUCT_READY_FALSE",
                "DESIGN_CONVERSION_DPO_POLICY_BRIDGE_PAYLOAD_FILE_PERSISTED",
                "11.DPO:DpoTrainer",
                "11.DPO:DpoTrainer.StepVector",
                "DESIGN_CONVERSION_DPO_POLICY_BASE_PROVIDER_INJECTED",
                "DESIGN_CONVERSION_DPO_POLICY_BASE_IDENTITY_VALIDATED",
                "DESIGN_CONVERSION_DPO_POLICY_BASE_MODELS_DISTINCT",
                "DESIGN_CONVERSION_DPO_POLICY_INITIAL_POLICY_REFERENCE_BITWISE_EQUAL",
                "DESIGN_CONVERSION_DPO_POLICY_BASE_AND_INITIAL_WEIGHT_HASHES_DISTINCT",
                $"dpo_policy_base_provider_kind:{providerKind}",
                $"dpo_policy_base_configuration_kind:{configKind}",
                $"dpo_policy_base_model_id:{baseModelId}",
                $"dpo_policy_base_revision:{baseRevision}",
                $"dpo_policy_base_checkpoint_sha256:{baseCheckpointSha256}",
                $"dpo_policy_config_sha256:{configSha256}",
                $"dpo_policy_initial_weight_sha256:{initialWeightSha256}",
                $"dpo_policy_checkpoint_sha256:{policySha256}",
                $"dpo_policy_checkpoint_file:{policyFileName}",
                $"dpo_policy_bridge_source_record_count:{sourceRecordCount.ToString(CultureInfo.InvariantCulture)}",
                $"dpo_policy_bridge_preference_count:{bridgePreferenceCount.ToString(CultureInfo.InvariantCulture)}",
                "dpo_policy_step_count:1",
                "DESIGN_CONVERSION_DPO_POLICY_CHECKPOINT_REF_TYPED",
                "DESIGN_CONVERSION_DPO_POLICY_CHECKPOINT_REF_NO_ABSOLUTE_PATH",
                "DESIGN_CONVERSION_DPO_POLICY_CHECKPOINT_REF_PAYLOAD_FILE_SIBLING",
                DesignConversionMachine
                    .DpoPolicyDirectInferenceConfiguredEvidence,
                "DESIGN_CONVERSION_DPO_CONTINUATION_RUNTIME_ENCODING_BOUND",
                $"dpo_runtime_encoding_schema:{bindingValue.BindingSchema}",
                $"dpo_runtime_encoding_source_descriptor_schema:{bindingValue.SourceDescriptorSchema}",
                $"dpo_runtime_encoding_token_mapping:{bindingValue.PolicyTokenMapping}",
                $"dpo_runtime_encoding_token_id_min:{bindingValue.TokenIdMinimum.ToString(CultureInfo.InvariantCulture)}",
                $"dpo_runtime_encoding_token_id_max:{bindingValue.TokenIdMaximum.ToString(CultureInfo.InvariantCulture)}",
                $"dpo_runtime_encoding_context_length:{bindingValue.ContextLength.ToString(CultureInfo.InvariantCulture)}",
                $"dpo_runtime_encoding_tokenizer_sha256:{bindingValue.TokenizerArtifactSha256}",
                $"dpo_runtime_encoding_vocabulary_sha256:{bindingValue.TokenizerVocabularyIdentitySha256}"
            }
            .Concat(DesignConversionDpoPolicyRandomContextContract
                .BuildRequiredEvidenceRefs(randomContext))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var trainingContext =
            new DesignConversionDpoPolicyCheckpointTrainingContext(
                ConfigKind: configKind,
                VocabSize: config.VocabSize,
                ContextLength: config.ContextLen,
                HiddenDim: config.HiddenDim,
                NumLayers: config.NumLayers,
                NumHeads: config.NumHeads,
                NumKvHeads: config.NumKvHeads,
                FfnHiddenDim: config.FfnHiddenDim,
                RawPayloadPersisted: true,
                AbsolutePathPersisted: false,
                SchemaVersion: 4,
                RopeTheta: config.RopeTheta,
                RmsNormEps: config.RmsNormEps,
                ProviderKind: providerKind,
                BaseModelId: baseModelId,
                BaseRevision: baseRevision,
                BaseCheckpointSha256: baseCheckpointSha256,
                ConfigSha256: configSha256,
                InitialWeightSha256: initialWeightSha256,
                Lora: null)
            {
                RandomContext = randomContext,
                LegalTeacherAdmissionCommitment =
                    BuildActivatedDirectPolicyLegalTeacherCommitment(
                        evidenceRefs)
            };
        var policyRef = new DesignConversionDpoPolicyCheckpointRef(
            ArtifactKind:
                DesignConversionDpoTrainerRouteBGate
                    .PolicyCheckpointArtifactKind,
            PolicyCheckpointFileName: policyFileName,
            Sha256: policySha256,
            SourceSystem: "11.DPO:DpoTrainer",
            SourceMechanism: "11.DPO:DpoTrainer.StepVector:fresh_loaded",
            RuntimeBridgeKind:
                DesignConversionMachine
                    .DpoPolicyDirectInferenceRuntimeBridgeKind,
            FreshLoadVerified: true,
            ConvertedToRuntimeWeights: true,
            SourceRecordCount: sourceRecordCount,
            BridgePreferenceCount: bridgePreferenceCount,
            DpoStepCount: 1,
            TrainingContext: trainingContext,
            EvidenceRefs: evidenceRefs,
            PolicyCheckpointByteLength: policyBytes.Length);
        return policyRef with
        {
            EvaluatedHeldOutAcceptanceAuthority =
                BuildActivatedDirectPolicyHeldOutAuthority(policyRef)
        };
    }

    private static DesignConversionDpoHeldOutEvaluation
        BuildActivatedDirectPolicyHeldOutAuthority(
            DesignConversionDpoPolicyCheckpointRef policyRef)
    {
        const int heldOutRecordCount = 50;
        var context = policyRef.TrainingContext;
        return new DesignConversionDpoHeldOutEvaluation(
            Schema:
                DesignConversionDpoPolicyCheckpointBridge.EvaluatedSchema,
            PackingId:
                DesignConversionDpoPolicyCheckpointBridge.EvaluatedPackingId,
            VocabSize: context.VocabSize,
            ContextLength: context.ContextLength,
            DpoStepCount: policyRef.DpoStepCount,
            MaxBridgePreferenceCount: policyRef.BridgePreferenceCount,
            ChildPolicyCheckpointSha256: policyRef.Sha256,
            ReferencePolicyCheckpointSha256:
                context.BaseCheckpointSha256!,
            ReferenceMode: "immediate_parent",
            DatasetId: "activated-outer-pair-heldout",
            PartitionSnapshotSha256: new string('b', 64),
            TrainRecordSetSha256: new string('c', 64),
            HeldOutRecordSetSha256: new string('d', 64),
            TrainSourceSetSha256: new string('e', 64),
            HeldOutSourceSetSha256: new string('f', 64),
            SelectedTrainInputSha256: new string('1', 64),
            HeldOutEvaluationInputSha256: new string('a', 64),
            TrainRecordCount:
                policyRef.SourceRecordCount - heldOutRecordCount,
            HeldOutRecordCount: heldOutRecordCount,
            EvaluatedHeldOutRecordCount: heldOutRecordCount,
            CorrectCount: heldOutRecordCount,
            Accuracy: 1d,
            MeanRelativeMargin: 0.5d,
            MeanChildMargin: 0.75d,
            MeanReferenceMargin: 0.25d,
            FreshLoadVerified: true,
            ReferenceFrozen: true,
            SourceDisjoint: true,
            HeldOutNotTrained: true)
        {
            G3Metrics = new DesignConversionG3AcceptanceMetrics(
                HeldOutCount: heldOutRecordCount,
                BaselineWinCount: heldOutRecordCount,
                BaselineWinRate: 1d,
                AbsoluteChildPreferenceCorrectCount: heldOutRecordCount,
                AbsoluteChildPreferenceAccuracy: 1d)
            {
                MeasurementNoiseScorerIdentity =
                    DesignConversionDpoPolicyCheckpointBridge
                        .EvaluatedMeasurementNoiseScorerIdentity,
                MeasurementNoiseEvaluationSetId = "activated-outer-pair-heldout"
            },
            CycleAccuracyLineage =
                new DesignConversionDpoHeldOutCycleAccuracyLineage(
                    CycleOrdinal: 2,
                    CurrentMeasuredAccuracy: 1d,
                    HasParentMeasurement: true,
                    ParentCycleOrdinal: 1,
                    ParentMeasuredAccuracy: 1d,
                    ParentPolicyCheckpointSha256:
                        context.BaseCheckpointSha256,
                    ParentDatasetId: "activated-outer-pair-heldout",
                    ParentHeldOutRecordSetSha256: new string('d', 64),
                    ParentHeldOutSourceSetSha256: new string('f', 64),
                    ParentHeldOutEvaluationInputSha256: new string('a', 64),
                    ComparableHeldOut: true,
                    AccuracyDelta: 0d)
        };
    }

    private static string[] BuildActivatedDirectPolicyHeldOutEvidence()
        =>
        [
            DesignConversionMachine
                .DpoRuntimeCheckpointAccuracyFromHeldOutEvaluationEvidence,
            DesignConversionMachine
                .DpoRuntimeCheckpointAccuracyCurrentCycleHeldOutEvaluationScopeEvidence,
            "dpo_runtime_checkpoint_accuracy_value:1",
            "dpo_runtime_checkpoint_accuracy_evaluated_heldout_count:50",
            $"dpo_runtime_checkpoint_accuracy_input_sha256:{new string('a', 64)}"
        ];

    private static DesignConversionDpoLegalTeacherAdmissionCommitment
        BuildActivatedDirectPolicyLegalTeacherCommitment(
            IEnumerable<string> checkpointEvidenceRefs)
    {
        var authority = LegalTrainingTeacherAdmissionAuthority.Create(
            [],
            maxTeacherCalls: 0,
            Enumerable.Repeat((byte)0x5a, 32).ToArray());
        Assert.True(authority.IsSuccess, authority.ErrorMessage);
        var trainingContent = BoundedDpoTrainingContentProjector.Project(
        [
            new DpoTrainingContentRowInput(
                "activated outer pair legal teacher commitment test",
                "accepted bounded DPO row",
                "rejected bounded DPO row",
                [1d, 0d],
                "activated-outer-pair-legal-teacher-commitment-v1")
        ]);
        Assert.True(trainingContent.IsSuccess, trainingContent.ErrorMessage);
        var provenance = TrainingTeacherProvenance
            .Deterministic("activated-outer-pair-test-custody") with
        {
            TrainingContentSha256 = trainingContent.Value!.Rows[0].Sha256
        };
        var admission = authority.Value!.ValidateBatch(
            trainingContent.Value,
            [provenance]);
        Assert.True(admission.IsSuccess, admission.ErrorMessage);
        var core = DesignConversionDpoLegalTeacherAdmissionCommitmentCodec
            .CaptureAdmission(admission.Value!);
        Assert.True(core.IsSuccess, core.ErrorMessage);
        var commitment =
            DesignConversionDpoLegalTeacherAdmissionCommitmentCodec
                .FinalizeCommitment(core.Value!, checkpointEvidenceRefs);
        Assert.True(commitment.IsSuccess, commitment.ErrorMessage);
        return commitment.Value!;
    }

    private static void ReplaceOnlyCachedRuntimeEvaluator(
        DesignConversionActivatedModelRuntimeEvaluator activatedEvaluator,
        IDesignConversionThinClientJobEvaluator replacement)
    {
        var cacheField = typeof(DesignConversionActivatedModelRuntimeEvaluator)
            .GetField(
                "_runtimeByTenantRef",
                BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(cacheField);
        var cache = Assert.IsAssignableFrom<System.Collections.IDictionary>(
            cacheField!.GetValue(activatedEvaluator));
        var tenantEntryValue = Assert.Single(
            cache.Values.Cast<object>());
        var runtimeEntries = Assert.IsAssignableFrom<System.Collections.IEnumerable>(
                tenantEntryValue)
            .Cast<object>()
            .ToArray();
        var runtimeEntry = Assert.Single(runtimeEntries);
        var evaluatorField = runtimeEntry.GetType().GetField(
            "<Evaluator>k__BackingField",
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(evaluatorField);
        evaluatorField!.SetValue(runtimeEntry, replacement);
        Assert.Same(replacement, evaluatorField.GetValue(runtimeEntry));
    }

    private static RecordingDelegatingPairRuntimeEvaluator
        WrapCachedPairRuntimeEvaluatorForModel(
            DesignConversionActivatedModelRuntimeEvaluator activatedEvaluator,
            string modelId)
    {
        var cacheField = typeof(DesignConversionActivatedModelRuntimeEvaluator)
            .GetField(
                "_runtimeByTenantRef",
                BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(cacheField);
        var cache = Assert.IsAssignableFrom<System.Collections.IDictionary>(
            cacheField!.GetValue(activatedEvaluator));
        var runtimeEntries = cache.Values
            .Cast<object>()
            .SelectMany(value =>
                Assert.IsAssignableFrom<System.Collections.IEnumerable>(value)
                    .Cast<object>())
            .ToArray();
        var runtimeEntry = Assert.Single(
            runtimeEntries,
            entry =>
            {
                var modelField = entry.GetType().GetField(
                    "<PromotedLocalModel>k__BackingField",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                var model = Assert.IsType<DesignConversionPromotedLocalModel>(
                    modelField?.GetValue(entry));
                return string.Equals(
                    model.ModelId,
                    modelId,
                    StringComparison.Ordinal);
            });
        var evaluatorField = runtimeEntry.GetType().GetField(
            "<Evaluator>k__BackingField",
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(evaluatorField);
        var inner = Assert.IsAssignableFrom<
            IDesignConversionThinClientJobEvaluator>(
                evaluatorField!.GetValue(runtimeEntry));
        var wrapper = new RecordingDelegatingPairRuntimeEvaluator(inner);
        evaluatorField.SetValue(runtimeEntry, wrapper);
        Assert.Same(wrapper, evaluatorField.GetValue(runtimeEntry));
        return wrapper;
    }

    private static async Task<ActivatedPairAssociationFixture>
        BuildActivatedPairAssociationFixtureAsync(string requestId)
    {
        var rootJobId = $"activated-pair-root-job:{requestId}";
        var rootRequestId = $"activated-pair-root-request:{requestId}";
        var rootOwnershipBinding = DesignConversionDpoLeakageGuard.HashRef(
            "dcm-runtime-rejected-root-ownership",
            string.Concat(rootJobId, "|", rootRequestId));
        var provenance = BuildTrustedRouteAdmissionProvenance(TenantA)
            .ToDictionary(pair => pair.Key, pair => pair.Value,
                StringComparer.Ordinal);
        provenance[DesignConversionDpoPolicyRuntimePairContract
            .RootOwnershipBindingProvenanceKey] = rootOwnershipBinding;
        var request = CreateInternalEvaluateRequest(
            TenantA,
            requestId,
            provenance);
        var evaluation = await EvaluatePairAssociationRequestAsync(request);
        return new ActivatedPairAssociationFixture(
            request,
            evaluation,
            BuildActivatedPairAssociationPair(
                evaluation,
                rootJobId,
                rootRequestId,
                rootOwnershipBinding),
            rootJobId,
            rootRequestId,
            rootOwnershipBinding);
    }

    private static async Task<DesignConversionInternalEvaluationResult>
        EvaluatePairAssociationRequestAsync(
            DesignConversionInternalEvaluateRequest request)
    {
        var composition = DesignConversionLocalServiceComposition
            .CreateDeterministicLocalForLiveHostTesting();
        var evaluated = await composition.Service.EvaluateAsync(request);
        Assert.True(evaluated.IsSuccess, evaluated.ErrorMessage);
        return evaluated.Value!;
    }

    private static DesignConversionDpoPolicyRuntimePair
        BuildActivatedPairAssociationPair(
            DesignConversionInternalEvaluationResult evaluation,
            string rootJobId,
            string rootRequestId,
            string rootOwnershipBinding)
    {
        var generatedOutput = evaluation.RuntimeFlowResult.Output;
        var generated = new DesignConversionDpoPolicyRuntimeSide(
            DesignConversionDpoPolicyRuntimePairContract.GeneratedSide,
            evaluation.SubmittedRequest.Html,
            evaluation.SubmittedRequest.Css,
            Sha256Text(string.Concat(
                evaluation.SubmittedRequest.Html,
                "\n",
                evaluation.SubmittedRequest.Css)),
            "custody:activated-pair:generated",
            "binding:activated-pair:generated",
            DesignConversionDpoGenerator
                .BuildIndependentPreferenceEvaluationIdentity(evaluation),
            evaluation.JobId,
            evaluation.SubmittedRequest.RequestId,
            generatedOutput.OutputId,
            generatedOutput.CandidateId,
            generatedOutput.Verdict.VerdictId);
        var incumbent = new DesignConversionDpoPolicyRuntimeSide(
            DesignConversionDpoPolicyRuntimePairContract.IncumbentSide,
            "<main>activated pair incumbent</main>",
            ".incumbent{display:block}",
            Sha256Text("activated-pair-incumbent"),
            "custody:activated-pair:incumbent",
            "binding:activated-pair:incumbent",
            Sha256Text("activated-pair-incumbent-evaluation"),
            evaluation.JobId + ":incumbent",
            evaluation.SubmittedRequest.RequestId + ":incumbent",
            generatedOutput.OutputId + ":incumbent",
            generatedOutput.CandidateId + ":incumbent",
            generatedOutput.Verdict.VerdictId + ":incumbent");
        const string generatedCandidateId =
            "teacher-authored:activated-pair-generated";
        var sourcePair = new DesignConversionTeacherAuthoredEvaluationPair(
            2,
            evaluation,
            evaluation,
            AuthoringEvidence: null!,
            GeneratedCandidateId: generatedCandidateId);
        return new DesignConversionDpoPolicyRuntimePair(
            DesignConversionDpoPolicyRuntimePairContract.Schema,
            2,
            rootJobId,
            rootRequestId,
            rootOwnershipBinding,
            generatedCandidateId,
            generated,
            incumbent,
            "artifact-root:activated-pair",
            IndependentEvaluation: null!,
            ObservedAtUtc: DateTimeOffset.Parse("2026-07-10T10:00:00Z"),
            PairBindingSha256: Sha256Text("activated-pair-binding"),
            SourcePair: sourcePair);
    }

    private static void AssertPairAssociationUnavailable(
        OperationResult<DesignConversionInternalEvaluationResult> result)
    {
        Assert.False(result.IsSuccess);
        Assert.Equal(
            DesignConversionMachine.DpoPolicyRuntimePairContentUnavailableCode,
            result.ErrorCode);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static PairAssociationWeakReferences
        RegisterEphemeralPairAssociation(
            DesignConversionActivatedDpoPolicyPairRuntimeAssociation association,
            ActivatedPairAssociationFixture fixture)
    {
        var ephemeralEvaluation = fixture.Evaluation with
        {
            EvidenceRefs = fixture.Evaluation.EvidenceRefs
                .Append("ephemeral-activated-pair-result")
                .ToArray()
        };
        var ephemeralEvaluator =
            new RecordingPairRuntimeEvaluator("ephemeral-selected");
        association.Register(
            ephemeralEvaluation,
            fixture.Request,
            ephemeralEvaluator);
        return new PairAssociationWeakReferences(
            new WeakReference(ephemeralEvaluation),
            new WeakReference(ephemeralEvaluator));
    }

    private sealed class OuterPreferenceEvaluationClient(
        string preferredDirection)
        : ILegalTeacherEvaluationInferenceClient
    {
        internal const string ModelId = "qwen-test:9b";
        internal static readonly string ManifestDigest = new('c', 64);
        internal static readonly string ModelLayerDigest = new('d', 64);
        private readonly string _requestId =
            "outer-activated-runtime-independent-evaluation:" +
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

    private sealed class RecordingDelegatingPairRuntimeEvaluator(
        IDesignConversionThinClientJobEvaluator inner)
        : IDesignConversionThinClientJobEvaluator,
          IDesignConversionDpoPolicyPairRuntimeEvaluator
    {
        private readonly IDesignConversionThinClientJobEvaluator _inner = inner;
        private readonly IDesignConversionDpoPolicyPairRuntimeEvaluator
            _pairInner = inner as
                IDesignConversionDpoPolicyPairRuntimeEvaluator ??
                throw new ArgumentException(
                    "The selected activated evaluator must support exact pairs.",
                    nameof(inner));
        private readonly List<DesignConversionInternalEvaluationResult>
            _pairEvaluations = [];
        private readonly List<DesignConversionDpoPolicyRuntimePair> _pairs = [];

        internal int PairCallCount => _pairEvaluations.Count;
        internal IReadOnlyList<DesignConversionInternalEvaluationResult>
            PairEvaluations => _pairEvaluations;
        internal IReadOnlyList<DesignConversionDpoPolicyRuntimePair> Pairs =>
            _pairs;

        public Task<OperationResult<DesignConversionInternalEvaluationResult>>
            EvaluateAsync(
                DesignConversionInternalEvaluateRequest request,
                CancellationToken cancellationToken = default)
            => _inner.EvaluateAsync(request, cancellationToken);

        Task<OperationResult<DesignConversionInternalEvaluationResult>>
            IDesignConversionDpoPolicyPairRuntimeEvaluator
                .ApplyDpoPolicyRuntimePairAsync(
            DesignConversionInternalEvaluationResult evaluation,
            DesignConversionDpoPolicyRuntimePair pair,
            CancellationToken cancellationToken)
        {
            _pairEvaluations.Add(evaluation);
            _pairs.Add(pair);
            return _pairInner.ApplyDpoPolicyRuntimePairAsync(
                evaluation,
                pair,
                cancellationToken);
        }
    }

    private sealed class RoutedCheckpointLoader(
        IReadOnlyList<DesignConversionMachineLoadResult> loadResults)
        : IDesignConversionActivatedModelCheckpointLoader
    {
        private readonly IReadOnlyDictionary<string,
            DesignConversionMachineLoadResult> _loadResults = loadResults
                .ToDictionary(
                    result => result.CheckpointSha256,
                    StringComparer.OrdinalIgnoreCase);
        private readonly List<CheckpointLoadCall> _calls = [];

        internal IReadOnlyList<CheckpointLoadCall> Calls => _calls;

        public Task<OperationResult<DesignConversionMachineLoadResult>>
            LoadAsync(
                DesignConversionActivatedModelVersion activeVersion,
                CancellationToken ct = default)
        {
            _calls.Add(new CheckpointLoadCall(activeVersion, ct));
            return Task.FromResult(
                _loadResults.TryGetValue(
                    activeVersion.CheckpointSha256,
                    out var result)
                    ? OperationResult<DesignConversionMachineLoadResult>.Ok(
                        result)
                    : OperationResult<DesignConversionMachineLoadResult>.Fail(
                        "No routed activated checkpoint matched.",
                        "ROUTED_ACTIVATED_CHECKPOINT_NOT_FOUND"));
        }
    }

    private sealed class NonPairThinRuntimeEvaluator(
        DesignConversionInternalService service)
        : IDesignConversionThinClientJobEvaluator
    {
        private readonly DesignConversionInternalService _service = service;
        private int _evaluateCallCount;

        internal int EvaluateCallCount =>
            Volatile.Read(ref _evaluateCallCount);

        public Task<OperationResult<DesignConversionInternalEvaluationResult>>
            EvaluateAsync(
                DesignConversionInternalEvaluateRequest request,
                CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref _evaluateCallCount);
            return _service.EvaluateAsync(request, cancellationToken);
        }
    }

    private sealed class UnexpectedCandidateStrategy : IDesignConversionImprovementCandidateStrategy
    {
        public Task<OperationResult<DesignConversionImprovementCandidate>> CreateNextCandidateAsync(
            DesignConversionCandidateUpdateRequest request,
            CancellationToken cancellationToken = default)
            => Task.FromResult(OperationResult<DesignConversionImprovementCandidate>.Fail(
                "Model activation route/runtime tests do not exercise the improvement candidate path.",
                "MODEL_ACTIVATION_ROUTE_UNEXPECTED_CANDIDATE_REQUEST"));
    }

    private sealed class RecordingCheckpointLoader(
        DesignConversionMachineLoadResult loadResult) : IDesignConversionActivatedModelCheckpointLoader
    {
        private readonly DesignConversionMachineLoadResult _loadResult =
            loadResult ?? throw new ArgumentNullException(nameof(loadResult));
        private readonly List<CheckpointLoadCall> _calls = [];

        public IReadOnlyList<CheckpointLoadCall> Calls => _calls;

        public Task<OperationResult<DesignConversionMachineLoadResult>> LoadAsync(
            DesignConversionActivatedModelVersion activeVersion,
            CancellationToken ct = default)
        {
            _calls.Add(new CheckpointLoadCall(activeVersion, ct));
            return Task.FromResult(OperationResult<DesignConversionMachineLoadResult>.Ok(_loadResult));
        }
    }

    private sealed class FailingCheckpointLoader(
        string errorCode) : IDesignConversionActivatedModelCheckpointLoader
    {
        private readonly List<CheckpointLoadCall> _calls = [];

        public IReadOnlyList<CheckpointLoadCall> Calls => _calls;

        public Task<OperationResult<DesignConversionMachineLoadResult>> LoadAsync(
            DesignConversionActivatedModelVersion activeVersion,
            CancellationToken ct = default)
        {
            _calls.Add(new CheckpointLoadCall(activeVersion, ct));
            return Task.FromResult(OperationResult<DesignConversionMachineLoadResult>.Fail(
                $"Injected checkpoint loader failure must remain private: {errorCode}",
                errorCode));
        }
    }

    private sealed record CheckpointLoadCall(
        DesignConversionActivatedModelVersion ActiveVersion,
        CancellationToken CancellationToken);

    private sealed record ActivatedPairAssociationFixture(
        DesignConversionInternalEvaluateRequest Request,
        DesignConversionInternalEvaluationResult Evaluation,
        DesignConversionDpoPolicyRuntimePair Pair,
        string RootJobId,
        string RootRequestId,
        string RootOwnershipBinding);

    private sealed record PairAssociationWeakReferences(
        WeakReference Evaluation,
        WeakReference Evaluator);

    private sealed class RecordingPairRuntimeEvaluator(string marker) :
        IDesignConversionDpoPolicyPairRuntimeEvaluator
    {
        private readonly List<DesignConversionInternalEvaluationResult>
            _evaluations = [];
        private readonly List<DesignConversionDpoPolicyRuntimePair> _pairs = [];

        internal int CallCount => _evaluations.Count;
        internal IReadOnlyList<DesignConversionInternalEvaluationResult>
            Evaluations => _evaluations;
        internal IReadOnlyList<DesignConversionDpoPolicyRuntimePair> Pairs =>
            _pairs;

        public Task<OperationResult<DesignConversionInternalEvaluationResult>>
            ApplyDpoPolicyRuntimePairAsync(
                DesignConversionInternalEvaluationResult evaluation,
                DesignConversionDpoPolicyRuntimePair pair,
                CancellationToken cancellationToken = default)
        {
            _evaluations.Add(evaluation);
            _pairs.Add(pair);
            return Task.FromResult(
                OperationResult<DesignConversionInternalEvaluationResult>.Ok(
                    evaluation with
                    {
                        Warnings = evaluation.Warnings.Append(marker).ToArray()
                    }));
        }
    }

}
