// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using DesignConversionDomain;
using DPO;
using ModelServer.Contracts;
using ModelServer.DesignConversion;
using ModelServer.Remote;
using TransformerModel;

public sealed class DesignConversionModelRepositoryPromotionUploaderTests
{
    private const string ServerBaseUrl = "https://models.example/repository";
    private static readonly DateTimeOffset FixedRemoteTime = DateTimeOffset.Parse("2026-07-27T08:30:00Z");
    private static readonly Encoding Utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    [Fact]
    public void Constructor_RequiresExplicitDependenciesAndSafeHttpsBaseUrl()
    {
        var calls = new List<string>();
        var repository = new RecordingModelRepository(calls);
        var locator = new RecordingRemoteUnitLocator(calls);
        var uploader = new DesignConversionModelRepositoryPromotionUploader(
            repository,
            locator,
            ServerBaseUrl);
        var nestedPathUploader = new DesignConversionModelRepositoryPromotionUploader(
            repository,
            locator,
            "https://models.example/repository/nested");

        Assert.NotNull(uploader);
        Assert.NotNull(nestedPathUploader);

        Assert.Throws<ArgumentNullException>(() => new DesignConversionModelRepositoryPromotionUploader(
            null!,
            locator,
            ServerBaseUrl));
        Assert.Throws<ArgumentNullException>(() => new DesignConversionModelRepositoryPromotionUploader(
            repository,
            null!,
            ServerBaseUrl));
        Assert.Throws<ArgumentNullException>(() => new DesignConversionModelRepositoryPromotionUploader(
            repository,
            locator,
            null!));
        Assert.Throws<ArgumentException>(() => new DesignConversionModelRepositoryPromotionUploader(
            repository,
            locator,
            "http://models.example/repository"));
        Assert.Throws<ArgumentException>(() => new DesignConversionModelRepositoryPromotionUploader(
            repository,
            locator,
            "https://models.example/repository?leak=true"));
        Assert.Throws<ArgumentException>(() => new DesignConversionModelRepositoryPromotionUploader(
            repository,
            locator,
            "https://models.example/repository/../private"));
        Assert.Throws<ArgumentException>(() => new DesignConversionModelRepositoryPromotionUploader(
            repository,
            locator,
            "https://models.example/repository\\private"));
        Assert.Throws<ArgumentException>(() => new DesignConversionModelRepositoryPromotionUploader(
            repository,
            locator,
            "https://models.example/repository/%252e%252e/private"));
        Assert.Throws<ArgumentException>(() => new DesignConversionModelRepositoryPromotionUploader(
            repository,
            locator,
            "https://models.example/repository/%252fprivate"));
    }

    [Fact]
    public void PreflightFingerprint_BindsStableRepositoryAndLocatorDestinationIdentities()
    {
        static OperationResult<string> Fingerprint(
            char repositoryIdentity,
            char locatorIdentity)
            => new DesignConversionModelRepositoryPromotionUploader(
                    new RecordingModelRepository([], repositoryIdentity),
                    new RecordingRemoteUnitLocator([], locatorIdentity),
                    ServerBaseUrl)
                .PreflightConfigurationFingerprintSha256();

        var first = Fingerprint('a', 'b');
        var sameFreshIdentity = Fingerprint('a', 'b');
        var differentRepository = Fingerprint('c', 'b');
        var differentLocator = Fingerprint('a', 'd');

        Assert.True(first.IsSuccess, first.ErrorMessage);
        Assert.True(sameFreshIdentity.IsSuccess, sameFreshIdentity.ErrorMessage);
        Assert.True(differentRepository.IsSuccess, differentRepository.ErrorMessage);
        Assert.True(differentLocator.IsSuccess, differentLocator.ErrorMessage);
        Assert.Equal(first.Value, sameFreshIdentity.Value);
        Assert.NotEqual(first.Value, differentRepository.Value);
        Assert.NotEqual(first.Value, differentLocator.Value);
    }

    [Fact]
    public void InMemoryRemoteUnitLocator_SeparateRegistriesHaveDistinctStableIdentities()
    {
        var firstLocator = new InMemoryRemoteUnitLocator();
        var secondLocator = new InMemoryRemoteUnitLocator();

        var first = firstLocator.PreflightStableConfigurationIdentitySha256();
        var firstRepeated = firstLocator.PreflightStableConfigurationIdentitySha256();
        var second = secondLocator.PreflightStableConfigurationIdentitySha256();

        Assert.True(first.IsSuccess, first.ErrorMessage);
        Assert.True(firstRepeated.IsSuccess, firstRepeated.ErrorMessage);
        Assert.True(second.IsSuccess, second.ErrorMessage);
        Assert.Equal(first.Value, firstRepeated.Value);
        Assert.NotEqual(first.Value, second.Value);
    }

