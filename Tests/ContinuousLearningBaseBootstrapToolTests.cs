// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Collections;
using System.Diagnostics;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ContinuousLearningBaseBootstrap;
using ContinuousLearning;
using Microsoft.Extensions.DependencyInjection;
using ModelServer.DesignConversion;
using Tokenizer;
using TransformerModel;
using TransformerModel.Config;
using ModelServerProgram = ModelServer.Program;
using ModelServerReadinessEndpoint = ModelServer.ContinuousLearningReadinessEndpoint;

namespace LlmFromScratch.Tests;

[Collection(global::ModelServerContinuousLearningEnvironmentCollection.Name)]
public sealed class ContinuousLearningBaseBootstrapToolTests
{
    private const string ValidAuditSalt = "YXVkaXQtc2FsdC1zZWNyZXQtMDEyMzQ1Njc4OUFCQ0Q=";
    private const string ValidLifecycleToken = "life-current-distinctive-token-123456";
    private const string ValidPreviousLifecycleToken = "life-previous-distinctive-token-123456";
    private const string ValidRouteAdmissionToken = "route-current-distinctive-token-123456";
    private const string ValidPreviousRouteAdmissionToken = "route-previous-distinctive-token-123456";
    private const string ValidPreviousExpiryInput = "2036-01-02T03:04:05Z";
    private const string ValidPreviousExpiryCanonical = "2036-01-02T03:04:05.0000000+00:00";
    private const string ValidRouteAdmissionTenantId = "tenant-tp1-live-local";

    private static readonly ModelConfig SmokeConfig = new()
    {
        VocabSize = 264,
        ContextLen = 16,
        HiddenDim = 8,
        NumLayers = 1,
        NumHeads = 2,
        NumKvHeads = 1,
        FfnHiddenDim = 16,
    };

    [Fact]
    public async Task HappyPath_ProducesValidatedColdArtifactsAndTruthfulManifest()
    {
        var root = CreateTempRoot();
        try
        {
            var corpusPath = WriteCorpus(root);
            var outputRoot = Path.Combine(root, "out");

            var result = await RunAsync(BuildValidArgs(outputRoot, corpusPath));

            Assert.Equal(0, result.ExitCode);
            Assert.Equal(string.Empty, result.Stderr.Trim());

            using var success = JsonDocument.Parse(result.Stdout);
            var successRoot = success.RootElement;
            Assert.True(successRoot.GetProperty("ok").GetBoolean());
            var destination = RequiredString(successRoot, "destination");
            var checkpointPath = RequiredString(successRoot, "checkpoint_path");
            var tokenizerPath = RequiredString(successRoot, "tokenizer_path");
            var corpusCopyPath = RequiredString(successRoot, "corpus_path");
            var manifestPath = RequiredString(successRoot, "manifest_path");

            Assert.Equal(
                Path.GetFullPath(Path.Combine(outputRoot, "smoke-model", "rev-001")),
                Path.GetFullPath(destination));
            Assert.True(Directory.Exists(destination));
            Assert.Equal(
                new[] { "checkpoint.bin", "manifest.json", "tokenizer-corpus.txt", "tokenizer.json" },
                Directory.EnumerateFiles(destination).Select(Path.GetFileName).OrderBy(static name => name, StringComparer.Ordinal).ToArray());
            Assert.Empty(Directory.EnumerateDirectories(outputRoot, "*bootstrap-tmp*", SearchOption.AllDirectories));

            LlmModel.ValidateFullCheckpointArtifact(checkpointPath, SmokeConfig);
            await using (var tokenizerStream = File.OpenRead(tokenizerPath))
            {
                var tokenizer = BpeTokenizer.ValidateAndLoadCanonicalArtifact(tokenizerStream, SmokeConfig.VocabSize);
                Assert.Equal(SmokeConfig.VocabSize, tokenizer.VocabSize);
            }

            Assert.Equal(Sha256(checkpointPath), RequiredString(successRoot, "checkpoint_sha256"));
            Assert.Equal(Sha256(tokenizerPath), RequiredString(successRoot, "tokenizer_sha256"));
            Assert.Equal(Sha256(corpusCopyPath), RequiredString(successRoot, "corpus_sha256"));

            using var manifest = JsonDocument.Parse(File.ReadAllText(manifestPath));
            var manifestRoot = manifest.RootElement;
            Assert.Equal("cold_internal_llm_v2_full_checkpoint", RequiredString(manifestRoot, "artifact_kind"));
            Assert.False(manifestRoot.GetProperty("is_trained").GetBoolean());
            Assert.False(manifestRoot.GetProperty("semantic_quality_claimed").GetBoolean());
            Assert.False(manifestRoot.GetProperty("production_behavior_ready").GetBoolean());
            Assert.Equal("smoke-model", RequiredString(manifestRoot, "model_id"));
            Assert.Equal("rev-001", RequiredString(manifestRoot, "revision"));
            Assert.Equal("bootstrap-smoke", RequiredString(manifestRoot, "profile"));
            Assert.Equal(SmokeConfig.VocabSize, manifestRoot.GetProperty("model_config").GetProperty("vocab_size").GetInt32());
            Assert.Equal(SmokeConfig.HiddenDim, manifestRoot.GetProperty("model_config").GetProperty("hidden_dim").GetInt32());
            Assert.Equal(Sha256(checkpointPath), RequiredString(manifestRoot.GetProperty("checkpoint"), "sha256"));
            Assert.Equal(Sha256(tokenizerPath), RequiredString(manifestRoot.GetProperty("tokenizer"), "sha256"));
            Assert.Equal(Sha256(corpusCopyPath), RequiredString(manifestRoot.GetProperty("tokenizer_corpus"), "sha256"));
            Assert.Equal(new FileInfo(checkpointPath).Length, manifestRoot.GetProperty("checkpoint").GetProperty("bytes").GetInt64());
            Assert.Equal(new FileInfo(tokenizerPath).Length, manifestRoot.GetProperty("tokenizer").GetProperty("bytes").GetInt64());
            Assert.Equal(new FileInfo(corpusCopyPath).Length, manifestRoot.GetProperty("tokenizer_corpus").GetProperty("bytes").GetInt64());
            AssertManifestMetadataResolvesToFile(manifestRoot, "checkpoint", checkpointPath);
            AssertManifestMetadataResolvesToFile(manifestRoot, "tokenizer", tokenizerPath);
            AssertManifestMetadataResolvesToFile(manifestRoot, "tokenizer_corpus", corpusCopyPath);
            Assert.False(string.IsNullOrWhiteSpace(RequiredString(manifestRoot, "source_commit")));
            Assert.False(string.IsNullOrWhiteSpace(RequiredString(manifestRoot.GetProperty("runtime"), "framework_description")));
        }
        finally
        {
            DeleteIfExists(root);
        }
    }

