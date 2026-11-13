// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

namespace ModelServer.AutonomousPromotion;

using System.Text.Json;
using System.Text.Json.Serialization;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using AutonomousLearning;
using ContinuousLearning;
using LoRA;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using TransformerModel.Config;

public sealed record AutonomousPromotionEvaluationConfiguration(
    ModelConfig ModelConfig,
    string BaseCheckpointPath,
    string TokenizerPath,
    string AdapterRoot,
    LoraConfig LoraConfig,
    IReadOnlyList<AutonomousPromotionEvaluationTargetConfiguration> Targets,
    HeldOutEvaluationLimits? Limits = null);

public sealed record AutonomousPromotionEvaluationTargetConfiguration(
    string Domain,
    ModelAdapter CandidateAdapter,
    TrainingSourceManifest PlannedTrainingSources,
    IReadOnlyList<HeldOutEvaluationCase> HeldOutCases);

public sealed record AutonomousPromotionEvaluationEvidence(
    string Domain,
    double BaselineAccuracy,
    double CandidateAccuracy,
    bool PromotionReady,
    AccuracyEvaluationArtifactIdentity Artifacts,
    string AdapterSha256,
    string TrainingSourceManifestSha256);

/// <summary>
/// The single production composition authority for the generic LoRA held-out
/// promotion gate. It performs inference-only measurement and never trains,
/// registers, activates, or publishes a model.
/// </summary>
public sealed class AutonomousPromotionEvaluationCompositionRoot
{
    private readonly IAccuracyEvaluator _evaluator;
    private readonly IReadOnlyDictionary<string, AutonomousPromotionEvaluationTargetConfiguration>
        _targets;
    private int _evaluationActive;

    private AutonomousPromotionEvaluationCompositionRoot(
        IAccuracyEvaluator evaluator,
        IReadOnlyDictionary<string, AutonomousPromotionEvaluationTargetConfiguration> targets)
    {
        _evaluator = evaluator;
        _targets = targets;
    }

