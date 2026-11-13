// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ModelServer;

using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure.Logical;
using ContinuousLearning;
using DesignConversionDomain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelServer.AutonomousPromotion;
using ModelServer.DesignConversion;
using TrainableRouter.Training;

internal sealed class CodeDocumentationSavedModelAssessmentTrustedRoot
{
    internal CodeDocumentationSavedModelAssessmentTrustedRoot(
        string trustedRepositoryRoot)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(trustedRepositoryRoot);
        RepositoryRoot = Normalize(trustedRepositoryRoot);
    }

    internal string RepositoryRoot { get; }

    internal bool Matches(string repositoryRoot)
    {
        try
        {
            return string.Equals(
                Normalize(repositoryRoot),
                RepositoryRoot,
                OperatingSystem.IsWindows()
                    ? StringComparison.OrdinalIgnoreCase
                    : StringComparison.Ordinal);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return false;
        }
    }

    private static string Normalize(string repositoryRoot)
        => Path.GetFullPath(repositoryRoot).TrimEnd(
            Path.DirectorySeparatorChar,
            Path.AltDirectorySeparatorChar);
}

public static class Program
{
    private static readonly ConditionalWeakTable<
        DesignConversionModelServerEndpointRegistry,
        DesignConversionLocalServiceComposition> LocalLoopbackRegistryCompositions = new();
    private static readonly ConditionalWeakTable<
        DesignConversionModelServerEndpointRegistry,
        IDesignConversionLocalArtifactDb> LocalLoopbackRegistryArtifactDbs = new();

    public const string LocalLoopbackUrl = "http://127.0.0.1:0";
    public const string ReadyFileArgumentName = "--ready-file";
    public const string ReadyFileEnvironmentName = "DESIGN_CONVERSION_MODELSERVER_READY_FILE";
    public const string LocalExecutableHostBoundary =
        "design_conversion_modelserver_executable_asp_net_loopback_host_no_public_deployment_no_product_ready";
    public const string ReadyFileContractBoundary =
        "design_conversion_modelserver_executable_ready_file_loopback_only_readiness_snapshot_no_product_ready";
    public const string LocalLoopbackArtifactUploadAdmissionToken =
        "design-conversion-local-loopback-artifact-upload-route-token";
    public const string LocalLoopbackArtifactUploadTenantId =
        "design-conversion-local-loopback-executable-host";
    public const string LocalLoopbackArtifactUploadRouteScope =
        "design-conversion-modelserver-executable-artifact-upload";
    // Owner-pinned production bootstrap authority: local loopback training may
    // mutate repository-owned artifacts only in the canonical C: checkout, not
    // in an environment-selected clone or another disk.
    internal const string RepositoryOwnedTrainingTrustedRepositoryRoot =
        @"C:\Projects\llm_mvp_core";
    public const string RepositoryOwnedTrainingEnabledEnvironmentName =
        "DESIGN_CONVERSION_MODELSERVER_REPOSITORY_OWNED_TRAINING_ENABLED";
    public const string RepositoryOwnedTrainingRepositoryRootEnvironmentName =
        "DESIGN_CONVERSION_MODELSERVER_REPOSITORY_ROOT";
    public const string RepositoryOwnedTrainingDomainSlugEnvironmentName =
        "DESIGN_CONVERSION_MODELSERVER_REPOSITORY_OWNED_TRAINING_DOMAIN_SLUG";
    public const string RepositoryOwnedTrainingDesignPatternSlugEnvironmentName =
        "DESIGN_CONVERSION_MODELSERVER_REPOSITORY_OWNED_TRAINING_DESIGN_PATTERN_SLUG";
    public const string RepositoryOwnedTrainingDatasetVersionEnvironmentName =
        "DESIGN_CONVERSION_MODELSERVER_REPOSITORY_OWNED_TRAINING_DATASET_VERSION";
    public const string RepositoryOwnedTrainingModelIdSlugEnvironmentName =
        "DESIGN_CONVERSION_MODELSERVER_REPOSITORY_OWNED_TRAINING_MODEL_ID_SLUG";
    public const string RepositoryOwnedTrainingModelVersionEnvironmentName =
        "DESIGN_CONVERSION_MODELSERVER_REPOSITORY_OWNED_TRAINING_MODEL_VERSION";
    public const string RepositoryOwnedTrainingRuntimeConfigSha256EnvironmentName =
        "DESIGN_CONVERSION_MODELSERVER_REPOSITORY_OWNED_TRAINING_RUNTIME_CONFIG_SHA256";
    public const string RepositoryOwnedTrainingRuntimeTopologySha256EnvironmentName =
        "DESIGN_CONVERSION_MODELSERVER_REPOSITORY_OWNED_TRAINING_RUNTIME_TOPOLOGY_SHA256";
    public const string RepositoryOwnedTrainingRuntimeTokenizerSha256EnvironmentName =
        "DESIGN_CONVERSION_MODELSERVER_REPOSITORY_OWNED_TRAINING_RUNTIME_TOKENIZER_SHA256";
    public const string RepositoryOwnedTrainingRuntimeTrainerSha256EnvironmentName =
        "DESIGN_CONVERSION_MODELSERVER_REPOSITORY_OWNED_TRAINING_RUNTIME_TRAINER_SHA256";
    public const string RepositoryOwnedTrainingRuntimeSchemaSha256EnvironmentName =
        "DESIGN_CONVERSION_MODELSERVER_REPOSITORY_OWNED_TRAINING_RUNTIME_SCHEMA_SHA256";
    public const string RepositoryOwnedTrainingRuntimeCompatibilityKeyEnvironmentName =
        "DESIGN_CONVERSION_MODELSERVER_REPOSITORY_OWNED_TRAINING_RUNTIME_COMPATIBILITY_KEY";
    public const string RepositoryOwnedTrainingAllowColdStartEnvironmentName =
        "DESIGN_CONVERSION_MODELSERVER_REPOSITORY_OWNED_TRAINING_ALLOW_COLD_START";
    public const string RepositoryOwnedTrainingColdStartReasonEnvironmentName =
        "DESIGN_CONVERSION_MODELSERVER_REPOSITORY_OWNED_TRAINING_COLD_START_REASON";
    public const string RepositoryOwnedTrainingProvenanceRefsEnvironmentName =
        "DESIGN_CONVERSION_MODELSERVER_REPOSITORY_OWNED_TRAINING_PROVENANCE_REFS";

    public static void Main(string[] args)
    { // Сначала выбираем явно запрошенную offline-стадию, затем обычный серверный запуск.
        if (CodeDocumentationBaselineBootstrapCommand.IsRequested(args)) // Выбираем отдельную offline-команду начального состояния по явному аргументу оператора.
        { // Отделяем этот запуск от обычного HTTP-сервера.
            using var cancellation = new CancellationTokenSource(); // Передаём команде отмену и освобождаем её источник при завершении.
            ConsoleCancelEventHandler cancel = (_, eventArgs) => // Связываем Ctrl+C с отменой текущей bootstrap-операции.
            { // Обрабатываем запрос оператора на остановку.
                eventArgs.Cancel = true; // Даём команде вернуть контролируемый результат вместо немедленного завершения процесса.
                cancellation.Cancel(); // Сообщаем bootstrap-пути об отмене.
            }; // Завершаем обработчик отмены отдельной команды.
            Console.CancelKeyPress += cancel; // Подключаем обработчик на время выполнения bootstrap.
            try // Выполняем команду с обязательным последующим снятием обработчика.
            { // Получаем и выводим результат отдельной стадии.
                var result = CodeDocumentationBaselineBootstrapCommand.Run(cancellation.Token, args); // Запускаем bootstrap с закреплённым корнем и процессными разрешениями.
                Console.WriteLine(result.Message); // Показываем оператору объяснение фактического результата.
                Console.WriteLine($"{result.Status} / NO_TRAINING" + // Отдельно обозначаем статус и отсутствие обучения.
                    (result.ErrorCode is null ? string.Empty : $" / {result.ErrorCode}")); // Добавляем точный код отказа, если он есть.
                if (result.Publication is { } publication) // Выводим идентификаторы только при наличии результата публикации.
                    Console.WriteLine($"step={publication.Step}; manifest={publication.ManifestSha256}; weights={publication.WeightsSha256}"); // Показываем шаг и хэши сохранённых артефактов для сверки.
                Environment.ExitCode = result.ExitCode; // Передаём вызывающему процессу успех, отказ или необходимость сверки.
            } // Завершаем обработку результата команды.
            finally // Снимаем подписку также при ошибке или отмене.
            { // Очищаем только обработчик этого запуска.
                Console.CancelKeyPress -= cancel; // Удаляем временную подписку на Ctrl+C.
            } // Завершаем очистку обработчика.
            return; // Завершаем процесс без запуска HTTP-хоста после bootstrap.
        } // Обычный серверный запуск продолжается только без bootstrap-аргумента.
        CreateLocalLoopbackHost(args).Run(); // Без bootstrap-команды запускаем прежний локальный HTTP-хост.
    } // Завершаем process entry без дополнительного пути создания модели.

    public static WebApplication CreateLocalLoopbackHost(
        string[]? args = null,
        IDesignConversionDpoPolicyBaseProvider? dpoPolicyBaseProvider = null,
        DesignConversionTrainingIntakeDpoPairStoreOptions?
            trainingIntakeDpoPairStoreOptions = null)
        => CreateLocalLoopbackHostOwnedStartupCore( // Route the public Host through a private core retaining the process-owned startup instance.
            args,
            Environment.GetEnvironmentVariable,
            RepositoryOwnedTrainingTrustedRepositoryRoot,
            ContinuousLearningModelServerStartup.CreateProtectedFromProcessEnvironment(), // Capture protected provenance at the actual public process entry before composing the Host.
            dpoPolicyBaseProvider,
            trainingIntakeDpoPairStoreOptions);

    public static WebApplication CreateLocalLoopbackHost(
        CodeDocumentationGenerationOptions generationOptions,
        string[]? args = null,
        IDesignConversionDpoPolicyBaseProvider? dpoPolicyBaseProvider = null,
        DesignConversionTrainingIntakeDpoPairStoreOptions? trainingIntakeDpoPairStoreOptions = null)
        => CreateLocalLoopbackHostOwnedStartupCore(args, Environment.GetEnvironmentVariable,
            RepositoryOwnedTrainingTrustedRepositoryRoot,
            ContinuousLearningModelServerStartup.CreateProtectedFromProcessEnvironment(),
            dpoPolicyBaseProvider, trainingIntakeDpoPairStoreOptions, generationOptions: generationOptions);

    internal static WebApplication CreateLocalLoopbackHostCore(
        string[]? args,
        Func<string, string?> readEnvironment,
        string trustedRepositoryRoot,
        IDesignConversionDpoPolicyBaseProvider? dpoPolicyBaseProvider = null,
        DesignConversionTrainingIntakeDpoPairStoreOptions?
            trainingIntakeDpoPairStoreOptions = null,
        DesignConversionModelServerRepositoryOwnedTrainingOptions?
            repositoryOwnedTrainingOptions = null,
        ICodeDocumentationMaterialBoundaryProvider?
            materialBoundaryProvider = null,
        ICodeDocumentationCertificationAuthorityProvider?
            certificationAuthorityProvider = null,
        ICodeDocumentationSavedModelAssessmentEvaluationClientFactory?
            evaluationClientFactory = null,
        RoutingProfileTrainer? profileTrainer = null,
        CodeDocumentationGenerationOptions? generationOptions = null)
        => CreateLocalLoopbackHostOwnedStartupCore(args, readEnvironment, trustedRepositoryRoot, // Keep the callback test/core entry on the same private Host implementation.
            ContinuousLearningModelServerStartup.Create(readEnvironment), dpoPolicyBaseProvider, // Compose callback startup with its ordinary reader so byte-identical data cannot mint process provenance.
            trainingIntakeDpoPairStoreOptions, repositoryOwnedTrainingOptions, materialBoundaryProvider, // Forward the existing training options and material boundary without a new approval injection seam.
            certificationAuthorityProvider, evaluationClientFactory, profileTrainer, generationOptions); // Preserve trusted host generation configuration, never request fields.