    [Fact]
    public async Task Tp1LiveLocalBundle_CreatesLoaderConsumablePreTrainingArtifactsWithoutPublicSecretLeak()
    {
        var root = CreateTempRoot();
        try
        {
            var corpusPath = WriteCorpus(root);
            var outputRoot = Path.Combine(root, "out");
            var capturePath = Path.Combine(root, "fake-server-capture.json");
            var serverCommandPath = WriteFakeServerCaptureCommand(root, capturePath);

            var result = await RunAsync(BuildValidTp1Args(
                outputRoot,
                corpusPath,
                serverCommandPath));

            Assert.Equal(0, result.ExitCode);
            Assert.Equal(string.Empty, result.Stderr.Trim());

            using var success = JsonDocument.Parse(result.Stdout);
            var successRoot = success.RootElement;
            Assert.True(successRoot.GetProperty("ok").GetBoolean());
            Assert.True(successRoot.GetProperty("tp1_live_local_bundle").GetBoolean());
            var destination = RequiredString(successRoot, "destination");
            var manifestPath = RequiredString(successRoot, "manifest_path");
            var bundleManifestPath = RequiredString(successRoot, "tp1_bundle_manifest_path");
            var environmentJsonPath = RequiredString(successRoot, "tp1_environment_json_path");
            var environmentPowerShellPath = RequiredString(successRoot, "tp1_environment_powershell_path");
            var measurementGateThresholdsPath = RequiredString(successRoot, "measurement_gate_thresholds_path");
            var artifactDbRoot = RequiredString(successRoot, "artifact_db_root");
            var readyFilePath = RequiredString(successRoot, "ready_file_path");
            var repositoryRoot = RequiredString(successRoot, "repository_root");
            var dpoPairRoot = RequiredString(successRoot, "dpo_pair_root");
            var roots = new[]
            {
                RequiredString(successRoot, "adapter_root"),
                RequiredString(successRoot, "cache_root"),
                RequiredString(successRoot, "registry_root"),
            };

            Assert.True(File.Exists(Path.Combine(destination, "checkpoint.bin")));
            Assert.True(File.Exists(Path.Combine(destination, "tokenizer.json")));
            Assert.True(File.Exists(Path.Combine(destination, "tokenizer-corpus.txt")));
            Assert.True(File.Exists(manifestPath));
            Assert.True(File.Exists(bundleManifestPath));
            Assert.True(File.Exists(environmentJsonPath));
            Assert.True(File.Exists(environmentPowerShellPath));
            Assert.True(File.Exists(measurementGateThresholdsPath));
            AssertInside(destination, bundleManifestPath);
            AssertInside(destination, environmentJsonPath);
            AssertInside(destination, environmentPowerShellPath);
            AssertInside(destination, measurementGateThresholdsPath);
            AssertOutsideProjects(destination);

            foreach (var generatedRoot in roots)
            {
                Assert.True(Directory.Exists(generatedRoot));
                AssertInside(destination, generatedRoot);
                AssertOutsideProjects(generatedRoot);
                Assert.Empty(Directory.EnumerateFileSystemEntries(generatedRoot));
            }
            Assert.True(Directory.Exists(dpoPairRoot));
            AssertInside(repositoryRoot, dpoPairRoot);
            AssertOutsideProjects(dpoPairRoot);
            Assert.EndsWith(
                Path.Combine(
                    "training-data",
                    "dpo",
                    "html-css",
                    "learning-unit",
                    "v0001",
                    "pairs"),
                dpoPairRoot,
                OperatingSystem.IsWindows()
                    ? StringComparison.OrdinalIgnoreCase
                    : StringComparison.Ordinal);
            Assert.Empty(Directory.EnumerateFileSystemEntries(dpoPairRoot));
            Assert.DoesNotContain(
                $"{Path.DirectorySeparatorChar}server-owned{Path.DirectorySeparatorChar}dpo-pairs",
                dpoPairRoot,
                StringComparison.OrdinalIgnoreCase);
            Assert.True(Directory.Exists(artifactDbRoot));
            Assert.Empty(Directory.EnumerateFileSystemEntries(artifactDbRoot));
            AssertInside(destination, artifactDbRoot);
            AssertOutsideProjects(artifactDbRoot);
            AssertInside(destination, readyFilePath);
            AssertOutsideProjects(readyFilePath);
            Assert.True(Directory.Exists(Path.GetDirectoryName(readyFilePath)));
            Assert.False(File.Exists(readyFilePath));

            var thresholds =
                DesignConversionModelServerMeasurementGateThresholdConfiguration.LoadFromFile(
                    measurementGateThresholdsPath);
            Assert.True(thresholds.IsSuccess, thresholds.ErrorMessage);

            var environment = ReadLaunchEnvironment(environmentJsonPath);
            Assert.Equal(ValidLifecycleToken, environment["DESIGN_CONVERSION_LIVE_MODEL_LIFECYCLE_CURRENT_TOKEN"]);
            Assert.Equal(ValidPreviousLifecycleToken, environment["DESIGN_CONVERSION_LIVE_MODEL_LIFECYCLE_PREVIOUS_TOKEN"]);
            Assert.Equal(ValidPreviousExpiryCanonical, environment["DESIGN_CONVERSION_LIVE_MODEL_LIFECYCLE_PREVIOUS_TOKEN_EXPIRES_AT_UTC"]);
            Assert.Equal(bool.TrueString, environment["DESIGN_CONVERSION_LIVE_ROUTE_ADMISSION_ENABLED"]);
            Assert.Equal(ValidRouteAdmissionToken, environment["DESIGN_CONVERSION_LIVE_ROUTE_CURRENT_TOKEN"]);
            Assert.Equal(ValidPreviousRouteAdmissionToken, environment["DESIGN_CONVERSION_LIVE_ROUTE_PREVIOUS_TOKEN"]);
            Assert.Equal(ValidPreviousExpiryCanonical, environment["DESIGN_CONVERSION_LIVE_ROUTE_PREVIOUS_TOKEN_EXPIRES_AT_UTC"]);
            Assert.Equal(ValidRouteAdmissionTenantId, environment["DESIGN_CONVERSION_LIVE_ROUTE_TENANT_ID"]);
            Assert.Equal(repositoryRoot, environment["XIIGEN_CONTINUOUS_LEARNING_REPOSITORY_ROOT"]);
            Assert.Equal(dpoPairRoot, environment["XIIGEN_CONTINUOUS_LEARNING_DPO_PAIR_ROOT"]);
            var beforeLoader = SnapshotTree(destination);
            var resolution = ContinuousLearningModelServerConfigurationLoader.Resolve(
                name => environment.GetValueOrDefault(name));

            Assert.Equal(
                ContinuousLearningConfigurationState.Enabled,
                resolution.State);
            Assert.NotNull(resolution.DeferredPlan);
            Assert.Equal(string.Empty, resolution.DeferredPlan!.TrainingConfig.CheckpointDir);
            Assert.Equal(0, resolution.DeferredPlan.TeacherConfiguration.MaxTeacherCalls);
            Assert.Equal(1, resolution.DeferredPlan.TeacherConfiguration.DpoMaxStepCount);
            Assert.Equal(
                new string('d', 64),
                resolution.DeferredPlan.TeacherConfiguration.ExpectedOllamaManifestDigest);
            Assert.Equal(
                new string('9', 64),
                resolution.DeferredPlan.TeacherConfiguration.ExpectedOllamaModelLayerDigest);
            Assert.Equal(
                new string('9', 64),
                environment["XIIGEN_CONTINUOUS_LEARNING_TP1_OLLAMA_MODEL_LAYER_DIGEST"]);

            AssertLoaderState(
                WithEnvironment(environment, "XIIGEN_CONTINUOUS_LEARNING_DPO_MAX_STEPS", "5"),
                ContinuousLearningConfigurationState.Enabled);
            AssertLoaderState(
                WithEnvironment(environment, "XIIGEN_CONTINUOUS_LEARNING_DPO_MAX_STEPS", "6"),
                ContinuousLearningConfigurationState.Invalid);
            AssertLoaderState(
                WithEnvironment(
                    environment,
                    "XIIGEN_CONTINUOUS_LEARNING_TP1_MODEL_ID",
                    BuildCanonicalTp1ModelId(256)),
                ContinuousLearningConfigurationState.Enabled);
            AssertLoaderState(
                WithEnvironment(
                    environment,
                    "XIIGEN_CONTINUOUS_LEARNING_TP1_MODEL_ID",
                    BuildCanonicalTp1ModelId(257)),
                ContinuousLearningConfigurationState.Invalid);
            AssertLoaderState(
                WithEnvironment(environment, "XIIGEN_CONTINUOUS_LEARNING_MAX_TEACHER_CALLS", "10000"),
                ContinuousLearningConfigurationState.Enabled);
            AssertLoaderState(
                WithEnvironment(environment, "XIIGEN_CONTINUOUS_LEARNING_MAX_TEACHER_CALLS", "10001"),
                ContinuousLearningConfigurationState.Invalid);
            Assert.Equal(beforeLoader, SnapshotTree(destination));

            using var publicManifest = JsonDocument.Parse(File.ReadAllText(manifestPath));
            var publicSummary = publicManifest.RootElement.GetProperty("tp1_live_local_bundle");
            Assert.True(publicSummary.GetProperty("training_not_started").GetBoolean());
            Assert.Equal(0, publicSummary.GetProperty("teacher_calls_executed").GetInt32());
            Assert.Equal(0, publicSummary.GetProperty("dpo_steps_executed").GetInt32());
            Assert.True(publicSummary.GetProperty("weights_not_trained").GetBoolean());
            Assert.False(publicSummary.GetProperty("deployment_product_readiness").GetBoolean());
            Assert.Equal(
                new string('d', 64),
                RequiredString(publicSummary, "tp1_ollama_manifest_digest"));
            Assert.Equal(
                new string('9', 64),
                RequiredString(publicSummary, "tp1_ollama_model_layer_digest"));

            using var bundleManifest = JsonDocument.Parse(File.ReadAllText(bundleManifestPath));
            Assert.False(bundleManifest.RootElement.TryGetProperty("teacher_model_license", out _));
            Assert.False(bundleManifest.RootElement.TryGetProperty("deployment_claim", out _));
            Assert.Equal(
                "operator_evidence_refs_only",
                RequiredString(bundleManifest.RootElement.GetProperty("teacher_runtime"), "legal_claim_source"));
            Assert.True(bundleManifest.RootElement.GetProperty("training_not_started").GetBoolean());
            Assert.Equal(0, bundleManifest.RootElement.GetProperty("teacher_calls_executed").GetInt32());
            Assert.Equal(0, bundleManifest.RootElement.GetProperty("dpo_steps_executed").GetInt32());
            Assert.True(bundleManifest.RootElement.GetProperty("weights_not_trained").GetBoolean());
            Assert.False(bundleManifest.RootElement.GetProperty("deployment_product_readiness").GetBoolean());
            var operatorClaims = bundleManifest.RootElement.GetProperty("operator_owned_claims");
            Assert.Equal(
                new string('d', 64),
                RequiredString(operatorClaims, "tp1_ollama_manifest_digest"));
            Assert.Equal(
                new string('9', 64),
                RequiredString(operatorClaims, "tp1_ollama_model_layer_digest"));
            Assert.Contains(
                "hostile concurrent local reparse race is not proven",
                RequiredString(publicSummary, "filesystem_safety_warning"),
                StringComparison.Ordinal);

            AssertPublicTextDoesNotContainSecrets(result.Stdout);
            AssertPublicTextDoesNotContainSecrets(result.Stderr);
            AssertPublicTextDoesNotContainSecrets(File.ReadAllText(manifestPath));
            AssertPublicTextDoesNotContainSecrets(File.ReadAllText(bundleManifestPath));
            AssertPublicTextDoesNotContainSecrets(File.ReadAllText(environmentPowerShellPath));

            var privateJson = File.ReadAllText(environmentJsonPath);
            foreach (var secret in RawSecretValues())
                Assert.Equal(1, CountOccurrences(privateJson, secret));
            AssertOnlyPrivateJsonContainsSecrets(destination, environmentJsonPath);

            var launchResult = await RunGeneratedPowerShellAsync(environmentPowerShellPath);
            Assert.Equal(privateJson, File.ReadAllText(environmentJsonPath));
            Assert.True(
                launchResult.ExitCode == 0,
                "Generated TP1 PowerShell launch script must start the fake local server successfully. " +
                $"ExitCode={launchResult.ExitCode}; stdout={launchResult.Stdout}; stderr={launchResult.Stderr}");
            AssertPublicTextDoesNotContainSecrets(launchResult.Stdout);
            AssertPublicTextDoesNotContainSecrets(launchResult.Stderr);
            Assert.True(File.Exists(capturePath));

            using var capture = JsonDocument.Parse(File.ReadAllText(capturePath));
            var capturedArguments = capture.RootElement.GetProperty("arguments")
                .EnumerateArray()
                .Select(static value => value.GetString())
                .ToArray();
            Assert.Equal(7, capturedArguments.Length);
            Assert.Equal("--ready-file", capturedArguments[0]);
            AssertHostEquivalentPath(readyFilePath, capturedArguments[1], "--ready-file value");
            Assert.Equal("--artifact-db-root", capturedArguments[2]);
            AssertHostEquivalentPath(artifactDbRoot, capturedArguments[3], "--artifact-db-root value");
            Assert.Equal("--measurement-gate-thresholds-file", capturedArguments[4]);
            AssertHostEquivalentPath(
                measurementGateThresholdsPath,
                capturedArguments[5],
                "--measurement-gate-thresholds-file value");
            Assert.Equal("--machine-backed-local", capturedArguments[6]);
            var capturedEnvironment = capture.RootElement.GetProperty("environment_variables");
            foreach (var pair in environment)
            {
                Assert.True(capturedEnvironment.TryGetProperty(pair.Key, out var captured), pair.Key);
                if (IsTp1LaunchPathEnvironmentVariable(pair.Key))
                    AssertHostEquivalentPath(pair.Value, captured.GetString(), pair.Key);
                else
                    Assert.Equal(pair.Value, captured.GetString());
            }
        }
        finally
        {
            DeleteIfExists(root);
        }
    }

