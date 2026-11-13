// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo(
    "LlmFromScratch.Tests")]

namespace TranslationDomainTools;

using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ContinuousLearning;
using DesignConversionDomain;
using DPO;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using TranslationDomain;
using TransformerModel;

public sealed record TranslationDpoModelServerCoordinatorOptions(
    string SeedCorpusPath,
    string OutputPath,
    string CheckpointDirectory,
    string TrainingDataArtifactRoot,
    string? ContinuationOutputPath = null,
    string? ContinuationCheckpointDirectory = null,
    bool AllowRepositoryOutput = false,
    TranslationRepositoryOwnedDpoDatasetContext?
        RepositoryOwnedDpoDatasetContext = null,
    TranslationRepositoryOwnedModelCheckpointContext?
        RepositoryOwnedModelCheckpointContext = null);

public sealed record TranslationDpoModelServerCoordinatorReport(
    bool ServerAuthorityLivenessVerified,
    bool ExactServerAuthorityIdentityVerified,
    bool ModelSetCreatedOnce,
    bool InitialTrainingCompleted,
    bool ContinuationRequested,
    bool ContinuationTrainingCompleted,
    TranslationDpoTrainEvaluateRunReport Initial,
    TranslationDpoCheckpointLifecycleRunReport? Continuation,
    bool SemanticQualityClaimed,
    bool ProductReadyClaimed,
    string HostBoundary);

/// <summary>
/// Owns the sole provider reference used by one coordinator run and admits at
/// most one underlying model-set request. A rejected second request does not
/// reach the server-owned provider.
/// </summary>
internal sealed class SingleInvocationDpoPolicyBaseModelSetProvider
{
    internal const string AlreadyRequestedErrorCode =
        "TRANSLATION_DPO_MODELSERVER_MODEL_SET_ALREADY_REQUESTED";
    private const string AlreadyRequestedErrorMessage =
        "The ModelServer-owned Translation DPO model set was already requested.";

    private readonly IDesignConversionDpoPolicyBaseProvider _provider;
    private int _observedInvocationCount;

    internal SingleInvocationDpoPolicyBaseModelSetProvider(
        IDesignConversionDpoPolicyBaseProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        _provider = provider;
    }

    internal int ObservedInvocationCount =>
        Volatile.Read(ref _observedInvocationCount);

    internal static SingleInvocationDpoPolicyBaseModelSetProvider? Resolve(
        IServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services.GetService<IDesignConversionDpoPolicyBaseProvider>()
            is { } provider
                ? new SingleInvocationDpoPolicyBaseModelSetProvider(provider)
                : null;
    }

    internal async Task<OperationResult<DesignConversionDpoPolicyBaseModelSet>>
        CreateModelSetAsync(CancellationToken cancellationToken)
    {
        if (Interlocked.CompareExchange(
                ref _observedInvocationCount,
                1,
                0) != 0)
        {
            return OperationResult<DesignConversionDpoPolicyBaseModelSet>.Fail(
                AlreadyRequestedErrorMessage,
                AlreadyRequestedErrorCode);
        }

        return await _provider.CreateModelSetAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}

/// <summary>
/// Runs Translation DPO only while the one ModelServer-owned composition is
/// alive. This outer executable consumes the server broker and policy-base
/// provider; it cannot issue or replace mutation-admission authority.
/// </summary>
public static class TranslationDpoModelServerCoordinator
{
    public const string InvalidArgumentsErrorCode =
        "TRANSLATION_DPO_COORDINATOR_ARGUMENTS_INVALID";
    public const string TrainingInactiveErrorCode =
        "TRANSLATION_DPO_MODELSERVER_TRAINING_INACTIVE";
    public const string AuthorityIdentityMismatchErrorCode =
        "TRANSLATION_DPO_MODELSERVER_AUTHORITY_IDENTITY_MISMATCH";
    public const string ModelSetUnavailableErrorCode =
        "TRANSLATION_DPO_MODELSERVER_MODEL_SET_UNAVAILABLE";
    public const string InitialTrainingFailedErrorCode =
        "TRANSLATION_DPO_MODELSERVER_INITIAL_TRAINING_FAILED";
    public const string ContinuationTrainingFailedErrorCode =
        "TRANSLATION_DPO_MODELSERVER_CONTINUATION_TRAINING_FAILED";
    public const string ReproducibilityContextUnavailableErrorCode =
        "TRANSLATION_DPO_MODELSERVER_REPRODUCIBILITY_CONTEXT_UNAVAILABLE";
    public const string ReproducibilityContextInvalidErrorCode =
        "TRANSLATION_DPO_MODELSERVER_REPRODUCIBILITY_CONTEXT_INVALID";
    public const string HostFailedErrorCode =
        "TRANSLATION_DPO_MODELSERVER_HOST_FAILED";
    public const string HostStopFailedErrorCode =
        "TRANSLATION_DPO_MODELSERVER_HOST_STOP_FAILED";
    public const string CancelledErrorCode = "CANCELLED";