    private static WebApplication CreateLocalLoopbackHostOwnedStartupCore( // Share Host composition privately while retaining the exact startup chosen by each entry point.
        string[]? args, Func<string, string?> readEnvironment, string trustedRepositoryRoot, // Keep the existing arguments, reader and fixed trusted-root contract unchanged.
        ContinuousLearningModelServerStartup continuousLearning, // Retain the entry point's startup object instead of reconstructing its provenance later.
        IDesignConversionDpoPolicyBaseProvider? dpoPolicyBaseProvider = null, // Preserve the existing optional policy-base dependency for ordinary Host composition.
        DesignConversionTrainingIntakeDpoPairStoreOptions? trainingIntakeDpoPairStoreOptions = null, // Preserve existing intake pair-store options without granting teaching disclosure authority.
        DesignConversionModelServerRepositoryOwnedTrainingOptions? repositoryOwnedTrainingOptions = null, // Preserve ordinary repository training options on the existing private core.
        ICodeDocumentationMaterialBoundaryProvider? materialBoundaryProvider = null, // Keep the existing material boundary dependency separate from startup approval provenance.
        ICodeDocumentationCertificationAuthorityProvider? certificationAuthorityProvider = null, // Keep the existing certification provider dependency without making it an approval issuer.
        ICodeDocumentationSavedModelAssessmentEvaluationClientFactory? evaluationClientFactory = null, // Keep the existing frozen evaluation-client dependency on the same composition path.
        RoutingProfileTrainer? profileTrainer = null,
        CodeDocumentationGenerationOptions? generationOptions = null)
    {
        try // Enclose existing Host construction so a failed construction cannot leave approval live.
        { // Run the unchanged composition body under that revocation boundary.
        ArgumentNullException.ThrowIfNull(readEnvironment);
        var normalizedArgs = args ?? [];
        var readyFile = LocalLoopbackReadyFileOptions.Resolve(
            normalizedArgs,
            readEnvironment);
        var effectiveRepositoryOwnedTrainingOptions =
            repositoryOwnedTrainingOptions is null
                ? LocalLoopbackRepositoryOwnedTrainingConfiguration.Resolve(
                    readEnvironment,
                    trustedRepositoryRoot)
                : LocalLoopbackRepositoryOwnedTrainingConfiguration
                    .ValidateExplicitOptions(
                        repositoryOwnedTrainingOptions,
                        trustedRepositoryRoot);
        if (readyFile is not null)
            LocalLoopbackReadyFile.RemoveStale(readyFile.FilePath);
        var autonomousPromotionEvaluation =
            AutonomousPromotionEvaluationConfigurationLoader.ResolveEndpoint(
                normalizedArgs,
                readEnvironment);
        if (continuousLearning.TrainingActivationRequested
            && dpoPolicyBaseProvider is not null)
        {
            throw new InvalidOperationException(
                "The explicit ContinuousLearning training-intake cycle requires the exact server-owned policy-base provider.");
        }

        CodeDocumentationCertificationSessionStore?
            serverOwnedMaterialBoundaryStore = null;
        var effectiveMaterialBoundaryProvider = materialBoundaryProvider;
        var assessmentProfiles = CodeDocumentationSavedModelAssessmentProfileResolver
            .Resolve(readEnvironment);
        var trustedAssessmentRoot =
            new CodeDocumentationSavedModelAssessmentTrustedRoot(
                trustedRepositoryRoot);
        var effectiveCertificationAuthorityProvider =
            certificationAuthorityProvider;
        if (effectiveCertificationAuthorityProvider is null)
        {
            var provisioning = CodeDocumentationCertificationProvisioning
                .LoadProvider(readEnvironment, trustedRepositoryRoot);
            if (provisioning.IsSuccess && provisioning.Value is not null)
                effectiveCertificationAuthorityProvider = provisioning.Value;
        }

        if (effectiveMaterialBoundaryProvider is null &&
            (continuousLearning.HasDeferredCompositionPlan ||
             assessmentProfiles.IsSuccess))
        {
            var materialBoundaryRegistration =
                CreateServerOwnedMaterialBoundaryRegistration(
                    trustedRepositoryRoot);
            serverOwnedMaterialBoundaryStore =
                materialBoundaryRegistration.Store;
            effectiveMaterialBoundaryProvider =
                materialBoundaryRegistration.Provider;
        }

        var builder = CreateLocalLoopbackBuilder(
            normalizedArgs,
            continuousLearning);
        if (profileTrainer is not null)
            builder.Services.AddSingleton(profileTrainer);
        builder.Services.AddSingleton(trustedAssessmentRoot);
        var generationConfiguration = generationOptions is null
            ? CodeDocumentationGenerationConfiguration.Resolve(readEnvironment, trustedRepositoryRoot)
            : new CodeDocumentationGenerationConfigurationResult(generationOptions, null);
        builder.Services.AddSingleton(generationConfiguration);
        builder.Services.AddSingleton(CodeDocumentationInitialTrainingConfiguration.Resolve(readEnvironment, generationConfiguration)); // Capture explicit first-B0 optimizer choice beside the existing generation configuration owner; default remains disabled.
        builder.Services.AddSingleton(CodeDocumentationScientificTrainingConfiguration.Resolve(readEnvironment)); // Capture separate first-RP01 scientific hold/publish trust; disabled by default.
        generationOptions = generationConfiguration.Options;
        if (generationOptions is not null)
        {
            if (!trustedAssessmentRoot.Matches(generationOptions.StartRequest.RepositoryRoot))
                throw new InvalidOperationException("CODE_DOCUMENTATION_GENERATION_TRUSTED_ROOT_MISMATCH");
            builder.Services.AddSingleton(new CodeDocumentationGenerationService(generationOptions));
        }
        if (effectiveCertificationAuthorityProvider is not null)
        {
            builder.Services.AddSingleton<ICodeDocumentationCertificationAuthorityProvider>(
                effectiveCertificationAuthorityProvider);
        }
        if (evaluationClientFactory is not null)
            builder.Services.AddSingleton(evaluationClientFactory);
        if (serverOwnedMaterialBoundaryStore is not null &&
            effectiveMaterialBoundaryProvider is not null)
        {
            var ownedStore = serverOwnedMaterialBoundaryStore;
            var ownedProvider = effectiveMaterialBoundaryProvider;
            builder.Services.AddSingleton(
                _ => new CodeDocumentationServerOwnedMaterialBoundaryLifetime(
                    ownedProvider,
                    ownedStore));
            builder.Services.AddSingleton<ICodeDocumentationMaterialBoundaryProvider>(
                services => services.GetRequiredService<
                    CodeDocumentationServerOwnedMaterialBoundaryLifetime>());
        }
        else if (effectiveMaterialBoundaryProvider is not null)
        {
            builder.Services.AddSingleton(effectiveMaterialBoundaryProvider);
        }

        WebApplication? builtApp = null;
        try
        {
            builtApp = builder.Build();
            if (serverOwnedMaterialBoundaryStore is not null)
            {
                // Resolve the DI-created owner before returning the host so
                // the container owns the store for the complete host lifetime.
                _ = builtApp.Services.GetRequiredService<
                    CodeDocumentationServerOwnedMaterialBoundaryLifetime>();
                serverOwnedMaterialBoundaryStore = null;
            }
        }
        catch
        {
            if (builtApp is not null)
            {
                try { builtApp.DisposeAsync().AsTask().GetAwaiter().GetResult(); }
                catch { }
            }
            serverOwnedMaterialBoundaryStore?.Dispose();
            throw;
        }
        var app = builtApp!;
        ContinuousLearningDesignConversionRouteMappingCapability?
            routeCapability = null;
        DesignConversionModelServerEndpointRegistry? registry = null;
        try
        {
            var serverOwnedDpoPolicyBaseProvider = app.Services.GetService<
                DesignConversionRetainedDpoPolicyBaseProviderGateway>();
            var effectiveDpoPolicyBaseProvider = dpoPolicyBaseProvider
                ?? serverOwnedDpoPolicyBaseProvider;
            var legalTeacherAdmissionAuthority = app.Services.GetRequiredService<
                ServerOwnedLegalTrainingTeacherAdmissionAuthority>();
            var serverOwnedLegalTeacherAdmissionAuthority =
                ResolveServerOwnedLegalTeacherAdmissionAuthority(
                    dpoPolicyBaseProvider,
                    serverOwnedDpoPolicyBaseProvider,
                    legalTeacherAdmissionAuthority);
            // Loopback host использует продуктовый конструктор: explicit native
            // pair-store root проходит в resolver, а не в тестовую подмену store.
            var artifactDb = CreateLocalLoopbackArtifactDb(
                readyFile?.FilePath,
                effectiveDpoPolicyBaseProvider,
                serverOwnedLegalTeacherAdmissionAuthority,
                trainingIntakeDpoPairStoreOptions);
            var routeMappingIssuer = continuousLearning.TrainingActivationRequested
                ? continuousLearning
                    .PrepareDesignConversionTrainingIntakeRouteMappingIssuer(
                        artifactDb,
                        serverOwnedDpoPolicyBaseProvider!,
                        serverOwnedLegalTeacherAdmissionAuthority!)
                    ?? throw new InvalidOperationException(
                        "The explicit ContinuousLearning route capability issuer could not be prepared.")
                : null;
            var localPngArtifactRoot = CreateLocalLoopbackArtifactUploadRoot(
                readyFile?.FilePath);
            var repositoryOwnedDpoTeacherConfiguration =
                continuousLearning.HasDeferredCompositionPlan
                    ? continuousLearning.TeacherConfiguration
                    : null;
            registry = continuousLearning
                .LocalIndependentPreferenceEvaluatorRequested
                ? CreateLocalLoopbackRouteRegistryWithLocalIndependentPreferenceEvaluatorAndLocalPngArtifactRoot(
                    artifactDb,
                    continuousLearning.TeacherConfiguration,
                    localPngArtifactRoot,
                    repositoryOwnedTrainingOptions:
                        effectiveRepositoryOwnedTrainingOptions,
                    repositoryOwnedDpoTeacherConfiguration:
                        repositoryOwnedDpoTeacherConfiguration,
                    trustedRepositoryRoot:
                        trustedRepositoryRoot)
                : CreateLocalLoopbackRouteRegistryWithLocalPngArtifactRoot(
                    localPngArtifactRoot,
                    artifactDb,
                    repositoryOwnedTrainingOptions:
                        effectiveRepositoryOwnedTrainingOptions,
                    repositoryOwnedDpoTeacherConfiguration:
                        repositoryOwnedDpoTeacherConfiguration,
                    trustedRepositoryRoot:
                        trustedRepositoryRoot);
            ConfigureLocalLoopbackRoutesCore(
                app,
                localPngArtifactRoot,
                registry,
                autonomousPromotionEvaluation,
                trustedRepositoryRoot);
            if (routeMappingIssuer is not null)
            {
                if (!LocalLoopbackRegistryArtifactDbs.TryGetValue(
                        registry,
                        out var mappedArtifactDb)
                    || !ReferenceEquals(mappedArtifactDb, artifactDb)
                    || !ReferenceEquals(
                        artifactDb.DpoPolicyBaseProvider,
                        serverOwnedDpoPolicyBaseProvider)
                    || !ReferenceEquals(
                        artifactDb.LegalTeacherAdmissionAuthority,
                        serverOwnedLegalTeacherAdmissionAuthority))
                {
                    throw new InvalidOperationException(
                        "The exact mapped DesignConversion route surface identity was lost.");
                }

                routeCapability = new MappedDesignConversionRouteCapability(
                    routeMappingIssuer,
                    registry,
                    artifactDb,
                    serverOwnedDpoPolicyBaseProvider!,
                    serverOwnedLegalTeacherAdmissionAuthority!);
                if (!continuousLearning
                        .TryBindDesignConversionTrainingIntakeRouteMapping(
                            routeCapability))
                {
                    routeCapability.Revoke();
                    throw new InvalidOperationException(
                        "The mapped DesignConversion route capability could not be bound.");
                }
            }

            if (readyFile is not null)
            {
                ConfigureLocalLoopbackReadyFileLifecycle(
                    app,
                    readyFile.FilePath,
                    continuousLearning,
                    autonomousPromotionEvaluation.IsConfigured);
            }
            return app;
        }
        catch (Exception original)
        {
            var failures = new LifecycleCleanupArbiter();
            failures.CaptureOriginal(original);
            try { routeCapability?.Revoke(); }
            catch (Exception cleanup) { failures.CaptureCleanup(cleanup); }
            try { registry?.Dispose(); }
            catch (Exception cleanup) { failures.CaptureCleanup(cleanup); }
            try { app.DisposeAsync().AsTask().GetAwaiter().GetResult(); }
            catch (Exception cleanup) { failures.CaptureCleanup(cleanup); }
            failures.ThrowIfAny();
            throw;
        }
        } // Finish the existing Host construction/cleanup before the outer revocation catch.
        catch { continuousLearning.RevokeProtectedTeachingDisclosureApprovals(); throw; } // Revoke the retained startup on construction failure and rethrow the original exception.
    }