    public static OperationResult<AutonomousPromotionEvaluationCompositionRoot> Create(
        AutonomousPromotionEvaluationConfiguration configuration)
    {
        if (configuration is null)
            return FailRoot("Promotion evaluation configuration is required.", "AUTONOMOUS_PROMOTION_CONFIGURATION_REQUIRED");

        try
        {
            ArgumentNullException.ThrowIfNull(configuration.ModelConfig);
            ArgumentNullException.ThrowIfNull(configuration.LoraConfig);
            ArgumentNullException.ThrowIfNull(configuration.Targets);

            var limits = configuration.Limits ?? new HeldOutEvaluationLimits();
            ValidateLimits(limits);
            if (!TrySnapshotBounded(
                    configuration.Targets,
                    limits.MaxDomains,
                    out var suppliedTargets))
            {
                return FailRoot(
                    "Promotion-evaluation domain limit was exceeded.",
                    "HELD_OUT_DOMAIN_LIMIT_EXCEEDED");
            }
            if (suppliedTargets.Length == 0)
                return FailRoot("At least one promotion-evaluation target is required.", "HELD_OUT_SET_NOT_FOUND");

            var boundedTargets = new List<BoundedTargetSnapshot>(suppliedTargets.Length);
            var suppliedDomains = new HashSet<string>(StringComparer.Ordinal);
            foreach (var target in suppliedTargets)
            {
                if (target is null
                    || string.IsNullOrWhiteSpace(target.Domain)
                    || !StringComparer.Ordinal.Equals(target.Domain, target.Domain.Trim()))
                {
                    return FailRoot("A canonical promotion-evaluation domain is required.", "HELD_OUT_DOMAIN_NONCANONICAL");
                }
                if (!suppliedDomains.Add(target.Domain))
                    return FailRoot("Promotion-evaluation domains must be unique.", "HELD_OUT_DOMAIN_DUPLICATE");
                if (!TrySnapshotBounded(
                        target.PlannedTrainingSources?.SourceIds,
                        limits.MaxTrainingSources,
                        out var sourceIds))
                {
                    return FailRoot(
                        "Promotion-evaluation training-source limit was exceeded.",
                        "TRAINING_SOURCE_LIMIT_EXCEEDED");
                }
                if (!TrySnapshotBounded(
                        target.HeldOutCases,
                        limits.MaxCases,
                        out var heldOutCases))
                {
                    return FailRoot(
                        "Promotion-evaluation held-out case limit was exceeded.",
                        "HELD_OUT_CASE_LIMIT_EXCEEDED");
                }
                if (target.CandidateAdapter is null
                    || !StringComparer.Ordinal.Equals(target.Domain, target.CandidateAdapter.Domain)
                    || target.CandidateAdapter.Provenance is null)
                {
                    return FailRoot("Candidate adapter provenance is required.", "ADAPTER_PROVENANCE_REQUIRED");
                }

                boundedTargets.Add(new BoundedTargetSnapshot(target, sourceIds, heldOutCases));
            }

            var baseCheckpointPath = RequireCanonicalRegularFile(
                configuration.BaseCheckpointPath,
                "BASE_CHECKPOINT");
            var tokenizerPath = RequireCanonicalRegularFile(
                configuration.TokenizerPath,
                "TOKENIZER_CHECKPOINT");
            var adapterRoot = RequireCanonicalDirectory(configuration.AdapterRoot);
            var modelIdentity = TrainingSourceProvenance.Identify(configuration.ModelConfig);
            var loraIdentity = TrainingSourceProvenance.Identify(configuration.LoraConfig);
            var targets = new Dictionary<string, AutonomousPromotionEvaluationTargetConfiguration>(
                StringComparer.Ordinal);
            var heldOutSets = new List<SourceDisjointHeldOutEvaluationSet>();

            foreach (var boundedTarget in boundedTargets)
            {
                var target = boundedTarget.Target;

                var manifestResult = TrainingSourceProvenance.BuildManifest(
                    boundedTarget.SourceIds);
                if (!manifestResult.IsSuccess
                    || manifestResult.Value is null
                    || !StringComparer.Ordinal.Equals(
                        manifestResult.Value.Sha256,
                        target.PlannedTrainingSources?.Sha256))
                {
                    return FailRoot("The planned training-source manifest is invalid.", "TRAINING_SOURCE_MANIFEST_INVALID");
                }

                var provenance = target.CandidateAdapter.Provenance!;
                if (!ManifestEquals(provenance.TrainingSources, manifestResult.Value)
                    || provenance.Model != modelIdentity
                    || !LoraIdentityEquals(provenance.Lora, loraIdentity))
                {
                    return FailRoot("Candidate provenance does not match the configured model, LoRA, or sources.", "ADAPTER_PROVENANCE_MISMATCH");
                }

                var adapterPath = RequireCanonicalRegularFile(
                    target.CandidateAdapter.AdapterPath,
                    "ADAPTER_CHECKPOINT");
                if (!IsOwnedPath(adapterRoot, adapterPath))
                    return FailRoot("Candidate adapter is outside the configured adapter root.", "ADAPTER_PATH_OUTSIDE_ROOT");

                var canonicalTarget = target with
                {
                    CandidateAdapter = target.CandidateAdapter with { AdapterPath = adapterPath },
                    PlannedTrainingSources = manifestResult.Value,
                    HeldOutCases = Array.AsReadOnly(boundedTarget.HeldOutCases),
                };
                targets.Add(target.Domain, canonicalTarget);
                heldOutSets.Add(new SourceDisjointHeldOutEvaluationSet(
                    target.Domain,
                    canonicalTarget.HeldOutCases));
            }

            var provider = new ConfiguredHeldOutEvaluationSetProvider(heldOutSets, limits);
            IAccuracyEvaluator evaluator = new HeldOutLoraAccuracyEvaluator(
                configuration.ModelConfig,
                baseCheckpointPath,
                tokenizerPath,
                provider,
                configuration.LoraConfig,
                limits);
            return OperationResult<AutonomousPromotionEvaluationCompositionRoot>.Ok(
                new AutonomousPromotionEvaluationCompositionRoot(evaluator, targets));
        }
        catch (Exception exception) when (IsNonFatal(exception))
        {
            return FailRoot(
                "Promotion evaluation configuration is invalid.",
                "AUTONOMOUS_PROMOTION_CONFIGURATION_INVALID");
        }
    }