    public const string HostBoundary =
        "outer_translation_tool_hosts_one_25_modelserver_composition_and_consumes_its_exact_broker_no_second_issuer_no_25_to_27_reference";

    private const string InvalidArgumentsMessage =
        "Translation DPO coordinator arguments are invalid.";
    private const string TrainingInactiveMessage =
        "ModelServer-owned Translation DPO training is not active.";
    private const string AuthorityIdentityMismatchMessage =
        "The resolved Translation DPO broker is not the exact ModelServer authority.";
    private const string ModelSetUnavailableMessage =
        "The ModelServer-owned Translation DPO model set is unavailable.";
    private const string InitialTrainingFailedMessage =
        "ModelServer-owned Translation DPO initial training failed.";
    private const string ContinuationTrainingFailedMessage =
        "ModelServer-owned Translation DPO continuation training failed.";
    private const string ReproducibilityContextUnavailableMessage =
        "The ModelServer-owned Translation DPO reproducibility context is unavailable.";
    private const string ReproducibilityContextInvalidMessage =
        "The ModelServer-owned Translation DPO reproducibility context is invalid.";
    private const string HostFailedMessage =
        "The local ModelServer host could not run Translation DPO training.";
    private const string HostStopFailedMessage =
        "The local ModelServer host could not complete its training-authority drain.";
    private const string CancelledMessage =
        "Translation DPO training was cancelled.";

    public static async Task<OperationResult<TranslationDpoModelServerCoordinatorReport>>
        RunAsync(
            TranslationDpoModelServerCoordinatorOptions options,
            CancellationToken cancellationToken = default)
    {
        var validated = ValidateOptions(options);
        if (!validated.IsSuccess || validated.Value is null)
            return validated.MapError<TranslationDpoModelServerCoordinatorReport>();

        WebApplication? host = null;
        OperationResult<TranslationDpoModelServerCoordinatorReport>? outcome = null;
        var cleanupFailed = false;
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            host = ModelServer.Program.CreateLocalLoopbackHost([]);
            await host.StartAsync(cancellationToken).ConfigureAwait(false);
            outcome = await RunWithStartedHostAsync(
                    host,
                    validated.Value,
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            outcome = OperationResult<TranslationDpoModelServerCoordinatorReport>.Fail(
                CancelledMessage,
                CancelledErrorCode);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            outcome = OperationResult<TranslationDpoModelServerCoordinatorReport>.Fail(
                HostFailedMessage,
                HostFailedErrorCode);
        }
        finally
        {
            if (host is not null)
            {
                try
                {
                    await host.StopAsync(CancellationToken.None)
                        .ConfigureAwait(false);
                }
                catch (Exception exception)
                    when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
                {
                    cleanupFailed = true;
                }

                try
                {
                    await host.DisposeAsync().ConfigureAwait(false);
                }
                catch (Exception exception)
                    when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
                {
                    cleanupFailed = true;
                }
            }
        }

        if (cleanupFailed)
        {
            return OperationResult<TranslationDpoModelServerCoordinatorReport>.Fail(
                HostStopFailedMessage,
                HostStopFailedErrorCode);
        }

        return outcome
               ?? OperationResult<TranslationDpoModelServerCoordinatorReport>.Fail(
                   HostFailedMessage,
                   HostFailedErrorCode);
    }