    [Fact(Timeout = 120000)]
    public async Task Tp1LiveLocalBundle_PrivateEnvironmentDrivesModelServerReadyFileReadinessWithoutTraining()
    {
        var root = CreateTempRoot();
        try
        {
            var corpusPath = WriteCorpus(root);
            var outputRoot = Path.Combine(root, "out");
            var capturePath = Path.Combine(root, "unused-fake-server-capture.json");
            var serverCommandPath = WriteFakeServerCaptureCommand(root, capturePath);
            var localRetentionPolicyRef = UniqueShaRef("retention");
            var outputLicenseRef = UniqueShaRef("license");
            var args = ReplaceOption(
                ReplaceOption(
                    BuildValidTp1Args(outputRoot, corpusPath, serverCommandPath),
                    "--local-retention-policy-ref",
                    localRetentionPolicyRef),
                "--output-license-ref",
                outputLicenseRef);

            var result = await RunAsync(args);

            Assert.Equal(0, result.ExitCode);
            Assert.Equal(string.Empty, result.Stderr.Trim());

            using var success = JsonDocument.Parse(result.Stdout);
            var successRoot = success.RootElement;
            var destination = RequiredString(successRoot, "destination");
            var manifestPath = RequiredString(successRoot, "manifest_path");
            var bundleManifestPath = RequiredString(successRoot, "tp1_bundle_manifest_path");
            var environmentJsonPath = RequiredString(successRoot, "tp1_environment_json_path");
            var readyFilePath = RequiredString(successRoot, "ready_file_path");
            var serverArguments = ReadLaunchServerArguments(environmentJsonPath);
            var environment = ReadLaunchEnvironment(environmentJsonPath);
            var adapterRoot = RequiredString(successRoot, "adapter_root");
            var cacheRoot = RequiredString(successRoot, "cache_root");
            var registryRoot = RequiredString(successRoot, "registry_root");
            var dpoPairRoot = RequiredString(successRoot, "dpo_pair_root");
            var inactiveRetainedRootSnapshots = new Dictionary<string, string[]>(
                StringComparer.Ordinal)
            {
                [adapterRoot] = ["F|.continuous-learning.adapter-owner.lock|0"],
                [cacheRoot] = [],
                [registryRoot] = ["F|.continuous-learning.registry-owner.lock|0"],
                [dpoPairRoot] = [],
            };

            var readyFileArgumentIndex = Array.IndexOf(
                serverArguments,
                ModelServerProgram.ReadyFileArgumentName);
            Assert.True(readyFileArgumentIndex >= 0);
            Assert.True(readyFileArgumentIndex + 1 < serverArguments.Length);
            Assert.Equal(readyFilePath, serverArguments[readyFileArgumentIndex + 1]);
            Assert.Equal(localRetentionPolicyRef, environment[
                ContinuousLearningModelServerConfigurationLoader
                    .LocalRetentionPolicyRefEnvironmentName]);
            Assert.Equal(outputLicenseRef, environment[
                ContinuousLearningModelServerConfigurationLoader
                    .OutputLicenseRefEnvironmentName]);
            AssertLoaderState(
                RemoveEnvironmentKey(
                    environment,
                    ContinuousLearningModelServerConfigurationLoader
                        .ModelIdEnvironmentName),
                ContinuousLearningConfigurationState.Invalid);

            using (new ProcessEnvironmentScope(environment))
            {
                await using var app = ModelServerProgram.CreateLocalLoopbackHost(serverArguments);
                await app.StartAsync();
                try
                {
                    using var readyFile = await WaitForReadyJsonAsync(readyFilePath);
                    using var client = new HttpClient
                    {
                        BaseAddress = ModelServerProgram.ResolveStartedLoopbackAddress(app),
                        Timeout = TimeSpan.FromSeconds(10),
                    };
                    using var response = await client.GetAsync(
                        ModelServerReadinessEndpoint.EndpointPath);
                    var readinessJson = await response.Content.ReadAsStringAsync();

                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                    using var endpointReadiness = JsonDocument.Parse(readinessJson);
                    var readyRoot = readyFile.RootElement;
                    Assert.Equal(
                        ModelServerReadinessEndpoint.EndpointPath,
                        readyRoot
                            .GetProperty("continuousLearningReadinessEndpointPath")
                            .GetString());
                    var startup = app.Services
                        .GetRequiredService<ContinuousLearningModelServerStartup>();
                    var startupSnapshot = startup.Readiness;
                    var legalTeacherAdmissionAuthority = app.Services
                        .GetRequiredService<
                            ServerOwnedLegalTrainingTeacherAdmissionAuthority>();
                    var startupReadinessJson = JsonSerializer.SerializeToElement(
                        startupSnapshot,
                        DesignConversionModelServerHttpHandler.JsonOptions);
                    var readySnapshot = readyRoot.GetProperty(
                        "continuousLearningReadiness");

                    AssertJsonDeepEquals(
                        "endpointReadiness_vs_startupReadinessJson",
                        endpointReadiness.RootElement,
                        startupReadinessJson);
                    Assert.False(readyRoot
                        .GetProperty("continuousLearningReadyFileSnapshotAuthoritative")
                        .GetBoolean());
                    Assert.True(readyRoot
                        .GetProperty("continuousLearningLiveReadinessEndpointAuthoritative")
                        .GetBoolean());
                    Assert.True(readySnapshot
                        .GetProperty("configuration_admitted")
                        .GetBoolean());
                    Assert.Equal(
                        "ready_file_snapshot_non_authoritative",
                        readySnapshot
                            .GetProperty("configuration_status")
                            .GetString());
                    Assert.Equal(
                        "none",
                        readySnapshot
                            .GetProperty("configuration_error_code")
                            .GetString());
                    Assert.Equal(
                        localRetentionPolicyRef,
                        readySnapshot
                            .GetProperty("local_retention_policy_ref")
                            .GetString());
                    Assert.Equal(
                        outputLicenseRef,
                        readySnapshot
                            .GetProperty("output_license_ref")
                            .GetString());
                    Assert.True(readySnapshot
                        .GetProperty("legal_evidence_configured")
                        .GetBoolean());
                    Assert.False(readySnapshot
                        .GetProperty("legal_evidence_verified")
                        .GetBoolean());
                    Assert.True(readySnapshot
                        .GetProperty("ollama_manifest_digest_configured")
                        .GetBoolean());
                    Assert.True(readySnapshot
                        .GetProperty("ollama_model_layer_digest_configured")
                        .GetBoolean());
                    Assert.False(readySnapshot
                        .GetProperty("composition_materialized")
                        .GetBoolean());
                    Assert.False(readySnapshot
                        .GetProperty("path_identity_verified")
                        .GetBoolean());
                    Assert.False(readySnapshot
                        .GetProperty("retained_object_identity_bound")
                        .GetBoolean());
                    Assert.False(readySnapshot
                        .GetProperty("stable_dpo_executor_configured")
                        .GetBoolean());
                    Assert.False(readySnapshot
                        .GetProperty("training_enabled")
                        .GetBoolean());
                    Assert.False(readySnapshot
                        .GetProperty("cycle_execution_available")
                        .GetBoolean());
                    Assert.False(readySnapshot
                        .GetProperty("performs_training")
                        .GetBoolean());
                    Assert.False(readySnapshot
                        .GetProperty("product_ready")
                        .GetBoolean());
                    Assert.True(startupSnapshot.ConfigurationAdmitted);
                    Assert.Equal("materialized_inactive", startupSnapshot.ConfigurationStatus);
                    Assert.Equal("none", startupSnapshot.ConfigurationErrorCode);
                    Assert.Equal(localRetentionPolicyRef, startupSnapshot.LocalRetentionPolicyRef);
                    Assert.Equal(outputLicenseRef, startupSnapshot.OutputLicenseRef);
                    Assert.True(startupSnapshot.LegalEvidenceConfigured);
                    Assert.True(startupSnapshot.LegalEvidenceVerified);
                    Assert.True(startupSnapshot.CompositionMaterialized);
                    Assert.True(startupSnapshot.PathIdentityVerified);
                    Assert.True(startupSnapshot.RetainedObjectIdentityBound);
                    Assert.True(startupSnapshot.StableDpoExecutorConfigured);
                    Assert.False(startupSnapshot.TrainingEnabled);
                    Assert.False(startupSnapshot.CycleExecutionAvailable);
                    Assert.False(startupSnapshot.PerformsTraining);
                    Assert.False(startupSnapshot.ProductReady);
                    Assert.False(
                        startup.IsLegalTrainingTeacherAdmissionActive());
                    Assert.Equal(
                        LegalTrainingTeacherAdmissionAuthority
                            .ConfigurationRequiredErrorCode,
                        legalTeacherAdmissionAuthority.ValidateBatch(
                            [TrainingTeacherProvenance.Deterministic(
                                "bootstrap-default-off")]).ErrorCode);

                    AssertPreTrainingBundleManifests(manifestPath, bundleManifestPath);
                    foreach (var retainedRoot in inactiveRetainedRootSnapshots)
                        Assert.Equal(
                            retainedRoot.Value,
                            SnapshotTree(retainedRoot.Key));
                    Assert.DoesNotContain(
                        SnapshotTree(destination),
                        entry => entry.Contains(
                            "trained-adapter",
                            StringComparison.OrdinalIgnoreCase));
                }
                finally
                {
                    await app.StopAsync();
                }

                Assert.False(File.Exists(readyFilePath));
            }
        }
        finally
        {
            DeleteIfExists(root);
        }
    }