    public OperationResult<AutonomousPromotionEvaluationEvidence> Evaluate(
        string domain,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrWhiteSpace(domain)
            || !StringComparer.Ordinal.Equals(domain, domain.Trim()))
        {
            return FailEvidence("A canonical evaluation domain is required.", "HELD_OUT_DOMAIN_NONCANONICAL");
        }
        if (!_targets.TryGetValue(domain, out var target))
            return FailEvidence("No promotion-evaluation target is configured for the domain.", "HELD_OUT_SET_NOT_FOUND");
        if (Interlocked.CompareExchange(ref _evaluationActive, 1, 0) != 0)
            return FailEvidence("A promotion evaluation is already running.", "HELD_OUT_EVALUATION_BUSY");

        try
        {
            var baseline = _evaluator.Evaluate(
                new AccuracyEvaluationRequest(
                    domain,
                    TargetAdapter: null,
                    target.PlannedTrainingSources,
                    AccuracyEvaluationPhase.Baseline,
                    ExpectedArtifacts: null),
                cancellationToken);
            if (!baseline.IsSuccess || baseline.Value is null)
                return baseline.MapError<AutonomousPromotionEvaluationEvidence>();

            cancellationToken.ThrowIfCancellationRequested();
            var candidate = _evaluator.Evaluate(
                new AccuracyEvaluationRequest(
                    domain,
                    target.CandidateAdapter,
                    target.PlannedTrainingSources,
                    AccuracyEvaluationPhase.Candidate,
                    baseline.Value.Artifacts),
                cancellationToken);
            if (!candidate.IsSuccess || candidate.Value is null)
                return candidate.MapError<AutonomousPromotionEvaluationEvidence>();

            if (candidate.Value.Artifacts != baseline.Value.Artifacts
                || !StringComparer.Ordinal.Equals(
                    candidate.Value.AdapterSha256,
                    target.CandidateAdapter.Provenance!.AdapterSha256)
                || !StringComparer.Ordinal.Equals(
                    candidate.Value.TrainingSourceManifestSha256,
                    target.PlannedTrainingSources.Sha256))
            {
                return FailEvidence(
                    "Held-out evidence is not bound to the configured promotion target.",
                    "ACCURACY_EVALUATION_PROVENANCE_MISMATCH");
            }

            return OperationResult<AutonomousPromotionEvaluationEvidence>.Ok(
                new AutonomousPromotionEvaluationEvidence(
                    domain,
                    baseline.Value.Accuracy,
                    candidate.Value.Accuracy,
                    candidate.Value.Accuracy > baseline.Value.Accuracy,
                    candidate.Value.Artifacts,
                    candidate.Value.AdapterSha256!,
                    candidate.Value.TrainingSourceManifestSha256));
        }
        finally
        {
            Volatile.Write(ref _evaluationActive, 0);
        }
    }

    private static string RequireCanonicalRegularFile(string path, string artifactName)
    {
        if (string.IsNullOrWhiteSpace(path) || !Path.IsPathFullyQualified(path))
            throw new InvalidDataException($"{artifactName} path must be canonical and absolute.");
        var fullPath = Path.GetFullPath(path);
        if (!PathEquals(path, fullPath) || !File.Exists(fullPath))
            throw new InvalidDataException($"{artifactName} file is required.");
        var attributes = File.GetAttributes(fullPath);
        if ((attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint)) != 0)
            throw new InvalidDataException($"{artifactName} must be a regular file.");
        EnsurePathChainHasNoReparsePoints(Path.GetDirectoryName(fullPath)!);
        return fullPath;
    }

    private static bool TrySnapshotBounded<T>(
        IEnumerable<T>? values,
        int maximumCount,
        out T[] snapshot)
    {
        if (values is null)
        {
            snapshot = [];
            return true;
        }

        var buffered = new List<T>(Math.Min(maximumCount, 1024));
        using var enumerator = values.GetEnumerator();
        while (enumerator.MoveNext())
        {
            if (buffered.Count == maximumCount)
            {
                snapshot = [];
                return false;
            }
            buffered.Add(enumerator.Current);
        }

        snapshot = buffered.ToArray();
        return true;
    }

    private static void ValidateLimits(HeldOutEvaluationLimits limits)
    {
        if (limits.MaxDomains <= 0
            || limits.MaxTrainingSources <= 0
            || limits.MaxCases <= 0
            || limits.MaxCaseIdChars <= 0
            || limits.MaxPromptChars <= 0
            || limits.MaxResponseChars <= 0
            || limits.MaxCorpusChars <= 0
            || limits.MaxBaseCheckpointBytes <= 0
            || limits.MaxTokenizerBytes <= 0
            || limits.MaxAdapterBytes <= 0
            || limits.MaxAggregateArtifactBytes <= 0
            || limits.MaxVocabSize < 256
            || limits.MaxContextLength <= 0
            || limits.MaxHiddenDimension <= 0
            || limits.MaxLayers <= 0
            || limits.MaxEstimatedParameters <= 0
            || limits.MaxLoraRank <= 0
            || limits.MaxLoraTargets <= 0
            || limits.MaxEstimatedAdapterParameters <= 0
            || limits.MaxLogitsElements <= 0
            || limits.MaxLogitsBytes <= 0
            || limits.MaxActivationElements <= 0
            || limits.MaxOperationBudget <= 0
            || limits.MaxActivationBytes <= 0
            || limits.MaxCumulativeLogitsElements <= 0
            || limits.MaxCumulativeLogitsBytes <= 0
            || limits.MaxCumulativeActivationElements <= 0
            || limits.MaxCumulativeActivationBytes <= 0
            || limits.MaxCumulativeOperationBudget <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(limits));
        }
    }

    private static string RequireCanonicalDirectory(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !Path.IsPathFullyQualified(path))
            throw new InvalidDataException("Adapter root must be canonical and absolute.");
        var fullPath = Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));
        if (!PathEquals(Path.TrimEndingDirectorySeparator(path), fullPath)
            || !Directory.Exists(fullPath)
            || File.Exists(fullPath))
        {
            throw new InvalidDataException("Adapter root is required.");
        }
        var attributes = File.GetAttributes(fullPath);
        if ((attributes & FileAttributes.ReparsePoint) != 0)
            throw new InvalidDataException("Adapter root must not be a reparse point.");
        EnsurePathChainHasNoReparsePoints(fullPath);
        return fullPath;
    }

    private static void EnsurePathChainHasNoReparsePoints(string path)
    {
        for (var directory = new DirectoryInfo(path);
             directory is not null;
             directory = directory.Parent)
        {
            if ((directory.Attributes & FileAttributes.ReparsePoint) != 0)
                throw new InvalidDataException("An artifact path contains a reparse point.");
        }
    }

    private static bool IsOwnedPath(string root, string path)
    {
        var prefix = root.EndsWith(Path.DirectorySeparatorChar)
            ? root
            : root + Path.DirectorySeparatorChar;
        return path.StartsWith(prefix, PathComparison);
    }

    private static bool PathEquals(string left, string right)
        => string.Equals(left, right, PathComparison);

    private static StringComparison PathComparison
        => OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

    private static bool ManifestEquals(TrainingSourceManifest left, TrainingSourceManifest right)
        => left is not null
           && left.SourceIds is not null
           && StringComparer.Ordinal.Equals(left.Sha256, right.Sha256)
           && left.SourceIds.SequenceEqual(right.SourceIds, StringComparer.Ordinal);

    private static bool LoraIdentityEquals(
        LoraConfigurationIdentity left,
        LoraConfigurationIdentity right)
        => left is not null
           && left.OrderedTargetModules is not null
           && left.Rank == right.Rank
           && left.Alpha.Equals(right.Alpha)
           && left.Scaling.Equals(right.Scaling)
           && left.Dropout.Equals(right.Dropout)
           && StringComparer.Ordinal.Equals(left.Sha256, right.Sha256)
           && left.OrderedTargetModules.SequenceEqual(
               right.OrderedTargetModules,
               StringComparer.Ordinal);

    private static OperationResult<AutonomousPromotionEvaluationCompositionRoot> FailRoot(
        string message,
        string code)
        => OperationResult<AutonomousPromotionEvaluationCompositionRoot>.Fail(message, code);

    private static OperationResult<AutonomousPromotionEvaluationEvidence> FailEvidence(
        string message,
        string code)
        => OperationResult<AutonomousPromotionEvaluationEvidence>.Fail(message, code);

    private static bool IsNonFatal(Exception exception)
        => SnapshotExceptionGraphClassifier.IsNonFatal(exception);

    private sealed record BoundedTargetSnapshot(
        AutonomousPromotionEvaluationTargetConfiguration Target,
        string[] SourceIds,
        HeldOutEvaluationCase[] HeldOutCases);
}