    private static (
        CodeDocumentationCertificationSessionStore Store,
        ICodeDocumentationMaterialBoundaryProvider Provider)
        CreateServerOwnedMaterialBoundaryRegistration(
            string trustedRepositoryRoot)
    {
        var expectedStoreId =
            ComputeServerOwnedMaterialBoundaryStoreId(trustedRepositoryRoot);
        var opened = CodeDocumentationCertificationSessionStore
            .OpenExistingForServerOwnedRoot(
                trustedRepositoryRoot,
                expectedStoreId);
        var store = opened.IsSuccess && opened.Value is not null
            ? opened.Value
            : opened.ErrorCode ==
              CodeDocumentationCertificationSessionStore.ErrorPrefix +
              "STORE_MISSING"
                ? CreateServerOwnedMaterialBoundaryStore(
                    trustedRepositoryRoot,
                    expectedStoreId)
                : throw new InvalidOperationException(
                    "The server-owned diagnostic material history could not be opened: " +
                    opened.ErrorCode);
        var provider = CodeDocumentationServerOwnedMaterialBoundaryProvider.Create(
            trustedRepositoryRoot,
            expectedStoreId,
            store);
        if (!provider.IsSuccess || provider.Value is null)
        {
            store.Dispose();
            throw new InvalidOperationException(
                "The server-owned diagnostic material boundary could not be composed: " +
                provider.ErrorCode);
        }

        return (store, provider.Value);
    }

    internal static CodeDocumentationCertificationSessionStore
        CreateServerOwnedMaterialBoundaryStore(
            string trustedRepositoryRoot,
            string expectedStoreId,
            Action? beforeCreate = null)
    {
        beforeCreate?.Invoke();
        var created = CodeDocumentationCertificationSessionStore
            .CreateNewForServerOwnedRoot(
                trustedRepositoryRoot,
                expectedStoreId);
        if (created.IsSuccess && created.Value is not null)
            return created.Value;

        // Another server may have completed the first initialization between
        // OpenExisting and CreateNew. The marker is published as the final
        // initialization step, so CreateNew can observe HISTORY_EXPECTED even
        // though a complete matching store now exists. Reopen both signals;
        // if the store is absent, partial or foreign, OpenExisting fails and
        // this path remains fail-closed rather than creating fresh history.
        if (created.ErrorCode is
            CodeDocumentationCertificationSessionStore.ErrorPrefix + "STORE_EXISTS" or
            CodeDocumentationCertificationSessionStore.ErrorPrefix + "HISTORY_EXPECTED")
        {
            var reopened = CodeDocumentationCertificationSessionStore
                .OpenExistingForServerOwnedRoot(
                    trustedRepositoryRoot,
                    expectedStoreId);
            if (reopened.IsSuccess && reopened.Value is not null)
                return reopened.Value;
            throw new InvalidOperationException(
                "The server-owned diagnostic material history could not be reopened after initialization race: " +
                reopened.ErrorCode);
        }

        throw new InvalidOperationException(
            "The server-owned diagnostic material history could not be initialized: " +
            created.ErrorCode);
    }