    [Fact]
    public async Task UploadAsync_SuccessUploadsLoadsRegistersLocatesAndReturnsVerifiedReceipt()
    {
        var fixture = await CreateFixtureAsync("success");
        try
        {
            var calls = new List<string>();
            var repository = new RecordingModelRepository(calls);
            var locator = new RecordingRemoteUnitLocator(calls);
            var uploader = new DesignConversionModelRepositoryPromotionUploader(
                repository,
                locator,
                ServerBaseUrl);

            var result = await uploader.UploadAsync(fixture.Manifest, fixture.GateReport);

            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.Equal(
                [
                    "upload:critic",
                    "upload:improver",
                    "load:critic",
                    "load:improver",
                    "register:critic",
                    "register:improver",
                    "locate:critic",
                    "locate:improver"
                ],
                calls);
            Assert.Equal(2, repository.UploadedManifests.Count);
            Assert.All(repository.UploadedManifests, manifest =>
            {
                Assert.Equal(fixture.CheckpointPath, manifest.PolicyCheckpointPath);
                Assert.Equal(fixture.TokenizerPath, manifest.TokenizerPath);
                Assert.Equal(0, manifest.VocabSize);
                Assert.Equal(0, manifest.ContextLen);
                Assert.Equal(0, manifest.HiddenDim);
                Assert.Equal(0, manifest.NumLayers);
                Assert.Equal(0, manifest.NumHeads);
                Assert.Equal(0, manifest.NumKvHeads);
                Assert.Equal(0, manifest.FfnHiddenDim);
                Assert.Equal(0, manifest.LoraRank);
                Assert.Equal(0f, manifest.LoraAlpha);
                Assert.Equal(0f, manifest.LoraDropout);
                Assert.Empty(manifest.LoraTargetModules);
                Assert.Equal(fixture.CheckpointSha256, manifest.ManifestSha256);
                Assert.Equal(fixture.CheckpointSha256, manifest.PolicyCheckpointSha256);
                Assert.Equal(fixture.CheckpointSha256, manifest.RuntimeProvenanceSha256);
                Assert.Equal(fixture.ExpectedSourceDatasetSha256, manifest.SourceDatasetSha256);
                Assert.Contains("learned-weight JSON checkpoint", manifest.TrainingText, StringComparison.Ordinal);
                Assert.Contains("non_transformer_json_checkpoint", manifest.TrainingText, StringComparison.Ordinal);
                Assert.Contains("not_transformer_not_dpo_not_cloud_ready", manifest.OutputText, StringComparison.Ordinal);
                Assert.DoesNotContain(fixture.CheckpointPath, BuildManifestPublicText(manifest), StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain(fixture.TokenizerPath, BuildManifestPublicText(manifest), StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain(fixture.PolicyPath, BuildManifestPublicText(manifest), StringComparison.OrdinalIgnoreCase);
            });

            var receipt = result.Value!;
            Assert.Equal(DesignConversionPromotionPublicationKind.VerifiedModelServerUpload, receipt.PublicationKind);
            Assert.True(receipt.UploadAttempted);
            Assert.True(receipt.UploadSucceeded);
            Assert.True(receipt.VerificationSucceeded);
            Assert.Equal(DesignConversionPromotionPublicationStatus.UploadedVerified, receipt.PublicationStatus);
            Assert.StartsWith("repo-upload:", receipt.UploadId, StringComparison.Ordinal);
            Assert.StartsWith("repo-prov:", receipt.ProvenanceRef, StringComparison.Ordinal);
            Assert.Equal(
                "https://models.example/repository/models/design_conversion_runtime_scaffold_critic",
                receipt.RemoteRef);
            Assert.Contains("MODEL_REPOSITORY_UPLOAD_BOTH_UNITS_SUCCEEDED", receipt.EvidenceRefs);
            Assert.Contains("MODEL_REPOSITORY_FRESH_LOAD_SHA_VERIFIED_BOTH_UNITS", receipt.EvidenceRefs);
            Assert.Contains("REMOTE_LOCATOR_REGISTERED_BOTH_UNITS", receipt.EvidenceRefs);
            Assert.Contains("REMOTE_LOCATOR_LOCATED_BOTH_UNITS", receipt.EvidenceRefs);
            Assert.Contains("DESIGN_CONVERSION_LEARNED_WEIGHT_JSON_CHECKPOINT_EXPLICIT_NON_TRANSFORMER", receipt.EvidenceRefs);
            AssertNoAbsolutePathLeak(BuildPublicReceiptSurface(receipt), fixture);
        }
        finally
        {
            DeleteDirectoryIfExists(fixture.TempRoot);
        }
    }

    [Fact]
    public async Task UploadAsync_SourceEncoderPathUploadsEvaluatedDpoPolicyCheckpointAndVerifiesFreshLoad()
    {
        var fixture = await CreateFixtureAsync(
            "source-encoder-upload",
            includeCriticLearnedWeights: false,
            includeImproverLearnedWeights: false);
        try
        {
            var evaluated = await CreateEvaluatedDpoArtifactsAsync(fixture, "source-encoder-upload");
            var runtimeCheckpointSha256 = await ReadFileSha256Async(
                evaluated.RuntimeCheckpointPath);
            Assert.NotEqual(
                evaluated.PolicyCheckpointSha256,
                runtimeCheckpointSha256);

            var manifest = fixture.Manifest with
            {
                CheckpointRef = evaluated.RuntimeCheckpointPath,
                ManifestRef = evaluated.RuntimeManifestRef,
                PromotionArtifacts = evaluated.Artifacts
            };
            var gateReport = BuildEvaluatedGateReport(fixture.GateReport, evaluated);
            var calls = new List<string>();
            var repository = new RecordingModelRepository(calls);
            var locator = new RecordingRemoteUnitLocator(calls);
            var uploader = new DesignConversionModelRepositoryPromotionUploader(
                repository,
                locator,
                ServerBaseUrl);

            var result = await uploader.UploadAsync(manifest, gateReport);

            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.Equal(
                ["upload:critic", "upload:improver", "load:critic", "load:improver", "register:critic", "register:improver", "locate:critic", "locate:improver"],
                calls);
            Assert.Equal(2, repository.UploadedManifests.Count);
            Assert.Single(repository.UploadedManifests
                .Select(uploaded => uploaded.PolicyCheckpointPath)
                .Distinct(StringComparer.OrdinalIgnoreCase));
            Assert.Single(repository.UploadedManifests
                .Select(uploaded => uploaded.ManifestSha256)
                .Distinct(StringComparer.OrdinalIgnoreCase));
            Assert.All(repository.LoadFallbackPaths, path => Assert.Equal(evaluated.PolicyCheckpointPath, path));
            Assert.All(
                repository.UploadedManifests,
                uploaded =>
                {
                    Assert.Equal(evaluated.PolicyCheckpointPath, uploaded.PolicyCheckpointPath);
                    Assert.Equal(evaluated.PolicyCheckpointSha256, uploaded.ManifestSha256);
                    Assert.Equal(evaluated.PolicyCheckpointSha256, uploaded.PolicyCheckpointSha256);
                    Assert.Equal(runtimeCheckpointSha256, uploaded.RuntimeProvenanceSha256);
                    Assert.NotEqual(uploaded.PolicyCheckpointSha256, uploaded.RuntimeProvenanceSha256);
                    Assert.Equal(evaluated.Context.VocabSize, uploaded.VocabSize);
                    Assert.Equal(evaluated.Context.ContextLength, uploaded.ContextLen);
                    Assert.Equal(evaluated.Context.HiddenDim, uploaded.HiddenDim);
                    Assert.Equal(evaluated.Context.NumLayers, uploaded.NumLayers);
                    Assert.Equal(evaluated.Context.NumHeads, uploaded.NumHeads);
                    Assert.Equal(evaluated.Context.NumKvHeads, uploaded.NumKvHeads);
                    Assert.Equal(evaluated.Context.FfnHiddenDim, uploaded.FfnHiddenDim);
                    Assert.Equal(evaluated.Context.Lora!.Rank, uploaded.LoraRank);
                    Assert.Equal(evaluated.Context.Lora.Alpha, uploaded.LoraAlpha);
                    Assert.Equal(evaluated.Context.Lora.Dropout, uploaded.LoraDropout);
                    Assert.Equal(evaluated.Context.Lora.TargetModules.ToArray(), uploaded.LoraTargetModules);
                    Assert.Contains("design_conversion_evaluated_dpo_policy_checkpoint", uploaded.PredicateIds);
                    Assert.DoesNotContain("non_transformer_zero_config", uploaded.PredicateIds);
                    Assert.DoesNotContain("not_transformer_not_dpo_not_cloud_ready", BuildManifestPublicText(uploaded));
                    Assert.DoesNotContain(evaluated.RawPolicyPayloadMarker, BuildManifestPublicText(uploaded));
                    AssertNoAbsolutePathLeak(
                        BuildManifestPublicText(uploaded),
                        fixture,
                        evaluated.SourceEncoderPath,
                        evaluated.PolicyCheckpointPath);
                });

            var receipt = result.Value!;
            Assert.Contains("DESIGN_CONVERSION_EVALUATED_DPO_POLICY_CHECKPOINT_UPLOADED", receipt.EvidenceRefs);
            Assert.Contains(
                "DESIGN_CONVERSION_REPOSITORY_VALIDATED_SIBLING_DPO_POLICY_WEIGHT_AUTHORITY",
                receipt.EvidenceRefs);
            Assert.Contains(
                "MODEL_REPOSITORY_SHARED_DPO_POLICY_CHECKPOINT_PUBLISHED_FOR_BOTH_LOGICAL_UNITS",
                receipt.EvidenceRefs);
            Assert.Contains("MODEL_REPOSITORY_POLICY_CHECKPOINT_ARTIFACT_SHA_VERIFIED_BOTH_UNITS", receipt.EvidenceRefs);
            Assert.Contains("critic_learned_weight_artifacts:0", receipt.EvidenceRefs);
            Assert.Contains("improver_learned_weight_artifacts:0", receipt.EvidenceRefs);
            Assert.Contains("dpo_policy_learned_weight_artifacts:1", receipt.EvidenceRefs);
            Assert.Contains("dpo_policy_learned_weight_observations:1", receipt.EvidenceRefs);
            Assert.Contains("loaded_learned_weight_total_artifacts:1", receipt.EvidenceRefs);
            Assert.Contains("loaded_learned_weight_total_observations:1", receipt.EvidenceRefs);
            Assert.Contains("repository_logical_unit_publications:2", receipt.EvidenceRefs);
            Assert.Contains("repository_distinct_policy_checkpoint_artifacts:1", receipt.EvidenceRefs);
            Assert.DoesNotContain(
                "DESIGN_CONVERSION_LEARNED_WEIGHT_JSON_CHECKPOINT_EXPLICIT_NON_TRANSFORMER",
                receipt.EvidenceRefs);
            Assert.DoesNotContain(evaluated.RawPolicyPayloadMarker, BuildPublicReceiptSurface(receipt));
            AssertNoAbsolutePathLeak(
                BuildPublicReceiptSurface(receipt),
                fixture,
                evaluated.SourceEncoderPath,
                evaluated.PolicyCheckpointPath);
        }
        finally
        {
            DeleteDirectoryIfExists(fixture.TempRoot);
        }
    }

    [Fact]
    public async Task UploadAsync_NonEvaluatedRemoteManifestShaMismatchFailsBeforeLoadRegisterOrLocator()
    {
        var fixture = await CreateFixtureAsync("non-eval-remote-manifest-sha-mismatch");
        try
        {
            var calls = new List<string>();
            var repository = new RecordingModelRepository(calls)
            {
                UploadResponseMutation = remote => remote with
                {
                    ManifestSha256 = Sha256Hex(
                        Utf8NoBom.GetBytes("wrong-non-eval-policy-contract-manifest-ref-sha"))
                }
            };
            var locator = new RecordingRemoteUnitLocator(calls);
            var uploader = new DesignConversionModelRepositoryPromotionUploader(
                repository,
                locator,
                ServerBaseUrl);

            var result = await uploader.UploadAsync(fixture.Manifest, fixture.GateReport);

            Assert.False(result.IsSuccess);
            Assert.Equal(
                "DESIGN_CONVERSION_REPOSITORY_UPLOAD_REMOTE_MANIFEST_MISMATCH",
                result.ErrorCode);
            Assert.Null(result.Value);
            Assert.Equal(["upload:critic"], calls);
            var uploaded = Assert.Single(repository.UploadedManifests);
            Assert.Equal(fixture.CheckpointPath, uploaded.PolicyCheckpointPath);
            Assert.Equal(fixture.CheckpointSha256, uploaded.ManifestSha256);
            Assert.Equal(fixture.CheckpointSha256, uploaded.PolicyCheckpointSha256);
            Assert.Equal(fixture.CheckpointSha256, uploaded.RuntimeProvenanceSha256);
            Assert.Empty(repository.LoadFallbackPaths);
            Assert.Empty(locator.Registered);
            AssertNoAbsolutePathLeak(result.ErrorMessage, fixture);
        }
        finally
        {
            DeleteDirectoryIfExists(fixture.TempRoot);
        }
    }

    [Fact]
    public async Task UploadAsync_EvaluatedDpoRemoteRuntimeShaForPolicyUploadFailsBeforeLoadRegisterOrLocator()
    {
        var fixture = await CreateFixtureAsync(
            "evaluated-remote-runtime-sha-for-policy-upload",
            includeCriticLearnedWeights: false,
            includeImproverLearnedWeights: false);
        try
        {
            var evaluated = await CreateEvaluatedDpoArtifactsAsync(
                fixture,
                "evaluated-remote-runtime-sha-for-policy-upload");
            var runtimeCheckpointSha256 = await ReadFileSha256Async(
                evaluated.RuntimeCheckpointPath);
            Assert.NotEqual(
                evaluated.PolicyCheckpointSha256,
                runtimeCheckpointSha256);
            var manifest = fixture.Manifest with
            {
                CheckpointRef = evaluated.RuntimeCheckpointPath,
                ManifestRef = evaluated.RuntimeManifestRef,
                PromotionArtifacts = evaluated.Artifacts
            };
            var calls = new List<string>();
            var repository = new RecordingModelRepository(calls)
            {
                UploadResponseMutation = remote => remote with
                {
                    ManifestSha256 = runtimeCheckpointSha256
                }
            };
            var locator = new RecordingRemoteUnitLocator(calls);
            var uploader = new DesignConversionModelRepositoryPromotionUploader(
                repository,
                locator,
                ServerBaseUrl);

            var result = await uploader.UploadAsync(
                manifest,
                BuildEvaluatedGateReport(fixture.GateReport, evaluated));

            Assert.False(result.IsSuccess);
            Assert.Equal(
                "DESIGN_CONVERSION_REPOSITORY_UPLOAD_REMOTE_MANIFEST_MISMATCH",
                result.ErrorCode);
            Assert.Null(result.Value);
            Assert.Equal(["upload:critic"], calls);
            var uploaded = Assert.Single(repository.UploadedManifests);
            Assert.Equal(evaluated.PolicyCheckpointPath, uploaded.PolicyCheckpointPath);
            Assert.Equal(evaluated.PolicyCheckpointSha256, uploaded.ManifestSha256);
            Assert.Equal(evaluated.PolicyCheckpointSha256, uploaded.PolicyCheckpointSha256);
            Assert.Equal(runtimeCheckpointSha256, uploaded.RuntimeProvenanceSha256);
            Assert.NotEqual(uploaded.PolicyCheckpointSha256, uploaded.RuntimeProvenanceSha256);
            Assert.Empty(repository.LoadFallbackPaths);
            Assert.Empty(locator.Registered);
            AssertNoAbsolutePathLeak(
                result.ErrorMessage,
                fixture,
                evaluated.SourceEncoderPath,
                evaluated.PolicyCheckpointPath);
        }
        finally
        {
            DeleteDirectoryIfExists(fixture.TempRoot);
        }
    }

    [Fact]
    public async Task UploadAsync_EvaluatedDpoLoadedAuthorityMismatchFailsBeforeRepositoryOrLocator()
    {
        var fixture = await CreateFixtureAsync(
            "evaluated-authority-mismatch",
            includeCriticLearnedWeights: false,
            includeImproverLearnedWeights: false);
        try
        {
            var evaluated = await CreateEvaluatedDpoArtifactsAsync(
                fixture,
                "evaluated-authority-mismatch");
            var manifest = fixture.Manifest with
            {
                CheckpointRef = evaluated.RuntimeCheckpointPath,
                ManifestRef = evaluated.RuntimeManifestRef,
                PromotionArtifacts = evaluated.Artifacts
            };
            var calls = new List<string>();
            var repository = new RecordingModelRepository(calls);
            var locator = new RecordingRemoteUnitLocator(calls);
            var matcherCallCount = 0;
            var uploader = new DesignConversionModelRepositoryPromotionUploader(
                repository,
                locator,
                ServerBaseUrl,
                (lease, load) =>
                {
                    matcherCallCount++;
                    Assert.True(lease.Revalidate());
                    Assert.NotNull(load.VerifiedDpoPolicyRuntimeScorerLoaded);
                    return false;
                });

            var result = await uploader.UploadAsync(
                manifest,
                BuildEvaluatedGateReport(fixture.GateReport, evaluated));

            Assert.False(result.IsSuccess);
            Assert.Equal(
                "DESIGN_CONVERSION_REPOSITORY_UPLOAD_EVALUATED_DPO_ARTIFACT_CONTRACT_INVALID",
                result.ErrorCode);
            Assert.Null(result.Value);
            Assert.Equal(1, matcherCallCount);
            Assert.Empty(calls);
            Assert.Empty(repository.UploadedManifests);
            Assert.Empty(repository.LoadFallbackPaths);
            Assert.Empty(locator.Registered);
            AssertNoAbsolutePathLeak(
                result.ErrorMessage,
                fixture,
                evaluated.RuntimeCheckpointPath,
                evaluated.SourceEncoderPath,
                evaluated.PolicyCheckpointPath);
        }
        finally
        {
            DeleteDirectoryIfExists(fixture.TempRoot);
        }
    }

    [Fact]
    public async Task UploadAsync_AlteredUploadResponseModelConfigFailsBeforeLocator()
    {
        var fixture = await CreateFixtureAsync("altered-upload-response-config");
        try
        {
            var evaluated = await CreateEvaluatedDpoArtifactsAsync(
                fixture,
                "altered-upload-response-config");
            var manifest = fixture.Manifest with
            {
                CheckpointRef = evaluated.RuntimeCheckpointPath,
                ManifestRef = evaluated.RuntimeManifestRef,
                PromotionArtifacts = evaluated.Artifacts
            };
            var calls = new List<string>();
            var repository = new RecordingModelRepository(calls)
            {
                UploadResponseMutation = remote => remote with
                {
                    ModelConfig = remote.ModelConfig with
                    {
                        RopeTheta = remote.ModelConfig.RopeTheta is { } value
                            ? value * 2f
                            : 1f
                    }
                }
            };
            var locator = new RecordingRemoteUnitLocator(calls);
            var uploader = new DesignConversionModelRepositoryPromotionUploader(
                repository,
                locator,
                ServerBaseUrl);

            var result = await uploader.UploadAsync(
                manifest,
                BuildEvaluatedGateReport(fixture.GateReport, evaluated));

            Assert.False(result.IsSuccess);
            Assert.Equal(
                "DESIGN_CONVERSION_REPOSITORY_UPLOAD_REMOTE_MANIFEST_MISMATCH",
                result.ErrorCode);
            Assert.Null(result.Value);
            Assert.Equal(["upload:critic"], calls);
            Assert.Single(repository.UploadedManifests);
            Assert.Empty(locator.Registered);
            AssertNoAbsolutePathLeak(
                result.ErrorMessage,
                fixture,
                evaluated.SourceEncoderPath,
                evaluated.PolicyCheckpointPath);
        }
        finally
        {
            DeleteDirectoryIfExists(fixture.TempRoot);
        }
    }

    [Fact]
    public Task UploadAsync_NullUploadResponseModelConfigFailsBeforeLocator()
        => AssertNullUploadResponseConfigFailsBeforeLocatorAsync(nullModelConfig: true);

    [Fact]
    public Task UploadAsync_NullUploadResponseLoraConfigFailsBeforeLocator()
        => AssertNullUploadResponseConfigFailsBeforeLocatorAsync(nullModelConfig: false);

    private static async Task AssertNullUploadResponseConfigFailsBeforeLocatorAsync(
        bool nullModelConfig)
    {
        var fixture = await CreateFixtureAsync(
            nullModelConfig ? "null-upload-model-config" : "null-upload-lora-config");
        try
        {
            var evaluated = await CreateEvaluatedDpoArtifactsAsync(
                fixture,
                nullModelConfig ? "null-upload-model-config" : "null-upload-lora-config");
            var manifest = fixture.Manifest with
            {
                CheckpointRef = evaluated.RuntimeCheckpointPath,
                ManifestRef = evaluated.RuntimeManifestRef,
                PromotionArtifacts = evaluated.Artifacts
            };
            var calls = new List<string>();
            var repository = new RecordingModelRepository(calls)
            {
                UploadResponseMutation = remote => nullModelConfig
                    ? remote with { ModelConfig = null! }
                    : remote with { LoraConfig = null! }
            };
            var locator = new RecordingRemoteUnitLocator(calls);
            var uploader = new DesignConversionModelRepositoryPromotionUploader(
                repository,
                locator,
                ServerBaseUrl);

            var result = await uploader.UploadAsync(
                manifest,
                BuildEvaluatedGateReport(fixture.GateReport, evaluated));

            Assert.False(result.IsSuccess);
            Assert.Equal(
                "DESIGN_CONVERSION_REPOSITORY_UPLOAD_REMOTE_MANIFEST_MISMATCH",
                result.ErrorCode);
            Assert.Null(result.Value);
            Assert.Equal(["upload:critic"], calls);
            Assert.Single(repository.UploadedManifests);
            Assert.Empty(repository.LoadFallbackPaths);
            Assert.Empty(locator.Registered);
            AssertNoAbsolutePathLeak(
                result.ErrorMessage,
                fixture,
                evaluated.SourceEncoderPath,
                evaluated.PolicyCheckpointPath);
        }
        finally
        {
            DeleteDirectoryIfExists(fixture.TempRoot);
        }
    }

    [Fact]
    public Task UploadAsync_AlteredLocatedModelConfigFailsBeforeReceipt()
        => AssertAlteredLocatedRemoteIdentityFailsBeforeReceiptAsync("model-config");

    [Fact]
    public Task UploadAsync_AlteredLocatedTrainingIdentityFailsBeforeReceipt()
        => AssertAlteredLocatedRemoteIdentityFailsBeforeReceiptAsync("training-identity");

    private static async Task AssertAlteredLocatedRemoteIdentityFailsBeforeReceiptAsync(string mutation)
    {
        var fixture = await CreateFixtureAsync($"altered-located-{mutation}");
        try
        {
            var evaluated = await CreateEvaluatedDpoArtifactsAsync(
                fixture,
                $"altered-located-{mutation}");
            var manifest = fixture.Manifest with
            {
                CheckpointRef = evaluated.RuntimeCheckpointPath,
                ManifestRef = evaluated.RuntimeManifestRef,
                PromotionArtifacts = evaluated.Artifacts
            };
            var calls = new List<string>();
            var repository = new RecordingModelRepository(calls);
            var locator = new RecordingRemoteUnitLocator(calls)
            {
                LocateResponseMutation = remote => mutation switch
                {
                    "model-config" => remote with
                    {
                        ModelConfig = remote.ModelConfig with
                        {
                            RmsNormEps = remote.ModelConfig.RmsNormEps is { } value
                                ? value * 2f
                                : 1f
                        }
                    },
                    "training-identity" => remote with
                    {
                        TrainingRunId = remote.TrainingRunId + "-mutated"
                    },
                    _ => throw new InvalidOperationException("Unknown located-manifest mutation.")
                }
            };
            var uploader = new DesignConversionModelRepositoryPromotionUploader(
                repository,
                locator,
                ServerBaseUrl);

            var result = await uploader.UploadAsync(
                manifest,
                BuildEvaluatedGateReport(fixture.GateReport, evaluated));

            Assert.False(result.IsSuccess);
            Assert.Equal(
                "DESIGN_CONVERSION_REPOSITORY_LOCATOR_IDENTITY_MISMATCH",
                result.ErrorCode);
            Assert.Null(result.Value);
            Assert.Equal(
                ["upload:critic", "upload:improver", "load:critic", "load:improver", "register:critic", "register:improver", "locate:critic"],
                calls);
            Assert.Equal(2, locator.Registered.Count);
            AssertNoAbsolutePathLeak(
                result.ErrorMessage,
                fixture,
                evaluated.SourceEncoderPath,
                evaluated.PolicyCheckpointPath);
        }
        finally
        {
            DeleteDirectoryIfExists(fixture.TempRoot);
        }
    }

    [Fact]
    public async Task UploadAsync_EvaluatedDpoSourceEncoderContractMismatchFailsBeforeRepositoryOrLocator()
    {
        var fixture = await CreateFixtureAsync("source-encoder-mismatch");
        try
        {
            var evaluated = await CreateEvaluatedDpoArtifactsAsync(fixture, "source-encoder-mismatch");
            var sourceContract = JsonSerializer.Deserialize<DesignConversionDpoSourceEncoderContract>(
                await File.ReadAllBytesAsync(evaluated.SourceEncoderPath),
                DesignConversionCheckpointJsonOptions.Standard);
            Assert.NotNull(sourceContract);
            var trustedSourceContract = sourceContract!;
            var mismatchedSourceContract = trustedSourceContract with
            {
                Binding = trustedSourceContract.Binding with
                {
                    PolicyCheckpointSha256 = Sha256Hex(Utf8NoBom.GetBytes("mismatched-policy-checkpoint-sha"))
                }
            };
            await File.WriteAllBytesAsync(
                evaluated.SourceEncoderPath,
                JsonSerializer.SerializeToUtf8Bytes(
                    mismatchedSourceContract,
                    DesignConversionCheckpointJsonOptions.Standard));
            var mismatchedSourceSha256 = await ReadFileSha256Async(evaluated.SourceEncoderPath);
            var mismatchedSourceLength = new FileInfo(evaluated.SourceEncoderPath).Length;
            var sourceDescriptor = evaluated.Artifacts.SourceEncoderContractArtifact!;
            var manifest = fixture.Manifest with
            {
                CheckpointRef = evaluated.RuntimeCheckpointPath,
                ManifestRef = evaluated.RuntimeManifestRef,
                PromotionArtifacts = evaluated.Artifacts with
                {
                    SourceEncoderContractArtifact = BuildArtifactDescriptor(
                        "design_conversion_dpo_source_encoder_contract",
                        evaluated.SourceEncoderPath,
                        mismatchedSourceSha256,
                        mismatchedSourceLength,
                        sourceDescriptor.ArtifactSetId,
                        sourceDescriptor.RuntimeCheckpointSha256)
                }
            };
            var gateReport = BuildEvaluatedGateReport(fixture.GateReport, evaluated);
            var calls = new List<string>();
            var repository = new RecordingModelRepository(calls);
            var locator = new RecordingRemoteUnitLocator(calls);
            var uploader = new DesignConversionModelRepositoryPromotionUploader(
                repository,
                locator,
                ServerBaseUrl);

            var result = await uploader.UploadAsync(manifest, gateReport);

            Assert.False(result.IsSuccess);
            Assert.Equal(
                "DESIGN_CONVERSION_REPOSITORY_UPLOAD_EVALUATED_DPO_ARTIFACT_CONTRACT_INVALID",
                result.ErrorCode);
            Assert.Null(result.Value);
            Assert.Empty(calls);
            Assert.Empty(repository.UploadedManifests);
            Assert.Empty(locator.Registered);
            AssertNoAbsolutePathLeak(
                result.ErrorMessage,
                fixture,
                evaluated.SourceEncoderPath,
                evaluated.PolicyCheckpointPath);
        }
        finally
        {
            DeleteDirectoryIfExists(fixture.TempRoot);
        }
    }

    [Fact]
    public async Task UploadAsync_EvaluatedDpoWrongRuntimeCheckpointIdentityFailsBeforeRepositoryOrLocator()
    {
        var fixture = await CreateFixtureAsync("wrong-runtime-checkpoint-identity");
        try
        {
            var evaluated = await CreateEvaluatedDpoArtifactsAsync(
                fixture,
                "wrong-runtime-checkpoint-identity");
            var mutatedArtifacts = await BuildWrongRuntimeIdentityArtifactsAsync(evaluated);
            var manifest = fixture.Manifest with
            {
                CheckpointRef = evaluated.RuntimeCheckpointPath,
                ManifestRef = evaluated.RuntimeManifestRef,
                PromotionArtifacts = mutatedArtifacts
            };
            var calls = new List<string>();
            var repository = new RecordingModelRepository(calls);
            var locator = new RecordingRemoteUnitLocator(calls);
            var uploader = new DesignConversionModelRepositoryPromotionUploader(
                repository,
                locator,
                ServerBaseUrl);

            var result = await uploader.UploadAsync(
                manifest,
                BuildEvaluatedGateReport(fixture.GateReport, evaluated));

            Assert.False(result.IsSuccess);
            Assert.Equal(
                "DESIGN_CONVERSION_REPOSITORY_UPLOAD_EVALUATED_DPO_ARTIFACT_CONTRACT_INVALID",
                result.ErrorCode);
            Assert.Null(result.Value);
            Assert.Empty(calls);
            Assert.Empty(repository.UploadedManifests);
            Assert.Empty(locator.Registered);
            AssertNoAbsolutePathLeak(
                result.ErrorMessage,
                fixture,
                evaluated.RuntimeCheckpointPath,
                evaluated.SourceEncoderPath,
                evaluated.PolicyCheckpointPath);
        }
        finally
        {
            DeleteDirectoryIfExists(fixture.TempRoot);
        }
    }

    [Fact]
    public async Task UploadAsync_SecondUploadFailureDoesNotLoadRegisterOrLocate()
    {
        var fixture = await CreateFixtureAsync("second-upload-failure");
        try
        {
            var calls = new List<string>();
            var repository = new RecordingModelRepository(calls)
            {
                FailUploadAlias = "improver"
            };
            var locator = new RecordingRemoteUnitLocator(calls);
            var uploader = new DesignConversionModelRepositoryPromotionUploader(
                repository,
                locator,
                ServerBaseUrl);

            var result = await uploader.UploadAsync(fixture.Manifest, fixture.GateReport);

            Assert.False(result.IsSuccess);
            Assert.Equal("REMOTE_UPLOAD_FAILED_SAFE", result.ErrorCode);
            Assert.Equal(["upload:critic", "upload:improver"], calls);
            Assert.Empty(locator.Registered);
        }
        finally
        {
            DeleteDirectoryIfExists(fixture.TempRoot);
        }
    }

    [Fact]
    public async Task UploadAsync_CollidingSafeRepositoryEndpointsFailsBeforeLoadOrLocator()
    {
        var fixture = await CreateFixtureAsync("colliding-safe-endpoints");
        try
        {
            var calls = new List<string>();
            var repository = new RecordingModelRepository(calls)
            {
                ImproverEndpointOverride =
                    $"/models/{Uri.EscapeDataString(DesignConversionRuntimeScaffoldCriticUnit.UnitId)}"
            };
            var locator = new RecordingRemoteUnitLocator(calls);
            var uploader = new DesignConversionModelRepositoryPromotionUploader(
                repository,
                locator,
                ServerBaseUrl);

            var result = await uploader.UploadAsync(fixture.Manifest, fixture.GateReport);

            Assert.False(result.IsSuccess);
            Assert.Equal("DESIGN_CONVERSION_REPOSITORY_UPLOAD_REMOTE_MANIFEST_MISMATCH", result.ErrorCode);
            Assert.Equal(["upload:critic", "upload:improver"], calls);
            Assert.Equal(2, repository.UploadedManifests.Count);
            Assert.Empty(locator.Registered);
            AssertNoAbsolutePathLeak(result.ErrorMessage, fixture);
        }
        finally
        {
            DeleteDirectoryIfExists(fixture.TempRoot);
        }
    }

    [Fact]
    public async Task UploadAsync_NoncanonicalSafeRepositoryEndpointFailsBeforeLoadOrLocator()
    {
        var fixture = await CreateFixtureAsync("noncanonical-safe-endpoint");
        try
        {
            var calls = new List<string>();
            var repository = new RecordingModelRepository(calls)
            {
                ImproverEndpointOverride = "/models/safe-improver-alias"
            };
            var locator = new RecordingRemoteUnitLocator(calls);
            var uploader = new DesignConversionModelRepositoryPromotionUploader(
                repository,
                locator,
                ServerBaseUrl);

            var result = await uploader.UploadAsync(fixture.Manifest, fixture.GateReport);

            Assert.False(result.IsSuccess);
            Assert.Equal("DESIGN_CONVERSION_REPOSITORY_UPLOAD_REMOTE_MANIFEST_MISMATCH", result.ErrorCode);
            Assert.Equal(["upload:critic", "upload:improver"], calls);
            Assert.Equal(2, repository.UploadedManifests.Count);
            Assert.Empty(locator.Registered);
            AssertNoAbsolutePathLeak(result.ErrorMessage, fixture);
        }
        finally
        {
            DeleteDirectoryIfExists(fixture.TempRoot);
        }
    }

    [Fact]
    public async Task UploadAsync_FreshLoadHashMismatchDoesNotRegisterOrLocate()
    {
        var fixture = await CreateFixtureAsync("fresh-load-mismatch");
        try
        {
            var calls = new List<string>();
            var repository = new RecordingModelRepository(calls)
            {
                HashMismatchLoadAlias = "improver"
            };
            var locator = new RecordingRemoteUnitLocator(calls);
            var uploader = new DesignConversionModelRepositoryPromotionUploader(
                repository,
                locator,
                ServerBaseUrl);

            var result = await uploader.UploadAsync(fixture.Manifest, fixture.GateReport);

            Assert.False(result.IsSuccess);
            Assert.Equal("DESIGN_CONVERSION_REPOSITORY_LOAD_HASH_MISMATCH", result.ErrorCode);
            Assert.Equal(
                ["upload:critic", "upload:improver", "load:critic", "load:improver"],
                calls);
            Assert.Empty(locator.Registered);
        }
        finally
        {
            DeleteDirectoryIfExists(fixture.TempRoot);
        }
    }

    [Fact]
    public async Task UploadAsync_FailedPromotionGateDoesNotCallRepositoryOrLocator()
    {
        var fixture = await CreateFixtureAsync("failed-gate");
        try
        {
            var calls = new List<string>();
            var repository = new RecordingModelRepository(calls);
            var locator = new RecordingRemoteUnitLocator(calls);
            var uploader = new DesignConversionModelRepositoryPromotionUploader(
                repository,
                locator,
                ServerBaseUrl);

            var rejectedGate = fixture.GateReport with
            {
                HeldOutScore = 0.40,
                RequiredHeldOutScore = 0.90
            };
            var result = await uploader.UploadAsync(fixture.Manifest, rejectedGate);

            Assert.False(result.IsSuccess);
            Assert.Equal("DESIGN_CONVERSION_REPOSITORY_UPLOAD_GATE_REJECTED", result.ErrorCode);
            Assert.Empty(calls);
            Assert.Empty(repository.UploadedManifests);
            Assert.Empty(locator.Registered);
        }
        finally
        {
            DeleteDirectoryIfExists(fixture.TempRoot);
        }
    }

    [Theory]
    [InlineData("metadata-mismatch")]
    [InlineData("missing-artifact")]
    public async Task UploadAsync_MetadataMismatchOrMissingArtifactDoesNotUploadOrRegister(string scenario)
    {
        var fixture = await CreateFixtureAsync($"pre-upload-{scenario}");
        try
        {
            var calls = new List<string>();
            var repository = new RecordingModelRepository(calls);
            var locator = new RecordingRemoteUnitLocator(calls);
            var uploader = new DesignConversionModelRepositoryPromotionUploader(
                repository,
                locator,
                ServerBaseUrl);

            var manifest = scenario == "metadata-mismatch"
                ? fixture.Manifest with { Version = "9.9.9" }
                : fixture.Manifest;
            if (scenario == "missing-artifact")
                File.Delete(fixture.TokenizerPath);

            var result = await uploader.UploadAsync(manifest, fixture.GateReport);

            Assert.False(result.IsSuccess);
            Assert.Equal(
                scenario == "metadata-mismatch"
                    ? "DESIGN_CONVERSION_REPOSITORY_UPLOAD_CHECKPOINT_METADATA_MISMATCH"
                    : "DESIGN_CONVERSION_REPOSITORY_UPLOAD_TOKENIZER_ARTIFACT_MISSING",
                result.ErrorCode);
            Assert.Empty(calls);
            Assert.Empty(repository.UploadedManifests);
            Assert.Empty(locator.Registered);
            AssertNoAbsolutePathLeak(result.ErrorMessage, fixture);
        }
        finally
        {
            DeleteDirectoryIfExists(fixture.TempRoot);
        }
    }

    [Fact]
    public async Task UploadAsync_MetadataOnlyCheckpointDoesNotUploadOrRegister()
    {
        var fixture = await CreateFixtureAsync(
            "metadata-only",
            includeCriticLearnedWeights: false,
            includeImproverLearnedWeights: true);
        try
        {
            var calls = new List<string>();
            var repository = new RecordingModelRepository(calls);
            var locator = new RecordingRemoteUnitLocator(calls);
            var uploader = new DesignConversionModelRepositoryPromotionUploader(
                repository,
                locator,
                ServerBaseUrl);

            var result = await uploader.UploadAsync(fixture.Manifest, fixture.GateReport);

            Assert.False(result.IsSuccess);
            Assert.Equal("DESIGN_CONVERSION_REPOSITORY_UPLOAD_LEARNED_WEIGHTS_REQUIRED", result.ErrorCode);
            Assert.Empty(calls);
            Assert.Empty(repository.UploadedManifests);
            Assert.Empty(locator.Registered);
        }
        finally
        {
            DeleteDirectoryIfExists(fixture.TempRoot);
        }
    }

    [Fact]
    public async Task UploadAsync_CancellationDoesNotCallRepositoryOrLocator()
    {
        var fixture = await CreateFixtureAsync("cancelled");
        try
        {
            var calls = new List<string>();
            var repository = new RecordingModelRepository(calls);
            var locator = new RecordingRemoteUnitLocator(calls);
            var uploader = new DesignConversionModelRepositoryPromotionUploader(
                repository,
                locator,
                ServerBaseUrl);
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();

            var result = await uploader.UploadAsync(fixture.Manifest, fixture.GateReport, cancellation.Token);

            Assert.False(result.IsSuccess);
            Assert.Equal("CANCELLED", result.ErrorCode);
            Assert.Empty(calls);
            Assert.Empty(repository.UploadedManifests);
            Assert.Empty(locator.Registered);
        }
        finally
        {
            DeleteDirectoryIfExists(fixture.TempRoot);
        }
    }

    [Fact]
    public async Task UploadAsync_SecondRegisterFailureFailsClosedAfterFirstRegistrationAndDocumentsRollbackRisk()
    {
        var fixture = await CreateFixtureAsync("second-register-failure");
        try
        {
            var calls = new List<string>();
            var repository = new RecordingModelRepository(calls);
            var locator = new RecordingRemoteUnitLocator(calls)
            {
                FailRegisterAlias = "improver"
            };
            var uploader = new DesignConversionModelRepositoryPromotionUploader(
                repository,
                locator,
                ServerBaseUrl);

            var result = await uploader.UploadAsync(fixture.Manifest, fixture.GateReport);

            Assert.False(result.IsSuccess);
            Assert.Equal("REMOTE_REGISTER_FAILED_SAFE", result.ErrorCode);
            Assert.Contains("no rollback API", result.ErrorMessage, StringComparison.Ordinal);
            Assert.Equal(
                [
                    "upload:critic",
                    "upload:improver",
                    "load:critic",
                    "load:improver",
                    "register:critic",
                    "register:improver"
                ],
                calls);
            var registered = Assert.Single(locator.Registered);
            Assert.Equal(DesignConversionRuntimeScaffoldCriticUnit.UnitId, registered.UnitId);
        }
        finally
        {
            DeleteDirectoryIfExists(fixture.TempRoot);
        }
    }

    private static async Task<PromotionFixture> CreateFixtureAsync(string name)
        => await CreateFixtureAsync(
            name,
            includeCriticLearnedWeights: true,
            includeImproverLearnedWeights: true);

    private static async Task<PromotionFixture> CreateFixtureAsync(
        string name,
        bool includeCriticLearnedWeights,
        bool includeImproverLearnedWeights)
    {
        var tempRoot = Path.GetFullPath(Path.Combine(
            Path.GetTempPath(),
            "llm_mvp_core",
            "design-conversion-repository-promotion-uploader-tests",
            Guid.NewGuid().ToString("N")));
        var artifactRoot = Path.Combine(tempRoot, "artifacts");
        Directory.CreateDirectory(artifactRoot);

        var checkpointPath = Path.Combine(artifactRoot, "machine-checkpoint.json");
        var tokenizerPath = Path.Combine(artifactRoot, "tokenizer.json");
        var policyPath = Path.Combine(artifactRoot, "policy.json");
        await File.WriteAllTextAsync(tokenizerPath, """{"kind":"design-conversion-tokenizer-test"}""", Utf8NoBom);
        await File.WriteAllTextAsync(policyPath, """{"kind":"supporting-policy-artifact-required-but-not-uploaded-as-transformer"}""", Utf8NoBom);

        var machine = new DesignConversionMachine();
        var template = machine.ExportCheckpoint();
        Assert.True(template.IsSuccess, template.ErrorMessage);
        var checkpoint = template.Value! with
        {
            Version = "0.1.140",
            AccuracyAfterLastBatch = 0.72,
            HasTrainingScaffoldHistory = true,
            CriticUnit = includeCriticLearnedWeights
                ? WithLearnedWeights(
                    template.Value.CriticUnit,
                    [ValidLearnedWeight("source_kind:figma", 0.31)])
                : template.Value.CriticUnit,
            ImproverUnit = includeImproverLearnedWeights
                ? WithLearnedWeights(
                    template.Value.ImproverUnit,
                    [ValidLearnedWeight("correction_category:spacing", 0.69)])
                : template.Value.ImproverUnit,
            TrainingHistory =
            [
                new DesignConversionTrainingHistoryEntryCheckpoint(
                    BatchId: $"{name}-batch",
                    RowsTrained: 2,
                    AccuracyBefore: 0.70,
                    AccuracyAfter: 0.72,
                    RewardMargin: 0.18,
                    CpuMs: 3,
                    PeakRamMb: 4,
                    ElapsedMs: 5,
                    PromotedUnitIds: [])
            ]
        };
        var import = machine.ImportCheckpoint(checkpoint);
        Assert.True(import.IsSuccess, import.ErrorMessage);
        var saved = await machine.SaveCheckpointAsync(checkpointPath);
        Assert.True(saved.IsSuccess, saved.ErrorMessage);
        var checkpointSha256 = await ReadFileSha256Async(checkpointPath);

        var manifest = new DesignConversionTrainingManifest(
            ManifestId: $"{name}-manifest",
            MachineId: saved.Value!.MachineId,
            DomainTag: saved.Value.DomainTag,
            Version: saved.Value.Version,
            CheckpointRef: saved.Value.CheckpointRef,
            ManifestRef: saved.Value.ManifestRef,
            CreatedAtUtc: DateTimeOffset.Parse("2026-07-27T08:00:00Z"),
            AcceptedRecordCount: 2,
            QuarantinedRecordCount: 0,
            UnitIds:
            [
                DesignConversionRuntimeScaffoldCriticUnit.UnitId,
                DesignConversionRuntimeScaffoldImproverUnit.UnitId
            ],
            Batches:
            [
                new DesignConversionTrainingManifestBatch(
                    BatchId: $"{name}-critic-batch",
                    UnitId: DesignConversionRuntimeScaffoldCriticUnit.UnitId,
                    RecordCount: 1,
                    RecordIds: [$"{name}-record-b"],
                    SourceIds: [$"{name}-source-2"]),
                new DesignConversionTrainingManifestBatch(
                    BatchId: $"{name}-improver-batch",
                    UnitId: DesignConversionRuntimeScaffoldImproverUnit.UnitId,
                    RecordCount: 1,
                    RecordIds: [$"{name}-record-a"],
                    SourceIds: [$"{name}-source-1"])
            ],
            EvidenceRefs: ["manifest-safe-evidence"],
            ModelServerUploadAttempted: false,
            ModelServerPromotionClaimed: false,
            DeterministicLogicClaimedAsLearning: false)
        {
            PromotionArtifacts = new DesignConversionTrainingManifestArtifacts(
                checkpointPath,
                tokenizerPath,
                policyPath)
        };

        var gateReport = new DesignConversionPromotionGateReport(
            HeldOutScore: 0.93,
            RequiredHeldOutScore: 0.90,
            HeldOutSourceDisjoint: true,
            DeterministicShortcutDetected: false,
            HiddenShortcutEvidenceDetected: false,
            FreshLoadPassed: true,
            BudgetTierPolicyPassed: true,
            BudgetTier: "bounded-test-tier",
            DpoQuarantineCount: 0,
            MaxDpoQuarantineCount: 0,
            RequiredArtifactPaths: [checkpointPath, tokenizerPath, policyPath],
            PresentArtifactPaths: [checkpointPath, tokenizerPath, policyPath],
            EvidenceRefs: ["gate-safe-evidence"]);

        return new PromotionFixture(
            TempRoot: tempRoot,
            CheckpointPath: Path.GetFullPath(checkpointPath),
            TokenizerPath: Path.GetFullPath(tokenizerPath),
            PolicyPath: Path.GetFullPath(policyPath),
            CheckpointSha256: checkpointSha256,
            ExpectedSourceDatasetSha256: BuildExpectedSourceDatasetSha256(manifest),
            Manifest: manifest,
            GateReport: gateReport);
    }

    private static DesignConversionRuntimeUnitCheckpoint WithLearnedWeights(
        DesignConversionRuntimeUnitCheckpoint template,
        IReadOnlyList<DesignConversionLearnedWeightCheckpoint> learnedWeights)
        => template with
        {
            ImplementationStatus = $"{template.ImplementationStatus}_with_local_learned_weight_artifact",
            EvidenceRefs = template.EvidenceRefs
                .Concat(
                [
                    DesignConversionMachine.LearnedWeightsPersistedEvidence,
                    DesignConversionMachine.LearnedWeightsSanitizedFieldsEvidence
                ])
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray(),
            LearnedWeightsPresent = true,
            LearnedWeights = learnedWeights
        };

    private static DesignConversionLearnedWeightCheckpoint ValidLearnedWeight(string featureKey, double weight)
        => new(
            FeatureKey: featureKey,
            Weight: weight,
            ObservationCount: 4,
            LastBatchId: "repository-promotion-uploader-test",
            Derivation: "sanitized_repository_promotion_test",
            SourceIdentityHashes:
            [
                "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"
            ],
            EvidenceRefs:
            [
                DesignConversionMachine.LearnedWeightsPersistedEvidence,
                DesignConversionMachine.LearnedWeightsSanitizedFieldsEvidence,
                "DESIGN_CONVERSION_LEARNED_WEIGHT_NO_RAW_TREE_HTML_SCREENSHOT"
            ]);

    private static string BuildExpectedSourceDatasetSha256(DesignConversionTrainingManifest manifest)
    {
        var refs = manifest.Batches
            .SelectMany(batch => batch.RecordIds.Select(recordId => $"record:{recordId}")
                .Concat(batch.SourceIds.Select(sourceId => $"source:{sourceId}")))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
        return Sha256Hex(Encoding.UTF8.GetBytes(string.Join("\n", refs)));
    }

    private static string BuildManifestPublicText(GeneratedCapabilityCheckpointManifest manifest)
        => string.Join(
            "|",
            manifest.TrainingText,
            manifest.OutputText,
            manifest.OriginalPrompt,
            manifest.ExampleId,
            string.Join(",", manifest.PredicateIds),
            manifest.TaskType,
            manifest.UnitFamilyId,
            manifest.TrainingBranch ?? string.Empty,
            manifest.TrainingRunId ?? string.Empty,
            manifest.SourceDatasetSha256 ?? string.Empty);

    private static string BuildPublicReceiptSurface(DesignConversionPromotionUploadReceipt receipt)
        => string.Join(
            "|",
            new[]
            {
                receipt.UploadId,
                receipt.RemoteRef,
                receipt.ProvenanceRef,
                receipt.PublicationStatus
            }.Concat(receipt.EvidenceRefs));

    private static void AssertNoAbsolutePathLeak(
        string surface,
        PromotionFixture fixture,
        params string[] additionalPaths)
    {
        foreach (var path in new[] { fixture.TempRoot, fixture.CheckpointPath, fixture.TokenizerPath, fixture.PolicyPath }
                     .Concat(additionalPaths))
        {
            Assert.DoesNotContain(path, surface, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(path.Replace('\\', '/'), surface, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(new Uri(path).AbsoluteUri, surface, StringComparison.OrdinalIgnoreCase);
        }
    }

    private static async Task<string> ReadFileSha256Async(string path)
        => Sha256Hex(await File.ReadAllBytesAsync(path));

    private static string Sha256Hex(byte[] bytes)
        => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static void SynchronizeFullCheckpointStateAndFreezeReference(
        LlmModel policy,
        LlmModel reference)
    {
        var policyEntries = policy.GetCheckpointParameterEntries();
        var referenceEntries = reference.GetCheckpointParameterEntries();
        Assert.Equal(policyEntries.Count, referenceEntries.Count);
        for (var index = 0; index < policyEntries.Count; index++)
        {
            var policyEntry = policyEntries[index];
            var referenceEntry = referenceEntries[index];
            Assert.Equal(policyEntry.Key, referenceEntry.Key);
            Assert.Equal(
                policyEntry.Parameter.Value.Shape.Dimensions,
                referenceEntry.Parameter.Value.Shape.Dimensions);
            Assert.NotSame(policyEntry.Parameter, referenceEntry.Parameter);
            Assert.NotSame(policyEntry.Parameter.Value, referenceEntry.Parameter.Value);
            Assert.NotSame(
                policyEntry.Parameter.Value.Storage,
                referenceEntry.Parameter.Value.Storage);
            Assert.NotSame(
                policyEntry.Parameter.Value.Data,
                referenceEntry.Parameter.Value.Data);
            Assert.Equal(
                policyEntry.Parameter.Value.Data.Length,
                referenceEntry.Parameter.Value.Data.Length);

            policyEntry.Parameter.Value.Data.CopyTo(
                referenceEntry.Parameter.Value.Data,
                0);
            referenceEntry.Parameter.Value.RequiresGrad = false;
            referenceEntry.Parameter.Value.Grad = null;
        }

        AssertCheckpointStateBitwiseEqualAndDistinct(policy, reference);
        Assert.All(
            reference.GetCheckpointParameterEntries(),
            entry =>
            {
                Assert.False(entry.Parameter.Value.RequiresGrad);
                Assert.Null(entry.Parameter.Value.Grad);
            });
    }

    private static void AssertCheckpointStateBitwiseEqualAndDistinct(
        LlmModel expected,
        LlmModel actual)
    {
        Assert.NotSame(expected, actual);
        var expectedEntries = expected.GetCheckpointParameterEntries();
        var actualEntries = actual.GetCheckpointParameterEntries();
        Assert.Equal(expectedEntries.Count, actualEntries.Count);
        for (var index = 0; index < expectedEntries.Count; index++)
        {
            var expectedEntry = expectedEntries[index];
            var actualEntry = actualEntries[index];
            Assert.Equal(expectedEntry.Key, actualEntry.Key);
            Assert.Equal(
                expectedEntry.Parameter.Value.Shape.Dimensions,
                actualEntry.Parameter.Value.Shape.Dimensions);
            Assert.NotSame(expectedEntry.Parameter, actualEntry.Parameter);
            Assert.NotSame(expectedEntry.Parameter.Value, actualEntry.Parameter.Value);
            Assert.NotSame(
                expectedEntry.Parameter.Value.Storage,
                actualEntry.Parameter.Value.Storage);
            Assert.NotSame(
                expectedEntry.Parameter.Value.Data,
                actualEntry.Parameter.Value.Data);
            Assert.Equal(
                expectedEntry.Parameter.Value.Data.Select(BitConverter.SingleToInt32Bits),
                actualEntry.Parameter.Value.Data.Select(BitConverter.SingleToInt32Bits));
        }
    }

    private static async Task<EvaluatedDpoArtifactFixture> CreateEvaluatedDpoArtifactsAsync(
        PromotionFixture fixture,
        string name)
    {
        var artifactRoot = Path.GetDirectoryName(fixture.PolicyPath)!;
        var paths = DesignConversionDpoPromotionArtifactContractCodec.ResolvePaths(
            fixture.TempRoot,
            artifactRoot,
            name);
        Assert.True(paths.IsSuccess, paths.ErrorMessage);

        var partition = BuildEvaluatedPartitionForRepositoryUploaderTests(name);
        var evaluated = await DesignConversionDpoPolicyCheckpointBridge
            .ExecuteEvaluatedAndConvertAsync(
                DesignConversionLegalTeacherCapabilityTestFactory
                    .CreateAdmitted(),
                partition,
                paths.Value!.OutputDirectory,
                name,
                paths.Value.RuntimeCheckpointPath,
                new DesignConversionDpoPolicyBaseTestProvider(),
                new DesignConversionDpoPolicyCheckpointBridgeOptions(1, 1),
                ct: CancellationToken.None);
        Assert.True(evaluated.IsSuccess, evaluated.ErrorMessage);
        using var bridgeLease = evaluated.Value!;
        var publication = await DesignConversionDpoPromotionArtifactContractCodec
            .PublishEvaluatedAsync(
                paths.Value,
                bridgeLease.Result,
                bridgeLease.Evaluation,
                CancellationToken.None);
        Assert.True(publication.IsSuccess, publication.ErrorMessage);
        Assert.True(DesignConversionDpoPromotionArtifactContractCodec
            .ValidateEvaluatedArtifactPublication(publication.Value));
        bridgeLease.CommitPublishedArtifacts();
        var canonicalAcquisition = DesignConversionDpoPromotionArtifactContractCodec
            .AcquireValidatedEvaluatedArtifactPublication(publication.Value);
        Assert.True(canonicalAcquisition.IsSuccess, canonicalAcquisition.ErrorMessage);
        using (var canonicalLease = canonicalAcquisition.Value!)
            Assert.True(canonicalLease.Revalidate());

        var artifacts = publication.Value!.ManifestArtifacts;
        var context = bridgeLease.Result.PolicyCheckpointRef!.TrainingContext;
        var policyCheckpointPath = Path.Combine(
            paths.Value.OutputDirectory,
            bridgeLease.Result.PolicyCheckpointPath);
        var policyCheckpointBytes = await File.ReadAllBytesAsync(
            policyCheckpointPath);
        var rawPayloadMarker = Convert.ToBase64String(
            policyCheckpointBytes.AsSpan(0, Math.Min(32, policyCheckpointBytes.Length)));

        return new EvaluatedDpoArtifactFixture(
            RuntimeCheckpointPath: Path.GetFullPath(artifacts.CheckpointPath),
            RuntimeManifestRef: bridgeLease.Result.RuntimeExport.ManifestRef,
            SourceEncoderPath: Path.GetFullPath(artifacts.SourceEncoderPath!),
            PolicyCheckpointPath: Path.GetFullPath(policyCheckpointPath),
            PolicyCheckpointSha256: bridgeLease.Result.PolicyCheckpointSha256,
            Context: context,
            Artifacts: artifacts,
            Publication: publication.Value,
            RawPolicyPayloadMarker: rawPayloadMarker);
    }

    private static DesignConversionDpoResolvedPartition
        BuildEvaluatedPartitionForRepositoryUploaderTests(string id)
    {
        var train = WithSourceIdentity(
            BuildTrainingRecord(
                $"{id}-train",
                DesignConversionDpoUnits.ImproverUnit,
                usefulness: 0.99),
            $"{id}-source-train");
        var heldOut = WithSourceIdentity(
            BuildTrainingRecord(
                $"{id}-heldout",
                DesignConversionDpoUnits.CriticUnit,
                usefulness: 0.98),
            $"{id}-source-heldout");
        var provisional = new DesignConversionDpoResolvedPartition(
            id,
            [train],
            [heldOut],
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty);
        Assert.True(
            DesignConversionDpoConsumedPartitionProjectionContract.TryCreate(
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

    private static DesignConversionDpoTrainingRecord BuildTrainingRecord(
        string id,
        string unitId,
        double usefulness)
    {
        var build = new DesignConversionDpoGenerator().BuildTrainReadyRecords(
        [
            DesignConversionDpoGeneratorTests.CreateExample(
                id,
                unitId,
                usefulness)
        ]);
        Assert.True(build.IsSuccess, build.ErrorMessage);
        Assert.Empty(build.Value!.QuarantinedRecords);
        return Assert.Single(build.Value.AcceptedRecords);
    }

    private static DesignConversionDpoTrainingRecord WithSourceIdentity(
        DesignConversionDpoTrainingRecord record,
        string sourceId)
    {
        var metadata = new Dictionary<string, string>(
            record.Provenance.Metadata,
            StringComparer.Ordinal)
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
            }
        };
    }

    private static DesignConversionPromotionGateReport BuildEvaluatedGateReport(
        DesignConversionPromotionGateReport template,
        EvaluatedDpoArtifactFixture evaluated)
    {
        var paths = new[]
        {
            evaluated.Artifacts.CheckpointPath,
            evaluated.Artifacts.TokenizerPath,
            evaluated.Artifacts.SourceEncoderPath!,
            evaluated.Artifacts.PolicyPath,
            evaluated.PolicyCheckpointPath
        };
        return template with
        {
            RequiredArtifactPaths = paths,
            PresentArtifactPaths = paths
        };
    }

    private static async Task<DesignConversionTrainingManifestArtifacts>
        BuildWrongRuntimeIdentityArtifactsAsync(EvaluatedDpoArtifactFixture evaluated)
    {
        var artifacts = evaluated.Artifacts;
        var sourceContract = evaluated.Publication.SourceEncoderContract;
        var policyContract = evaluated.Publication.PolicyCheckpointContract;
        var original = sourceContract.Binding;
        var wrongRuntimeSha256 = Sha256Hex(
            Utf8NoBom.GetBytes("self-consistent-wrong-runtime-checkpoint-identity"));
        Assert.NotEqual(original.RuntimeCheckpointSha256, wrongRuntimeSha256);

        var wrongBinding = original with
        {
            RuntimeCheckpointSha256 = wrongRuntimeSha256
        };
        var wrongArtifactSetId = DesignConversionDpoPromotionArtifactContractCodec.BuildArtifactSetId(
            wrongBinding.PolicyCheckpointFileName,
            wrongBinding.PolicyCheckpointSha256,
            wrongBinding.PolicyCheckpointByteLength,
            wrongBinding.RuntimeCheckpointFileName,
            wrongBinding.RuntimeCheckpointSha256,
            wrongBinding.RuntimeCheckpointByteLength,
            wrongBinding.TokenizerArtifactFileName,
            wrongBinding.TokenizerArtifactSha256,
            wrongBinding.TokenizerArtifactByteLength,
            wrongBinding.SourceEncoderDescriptor,
            policyContract.Evaluation);
        wrongBinding = wrongBinding with
        {
            ArtifactSetId = wrongArtifactSetId
        };

        var wrongSourceContract = sourceContract with { Binding = wrongBinding };
        var wrongPolicyContract = policyContract with { Binding = wrongBinding };
        var sourceBytes = JsonSerializer.SerializeToUtf8Bytes(
            wrongSourceContract,
            DesignConversionCheckpointJsonOptions.Standard);
        var policyContractBytes = JsonSerializer.SerializeToUtf8Bytes(
            wrongPolicyContract,
            DesignConversionCheckpointJsonOptions.Standard);
        await File.WriteAllBytesAsync(artifacts.SourceEncoderPath!, sourceBytes);
        await File.WriteAllBytesAsync(artifacts.PolicyPath, policyContractBytes);

        return artifacts with
        {
            RuntimeCheckpointArtifact = artifacts.RuntimeCheckpointArtifact! with
            {
                Sha256 = wrongRuntimeSha256,
                ArtifactSetId = wrongArtifactSetId,
                RuntimeCheckpointSha256 = wrongRuntimeSha256
            },
            TokenizerArtifact = artifacts.TokenizerArtifact! with
            {
                ArtifactSetId = wrongArtifactSetId,
                RuntimeCheckpointSha256 = wrongRuntimeSha256
            },
            SourceEncoderContractArtifact = artifacts.SourceEncoderContractArtifact! with
            {
                Sha256 = Sha256Hex(sourceBytes),
                ByteLength = sourceBytes.LongLength,
                ArtifactSetId = wrongArtifactSetId,
                RuntimeCheckpointSha256 = wrongRuntimeSha256
            },
            PolicyCheckpointContractArtifact = artifacts.PolicyCheckpointContractArtifact! with
            {
                Sha256 = Sha256Hex(policyContractBytes),
                ByteLength = policyContractBytes.LongLength,
                ArtifactSetId = wrongArtifactSetId,
                RuntimeCheckpointSha256 = wrongRuntimeSha256
            },
            PolicyCheckpointArtifact = artifacts.PolicyCheckpointArtifact! with
            {
                ArtifactSetId = wrongArtifactSetId,
                RuntimeCheckpointSha256 = wrongRuntimeSha256
            }
        };
    }

    private static IReadOnlyList<string> BuildCanonicalPolicyEvidenceRefs(
        DesignConversionDpoPolicyBaseIdentity identity,
        string configSha256,
        string initialWeightSha256,
        DesignConversionDpoPolicyCheckpointLoraContext lora,
        string policyCheckpointFileName,
        string policyCheckpointSha256)
        =>
        [
            DesignConversionMachine.DpoTrainerPolicyCheckpointFreshLoadedEvidence,
            DesignConversionMachine.DpoTrainerPolicyCheckpointConvertedEvidence,
            "DESIGN_CONVERSION_DPO_TRAINER_POLICY_CHECKPOINT_SOURCE_11_DPO",
            "DESIGN_CONVERSION_DPO_POLICY_BRIDGE_PRODUCT_READY_FALSE",
            DesignConversionMachine.DpoPolicyDirectInferenceConfiguredEvidence,
            "DESIGN_CONVERSION_DPO_POLICY_BRIDGE_PAYLOAD_FILE_PERSISTED",
            "DESIGN_CONVERSION_DPO_POLICY_BRIDGE_SOURCE_ENCODER_SIGNAL",
            "DESIGN_CONVERSION_DPO_POLICY_BASE_PROVIDER_INJECTED",
            "DESIGN_CONVERSION_DPO_POLICY_BASE_IDENTITY_VALIDATED",
            "DESIGN_CONVERSION_DPO_POLICY_BASE_MODELS_DISTINCT",
            "DESIGN_CONVERSION_DPO_POLICY_INITIAL_POLICY_REFERENCE_BITWISE_EQUAL",
            "DESIGN_CONVERSION_DPO_POLICY_REFERENCE_FROZEN_AND_UNCHANGED",
            "DESIGN_CONVERSION_DPO_POLICY_BASE_AND_INITIAL_WEIGHT_HASHES_DISTINCT",
            "11.DPO:DpoTrainer",
            "11.DPO:DpoTrainer.StepVector",
            $"dpo_policy_base_provider_kind:{identity.ProviderKind}",
            $"dpo_policy_base_configuration_kind:{identity.ConfigurationKind}",
            $"dpo_policy_base_model_id:{identity.BaseModelId}",
            $"dpo_policy_base_revision:{identity.BaseRevision}",
            $"dpo_policy_base_checkpoint_sha256:{identity.BaseCheckpointSha256}",
            $"dpo_policy_config_sha256:{configSha256}",
            $"dpo_policy_initial_weight_sha256:{initialWeightSha256}",
            $"dpo_policy_checkpoint_sha256:{policyCheckpointSha256}",
            $"dpo_policy_checkpoint_file:{policyCheckpointFileName}",
            "dpo_policy_bridge_source_record_count:2",
            "dpo_policy_bridge_preference_count:1",
            "dpo_policy_step_count:1",
            DesignConversionMachine.DpoPolicyLoraAppliedEvidence,
            "10.LoRA:LoraApplicator",
            $"dpo_policy_lora_rank:{lora.Rank}",
            $"dpo_policy_lora_alpha:{lora.Alpha.ToString("0.######", System.Globalization.CultureInfo.InvariantCulture)}",
            $"dpo_policy_lora_dropout:{lora.Dropout.ToString("0.######", System.Globalization.CultureInfo.InvariantCulture)}",
            $"dpo_policy_lora_target_modules:{string.Join(",", lora.TargetModules)}",
            $"dpo_policy_lora_adapter_count:{lora.AdapterCount}",
            "DESIGN_CONVERSION_DPO_POLICY_CHECKPOINT_REF_TYPED",
            "DESIGN_CONVERSION_DPO_POLICY_CHECKPOINT_REF_NO_ABSOLUTE_PATH",
            "DESIGN_CONVERSION_DPO_POLICY_CHECKPOINT_REF_PAYLOAD_FILE_SIBLING"
        ];

    private static DesignConversionDpoHeldOutEvaluation BuildCanonicalEvaluation(
        string name,
        string policyCheckpointSha256,
        string referenceCheckpointSha256,
        TransformerModel.Config.ModelConfig config)
    {
        string Hash(string value) => Sha256Hex(Utf8NoBom.GetBytes(value));
        var evaluation = new DesignConversionDpoHeldOutEvaluation(
            Schema: DesignConversionDpoPolicyCheckpointBridge.EvaluatedSchema,
            PackingId: DesignConversionDpoPolicyCheckpointBridge.EvaluatedPackingId,
            VocabSize: config.VocabSize,
            ContextLength: config.ContextLen,
            DpoStepCount: 1,
            MaxBridgePreferenceCount: 1,
            ChildPolicyCheckpointSha256: policyCheckpointSha256,
            ReferencePolicyCheckpointSha256: referenceCheckpointSha256,
            ReferenceMode: "provider_fresh_step_zero",
            DatasetId: name,
            PartitionSnapshotSha256: Hash($"{name}:partition"),
            TrainRecordSetSha256: Hash($"{name}:train-records"),
            HeldOutRecordSetSha256: Hash($"{name}:heldout-records"),
            TrainSourceSetSha256: Hash($"{name}:train-sources"),
            HeldOutSourceSetSha256: Hash($"{name}:heldout-sources"),
            SelectedTrainInputSha256: Hash($"{name}:train-input"),
            HeldOutEvaluationInputSha256: Hash($"{name}:heldout-input"),
            TrainRecordCount: 1,
            HeldOutRecordCount: 1,
            EvaluatedHeldOutRecordCount: 1,
            CorrectCount: 1,
            Accuracy: 1d,
            MeanRelativeMargin: 1d,
            MeanChildMargin: 2d,
            MeanReferenceMargin: 1d,
            FreshLoadVerified: true,
            ReferenceFrozen: true,
            SourceDisjoint: true,
            HeldOutNotTrained: true)
        {
            G3Metrics = new DesignConversionG3AcceptanceMetrics(
                HeldOutCount: 1,
                BaselineWinCount: 1,
                BaselineWinRate: 1d,
                AbsoluteChildPreferenceCorrectCount: 1,
                AbsoluteChildPreferenceAccuracy: 1d)
            {
                MeasurementNoiseScorerIdentity =
                    DesignConversionDpoPolicyCheckpointBridge
                        .EvaluatedMeasurementNoiseScorerIdentity,
                MeasurementNoiseEvaluationSetId = name
            }
        };
        return evaluation with
        {
            CycleAccuracyLineage = new DesignConversionDpoHeldOutCycleAccuracyLineage(
                CycleOrdinal: 1,
                CurrentMeasuredAccuracy: evaluation.Accuracy,
                HasParentMeasurement: false,
                ParentCycleOrdinal: null,
                ParentMeasuredAccuracy: null,
                ParentPolicyCheckpointSha256: null,
                ParentDatasetId: null,
                ParentHeldOutRecordSetSha256: null,
                ParentHeldOutSourceSetSha256: null,
                ParentHeldOutEvaluationInputSha256: null,
                ComparableHeldOut: false,
                AccuracyDelta: null)
        };
    }

    private static DesignConversionTrainingManifestArtifactDescriptor BuildArtifactDescriptor(
        string kind,
        string path,
        string sha256,
        long byteLength,
        string artifactSetId,
        string runtimeCheckpointSha256)
    {
        var canonicalPath = Path.GetFullPath(path);
        return new DesignConversionTrainingManifestArtifactDescriptor(
            kind,
            canonicalPath,
            Path.GetFileName(canonicalPath),
            sha256,
            byteLength,
            artifactSetId,
            runtimeCheckpointSha256);
    }

    private static void DeleteDirectoryIfExists(string path)
    {
        if (!Directory.Exists(path))
            return;

        try
        {
            Directory.Delete(path, recursive: true);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    private static string UnitAlias(string unitId)
        => string.Equals(unitId, DesignConversionRuntimeScaffoldCriticUnit.UnitId, StringComparison.Ordinal)
            ? "critic"
            : string.Equals(unitId, DesignConversionRuntimeScaffoldImproverUnit.UnitId, StringComparison.Ordinal)
                ? "improver"
                : unitId;

    private sealed record PromotionFixture(
        string TempRoot,
        string CheckpointPath,
        string TokenizerPath,
        string PolicyPath,
        string CheckpointSha256,
        string ExpectedSourceDatasetSha256,
        DesignConversionTrainingManifest Manifest,
        DesignConversionPromotionGateReport GateReport);

    private sealed record EvaluatedDpoArtifactFixture(
        string RuntimeCheckpointPath,
        string RuntimeManifestRef,
        string SourceEncoderPath,
        string PolicyCheckpointPath,
        string PolicyCheckpointSha256,
        DesignConversionDpoPolicyCheckpointTrainingContext Context,
        DesignConversionTrainingManifestArtifacts Artifacts,
        DesignConversionDpoPromotionArtifactPublication Publication,
        string RawPolicyPayloadMarker);

    private sealed class RecordingModelRepository(
        List<string> calls,
        char identityCharacter = 'a') : IModelRepository
    {
        public OperationResult<string> PreflightStableConfigurationIdentitySha256()
            => OperationResult<string>.Ok(
                "sha256:" + new string(identityCharacter, 64));

        public List<GeneratedCapabilityCheckpointManifest> UploadedManifests { get; } = [];
        public List<string> LoadFallbackPaths { get; } = [];
        public string? FailUploadAlias { get; init; }
        public string? HashMismatchLoadAlias { get; init; }
        public string? ImproverEndpointOverride { get; init; }
        public Func<RemoteUnitManifest, RemoteUnitManifest>? UploadResponseMutation { get; init; }

        public Task<OperationResult<Stream>> LoadWeightsAsync(
            string unitId,
            string? serverBaseUrl,
            string localFallbackPath,
            CancellationToken ct = default)
        {
            var alias = UnitAlias(unitId);
            calls.Add($"load:{alias}");
            LoadFallbackPaths.Add(Path.GetFullPath(localFallbackPath));
            if (ct.IsCancellationRequested)
                return Task.FromResult(OperationResult<Stream>.Fail("cancelled", "CANCELLED"));

            if (string.Equals(HashMismatchLoadAlias, alias, StringComparison.Ordinal))
            {
                Stream mismatch = new MemoryStream(Utf8NoBom.GetBytes("mismatched checkpoint bytes"));
                return Task.FromResult(OperationResult<Stream>.Ok(mismatch));
            }

            Stream stream = new MemoryStream(File.ReadAllBytes(localFallbackPath));
            return Task.FromResult(OperationResult<Stream>.Ok(stream));
        }

        public Task<OperationResult<RemoteUnitManifest>> UploadAsync(
            GeneratedCapabilityCheckpointManifest manifest,
            string serverBaseUrl,
            CancellationToken ct = default)
        {
            var alias = UnitAlias(manifest.UnitId);
            calls.Add($"upload:{alias}");
            if (ct.IsCancellationRequested)
                return Task.FromResult(OperationResult<RemoteUnitManifest>.Fail("cancelled", "CANCELLED"));

            if (string.Equals(FailUploadAlias, alias, StringComparison.Ordinal))
            {
                return Task.FromResult(OperationResult<RemoteUnitManifest>.Fail(
                    "remote upload failed without private details",
                    "REMOTE_UPLOAD_FAILED_SAFE"));
            }

            UploadedManifests.Add(manifest);
            var unitEndpoint = string.Equals(alias, "improver", StringComparison.Ordinal) &&
                               ImproverEndpointOverride is not null
                ? ImproverEndpointOverride
                : $"/models/{Uri.EscapeDataString(manifest.UnitId)}";
            var remote = new RemoteUnitManifest(
                UnitId: manifest.UnitId,
                UnitRole: manifest.UnitRole,
                DomainTag: manifest.DomainTag,
                Version: manifest.Version,
                ServerBaseUrl: serverBaseUrl.TrimEnd('/'),
                UnitEndpoint: unitEndpoint,
                ManifestSha256: Sha256Hex(File.ReadAllBytes(manifest.PolicyCheckpointPath)).ToUpperInvariant(),
                TrainingBranch: manifest.TrainingBranch ?? string.Empty,
                TrainingRunId: manifest.TrainingRunId ?? string.Empty,
                SourceDatasetSha256: manifest.SourceDatasetSha256 ?? string.Empty,
                ModelConfig: new RemoteModelConfig(
                    manifest.VocabSize,
                    manifest.ContextLen,
                    manifest.HiddenDim,
                    manifest.NumLayers,
                    manifest.NumHeads,
                    manifest.NumKvHeads,
                    manifest.FfnHiddenDim,
                    manifest.RopeTheta,
                    manifest.RmsNormEps),
                LoraConfig: new RemoteLoraConfig(
                    manifest.LoraRank,
                    manifest.LoraAlpha,
                    manifest.LoraDropout,
                    manifest.LoraTargetModules),
                TokenizerPath: manifest.TokenizerPath,
                RegisteredAtUtc: FixedRemoteTime);
            remote = UploadResponseMutation?.Invoke(remote) ?? remote;

            return Task.FromResult(OperationResult<RemoteUnitManifest>.Ok(remote));
        }
    }

    private sealed class RecordingRemoteUnitLocator(
        List<string> calls,
        char identityCharacter = 'b') : IRemoteUnitLocator
    {
        public OperationResult<string> PreflightStableConfigurationIdentitySha256()
            => OperationResult<string>.Ok(
                "sha256:" + new string(identityCharacter, 64));

        private readonly Dictionary<string, RemoteUnitManifest> _registered = new(StringComparer.Ordinal);

        public IReadOnlyList<RemoteUnitManifest> Registered => _registered.Values.ToArray();
        public string? FailRegisterAlias { get; init; }
        public Func<RemoteUnitManifest, RemoteUnitManifest>? LocateResponseMutation { get; init; }

        public Task<OperationResult<RemoteUnitManifest>> LocateAsync(
            string unitId,
            CancellationToken ct = default)
        {
            var alias = UnitAlias(unitId);
            calls.Add($"locate:{alias}");
            if (ct.IsCancellationRequested)
                return Task.FromResult(OperationResult<RemoteUnitManifest>.Fail("cancelled", "CANCELLED"));

            if (!_registered.TryGetValue(unitId, out var manifest))
            {
                return Task.FromResult(OperationResult<RemoteUnitManifest>.Fail(
                    "not found",
                    "REMOTE_UNIT_NOT_FOUND"));
            }

            var located = LocateResponseMutation?.Invoke(manifest) ?? manifest;
            return Task.FromResult(OperationResult<RemoteUnitManifest>.Ok(located));
        }

        public Task<OperationResult<bool>> RegisterAsync(
            RemoteUnitManifest manifest,
            CancellationToken ct = default)
        {
            var alias = UnitAlias(manifest.UnitId);
            calls.Add($"register:{alias}");
            if (ct.IsCancellationRequested)
                return Task.FromResult(OperationResult<bool>.Fail("cancelled", "CANCELLED"));

            if (string.Equals(FailRegisterAlias, alias, StringComparison.Ordinal))
            {
                return Task.FromResult(OperationResult<bool>.Fail(
                    "remote register failed without rollback support",
                    "REMOTE_REGISTER_FAILED_SAFE"));
            }

            _registered[manifest.UnitId] = manifest;
            return Task.FromResult(OperationResult<bool>.Ok(true));
        }
    }
}