public sealed record AutonomousPromotionEvaluationRequest(string Domain);

public sealed record AutonomousPromotionEvaluationResponse(
    bool IsSuccess,
    string? Domain,
    bool PromotionReady,
    bool ProductReady,
    bool PerformsTraining,
    bool EvaluatorEvidenceAvailable,
    double? BaselineAccuracy,
    double? CandidateAccuracy,
    AccuracyEvaluationArtifactIdentity? Artifacts,
    string? AdapterSha256,
    string? TrainingSourceManifestSha256,
    string ErrorCode,
    string ErrorMessage);

public sealed class AutonomousPromotionEvaluationEndpoint
{
    public const string EndpointPath = "/api/model-promotion/held-out-readiness";
    public const int MaximumRequestBytes = 4096;

    private readonly AutonomousPromotionEvaluationCompositionRoot? _composition;
    private readonly string _notReadyCode;
    private readonly string _notReadyMessage;

    public bool IsConfigured => _composition is not null;

    private AutonomousPromotionEvaluationEndpoint(
        AutonomousPromotionEvaluationCompositionRoot? composition,
        string notReadyCode,
        string notReadyMessage)
    {
        _composition = composition;
        _notReadyCode = notReadyCode;
        _notReadyMessage = notReadyMessage;
    }