    internal static string ComputeServerOwnedMaterialBoundaryStoreId(
        string trustedRepositoryRoot)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(trustedRepositoryRoot);
        var canonicalRoot = Path.GetFullPath(trustedRepositoryRoot)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(
            "code-documentation-certification-session-store-v1\n" +
            canonicalRoot))).ToLowerInvariant();
    }

    internal static ServerOwnedLegalTrainingTeacherAdmissionAuthority?
        ResolveServerOwnedLegalTeacherAdmissionAuthority(
            IDesignConversionDpoPolicyBaseProvider? callerProvidedProvider,
            IDesignConversionDpoPolicyBaseProvider? serverOwnedProvider,
            ServerOwnedLegalTrainingTeacherAdmissionAuthority?
                serverOwnedAuthority)
        => callerProvidedProvider is null &&
           serverOwnedProvider is not null &&
           serverOwnedAuthority is { IsAvailable: true }
            ? serverOwnedAuthority
            : null;

    public static WebApplicationBuilder CreateLocalLoopbackBuilder(string[]? args = null)
        => CreateLocalLoopbackBuilder(
            args ?? [],
            ContinuousLearningModelServerStartup.CreateProtectedFromProcessEnvironment()); // Give the public Builder the same no-argument process-owned provenance as public Host.

    private static WebApplicationBuilder CreateLocalLoopbackBuilder(
        string[] args,
        ContinuousLearningModelServerStartup continuousLearning)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = args,
            ApplicationName = typeof(Program).Assembly.FullName
        });
        builder.WebHost.UseKestrel();
        builder.WebHost.UseUrls(LocalLoopbackUrl);
        builder.Logging.ClearProviders();

        builder.Services.AddSingleton(continuousLearning);
        builder.Services.AddSingleton<RoutingProfileTrainer>();
        builder.Services.AddSingleton(services => new CodeDocumentationTrainingRunner(
            services.GetRequiredService<RoutingProfileTrainer>()));
        builder.Services.AddSingleton(services => new CodeDocumentationBatchFileRunner(
            services.GetRequiredService<CodeDocumentationTrainingRunner>()));
        builder.Services.AddSingleton(
            continuousLearning.LegalTeacherAdmissionAuthority);
        builder.Services.AddSingleton<IServerOwnedDpoTrainingLeaseBroker>(
            continuousLearning.LegalTeacherAdmissionAuthority);
        if (continuousLearning.HasDeferredCompositionPlan)
        {
            builder.Services.AddSingleton(
                new DesignConversionRetainedDpoPolicyBaseProviderGateway());
            builder.Services.AddSingleton<IDesignConversionDpoPolicyBaseProvider>(
                services => services.GetRequiredService<
                    DesignConversionRetainedDpoPolicyBaseProviderGateway>());
            builder.Services.AddSingleton<ServerOwnedContinuousLearningFileAccess>(
                _ =>
                {
                    try
                    {
                        return continuousLearning.AcquireRetainedFileAccess();
                    }
                    catch (Exception exception)
                        when (exception is ContinuousLearningPathIdentityException
                              or ContinuousLearningMutationAdmissionDomainAlreadyOwnedException)
                    {
                        continuousLearning
                            .MarkRetainedFileAccessAcquisitionFailed(exception);
                        throw;
                    }
                });
            builder.Services.AddHostedService<
                ContinuousLearningRetainedObjectHostedActivator>();
            builder.Services.AddHostedService<
                DesignConversionDpoTeacherAttestationHostedAssociation>();
        }
        return builder;
    }

    public static void ConfigureLocalLoopbackRoutes(
        WebApplication app,
        string? artifactUploadRoot = null,
        IDesignConversionLocalArtifactDb? artifactDb = null,
        AutonomousPromotionEvaluationEndpoint? autonomousPromotionEvaluation = null)
    {
        var localPngArtifactRoot = ResolveLocalLoopbackPngArtifactRoot(
            artifactUploadRoot);
        ConfigureLocalLoopbackRoutesCore(
            app,
            localPngArtifactRoot,
            CreateLocalLoopbackRouteRegistryWithLocalPngArtifactRoot(
                localPngArtifactRoot,
                artifactDb),
            autonomousPromotionEvaluation,
            RepositoryOwnedTrainingTrustedRepositoryRoot);
    }

    private static void ConfigureLocalLoopbackRoutesCore(
        WebApplication app,
        string? artifactUploadRoot,
        DesignConversionModelServerEndpointRegistry registry,
        AutonomousPromotionEvaluationEndpoint? autonomousPromotionEvaluation,
        string trustedRepositoryRoot)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(registry);

        var startup = app.Services.GetRequiredService<ContinuousLearningModelServerStartup>(); // Read the exact startup singleton already retained by this Host's DI container.
        app.Lifetime.ApplicationStopping.Register(startup.RevokeProtectedTeachingDisclosureApprovals); // Revoke that same startup when this Host begins stopping.

        app.Use(async (context, next) =>
        {
            context.Response.OnStarting(() =>
            {
                ApplyLocalExecutableBoundaryHeaders(context.Response);
                return Task.CompletedTask;
            });
            await next(context);
        });

        app.ConfigureDesignConversionModelServer(
            registry,
            CreateLocalLoopbackArtifactUploadHandler(artifactUploadRoot));
        app.MapAutonomousPromotionEvaluation(
            autonomousPromotionEvaluation
            ?? AutonomousPromotionEvaluationEndpoint.NotConfigured());
        app.MapContinuousLearningReadiness();
        app.MapCodeDocumentationRepositoryOwnedDpoTrainingCore(
            trustedRepositoryRoot);
        app.MapCodeDocumentationOneLessonPreflight(trustedRepositoryRoot);
        app.MapCodeDocumentationSavedModelAssessment();
        app.MapCodeDocumentationGeneration();
    }

    public static DesignConversionModelServerEndpointRegistry CreateLocalLoopbackRouteRegistry(
        IDesignConversionLocalArtifactDb? artifactDb = null,
        IDesignConversionDpoPolicyBaseProvider? dpoPolicyBaseProvider = null,
        DesignConversionModelServerMeasurementGateThresholdConfiguration?
            measurementGateThresholdConfiguration = null,
        DesignConversionTrainingIntakeDpoPairStoreOptions?
            trainingIntakeDpoPairStoreOptions = null)
        => CreateLocalLoopbackRouteRegistryCore(
            artifactDb,
            dpoPolicyBaseProvider,
            measurementGateThresholdConfiguration,
            localIndependentPreferenceEvaluatorConfiguration: null,
            localPngArtifactRoot: null,
            trainingIntakeDpoPairStoreOptions:
                trainingIntakeDpoPairStoreOptions);

    internal static DesignConversionModelServerEndpointRegistry
        CreateLocalLoopbackRouteRegistryWithLocalPngArtifactRoot(
            string localPngArtifactRoot,
            IDesignConversionLocalArtifactDb? artifactDb = null,
            IDesignConversionDpoPolicyBaseProvider? dpoPolicyBaseProvider = null,
            DesignConversionModelServerMeasurementGateThresholdConfiguration?
                measurementGateThresholdConfiguration = null,
            DesignConversionTrainingIntakeDpoPairStoreOptions?
                trainingIntakeDpoPairStoreOptions = null,
            DesignConversionModelServerRepositoryOwnedTrainingOptions?
                repositoryOwnedTrainingOptions = null,
            ContinuousLearningTp1DpoConfiguration?
                repositoryOwnedDpoTeacherConfiguration = null,
            string? trustedRepositoryRoot = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(localPngArtifactRoot);
        return CreateLocalLoopbackRouteRegistryCore(
            artifactDb,
            dpoPolicyBaseProvider,
            measurementGateThresholdConfiguration,
            localIndependentPreferenceEvaluatorConfiguration: null,
            localPngArtifactRoot: localPngArtifactRoot,
            trainingIntakeDpoPairStoreOptions:
                trainingIntakeDpoPairStoreOptions,
            repositoryOwnedTrainingOptions:
                repositoryOwnedTrainingOptions,
            repositoryOwnedDpoTeacherConfiguration:
                repositoryOwnedDpoTeacherConfiguration,
            trustedRepositoryRoot:
                trustedRepositoryRoot);
    }

    internal static DesignConversionModelServerEndpointRegistry
        CreateLocalLoopbackRouteRegistryWithLocalIndependentPreferenceEvaluator(
            IDesignConversionLocalArtifactDb artifactDb,
            ContinuousLearningTp1DpoConfiguration
                localIndependentPreferenceEvaluatorConfiguration,
            DesignConversionModelServerMeasurementGateThresholdConfiguration?
                measurementGateThresholdConfiguration = null,
            DesignConversionModelServerRepositoryOwnedTrainingOptions?
                repositoryOwnedTrainingOptions = null,
            ContinuousLearningTp1DpoConfiguration?
                repositoryOwnedDpoTeacherConfiguration = null,
            string? trustedRepositoryRoot = null)
        => CreateLocalLoopbackRouteRegistryWithLocalIndependentPreferenceEvaluatorCore(
            artifactDb,
            localIndependentPreferenceEvaluatorConfiguration,
            measurementGateThresholdConfiguration,
            localPngArtifactRoot: null,
            repositoryOwnedTrainingOptions:
                repositoryOwnedTrainingOptions,
            repositoryOwnedDpoTeacherConfiguration:
                repositoryOwnedDpoTeacherConfiguration,
            trustedRepositoryRoot:
                trustedRepositoryRoot);

    internal static DesignConversionModelServerEndpointRegistry
        CreateLocalLoopbackRouteRegistryWithLocalIndependentPreferenceEvaluatorAndLocalPngArtifactRoot(
            IDesignConversionLocalArtifactDb artifactDb,
            ContinuousLearningTp1DpoConfiguration
                localIndependentPreferenceEvaluatorConfiguration,
            string localPngArtifactRoot,
            DesignConversionModelServerMeasurementGateThresholdConfiguration?
                measurementGateThresholdConfiguration = null,
            DesignConversionModelServerRepositoryOwnedTrainingOptions?
                repositoryOwnedTrainingOptions = null,
            ContinuousLearningTp1DpoConfiguration?
                repositoryOwnedDpoTeacherConfiguration = null,
            string? trustedRepositoryRoot = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(localPngArtifactRoot);
        return CreateLocalLoopbackRouteRegistryWithLocalIndependentPreferenceEvaluatorCore(
            artifactDb,
            localIndependentPreferenceEvaluatorConfiguration,
            measurementGateThresholdConfiguration,
            localPngArtifactRoot,
            repositoryOwnedTrainingOptions,
            repositoryOwnedDpoTeacherConfiguration,
            trustedRepositoryRoot);
    }

    private static DesignConversionModelServerEndpointRegistry
        CreateLocalLoopbackRouteRegistryWithLocalIndependentPreferenceEvaluatorCore(
            IDesignConversionLocalArtifactDb artifactDb,
            ContinuousLearningTp1DpoConfiguration
                localIndependentPreferenceEvaluatorConfiguration,
            DesignConversionModelServerMeasurementGateThresholdConfiguration?
                measurementGateThresholdConfiguration,
            string? localPngArtifactRoot,
            DesignConversionModelServerRepositoryOwnedTrainingOptions?
                repositoryOwnedTrainingOptions,
            ContinuousLearningTp1DpoConfiguration?
                repositoryOwnedDpoTeacherConfiguration,
            string? trustedRepositoryRoot)
    {
        ArgumentNullException.ThrowIfNull(artifactDb);
        ArgumentNullException.ThrowIfNull(
            localIndependentPreferenceEvaluatorConfiguration);
        return CreateLocalLoopbackRouteRegistryCore(
            artifactDb,
            dpoPolicyBaseProvider: null,
            measurementGateThresholdConfiguration:
                measurementGateThresholdConfiguration,
            localIndependentPreferenceEvaluatorConfiguration:
                localIndependentPreferenceEvaluatorConfiguration,
            localPngArtifactRoot: localPngArtifactRoot,
            repositoryOwnedTrainingOptions:
                repositoryOwnedTrainingOptions,
            repositoryOwnedDpoTeacherConfiguration:
                repositoryOwnedDpoTeacherConfiguration,
            trustedRepositoryRoot:
                trustedRepositoryRoot);
    }

    private static DesignConversionModelServerEndpointRegistry
        CreateLocalLoopbackRouteRegistryCore(
            IDesignConversionLocalArtifactDb? artifactDb,
            IDesignConversionDpoPolicyBaseProvider? dpoPolicyBaseProvider,
            DesignConversionModelServerMeasurementGateThresholdConfiguration?
                measurementGateThresholdConfiguration,
            ContinuousLearningTp1DpoConfiguration?
                localIndependentPreferenceEvaluatorConfiguration,
            string? localPngArtifactRoot,
            DesignConversionTrainingIntakeDpoPairStoreOptions?
                trainingIntakeDpoPairStoreOptions = null,
            DesignConversionModelServerRepositoryOwnedTrainingOptions?
                repositoryOwnedTrainingOptions = null,
            ContinuousLearningTp1DpoConfiguration?
                repositoryOwnedDpoTeacherConfiguration = null,
            string? trustedRepositoryRoot = null)
    {
        var resolvedLocalPngArtifactRoot = ResolveLocalLoopbackPngArtifactRoot(
            localPngArtifactRoot);
        var composition = CreateLocalLoopbackServiceCompositionWithLocalPngArtifactRoot(
            resolvedLocalPngArtifactRoot,
            measurementGateThresholdConfiguration);
        var artifactRefResolver =
            new DesignConversionServerOwnedLocalArtifactRefResolver(
                resolvedLocalPngArtifactRoot);
        var effectiveArtifactDb = artifactDb ?? CreateLocalLoopbackArtifactDb(
            dpoPolicyBaseProvider: dpoPolicyBaseProvider,
            trainingIntakeDpoPairStoreOptions:
                trainingIntakeDpoPairStoreOptions);
        var effectiveRepositoryOwnedTrainingOptions =
            LocalLoopbackRepositoryOwnedTrainingConfiguration
                .ValidateExplicitOptionsOrMissing(
                    repositoryOwnedTrainingOptions,
                    trustedRepositoryRoot);
        var registry = localIndependentPreferenceEvaluatorConfiguration is null
            ? DesignConversionModelServerAspNetHostStartup.CreateLocalRouteRegistry(
                composition,
                new DesignConversionRuntimeImprovementCandidateStrategy(),
                effectiveArtifactDb,
                artifactRefResolver: artifactRefResolver,
                measurementGateThresholdConfiguration:
                    measurementGateThresholdConfiguration,
                repositoryOwnedTrainingOptions:
                    effectiveRepositoryOwnedTrainingOptions,
                repositoryOwnedDpoTeacherConfiguration:
                    repositoryOwnedDpoTeacherConfiguration)
            : DesignConversionModelServerAspNetHostStartup
                .CreateLocalRouteRegistryWithLocalIndependentPreferenceEvaluator(
                    composition,
                    new DesignConversionRuntimeImprovementCandidateStrategy(),
                    effectiveArtifactDb,
                    localIndependentPreferenceEvaluatorConfiguration,
                    measurementGateThresholdConfiguration,
                    artifactRefResolver: artifactRefResolver,
                    repositoryOwnedTrainingOptions:
                        effectiveRepositoryOwnedTrainingOptions,
                    repositoryOwnedDpoTeacherConfiguration:
                        repositoryOwnedDpoTeacherConfiguration ??
                        localIndependentPreferenceEvaluatorConfiguration);
        LocalLoopbackRegistryCompositions.Add(registry, composition);
        LocalLoopbackRegistryArtifactDbs.Add(registry, effectiveArtifactDb);
        return registry;
    }

    internal static bool TryGetLocalLoopbackRouteRegistryServiceComposition(
        DesignConversionModelServerEndpointRegistry registry,
        out DesignConversionLocalServiceComposition? composition)
    {
        ArgumentNullException.ThrowIfNull(registry);
        return LocalLoopbackRegistryCompositions.TryGetValue(registry, out composition);
    }

    public static DesignConversionLocalServiceComposition CreateLocalLoopbackServiceComposition(
        DesignConversionModelServerMeasurementGateThresholdConfiguration?
            measurementGateThresholdConfiguration = null)
        => CreateLocalLoopbackServiceCompositionWithLocalPngArtifactRoot(
            CreateLocalLoopbackBrowserArtifactRoot(),
            measurementGateThresholdConfiguration);

    internal static DesignConversionLocalServiceComposition CreateLocalLoopbackServiceCompositionWithLocalPngArtifactRoot(
            string localPngArtifactRoot,
            DesignConversionModelServerMeasurementGateThresholdConfiguration?
                measurementGateThresholdConfiguration = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(localPngArtifactRoot);
        var resolvedLocalPngArtifactRoot = ResolveLocalLoopbackPngArtifactRoot(
            localPngArtifactRoot);
        return DesignConversionLocalServiceComposition
            .CreateServerOwnedBrowserBackedMachineLocalWithDefaultJudge(
                CreateRequiredLocalLoopbackBrowserAdapter(resolvedLocalPngArtifactRoot),
                CreateLocalLoopbackMachine(measurementGateThresholdConfiguration),
                resolvedLocalPngArtifactRoot);
    }

    private static DesignConversionMachine CreateLocalLoopbackMachine(
        DesignConversionModelServerMeasurementGateThresholdConfiguration?
            measurementGateThresholdConfiguration)
        => measurementGateThresholdConfiguration is null
            ? new DesignConversionMachine()
            : new DesignConversionMachine(
                new DesignConversionRoutingFlow(),
                DesignConversionMachineEvaluationOptions.FromMeasurementGateThresholds(
                    measurementGateThresholdConfiguration.ToDomainThresholds()));

    public static string CreateLocalLoopbackArtifactUploadRoot(string? readyFilePath = null)
    {
        if (!string.IsNullOrWhiteSpace(readyFilePath))
        {
            var readyDirectory = Path.GetDirectoryName(Path.GetFullPath(readyFilePath));
            if (!string.IsNullOrWhiteSpace(readyDirectory))
                return Path.Combine(readyDirectory, ".artifact-upload");
        }

        return Path.Combine(
            Path.GetTempPath(),
            "xiigen-design-conversion-modelserver",
            "executable-artifact-upload",
            Environment.ProcessId.ToString());
    }

    public static string CreateLocalLoopbackArtifactDbRoot(string? readyFilePath = null)
    {
        if (!string.IsNullOrWhiteSpace(readyFilePath))
        {
            var readyDirectory = Path.GetDirectoryName(Path.GetFullPath(readyFilePath));
            if (!string.IsNullOrWhiteSpace(readyDirectory))
                return Path.Combine(readyDirectory, ".artifact-db");
        }

        return Path.Combine(
            Path.GetTempPath(),
            "xiigen-design-conversion-modelserver",
            "executable-artifact-db",
            Environment.ProcessId.ToString());
    }

    public static JsonlDesignConversionLocalArtifactDb CreateLocalLoopbackArtifactDb(
        string? readyFilePath = null,
        IDesignConversionDpoPolicyBaseProvider? dpoPolicyBaseProvider = null,
        DesignConversionTrainingIntakeDpoPairStoreOptions?
            trainingIntakeDpoPairStoreOptions = null)
        => new(
            CreateLocalLoopbackArtifactDbRoot(readyFilePath),
            dpoPolicyBaseProvider: dpoPolicyBaseProvider,
            trainingIntakeDpoPairStoreOptions:
                trainingIntakeDpoPairStoreOptions);

    internal static JsonlDesignConversionLocalArtifactDb CreateLocalLoopbackArtifactDb(
        string? readyFilePath,
        IDesignConversionDpoPolicyBaseProvider? dpoPolicyBaseProvider,
        ServerOwnedLegalTrainingTeacherAdmissionAuthority?
            legalTeacherAdmissionAuthority,
        DesignConversionTrainingIntakeDpoPairStoreOptions?
            trainingIntakeDpoPairStoreOptions = null)
        => new(
            CreateLocalLoopbackArtifactDbRoot(readyFilePath),
            legalTeacherAdmissionAuthority,
            dpoPolicyBaseProvider: dpoPolicyBaseProvider,
            trainingIntakeDpoPairStoreOptions:
                trainingIntakeDpoPairStoreOptions);

    public static DesignConversionModelServerArtifactUploadHttpHandler CreateLocalLoopbackArtifactUploadHandler(
        string? artifactUploadRoot = null)
        => new(
            new DesignConversionModelServerArtifactUploadStore(
                string.IsNullOrWhiteSpace(artifactUploadRoot)
                    ? CreateLocalLoopbackArtifactUploadRoot()
                    : artifactUploadRoot),
            DesignConversionModelServerRouteAdmission.Create(
                new DesignConversionModelServerRouteAdmissionOptions(
                    enabled: true,
                    currentToken: LocalLoopbackArtifactUploadAdmissionToken,
                    routeScope: LocalLoopbackArtifactUploadRouteScope,
                    tenantId: LocalLoopbackArtifactUploadTenantId)));

    public static string? ResolveReadyFilePath(string[]? args = null)
        => LocalLoopbackReadyFileOptions.Resolve(
            args ?? [],
            Environment.GetEnvironmentVariable)?.FilePath;

    public static Uri ResolveStartedLoopbackAddress(WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var addresses = app.Services
            .GetRequiredService<IServer>()
            .Features
            .Get<IServerAddressesFeature>()?
            .Addresses
            .ToArray() ?? [];
        if (addresses.Length != 1)
        {
            throw new InvalidOperationException(
                "DesignConversion ModelServer executable host expected exactly one started loopback address.");
        }

        var uri = new Uri(addresses[0]);
        if (!string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) ||
            !IPAddress.TryParse(uri.Host, out var ipAddress) ||
            !IPAddress.IsLoopback(ipAddress) ||
            uri.Port <= 0)
        {
            throw new InvalidOperationException(
                "DesignConversion ModelServer executable host must publish a concrete loopback-only HTTP base URL.");
        }

        return uri;
    }

    public static IReadOnlyDictionary<string, object?> CreateLocalLoopbackReadyFilePayload(
        Uri baseAddress,
        ContinuousLearningModelServerStartup continuousLearning,
        bool autonomousPromotionEvaluationConfigured = false)
    {
        ArgumentNullException.ThrowIfNull(baseAddress);
        ArgumentNullException.ThrowIfNull(continuousLearning);

        var composition = CreateLocalLoopbackServiceComposition();
        var compositionMetadata = composition.Descriptor.BoundaryMetadata;
        var routePaths = CreateLocalLoopbackReadyRoutePaths();
        var observedContinuousLearningReadiness =
            continuousLearning.ObserveReadiness();
        var readyFileContinuousLearningReadiness =
            observedContinuousLearningReadiness with
            {
                ConfigurationStatus =
                    "ready_file_snapshot_non_authoritative",
                LegalEvidenceVerified = false,
                CompositionMaterialized = false,
                PathIdentityVerified = false,
                RetainedObjectIdentityBound = false,
                StableDpoExecutorConfigured = false,
                ProductReady = false,
            };
        return new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["baseUrl"] = baseAddress.ToString().TrimEnd('/'),
            ["endpointPath"] = DesignConversionModelServerEndpointRegistry.EndpointPath,
            ["continuousLearningReadinessEndpointPath"] =
                ContinuousLearningReadinessEndpoint.EndpointPath,
            ["continuousLearningReadiness"] =
                readyFileContinuousLearningReadiness,
            ["continuousLearningReadyFileSnapshotAuthoritative"] = false,
            ["continuousLearningLiveReadinessEndpointAuthoritative"] = true,
            ["routePaths"] = routePaths,
            ["routes"] = CreateLocalLoopbackReadyRoutes(),
            ["loopbackOnly"] = true,
            ["localhostOnly"] = true,
            ["publicDeploymentVerified"] = false,
            ["publicHttpEndpointVerified"] = false,
            ["publicProductionDefaultReady"] = false,
            ["productReady"] = false,
            ["performsTraining"] = observedContinuousLearningReadiness.PerformsTraining,
            ["trainingRoutesMapped"] = true,
            ["dpoRoutesMapped"] = true,
            ["artifactUploadConfigured"] = true,
            ["artifactUploadRouteMapped"] = true,
            ["artifactUploadEndpointPath"] = DesignConversionModelServerArtifactUploadHttpHandler.EndpointPath,
            ["artifactUploadMaxBytes"] = DesignConversionModelServerArtifactUploadStore.DefaultMaxUploadBytes,
            ["artifactUploadFailClosedAdmissionRequired"] = true,
            ["artifactUploadStartupStatus"] = "mapped",
            ["artifactUploadNonReadyCode"] = "none",
            ["artifactUploadNonReadyReason"] = "none",
            ["autonomousPromotionEvaluationRouteMapped"] = true,
            ["autonomousPromotionEvaluationEndpointPath"] =
                AutonomousPromotionEvaluationEndpoint.EndpointPath,
            ["autonomousPromotionEvaluationConfigured"] =
                autonomousPromotionEvaluationConfigured,
            ["autonomousPromotionEvaluationOptInRequired"] = true,
            ["autonomousPromotionEvaluationPerformsTraining"] = false,
            ["autonomousPromotionEvaluationProductReady"] = false,
            ["containsRawPrivateData"] = false,
            ["compositionKind"] = composition.Descriptor.CompositionKind,
            ["browserBackedLocalDefaultComposition"] = string.Equals(
                composition.Descriptor.CompositionKind,
                DesignConversionLocalServiceComposition.BrowserBackedLocalDefaultCompositionKind,
                StringComparison.Ordinal),
            ["browserRendererDefaultWired"] = string.Equals(
                compositionMetadata["browser_renderer_default_wired"],
                bool.TrueString,
                StringComparison.Ordinal),
            ["browserRendererDefaultScope"] = compositionMetadata["browser_renderer_default_scope"],
            ["browserRendererDefaultProductionWired"] = string.Equals(
                compositionMetadata["browser_renderer_default_production_wired"],
                bool.TrueString,
                StringComparison.Ordinal),
            ["renderProviderKind"] = compositionMetadata["render_provider_kind"],
            ["comparisonProviderKind"] = ResolveLocalLoopbackComparisonProviderKind(composition.ComparisonProvider),
            ["serverOwnedLocalCompositionAuthority"] = string.Equals(
                compositionMetadata["server_owned_local_live_host_authority"],
                bool.TrueString,
                StringComparison.Ordinal),
            ["serverOwnedLocalComparisonProvider"] =
                composition.ComparisonProvider is ServerOwnedLocalPngComparisonProvider,
            ["externalJudgeCarrierPresent"] = false,
            ["visualJudgeBoundary"] = compositionMetadata["visual_judge_boundary"],
            ["deterministicLocalCompositionDefault"] = string.Equals(
                composition.Descriptor.CompositionKind,
                DesignConversionLocalServiceComposition.DeterministicLocalCompositionKind,
                StringComparison.Ordinal),
            ["deterministicRenderProviderDefault"] =
                composition.RenderProvider is DeterministicDesignConversionRenderProvider,
            ["deterministicComparisonProviderDefault"] =
                composition.ComparisonProvider is DeterministicDesignConversionComparisonProvider,
            ["deterministicBoundary"] = "none",
            ["browserRuntimeSetup"] = "server_owned_local_chromium_default",
            ["browserRuntimeExecutableResolved"] =
                LocalChromiumBrowserProcessAdapter.TryResolveServerOwnedDefaultExecutablePath() is not null,
            ["browserRuntimeUnavailableBehavior"] = "fail_closed_no_deterministic_fallback",
            ["externalVisualAiConfiguredByDefault"] = false,
            ["comparisonProviderUnavailableBehavior"] = "fail_closed_no_deterministic_fallback",
            ["boundary"] = LocalExecutableHostBoundary,
            ["readyFileContractBoundary"] = ReadyFileContractBoundary
        };
    }

    private static void ConfigureLocalLoopbackReadyFileLifecycle(
        WebApplication app,
        string readyFile,
        ContinuousLearningModelServerStartup continuousLearning,
        bool autonomousPromotionEvaluationConfigured)
    {
        ArgumentNullException.ThrowIfNull(continuousLearning);

        LocalLoopbackReadyFile.RemoveStale(readyFile);
        app.Lifetime.ApplicationStarted.Register(() =>
        {
            var baseAddress = ResolveStartedLoopbackAddress(app);
            LocalLoopbackReadyFile.WriteAtomic(
                readyFile,
                CreateLocalLoopbackReadyFilePayload(
                    baseAddress,
                    continuousLearning,
                    autonomousPromotionEvaluationConfigured));
        });
        app.Lifetime.ApplicationStopped.Register(() => LocalLoopbackReadyFile.RemoveSafe(readyFile));
    }

    private static string CreateLocalLoopbackBrowserArtifactRoot()
        => Path.Combine(
            Path.GetTempPath(),
            "llm_mvp_core",
            "design-conversion-browser-qa");

    private static string ResolveLocalLoopbackPngArtifactRoot(
        string? localPngArtifactRoot)
        => Path.GetFullPath(
            string.IsNullOrWhiteSpace(localPngArtifactRoot)
                ? CreateLocalLoopbackBrowserArtifactRoot()
                : localPngArtifactRoot);

    private static IDesignConversionBrowserProcessAdapter CreateRequiredLocalLoopbackBrowserAdapter(
        string browserArtifactRoot)
    {
        try
        {
            return LocalChromiumBrowserProcessAdapter.CreateServerOwnedDefault(
                artifactRoot: browserArtifactRoot);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                "DesignConversion ModelServer executable host could not create the required local browser adapter; deterministic fallback is forbidden.",
                ex);
        }
    }

    private static string ResolveLocalLoopbackComparisonProviderKind(IDesignConversionComparisonProvider comparisonProvider)
        => comparisonProvider switch
        {
            DeterministicDesignConversionComparisonProvider => DeterministicDesignConversionComparisonProvider.ProviderKind,
            ServerOwnedLocalPngComparisonProvider => ServerOwnedLocalPngComparisonProvider.ProviderKind,
            _ => comparisonProvider.GetType().FullName ?? comparisonProvider.GetType().Name
        };

    internal static class LocalLoopbackRepositoryOwnedTrainingConfiguration
    {
        public static DesignConversionModelServerRepositoryOwnedTrainingOptions
            Resolve(Func<string, string?> readEnvironment)
            => Resolve(
                readEnvironment,
                ResolveTrustedRepositoryRoot());

        internal static DesignConversionModelServerRepositoryOwnedTrainingOptions
            Resolve(
                Func<string, string?> readEnvironment,
                string trustedRepositoryRoot)
        {
            ArgumentNullException.ThrowIfNull(readEnvironment);
            var normalizedTrustedRepositoryRoot = NormalizeRepositoryRoot(
                trustedRepositoryRoot);
            if (normalizedTrustedRepositoryRoot is null)
                return DesignConversionModelServerRepositoryOwnedTrainingOptions
                    .InvalidServerConfiguration();

            var enabledValue = readEnvironment(
                RepositoryOwnedTrainingEnabledEnvironmentName);
            if (string.IsNullOrWhiteSpace(enabledValue))
                return DesignConversionModelServerRepositoryOwnedTrainingOptions
                    .MissingServerConfiguration();
            if (!TryParseBoolean(enabledValue, out var enabled))
                return DesignConversionModelServerRepositoryOwnedTrainingOptions
                    .InvalidServerConfiguration();
            if (!enabled)
                return DesignConversionModelServerRepositoryOwnedTrainingOptions
                    .DisabledIsNotBypass();

            var assertedRepositoryRoot = ReadRequired(
                readEnvironment,
                RepositoryOwnedTrainingRepositoryRootEnvironmentName);
            if (assertedRepositoryRoot is not null)
            {
                var normalizedAssertedRepositoryRoot = NormalizeRepositoryRoot(
                    assertedRepositoryRoot);
                if (normalizedAssertedRepositoryRoot is null ||
                    !SameRepositoryRoot(
                        normalizedAssertedRepositoryRoot,
                        normalizedTrustedRepositoryRoot))
                {
                    return DesignConversionModelServerRepositoryOwnedTrainingOptions
                        .InvalidServerConfiguration();
                }
            }

            var domainSlug = ReadRequired(
                readEnvironment,
                RepositoryOwnedTrainingDomainSlugEnvironmentName);
            var designPatternSlug = ReadRequired(
                readEnvironment,
                RepositoryOwnedTrainingDesignPatternSlugEnvironmentName);
            var datasetVersion = ReadRequired(
                readEnvironment,
                RepositoryOwnedTrainingDatasetVersionEnvironmentName);
            var modelIdSlug = ReadRequired(
                readEnvironment,
                RepositoryOwnedTrainingModelIdSlugEnvironmentName);
            var modelVersion = ReadRequired(
                readEnvironment,
                RepositoryOwnedTrainingModelVersionEnvironmentName);
            var configSha256 = ReadRequired(
                readEnvironment,
                RepositoryOwnedTrainingRuntimeConfigSha256EnvironmentName);
            var topologySha256 = ReadRequired(
                readEnvironment,
                RepositoryOwnedTrainingRuntimeTopologySha256EnvironmentName);
            var tokenizerSha256 = ReadRequired(
                readEnvironment,
                RepositoryOwnedTrainingRuntimeTokenizerSha256EnvironmentName);
            var trainerSha256 = ReadRequired(
                readEnvironment,
                RepositoryOwnedTrainingRuntimeTrainerSha256EnvironmentName);
            var schemaSha256 = ReadRequired(
                readEnvironment,
                RepositoryOwnedTrainingRuntimeSchemaSha256EnvironmentName);
            var compatibilityKey = ReadRequired(
                readEnvironment,
                RepositoryOwnedTrainingRuntimeCompatibilityKeyEnvironmentName);
            var provenanceRefs = ReadRequiredList(
                readEnvironment,
                RepositoryOwnedTrainingProvenanceRefsEnvironmentName);
            if (domainSlug is null ||
                designPatternSlug is null ||
                datasetVersion is null ||
                modelIdSlug is null ||
                modelVersion is null ||
                configSha256 is null ||
                topologySha256 is null ||
                tokenizerSha256 is null ||
                trainerSha256 is null ||
                schemaSha256 is null ||
                compatibilityKey is null ||
                provenanceRefs is null)
            {
                return DesignConversionModelServerRepositoryOwnedTrainingOptions
                    .MissingServerConfiguration();
            }

            var allowColdStart = false;
            var allowColdStartValue = readEnvironment(
                RepositoryOwnedTrainingAllowColdStartEnvironmentName);
            if (!string.IsNullOrWhiteSpace(allowColdStartValue) &&
                !TryParseBoolean(allowColdStartValue, out allowColdStart))
            {
                return DesignConversionModelServerRepositoryOwnedTrainingOptions
                    .InvalidServerConfiguration();
            }

            var coldStartReason = NormalizeOptional(
                readEnvironment(RepositoryOwnedTrainingColdStartReasonEnvironmentName));
            var spec = new DesignConversionRepositoryOwnedTrainingRunContextSpec(
                normalizedTrustedRepositoryRoot,
                domainSlug,
                designPatternSlug,
                datasetVersion,
                modelIdSlug,
                modelVersion,
                new RepositoryOwnedModelRuntimeCompatibility(
                    configSha256,
                    topologySha256,
                    tokenizerSha256,
                    trainerSha256,
                    schemaSha256,
                    compatibilityKey),
                allowColdStart,
                coldStartReason,
                provenanceRefs);

            return ValidateAddressShapeAndModelPolicy(spec)
                ? new DesignConversionModelServerRepositoryOwnedTrainingOptions(
                    spec)
                : DesignConversionModelServerRepositoryOwnedTrainingOptions
                    .InvalidServerConfiguration();
        }

        internal static DesignConversionModelServerRepositoryOwnedTrainingOptions
            ValidateExplicitOptionsOrMissing(
                DesignConversionModelServerRepositoryOwnedTrainingOptions?
                    repositoryOwnedTrainingOptions,
                string? trustedRepositoryRoot)
            => repositoryOwnedTrainingOptions is null
                ? DesignConversionModelServerRepositoryOwnedTrainingOptions
                    .MissingServerConfiguration()
                : ValidateExplicitOptions(
                    repositoryOwnedTrainingOptions,
                    trustedRepositoryRoot);

        internal static DesignConversionModelServerRepositoryOwnedTrainingOptions
            ValidateExplicitOptions(
                DesignConversionModelServerRepositoryOwnedTrainingOptions
                    repositoryOwnedTrainingOptions,
                string? trustedRepositoryRoot)
        {
            ArgumentNullException.ThrowIfNull(repositoryOwnedTrainingOptions);
            var normalizedTrustedRepositoryRoot = NormalizeRepositoryRoot(
                trustedRepositoryRoot);
            if (normalizedTrustedRepositoryRoot is null)
                return DesignConversionModelServerRepositoryOwnedTrainingOptions
                    .InvalidServerConfiguration();

            if (repositoryOwnedTrainingOptions.RunContextSpec is null)
                return new DesignConversionModelServerRepositoryOwnedTrainingOptions(
                    RunContextSpec: null,
                    ConfigurationErrorCode:
                        repositoryOwnedTrainingOptions
                            .ConfigurationErrorCode,
                    ConfigurationErrorMessage:
                        repositoryOwnedTrainingOptions
                            .ConfigurationErrorMessage);

            var explicitRepositoryRoot = NormalizeRepositoryRoot(
                repositoryOwnedTrainingOptions.RunContextSpec.RepositoryRoot);
            if (explicitRepositoryRoot is null ||
                !SameRepositoryRoot(
                    explicitRepositoryRoot,
                    normalizedTrustedRepositoryRoot))
            {
                return DesignConversionModelServerRepositoryOwnedTrainingOptions
                    .InvalidServerConfiguration();
            }

            var validatedSpec = repositoryOwnedTrainingOptions.RunContextSpec with
            {
                RepositoryRoot = normalizedTrustedRepositoryRoot,
                ProvenanceRefs =
                    repositoryOwnedTrainingOptions
                        .RunContextSpec
                        .ProvenanceRefs
                        ?.ToArray() ?? null!
            };
            return ValidateAddressShapeAndModelPolicy(validatedSpec)
                ? new DesignConversionModelServerRepositoryOwnedTrainingOptions(
                    validatedSpec,
                    repositoryOwnedTrainingOptions.ConfigurationErrorCode,
                    repositoryOwnedTrainingOptions.ConfigurationErrorMessage)
                : DesignConversionModelServerRepositoryOwnedTrainingOptions
                    .InvalidServerConfiguration();
        }

        private static string ResolveTrustedRepositoryRoot()
            => NormalizeRepositoryRoot(
                RepositoryOwnedTrainingTrustedRepositoryRoot) ??
               RepositoryOwnedTrainingTrustedRepositoryRoot;

        private static string? NormalizeRepositoryRoot(string? repositoryRoot)
        {
            var normalized = NormalizeOptional(repositoryRoot);
            if (normalized is null)
                return null;

            normalized = NormalizeWslCDrivePath(normalized);
            try
            {
                return TrimTrailingDirectorySeparators(
                    Path.GetFullPath(normalized));
            }
            catch (Exception exception)
                when (exception is ArgumentException
                      or NotSupportedException
                      or PathTooLongException)
            {
                return null;
            }
        }

        private static string NormalizeWslCDrivePath(string repositoryRoot)
        {
            if (repositoryRoot.Length >= 7 &&
                repositoryRoot.StartsWith("/mnt/", StringComparison.OrdinalIgnoreCase) &&
                char.IsLetter(repositoryRoot[5]) &&
                repositoryRoot[6] == '/')
            {
                var drive = char.ToUpperInvariant(repositoryRoot[5]);
                var rest = repositoryRoot[7..]
                    .Replace('/', Path.DirectorySeparatorChar);
                return drive + @":\" + rest;
            }

            return repositoryRoot;
        }

        private static bool SameRepositoryRoot(
            string left,
            string right)
            => string.Equals(
                TrimTrailingDirectorySeparators(left),
                TrimTrailingDirectorySeparators(right),
                OperatingSystem.IsWindows()
                    ? StringComparison.OrdinalIgnoreCase
                    : StringComparison.Ordinal);

        private static string TrimTrailingDirectorySeparators(string path)
        {
            var root = Path.GetPathRoot(path) ?? string.Empty;
            while (path.Length > root.Length &&
                   (path[^1] == Path.DirectorySeparatorChar ||
                    path[^1] == Path.AltDirectorySeparatorChar))
            {
                path = path[..^1];
            }

            return path;
        }

        private static bool ValidateAddressShapeAndModelPolicy(
            DesignConversionRepositoryOwnedTrainingRunContextSpec spec)
        {
            var dpoAddress = RepositoryOwnedTrainingArtifactLayout
                .CreateDpoDatasetAddress(
                    spec.RepositoryRoot,
                    spec.DomainSlug,
                    spec.DesignPatternSlug,
                    spec.DatasetVersion);
            if (!dpoAddress.IsSuccess || dpoAddress.Value is null)
                return false;

            var modelAddress = RepositoryOwnedTrainingArtifactLayout
                .CreateModelCheckpointAddress(
                    spec.RepositoryRoot,
                    spec.DomainSlug,
                    spec.DesignPatternSlug,
                    spec.ModelIdSlug,
                    spec.ModelVersion,
                    step: 1);
            if (!modelAddress.IsSuccess || modelAddress.Value is null)
                return false;

            return new DesignConversionRepositoryOwnedModelCheckpointContext(
                Enabled: true,
                Request: new DesignConversionRepositoryOwnedModelCheckpointRequest(
                    modelAddress.Value.RepositoryRoot,
                    modelAddress.Value.DomainSlug,
                    modelAddress.Value.DesignPatternSlug,
                    modelAddress.Value.ModelIdSlug,
                    modelAddress.Value.ModelVersion,
                    spec.RuntimeCompatibility,
                    spec.AllowColdStart,
                    spec.ColdStartReason,
                    spec.ProvenanceRefs))
                .Validate()
                .IsSuccess;
        }

        private static string? ReadRequired(
            Func<string, string?> readEnvironment,
            string name)
            => NormalizeOptional(readEnvironment(name));

        private static IReadOnlyList<string>? ReadRequiredList(
            Func<string, string?> readEnvironment,
            string name)
        {
            var raw = readEnvironment(name);
            if (string.IsNullOrWhiteSpace(raw))
                return null;

            var values = raw
                .Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Select(value => value.Trim())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .ToArray();
            return values.Length == 0 ? null : Array.AsReadOnly(values);
        }

        private static string? NormalizeOptional(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static bool TryParseBoolean(string value, out bool result)
        {
            switch (value.Trim().ToLowerInvariant())
            {
                case "1":
                case "true":
                case "yes":
                case "on":
                    result = true;
                    return true;
                case "0":
                case "false":
                case "no":
                case "off":
                    result = false;
                    return true;
                default:
                    result = false;
                    return false;
            }
        }
    }

    private static IReadOnlyList<string> CreateLocalLoopbackReadyRoutePaths()
        =>
        [
            ..DesignConversionModelServerAspNetHostStartup.FullLocalRouteSet(),
            DesignConversionModelServerArtifactUploadHttpHandler.EndpointPath,
            AutonomousPromotionEvaluationEndpoint.EndpointPath,
            CodeDocumentationRepositoryOwnedDpoTrainingEndpoint.EndpointPath,
            CodeDocumentationSavedModelAssessmentEndpoint.EndpointPath,
            CodeDocumentationSavedModelAssessmentEndpoint.CalibrationEndpointPath,
            CodeDocumentationSavedModelAssessmentEndpoint.CertificationEndpointPath,
            ContinuousLearningReadinessEndpoint.EndpointPath
        ];

    private static IReadOnlyList<string> CreateLocalLoopbackReadyRoutes()
        =>
        [
            DesignConversionModelServerEndpointRegistry.EndpointRoute,
            DesignConversionModelServerEndpointRegistry.InternalEndpointRoute,
            DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointRoute,
            DesignConversionModelServerEndpointRegistry.VisualCostReadbackEndpointRoute,
            DesignConversionModelServerEndpointRegistry.StatusReadbackEndpointRoute,
            DesignConversionModelServerEndpointRegistry.DpoUsefulnessEndpointRoute,
            DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointRoute,
            DesignConversionModelServerEndpointRegistry.Flow38QuarantineEndpointRoute,
            DesignConversionModelServerEndpointRegistry.ModelActivationEndpointRoute,
            DesignConversionModelServerEndpointRegistry.HumanApprovalChallengeEndpointRoute,
            DesignConversionModelServerEndpointRegistry.HumanApprovalApproveEndpointRoute,
            "POST " + DesignConversionModelServerArtifactUploadHttpHandler.EndpointPath,
            "POST " + AutonomousPromotionEvaluationEndpoint.EndpointPath,
            "POST " + CodeDocumentationRepositoryOwnedDpoTrainingEndpoint.EndpointPath,
            "POST " + CodeDocumentationSavedModelAssessmentEndpoint.EndpointPath,
            "POST " + CodeDocumentationSavedModelAssessmentEndpoint.CalibrationEndpointPath,
            "POST " + CodeDocumentationSavedModelAssessmentEndpoint.CertificationEndpointPath,
            "GET " + ContinuousLearningReadinessEndpoint.EndpointPath
        ];

    private static void ApplyLocalExecutableBoundaryHeaders(HttpResponse response)
    {
        response.Headers["x-design-conversion-modelserver-executable-host-boundary"] =
            LocalExecutableHostBoundary;
        response.Headers["x-design-conversion-modelserver-localhost-loopback-only"] =
            bool.TrueString;
        response.Headers["x-design-conversion-public-deployment-verified"] =
            bool.FalseString;
        response.Headers["x-design-conversion-public-production-default-ready"] =
            bool.FalseString;
        response.Headers["x-design-conversion-product-ready"] =
            bool.FalseString;
    }

    private sealed class MappedDesignConversionRouteCapability
        : ContinuousLearningDesignConversionRouteMappingCapability
    {
        internal MappedDesignConversionRouteCapability(
            ContinuousLearningDesignConversionRouteMappingCapability.Issuer
                issuer,
            DesignConversionModelServerEndpointRegistry registry,
            JsonlDesignConversionLocalArtifactDb artifactDb,
            DesignConversionRetainedDpoPolicyBaseProviderGateway gateway,
            ServerOwnedLegalTrainingTeacherAdmissionAuthority legalAuthority)
            : base(
                issuer,
                registry,
                artifactDb,
                gateway,
                legalAuthority)
        {
        }
    }

    private sealed record LocalLoopbackReadyFileOptions(string FilePath)
    {
        public static LocalLoopbackReadyFileOptions? Resolve(
            string[] args,
            Func<string, string?> readEnvironment)
        {
            ArgumentNullException.ThrowIfNull(args);
            ArgumentNullException.ThrowIfNull(readEnvironment);

            string? cliReadyFile = null;
            var cliReadyFileConfigured = false;
            for (var index = 0; index < args.Length; index++)
            {
                var argument = args[index];
                if (string.Equals(argument, ReadyFileArgumentName, StringComparison.Ordinal))
                {
                    if (cliReadyFileConfigured)
                        throw new ArgumentException("DesignConversion ModelServer ready-file was specified more than once.");
                    if (index + 1 >= args.Length)
                        throw new ArgumentException("DesignConversion ModelServer ready-file path is required.");

                    cliReadyFile = args[++index];
                    cliReadyFileConfigured = true;
                    continue;
                }

                var assignmentPrefix = ReadyFileArgumentName + "=";
                if (argument.StartsWith(assignmentPrefix, StringComparison.Ordinal))
                {
                    if (cliReadyFileConfigured)
                        throw new ArgumentException("DesignConversion ModelServer ready-file was specified more than once.");

                    cliReadyFile = argument[assignmentPrefix.Length..];
                    cliReadyFileConfigured = true;
                }
            }

            var configuredReadyFile = cliReadyFileConfigured
                ? cliReadyFile
                : readEnvironment(ReadyFileEnvironmentName);
            if (string.IsNullOrWhiteSpace(configuredReadyFile))
            {
                if (cliReadyFileConfigured)
                    throw new ArgumentException("DesignConversion ModelServer ready-file path is required.");
                return null;
            }

            return new LocalLoopbackReadyFileOptions(
                LocalLoopbackReadyFile.NormalizeReadyFile(configuredReadyFile));
        }
    }

    private static class LocalLoopbackReadyFile
    {
        internal static string NormalizeReadyFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path) ||
                path.IndexOf('\0') >= 0 ||
                path.Any(char.IsControl))
            {
                throw new ArgumentException("DesignConversion ModelServer ready-file path is required.");
            }

            var trimmed = path.Trim();
            if (EndsWithDirectorySeparator(trimmed))
            {
                throw new ArgumentException("DesignConversion ModelServer ready-file path must name a file, not a directory.");
            }

            string fullPath;
            try
            {
                fullPath = Path.GetFullPath(trimmed);
            }
            catch (Exception ex) when (
                ex is ArgumentException or IOException or NotSupportedException or PathTooLongException)
            {
                throw new ArgumentException("DesignConversion ModelServer ready-file path is invalid.", ex);
            }

            var fileName = Path.GetFileName(fullPath);
            if (string.IsNullOrWhiteSpace(fileName) || EndsWithDirectorySeparator(fullPath))
            {
                throw new ArgumentException("DesignConversion ModelServer ready-file path must name a file, not a directory.");
            }

            RejectExistingDirectoryOrReparsePoint(fullPath);
            return fullPath;
        }

        internal static void RemoveStale(string path)
        {
            path = NormalizeReadyFile(path);
            if (!File.Exists(path))
                return;

            EnsureRegularFile(path);
            File.Delete(path);
        }

        internal static void RemoveSafe(string path)
        {
            try
            {
                RemoveStale(path);
            }
            catch (Exception ex) when (
                ex is ArgumentException or IOException or UnauthorizedAccessException or NotSupportedException)
            {
                // Shutdown cleanup must not convert an already-stopping local host into a product-ready claim.
            }
        }

        internal static void WriteAtomic(
            string path,
            IReadOnlyDictionary<string, object?> payload)
        {
            ArgumentNullException.ThrowIfNull(payload);

            path = NormalizeReadyFile(path);
            var directory = Path.GetDirectoryName(path);
            if (string.IsNullOrWhiteSpace(directory))
            {
                throw new ArgumentException("DesignConversion ModelServer ready-file parent directory is required.");
            }

            EnsureSafeParentDirectory(directory);
            var fileName = Path.GetFileName(path);
            var temporaryPath = Path.Combine(
                directory,
                $".{fileName}.{Guid.NewGuid():N}.tmp");

            try
            {
                EnsureRegularFileOrMissing(path);
                EnsureRegularFileOrMissing(temporaryPath);
                var json = JsonSerializer.Serialize(
                    payload,
                    DesignConversionModelServerHttpHandler.JsonOptions);
                using (var stream = new FileStream(
                           temporaryPath,
                           FileMode.CreateNew,
                           FileAccess.Write,
                           FileShare.None,
                           bufferSize: 4096,
                           FileOptions.WriteThrough))
                using (var writer = new StreamWriter(
                           stream,
                           new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)))
                {
                    writer.Write(json);
                    writer.Flush();
                    stream.Flush(flushToDisk: true);
                }

                EnsureRegularFile(temporaryPath);
                EnsureRegularFileOrMissing(path);
                File.Move(temporaryPath, path, overwrite: true);
                EnsureRegularFile(path);
            }
            finally
            {
                if (File.Exists(temporaryPath))
                    File.Delete(temporaryPath);
            }
        }

        private static void EnsureSafeParentDirectory(string directory)
        {
            if (File.Exists(directory))
            {
                throw new ArgumentException("DesignConversion ModelServer ready-file parent path is a file.");
            }

            if (Directory.Exists(directory))
            {
                var attributes = File.GetAttributes(directory);
                if ((attributes & FileAttributes.ReparsePoint) != 0)
                {
                    throw new ArgumentException(
                        "DesignConversion ModelServer ready-file parent directory must not be a reparse point.");
                }
            }

            Directory.CreateDirectory(directory);
        }

        private static void EnsureRegularFileOrMissing(string path)
        {
            if (!File.Exists(path) && !Directory.Exists(path))
                return;

            EnsureRegularFile(path);
        }

        private static void EnsureRegularFile(string path)
        {
            var attributes = File.GetAttributes(path);
            if ((attributes & FileAttributes.Directory) != 0 ||
                (attributes & FileAttributes.ReparsePoint) != 0)
            {
                throw new ArgumentException(
                    "DesignConversion ModelServer ready-file path must be a regular file.");
            }
        }

        private static void RejectExistingDirectoryOrReparsePoint(string path)
        {
            if (!File.Exists(path) && !Directory.Exists(path))
                return;

            var attributes = File.GetAttributes(path);
            if ((attributes & FileAttributes.Directory) != 0)
            {
                throw new ArgumentException("DesignConversion ModelServer ready-file path must name a file, not a directory.");
            }

            if ((attributes & FileAttributes.ReparsePoint) != 0)
            {
                throw new ArgumentException(
                    "DesignConversion ModelServer ready-file path must not be a reparse point.");
            }
        }

        private static bool EndsWithDirectorySeparator(string path)
            => path.Length > 0 &&
               (path[^1] == Path.DirectorySeparatorChar ||
                path[^1] == Path.AltDirectorySeparatorChar);
    }
}