    private static async Task<OperationResult<TranslationDpoModelServerCoordinatorReport>>
        RunWithStartedHostAsync(
            WebApplication host,
            NormalizedOptions options,
            CancellationToken cancellationToken)
    {
        var startup = host.Services.GetRequiredService<
            ContinuousLearningModelServerStartup>();
        var readiness = startup.Readiness;
        if (!IsTrainingAuthorityLive(readiness))
        {
            return OperationResult<TranslationDpoModelServerCoordinatorReport>.Fail(
                TrainingInactiveMessage,
                TrainingInactiveErrorCode);
        }

        var authority = host.Services.GetRequiredService<
            ServerOwnedLegalTrainingTeacherAdmissionAuthority>();
        var broker = host.Services.GetRequiredService<
            IServerOwnedDpoTrainingLeaseBroker>();
        if (!ReferenceEquals(authority, broker))
        {
            return OperationResult<TranslationDpoModelServerCoordinatorReport>.Fail(
                AuthorityIdentityMismatchMessage,
                AuthorityIdentityMismatchErrorCode);
        }

        var modelSetProvider =
            SingleInvocationDpoPolicyBaseModelSetProvider.Resolve(
                host.Services);
        if (modelSetProvider is null)
        {
            return OperationResult<TranslationDpoModelServerCoordinatorReport>.Fail(
                ModelSetUnavailableMessage,
                ModelSetUnavailableErrorCode);
        }

        OperationResult<DesignConversionDpoPolicyBaseModelSet>? modelSetResult;
        try
        {
            modelSetResult = await modelSetProvider
                .CreateModelSetAsync(cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return OperationResult<TranslationDpoModelServerCoordinatorReport>.Fail(
                CancelledMessage,
                CancelledErrorCode);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<TranslationDpoModelServerCoordinatorReport>.Fail(
                ModelSetUnavailableMessage,
                ModelSetUnavailableErrorCode);
        }

        if (modelSetResult is null
            || !modelSetResult.IsSuccess
            || modelSetResult.Value is null)
        {
            return OperationResult<TranslationDpoModelServerCoordinatorReport>.Fail(
                ModelSetUnavailableMessage,
                ModelSetUnavailableErrorCode);
        }

        var reproducibilityContext = AdaptServerReproducibilityContext(
            modelSetResult.Value.RandomContext);
        if (!reproducibilityContext.IsSuccess ||
            reproducibilityContext.Value is null)
        {
            return reproducibilityContext.MapError<
                TranslationDpoModelServerCoordinatorReport>();
        }

        SingleModelSetConsumption models;
        try
        {
            models = new SingleModelSetConsumption(modelSetResult.Value);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<TranslationDpoModelServerCoordinatorReport>.Fail(
                ModelSetUnavailableMessage,
                ModelSetUnavailableErrorCode);
        }
        cancellationToken.ThrowIfCancellationRequested();
        var tokenization = new TranslationDpoTokenizationSettings(
            models.ModelConfig.VocabSize,
            models.ModelConfig.ContextLen);
        var initial = await TranslationDpoTrainEvaluateRunner.RunAsync(
                options.SeedCorpusPath,
                models.TakeInitialPolicy,
                models.TakeInitialReference,
                models.TakeFreshLoadTarget,
                broker,
                new TranslationDpoTrainEvaluateRunnerOptions(
                    options.OutputPath,
                    options.CheckpointDirectory,
                    options.TrainingDataArtifactRoot,
                    TokenizationSettings: tokenization,
                    AllowRepositoryOutput: options.AllowRepositoryOutput,
                    ReproducibilityContext: reproducibilityContext.Value,
                    RepositoryOwnedDpoDatasetContext:
                        options.RepositoryOwnedDpoDatasetContext,
                    RepositoryOwnedModelCheckpointContext:
                        options.RepositoryOwnedModelCheckpointContext),
                cancellationToken)
            .ConfigureAwait(false);
        if (!initial.IsSuccess || initial.Value is null)
        {
            // WHY: the CLI must explain which internal guard stopped the
            // training run without leaking private paths from the runner error
            // message. The public coordinator error code stays stable, while
            // the inner code gives the executor a human-debuggable next step.
            var safeInitialMessage = string.IsNullOrWhiteSpace(
                initial.ErrorCode)
                ? InitialTrainingFailedMessage
                : InitialTrainingFailedMessage +
                  " Inner runner error code: " +
                  initial.ErrorCode + ".";
            if (string.Equals(
                    initial.ErrorCode,
                    "HANDOFF_BUILD_FAILED",
                    StringComparison.Ordinal) &&
                !string.IsNullOrWhiteSpace(initial.ErrorMessage))
            {
                safeInitialMessage +=
                    " Handoff reason: " + initial.ErrorMessage + ".";
            }
            return OperationResult<TranslationDpoModelServerCoordinatorReport>.Fail(
                safeInitialMessage,
                InitialTrainingFailedErrorCode);
        }

        TranslationDpoCheckpointLifecycleRunReport? continuation = null;
        if (options.ContinuationRequested)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var continued = TranslationDpoTrainEvaluateRunner
                .RunFreshLoadContinueEvaluate(
                    options.SeedCorpusPath,
                    initial.Value,
                    models.TakePreparedFreshLoadTargetForContinuation,
                    models.TakeContinuationReference,
                    broker,
                    new TranslationDpoCheckpointLifecycleRunnerOptions(
                        options.ContinuationOutputPath!,
                        options.ContinuationCheckpointDirectory!,
                        TokenizationSettings: tokenization,
                        AllowRepositoryOutput: options.AllowRepositoryOutput,
                        ReproducibilityContext: reproducibilityContext.Value));
            if (!continued.IsSuccess || continued.Value is null)
            {
                return OperationResult<TranslationDpoModelServerCoordinatorReport>.Fail(
                    ContinuationTrainingFailedMessage,
                    ContinuationTrainingFailedErrorCode);
            }

            continuation = continued.Value;
        }

        return OperationResult<TranslationDpoModelServerCoordinatorReport>.Ok(
            new TranslationDpoModelServerCoordinatorReport(
                ServerAuthorityLivenessVerified: true,
                ExactServerAuthorityIdentityVerified: true,
                ModelSetCreatedOnce:
                    modelSetProvider.ObservedInvocationCount == 1,
                InitialTrainingCompleted: true,
                ContinuationRequested: options.ContinuationRequested,
                ContinuationTrainingCompleted: continuation is not null,
                Initial: initial.Value,
                Continuation: continuation,
                SemanticQualityClaimed: false,
                ProductReadyClaimed: false,
                HostBoundary: HostBoundary));
    }