    public static AutonomousPromotionEvaluationEndpoint Ready(
        AutonomousPromotionEvaluationCompositionRoot composition)
        => new(
            composition ?? throw new ArgumentNullException(nameof(composition)),
            "none",
            "none");

    public static AutonomousPromotionEvaluationEndpoint NotConfigured()
        => new(
            null,
            "AUTONOMOUS_PROMOTION_EVALUATION_NOT_CONFIGURED",
            "Autonomous promotion evaluation is not configured.");

    public AutonomousPromotionEvaluationResponse Evaluate(
        string domain,
        CancellationToken cancellationToken = default)
    {
        if (_composition is null)
            return Failure(domain, _notReadyCode, _notReadyMessage);

        var result = _composition.Evaluate(domain, cancellationToken);
        if (!result.IsSuccess || result.Value is null)
            return Failure(domain, result.ErrorCode, result.ErrorMessage);

        var evidence = result.Value;
        return new AutonomousPromotionEvaluationResponse(
            IsSuccess: true,
            evidence.Domain,
            evidence.PromotionReady,
            ProductReady: false,
            PerformsTraining: false,
            EvaluatorEvidenceAvailable: true,
            evidence.BaselineAccuracy,
            evidence.CandidateAccuracy,
            evidence.Artifacts,
            evidence.AdapterSha256,
            evidence.TrainingSourceManifestSha256,
            ErrorCode: "none",
            ErrorMessage: "none");
    }

    private static AutonomousPromotionEvaluationResponse Failure(
        string? domain,
        string code,
        string message)
        => new(
            IsSuccess: false,
            domain,
            PromotionReady: false,
            ProductReady: false,
            PerformsTraining: false,
            EvaluatorEvidenceAvailable: false,
            BaselineAccuracy: null,
            CandidateAccuracy: null,
            Artifacts: null,
            AdapterSha256: null,
            TrainingSourceManifestSha256: null,
            code,
            message);
}

public static class AutonomousPromotionEvaluationConfigurationLoader
{
    public const string ConfigurationArgumentName =
        "--autonomous-promotion-evaluation-config";
    public const string ConfigurationEnvironmentName =
        "XIIGEN_AUTONOMOUS_PROMOTION_EVALUATION_CONFIG";
    public const long MaximumConfigurationBytes = 8L * 1024 * 1024;

    private static readonly JsonSerializerOptions StrictJsonOptions = new()
    {
        PropertyNameCaseInsensitive = false,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        MaxDepth = 64,
    };