internal sealed class ContinuousLearningRetainedObjectHostedActivator : IHostedService
{
    private readonly ServerOwnedContinuousLearningFileAccess _fileAccess;
    private readonly DesignConversionRetainedDpoPolicyBaseProviderGateway
        _policyBaseProviderGateway;
    private readonly ContinuousLearningModelServerStartup _startup;
    private readonly CodeDocumentationInitialTrainingConfiguration? _initialTraining; // Use the captured server-owned initial selector rather than client wire fields.

    public ContinuousLearningRetainedObjectHostedActivator(
        ServerOwnedContinuousLearningFileAccess fileAccess,
        DesignConversionRetainedDpoPolicyBaseProviderGateway
            policyBaseProviderGateway,
        ContinuousLearningModelServerStartup startup, // Receive the existing startup readiness and materialization lifecycle owner used by this hosted activator.
        CodeDocumentationInitialTrainingConfiguration? initialTraining = null) // Use the captured server-owned initial selector rather than client wire fields.
    {
        _fileAccess = fileAccess;
        _policyBaseProviderGateway = policyBaseProviderGateway;
        _startup = startup;
        _initialTraining = initialTraining; // Use the captured server-owned initial selector rather than client wire fields.
    }

    public Task StartAsync(CancellationToken cancellationToken)
        => StartAsync(
            cancellationToken,
            afterGatewayBindingBeforeReadinessPromotionForTesting: null);