    [Theory]
    [InlineData("missing digest", "--tp1-ollama-manifest-digest", null, "BOOTSTRAP_MISSING_ARGUMENT")]
    [InlineData("malformed digest", "--tp1-ollama-manifest-digest", "ABC", "BOOTSTRAP_INVALID_TP1_BUNDLE_ARGUMENT")]
    [InlineData("missing model layer digest", "--tp1-ollama-model-layer-digest", null, "BOOTSTRAP_MISSING_ARGUMENT")]
    [InlineData("malformed model layer digest", "--tp1-ollama-model-layer-digest", "ABC", "BOOTSTRAP_INVALID_TP1_BUNDLE_ARGUMENT")]
    [InlineData("malformed evidence", "--teacher-legal-evidence-ref", "not-a-sha-ref", "BOOTSTRAP_INVALID_TP1_BUNDLE_ARGUMENT")]
    [InlineData("short lifecycle secret", "--lifecycle-auth-token-current", "short", "BOOTSTRAP_INVALID_TP1_BUNDLE_ARGUMENT")]
    [InlineData("bad audit salt", "--audit-salt-base64", "Zm9v", "BOOTSTRAP_INVALID_TP1_BUNDLE_ARGUMENT")]
    [InlineData("random salt plus explicit salt", "--allow-random-audit-salt", "__append_flag__", "BOOTSTRAP_INVALID_TP1_BUNDLE_ARGUMENT")]
    [InlineData("dpo above production maximum", "--dpo-max-steps", "6", "BOOTSTRAP_INVALID_TP1_BUNDLE_ARGUMENT")]
    [InlineData("model id above production maximum", "--tp1-model-id", "__model_257__", "BOOTSTRAP_INVALID_TP1_BUNDLE_ARGUMENT")]
    [InlineData("closed provider model id", "--tp1-model-id", "qwen-openai:tp1", "BOOTSTRAP_INVALID_TP1_BUNDLE_ARGUMENT")]
    [InlineData("missing route tenant", "--route-admission-tenant-id", null, "BOOTSTRAP_MISSING_ARGUMENT")]
    [InlineData("noncanonical route tenant", "--route-admission-tenant-id", "Tenant_Invalid", "BOOTSTRAP_INVALID_TP1_BUNDLE_ARGUMENT")]
    [InlineData("missing live command", "--live-local-server-command", null, "BOOTSTRAP_MISSING_ARGUMENT")]
    [InlineData("unavailable live command", "--live-local-server-command", "__missing_command__", "BOOTSTRAP_INVALID_TP1_BUNDLE_ARGUMENT")]
    [InlineData("missing repository root", "--repository-root", null, "BOOTSTRAP_MISSING_ARGUMENT")]
    [InlineData("repository root without sentinels", "--repository-root", "__repo_without_sentinels__", "BOOTSTRAP_INVALID_TP1_BUNDLE_ARGUMENT")]
    [InlineData("unsafe DPO domain slug", "--dpo-domain-slug", "HtmlCss", "BOOTSTRAP_INVALID_TP1_BUNDLE_ARGUMENT")]
    [InlineData("unsafe DPO design-pattern slug", "--dpo-design-pattern-slug", "learning/unit", "BOOTSTRAP_INVALID_TP1_BUNDLE_ARGUMENT")]
    [InlineData("unsafe DPO dataset version", "--dpo-dataset-version", "v1.0", "BOOTSTRAP_INVALID_TP1_BUNDLE_ARGUMENT")]
    public async Task Tp1LiveLocalBundle_InvalidOperatorInputsFailBeforeOutputRootCreation(
        string name,
        string option,
        string? replacement,
        string expectedErrorCode)
    {
        var root = CreateTempRoot();
        try
        {
            var corpusPath = WriteCorpus(root);
            var outputRoot = Path.Combine(root, $"out-{Guid.NewGuid():N}");
            var serverCommandPath = WriteFakeServerCaptureCommand(
                root,
                Path.Combine(root, "unused-capture.json"));
            var args = BuildValidTp1Args(outputRoot, corpusPath, serverCommandPath);
            replacement = replacement switch
            {
                "__model_257__" => BuildCanonicalTp1ModelId(257),
                "__missing_command__" => Path.Combine(root, "missing-server-command.exe"),
                "__repo_without_sentinels__" => Path.Combine(root, "not-a-repository"),
                _ => replacement,
            };
            if (replacement == Path.Combine(root, "not-a-repository"))
                Directory.CreateDirectory(replacement);
            if (option == "--tp1-model-id")
            {
                Assert.False(
                    ContinuousLearningTp1DpoConfiguration.IsCanonicalOllamaModelId(replacement));
            }
            args = replacement is null
                ? RemoveOptionWithValue(args, option)
                : replacement == "__append_flag__"
                    ? [.. args, option]
                    : ReplaceOption(args, option, replacement);

            var result = await RunAsync(args);

            Assert.True(result.ExitCode != 0, name);
            AssertFailure(result, expectedErrorCode);
            AssertPublicTextDoesNotContainSecrets(result.Stdout);
            AssertPublicTextDoesNotContainSecrets(result.Stderr);
            Assert.False(Directory.Exists(outputRoot), name);
        }
        finally
        {
            DeleteIfExists(root);
        }
    }

    [Fact]
    public async Task Tp1LiveLocalBundle_OutputUnderProjectsIsRejectedBeforeOutputCreation()
    {
        var root = CreateTempRoot();
        var unsafeOutputRoot = Path.Combine(@"C:\Projects", $"tp1-bootstrap-reject-{Guid.NewGuid():N}");
        try
        {
            var corpusPath = WriteCorpus(root);
            var serverCommandPath = WriteFakeServerCaptureCommand(
                root,
                Path.Combine(root, "unused-capture.json"));

            var result = await RunAsync(BuildValidTp1Args(
                unsafeOutputRoot,
                corpusPath,
                serverCommandPath));

            Assert.NotEqual(0, result.ExitCode);
            AssertFailure(result, "BOOTSTRAP_OUTPUT_UNDER_PROJECTS");
            Assert.False(Directory.Exists(unsafeOutputRoot));
        }
        finally
        {
            DeleteIfExists(root);
            Assert.False(Directory.Exists(unsafeOutputRoot));
        }
    }

    [Fact]
    public async Task OutputUnderProjects_IsRejectedWithoutWrites()
    {
        var root = CreateTempRoot();
        var unsafeOutputRoot = Path.Combine(@"C:\Projects", $"bootstrap-tool-reject-{Guid.NewGuid():N}");
        try
        {
            var corpusPath = WriteCorpus(root);

            var result = await RunAsync(BuildValidArgs(unsafeOutputRoot, corpusPath));

            Assert.NotEqual(0, result.ExitCode);
            AssertFailure(result, "BOOTSTRAP_OUTPUT_UNDER_PROJECTS");
            Assert.False(Directory.Exists(unsafeOutputRoot));
        }
        finally
        {
            DeleteIfExists(root);
            Assert.False(Directory.Exists(unsafeOutputRoot));
        }
    }

    [Fact]
    public async Task ProjectsSiblingPrefix_IsAllowedAndCleanedUp()
    {
        var scratchRoot = CreateTempRoot();
        var projects2Root = Path.Combine(
            Path.GetPathRoot(Path.GetFullPath(@"C:\Projects"))
                ?? throw new InvalidOperationException("Could not resolve C: root."),
            "Projects2");
        var projects2Existed = Directory.Exists(projects2Root);
        var outputRoot = Path.Combine(
            projects2Root,
            $"bootstrap-tool-prefix-boundary-{Guid.NewGuid():N}");

        try
        {
            var corpusPath = WriteCorpus(scratchRoot);

            var result = await RunAsync(BuildValidArgs(outputRoot, corpusPath));

            Assert.Equal(0, result.ExitCode);
            using var success = JsonDocument.Parse(result.Stdout);
            var destination = RequiredString(success.RootElement, "destination");
            Assert.Equal(
                Path.GetFullPath(Path.Combine(outputRoot, "smoke-model", "rev-001")),
                Path.GetFullPath(destination));
            Assert.True(Directory.Exists(destination));
        }
        finally
        {
            DeleteIfExists(outputRoot);
            if (!projects2Existed
                && Directory.Exists(projects2Root)
                && !Directory.EnumerateFileSystemEntries(projects2Root).Any())
            {
                Directory.Delete(projects2Root);
            }

            DeleteIfExists(scratchRoot);
        }
    }