    public static AutonomousPromotionEvaluationEndpoint ResolveEndpoint(
        IReadOnlyList<string> args,
        Func<string, string?> readEnvironment)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(readEnvironment);

        var configuredByCli = false;
        string? configuredPath = null;
        for (var index = 0; index < args.Count; index++)
        {
            var argument = args[index];
            if (StringComparer.Ordinal.Equals(argument, ConfigurationArgumentName))
            {
                if (configuredByCli || index + 1 >= args.Count)
                    throw new ArgumentException("Autonomous promotion evaluation config is invalid.");
                configuredPath = args[++index];
                configuredByCli = true;
                continue;
            }

            var prefix = ConfigurationArgumentName + "=";
            if (argument.StartsWith(prefix, StringComparison.Ordinal))
            {
                if (configuredByCli)
                    throw new ArgumentException("Autonomous promotion evaluation config was specified more than once.");
                configuredPath = argument[prefix.Length..];
                configuredByCli = true;
            }
        }

        configuredPath = configuredByCli
            ? configuredPath
            : readEnvironment(ConfigurationEnvironmentName);
        if (string.IsNullOrWhiteSpace(configuredPath))
        {
            if (configuredByCli)
                throw new ArgumentException("Autonomous promotion evaluation config path is required.");
            return AutonomousPromotionEvaluationEndpoint.NotConfigured();
        }

        var loaded = Load(configuredPath);
        if (!loaded.IsSuccess || loaded.Value is null)
        {
            throw new ArgumentException(
                loaded.ErrorCode ?? "AUTONOMOUS_PROMOTION_CONFIGURATION_INVALID");
        }
        return AutonomousPromotionEvaluationEndpoint.Ready(loaded.Value);
    }

    public static OperationResult<AutonomousPromotionEvaluationCompositionRoot> Load(
        string configurationPath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(configurationPath)
                || !Path.IsPathFullyQualified(configurationPath))
            {
                return Fail("Promotion evaluation config path must be absolute.");
            }
            var fullPath = Path.GetFullPath(configurationPath);
            var comparison = OperatingSystem.IsWindows()
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;
            if (!string.Equals(configurationPath, fullPath, comparison)
                || !File.Exists(fullPath))
            {
                return Fail("Promotion evaluation config file is required.");
            }
            var attributes = File.GetAttributes(fullPath);
            if ((attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint)) != 0)
                return Fail("Promotion evaluation config must be a regular file.");
            EnsurePathChainHasNoReparsePoints(Path.GetDirectoryName(fullPath)!);

            using var stream = new FileStream(
                fullPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 4096,
                FileOptions.SequentialScan);
            if (stream.Length is <= 0 or > MaximumConfigurationBytes)
                return Fail("Promotion evaluation config size is invalid.");
            var configuration = JsonSerializer.Deserialize<AutonomousPromotionEvaluationConfiguration>(
                stream,
                StrictJsonOptions);
            return configuration is null
                ? Fail("Promotion evaluation config is empty.")
                : AutonomousPromotionEvaluationCompositionRoot.Create(configuration);
        }
        catch (Exception exception) when (IsNonFatal(exception))
        {
            return Fail("Promotion evaluation config could not be loaded.");
        }
    }

    private static OperationResult<AutonomousPromotionEvaluationCompositionRoot> Fail(string message)
        => OperationResult<AutonomousPromotionEvaluationCompositionRoot>.Fail(
            message,
            "AUTONOMOUS_PROMOTION_CONFIGURATION_INVALID");

    private static void EnsurePathChainHasNoReparsePoints(string path)
    {
        for (var directory = new DirectoryInfo(path);
             directory is not null;
             directory = directory.Parent)
        {
            if ((directory.Attributes & FileAttributes.ReparsePoint) != 0)
                throw new InvalidDataException(
                    "Promotion evaluation config path contains a reparse point.");
        }
    }

    private static bool IsNonFatal(Exception exception)
        => SnapshotExceptionGraphClassifier.IsNonFatal(exception);
}