    internal async Task StartAsync(
        CancellationToken cancellationToken,
        Action? afterGatewayBindingBeforeReadinessPromotionForTesting)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _ = _fileAccess;
        _startup.BindRetainedFileAccess(_fileAccess);
        var initialCheckpoint = _initialTraining?.BindCheckpointForStartup(); // Use the captured server-owned initial selector rather than client wire fields.
        if (initialCheckpoint is { IsSuccess: false }) // Detect a refused initial-checkpoint binding before inactive composition can begin.
        { // Hosted startup cannot promote readiness after a declaration refusal.
            _startup.RejectInactiveTp1Materialization(); // Mark startup materialization and readiness failed through their existing lifecycle owner.
            _policyBaseProviderGateway.Close(); // Withdraw the gateway before any failed startup could publish readiness.
            return; // Stop hosted startup before inactive composition and policy-provider activation after the refusal.
        } // Hosted startup cannot promote readiness after a declaration refusal.
        _startup.MaterializeInactiveTp1Composition(_fileAccess, cancellationToken, // Preserve caller cancellation at the retained-read and import boundary.
            initialFullCheckpoint: initialCheckpoint?.Value); // Carry the same immutable full-LoRA declaration through startup and later policy loading.
        if (!_startup.Readiness.CompositionMaterialized)
        {
            _policyBaseProviderGateway.Close();
            return;
        }