    [Fact]
    public async Task ExistingDestination_RejectsWithoutForce_AndForceReplacesOnlyExactDestination()
    {
        var root = CreateTempRoot();
        try
        {
            var corpusPath = WriteCorpus(root);
            var outputRoot = Path.Combine(root, "out");
            var args = BuildValidArgs(outputRoot, corpusPath);

            Assert.Equal(0, (await RunAsync(args)).ExitCode);
            var destination = Path.Combine(outputRoot, "smoke-model", "rev-001");
            var sentinel = Path.Combine(destination, "sentinel.txt");
            File.WriteAllText(sentinel, "must survive reject and disappear on force");

            var neighbor = Path.Combine(outputRoot, "smoke-model", "neighbor-revision");
            Directory.CreateDirectory(neighbor);
            var neighborSentinel = Path.Combine(neighbor, "neighbor.txt");
            File.WriteAllText(neighborSentinel, "must not be touched");

            var rejected = await RunAsync(args);

            Assert.NotEqual(0, rejected.ExitCode);
            AssertFailure(rejected, "BOOTSTRAP_DESTINATION_EXISTS");
            Assert.True(File.Exists(sentinel));
            Assert.True(File.Exists(neighborSentinel));

            var forced = await RunAsync([.. args, "--force"]);

            Assert.Equal(0, forced.ExitCode);
            Assert.False(File.Exists(sentinel));
            Assert.True(File.Exists(neighborSentinel));
            LlmModel.ValidateFullCheckpointArtifact(Path.Combine(destination, "checkpoint.bin"), SmokeConfig);
            await using var tokenizerStream = File.OpenRead(Path.Combine(destination, "tokenizer.json"));
            var tokenizer = BpeTokenizer.ValidateAndLoadCanonicalArtifact(tokenizerStream, SmokeConfig.VocabSize);
            Assert.Equal(SmokeConfig.VocabSize, tokenizer.VocabSize);
        }
        finally
        {
            DeleteIfExists(root);
        }
    }

    [Fact]
    public async Task ForceFailureAfterNewDestinationPublication_RestoresOldDestinationAndCleansSwapDirectories()
    {
        var root = CreateTempRoot();
        try
        {
            var corpusPath = WriteCorpus(root);
            var outputRoot = Path.Combine(root, "out");
            var args = BuildValidArgs(outputRoot, corpusPath);

            Assert.Equal(0, (await RunAsync(args)).ExitCode);
            var modelRoot = Path.Combine(outputRoot, "smoke-model");
            var destination = Path.Combine(modelRoot, "rev-001");
            var sentinel = Path.Combine(destination, "sentinel.txt");
            const string SentinelText = "old destination must be restored exactly";
            File.WriteAllText(sentinel, SentinelText);

            var neighbor = Path.Combine(modelRoot, "neighbor-revision");
            Directory.CreateDirectory(neighbor);
            var neighborSentinel = Path.Combine(neighbor, "neighbor.txt");
            const string NeighborText = "neighbor must remain untouched";
            File.WriteAllText(neighborSentinel, NeighborText);

            var hookObservedPublishedDestination = false;
            var dependencies = BootstrapCliDependencies.Production with
            {
                AfterForceDestinationPublishedBeforeBackupCleanup = paths =>
                {
                    hookObservedPublishedDestination = true;
                    Assert.Equal(Path.GetFullPath(destination), Path.GetFullPath(paths.Destination));
                    Assert.True(Directory.Exists(paths.Destination));
                    Assert.True(Directory.Exists(paths.BackupDirectory));
                    Assert.False(Directory.Exists(paths.StagingDirectory));
                    throw new IOException("Injected failure after destination publication.");
                },
            };

            var failed = await RunAsync([.. args, "--force"], dependencies);

            Assert.True(hookObservedPublishedDestination);
            Assert.NotEqual(0, failed.ExitCode);
            AssertFailure(failed, "BOOTSTRAP_PUBLICATION_FAILED");
            Assert.True(File.Exists(sentinel));
            Assert.Equal(SentinelText, File.ReadAllText(sentinel));
            Assert.True(File.Exists(neighborSentinel));
            Assert.Equal(NeighborText, File.ReadAllText(neighborSentinel));
            Assert.Empty(Directory.EnumerateDirectories(modelRoot, "*bootstrap-backup*", SearchOption.AllDirectories));
            Assert.Empty(Directory.EnumerateDirectories(modelRoot, "*bootstrap-tmp*", SearchOption.AllDirectories));
        }
        finally
        {
            DeleteIfExists(root);
        }
    }

    [Fact]
    public async Task Tp1LateFailure_RemovesOnlyNewEmptyRepositoryDpoDirectoryChain()
    {
        var root = CreateTempRoot();
        try
        {
            var corpusPath = WriteCorpus(root);
            var outputRoot = Path.Combine(root, "out");
            var serverCommandPath = WriteFakeServerCaptureCommand(
                root,
                Path.Combine(root, "unused-capture.json"));
            var repositoryRoot = Path.Combine(root, "canonical-repository");
            var args = BuildValidTp1Args(
                outputRoot,
                corpusPath,
                serverCommandPath,
                repositoryRoot);
            var dependencies = BootstrapCliDependencies.Production with
            {
                AfterDpoPairRootCreatedBeforePublication = _ =>
                    throw new IOException("Injected late failure."),
            };

            var result = await RunAsync(args, dependencies);

            AssertFailure(result, "BOOTSTRAP_FAILED");
            Assert.False(Directory.Exists(
                Path.Combine(repositoryRoot, "training-data")));
        }
        finally
        {
            DeleteIfExists(root);
        }
    }

    [Fact]
    public async Task Tp1FailureDuringDpoDirectoryChainCreation_RemovesPartialEmptyChain()
    {
        var root = CreateTempRoot();
        try
        {
            var corpusPath = WriteCorpus(root);
            var outputRoot = Path.Combine(root, "out");
            var serverCommandPath = WriteFakeServerCaptureCommand(
                root,
                Path.Combine(root, "unused-capture.json"));
            var repositoryRoot = Path.Combine(root, "canonical-repository");
            var args = BuildValidTp1Args(
                outputRoot,
                corpusPath,
                serverCommandPath,
                repositoryRoot);
            var observedCreatedDirectories = new List<string>();
            var dependencies = BootstrapCliDependencies.Production with
            {
                AfterDpoDirectoryCreated = directory =>
                {
                    observedCreatedDirectories.Add(directory);
                    if (Path.GetFileName(directory) == "dpo")
                        throw new IOException("Injected middle-chain failure.");
                },
            };

            var result = await RunAsync(args, dependencies);

            AssertFailure(result, "BOOTSTRAP_FAILED");
            Assert.Equal(2, observedCreatedDirectories.Count);
            Assert.EndsWith(
                Path.Combine("training-data", "dpo"),
                observedCreatedDirectories[^1],
                StringComparison.OrdinalIgnoreCase);
            Assert.False(Directory.Exists(
                Path.Combine(repositoryRoot, "training-data")));
        }
        finally
        {
            DeleteIfExists(root);
        }
    }

    [Fact]
    public async Task Tp1LateFailure_PreservesPreExistingNonEmptyPairStore()
    {
        var root = CreateTempRoot();
        try
        {
            var corpusPath = WriteCorpus(root);
            var outputRoot = Path.Combine(root, "out");
            var serverCommandPath = WriteFakeServerCaptureCommand(
                root,
                Path.Combine(root, "unused-capture.json"));
            var repositoryRoot = Path.Combine(root, "canonical-repository");
            var args = BuildValidTp1Args(
                outputRoot,
                corpusPath,
                serverCommandPath,
                repositoryRoot);
            var pairRoot = Path.Combine(
                repositoryRoot,
                "training-data",
                "dpo",
                "html-css",
                "learning-unit",
                "v0001",
                "pairs");
            Directory.CreateDirectory(pairRoot);
            var sentinel = Path.Combine(pairRoot, "existing.json");
            File.WriteAllText(sentinel, "foreign data");
            var dependencies = BootstrapCliDependencies.Production with
            {
                AfterDpoPairRootCreatedBeforePublication = _ =>
                    throw new IOException("Injected late failure."),
            };

            var result = await RunAsync(args, dependencies);

            AssertFailure(result, "BOOTSTRAP_FAILED");
            Assert.Equal("foreign data", File.ReadAllText(sentinel));
        }
        finally
        {
            DeleteIfExists(root);
        }
    }

    [Fact]
    public async Task Tp1CancellationAfterPairRootCreation_RemovesNewEmptyChain()
    {
        var root = CreateTempRoot();
        try
        {
            var corpusPath = WriteCorpus(root);
            var outputRoot = Path.Combine(root, "out");
            var serverCommandPath = WriteFakeServerCaptureCommand(
                root,
                Path.Combine(root, "unused-capture.json"));
            var repositoryRoot = Path.Combine(root, "canonical-repository");
            var args = BuildValidTp1Args(
                outputRoot,
                corpusPath,
                serverCommandPath,
                repositoryRoot);
            using var cancellation = new CancellationTokenSource();
            var dependencies = BootstrapCliDependencies.Production with
            {
                AfterDpoPairRootCreatedBeforePublication = _ => cancellation.Cancel(),
            };

            var result = await RunAsync(args, dependencies, cancellation.Token);

            AssertFailure(result, "BOOTSTRAP_CANCELLED");
            Assert.False(Directory.Exists(
                Path.Combine(repositoryRoot, "training-data")));
        }
        finally
        {
            DeleteIfExists(root);
        }
    }