public static class AutonomousPromotionEvaluationEndpointMapper
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = false,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        MaxDepth = 16,
    };

    public static IEndpointConventionBuilder MapAutonomousPromotionEvaluation(
        this IEndpointRouteBuilder endpoints,
        AutonomousPromotionEvaluationEndpoint endpoint)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        ArgumentNullException.ThrowIfNull(endpoint);
        return endpoints.MapPost(
            AutonomousPromotionEvaluationEndpoint.EndpointPath,
            context => DispatchAsync(context, endpoint));
    }

    private static async Task DispatchAsync(
        HttpContext context,
        AutonomousPromotionEvaluationEndpoint endpoint)
    {
        AutonomousPromotionEvaluationResponse response;
        if (context.Request.ContentLength is > AutonomousPromotionEvaluationEndpoint.MaximumRequestBytes)
        {
            response = InvalidRequest("AUTONOMOUS_PROMOTION_REQUEST_TOO_LARGE");
        }
        else
        {
            try
            {
                await using var buffered = new MemoryStream();
                var buffer = new byte[4096];
                while (true)
                {
                    var read = await context.Request.Body.ReadAsync(
                        buffer,
                        context.RequestAborted);
                    if (read == 0)
                        break;
                    if (buffered.Length + read > AutonomousPromotionEvaluationEndpoint.MaximumRequestBytes)
                    {
                        response = InvalidRequest("AUTONOMOUS_PROMOTION_REQUEST_TOO_LARGE");
                        await WriteAsync(context, response, ResolveStatusCode(response));
                        return;
                    }
                    await buffered.WriteAsync(buffer.AsMemory(0, read), context.RequestAborted);
                }
                buffered.Position = 0;
                var request = await JsonSerializer.DeserializeAsync<AutonomousPromotionEvaluationRequest>(
                    buffered,
                    JsonOptions,
                    context.RequestAborted);
                response = request is null
                    ? InvalidRequest("AUTONOMOUS_PROMOTION_REQUEST_INVALID")
                    : endpoint.Evaluate(request.Domain, context.RequestAborted);
            }
            catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception) when (IsNonFatal(exception))
            {
                response = InvalidRequest("AUTONOMOUS_PROMOTION_REQUEST_INVALID");
            }
        }

        await WriteAsync(
            context,
            response,
            ResolveStatusCode(response));
    }

    private static int ResolveStatusCode(AutonomousPromotionEvaluationResponse response)
        => response.IsSuccess
            ? StatusCodes.Status200OK
            : response.ErrorCode switch
            {
                "AUTONOMOUS_PROMOTION_REQUEST_TOO_LARGE" =>
                    StatusCodes.Status413PayloadTooLarge,
                "AUTONOMOUS_PROMOTION_EVALUATION_NOT_CONFIGURED" =>
                    StatusCodes.Status503ServiceUnavailable,
                _ => StatusCodes.Status422UnprocessableEntity,
            };

    private static AutonomousPromotionEvaluationResponse InvalidRequest(string code)
        => new(
            IsSuccess: false,
            Domain: null,
            PromotionReady: false,
            ProductReady: false,
            PerformsTraining: false,
            EvaluatorEvidenceAvailable: false,
            BaselineAccuracy: null,
            CandidateAccuracy: null,
            Artifacts: null,
            AdapterSha256: null,
            TrainingSourceManifestSha256: null,
            code,
            "Autonomous promotion evaluation request is invalid.");

    private static async Task WriteAsync(
        HttpContext context,
        AutonomousPromotionEvaluationResponse response,
        int statusCode)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json; charset=utf-8";
        context.Response.Headers.CacheControl = "no-store";
        context.Response.Headers["x-autonomous-promotion-performs-training"] =
            bool.FalseString;
        context.Response.Headers["x-autonomous-promotion-product-ready"] =
            bool.FalseString;
        context.Response.Headers["x-autonomous-promotion-promotion-ready"] =
            response.PromotionReady.ToString();
        context.Response.Headers["x-autonomous-promotion-evaluator-evidence-available"] =
            response.EvaluatorEvidenceAvailable.ToString();
        await JsonSerializer.SerializeAsync(
            context.Response.Body,
            response,
            JsonOptions,
            context.RequestAborted);
    }

    private static bool IsNonFatal(Exception exception)
        => SnapshotExceptionGraphClassifier.IsNonFatal(exception);
}