    private static bool IsTrainingAuthorityLive(
        ContinuousLearningProductionReadinessStatus readiness)
        => readiness.ConfigurationAdmitted
           && readiness.LegalEvidenceConfigured
           && readiness.LegalEvidenceVerified
           && readiness.CompositionMaterialized
           && readiness.PathIdentityVerified
           && readiness.RetainedObjectIdentityBound
           && readiness.StableDpoExecutorConfigured
           && readiness.TrainingEnabled
           && readiness.CycleExecutionAvailable
           && readiness.PerformsTraining;

    internal static OperationResult<DpoTrainingReproducibilityContext>
        AdaptServerReproducibilityContextForTesting(
            DesignConversionDpoPolicyRandomContext? context)
        => AdaptServerReproducibilityContext(context);

    private static OperationResult<DpoTrainingReproducibilityContext>
        AdaptServerReproducibilityContext(
            DesignConversionDpoPolicyRandomContext? context)
    {
        if (context is null)
        {
            return OperationResult<DpoTrainingReproducibilityContext>.Fail(
                ReproducibilityContextUnavailableMessage,
                ReproducibilityContextUnavailableErrorCode);
        }

        // Только внешний coordinator видит оба мира: DesignConversion отдаёт
        // серверный random context, а TranslationDomain принимает общий DPO
        // contract. Сам домен перевода не получает зависимость на 44-й проект.
        if (!DesignConversionDpoPolicyRandomContextContract.IsValidCurrent(
                context))
        {
            return OperationResult<DpoTrainingReproducibilityContext>.Fail(
                ReproducibilityContextInvalidMessage,
                ReproducibilityContextInvalidErrorCode);
        }

        try
        {
            var seededTopology = DesignConversionDpoPolicySeededLoraTopologyBridge
                .CreateCanonicalSeededLoraTopology();
            return OperationResult<DpoTrainingReproducibilityContext>.Ok(
                DpoTrainingReproducibilityContext.CreateCurrent(
                    context.RootInitializationSeed,
                    context.TrainingConfigurationFingerprint,
                    seededTopology,
                    context.PolicyDropoutStreamDomain,
                    context.ReferenceDropoutStreamDomain,
                    context.FreshLoadDropoutStreamDomain));
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<DpoTrainingReproducibilityContext>.Fail(
                ReproducibilityContextInvalidMessage,
                ReproducibilityContextInvalidErrorCode);
        }
    }