    [Fact]
    public async Task InvalidArgumentsConfigCorpusAndImportishOptions_AreRejectedWithoutOutput()
    {
        var root = CreateTempRoot();
        try
        {
            var corpusPath = WriteCorpus(root);
            var emptyCorpus = Path.Combine(root, "empty.txt");
            File.WriteAllText(emptyCorpus, string.Empty);
            var outputRoot = Path.Combine(root, "out");

            var cases = new (string Name, string[] Args, string ErrorCode)[]
            {
                ("missing model id", BuildArgs(outputRoot, corpusPath, omit: "--model-id"), "BOOTSTRAP_MISSING_ARGUMENT"),
                ("unsafe identity", BuildValidArgs(outputRoot, corpusPath, "--model-id", "..\\escape"), "BOOTSTRAP_INVALID_IDENTITY"),
                ("invalid profile", BuildValidArgs(outputRoot, corpusPath, "--profile", "real-training"), "BOOTSTRAP_INVALID_PROFILE"),
                ("invalid config", BuildValidArgs(outputRoot, corpusPath, "--hidden-dim", "10", "--num-heads", "4"), "BOOTSTRAP_INVALID_CONFIG"),
                ("empty corpus", BuildValidArgs(outputRoot, emptyCorpus), "BOOTSTRAP_INVALID_CORPUS"),
                ("unknown import", [.. BuildValidArgs(outputRoot, corpusPath), "--import-checkpoint", Path.Combine(root, "foreign.bin")], "BOOTSTRAP_UNKNOWN_OPTION"),
                ("unknown gguf", [.. BuildValidArgs(outputRoot, corpusPath), "--gguf", Path.Combine(root, "model.gguf")], "BOOTSTRAP_UNKNOWN_OPTION"),
                ("unknown probe", [.. BuildValidArgs(outputRoot, corpusPath), "--probe-path", Path.Combine(root, "probe.json")], "BOOTSTRAP_UNKNOWN_OPTION"),
            };

            foreach (var testCase in cases)
            {
                var caseOutputRoot = Path.Combine(root, $"out-{Guid.NewGuid():N}");
                var args = ReplaceOption(testCase.Args, "--output-root", caseOutputRoot);

                var result = await RunAsync(args);

                Assert.True(result.ExitCode != 0, testCase.Name);
                AssertFailure(result, testCase.ErrorCode);
                Assert.False(Directory.Exists(caseOutputRoot), testCase.Name);
            }
        }
        finally
        {
            DeleteIfExists(root);
        }
    }

    [Fact]
    public async Task OversizeSmokeConfig_IsRejectedBeforeModelAllocationAndWithoutOutput()
    {
        var root = CreateTempRoot();
        try
        {
            var corpusPath = WriteCorpus(root);
            var outputRoot = Path.Combine(root, "out");

            var result = await RunAsync(BuildValidArgs(
                outputRoot,
                corpusPath,
                "--hidden-dim",
                "1000000",
                "--num-heads",
                "1",
                "--num-kv-heads",
                "1",
                "--ffn-hidden-dim",
                "1000000"));

            Assert.NotEqual(0, result.ExitCode);
            AssertFailure(result, "BOOTSTRAP_INVALID_CONFIG");
            Assert.False(Directory.Exists(outputRoot));
        }
        finally
        {
            DeleteIfExists(root);
        }
    }

    [Theory]
    [InlineData("checkpoint")]
    [InlineData("tokenizer")]
    public async Task TamperedArtifacts_AreValidatedBeforeSuccess_AndNeverPublished(string mutation)
    {
        var root = CreateTempRoot();
        try
        {
            var corpusPath = WriteCorpus(root);
            var outputRoot = Path.Combine(root, "out");
            var dependencies = BootstrapCliDependencies.Production with
            {
                BeforeValidation = paths =>
                {
                    if (mutation == "checkpoint")
                    {
                        using var stream = new FileStream(paths.CheckpointPath, FileMode.Open, FileAccess.Write, FileShare.None);
                        stream.Write(new byte[] { 0, 0, 0, 0 });
                    }
                    else if (mutation == "tokenizer")
                    {
                        File.WriteAllText(paths.TokenizerPath, "{\"Vocab\":{},\"Merges\":[]}");
                    }
                    else
                    {
                        throw new InvalidOperationException("Unknown mutation.");
                    }
                },
            };

            var result = await RunAsync(BuildValidArgs(outputRoot, corpusPath), dependencies);

            Assert.NotEqual(0, result.ExitCode);
            AssertFailure(result, "BOOTSTRAP_ARTIFACT_VALIDATION_FAILED");
            Assert.False(Directory.Exists(Path.Combine(outputRoot, "smoke-model", "rev-001")));
            Assert.Empty(Directory.Exists(outputRoot)
                ? Directory.EnumerateDirectories(outputRoot, "*bootstrap-tmp*", SearchOption.AllDirectories)
                : []);
        }
        finally
        {
            DeleteIfExists(root);
        }
    }

    private static async Task<(int ExitCode, string Stdout, string Stderr)> RunAsync(
        string[] args,
        BootstrapCliDependencies? dependencies = null,
        CancellationToken cancellationToken = default)
    {
        using var stdout = new StringWriter();
        using var stderr = new StringWriter();
        var exitCode = dependencies is null
            ? await BootstrapCli.RunAsync(args, stdout, stderr, cancellationToken)
            : await BootstrapCli.RunAsync(args, stdout, stderr, cancellationToken, dependencies);
        return (exitCode, stdout.ToString(), stderr.ToString());
    }

    private static string[] BuildValidArgs(
        string outputRoot,
        string corpusPath,
        params string[] overrides)
        => BuildArgs(outputRoot, corpusPath, overrides: overrides);

    private static string[] BuildValidTp1Args(
        string outputRoot,
        string corpusPath,
        string liveLocalServerCommandPath,
        string? repositoryRoot = null)
    {
        repositoryRoot ??= Path.Combine(
            Path.GetDirectoryName(Path.GetFullPath(corpusPath))
                ?? throw new InvalidOperationException("Corpus parent is missing."),
            "canonical-repository");
        CreateRepositorySentinels(repositoryRoot);
        var options = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["--tp1-model-id"] = "qwen3.5:9b-q4_K_M",
            ["--tp1-ollama-manifest-digest"] = new string('d', 64),
            ["--tp1-ollama-model-layer-digest"] = new string('9', 64),
            ["--base-model-id"] = "registry/qwen3.5-base-9b",
            ["--base-model-revision"] = "base-checkpoint-r17",
            ["--max-teacher-calls"] = "0",
            ["--dpo-max-steps"] = "1",
            ["--audit-salt-base64"] = ValidAuditSalt,
            ["--local-retention-policy-ref"] = $"sha256:{new string('a', 64)}",
            ["--output-license-ref"] = $"sha256:{new string('b', 64)}",
            ["--teacher-legal-evidence-ref"] = $"sha256:{new string('c', 64)}",
            ["--teacher-license-evidence-ref"] = $"sha256:{new string('e', 64)}",
            ["--deployment-evidence-ref"] = $"sha256:{new string('f', 64)}",
            ["--repository-root"] = repositoryRoot,
            ["--dpo-domain-slug"] = "html-css",
            ["--dpo-design-pattern-slug"] = "learning-unit",
            ["--dpo-dataset-version"] = "v0001",
            ["--lifecycle-auth-token-current"] = ValidLifecycleToken,
            ["--lifecycle-auth-token-previous"] = ValidPreviousLifecycleToken,
            ["--lifecycle-auth-token-previous-expires-at-utc"] = ValidPreviousExpiryInput,
            ["--route-admission-token-current"] = ValidRouteAdmissionToken,
            ["--route-admission-token-previous"] = ValidPreviousRouteAdmissionToken,
            ["--route-admission-token-previous-expires-at-utc"] = ValidPreviousExpiryInput,
            ["--route-admission-tenant-id"] = ValidRouteAdmissionTenantId,
            ["--live-local-server-command"] = liveLocalServerCommandPath,
            ["--measurement-required-held-out-quality-score"] = "0.95",
            ["--measurement-max-runtime-ms"] = "1000",
            ["--measurement-max-cost-usd"] = "1.5",
            ["--measurement-max-peak-ram-mb"] = "1024",
            ["--training-batch-size"] = "1",
            ["--training-seq-len"] = "8",
            ["--training-max-lr"] = "0.01",
            ["--training-min-lr"] = "0.001",
            ["--training-warmup-steps"] = "0",
            ["--training-total-steps"] = "10",
            ["--training-grad-accum-steps"] = "1",
            ["--training-grad-clip-norm"] = "1",
            ["--training-weight-decay"] = "0",
            ["--training-checkpoint-every"] = "1",
            ["--training-log-every"] = "1",
        };