        await ActivatePolicyBaseProviderAsync(
            cancellationToken,
            afterProviderCreationBeforeBindingForTesting: null,
            afterGatewayBindingBeforeReadinessPromotionForTesting);
    }

    internal async Task ActivatePolicyBaseProviderAsync(
        CancellationToken cancellationToken,
        Action? afterProviderCreationBeforeBindingForTesting)
        => await ActivatePolicyBaseProviderAsync(
            cancellationToken,
            afterProviderCreationBeforeBindingForTesting is null
                ? null
                : _ => afterProviderCreationBeforeBindingForTesting(),
            afterGatewayBindingBeforeReadinessPromotionForTesting: null);

    internal async Task ActivatePolicyBaseProviderAsync(
        CancellationToken cancellationToken,
        Action<DesignConversionCheckpointBackedDpoPolicyBaseProvider>?
            afterProviderCreationBeforeBindingForTesting,
        Action? afterGatewayBindingBeforeReadinessPromotionForTesting = null)
    {
        ContinuousLearningDpoPolicyBaseBindingToken? bindingToken = null;
        try
        {
            var bindingConfiguration =
                _startup.CaptureDpoPolicyBaseBindingConfiguration(_fileAccess);
            var provider = await DesignConversionRetainedDpoPolicyBaseComposition
                .CreateInactiveProviderAsync(
                    _fileAccess,
                    bindingConfiguration.ModelConfig,
                    bindingConfiguration.ModelId,
                    bindingConfiguration.ModelVersion,
                    bindingConfiguration.RootInitializationSeed,
                    bindingConfiguration.TrainingConfigurationFingerprint,
                    cancellationToken, // Preserve caller cancellation at the retained-read and import boundary.
                    bindingConfiguration.InitialFullCheckpoint); // Carry the same immutable full-LoRA declaration through startup and later policy loading.
            if (cancellationToken.IsCancellationRequested)
                cancellationToken.ThrowIfCancellationRequested();
            if (!provider.IsSuccess || provider.Value is null)
            {
                MarkPolicyBaseBindingFailed();
                return;
            }

            afterProviderCreationBeforeBindingForTesting?.Invoke(provider.Value);
            _startup.EnsureDpoPolicyBaseBindingRetained(_fileAccess);
            var bound = _policyBaseProviderGateway.BindExactProvider(
                provider.Value,
                _startup,
                _fileAccess,
                cancellationToken);
            if (!bound.IsSuccess && cancellationToken.IsCancellationRequested)
                cancellationToken.ThrowIfCancellationRequested();
            if (!bound.IsSuccess || bound.Value is null)
            {
                MarkPolicyBaseBindingFailed();
                return;
            }

            bindingToken = bound.Value;
            afterGatewayBindingBeforeReadinessPromotionForTesting?.Invoke();
            cancellationToken.ThrowIfCancellationRequested();
            if (!_startup.TryMarkDpoPolicyBaseProviderBound(
                    _fileAccess,
                    bindingToken))
            {
                _policyBaseProviderGateway.CloseExactBinding(bindingToken);
            }
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            MarkPolicyBaseBindingFailed(bindingToken);
            throw;
        }
        catch (ContinuousLearningPathIdentityException)
        {
            MarkPolicyBaseBindingFailed(bindingToken);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            MarkPolicyBaseBindingFailed(bindingToken);
        }
    }

    private void MarkPolicyBaseBindingFailed(
        ContinuousLearningDpoPolicyBaseBindingToken? bindingToken = null)
    {
        if (bindingToken is null)
        {
            _startup.MarkDpoPolicyBaseBindingFailed();
            _policyBaseProviderGateway.Close();
        }
        else
        {
            _policyBaseProviderGateway.CloseExactBinding(bindingToken);
            _startup.MarkDpoPolicyBaseBindingFailed(bindingToken);
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _ = cancellationToken;
        var failures = new LifecycleCleanupArbiter();
        var gatewayRetirementInitiated = false;
        var startupWithdrawalBegun = await TryCloseGatewayUntilStartupWithdrawalBeginsAsync(
                failures)
            .ConfigureAwait(false);
        gatewayRetirementInitiated = startupWithdrawalBegun;
        var startupRetirementInitiated = false;
        if (startupWithdrawalBegun)
        {
            try
            {
                _startup.MarkDpoPolicyBaseStopped();
                startupRetirementInitiated = true;
            }
            catch (Exception exception)
            {
                failures.CaptureCleanup(exception);
            }
        }
        if (startupRetirementInitiated)
        {
            try
            {
                await _startup.AwaitRetirementAsync().ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                failures.CaptureCleanup(exception);
            }
        }
        if (gatewayRetirementInitiated)
        {
            try
            {
                await _policyBaseProviderGateway.AwaitRetirementAsync()
                    .ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                failures.CaptureCleanup(exception);
            }
        }
        try
        {
            _fileAccess.Dispose();
        }
        catch (Exception exception)
        {
            failures.CaptureCleanup(exception);
        }
        failures.ThrowIfAny();
    }

    private async Task<bool> TryCloseGatewayUntilStartupWithdrawalBeginsAsync(
        LifecycleCleanupArbiter failures)
    {
        var first = await TryCloseGatewayOnceAsync()
            .ConfigureAwait(false);
        if (first.StartupWithdrawalBegun)
            return true;

        var second = await TryCloseGatewayOnceAsync()
            .ConfigureAwait(false);
        if (second.StartupWithdrawalBegun)
            return true;

        if (first.Failure is not null)
            failures.CaptureCleanup(first.Failure);
        if (second.Failure is not null)
            failures.CaptureCleanup(second.Failure);
        if (!failures.HasCleanupFailure)
        {
            failures.CaptureCleanup(new InvalidOperationException(
                "The retained DPO policy base gateway did not begin startup withdrawal."));
        }
        return false;
    }

    private async Task<GatewayStartupWithdrawalAttempt>
        TryCloseGatewayOnceAsync()
    {
        try
        {
            _policyBaseProviderGateway.Close();
            var startupWithdrawalBegun = await _policyBaseProviderGateway
                .AwaitStartupWithdrawalBegunAsync()
                .ConfigureAwait(false);
            return new GatewayStartupWithdrawalAttempt(
                startupWithdrawalBegun,
                Failure: null);
        }
        catch (Exception exception)
        {
            return new GatewayStartupWithdrawalAttempt(
                StartupWithdrawalBegun: false,
                exception);
        }
    }

    private readonly record struct GatewayStartupWithdrawalAttempt(
        bool StartupWithdrawalBegun,
        Exception? Failure);
}