    private static OperationResult<NormalizedOptions> ValidateOptions(
        TranslationDpoModelServerCoordinatorOptions? options)
    {
        if (options is null)
            return InvalidOptions();

        var hasContinuationOutput =
            !string.IsNullOrWhiteSpace(options.ContinuationOutputPath);
        var hasContinuationCheckpoint =
            !string.IsNullOrWhiteSpace(options.ContinuationCheckpointDirectory);
        if (hasContinuationOutput != hasContinuationCheckpoint)
            return InvalidOptions();

        try
        {
            var seed = NormalizeAbsolutePath(options.SeedCorpusPath);
            var output = NormalizeAbsolutePath(options.OutputPath);
            var checkpoint = NormalizeAbsolutePath(options.CheckpointDirectory);
            var artifactRoot = NormalizeAbsolutePath(
                options.TrainingDataArtifactRoot);
            var continuationOutput = hasContinuationOutput
                ? NormalizeAbsolutePath(options.ContinuationOutputPath!)
                : null;
            var continuationCheckpoint = hasContinuationCheckpoint
                ? NormalizeAbsolutePath(options.ContinuationCheckpointDirectory!)
                : null;

            if (SamePath(seed, output)
                || SamePath(output, checkpoint)
                || SamePath(output, artifactRoot)
                || (continuationOutput is not null
                    && (SamePath(continuationOutput, output)
                        || SamePath(continuationOutput, seed)))
                || (continuationCheckpoint is not null
                    && SamePath(continuationCheckpoint, checkpoint)))
            {
                return InvalidOptions();
            }

            var repositoryDpoContext =
                options.RepositoryOwnedDpoDatasetContext ??
                TranslationRepositoryOwnedDpoDatasetContext.Disabled;
            var repositoryDpoValidation = repositoryDpoContext.Validate();
            if (!repositoryDpoValidation.IsSuccess)
                return InvalidOptions();

            var repositoryModelContext =
                options.RepositoryOwnedModelCheckpointContext ??
                TranslationRepositoryOwnedModelCheckpointContext.Disabled;
            var repositoryModelValidation = repositoryModelContext.Validate();
            if (!repositoryModelValidation.IsSuccess)
                return InvalidOptions();
            if (repositoryModelContext.Enabled && !repositoryDpoContext.Enabled)
                return InvalidOptions();

            return OperationResult<NormalizedOptions>.Ok(new NormalizedOptions(
                seed,
                output,
                checkpoint,
                artifactRoot,
                continuationOutput,
                continuationCheckpoint,
                options.AllowRepositoryOutput,
                options.RepositoryOwnedDpoDatasetContext,
                options.RepositoryOwnedModelCheckpointContext));
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return InvalidOptions();
        }
    }

