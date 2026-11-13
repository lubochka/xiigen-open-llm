// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ContinuousLearning;
using Tokenizer;
using TransformerModel;
using TransformerModel.Config;

[assembly: InternalsVisibleTo("LlmFromScratch.Tests")]

namespace ContinuousLearningBaseBootstrap;

internal static class Program
{
    private static Task<int> Main(string[] args)
        => BootstrapCli.RunAsync(args, Console.Out, Console.Error, CancellationToken.None);
}

public static class BootstrapCli
{
    private const string ArtifactKind = "cold_internal_llm_v2_full_checkpoint";
    private const string SmokeProfile = "bootstrap-smoke";
    private const string Tp1LiveLocalBundleFlag = "--tp1-live-local-bundle";
    private const string AllowRandomAuditSaltFlag = "--allow-random-audit-salt";
    private const string Tp1LiveLocalBundleArtifactKind = "continuous_learning_tp1_live_local_pretraining_bundle";
    private const string Tp1LiveLocalBundleProfile = "tp1-live-local-bundle";
    private const string MeasurementGateThresholdsFileName = "measurement-gate-thresholds.json";
    private const string Tp1EnvironmentJsonFileName = "tp1-live-local-environment.json";
    private const string Tp1EnvironmentPowerShellFileName = "tp1-live-local-env.ps1";
    private const string Tp1BundleManifestFileName = "tp1-live-local-bundle.json";
    private const string LiveLocalReadyFileName = "live-local-ready.json";
    private const string ProjectsRootLiteral = @"C:\Projects";
    private const string ModelLifecycleCurrentTokenEnvironmentName =
        "DESIGN_CONVERSION_LIVE_MODEL_LIFECYCLE_CURRENT_TOKEN";
    private const string ModelLifecyclePreviousTokenEnvironmentName =
        "DESIGN_CONVERSION_LIVE_MODEL_LIFECYCLE_PREVIOUS_TOKEN";
    private const string ModelLifecyclePreviousTokenExpiresEnvironmentName =
        "DESIGN_CONVERSION_LIVE_MODEL_LIFECYCLE_PREVIOUS_TOKEN_EXPIRES_AT_UTC";
    private const string RouteAdmissionEnabledEnvironmentName =
        "DESIGN_CONVERSION_LIVE_ROUTE_ADMISSION_ENABLED";
    private const string RouteAdmissionCurrentTokenEnvironmentName =
        "DESIGN_CONVERSION_LIVE_ROUTE_CURRENT_TOKEN";
    private const string RouteAdmissionPreviousTokenEnvironmentName =
        "DESIGN_CONVERSION_LIVE_ROUTE_PREVIOUS_TOKEN";
    private const string RouteAdmissionPreviousTokenExpiresEnvironmentName =
        "DESIGN_CONVERSION_LIVE_ROUTE_PREVIOUS_TOKEN_EXPIRES_AT_UTC";
    private const string RouteAdmissionTenantIdEnvironmentName =
        "DESIGN_CONVERSION_LIVE_ROUTE_TENANT_ID";
    private const int FloatByteCount = sizeof(float);
    private const int AuditSaltByteCount = 32;
    private const int MinimumLocalSecretLength = 16;
    private const int MaximumLocalSecretLength = 4096;
    private const int MaximumDpoStepCount = 5;
    private const int MaximumCanonicalTenantIdLength = 128;
    private const int MaxSmokeVocabSize = 8_192;
    private const int MaxSmokeContextLen = 512;
    private const int MaxSmokeHiddenDim = 512;
    private const int MaxSmokeNumLayers = 8;
    private const int MaxSmokeNumHeads = 64;
    private const int MaxSmokeFfnHiddenDim = 2_048;
    private const long MaxSmokeModelParameterBytes = 64L * 1024 * 1024;
    private const long MaxSmokeActivationBytes = 8L * 1024 * 1024;
    private const int SuccessExitCode = 0;
    private const int FailureExitCode = 1;
    private const int CancellationExitCode = 130;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
    };

    private static readonly HashSet<string> ValueOptions = new(StringComparer.Ordinal)
    {
        "--output-root",
        "--model-id",
        "--revision",
        "--profile",
        "--vocab-size",
        "--context-len",
        "--hidden-dim",
        "--num-layers",
        "--num-heads",
        "--num-kv-heads",
        "--ffn-hidden-dim",
        "--tokenizer-corpus",
        "--tp1-model-id",
        "--tp1-ollama-manifest-digest",
        "--tp1-ollama-model-layer-digest",
        "--base-model-id",
        "--base-model-revision",
        "--max-teacher-calls",
        "--dpo-max-steps",
        "--audit-salt-base64",
        "--local-retention-policy-ref",
        "--output-license-ref",
        "--teacher-legal-evidence-ref",
        "--teacher-license-evidence-ref",
        "--deployment-evidence-ref",
        "--repository-root",
        "--dpo-domain-slug",
        "--dpo-design-pattern-slug",
        "--dpo-dataset-version",
        "--lifecycle-auth-token-current",
        "--lifecycle-auth-token-previous",
        "--lifecycle-auth-token-previous-expires-at-utc",
        "--route-admission-token-current",
        "--route-admission-token-previous",
        "--route-admission-token-previous-expires-at-utc",
        "--route-admission-tenant-id",
        "--live-local-server-command",
        "--measurement-required-held-out-quality-score",
        "--measurement-max-runtime-ms",
        "--measurement-max-cost-usd",
        "--measurement-max-peak-ram-mb",
        "--training-batch-size",
        "--training-seq-len",
        "--training-max-lr",
        "--training-min-lr",
        "--training-warmup-steps",
        "--training-total-steps",
        "--training-grad-accum-steps",
        "--training-grad-clip-norm",
        "--training-weight-decay",
        "--training-checkpoint-every",
        "--training-log-every",
    };

    private static readonly HashSet<string> FlagOptions = new(StringComparer.Ordinal)
    {
        "--force",
        Tp1LiveLocalBundleFlag,
        AllowRandomAuditSaltFlag,
    };

    private static readonly HashSet<string> Tp1ValueOptions = new(StringComparer.Ordinal)
    {
        "--tp1-model-id",
        "--tp1-ollama-manifest-digest",
        "--tp1-ollama-model-layer-digest",
        "--base-model-id",
        "--base-model-revision",
        "--max-teacher-calls",
        "--dpo-max-steps",
        "--audit-salt-base64",
        "--local-retention-policy-ref",
        "--output-license-ref",
        "--teacher-legal-evidence-ref",
        "--teacher-license-evidence-ref",
        "--deployment-evidence-ref",
        "--repository-root",
        "--dpo-domain-slug",
        "--dpo-design-pattern-slug",
        "--dpo-dataset-version",
        "--lifecycle-auth-token-current",
        "--lifecycle-auth-token-previous",
        "--lifecycle-auth-token-previous-expires-at-utc",
        "--route-admission-token-current",
        "--route-admission-token-previous",
        "--route-admission-token-previous-expires-at-utc",
        "--route-admission-tenant-id",
        "--live-local-server-command",
        "--measurement-required-held-out-quality-score",
        "--measurement-max-runtime-ms",
        "--measurement-max-cost-usd",
        "--measurement-max-peak-ram-mb",
        "--training-batch-size",
        "--training-seq-len",
        "--training-max-lr",
        "--training-min-lr",
        "--training-warmup-steps",
        "--training-total-steps",
        "--training-grad-accum-steps",
        "--training-grad-clip-norm",
        "--training-weight-decay",
        "--training-checkpoint-every",
        "--training-log-every",
    };

    private static readonly string[] RequiredValueOptions =
    [
        "--output-root",
        "--model-id",
        "--revision",
        "--profile",
        "--vocab-size",
        "--context-len",
        "--hidden-dim",
        "--num-layers",
        "--num-heads",
        "--num-kv-heads",
        "--ffn-hidden-dim",
        "--tokenizer-corpus",
    ];

    private static readonly string[] RequiredTp1ValueOptions =
    [
        "--tp1-model-id",
        "--tp1-ollama-manifest-digest",
        "--tp1-ollama-model-layer-digest",
        "--base-model-id",
        "--base-model-revision",
        "--max-teacher-calls",
        "--dpo-max-steps",
        "--local-retention-policy-ref",
        "--output-license-ref",
        "--teacher-legal-evidence-ref",
        "--teacher-license-evidence-ref",
        "--deployment-evidence-ref",
        "--repository-root",
        "--dpo-domain-slug",
        "--dpo-design-pattern-slug",
        "--dpo-dataset-version",
        "--lifecycle-auth-token-current",
        "--route-admission-token-current",
        "--route-admission-tenant-id",
        "--live-local-server-command",
        "--measurement-required-held-out-quality-score",
        "--measurement-max-runtime-ms",
        "--measurement-max-cost-usd",
        "--measurement-max-peak-ram-mb",
        "--training-batch-size",
        "--training-seq-len",
        "--training-max-lr",
        "--training-min-lr",
        "--training-warmup-steps",
        "--training-total-steps",
        "--training-grad-accum-steps",
        "--training-grad-clip-norm",
        "--training-weight-decay",
        "--training-checkpoint-every",
        "--training-log-every",
    ];

    public static Task<int> RunAsync(
        string[] args,
        TextWriter stdout,
        TextWriter stderr,
        CancellationToken cancellationToken)
        => RunAsync(
            args,
            stdout,
            stderr,
            cancellationToken,
            BootstrapCliDependencies.Production);

    internal static async Task<int> RunAsync(
        string[] args,
        TextWriter stdout,
        TextWriter stderr,
        CancellationToken cancellationToken,
        BootstrapCliDependencies dependencies)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(stdout);
        ArgumentNullException.ThrowIfNull(stderr);
        ArgumentNullException.ThrowIfNull(dependencies);

        try
        {
            var success = await ExecuteAsync(
                    args,
                    dependencies,
                    cancellationToken)
                .ConfigureAwait(false);
            await stdout.WriteLineAsync(JsonSerializer.Serialize(success, JsonOptions))
                .ConfigureAwait(false);
            return SuccessExitCode;
        }
        catch (OperationCanceledException)
        {
            await WriteErrorAsync(
                    stderr,
                    "BOOTSTRAP_CANCELLED",
                    "Bootstrap was cancelled.")
                .ConfigureAwait(false);
            return CancellationExitCode;
        }
        catch (BootstrapException exception)
        {
            await WriteErrorAsync(stderr, exception.ErrorCode, exception.Message)
                .ConfigureAwait(false);
            return FailureExitCode;
        }
        catch (Exception exception) when (IsRecoverableBootstrapFailure(exception))
        {
            await WriteErrorAsync(
                    stderr,
                    "BOOTSTRAP_FAILED",
                    "Bootstrap failed before publication completed.")
                .ConfigureAwait(false);
            return FailureExitCode;
        }
    }

    private static async Task<Dictionary<string, object?>> ExecuteAsync(
        string[] args,
        BootstrapCliDependencies dependencies,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var parsed = ParseArguments(args);
        var request = await BuildRequestAsync(parsed, cancellationToken).ConfigureAwait(false);
        var destination = Path.GetFullPath(Path.Combine(
            request.OutputRoot,
            request.ModelId,
            request.Revision));
        var destinationParent = Path.GetDirectoryName(destination)
            ?? throw new BootstrapException(
                "BOOTSTRAP_INVALID_ARGUMENT",
                "Destination parent directory could not be resolved.");

        if (File.Exists(destination))
            throw new BootstrapException(
                "BOOTSTRAP_DESTINATION_EXISTS",
                "Destination path already exists and is not a directory.");
        if (Directory.Exists(destination) && !request.Force)
            throw new BootstrapException(
                "BOOTSTRAP_DESTINATION_EXISTS",
                "Destination already exists. Use --force to replace that exact revision.");

        string? stagingDirectory = null;
        var dpoDirectoriesCreatedByThisCall = new List<string>();
        try
        {
            Directory.CreateDirectory(destinationParent);
            RejectExistingReparseSegments(destinationParent);
            stagingDirectory = Path.Combine(
                destinationParent,
                $"{request.Revision}.bootstrap-tmp-{Guid.NewGuid():N}");
            RejectUnderProjects(stagingDirectory);
            Directory.CreateDirectory(stagingDirectory);
            RejectExistingReparseSegments(stagingDirectory);

            var checkpointPath = Path.Combine(stagingDirectory, "checkpoint.bin");
            var tokenizerPath = Path.Combine(stagingDirectory, "tokenizer.json");
            var corpusPath = Path.Combine(stagingDirectory, "tokenizer-corpus.txt");
            var manifestPath = Path.Combine(stagingDirectory, "manifest.json");
            var paths = new BootstrapArtifactPaths(
                Destination: destination,
                StagingDirectory: stagingDirectory,
                CheckpointPath: checkpointPath,
                TokenizerPath: tokenizerPath,
                CorpusPath: corpusPath,
                ManifestPath: manifestPath);

            await WriteColdArtifactsAsync(request, paths, cancellationToken)
                .ConfigureAwait(false);

            ValidateProducedArtifacts(paths, request.Config, dependencies, cancellationToken);
            var artifactMetadata = BuildArtifactMetadata(paths);
            var tp1Bundle = request.Tp1LiveLocalBundle is null
                ? null
                : await WriteTp1LiveLocalBundleAsync(
                        request,
                        paths,
                        cancellationToken)
                    .ConfigureAwait(false);
            await WriteManifestAsync(
                    request,
                    paths,
                    artifactMetadata,
                    tp1Bundle,
                    cancellationToken)
                .ConfigureAwait(false);

            if (tp1Bundle is not null)
            {
                CreateRepositoryOwnedDirectoryChain(
                    tp1Bundle.FinalRepositoryRoot,
                    tp1Bundle.FinalDpoPairRoot,
                    dpoDirectoriesCreatedByThisCall,
                    dependencies.AfterDpoDirectoryCreated);
                dependencies.AfterDpoPairRootCreatedBeforePublication?.Invoke(
                    tp1Bundle.FinalDpoPairRoot);
            }

            artifactMetadata["manifest"] = BuildFileMetadata(
                manifestPath,
                Path.Combine(destination, "manifest.json"));
            cancellationToken.ThrowIfCancellationRequested();
            PublishStagingDirectory(
                stagingDirectory,
                destination,
                request.Force,
                cancellationToken,
                dependencies);
            stagingDirectory = null;

            var finalCheckpointPath = Path.Combine(destination, "checkpoint.bin");
            var finalTokenizerPath = Path.Combine(destination, "tokenizer.json");
            var finalCorpusPath = Path.Combine(destination, "tokenizer-corpus.txt");
            var finalManifestPath = Path.Combine(destination, "manifest.json");
            var result = new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["ok"] = true,
                ["destination"] = destination,
                ["checkpoint_path"] = finalCheckpointPath,
                ["tokenizer_path"] = finalTokenizerPath,
                ["corpus_path"] = finalCorpusPath,
                ["manifest_path"] = finalManifestPath,
                ["checkpoint_sha256"] = Sha256File(finalCheckpointPath),
                ["tokenizer_sha256"] = Sha256File(finalTokenizerPath),
                ["corpus_sha256"] = Sha256File(finalCorpusPath),
                ["manifest_sha256"] = Sha256File(finalManifestPath),
                ["model_id"] = request.ModelId,
                ["revision"] = request.Revision,
                ["profile"] = request.Profile,
            };
            if (tp1Bundle is not null)
            {
                result["tp1_live_local_bundle"] = true;
                result["tp1_bundle_manifest_path"] = tp1Bundle.FinalBundleManifestPath;
                result["tp1_environment_json_path"] = tp1Bundle.FinalEnvironmentJsonPath;
                result["tp1_environment_powershell_path"] = tp1Bundle.FinalEnvironmentPowerShellPath;
                result["measurement_gate_thresholds_path"] = tp1Bundle.FinalMeasurementGateThresholdsPath;
                result["adapter_root"] = tp1Bundle.FinalAdapterRoot;
                result["cache_root"] = tp1Bundle.FinalCacheRoot;
                result["registry_root"] = tp1Bundle.FinalRegistryRoot;
                result["repository_root"] = tp1Bundle.FinalRepositoryRoot;
                result["dpo_pair_root"] = tp1Bundle.FinalDpoPairRoot;
                result["artifact_db_root"] = tp1Bundle.FinalArtifactDbRoot;
                result["ready_file_path"] = tp1Bundle.FinalReadyFilePath;
            }

            return result;
        }
        catch
        {
            DeleteDirectoryIfExists(stagingDirectory);
            DeleteEmptyDirectoriesCreatedByThisCall(
                dpoDirectoriesCreatedByThisCall,
                request.Tp1LiveLocalBundle?.DpoDatasetAddress.RepositoryRoot);
            throw;
        }
    }

    private static ParsedArguments ParseArguments(string[] args)
    {
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        var flags = new HashSet<string>(StringComparer.Ordinal);

        for (var index = 0; index < args.Length; index++)
        {
            var option = args[index];
            if (!option.StartsWith("--", StringComparison.Ordinal))
                throw new BootstrapException(
                    "BOOTSTRAP_UNKNOWN_OPTION",
                    $"Unexpected argument '{option}'.");

            if (FlagOptions.Contains(option))
            {
                if (!flags.Add(option))
                    throw new BootstrapException(
                        "BOOTSTRAP_DUPLICATE_OPTION",
                        $"Duplicate option '{option}'.");
                continue;
            }

            if (!ValueOptions.Contains(option))
                throw new BootstrapException(
                    "BOOTSTRAP_UNKNOWN_OPTION",
                    $"Unknown option '{option}'.");

            if (values.ContainsKey(option))
                throw new BootstrapException(
                    "BOOTSTRAP_DUPLICATE_OPTION",
                    $"Duplicate option '{option}'.");

            if (index + 1 >= args.Length
                || args[index + 1].StartsWith("--", StringComparison.Ordinal))
            {
                throw new BootstrapException(
                    "BOOTSTRAP_MISSING_ARGUMENT",
                    $"Option '{option}' requires a value.");
            }

            values[option] = args[++index];
        }

        return new ParsedArguments(values, flags);
    }

    private static async Task<BootstrapRequest> BuildRequestAsync(
        ParsedArguments parsed,
        CancellationToken cancellationToken)
    {
        foreach (var required in RequiredValueOptions)
        {
            if (!parsed.Values.ContainsKey(required))
                throw new BootstrapException(
                    "BOOTSTRAP_MISSING_ARGUMENT",
                    $"Missing required option '{required}'.");
        }

        if (!parsed.Tp1LiveLocalBundle
            && (parsed.Values.Keys.Any(Tp1ValueOptions.Contains)
                || parsed.AllowRandomAuditSalt))
        {
            throw new BootstrapException(
                "BOOTSTRAP_TP1_BUNDLE_FLAG_REQUIRED",
                $"TP1 live-local bundle options require '{Tp1LiveLocalBundleFlag}'.");
        }

        var modelId = parsed.Values["--model-id"];
        var revision = parsed.Values["--revision"];
        ValidateIdentity(modelId, "--model-id");
        ValidateIdentity(revision, "--revision");

        var profile = parsed.Values["--profile"];
        if (!StringComparer.Ordinal.Equals(profile, SmokeProfile))
            throw new BootstrapException(
                "BOOTSTRAP_INVALID_PROFILE",
                $"Profile must be '{SmokeProfile}'.");

        var config = new ModelConfig
        {
            VocabSize = ParseConfigInt(parsed.Values["--vocab-size"], "--vocab-size"),
            ContextLen = ParseConfigInt(parsed.Values["--context-len"], "--context-len"),
            HiddenDim = ParseConfigInt(parsed.Values["--hidden-dim"], "--hidden-dim"),
            NumLayers = ParseConfigInt(parsed.Values["--num-layers"], "--num-layers"),
            NumHeads = ParseConfigInt(parsed.Values["--num-heads"], "--num-heads"),
            NumKvHeads = ParseConfigInt(parsed.Values["--num-kv-heads"], "--num-kv-heads"),
            FfnHiddenDim = ParseConfigInt(parsed.Values["--ffn-hidden-dim"], "--ffn-hidden-dim"),
        };
        ValidateConfig(config);

        var outputRoot = CanonicalizePath(
            parsed.Values["--output-root"],
            "BOOTSTRAP_INVALID_ARGUMENT",
            "Output root path is invalid.");
        RejectUnderProjects(outputRoot);
        RejectExistingReparseSegments(outputRoot);

        var destination = Path.GetFullPath(Path.Combine(outputRoot, modelId, revision));
        var tp1LiveLocalBundle = parsed.Tp1LiveLocalBundle
            ? BuildTp1LiveLocalBundleRequest(parsed, config, destination)
            : null;

        var corpusSourcePath = CanonicalizePath(
            parsed.Values["--tokenizer-corpus"],
            "BOOTSTRAP_INVALID_CORPUS",
            "Tokenizer corpus path is invalid.");
        var corpusText = await ReadCorpusTextAsync(corpusSourcePath, cancellationToken)
            .ConfigureAwait(false);

        return new BootstrapRequest(
            OutputRoot: outputRoot,
            ModelId: modelId,
            Revision: revision,
            Profile: profile,
            Config: config,
            CorpusSourcePath: corpusSourcePath,
            CorpusText: corpusText,
            Force: parsed.Force,
            Tp1LiveLocalBundle: tp1LiveLocalBundle);
    }

    private static Tp1LiveLocalBundleRequest BuildTp1LiveLocalBundleRequest(
        ParsedArguments parsed,
        ModelConfig modelConfig,
        string destination)
    {
        foreach (var required in RequiredTp1ValueOptions)
        {
            if (!parsed.Values.ContainsKey(required))
                throw new BootstrapException(
                    "BOOTSTRAP_MISSING_ARGUMENT",
                    $"Missing required option '{required}'.");
        }

        if (!parsed.AllowRandomAuditSalt
            && !parsed.Values.ContainsKey("--audit-salt-base64"))
        {
            throw new BootstrapException(
                "BOOTSTRAP_MISSING_ARGUMENT",
                "Missing required option '--audit-salt-base64'.");
        }

        if (parsed.AllowRandomAuditSalt
            && parsed.Values.ContainsKey("--audit-salt-base64"))
        {
            throw new BootstrapException(
                "BOOTSTRAP_INVALID_TP1_BUNDLE_ARGUMENT",
                "'--audit-salt-base64' cannot be combined with '--allow-random-audit-salt'.");
        }

        var tp1ModelId = ReadRequiredTp1Value(parsed, "--tp1-model-id");
        if (!ContinuousLearningTp1DpoConfiguration.IsCanonicalOllamaModelId(tp1ModelId))
            throw InvalidTp1Argument("TP1 local teacher model id must be a canonical Qwen Ollama model id.");

        var ollamaDigest = ReadRequiredTp1Value(parsed, "--tp1-ollama-manifest-digest");
        if (!ContinuousLearningTp1DpoConfiguration.IsCanonicalOllamaManifestDigest(ollamaDigest))
            throw InvalidTp1Argument("TP1 Ollama manifest digest must be exactly 64 lowercase hex characters.");
        var ollamaModelLayerDigest = ReadRequiredTp1Value(
            parsed,
            "--tp1-ollama-model-layer-digest");
        if (!ContinuousLearningTp1DpoConfiguration
                .IsCanonicalOllamaModelLayerDigest(ollamaModelLayerDigest))
        {
            throw InvalidTp1Argument(
                "TP1 Ollama model-layer digest must be exactly 64 lowercase hex characters.");
        }

        var baseModelId = ReadRequiredTp1Value(parsed, "--base-model-id");
        var baseModelRevision = ReadRequiredTp1Value(parsed, "--base-model-revision");
        if (!IsLogicalModelIdentity(baseModelId, maximumLength: 256, allowSlash: true)
            || !IsLogicalModelIdentity(baseModelRevision, maximumLength: 160, allowSlash: false))
        {
            throw InvalidTp1Argument("Base model identity is invalid.");
        }

        var localRetentionPolicyRef = ReadRequiredCanonicalEvidenceRef(parsed, "--local-retention-policy-ref");
        var outputLicenseRef = ReadRequiredCanonicalEvidenceRef(parsed, "--output-license-ref");
        var teacherLegalEvidenceRef = ReadRequiredCanonicalEvidenceRef(parsed, "--teacher-legal-evidence-ref");
        var teacherLicenseEvidenceRef = ReadRequiredCanonicalEvidenceRef(parsed, "--teacher-license-evidence-ref");
        var deploymentEvidenceRef = ReadRequiredCanonicalEvidenceRef(parsed, "--deployment-evidence-ref");
        var dpoDatasetAddress = ReadRepositoryOwnedDpoDatasetAddress(parsed);

        var auditSaltBase64 = parsed.AllowRandomAuditSalt
            ? Convert.ToBase64String(RandomNumberGenerator.GetBytes(AuditSaltByteCount))
            : ReadRequiredTp1Value(parsed, "--audit-salt-base64");
        if (!IsCanonicalAuditSalt(auditSaltBase64))
            throw InvalidTp1Argument("Audit salt must be canonical base64 for exactly 32 bytes.");

        var lifecycle = BuildSecretPair(
            parsed,
            "--lifecycle-auth-token-current",
            "--lifecycle-auth-token-previous",
            "--lifecycle-auth-token-previous-expires-at-utc",
            "lifecycle");
        var route = BuildSecretPair(
            parsed,
            "--route-admission-token-current",
            "--route-admission-token-previous",
            "--route-admission-token-previous-expires-at-utc",
            "route admission");
        var routeAdmissionTenantId = ReadRequiredTp1Value(parsed, "--route-admission-tenant-id");
        if (!IsCanonicalTenantId(routeAdmissionTenantId))
            throw InvalidTp1Argument("Route admission tenant id must be a canonical lowercase tenant identity.");
        var liveLocalServerCommandPath = ReadLiveLocalServerCommandPath(parsed, destination);

        var maxTeacherCalls = ReadBoundedIntOption(parsed, "--max-teacher-calls", 0, 10_000);
        var dpoMaxSteps = ReadBoundedIntOption(parsed, "--dpo-max-steps", 1, MaximumDpoStepCount);
        var measurement = new Tp1MeasurementGateThresholds(
            RequiredHeldOutQualityScore: ReadBoundedDoubleOption(
                parsed,
                "--measurement-required-held-out-quality-score",
                0d,
                1d),
            MaxRuntimeMs: ReadBoundedLongOption(
                parsed,
                "--measurement-max-runtime-ms",
                1,
                long.MaxValue),
            MaxCostUsd: ReadBoundedDecimalOption(
                parsed,
                "--measurement-max-cost-usd",
                0m,
                decimal.MaxValue),
            MaxPeakRamMb: ReadBoundedLongOption(
                parsed,
                "--measurement-max-peak-ram-mb",
                1,
                long.MaxValue));
        var training = new Tp1TrainingConfiguration(
            BatchSize: ReadBoundedIntOption(parsed, "--training-batch-size", 1, 1024),
            SeqLen: ReadBoundedIntOption(parsed, "--training-seq-len", 1, modelConfig.ContextLen),
            MaxLr: ReadBoundedFloatOption(parsed, "--training-max-lr", float.Epsilon, 1f),
            MinLr: ReadBoundedFloatOption(parsed, "--training-min-lr", 0f, 1f),
            WarmupSteps: ReadBoundedIntOption(parsed, "--training-warmup-steps", 0, 10_000_000),
            TotalSteps: ReadBoundedIntOption(parsed, "--training-total-steps", 1, 10_000_000),
            GradAccumSteps: ReadBoundedIntOption(parsed, "--training-grad-accum-steps", 1, 1024),
            GradClipNorm: ReadBoundedFloatOption(parsed, "--training-grad-clip-norm", float.Epsilon, 1_000_000f),
            WeightDecay: ReadBoundedFloatOption(parsed, "--training-weight-decay", 0f, 1_000_000f),
            CheckpointEvery: ReadBoundedIntOption(parsed, "--training-checkpoint-every", 1, 10_000_000),
            LogEvery: ReadBoundedIntOption(parsed, "--training-log-every", 1, 10_000_000));

        if (training.MinLr > training.MaxLr
            || training.WarmupSteps > training.TotalSteps
            || training.CheckpointEvery > training.TotalSteps
            || training.LogEvery > training.TotalSteps)
        {
            throw InvalidTp1Argument("Training bounds are internally inconsistent.");
        }

        return new Tp1LiveLocalBundleRequest(
            ModelId: tp1ModelId,
            OllamaManifestDigest: ollamaDigest,
            OllamaModelLayerDigest: ollamaModelLayerDigest,
            BaseModelId: baseModelId,
            BaseModelRevision: baseModelRevision,
            MaxTeacherCalls: maxTeacherCalls,
            DpoMaxStepCount: dpoMaxSteps,
            AuditSaltBase64: auditSaltBase64,
            AuditSaltGenerated: parsed.AllowRandomAuditSalt,
            LocalRetentionPolicyRef: localRetentionPolicyRef,
            OutputLicenseRef: outputLicenseRef,
            TeacherLegalEvidenceRef: teacherLegalEvidenceRef,
            TeacherLicenseEvidenceRef: teacherLicenseEvidenceRef,
            DeploymentEvidenceRef: deploymentEvidenceRef,
            DpoDatasetAddress: dpoDatasetAddress,
            LifecycleTokens: lifecycle,
            RouteAdmissionTokens: route,
            RouteAdmissionTenantId: routeAdmissionTenantId,
            LiveLocalServerCommandPath: liveLocalServerCommandPath,
            MeasurementGateThresholds: measurement,
            TrainingConfiguration: training);
    }

    private static string ReadLiveLocalServerCommandPath(
        ParsedArguments parsed,
        string destination)
    {
        var value = ReadRequiredTp1Value(parsed, "--live-local-server-command");
        string path;
        try
        {
            path = Path.GetFullPath(value);
            RejectExistingReparseSegments(path);
            var file = new FileInfo(path);
            if (!file.Exists
                || (file.Attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint)) != 0)
            {
                throw InvalidTp1Argument(
                    "Live-local server command must be an existing regular non-reparse file.");
            }
        }
        catch (BootstrapException)
        {
            throw;
        }
        catch (Exception exception) when (IsRecoverableBootstrapFailure(exception))
        {
            throw new BootstrapException(
                "BOOTSTRAP_INVALID_TP1_BUNDLE_ARGUMENT",
                "Live-local server command path is invalid or unavailable.",
                exception);
        }

        var normalizedDestination = NormalizeForBoundary(destination);
        var normalizedCommand = NormalizeForBoundary(path);
        if (StringComparer.OrdinalIgnoreCase.Equals(normalizedCommand, normalizedDestination)
            || normalizedCommand.StartsWith(
                normalizedDestination + Path.DirectorySeparatorChar,
                StringComparison.OrdinalIgnoreCase))
        {
            throw InvalidTp1Argument(
                "Live-local server command must be outside the published bundle destination.");
        }

        return path;
    }

    private static RepositoryOwnedDpoDatasetAddress ReadRepositoryOwnedDpoDatasetAddress(
        ParsedArguments parsed)
    {
        var repositoryRoot = ReadRequiredTp1Value(parsed, "--repository-root");
        var domainSlug = ReadRequiredTp1Value(parsed, "--dpo-domain-slug");
        var designPatternSlug = ReadRequiredTp1Value(
            parsed,
            "--dpo-design-pattern-slug");
        var datasetVersion = ReadRequiredTp1Value(
            parsed,
            "--dpo-dataset-version");
        var address = RepositoryOwnedTrainingArtifactLayout.CreateDpoDatasetAddress(
            repositoryRoot,
            domainSlug,
            designPatternSlug,
            datasetVersion);
        if (!address.IsSuccess || address.Value is null)
        {
            throw InvalidTp1Argument(
                "Repository-owned DPO dataset address must point to a valid repository root and safe domain/pattern/version slugs.");
        }

        return address.Value;
    }

    private static string ReadRequiredTp1Value(ParsedArguments parsed, string option)
    {
        if (!parsed.Values.TryGetValue(option, out var value)
            || string.IsNullOrWhiteSpace(value)
            || !StringComparer.Ordinal.Equals(value, value.Trim())
            || value.Any(char.IsControl))
        {
            throw new BootstrapException(
                "BOOTSTRAP_INVALID_TP1_BUNDLE_ARGUMENT",
                $"Option '{option}' is required and must be a trimmed non-control value.");
        }

        return value;
    }

    private static string ReadRequiredCanonicalEvidenceRef(ParsedArguments parsed, string option)
    {
        var value = ReadRequiredTp1Value(parsed, option);
        if (!IsCanonicalEvidenceRef(value))
            throw InvalidTp1Argument($"Option '{option}' must be a canonical sha256 evidence ref.");
        return value;
    }

    private static Tp1SecretPair BuildSecretPair(
        ParsedArguments parsed,
        string currentOption,
        string previousOption,
        string previousExpiresOption,
        string scope)
    {
        var current = ReadRequiredSecret(parsed, currentOption, scope);
        var hasPrevious = parsed.Values.TryGetValue(previousOption, out var previousRaw)
                          && !string.IsNullOrWhiteSpace(previousRaw);
        var hasExpiry = parsed.Values.TryGetValue(previousExpiresOption, out var expiryRaw)
                        && !string.IsNullOrWhiteSpace(expiryRaw);

        if (!hasPrevious && hasExpiry)
            throw InvalidTp1Argument($"{scope} previous-token expiration requires a previous token.");

        if (!hasPrevious)
            return new Tp1SecretPair(current, PreviousToken: null, PreviousTokenExpiresAtUtc: null);

        var previous = ValidateSecret(previousRaw!, previousOption, scope);
        if (!hasExpiry
            || !DateTimeOffset.TryParse(
                expiryRaw,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var expiresAt))
        {
            throw InvalidTp1Argument($"{scope} previous token requires a valid UTC expiration.");
        }

        if (StringComparer.Ordinal.Equals(current, previous))
            throw InvalidTp1Argument($"{scope} current and previous tokens must be distinct.");

        return new Tp1SecretPair(current, previous, expiresAt);
    }

    private static string ReadRequiredSecret(
        ParsedArguments parsed,
        string option,
        string scope)
    {
        if (!parsed.Values.TryGetValue(option, out var value))
            throw InvalidTp1Argument($"{scope} token option '{option}' is required.");
        return ValidateSecret(value, option, scope);
    }

    private static string ValidateSecret(string value, string option, string scope)
    {
        if (!StringComparer.Ordinal.Equals(value, value.Trim())
            || value.Length < MinimumLocalSecretLength
            || value.Length > MaximumLocalSecretLength
            || value.Any(char.IsControl))
        {
            throw InvalidTp1Argument($"{scope} token option '{option}' is invalid.");
        }

        return value;
    }

    private static int ReadBoundedIntOption(
        ParsedArguments parsed,
        string option,
        int minimum,
        int maximum)
    {
        var value = ReadRequiredTp1Value(parsed, option);
        if (!int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var parsedValue)
            || parsedValue < minimum
            || parsedValue > maximum
            || !StringComparer.Ordinal.Equals(parsedValue.ToString(CultureInfo.InvariantCulture), value))
        {
            throw InvalidTp1Argument($"Option '{option}' must be an integer in range.");
        }

        return parsedValue;
    }

    private static long ReadBoundedLongOption(
        ParsedArguments parsed,
        string option,
        long minimum,
        long maximum)
    {
        var value = ReadRequiredTp1Value(parsed, option);
        if (!long.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var parsedValue)
            || parsedValue < minimum
            || parsedValue > maximum
            || !StringComparer.Ordinal.Equals(parsedValue.ToString(CultureInfo.InvariantCulture), value))
        {
            throw InvalidTp1Argument($"Option '{option}' must be an integer in range.");
        }

        return parsedValue;
    }

    private static float ReadBoundedFloatOption(
        ParsedArguments parsed,
        string option,
        float minimum,
        float maximum)
    {
        var value = ReadRequiredTp1Value(parsed, option);
        if (!float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsedValue)
            || !float.IsFinite(parsedValue)
            || parsedValue < minimum
            || parsedValue > maximum
            || !StringComparer.Ordinal.Equals(parsedValue.ToString("R", CultureInfo.InvariantCulture), value))
        {
            throw InvalidTp1Argument($"Option '{option}' must be a finite float in canonical round-trip form.");
        }

        return parsedValue;
    }

    private static double ReadBoundedDoubleOption(
        ParsedArguments parsed,
        string option,
        double minimum,
        double maximum)
    {
        var value = ReadRequiredTp1Value(parsed, option);
        if (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsedValue)
            || !double.IsFinite(parsedValue)
            || parsedValue < minimum
            || parsedValue > maximum
            || !StringComparer.Ordinal.Equals(parsedValue.ToString("R", CultureInfo.InvariantCulture), value))
        {
            throw InvalidTp1Argument($"Option '{option}' must be a finite double in canonical round-trip form.");
        }

        return parsedValue;
    }

    private static decimal ReadBoundedDecimalOption(
        ParsedArguments parsed,
        string option,
        decimal minimum,
        decimal maximum)
    {
        var value = ReadRequiredTp1Value(parsed, option);
        if (!decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsedValue)
            || parsedValue < minimum
            || parsedValue > maximum
            || !StringComparer.Ordinal.Equals(parsedValue.ToString(CultureInfo.InvariantCulture), value))
        {
            throw InvalidTp1Argument($"Option '{option}' must be a decimal in canonical form.");
        }

        return parsedValue;
    }

    private static BootstrapException InvalidTp1Argument(string message)
        => new("BOOTSTRAP_INVALID_TP1_BUNDLE_ARGUMENT", message);

    private static bool IsCanonicalEvidenceRef(string value)
        => value.Length == 71
           && value.StartsWith("sha256:", StringComparison.Ordinal)
           && value.Skip(7).All(static character => character is >= '0' and <= '9' or >= 'a' and <= 'f');

    private static bool IsCanonicalAuditSalt(string value)
    {
        if (value.Length != 44)
            return false;

        Span<byte> decoded = stackalloc byte[AuditSaltByteCount];
        return Convert.TryFromBase64String(value, decoded, out var written)
               && written == AuditSaltByteCount
               && StringComparer.Ordinal.Equals(
                   Convert.ToBase64String(decoded.ToArray()),
                   value);
    }

    private static bool IsCanonicalTenantId(string value)
    {
        if (value.Length is < 1 or > MaximumCanonicalTenantIdLength
            || !StringComparer.Ordinal.Equals(value, value.ToLowerInvariant()))
        {
            return false;
        }

        var previousSeparator = false;
        for (var index = 0; index < value.Length; index++)
        {
            var character = value[index];
            if (character is >= 'a' and <= 'z' or >= '0' and <= '9')
            {
                previousSeparator = false;
                continue;
            }

            if (character == '-'
                && index > 0
                && index < value.Length - 1
                && !previousSeparator)
            {
                previousSeparator = true;
                continue;
            }

            return false;
        }

        return true;
    }

    private static bool IsLogicalModelIdentity(
        string value,
        int maximumLength,
        bool allowSlash)
    {
        if (value.Length > maximumLength
            || Path.IsPathRooted(value)
            || value.Contains('\\')
            || value.Contains("//", StringComparison.Ordinal)
            || value.StartsWith("./", StringComparison.Ordinal)
            || value.StartsWith("../", StringComparison.Ordinal)
            || value.EndsWith('/'))
        {
            return false;
        }

        var segments = value.Split('/');
        if (!allowSlash && segments.Length != 1)
            return false;
        if (segments.Any(static segment => segment.Length == 0 || segment is "." or ".."))
            return false;

        return value.All(character =>
            char.IsAsciiLetterOrDigit(character)
            || character is '_' or '-' or '.' or ':' or '@'
            || allowSlash && character == '/');
    }

    private static async Task<string> ReadCorpusTextAsync(
        string corpusSourcePath,
        CancellationToken cancellationToken)
    {
        try
        {
            var fileInfo = new FileInfo(corpusSourcePath);
            if (!fileInfo.Exists || fileInfo.Length <= 0)
                throw new BootstrapException(
                    "BOOTSTRAP_INVALID_CORPUS",
                    "Tokenizer corpus must be a nonempty readable file.");

            var text = await File.ReadAllTextAsync(corpusSourcePath, Encoding.UTF8, cancellationToken)
                .ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(text))
                throw new BootstrapException(
                    "BOOTSTRAP_INVALID_CORPUS",
                    "Tokenizer corpus must contain non-whitespace text.");

            return text;
        }
        catch (BootstrapException)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception) when (IsRecoverableBootstrapFailure(exception))
        {
            throw new BootstrapException(
                "BOOTSTRAP_INVALID_CORPUS",
                "Tokenizer corpus must be a nonempty readable file.",
                exception);
        }
    }

    private static async Task WriteColdArtifactsAsync(
        BootstrapRequest request,
        BootstrapArtifactPaths paths,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await CopyFileAsync(
                request.CorpusSourcePath,
                paths.CorpusPath,
                cancellationToken)
            .ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();
        var model = new LlmModel(request.Config);
        model.SaveCheckpoint(paths.CheckpointPath);

        cancellationToken.ThrowIfCancellationRequested();
        var tokenizer = new BpeTokenizer();
        tokenizer.Train(request.CorpusText, request.Config.VocabSize);
        tokenizer.Save(paths.TokenizerPath);
    }

    private static async Task CopyFileAsync(
        string sourcePath,
        string destinationPath,
        CancellationToken cancellationToken)
    {
        await using var source = new FileStream(
            sourcePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 81920,
            useAsync: true);
        await using var destination = new FileStream(
            destinationPath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 81920,
            useAsync: true);

        await source.CopyToAsync(destination, cancellationToken).ConfigureAwait(false);
        await destination.FlushAsync(cancellationToken).ConfigureAwait(false);
        destination.Flush(flushToDisk: true);
    }

    private static void ValidateProducedArtifacts(
        BootstrapArtifactPaths paths,
        ModelConfig config,
        BootstrapCliDependencies dependencies,
        CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            dependencies.BeforeValidation?.Invoke(paths);
            cancellationToken.ThrowIfCancellationRequested();

            LlmModel.ValidateFullCheckpointArtifact(
                paths.CheckpointPath,
                config,
                cancellationToken: cancellationToken);

            using var tokenizerStream = new FileStream(
                paths.TokenizerPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read);
            var loadedTokenizer = BpeTokenizer.ValidateAndLoadCanonicalArtifact(
                tokenizerStream,
                config.VocabSize,
                cancellationToken);
            if (loadedTokenizer.VocabSize != config.VocabSize)
                throw new InvalidDataException(
                    "Validated tokenizer vocabulary does not match the model config.");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new BootstrapException(
                "BOOTSTRAP_ARTIFACT_VALIDATION_FAILED",
                "Bootstrap artifacts failed production validation.",
                exception);
        }
    }

    private static Dictionary<string, Dictionary<string, object?>> BuildArtifactMetadata(
        BootstrapArtifactPaths paths)
        => new(StringComparer.Ordinal)
        {
            ["checkpoint"] = BuildFileMetadata(
                paths.CheckpointPath,
                Path.Combine(paths.Destination, "checkpoint.bin")),
            ["tokenizer"] = BuildFileMetadata(
                paths.TokenizerPath,
                Path.Combine(paths.Destination, "tokenizer.json")),
            ["tokenizer_corpus"] = BuildFileMetadata(
                paths.CorpusPath,
                Path.Combine(paths.Destination, "tokenizer-corpus.txt")),
        };

    private static Dictionary<string, object?> BuildFileMetadata(
        string sourcePath,
        string publishedPath)
    {
        var fileInfo = new FileInfo(sourcePath);
        return new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["path"] = publishedPath,
            ["sha256"] = Sha256File(sourcePath),
            ["bytes"] = fileInfo.Length,
        };
    }

    private static async Task<Tp1LiveLocalBundlePublication> WriteTp1LiveLocalBundleAsync(
        BootstrapRequest request,
        BootstrapArtifactPaths paths,
        CancellationToken cancellationToken)
    {
        var bundle = request.Tp1LiveLocalBundle
            ?? throw new InvalidOperationException("TP1 bundle request is missing.");
        var stagingConfigRoot = Path.Combine(paths.StagingDirectory, "config");
        var stagingPrivateRoot = Path.Combine(paths.StagingDirectory, "private");
        var stagingServerOwnedRoot = Path.Combine(paths.StagingDirectory, "server-owned");
        var stagingAdapterRoot = Path.Combine(stagingServerOwnedRoot, "adapters");
        var stagingCacheRoot = Path.Combine(stagingServerOwnedRoot, "cache");
        var stagingRegistryRoot = Path.Combine(stagingServerOwnedRoot, "registry");
        var stagingArtifactDbRoot = Path.Combine(stagingServerOwnedRoot, "artifact-db");
        var stagingRuntimeRoot = Path.Combine(stagingServerOwnedRoot, "runtime");
        var stagingReadyFilePath = Path.Combine(stagingRuntimeRoot, LiveLocalReadyFileName);
        var stagingMeasurementGateThresholdsPath = Path.Combine(stagingConfigRoot, MeasurementGateThresholdsFileName);
        var stagingBundleManifestPath = Path.Combine(stagingConfigRoot, Tp1BundleManifestFileName);
        var stagingEnvironmentJsonPath = Path.Combine(stagingPrivateRoot, Tp1EnvironmentJsonFileName);
        var stagingEnvironmentPowerShellPath = Path.Combine(stagingPrivateRoot, Tp1EnvironmentPowerShellFileName);

        var finalConfigRoot = Path.Combine(paths.Destination, "config");
        var finalPrivateRoot = Path.Combine(paths.Destination, "private");
        var finalServerOwnedRoot = Path.Combine(paths.Destination, "server-owned");
        var finalAdapterRoot = Path.Combine(finalServerOwnedRoot, "adapters");
        var finalCacheRoot = Path.Combine(finalServerOwnedRoot, "cache");
        var finalRegistryRoot = Path.Combine(finalServerOwnedRoot, "registry");
        var finalDpoPairRoot = bundle.DpoDatasetAddress.PairStoreRoot;
        var finalArtifactDbRoot = Path.Combine(finalServerOwnedRoot, "artifact-db");
        var finalRuntimeRoot = Path.Combine(finalServerOwnedRoot, "runtime");
        var finalReadyFilePath = Path.Combine(finalRuntimeRoot, LiveLocalReadyFileName);
        var finalMeasurementGateThresholdsPath = Path.Combine(finalConfigRoot, MeasurementGateThresholdsFileName);
        var finalBundleManifestPath = Path.Combine(finalConfigRoot, Tp1BundleManifestFileName);
        var finalEnvironmentJsonPath = Path.Combine(finalPrivateRoot, Tp1EnvironmentJsonFileName);
        var finalEnvironmentPowerShellPath = Path.Combine(finalPrivateRoot, Tp1EnvironmentPowerShellFileName);

        foreach (var path in new[]
        {
            stagingConfigRoot,
            stagingPrivateRoot,
            stagingServerOwnedRoot,
            stagingAdapterRoot,
            stagingCacheRoot,
            stagingRegistryRoot,
            stagingArtifactDbRoot,
            stagingRuntimeRoot,
            stagingReadyFilePath,
            stagingMeasurementGateThresholdsPath,
            stagingBundleManifestPath,
            stagingEnvironmentJsonPath,
            stagingEnvironmentPowerShellPath,
        })
        {
            EnsurePathUnderRoot(paths.StagingDirectory, path);
        }

        foreach (var path in new[]
        {
            finalConfigRoot,
            finalPrivateRoot,
            finalServerOwnedRoot,
            finalAdapterRoot,
            finalCacheRoot,
            finalRegistryRoot,
            finalArtifactDbRoot,
            finalRuntimeRoot,
            finalReadyFilePath,
            finalMeasurementGateThresholdsPath,
            finalBundleManifestPath,
            finalEnvironmentJsonPath,
            finalEnvironmentPowerShellPath,
        })
        {
            EnsurePathUnderRoot(paths.Destination, path);
        }
        EnsurePathUnderRoot(
            bundle.DpoDatasetAddress.RepositoryRoot,
            finalDpoPairRoot);

        var continuousLearningEnvironmentValues = BuildContinuousLearningEnvironmentValues(
            request,
            paths,
            finalAdapterRoot,
            finalCacheRoot,
            finalRegistryRoot,
            finalDpoPairRoot);
        var launchEnvironmentValues = BuildLaunchEnvironmentValues(
            bundle,
            continuousLearningEnvironmentValues);
        var serverArguments = BuildLiveLocalServerArguments(
            finalReadyFilePath,
            finalArtifactDbRoot,
            finalMeasurementGateThresholdsPath);
        var publicManifestSummary = BuildTp1PublicManifestSummary(
            request,
            finalBundleManifestPath,
            finalEnvironmentJsonPath,
            finalEnvironmentPowerShellPath,
            finalMeasurementGateThresholdsPath,
            finalAdapterRoot,
            finalCacheRoot,
            finalRegistryRoot,
            finalDpoPairRoot,
            finalArtifactDbRoot,
            finalReadyFilePath);

        Directory.CreateDirectory(stagingConfigRoot);
        Directory.CreateDirectory(stagingPrivateRoot);
        Directory.CreateDirectory(stagingAdapterRoot);
        Directory.CreateDirectory(stagingCacheRoot);
        Directory.CreateDirectory(stagingRegistryRoot);
        Directory.CreateDirectory(stagingArtifactDbRoot);
        Directory.CreateDirectory(stagingRuntimeRoot);
        await WriteJsonFileAsync(
                stagingMeasurementGateThresholdsPath,
                BuildMeasurementGateThresholdsJson(bundle.MeasurementGateThresholds),
                cancellationToken)
            .ConfigureAwait(false);
        await WriteJsonFileAsync(
                stagingBundleManifestPath,
                BuildTp1PublicBundleManifest(
                    request,
                    launchEnvironmentValues.Keys.ToArray(),
                    publicManifestSummary),
                cancellationToken)
            .ConfigureAwait(false);
        await WriteJsonFileAsync(
                stagingEnvironmentJsonPath,
                BuildTp1PrivateEnvironmentArtifact(
                    request,
                    launchEnvironmentValues,
                    serverArguments,
                    finalBundleManifestPath,
                    finalMeasurementGateThresholdsPath),
                cancellationToken)
            .ConfigureAwait(false);
        await File.WriteAllTextAsync(
                stagingEnvironmentPowerShellPath,
                BuildTp1PowerShellLaunchArtifact(),
                Encoding.UTF8,
                cancellationToken)
            .ConfigureAwait(false);

        return new Tp1LiveLocalBundlePublication(
            FinalBundleManifestPath: finalBundleManifestPath,
            FinalEnvironmentJsonPath: finalEnvironmentJsonPath,
            FinalEnvironmentPowerShellPath: finalEnvironmentPowerShellPath,
            FinalMeasurementGateThresholdsPath: finalMeasurementGateThresholdsPath,
            FinalAdapterRoot: finalAdapterRoot,
            FinalCacheRoot: finalCacheRoot,
            FinalRegistryRoot: finalRegistryRoot,
            FinalRepositoryRoot: bundle.DpoDatasetAddress.RepositoryRoot,
            FinalDpoPairRoot: finalDpoPairRoot,
            FinalArtifactDbRoot: finalArtifactDbRoot,
            FinalReadyFilePath: finalReadyFilePath,
            PublicManifestSummary: publicManifestSummary);
    }

    private static SortedDictionary<string, string> BuildContinuousLearningEnvironmentValues(
        BootstrapRequest request,
        BootstrapArtifactPaths paths,
        string finalAdapterRoot,
        string finalCacheRoot,
        string finalRegistryRoot,
        string finalDpoPairRoot)
    {
        var bundle = request.Tp1LiveLocalBundle
            ?? throw new InvalidOperationException("TP1 bundle request is missing.");

        var values = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["XIIGEN_CONTINUOUS_LEARNING_COMPOSITION_ENABLED"] = bool.TrueString,
            ["XIIGEN_CONTINUOUS_LEARNING_TP1_MODEL_ID"] = bundle.ModelId,
            ["XIIGEN_CONTINUOUS_LEARNING_TP1_OLLAMA_MANIFEST_DIGEST"] = bundle.OllamaManifestDigest,
            ["XIIGEN_CONTINUOUS_LEARNING_TP1_OLLAMA_MODEL_LAYER_DIGEST"] = bundle.OllamaModelLayerDigest,
            ["XIIGEN_CONTINUOUS_LEARNING_BASE_MODEL_ID"] = bundle.BaseModelId,
            ["XIIGEN_CONTINUOUS_LEARNING_BASE_MODEL_REVISION"] = bundle.BaseModelRevision,
            ["XIIGEN_CONTINUOUS_LEARNING_MAX_TEACHER_CALLS"] = bundle.MaxTeacherCalls.ToString(CultureInfo.InvariantCulture),
            ["XIIGEN_CONTINUOUS_LEARNING_DPO_MAX_STEPS"] = bundle.DpoMaxStepCount.ToString(CultureInfo.InvariantCulture),
            ["XIIGEN_CONTINUOUS_LEARNING_AUDIT_SALT_BASE64"] = bundle.AuditSaltBase64,
            ["XIIGEN_CONTINUOUS_LEARNING_TP1_LOCAL_RETENTION_POLICY_REF"] = bundle.LocalRetentionPolicyRef,
            ["XIIGEN_CONTINUOUS_LEARNING_TP1_OUTPUT_LICENSE_REF"] = bundle.OutputLicenseRef,
            ["XIIGEN_CONTINUOUS_LEARNING_BASE_CHECKPOINT"] = Path.Combine(paths.Destination, "checkpoint.bin"),
            ["XIIGEN_CONTINUOUS_LEARNING_TOKENIZER"] = Path.Combine(paths.Destination, "tokenizer.json"),
            ["XIIGEN_CONTINUOUS_LEARNING_ADAPTER_ROOT"] = finalAdapterRoot,
            ["XIIGEN_CONTINUOUS_LEARNING_CACHE_ROOT"] = finalCacheRoot,
            ["XIIGEN_CONTINUOUS_LEARNING_REGISTRY_ROOT"] = finalRegistryRoot,
            ["XIIGEN_CONTINUOUS_LEARNING_REPOSITORY_ROOT"] = bundle.DpoDatasetAddress.RepositoryRoot,
            ["XIIGEN_CONTINUOUS_LEARNING_DPO_PAIR_ROOT"] = finalDpoPairRoot,
            ["XIIGEN_CONTINUOUS_LEARNING_MODEL_VOCAB_SIZE"] = request.Config.VocabSize.ToString(CultureInfo.InvariantCulture),
            ["XIIGEN_CONTINUOUS_LEARNING_MODEL_CONTEXT_LEN"] = request.Config.ContextLen.ToString(CultureInfo.InvariantCulture),
            ["XIIGEN_CONTINUOUS_LEARNING_MODEL_HIDDEN_DIM"] = request.Config.HiddenDim.ToString(CultureInfo.InvariantCulture),
            ["XIIGEN_CONTINUOUS_LEARNING_MODEL_NUM_LAYERS"] = request.Config.NumLayers.ToString(CultureInfo.InvariantCulture),
            ["XIIGEN_CONTINUOUS_LEARNING_MODEL_NUM_HEADS"] = request.Config.NumHeads.ToString(CultureInfo.InvariantCulture),
            ["XIIGEN_CONTINUOUS_LEARNING_MODEL_NUM_KV_HEADS"] = request.Config.NumKvHeads.ToString(CultureInfo.InvariantCulture),
            ["XIIGEN_CONTINUOUS_LEARNING_MODEL_FFN_HIDDEN_DIM"] = request.Config.FfnHiddenDim.ToString(CultureInfo.InvariantCulture),
            ["XIIGEN_CONTINUOUS_LEARNING_MODEL_ROPE_THETA"] = request.Config.RopeTheta.ToString("R", CultureInfo.InvariantCulture),
            ["XIIGEN_CONTINUOUS_LEARNING_MODEL_RMS_NORM_EPS"] = request.Config.RmsNormEps.ToString("R", CultureInfo.InvariantCulture),
            ["XIIGEN_CONTINUOUS_LEARNING_TRAINING_BATCH_SIZE"] = bundle.TrainingConfiguration.BatchSize.ToString(CultureInfo.InvariantCulture),
            ["XIIGEN_CONTINUOUS_LEARNING_TRAINING_SEQ_LEN"] = bundle.TrainingConfiguration.SeqLen.ToString(CultureInfo.InvariantCulture),
            ["XIIGEN_CONTINUOUS_LEARNING_TRAINING_MAX_LR"] = bundle.TrainingConfiguration.MaxLr.ToString("R", CultureInfo.InvariantCulture),
            ["XIIGEN_CONTINUOUS_LEARNING_TRAINING_MIN_LR"] = bundle.TrainingConfiguration.MinLr.ToString("R", CultureInfo.InvariantCulture),
            ["XIIGEN_CONTINUOUS_LEARNING_TRAINING_WARMUP_STEPS"] = bundle.TrainingConfiguration.WarmupSteps.ToString(CultureInfo.InvariantCulture),
            ["XIIGEN_CONTINUOUS_LEARNING_TRAINING_TOTAL_STEPS"] = bundle.TrainingConfiguration.TotalSteps.ToString(CultureInfo.InvariantCulture),
            ["XIIGEN_CONTINUOUS_LEARNING_TRAINING_GRAD_ACCUM_STEPS"] = bundle.TrainingConfiguration.GradAccumSteps.ToString(CultureInfo.InvariantCulture),
            ["XIIGEN_CONTINUOUS_LEARNING_TRAINING_GRAD_CLIP_NORM"] = bundle.TrainingConfiguration.GradClipNorm.ToString("R", CultureInfo.InvariantCulture),
            ["XIIGEN_CONTINUOUS_LEARNING_TRAINING_WEIGHT_DECAY"] = bundle.TrainingConfiguration.WeightDecay.ToString("R", CultureInfo.InvariantCulture),
            ["XIIGEN_CONTINUOUS_LEARNING_TRAINING_CHECKPOINT_EVERY"] = bundle.TrainingConfiguration.CheckpointEvery.ToString(CultureInfo.InvariantCulture),
            ["XIIGEN_CONTINUOUS_LEARNING_TRAINING_LOG_EVERY"] = bundle.TrainingConfiguration.LogEvery.ToString(CultureInfo.InvariantCulture),
        };
        return values;
    }

    private static SortedDictionary<string, string> BuildLaunchEnvironmentValues(
        Tp1LiveLocalBundleRequest bundle,
        SortedDictionary<string, string> continuousLearningEnvironmentValues)
    {
        var values = new SortedDictionary<string, string>(
            continuousLearningEnvironmentValues,
            StringComparer.Ordinal)
        {
            [ModelLifecycleCurrentTokenEnvironmentName] = bundle.LifecycleTokens.CurrentToken,
            [ModelLifecyclePreviousTokenEnvironmentName] = bundle.LifecycleTokens.PreviousToken ?? string.Empty,
            [ModelLifecyclePreviousTokenExpiresEnvironmentName] =
                FormatNullableInstant(bundle.LifecycleTokens.PreviousTokenExpiresAtUtc) ?? string.Empty,
            [RouteAdmissionEnabledEnvironmentName] = bool.TrueString,
            [RouteAdmissionCurrentTokenEnvironmentName] = bundle.RouteAdmissionTokens.CurrentToken,
            [RouteAdmissionPreviousTokenEnvironmentName] = bundle.RouteAdmissionTokens.PreviousToken ?? string.Empty,
            [RouteAdmissionPreviousTokenExpiresEnvironmentName] =
                FormatNullableInstant(bundle.RouteAdmissionTokens.PreviousTokenExpiresAtUtc) ?? string.Empty,
            [RouteAdmissionTenantIdEnvironmentName] = bundle.RouteAdmissionTenantId,
        };
        return values;
    }

    private static string[] BuildLiveLocalServerArguments(
        string finalReadyFilePath,
        string finalArtifactDbRoot,
        string finalMeasurementGateThresholdsPath)
        =>
        [
            "--ready-file",
            finalReadyFilePath,
            "--artifact-db-root",
            finalArtifactDbRoot,
            "--measurement-gate-thresholds-file",
            finalMeasurementGateThresholdsPath,
            "--machine-backed-local",
        ];

    private static SortedDictionary<string, object?> BuildMeasurementGateThresholdsJson(
        Tp1MeasurementGateThresholds thresholds)
        => new(StringComparer.Ordinal)
        {
            ["required_held_out_quality_score"] = thresholds.RequiredHeldOutQualityScore,
            ["max_runtime_ms"] = thresholds.MaxRuntimeMs,
            ["max_cost_usd"] = thresholds.MaxCostUsd,
            ["max_peak_ram_mb"] = thresholds.MaxPeakRamMb,
        };

    private static Dictionary<string, object?> BuildTp1PublicManifestSummary(
        BootstrapRequest request,
        string finalBundleManifestPath,
        string finalEnvironmentJsonPath,
        string finalEnvironmentPowerShellPath,
        string finalMeasurementGateThresholdsPath,
        string finalAdapterRoot,
        string finalCacheRoot,
        string finalRegistryRoot,
        string finalDpoPairRoot,
        string finalArtifactDbRoot,
        string finalReadyFilePath)
    {
        var bundle = request.Tp1LiveLocalBundle
            ?? throw new InvalidOperationException("TP1 bundle request is missing.");
        return new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["artifact_kind"] = Tp1LiveLocalBundleArtifactKind,
            ["bundle_profile"] = Tp1LiveLocalBundleProfile,
            ["training_not_started"] = true,
            ["teacher_calls_executed"] = 0,
            ["dpo_steps_executed"] = 0,
            ["weights_not_trained"] = true,
            ["deployment_product_readiness"] = false,
            ["tp1_model_id"] = bundle.ModelId,
            ["tp1_ollama_manifest_digest"] = bundle.OllamaManifestDigest,
            ["tp1_ollama_model_layer_digest"] = bundle.OllamaModelLayerDigest,
            ["base_model_id"] = bundle.BaseModelId,
            ["base_model_revision"] = bundle.BaseModelRevision,
            ["local_retention_policy_ref"] = bundle.LocalRetentionPolicyRef,
            ["output_license_ref"] = bundle.OutputLicenseRef,
            ["teacher_legal_evidence_ref"] = bundle.TeacherLegalEvidenceRef,
            ["teacher_license_evidence_ref"] = bundle.TeacherLicenseEvidenceRef,
            ["deployment_evidence_ref"] = bundle.DeploymentEvidenceRef,
            ["repository_root"] = bundle.DpoDatasetAddress.RepositoryRoot,
            ["dpo_dataset_root"] = bundle.DpoDatasetAddress.DatasetRoot,
            ["dpo_dataset_root_relative_path"] = bundle.DpoDatasetAddress.DatasetRootRelativePath,
            ["dpo_manifest_path"] = bundle.DpoDatasetAddress.ManifestPath,
            ["dpo_manifest_relative_path"] = bundle.DpoDatasetAddress.ManifestRelativePath,
            ["audit_salt_configured"] = true,
            ["audit_salt_generated"] = bundle.AuditSaltGenerated,
            ["lifecycle_token_configured"] = true,
            ["lifecycle_previous_token_configured"] = bundle.LifecycleTokens.PreviousToken is not null,
            ["route_admission_token_configured"] = true,
            ["route_admission_previous_token_configured"] = bundle.RouteAdmissionTokens.PreviousToken is not null,
            ["bundle_manifest_path"] = finalBundleManifestPath,
            ["operator_launch_environment_json_path"] = finalEnvironmentJsonPath,
            ["operator_launch_environment_powershell_path"] = finalEnvironmentPowerShellPath,
            ["measurement_gate_thresholds_path"] = finalMeasurementGateThresholdsPath,
            ["adapter_root"] = finalAdapterRoot,
            ["cache_root"] = finalCacheRoot,
            ["registry_root"] = finalRegistryRoot,
            ["dpo_pair_root"] = finalDpoPairRoot,
            ["artifact_db_root"] = finalArtifactDbRoot,
            ["ready_file_path"] = finalReadyFilePath,
            ["live_local_server_command_configured"] = true,
            ["filesystem_safety_warning"] =
                "Pre-existing reparse segments are blocked; hardening against a hostile concurrent local reparse race is not proven.",
            ["private_values_redacted"] = true,
        };
    }

    private static Dictionary<string, object?> BuildTp1PublicBundleManifest(
        BootstrapRequest request,
        IReadOnlyCollection<string> environmentKeys,
        IReadOnlyDictionary<string, object?> publicManifestSummary)
    {
        var bundle = request.Tp1LiveLocalBundle
            ?? throw new InvalidOperationException("TP1 bundle request is missing.");
        return new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["artifact_kind"] = Tp1LiveLocalBundleArtifactKind,
            ["bundle_profile"] = Tp1LiveLocalBundleProfile,
            ["training_not_started"] = true,
            ["teacher_calls_executed"] = 0,
            ["dpo_steps_executed"] = 0,
            ["weights_not_trained"] = true,
            ["deployment_product_readiness"] = false,
            ["launch_environment_keys"] = environmentKeys.OrderBy(static key => key, StringComparer.Ordinal).ToArray(),
            ["continuous_learning_environment_keys"] = environmentKeys
                .Where(static key => key.StartsWith("XIIGEN_CONTINUOUS_LEARNING_", StringComparison.Ordinal))
                .OrderBy(static key => key, StringComparer.Ordinal)
                .ToArray(),
            ["teacher_runtime"] = new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["configured_model_family_hint"] = "qwen",
                ["configured_hosting_provider_hint"] = "ollama-local",
                ["descriptive_only_not_legal_proof"] = true,
                ["legal_claim_source"] = "operator_evidence_refs_only",
                ["teacher_legal_evidence_ref"] = bundle.TeacherLegalEvidenceRef,
                ["teacher_license_evidence_ref"] = bundle.TeacherLicenseEvidenceRef,
            },
            ["operator_owned_claims"] = new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["tp1_model_id"] = bundle.ModelId,
                ["tp1_ollama_manifest_digest"] = bundle.OllamaManifestDigest,
                ["tp1_ollama_model_layer_digest"] = bundle.OllamaModelLayerDigest,
                ["base_model_id"] = bundle.BaseModelId,
                ["base_model_revision"] = bundle.BaseModelRevision,
                ["local_retention_policy_ref"] = bundle.LocalRetentionPolicyRef,
                ["output_license_ref"] = bundle.OutputLicenseRef,
                ["teacher_legal_evidence_ref"] = bundle.TeacherLegalEvidenceRef,
                ["teacher_license_evidence_ref"] = bundle.TeacherLicenseEvidenceRef,
                ["deployment_evidence_ref"] = bundle.DeploymentEvidenceRef,
                ["audit_salt_configured"] = true,
                ["audit_salt_generated"] = bundle.AuditSaltGenerated,
                ["lifecycle_token_configured"] = true,
                ["lifecycle_previous_token_configured"] = bundle.LifecycleTokens.PreviousToken is not null,
                ["route_admission_token_configured"] = true,
                ["route_admission_previous_token_configured"] = bundle.RouteAdmissionTokens.PreviousToken is not null,
            },
            ["paths"] = publicManifestSummary,
            ["private_artifacts"] = new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["contains_secrets"] = true,
                ["raw_values_redacted"] = true,
                ["operator_launch_environment_json_path"] = publicManifestSummary["operator_launch_environment_json_path"],
                ["operator_launch_environment_powershell_path"] = publicManifestSummary["operator_launch_environment_powershell_path"],
            },
        };
    }

    private static Dictionary<string, object?> BuildTp1PrivateEnvironmentArtifact(
        BootstrapRequest request,
        IReadOnlyDictionary<string, string> environmentValues,
        IReadOnlyList<string> serverArguments,
        string finalBundleManifestPath,
        string finalMeasurementGateThresholdsPath)
    {
        var bundle = request.Tp1LiveLocalBundle
            ?? throw new InvalidOperationException("TP1 bundle request is missing.");
        return new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["artifact_kind"] = "continuous_learning_tp1_live_local_launch_environment",
            ["bundle_profile"] = Tp1LiveLocalBundleProfile,
            ["contains_private_values"] = true,
            ["public_bundle_manifest_path"] = finalBundleManifestPath,
            ["measurement_gate_thresholds_path"] = finalMeasurementGateThresholdsPath,
            ["server_command_path"] = bundle.LiveLocalServerCommandPath,
            ["server_arguments"] = serverArguments,
            ["environment_variables"] = environmentValues,
            ["safety_state"] = new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["training_not_started"] = true,
                ["teacher_calls_executed"] = 0,
                ["dpo_steps_executed"] = 0,
                ["weights_not_trained"] = true,
                ["deployment_product_readiness"] = false,
            },
        };
    }

    private static string BuildTp1PowerShellLaunchArtifact()
    {
        var builder = new StringBuilder();
        builder.AppendLine("# TP1 live-local launcher. Private values are read only from the adjacent JSON artifact.");
        builder.AppendLine("Set-StrictMode -Version Latest");
        builder.AppendLine("$ErrorActionPreference = 'Stop'");
        builder.AppendLine("$isWindowsHost = [System.IO.Path]::DirectorySeparatorChar -eq '\\'");
        builder.AppendLine("function Assert-XiigenSafeWslDistroName([string]$Name) {");
        builder.AppendLine("    if ([string]::IsNullOrWhiteSpace($Name)) { throw 'WSL distro name is required to convert Linux paths for Windows PowerShell.' }");
        builder.AppendLine("    if ($Name -eq '.' -or $Name -eq '..' -or $Name -match '[\\/:*?\"\"<>|]' -or $Name -match '[\\x00-\\x1f]') {");
        builder.AppendLine("        throw \"WSL distro name contains an unsafe UNC share character: $Name\"");
        builder.AppendLine("    }");
        builder.AppendLine("    return $Name");
        builder.AppendLine("}");
        builder.AppendLine("function Get-XiigenWslDistroName {");
        builder.AppendLine("    $fromEnvironment = [Environment]::GetEnvironmentVariable('XIIGEN_WSL_DISTRO_NAME', 'Process')");
        builder.AppendLine("    if (-not [string]::IsNullOrWhiteSpace($fromEnvironment)) { return Assert-XiigenSafeWslDistroName $fromEnvironment }");
        builder.AppendLine("    if ($PSScriptRoot -match '^\\\\\\\\wsl(?:\\.localhost|\\$)\\\\([^\\\\]+)\\\\') { return Assert-XiigenSafeWslDistroName $Matches[1] }");
        builder.AppendLine("    throw 'WSL distro name is missing; Linux paths cannot be converted for Windows PowerShell without guessing.'");
        builder.AppendLine("}");
        builder.AppendLine("function Convert-XiigenKnownPathForCurrentHost([string]$Value, [string]$Description) {");
        builder.AppendLine("    if ([string]::IsNullOrWhiteSpace($Value)) { throw \"Known path value is empty: $Description\" }");
        builder.AppendLine("    if (-not $isWindowsHost) { return $Value }");
        builder.AppendLine("    if ($Value -match '^[A-Za-z]:[\\\\/]') { return $Value }");
        builder.AppendLine("    if ($Value.StartsWith('\\\\')) { return $Value }");
        builder.AppendLine("    if ($Value -match '^/mnt/([A-Za-z])(?:/(.*))?$') {");
        builder.AppendLine("        $drive = ([string]$Matches[1]).ToUpperInvariant()");
        builder.AppendLine("        $suffix = [string]$Matches[2]");
        builder.AppendLine("        if ([string]::IsNullOrEmpty($suffix)) { return $drive + ':\\' }");
        builder.AppendLine("        return $drive + ':\\' + ($suffix -replace '/', '\\')");
        builder.AppendLine("    }");
        builder.AppendLine("    if ($Value.StartsWith('/')) {");
        builder.AppendLine("        $distro = Get-XiigenWslDistroName");
        builder.AppendLine("        return '\\\\wsl.localhost\\' + $distro + ($Value -replace '/', '\\')");
        builder.AppendLine("    }");
        builder.AppendLine("    throw \"Known path value is not absolute for Windows PowerShell: $Description = $Value\"");
        builder.AppendLine("}");
        builder.AppendLine("$environmentFile = Join-Path -Path $PSScriptRoot -ChildPath 'tp1-live-local-environment.json'");
        builder.AppendLine("$environmentJson = Get-Content -LiteralPath $environmentFile -Raw");
        builder.AppendLine("if ((Get-Command ConvertFrom-Json).Parameters.ContainsKey('DateKind')) {");
        builder.AppendLine("    $launch = $environmentJson | ConvertFrom-Json -DateKind String");
        builder.AppendLine("} else {");
        builder.AppendLine("    $launch = $environmentJson | ConvertFrom-Json");
        builder.AppendLine("}");
        builder.AppendLine("$pathEnvironmentNames = @(");
        builder.AppendLine("    'XIIGEN_CONTINUOUS_LEARNING_BASE_CHECKPOINT',");
        builder.AppendLine("    'XIIGEN_CONTINUOUS_LEARNING_TOKENIZER',");
        builder.AppendLine("    'XIIGEN_CONTINUOUS_LEARNING_ADAPTER_ROOT',");
        builder.AppendLine("    'XIIGEN_CONTINUOUS_LEARNING_CACHE_ROOT',");
        builder.AppendLine("    'XIIGEN_CONTINUOUS_LEARNING_REGISTRY_ROOT',");
        builder.AppendLine("    'XIIGEN_CONTINUOUS_LEARNING_REPOSITORY_ROOT',");
        builder.AppendLine("    'XIIGEN_CONTINUOUS_LEARNING_DPO_PAIR_ROOT'");
        builder.AppendLine(")");
        builder.AppendLine("foreach ($entry in $launch.environment_variables.PSObject.Properties) {");
        builder.AppendLine("    $value = [string]$entry.Value");
        builder.AppendLine("    if ($pathEnvironmentNames -contains $entry.Name) {");
        builder.AppendLine("        $value = Convert-XiigenKnownPathForCurrentHost $value $entry.Name");
        builder.AppendLine("    }");
        builder.AppendLine("    [Environment]::SetEnvironmentVariable($entry.Name, $value, [EnvironmentVariableTarget]::Process)");
        builder.AppendLine("}");
        builder.AppendLine("$serverCommand = Convert-XiigenKnownPathForCurrentHost ([string]$launch.server_command_path) 'server_command_path'");
        builder.AppendLine("$pathArgumentFlags = @('--ready-file', '--artifact-db-root', '--measurement-gate-thresholds-file')");
        builder.AppendLine("$serverArguments = @()");
        builder.AppendLine("$convertNextArgument = $false");
        builder.AppendLine("$previousFlag = $null");
        builder.AppendLine("foreach ($argument in @($launch.server_arguments | ForEach-Object { [string]$_ })) {");
        builder.AppendLine("    if ($convertNextArgument) {");
        builder.AppendLine("        $serverArguments += Convert-XiigenKnownPathForCurrentHost $argument \"argument after $previousFlag\"");
        builder.AppendLine("        $convertNextArgument = $false");
        builder.AppendLine("        $previousFlag = $null");
        builder.AppendLine("        continue");
        builder.AppendLine("    }");
        builder.AppendLine("    $serverArguments += $argument");
        builder.AppendLine("    if ($pathArgumentFlags -contains $argument) {");
        builder.AppendLine("        $convertNextArgument = $true");
        builder.AppendLine("        $previousFlag = $argument");
        builder.AppendLine("    }");
        builder.AppendLine("}");
        builder.AppendLine("if ($convertNextArgument) { throw \"Path argument flag has no following value: $previousFlag\" }");
        builder.AppendLine("& $serverCommand @serverArguments");
        builder.AppendLine("if ($null -ne $LASTEXITCODE) { exit $LASTEXITCODE }");
        return builder.ToString();
    }

    private static async Task WriteJsonFileAsync(
        string path,
        object value,
        CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(
            path,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 81920,
            useAsync: true);
        await JsonSerializer.SerializeAsync(stream, value, JsonOptions, cancellationToken)
            .ConfigureAwait(false);
        await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
        stream.Flush(flushToDisk: true);
    }

    private static string? FormatNullableInstant(DateTimeOffset? value)
        => value is null ? null : FormatInstant(value.Value);

    private static string FormatInstant(DateTimeOffset value)
        => value.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture);

    private static async Task WriteManifestAsync(
        BootstrapRequest request,
        BootstrapArtifactPaths paths,
        IReadOnlyDictionary<string, Dictionary<string, object?>> artifactMetadata,
        Tp1LiveLocalBundlePublication? tp1LiveLocalBundle,
        CancellationToken cancellationToken)
    {
        var manifest = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["artifact_kind"] = ArtifactKind,
            ["is_trained"] = false,
            ["semantic_quality_claimed"] = false,
            ["production_behavior_ready"] = false,
            ["model_id"] = request.ModelId,
            ["revision"] = request.Revision,
            ["profile"] = request.Profile,
            ["destination"] = paths.Destination,
            ["source_commit"] = ResolveSourceCommit(),
            ["model_config"] = BuildModelConfigManifest(request.Config),
            ["checkpoint"] = artifactMetadata["checkpoint"],
            ["tokenizer"] = artifactMetadata["tokenizer"],
            ["tokenizer_corpus"] = artifactMetadata["tokenizer_corpus"],
            ["runtime"] = BuildRuntimeManifest(),
            ["environment"] = BuildEnvironmentManifest(),
        };
        if (tp1LiveLocalBundle is not null)
            manifest["tp1_live_local_bundle"] = tp1LiveLocalBundle.PublicManifestSummary;

        await using var stream = new FileStream(
            paths.ManifestPath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 81920,
            useAsync: true);
        await JsonSerializer.SerializeAsync(stream, manifest, JsonOptions, cancellationToken)
            .ConfigureAwait(false);
        await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
        stream.Flush(flushToDisk: true);
    }

    private static Dictionary<string, object?> BuildModelConfigManifest(ModelConfig config)
        => new(StringComparer.Ordinal)
        {
            ["vocab_size"] = config.VocabSize,
            ["context_len"] = config.ContextLen,
            ["hidden_dim"] = config.HiddenDim,
            ["num_layers"] = config.NumLayers,
            ["num_heads"] = config.NumHeads,
            ["num_kv_heads"] = config.NumKvHeads,
            ["ffn_hidden_dim"] = config.FfnHiddenDim,
            ["rope_theta"] = config.RopeTheta,
            ["rms_norm_eps"] = config.RmsNormEps,
            ["head_dim"] = config.HeadDim,
            ["kv_group_size"] = config.KvGroupSize,
        };

    private static Dictionary<string, object?> BuildRuntimeManifest()
        => new(StringComparer.Ordinal)
        {
            ["framework_description"] = RuntimeInformation.FrameworkDescription,
            ["os_description"] = RuntimeInformation.OSDescription,
            ["process_architecture"] = RuntimeInformation.ProcessArchitecture.ToString(),
            ["entry_assembly"] = Assembly.GetEntryAssembly()?.GetName().Name ?? "unknown",
        };

    private static Dictionary<string, object?> BuildEnvironmentManifest()
        => new(StringComparer.Ordinal)
        {
            ["current_directory"] = Directory.GetCurrentDirectory(),
            ["machine_name"] = Environment.MachineName,
            ["user_interactive"] = Environment.UserInteractive,
        };

    private static void CreateRepositoryOwnedDirectoryChain(
        string repositoryRoot,
        string targetDirectory,
        ICollection<string> createdDirectories,
        Action<string>? afterDirectoryCreated)
    {
        var canonicalRepositoryRoot = Path.TrimEndingDirectorySeparator(
            Path.GetFullPath(repositoryRoot));
        var canonicalTarget = Path.TrimEndingDirectorySeparator(
            Path.GetFullPath(targetDirectory));
        EnsurePathUnderRoot(canonicalRepositoryRoot, canonicalTarget);

        var missing = new Stack<string>();
        for (var current = canonicalTarget;
             !Directory.Exists(current);
             current = Path.GetDirectoryName(current)
                 ?? throw new InvalidDataException(
                     "Repository-owned DPO directory parent could not be resolved."))
        {
            if (string.Equals(
                    current,
                    canonicalRepositoryRoot,
                    OperatingSystem.IsWindows()
                        ? StringComparison.OrdinalIgnoreCase
                        : StringComparison.Ordinal))
            {
                throw new InvalidDataException(
                    "Repository root must exist before DPO directory creation.");
            }

            missing.Push(current);
        }

        while (missing.TryPop(out var directory))
        {
            // WHY: another process can create a segment between the initial
            // scan and this step. A segment already present at the immediate
            // pre-create check is foreign and never becomes rollback-owned.
            if (Directory.Exists(directory))
                continue;
            Directory.CreateDirectory(directory);
            createdDirectories.Add(directory);
            afterDirectoryCreated?.Invoke(directory);
        }
    }

    private static void DeleteEmptyDirectoriesCreatedByThisCall(
        IReadOnlyList<string> createdDirectories,
        string? repositoryRoot)
    {
        if (createdDirectories.Count == 0 || string.IsNullOrWhiteSpace(repositoryRoot))
            return;

        var canonicalRepositoryRoot = Path.TrimEndingDirectorySeparator(
            Path.GetFullPath(repositoryRoot));
        for (var index = createdDirectories.Count - 1; index >= 0; index--)
        {
            var directory = Path.TrimEndingDirectorySeparator(
                Path.GetFullPath(createdDirectories[index]));
            // WHY: rollback owns only directories created by this invocation;
            // it stops at the repository and preserves any foreign/non-empty data.
            EnsurePathUnderRoot(canonicalRepositoryRoot, directory);
            if (!Directory.Exists(directory)
                || Directory.EnumerateFileSystemEntries(directory).Any())
            {
                break;
            }

            Directory.Delete(directory, recursive: false);
        }
    }

    private static void PublishStagingDirectory(
        string stagingDirectory,
        string destination,
        bool force,
        CancellationToken cancellationToken,
        BootstrapCliDependencies dependencies)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!force || !Directory.Exists(destination))
            {
                Directory.Move(stagingDirectory, destination);
                return;
            }

            var backupDirectory = $"{destination}.bootstrap-backup-{Guid.NewGuid():N}";
            var existingMovedToBackup = false;
            var newDestinationPublished = false;

            try
            {
                Directory.Move(destination, backupDirectory);
                existingMovedToBackup = true;

                cancellationToken.ThrowIfCancellationRequested();
                Directory.Move(stagingDirectory, destination);
                newDestinationPublished = true;

                dependencies.AfterForceDestinationPublishedBeforeBackupCleanup?.Invoke(
                    new BootstrapForcePublicationPaths(
                        Destination: destination,
                        BackupDirectory: backupDirectory,
                        StagingDirectory: stagingDirectory));

                Directory.Delete(backupDirectory, recursive: true);
                existingMovedToBackup = false;
            }
            catch (Exception publicationException)
            {
                if (existingMovedToBackup)
                    RestoreBackupAfterFailedForcePublication(
                        destination,
                        backupDirectory,
                        newDestinationPublished,
                        publicationException);
                throw;
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception) when (IsRecoverableBootstrapFailure(exception))
        {
            throw new BootstrapException(
                "BOOTSTRAP_PUBLICATION_FAILED",
                "Bootstrap artifacts could not be published atomically.",
                exception);
        }
    }

    private static void RestoreBackupAfterFailedForcePublication(
        string destination,
        string backupDirectory,
        bool deletePublishedDestination,
        Exception publicationException)
    {
        try
        {
            if (!Directory.Exists(backupDirectory))
                throw new IOException(
                    "Force replacement backup directory is missing; previous destination cannot be restored.");

            if (deletePublishedDestination)
                DeleteDirectoryIfExists(destination);

            if (Directory.Exists(destination))
                throw new IOException(
                    "Destination path still exists; previous destination cannot be restored.");

            Directory.Move(backupDirectory, destination);
        }
        catch (Exception restoreException) when (IsRecoverableBootstrapFailure(restoreException))
        {
            throw new BootstrapException(
                "BOOTSTRAP_PUBLICATION_FAILED",
                "Bootstrap force replacement failed and the previous destination could not be restored.",
                new AggregateException(publicationException, restoreException));
        }
    }

    private static void RejectUnderProjects(string path)
    {
        var projectsRoot = NormalizeForBoundary(Path.GetFullPath(ProjectsRootLiteral));
        var candidate = NormalizeForBoundary(Path.GetFullPath(path));

        if (StringComparer.OrdinalIgnoreCase.Equals(candidate, projectsRoot)
            || candidate.StartsWith(
                projectsRoot + Path.DirectorySeparatorChar,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new BootstrapException(
                "BOOTSTRAP_OUTPUT_UNDER_PROJECTS",
                "Output under C:\\Projects is not allowed.");
        }
    }

    private static void RejectExistingReparseSegments(string path)
    {
        var fullPath = Path.GetFullPath(path);
        var root = Path.GetPathRoot(fullPath);
        string? current = root;

        foreach (var segment in Path.GetRelativePath(root ?? string.Empty, fullPath)
                     .Split(
                         [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
                         StringSplitOptions.RemoveEmptyEntries))
        {
            current = string.IsNullOrEmpty(current)
                ? segment
                : Path.Combine(current, segment);

            if (!File.Exists(current) && !Directory.Exists(current))
                return;

            if ((File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0)
            {
                throw new BootstrapException(
                    "BOOTSTRAP_REPARSE_POINT_REJECTED",
                    "Bootstrap paths must not contain existing reparse points.");
            }
        }
    }

    private static void EnsurePathUnderRoot(string root, string path)
    {
        var normalizedRoot = NormalizeForBoundary(root);
        var normalizedPath = NormalizeForBoundary(path);

        if (!StringComparer.OrdinalIgnoreCase.Equals(normalizedPath, normalizedRoot)
            && !normalizedPath.StartsWith(
                normalizedRoot + Path.DirectorySeparatorChar,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new BootstrapException(
                "BOOTSTRAP_INTERNAL_PATH_ESCAPE",
                "Bootstrap attempted to prepare a path outside the intended output root.");
        }
    }

    private static string NormalizeForBoundary(string path)
        => Path.TrimEndingDirectorySeparator(Path.GetFullPath(path))
            .Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

    private static void ValidateIdentity(string value, string optionName)
    {
        if (string.IsNullOrWhiteSpace(value)
            || value is "." or ".."
            || value.Length > 128
            || value.Any(static character =>
                character == Path.DirectorySeparatorChar
                || character == Path.AltDirectorySeparatorChar
                || character == ':'
                || char.IsControl(character)
                || char.IsWhiteSpace(character)
                || Path.GetInvalidFileNameChars().Contains(character)))
        {
            throw new BootstrapException(
                "BOOTSTRAP_INVALID_IDENTITY",
                $"{optionName} contains an invalid model identity segment.");
        }

        foreach (var character in value)
        {
            if (!(char.IsAsciiLetterOrDigit(character)
                || character == '-'
                || character == '_'
                || character == '.'))
            {
                throw new BootstrapException(
                    "BOOTSTRAP_INVALID_IDENTITY",
                    $"{optionName} contains an invalid model identity segment.");
            }
        }
    }

    private static int ParseConfigInt(string value, string optionName)
    {
        if (!int.TryParse(
                value,
                System.Globalization.NumberStyles.None,
                System.Globalization.CultureInfo.InvariantCulture,
                out var parsed))
        {
            throw new BootstrapException(
                "BOOTSTRAP_INVALID_CONFIG",
                $"{optionName} must be an integer.");
        }

        return parsed;
    }

    private static void ValidateConfig(ModelConfig config)
    {
        if (config.VocabSize < 256
            || config.VocabSize > MaxSmokeVocabSize
            || config.ContextLen <= 0
            || config.ContextLen > MaxSmokeContextLen
            || config.HiddenDim <= 0
            || config.HiddenDim > MaxSmokeHiddenDim
            || config.NumLayers <= 0
            || config.NumLayers > MaxSmokeNumLayers
            || config.NumHeads <= 0
            || config.NumHeads > config.HiddenDim
            || config.NumHeads > MaxSmokeNumHeads
            || config.NumKvHeads <= 0
            || config.NumKvHeads > config.NumHeads
            || config.FfnHiddenDim <= 0
            || config.FfnHiddenDim > MaxSmokeFfnHiddenDim
            || config.HiddenDim % config.NumHeads != 0
            || config.NumHeads % config.NumKvHeads != 0
            || !TryComputeSmokeModelBudget(
                config,
                out var parameterBytes,
                out var activationBytes)
            || parameterBytes > MaxSmokeModelParameterBytes
            || activationBytes > MaxSmokeActivationBytes)
        {
            throw new BootstrapException(
                "BOOTSTRAP_INVALID_CONFIG",
                "Model configuration is outside the bootstrap bounds.");
        }
    }

    private static bool TryComputeSmokeModelBudget(
        ModelConfig config,
        out long parameterBytes,
        out long activationBytes)
    {
        parameterBytes = 0;
        activationBytes = 0;

        try
        {
            checked
            {
                var hiddenDim = (long)config.HiddenDim;
                var headDim = hiddenDim / config.NumHeads;
                var kvProjectionRows = (long)config.NumKvHeads * headDim;
                var embeddingElements = (long)config.VocabSize * hiddenDim;
                var perBlockElements =
                    (2L * hiddenDim)
                    + (hiddenDim * hiddenDim)
                    + (2L * kvProjectionRows * hiddenDim)
                    + (hiddenDim * hiddenDim)
                    + (3L * config.FfnHiddenDim * hiddenDim);
                var parameterElements =
                    embeddingElements
                    + embeddingElements
                    + hiddenDim
                    + ((long)config.NumLayers * perBlockElements);

                parameterBytes = parameterElements * FloatByteCount;
                activationBytes = (long)config.ContextLen * hiddenDim * FloatByteCount;
                return true;
            }
        }
        catch (OverflowException)
        {
            return false;
        }
    }

    private static string CanonicalizePath(
        string value,
        string errorCode,
        string message)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Path is empty.", nameof(value));
            return Path.GetFullPath(value);
        }
        catch (Exception exception) when (
            exception is ArgumentException
                or NotSupportedException
                or PathTooLongException)
        {
            throw new BootstrapException(errorCode, message, exception);
        }
    }

    private static string Sha256File(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    private static string ResolveSourceCommit()
    {
        foreach (var candidate in EnumerateGitProbeDirectories())
        {
            var commit = TryRunGitRevParse(candidate);
            if (commit is not null)
                return commit;
        }

        return "unknown";
    }

    private static IEnumerable<string> EnumerateGitProbeDirectories()
    {
        yield return Directory.GetCurrentDirectory();
        yield return AppContext.BaseDirectory;
    }

    private static string? TryRunGitRevParse(string workingDirectory)
    {
        try
        {
            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = "rev-parse HEAD",
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            if (!process.Start())
                return null;

            if (!process.WaitForExit(milliseconds: 2000))
            {
                try
                {
                    process.Kill(entireProcessTree: true);
                }
                catch (Exception killException) when (IsRecoverableBootstrapFailure(killException))
                {
                }

                return null;
            }

            if (process.ExitCode != 0)
                return null;

            var output = process.StandardOutput.ReadToEnd().Trim();
            return IsSha1(output) ? output : null;
        }
        catch (Exception exception) when (IsRecoverableBootstrapFailure(exception))
        {
            return null;
        }
    }

    private static bool IsSha1(string value)
        => value.Length == 40
            && value.All(static character =>
                char.IsAsciiHexDigit(character));

    private static void DeleteDirectoryIfExists(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
            return;

        Directory.Delete(path, recursive: true);
    }

    private static bool IsRecoverableBootstrapFailure(Exception exception)
        => exception is IOException
            or UnauthorizedAccessException
            or ArgumentException
            or NotSupportedException
            or InvalidOperationException
            or InvalidDataException
            or PathTooLongException
            or System.ComponentModel.Win32Exception;

    private static async Task WriteErrorAsync(
        TextWriter stderr,
        string errorCode,
        string message)
    {
        var failure = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["ok"] = false,
            ["error_code"] = errorCode,
            ["message"] = message,
        };
        await stderr.WriteLineAsync(JsonSerializer.Serialize(failure, JsonOptions))
            .ConfigureAwait(false);
    }

    private sealed record ParsedArguments(
        IReadOnlyDictionary<string, string> Values,
        IReadOnlySet<string> Flags)
    {
        public bool Force => Flags.Contains("--force");

        public bool Tp1LiveLocalBundle => Flags.Contains(Tp1LiveLocalBundleFlag);

        public bool AllowRandomAuditSalt => Flags.Contains(AllowRandomAuditSaltFlag);
    }

    private sealed record BootstrapRequest(
        string OutputRoot,
        string ModelId,
        string Revision,
        string Profile,
        ModelConfig Config,
        string CorpusSourcePath,
        string CorpusText,
        bool Force,
        Tp1LiveLocalBundleRequest? Tp1LiveLocalBundle);

    private sealed record Tp1LiveLocalBundleRequest(
        string ModelId,
        string OllamaManifestDigest,
        string OllamaModelLayerDigest,
        string BaseModelId,
        string BaseModelRevision,
        int MaxTeacherCalls,
        int DpoMaxStepCount,
        string AuditSaltBase64,
        bool AuditSaltGenerated,
        string LocalRetentionPolicyRef,
        string OutputLicenseRef,
        string TeacherLegalEvidenceRef,
        string TeacherLicenseEvidenceRef,
        string DeploymentEvidenceRef,
        RepositoryOwnedDpoDatasetAddress DpoDatasetAddress,
        Tp1SecretPair LifecycleTokens,
        Tp1SecretPair RouteAdmissionTokens,
        string RouteAdmissionTenantId,
        string LiveLocalServerCommandPath,
        Tp1MeasurementGateThresholds MeasurementGateThresholds,
        Tp1TrainingConfiguration TrainingConfiguration);

    private sealed record Tp1SecretPair(
        string CurrentToken,
        string? PreviousToken,
        DateTimeOffset? PreviousTokenExpiresAtUtc);

    private sealed record Tp1MeasurementGateThresholds(
        double RequiredHeldOutQualityScore,
        long MaxRuntimeMs,
        decimal MaxCostUsd,
        long MaxPeakRamMb);

    private sealed record Tp1TrainingConfiguration(
        int BatchSize,
        int SeqLen,
        float MaxLr,
        float MinLr,
        int WarmupSteps,
        int TotalSteps,
        int GradAccumSteps,
        float GradClipNorm,
        float WeightDecay,
        int CheckpointEvery,
        int LogEvery);

    private sealed record Tp1LiveLocalBundlePublication(
        string FinalBundleManifestPath,
        string FinalEnvironmentJsonPath,
        string FinalEnvironmentPowerShellPath,
        string FinalMeasurementGateThresholdsPath,
        string FinalAdapterRoot,
        string FinalCacheRoot,
        string FinalRegistryRoot,
        string FinalRepositoryRoot,
        string FinalDpoPairRoot,
        string FinalArtifactDbRoot,
        string FinalReadyFilePath,
        IReadOnlyDictionary<string, object?> PublicManifestSummary);
}

internal sealed record BootstrapCliDependencies(
    Action<BootstrapArtifactPaths>? BeforeValidation,
    Action<BootstrapForcePublicationPaths>? AfterForceDestinationPublishedBeforeBackupCleanup,
    Action<string>? AfterDpoPairRootCreatedBeforePublication = null,
    Action<string>? AfterDpoDirectoryCreated = null)
{
    public static BootstrapCliDependencies Production { get; } = new(
        BeforeValidation: null,
        AfterForceDestinationPublishedBeforeBackupCleanup: null,
        AfterDpoPairRootCreatedBeforePublication: null,
        AfterDpoDirectoryCreated: null);
}

internal sealed record BootstrapArtifactPaths(
    string Destination,
    string StagingDirectory,
    string CheckpointPath,
    string TokenizerPath,
    string CorpusPath,
    string ManifestPath);

internal sealed record BootstrapForcePublicationPaths(
    string Destination,
    string BackupDirectory,
    string StagingDirectory);

internal sealed class BootstrapException : Exception
{
    public BootstrapException(string errorCode, string message)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    public BootstrapException(string errorCode, string message, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }

    public string ErrorCode { get; }
}