        return
        [
            .. BuildValidArgs(outputRoot, corpusPath),
            "--tp1-live-local-bundle",
            .. options.SelectMany(static option => new[] { option.Key, option.Value }),
        ];
    }

    private static string[] BuildArgs(
        string outputRoot,
        string corpusPath,
        string? omit = null,
        params string[] overrides)
    {
        var options = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["--output-root"] = outputRoot,
            ["--model-id"] = "smoke-model",
            ["--revision"] = "rev-001",
            ["--profile"] = "bootstrap-smoke",
            ["--vocab-size"] = SmokeConfig.VocabSize.ToString(),
            ["--context-len"] = SmokeConfig.ContextLen.ToString(),
            ["--hidden-dim"] = SmokeConfig.HiddenDim.ToString(),
            ["--num-layers"] = SmokeConfig.NumLayers.ToString(),
            ["--num-heads"] = SmokeConfig.NumHeads.ToString(),
            ["--num-kv-heads"] = SmokeConfig.NumKvHeads.ToString(),
            ["--ffn-hidden-dim"] = SmokeConfig.FfnHiddenDim.ToString(),
            ["--tokenizer-corpus"] = corpusPath,
        };

        if (omit is not null)
            options.Remove(omit);
        for (var index = 0; index < overrides.Length; index += 2)
            options[overrides[index]] = overrides[index + 1];

        return options.SelectMany(static option => new[] { option.Key, option.Value }).ToArray();
    }

    private static string[] ReplaceOption(string[] args, string option, string value)
    {
        var clone = args.ToArray();
        var index = Array.IndexOf(clone, option);
        Assert.True(index >= 0 && index + 1 < clone.Length);
        clone[index + 1] = value;
        return clone;
    }

    private static string[] RemoveOptionWithValue(string[] args, string option)
    {
        var index = Array.IndexOf(args, option);
        Assert.True(index >= 0 && index + 1 < args.Length);
        return args
            .Where((_, itemIndex) => itemIndex != index && itemIndex != index + 1)
            .ToArray();
    }

    private static void AssertFailure(
        (int ExitCode, string Stdout, string Stderr) result,
        string expectedErrorCode)
    {
        Assert.Equal(string.Empty, result.Stdout.Trim());
        using var failure = JsonDocument.Parse(result.Stderr);
        Assert.False(failure.RootElement.GetProperty("ok").GetBoolean());
        Assert.Equal(expectedErrorCode, RequiredString(failure.RootElement, "error_code"));
        Assert.False(string.IsNullOrWhiteSpace(RequiredString(failure.RootElement, "message")));
    }

    private static string WriteCorpus(string root)
    {
        var path = Path.Combine(root, "corpus.txt");
        var text = string.Join(
            Environment.NewLine,
            Enumerable.Range(0, 64).Select(static i =>
                $"bootstrap smoke corpus line {i} alpha beta gamma delta epsilon zeta repeated repeated tokens {i % 7}"));
        File.WriteAllText(path, text, Encoding.UTF8);
        return path;
    }

    private static void CreateRepositorySentinels(string repositoryRoot)
    {
        Directory.CreateDirectory(repositoryRoot);
        File.WriteAllText(
            Path.Combine(repositoryRoot, "LlmFromScratch.sln"),
            "fixture solution sentinel",
            Encoding.UTF8);
        Directory.CreateDirectory(Path.Combine(repositoryRoot, "14.ContinuousLearning"));
        Directory.CreateDirectory(Path.Combine(repositoryRoot, "11.DPO"));
    }

    private static string CreateTempRoot()
    {
        var path = Path.Combine(Path.GetTempPath(), $"continuous-learning-bootstrap-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);
        Assert.DoesNotContain(
            Path.GetFullPath(@"C:\Projects").TrimEnd(Path.DirectorySeparatorChar),
            Path.GetFullPath(path),
            StringComparison.OrdinalIgnoreCase);
        return path;
    }

    private static string RequiredString(JsonElement element, string propertyName)
        => element.GetProperty(propertyName).GetString()
            ?? throw new InvalidOperationException($"{propertyName} was null.");

    private static void AssertJsonDeepEquals(
        string label,
        JsonElement expected,
        JsonElement actual)
    {
        if (JsonElement.DeepEquals(expected, actual))
            return;

        Assert.Fail(BuildJsonDiffMessage(label, expected, actual));
    }

    private static string BuildJsonDiffMessage(
        string label,
        JsonElement expected,
        JsonElement actual)
    {
        var expectedFields = new SortedDictionary<string, string>(
            StringComparer.Ordinal);
        var actualFields = new SortedDictionary<string, string>(
            StringComparer.Ordinal);
        FlattenJsonElement("$", expected, expectedFields);
        FlattenJsonElement("$", actual, actualFields);
        var keys = expectedFields.Keys
            .Concat(actualFields.Keys)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(static key => key, StringComparer.Ordinal);
        var diff = keys
            .Where(key =>
                !expectedFields.TryGetValue(key, out var expectedValue)
                || !actualFields.TryGetValue(key, out var actualValue)
                || !string.Equals(expectedValue, actualValue, StringComparison.Ordinal))
            .Select(key =>
            {
                expectedFields.TryGetValue(key, out var expectedValue);
                actualFields.TryGetValue(key, out var actualValue);
                return $"{key}: expected={expectedValue ?? "<missing>"} actual={actualValue ?? "<missing>"}";
            });

        return string.Join(
            Environment.NewLine,
            [
                label,
                "expected:",
                JsonSerializer.Serialize(expected, DesignConversionModelServerHttpHandler.JsonOptions),
                "actual:",
                JsonSerializer.Serialize(actual, DesignConversionModelServerHttpHandler.JsonOptions),
                "diff:",
                .. diff,
            ]);
    }

    private static void FlattenJsonElement(
        string path,
        JsonElement element,
        IDictionary<string, string> fields)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                    FlattenJsonElement(
                        path + "." + property.Name,
                        property.Value,
                        fields);
                break;
            case JsonValueKind.Array:
                var index = 0;
                foreach (var value in element.EnumerateArray())
                    FlattenJsonElement(
                        $"{path}[{index++}]",
                        value,
                        fields);
                break;
            default:
                fields[path] = element.GetRawText();
                break;
        }
    }

    private static Dictionary<string, string> ReadLaunchEnvironment(string path)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return document.RootElement
            .GetProperty("environment_variables")
            .EnumerateObject()
            .ToDictionary(
                static property => property.Name,
                static property => property.Value.GetString()
                    ?? throw new InvalidOperationException($"{property.Name} was null."),
                StringComparer.Ordinal);
    }

    private static string[] ReadLaunchServerArguments(string path)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return document.RootElement
            .GetProperty("server_arguments")
            .EnumerateArray()
            .Select(static value => value.GetString()
                ?? throw new InvalidOperationException("server_arguments contained null."))
            .ToArray();
    }

    private static Dictionary<string, string> RemoveEnvironmentKey(
        IReadOnlyDictionary<string, string> source,
        string key)
    {
        var clone = new Dictionary<string, string>(source, StringComparer.Ordinal);
        Assert.True(clone.Remove(key), key);
        return clone;
    }

    private static void AssertPreTrainingBundleManifests(
        string manifestPath,
        string bundleManifestPath)
    {
        using var publicManifest = JsonDocument.Parse(File.ReadAllText(manifestPath));
        var publicSummary = publicManifest.RootElement.GetProperty("tp1_live_local_bundle");
        Assert.True(publicSummary.GetProperty("training_not_started").GetBoolean());
        Assert.Equal(0, publicSummary.GetProperty("teacher_calls_executed").GetInt32());
        Assert.Equal(0, publicSummary.GetProperty("dpo_steps_executed").GetInt32());
        Assert.True(publicSummary.GetProperty("weights_not_trained").GetBoolean());
        Assert.False(publicSummary.GetProperty("deployment_product_readiness").GetBoolean());

        using var bundleManifest = JsonDocument.Parse(File.ReadAllText(bundleManifestPath));
        Assert.True(bundleManifest.RootElement.GetProperty("training_not_started").GetBoolean());
        Assert.Equal(0, bundleManifest.RootElement.GetProperty("teacher_calls_executed").GetInt32());
        Assert.Equal(0, bundleManifest.RootElement.GetProperty("dpo_steps_executed").GetInt32());
        Assert.True(bundleManifest.RootElement.GetProperty("weights_not_trained").GetBoolean());
        Assert.False(bundleManifest.RootElement.GetProperty("deployment_product_readiness").GetBoolean());
    }

    private static async Task<JsonDocument> WaitForReadyJsonAsync(string path)
    {
        var deadline = DateTimeOffset.UtcNow.AddSeconds(10);
        Exception? lastError = null;
        while (DateTimeOffset.UtcNow < deadline)
        {
            if (File.Exists(path))
            {
                try
                {
                    await using var stream = new FileStream(
                        path,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.Read,
                        bufferSize: 4096,
                        useAsync: true);
                    return await JsonDocument.ParseAsync(stream);
                }
                catch (Exception ex) when (ex is IOException or JsonException)
                {
                    lastError = ex;
                }
            }

            await Task.Delay(TimeSpan.FromMilliseconds(100));
        }

        throw new TimeoutException(
            $"Ready file was not written with valid JSON: {path}",
            lastError);
    }

    private static Dictionary<string, string> WithEnvironment(
        IReadOnlyDictionary<string, string> source,
        string key,
        string value)
    {
        var clone = new Dictionary<string, string>(source, StringComparer.Ordinal)
        {
            [key] = value,
        };
        return clone;
    }

    private static void AssertLoaderState(
        IReadOnlyDictionary<string, string> environment,
        ContinuousLearningConfigurationState expectedState)
    {
        var resolution = ContinuousLearningModelServerConfigurationLoader.Resolve(
            name => environment.GetValueOrDefault(name));
        Assert.True(
            resolution.State == expectedState,
            $"Expected ContinuousLearning loader state {expectedState}, got {resolution.State}; error={resolution.ErrorCode}.");
        if (expectedState == ContinuousLearningConfigurationState.Enabled)
            Assert.NotNull(resolution.DeferredPlan);
        else
            Assert.Null(resolution.DeferredPlan);
    }

    private static string BuildCanonicalTp1ModelId(int length)
    {
        const string Prefix = "qwen-";
        const string Tag = ":tp1";
        Assert.True(length >= Prefix.Length + Tag.Length);
        return Prefix + new string('a', length - Prefix.Length - Tag.Length) + Tag;
    }

    private static string UniqueShaRef(string label)
        => "sha256:" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(
            label + "-" + Guid.NewGuid().ToString("N")))).ToLowerInvariant();

    private static string WriteFakeServerCaptureCommand(string root, string capturePath)
    {
        Assert.Equal(
            Path.GetFullPath(root),
            Path.GetFullPath(Path.GetDirectoryName(capturePath) ?? string.Empty));
        var commandPath = Path.Combine(root, "fake-live-local-server.ps1");
        var captureFileNameLiteral = PowerShellSingleQuoted(Path.GetFileName(capturePath));
        var script = $$"""
            $ErrorActionPreference = 'Stop'
            # Capture пишется рядом с fake-server скриптом, чтобы один и тот же
            # тест честно работал и в Linux pwsh, и в Windows pwsh.exe над WSL UNC.
            $capturePath = Join-Path -Path $PSScriptRoot -ChildPath {{captureFileNameLiteral}}
            $capturedEnvironment = [ordered]@{}
            Get-ChildItem Env: |
                Where-Object { $_.Name -like 'XIIGEN_CONTINUOUS_LEARNING_*' -or $_.Name -like 'DESIGN_CONVERSION_LIVE_*' } |
                Sort-Object Name |
                ForEach-Object { $capturedEnvironment[$_.Name] = $_.Value }
            $payload = [ordered]@{
                arguments = @($args)
                environment_variables = $capturedEnvironment
            }
            $payload | ConvertTo-Json -Depth 5 -Compress | Set-Content -LiteralPath $capturePath -Encoding UTF8
            exit 0
            """;
        File.WriteAllText(commandPath, script, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        return commandPath;
    }

    private static bool IsTp1LaunchPathEnvironmentVariable(string name)
        => name is "XIIGEN_CONTINUOUS_LEARNING_BASE_CHECKPOINT"
            or "XIIGEN_CONTINUOUS_LEARNING_TOKENIZER"
            or "XIIGEN_CONTINUOUS_LEARNING_ADAPTER_ROOT"
            or "XIIGEN_CONTINUOUS_LEARNING_CACHE_ROOT"
            or "XIIGEN_CONTINUOUS_LEARNING_REGISTRY_ROOT"
            or "XIIGEN_CONTINUOUS_LEARNING_REPOSITORY_ROOT"
            or "XIIGEN_CONTINUOUS_LEARNING_DPO_PAIR_ROOT";

    private static void AssertHostEquivalentPath(
        string expectedLinuxPath,
        string? actualPath,
        string label)
    {
        Assert.False(string.IsNullOrWhiteSpace(actualPath), label);
        if (string.Equals(expectedLinuxPath, actualPath, StringComparison.Ordinal))
            return;

        var candidates = EnumerateHostEquivalentPaths(expectedLinuxPath).ToArray();
        Assert.True(
            candidates.Any(candidate => string.Equals(candidate, actualPath, StringComparison.OrdinalIgnoreCase)),
            $"{label}: expected path equivalent to {expectedLinuxPath}, got {actualPath}. " +
            $"Allowed equivalents: {string.Join(", ", candidates)}");
    }

    private static IEnumerable<string> EnumerateHostEquivalentPaths(string linuxPath)
    {
        yield return linuxPath;

        const string MountPrefix = "/mnt/";
        if (linuxPath.StartsWith(MountPrefix, StringComparison.OrdinalIgnoreCase)
            && linuxPath.Length >= MountPrefix.Length + 1
            && char.IsAsciiLetter(linuxPath[MountPrefix.Length]))
        {
            var afterDriveIndex = MountPrefix.Length + 1;
            if (linuxPath.Length == afterDriveIndex || linuxPath[afterDriveIndex] == '/')
            {
                var driveLetter = char.ToUpperInvariant(linuxPath[MountPrefix.Length]);
                var suffix = linuxPath.Length == afterDriveIndex
                    ? string.Empty
                    : linuxPath[afterDriveIndex..].Replace('/', '\\');
                yield return driveLetter + ":" + (suffix.Length == 0 ? "\\" : suffix);
            }
        }

        if (!linuxPath.StartsWith("/", StringComparison.Ordinal))
            yield break;

        var distroName = Environment.GetEnvironmentVariable("WSL_DISTRO_NAME");
        if (string.IsNullOrWhiteSpace(distroName))
            yield break;

        var uncSuffix = linuxPath.Replace('/', '\\');
        yield return @"\\wsl.localhost\" + distroName + uncSuffix;
        yield return @"\\wsl$\" + distroName + uncSuffix;
    }

    private static async Task<(int ExitCode, string Stdout, string Stderr)>
        RunGeneratedPowerShellAsync(string scriptPath)
    {
        var startInfo = global::PowerShellTestHarness.CreateScriptStartInfo(
            scriptPath,
            Directory.GetCurrentDirectory(),
            includeNoLogo: true,
            includeNonInteractive: true);
        startInfo.RedirectStandardOutput = true;
        startInfo.RedirectStandardError = true;
        startInfo.CreateNoWindow = true;

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("PowerShell capture harness did not start.");
        var stdoutTask = process.StandardOutput.ReadToEndAsync();
        var stderrTask = process.StandardError.ReadToEndAsync();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        try
        {
            await process.WaitForExitAsync(timeout.Token);
        }
        catch (OperationCanceledException)
        {
            process.Kill(entireProcessTree: true);
            throw;
        }

        return (process.ExitCode, await stdoutTask, await stderrTask);
    }

    private static IEnumerable<string> RawSecretValues()
    {
        yield return ValidLifecycleToken;
        yield return ValidPreviousLifecycleToken;
        yield return ValidRouteAdmissionToken;
        yield return ValidPreviousRouteAdmissionToken;
        yield return ValidAuditSalt;
    }

    private static void AssertOnlyPrivateJsonContainsSecrets(
        string destination,
        string privateEnvironmentJsonPath)
    {
        var expectedPath = Path.GetFullPath(privateEnvironmentJsonPath);
        foreach (var secret in RawSecretValues())
        {
            var holders = Directory.EnumerateFiles(destination, "*", SearchOption.AllDirectories)
                .Where(path => FileContainsUtf8(path, secret))
                .Select(Path.GetFullPath)
                .ToArray();
            Assert.Equal(new[] { expectedPath }, holders);
        }
    }

    private static bool FileContainsUtf8(string path, string value)
    {
        var bytes = File.ReadAllBytes(path);
        var expected = Encoding.UTF8.GetBytes(value);
        return bytes.AsSpan().IndexOf(expected) >= 0;
    }

    private static int CountOccurrences(string text, string value)
    {
        var count = 0;
        var start = 0;
        while ((start = text.IndexOf(value, start, StringComparison.Ordinal)) >= 0)
        {
            count++;
            start += value.Length;
        }
        return count;
    }

    private static string PowerShellSingleQuoted(string value)
        => $"'{value.Replace("'", "''", StringComparison.Ordinal)}'";

    private static void AssertInside(string root, string candidate)
    {
        var normalizedRoot = NormalizeForBoundary(root);
        var normalizedCandidate = NormalizeForBoundary(candidate);
        Assert.True(
            string.Equals(normalizedCandidate, normalizedRoot, StringComparison.OrdinalIgnoreCase)
            || normalizedCandidate.StartsWith(
                normalizedRoot + Path.DirectorySeparatorChar,
                StringComparison.OrdinalIgnoreCase),
            $"{candidate} should be under {root}");
    }

    private static void AssertOutsideProjects(string candidate)
    {
        var projects = NormalizeForBoundary(@"C:\Projects");
        var normalizedCandidate = NormalizeForBoundary(candidate);
        Assert.False(
            string.Equals(normalizedCandidate, projects, StringComparison.OrdinalIgnoreCase)
            || normalizedCandidate.StartsWith(
                projects + Path.DirectorySeparatorChar,
                StringComparison.OrdinalIgnoreCase),
            $"{candidate} should not be under C:\\Projects");
    }

    private static string NormalizeForBoundary(string path)
        => Path.TrimEndingDirectorySeparator(Path.GetFullPath(path))
            .Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

    private static string[] SnapshotTree(string root)
        => Directory
            .EnumerateFileSystemEntries(root, "*", SearchOption.AllDirectories)
            .Select(path =>
                File.Exists(path)
                    ? $"F|{Path.GetRelativePath(root, path)}|{new FileInfo(path).Length}"
                    : $"D|{Path.GetRelativePath(root, path)}")
            .Order(StringComparer.Ordinal)
            .ToArray();

    private static void AssertPublicTextDoesNotContainSecrets(string text)
    {
        foreach (var secret in RawSecretValues())
            Assert.DoesNotContain(secret, text, StringComparison.Ordinal);
    }

    private static void AssertManifestMetadataResolvesToFile(
        JsonElement manifestRoot,
        string propertyName,
        string expectedPath)
    {
        var metadata = manifestRoot.GetProperty(propertyName);
        var manifestPath = RequiredString(metadata, "path");
        Assert.Equal(Path.GetFullPath(expectedPath), Path.GetFullPath(manifestPath));
        Assert.True(File.Exists(manifestPath));
    }

    private static string Sha256(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    private static void DeleteIfExists(string path)
    {
        if (Directory.Exists(path))
            Directory.Delete(path, recursive: true);
    }

    private sealed class ProcessEnvironmentScope : IDisposable
    {
        private readonly Dictionary<string, string?> _prior =
            new(StringComparer.Ordinal);

        public ProcessEnvironmentScope(IReadOnlyDictionary<string, string> values)
        {
            ArgumentNullException.ThrowIfNull(values);

            var names = new SortedSet<string>(StringComparer.Ordinal);
            foreach (DictionaryEntry entry in Environment.GetEnvironmentVariables())
            {
                if (entry.Key is string name && IsScopedModelServerEnvironmentName(name))
                    names.Add(name);
            }
            foreach (var name in values.Keys)
                names.Add(name);
            names.Add(ModelServerProgram.ReadyFileEnvironmentName);

            foreach (var name in names)
            {
                _prior[name] = Environment.GetEnvironmentVariable(name);
                Environment.SetEnvironmentVariable(name, null);
            }

            foreach (var (name, value) in values)
                Environment.SetEnvironmentVariable(name, value);
        }

        public void Dispose()
        {
            foreach (var (name, value) in _prior)
                Environment.SetEnvironmentVariable(name, value);
        }

        private static bool IsScopedModelServerEnvironmentName(string name)
            => name.StartsWith(
                   "XIIGEN_CONTINUOUS_LEARNING_",
                   StringComparison.Ordinal)
               || name.StartsWith(
                   "DESIGN_CONVERSION_LIVE_",
                   StringComparison.Ordinal)
               || string.Equals(
                   name,
                   ModelServerProgram.ReadyFileEnvironmentName,
                   StringComparison.Ordinal);
    }
}