    private static string NormalizeAbsolutePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || !Path.IsPathFullyQualified(path))
            throw new ArgumentException();
        return Path.GetFullPath(path);
    }

    private static bool SamePath(string left, string right)
        => string.Equals(
            Path.TrimEndingDirectorySeparator(left),
            Path.TrimEndingDirectorySeparator(right),
            OperatingSystem.IsWindows()
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal);

    private static OperationResult<NormalizedOptions> InvalidOptions()
        => OperationResult<NormalizedOptions>.Fail(
            InvalidArgumentsMessage,
            InvalidArgumentsErrorCode);

    private sealed class SingleModelSetConsumption
    {
        private readonly LlmModel _initialPolicy;
        private readonly LlmModel _reference;
        private readonly LlmModel _freshLoadTarget;
        private int _initialPolicyTaken;
        private int _initialReferenceTaken;
        private int _freshLoadTargetPreparedForInitial;
        private int _freshLoadTargetTakenForContinuation;
        private int _continuationReferenceTaken;

        internal SingleModelSetConsumption(
            DesignConversionDpoPolicyBaseModelSet modelSet)
        {
            ArgumentNullException.ThrowIfNull(modelSet);
            _initialPolicy = modelSet.Policy
                ?? throw new ArgumentException();
            _reference = modelSet.Reference
                ?? throw new ArgumentException();
            _freshLoadTarget = modelSet.FreshLoadTarget
                ?? throw new ArgumentException();
            if (ReferenceEquals(_initialPolicy, _reference)
                || ReferenceEquals(_initialPolicy, _freshLoadTarget)
                || ReferenceEquals(_reference, _freshLoadTarget))
            {
                throw new ArgumentException();
            }

            ModelConfig = _initialPolicy.Config;
        }

        internal TransformerModel.Config.ModelConfig ModelConfig { get; }

        internal LlmModel TakeInitialPolicy()
            => TakeOnce(_initialPolicy, ref _initialPolicyTaken);

        internal LlmModel TakeInitialReference()
            => TakeOnce(_reference, ref _initialReferenceTaken);

        internal LlmModel TakeFreshLoadTarget()
            => TakeOnce(
                _freshLoadTarget,
                ref _freshLoadTargetPreparedForInitial);

        internal LlmModel TakePreparedFreshLoadTargetForContinuation()
        {
            if (Volatile.Read(ref _freshLoadTargetPreparedForInitial) == 0)
            {
                // Continuation должен получить ту же fresh-load модель, которую
                // initial-run уже включил в атомарную seeded-тройку. Иначе
                // LoadCheckpoint опять окажется раньше подготовки LoRA.
                throw new InvalidOperationException(
                    "The server-owned Translation DPO fresh-load model was not prepared during initial training.");
            }

            return TakeOnce(
                _freshLoadTarget,
                ref _freshLoadTargetTakenForContinuation);
        }

        internal LlmModel TakeContinuationReference()
            => TakeOnce(_reference, ref _continuationReferenceTaken);

        private static LlmModel TakeOnce(LlmModel model, ref int consumed)
            => Interlocked.Exchange(ref consumed, 1) == 0
                ? model
                : throw new InvalidOperationException(
                    "A server-owned Translation DPO model role was consumed more than once.");
    }

    private sealed record NormalizedOptions(
        string SeedCorpusPath,
        string OutputPath,
        string CheckpointDirectory,
        string TrainingDataArtifactRoot,
        string? ContinuationOutputPath,
        string? ContinuationCheckpointDirectory,
        bool AllowRepositoryOutput,
        TranslationRepositoryOwnedDpoDatasetContext?
            RepositoryOwnedDpoDatasetContext,
        TranslationRepositoryOwnedModelCheckpointContext?
            RepositoryOwnedModelCheckpointContext)
    {
        internal bool ContinuationRequested =>
            ContinuationOutputPath is not null;
    }
}
