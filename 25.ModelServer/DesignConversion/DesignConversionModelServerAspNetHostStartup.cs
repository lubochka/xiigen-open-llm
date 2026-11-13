// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ModelServer.DesignConversion;

using AgentInfrastructure;
using ContinuousLearning;
using DesignConversionDomain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using ModelServer.Contracts;

public sealed record DesignConversionModelServerAspNetHostStartupMetadata(
    string StartupSurface,
    string Boundary,
    string MappedRoute,
    IReadOnlyList<string> MappedRoutes,
    bool LocalOnly,
    bool RefsOnly,
    bool PublicDeploymentVerified,
    bool CrossRepoCallerVerified,
    bool ExternalProviderConfigured,
    bool ProductReady);

public sealed record DesignConversionFlow38PublicBoundarySnapshot(
    bool RouteMapped,
    string? EndpointPath,
    bool MetadataValid,
    string MetadataStatus,
    IReadOnlyList<string> MetadataIssues,
    string ContractSchemaVersion,
    bool QuarantineOnly,
    bool StoreConfigured,
    bool Persisted,
    string LegalVerdict,
    string AllowedUse,
    string Route,
    bool SemanticQualityClaimed,
    bool DpoTrainReady,
    bool DpoPairCreated,
    bool TrainingTriggered,
    bool CommonTrainingAllowed,
    bool RawOutputAccepted,
    bool RefsOnlyPublicResponse,
    bool ProductReady)
{
    public const string ValidStatus = "valid";
    public const string RouteNotRegisteredStatus = "route_not_registered";
    public const string MissingValue = "metadata_missing";
    public const string MalformedValue = "metadata_malformed";

    public bool Configured => RouteMapped && MetadataValid && StoreConfigured;

    public static DesignConversionFlow38PublicBoundarySnapshot FromRegistry(
        DesignConversionModelServerEndpointRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);

        var resolved = registry.Resolve(
            DesignConversionModelServerEndpointRegistry.Flow38QuarantineEndpointMethod,
            DesignConversionModelServerEndpointRegistry.Flow38QuarantineEndpointPath);
        return resolved.IsSuccess
            ? FromRegistration(resolved.Value)
            : MissingRoute();
    }

    public static DesignConversionFlow38PublicBoundarySnapshot FromRegistration(
        DesignConversionModelServerEndpointRegistration? registration)
    {
        if (registration is null)
            return MissingRoute();

        var issues = new List<string>();
        if (!string.Equals(
                registration.Method,
                DesignConversionModelServerEndpointRegistry.Flow38QuarantineEndpointMethod,
                StringComparison.Ordinal))
        {
            issues.Add("unexpected:endpoint_method");
        }

        if (!string.Equals(
                registration.Path,
                DesignConversionModelServerEndpointRegistry.Flow38QuarantineEndpointPath,
                StringComparison.Ordinal))
        {
            issues.Add("unexpected:endpoint_path");
        }

        var metadata = registration.BoundaryMetadata;
        var contractSchemaVersion = ReadExpectedString(
            metadata,
            "flow38_quarantine_contract_schema_version",
            DesignConversionModelServerFlow38QuarantineHandler.SchemaVersion,
            issues);
        var quarantineOnly = ReadBoolean(metadata, "flow38_quarantine_only", expected: true, issues);
        var storeConfigured = ReadBoolean(metadata, "flow38_quarantine_store_configured", expected: null, issues);
        var persisted = ReadBoolean(metadata, "flow38_quarantine_persisted", expected: false, issues);
        var legalVerdict = ReadExpectedString(
            metadata,
            "legal_verdict",
            DesignConversionModelServerFlow38QuarantineHandler.QuarantineVerdict,
            issues);
        var allowedUse = ReadExpectedString(
            metadata,
            "allowed_use",
            DesignConversionModelServerFlow38QuarantineHandler.PersonalAdaptiveUse,
            issues);
        var route = ReadExpectedString(
            metadata,
            "route",
            DesignConversionModelServerFlow38QuarantineHandler.AdaptiveOrQuarantineRoute,
            issues);
        var semanticQualityClaimed = ReadBoolean(metadata, "semantic_quality_claimed", expected: false, issues);
        var dpoTrainReady = ReadBoolean(metadata, "dpo_train_ready", expected: false, issues);
        var dpoPairCreated = ReadBoolean(metadata, "dpo_pair_created", expected: false, issues);
        var trainingTriggered = ReadBoolean(metadata, "training_triggered", expected: false, issues);
        var commonTrainingAllowed = ReadBoolean(metadata, "common_training_allowed", expected: false, issues);
        var rawOutputAccepted = ReadBoolean(metadata, "raw_output_accepted", expected: false, issues);
        var refsOnlyPublicResponse = ReadBoolean(metadata, "refs_only_public_response", expected: true, issues);
        var productReady = ReadBoolean(metadata, "product_ready", expected: false, issues);
        var frozenIssues = issues.ToArray();

        return new DesignConversionFlow38PublicBoundarySnapshot(
            RouteMapped: true,
            EndpointPath: registration.Path,
            MetadataValid: frozenIssues.Length == 0,
            MetadataStatus: frozenIssues.Length == 0 ? ValidStatus : frozenIssues[0],
            MetadataIssues: frozenIssues,
            ContractSchemaVersion: contractSchemaVersion,
            QuarantineOnly: quarantineOnly,
            StoreConfigured: storeConfigured,
            Persisted: persisted,
            LegalVerdict: legalVerdict,
            AllowedUse: allowedUse,
            Route: route,
            SemanticQualityClaimed: semanticQualityClaimed,
            DpoTrainReady: dpoTrainReady,
            DpoPairCreated: dpoPairCreated,
            TrainingTriggered: trainingTriggered,
            CommonTrainingAllowed: commonTrainingAllowed,
            RawOutputAccepted: rawOutputAccepted,
            RefsOnlyPublicResponse: refsOnlyPublicResponse,
            ProductReady: productReady);
    }

    public IReadOnlyDictionary<string, object?> ToPublicFields()
        => new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["flow38QuarantineRouteMapped"] = RouteMapped,
            ["flow38QuarantineConfigured"] = Configured,
            ["flow38QuarantineStoreConfigured"] = Configured,
            ["flow38QuarantineStoreConfiguredObserved"] = StoreConfigured,
            ["flow38QuarantineEndpointPath"] = RouteMapped ? EndpointPath : null,
            ["flow38BoundaryMetadataValid"] = MetadataValid,
            ["flow38BoundaryMetadataStatus"] = MetadataStatus,
            ["flow38BoundaryMetadataIssues"] = MetadataIssues,
            ["flow38ContractSchemaVersion"] = ContractSchemaVersion,
            ["flow38QuarantineOnly"] = QuarantineOnly,
            ["flow38Persisted"] = Persisted,
            ["flow38LegalVerdict"] = LegalVerdict,
            ["flow38AllowedUse"] = AllowedUse,
            ["flow38Route"] = Route,
            ["flow38SemanticQualityClaimed"] = SemanticQualityClaimed,
            ["flow38DpoTrainReady"] = DpoTrainReady,
            ["flow38DpoPairCreated"] = DpoPairCreated,
            ["flow38TrainingTriggered"] = TrainingTriggered,
            ["flow38CommonTrainingAllowed"] = CommonTrainingAllowed,
            ["flow38RawOutputAccepted"] = RawOutputAccepted,
            ["flow38RefsOnlyPublicResponse"] = RefsOnlyPublicResponse,
            ["flow38ProductReady"] = ProductReady
        };

    private static DesignConversionFlow38PublicBoundarySnapshot MissingRoute()
        => new(
            RouteMapped: false,
            EndpointPath: null,
            MetadataValid: false,
            MetadataStatus: RouteNotRegisteredStatus,
            MetadataIssues: [RouteNotRegisteredStatus],
            ContractSchemaVersion: MissingValue,
            QuarantineOnly: false,
            StoreConfigured: false,
            Persisted: false,
            LegalVerdict: MissingValue,
            AllowedUse: MissingValue,
            Route: MissingValue,
            SemanticQualityClaimed: false,
            DpoTrainReady: false,
            DpoPairCreated: false,
            TrainingTriggered: false,
            CommonTrainingAllowed: false,
            RawOutputAccepted: false,
            RefsOnlyPublicResponse: false,
            ProductReady: false);

    private static string ReadExpectedString(
        IReadOnlyDictionary<string, string>? metadata,
        string key,
        string expected,
        ICollection<string> issues)
    {
        if (metadata is null || !metadata.TryGetValue(key, out var value))
        {
            issues.Add("missing:" + key);
            return MissingValue;
        }

        if (string.IsNullOrWhiteSpace(value) || value.Length > 128 || value.Any(char.IsControl))
        {
            issues.Add("malformed:" + key);
            return MalformedValue;
        }

        if (!string.Equals(value, expected, StringComparison.Ordinal))
            issues.Add("unexpected:" + key);

        return value;
    }

    private static bool ReadBoolean(
        IReadOnlyDictionary<string, string>? metadata,
        string key,
        bool? expected,
        ICollection<string> issues)
    {
        if (metadata is null || !metadata.TryGetValue(key, out var value))
        {
            issues.Add("missing:" + key);
            return false;
        }

        bool parsed;
        if (string.Equals(value, bool.TrueString, StringComparison.Ordinal))
        {
            parsed = true;
        }
        else if (string.Equals(value, bool.FalseString, StringComparison.Ordinal))
        {
            parsed = false;
        }
        else
        {
            issues.Add("malformed:" + key);
            return false;
        }

        if (expected.HasValue && parsed != expected.Value)
        {
            issues.Add("unexpected:" + key);
            return false;
        }

        return parsed;
    }
}

public sealed record DesignConversionModelServerRepositoryPromotionOptions(
    IModelRepository Repository,
    IRemoteUnitLocator Locator,
    string ServerBaseUrl,
    string TenantId)
{
    public override string ToString()
        => $"{nameof(DesignConversionModelServerRepositoryPromotionOptions)} {{ " +
           $"{nameof(Repository)}Configured = {(Repository is not null).ToString()}, " +
           $"{nameof(Locator)}Configured = {(Locator is not null).ToString()}, " +
           $"{nameof(ServerBaseUrl)}Configured = {(!string.IsNullOrWhiteSpace(ServerBaseUrl)).ToString()}, " +
           $"{nameof(TenantId)}Configured = {(!string.IsNullOrWhiteSpace(TenantId)).ToString()}, " +
           $"{nameof(TenantId)} = [REDACTED] }}";
}

public sealed record DesignConversionModelServerPromotionHostOptions(
    string ArtifactRootDirectory,
    HttpClient? PromotionHttpClient = null,
    DesignConversionModelServerPromotionHttpUploaderOptions? PromotionHttpUploaderOptions = null,
    DesignConversionModelServerRepositoryPromotionOptions? RepositoryPromotionOptions = null,
    IDesignConversionDpoPolicyBaseProvider? DpoPolicyBaseProvider = null,
    DesignConversionModelServerRepositoryOwnedTrainingOptions?
        RepositoryOwnedTrainingOptions = null,
    // Явный native state root нужен только для узкого WSL `/mnt/c`
    // pair-store пути; ToString ниже не раскрывает этот приватный путь.
    DesignConversionTrainingIntakeDpoPairStoreOptions?
        TrainingIntakeDpoPairStoreOptions = null)
{
    public override string ToString()
        => $"{nameof(DesignConversionModelServerPromotionHostOptions)} {{ " +
           $"{nameof(ArtifactRootDirectory)} = [REDACTED], " +
           $"{nameof(PromotionHttpClient)}Configured = {(PromotionHttpClient is not null).ToString()}, " +
           $"{nameof(PromotionHttpUploaderOptions)}Configured = {(PromotionHttpUploaderOptions is not null).ToString()}, " +
           $"{nameof(PromotionHttpUploaderOptions)} = [REDACTED], " +
           $"{nameof(RepositoryPromotionOptions)}Configured = {(RepositoryPromotionOptions is not null).ToString()}, " +
           $"{nameof(RepositoryPromotionOptions)} = {RepositoryPromotionOptions?.ToString() ?? string.Empty}, " +
           $"{nameof(DpoPolicyBaseProvider)}Configured = {(DpoPolicyBaseProvider is not null).ToString()}, " +
           $"{nameof(RepositoryOwnedTrainingOptions)}Configured = {(RepositoryOwnedTrainingOptions is not null).ToString()}, " +
           $"{nameof(RepositoryOwnedTrainingOptions)} = [REDACTED], " +
           $"{nameof(TrainingIntakeDpoPairStoreOptions)}Configured = {(TrainingIntakeDpoPairStoreOptions is not null).ToString()}, " +
           $"{nameof(TrainingIntakeDpoPairStoreOptions)} = [REDACTED] }}";
}

public static class DesignConversionModelServerAspNetHostStartup
{
    public const string HostStartupSurface = nameof(DesignConversionModelServerAspNetHostStartup);
    public const string HostStartupBoundary =
        "asp_net_host_startup_local_route_registration_refs_only_no_public_deployment";

    public static IEndpointConventionBuilder ConfigureDesignConversionModelServer(
        this WebApplication app,
        DesignConversionModelServerEndpointRegistry registry)
    {
        if (app is null)
            throw new ArgumentNullException(nameof(app));

        RegisterRegistryLifetime(app, registry);
        return ConfigureDesignConversionModelServerCore(app, registry);
    }

    public static IEndpointConventionBuilder ConfigureDesignConversionModelServer(
        this WebApplication app,
        DesignConversionModelServerEndpointRegistry registry,
        DesignConversionModelServerArtifactUploadHttpHandler artifactUploadHandler)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(artifactUploadHandler);

        RegisterRegistryLifetime(app, registry);
        return ConfigureDesignConversionModelServerCore(
            app,
            registry,
            artifactUploadHandler: artifactUploadHandler);
    }

    public static IEndpointConventionBuilder ConfigureDesignConversionModelServer(
        this IEndpointRouteBuilder endpoints,
        DesignConversionModelServerEndpointRegistry registry)
        => ConfigureDesignConversionModelServerCore(endpoints, registry);

    public static IEndpointConventionBuilder ConfigureDesignConversionModelServer(
        this IEndpointRouteBuilder endpoints,
        DesignConversionModelServerEndpointRegistry registry,
        DesignConversionModelServerArtifactUploadHttpHandler artifactUploadHandler)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        ArgumentNullException.ThrowIfNull(artifactUploadHandler);

        return ConfigureDesignConversionModelServerCore(
            endpoints,
            registry,
            artifactUploadHandler: artifactUploadHandler);
    }

    private static IEndpointConventionBuilder ConfigureDesignConversionModelServerCore(
        IEndpointRouteBuilder endpoints,
        DesignConversionModelServerEndpointRegistry registry,
        PromotionHostArtifactDbSetup? promotionHostSetup = null,
        DesignConversionModelServerArtifactUploadHttpHandler? artifactUploadHandler = null)
    {
        if (endpoints is null)
            throw new ArgumentNullException(nameof(endpoints));
        if (registry is null)
            throw new ArgumentNullException(nameof(registry));
        DesignConversionModelServerEndpointRegistry
            .ThrowIfServerOwnedLocalImprovementLoopRegistrationAuthorityMissing(registry);

        var mappedRoutes = FullLocalRouteSet(artifactUploadHandler);
        var builders = new List<IEndpointConventionBuilder>
        {
            endpoints
                .MapDesignConversionModelServerEvaluate(registry)
                .WithMetadata(CreateStartupMetadata(
                    DesignConversionModelServerEndpointRegistry.EndpointPath,
                    mappedRoutes,
                    promotionHostSetup)),
            endpoints
                .MapDesignConversionModelServerInternalEvaluate(registry)
                .WithMetadata(CreateStartupMetadata(
                    DesignConversionModelServerEndpointRegistry.InternalEndpointPath,
                    mappedRoutes,
                    promotionHostSetup)),
            endpoints
                .MapDesignConversionModelServerImprovementLoop(registry)
                .WithMetadata(CreateStartupMetadata(
                    DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath,
                    mappedRoutes,
                    promotionHostSetup)),
            endpoints
                .MapDesignConversionModelServerVisualCostReadback(registry)
                .WithMetadata(CreateStartupMetadata(
                    DesignConversionModelServerEndpointRegistry.VisualCostReadbackEndpointPath,
                    mappedRoutes,
                    promotionHostSetup)),
            endpoints
                .MapDesignConversionModelServerStatusReadback(registry)
                .WithMetadata(CreateStartupMetadata(
                    DesignConversionModelServerEndpointRegistry.StatusReadbackEndpointPath,
                    mappedRoutes,
                    promotionHostSetup)),
            endpoints
                .MapDesignConversionModelServerDpoUsefulness(registry)
                .WithMetadata(CreateStartupMetadata(
                    DesignConversionModelServerEndpointRegistry.DpoUsefulnessEndpointPath,
                    mappedRoutes,
                    promotionHostSetup)),
            endpoints
                .MapDesignConversionModelServerTrainingIntake(registry)
                .WithMetadata(CreateStartupMetadata(
                    DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointPath,
                    mappedRoutes,
                    promotionHostSetup)),
            endpoints
                .MapDesignConversionModelServerFlow38Quarantine(registry)
                .WithMetadata(CreateStartupMetadata(
                    DesignConversionModelServerEndpointRegistry.Flow38QuarantineEndpointPath,
                    mappedRoutes,
                    promotionHostSetup)),
            endpoints
                .MapDesignConversionModelServerModelActivation(registry)
                .WithMetadata(CreateStartupMetadata(
                    DesignConversionModelServerEndpointRegistry.ModelActivationEndpointPath,
                    mappedRoutes,
                    promotionHostSetup)),
            endpoints
                .MapDesignConversionModelServerHumanApprovalChallenge(registry)
                .WithMetadata(CreateStartupMetadata(
                    DesignConversionModelServerEndpointRegistry.HumanApprovalChallengeEndpointPath,
                    mappedRoutes,
                    promotionHostSetup)),
            endpoints
                .MapDesignConversionModelServerHumanApprovalApprove(registry)
                .WithMetadata(CreateStartupMetadata(
                    DesignConversionModelServerEndpointRegistry.HumanApprovalApproveEndpointPath,
                    mappedRoutes,
                    promotionHostSetup))
        };

        if (artifactUploadHandler is not null)
        {
            builders.Add(
                endpoints
                    .MapDesignConversionModelServerArtifactUpload(artifactUploadHandler)
                    .WithMetadata(CreateStartupMetadata(
                        DesignConversionModelServerArtifactUploadHttpHandler.EndpointPath,
                        mappedRoutes,
                        promotionHostSetup)));
        }

        return new DesignConversionModelServerCompositeEndpointConventionBuilder(builders);
    }

    private static void RegisterRegistryLifetime(
        WebApplication app,
        DesignConversionModelServerEndpointRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);
        DesignConversionModelServerEndpointRegistry
            .ThrowIfServerOwnedLocalImprovementLoopRegistrationAuthorityMissing(registry);
        app.Lifetime.ApplicationStopping.Register(registry.Dispose);
    }

    public static IEndpointConventionBuilder ConfigureNonTrainingDesignConversionModelServer(
        this WebApplication app,
        DesignConversionModelServerEndpointRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(app);
        return ((IEndpointRouteBuilder)app).ConfigureNonTrainingDesignConversionModelServer(registry);
    }

    public static IEndpointConventionBuilder ConfigureNonTrainingDesignConversionModelServer(
        this IEndpointRouteBuilder endpoints,
        DesignConversionModelServerEndpointRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        ArgumentNullException.ThrowIfNull(registry);
        DesignConversionModelServerEndpointRegistry
            .ThrowIfServerOwnedLocalImprovementLoopRegistrationAuthorityMissing(registry);

        var mappedRoutes = NonTrainingLocalRouteSet();
        var builders = new IEndpointConventionBuilder[]
        {
            endpoints
                .MapDesignConversionModelServerImprovementLoop(registry)
                .WithMetadata(CreateStartupMetadata(
                    DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath,
                    mappedRoutes,
                    promotionHostSetup: null)),
            endpoints
                .MapDesignConversionModelServerFlow38Quarantine(registry)
                .WithMetadata(CreateStartupMetadata(
                    DesignConversionModelServerEndpointRegistry.Flow38QuarantineEndpointPath,
                    mappedRoutes,
                    promotionHostSetup: null))
        };

        return new DesignConversionModelServerCompositeEndpointConventionBuilder(builders);
    }

    public static IEndpointConventionBuilder ConfigureNonDurableDesignConversionModelServer(
        this WebApplication app,
        DesignConversionModelServerEndpointRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(app);
        return ((IEndpointRouteBuilder)app).ConfigureNonDurableDesignConversionModelServer(registry);
    }

    public static IEndpointConventionBuilder ConfigureNonDurableDesignConversionModelServer(
        this WebApplication app,
        DesignConversionModelServerEndpointRegistry registry,
        DesignConversionModelServerArtifactUploadHttpHandler artifactUploadHandler)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(artifactUploadHandler);
        return ((IEndpointRouteBuilder)app).ConfigureNonDurableDesignConversionModelServer(
            registry,
            artifactUploadHandler);
    }

    public static IEndpointConventionBuilder ConfigureNonDurableDesignConversionModelServer(
        this IEndpointRouteBuilder endpoints,
        DesignConversionModelServerEndpointRegistry registry)
        => ConfigureNonDurableDesignConversionModelServerCore(
            endpoints,
            registry,
            artifactUploadHandler: null);

    public static IEndpointConventionBuilder ConfigureNonDurableDesignConversionModelServer(
        this IEndpointRouteBuilder endpoints,
        DesignConversionModelServerEndpointRegistry registry,
        DesignConversionModelServerArtifactUploadHttpHandler artifactUploadHandler)
    {
        ArgumentNullException.ThrowIfNull(artifactUploadHandler);
        return ConfigureNonDurableDesignConversionModelServerCore(
            endpoints,
            registry,
            artifactUploadHandler);
    }

    private static IEndpointConventionBuilder ConfigureNonDurableDesignConversionModelServerCore(
        IEndpointRouteBuilder endpoints,
        DesignConversionModelServerEndpointRegistry registry,
        DesignConversionModelServerArtifactUploadHttpHandler? artifactUploadHandler)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        ArgumentNullException.ThrowIfNull(registry);
        DesignConversionModelServerEndpointRegistry
            .ThrowIfServerOwnedLocalImprovementLoopRegistrationAuthorityMissing(registry);

        var mappedRoutes = NonDurableLocalRouteSet(artifactUploadHandler);
        var builders = new List<IEndpointConventionBuilder>
        {
            endpoints
                .MapDesignConversionModelServerEvaluate(registry)
                .WithMetadata(CreateStartupMetadata(
                    DesignConversionModelServerEndpointRegistry.EndpointPath,
                    mappedRoutes,
                    promotionHostSetup: null)),
            endpoints
                .MapDesignConversionModelServerInternalEvaluate(registry)
                .WithMetadata(CreateStartupMetadata(
                    DesignConversionModelServerEndpointRegistry.InternalEndpointPath,
                    mappedRoutes,
                    promotionHostSetup: null)),
            endpoints
                .MapDesignConversionModelServerImprovementLoop(registry)
                .WithMetadata(CreateStartupMetadata(
                    DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath,
                    mappedRoutes,
                    promotionHostSetup: null)),
            endpoints
                .MapDesignConversionModelServerVisualCostReadback(registry)
                .WithMetadata(CreateStartupMetadata(
                    DesignConversionModelServerEndpointRegistry.VisualCostReadbackEndpointPath,
                    mappedRoutes,
                    promotionHostSetup: null)),
            endpoints
                .MapDesignConversionModelServerStatusReadback(registry)
                .WithMetadata(CreateStartupMetadata(
                    DesignConversionModelServerEndpointRegistry.StatusReadbackEndpointPath,
                    mappedRoutes,
                    promotionHostSetup: null)),
            endpoints
                .MapDesignConversionModelServerFlow38Quarantine(registry)
                .WithMetadata(CreateStartupMetadata(
                    DesignConversionModelServerEndpointRegistry.Flow38QuarantineEndpointPath,
                    mappedRoutes,
                    promotionHostSetup: null)),
            endpoints
                .MapDesignConversionModelServerModelActivation(registry)
                .WithMetadata(CreateStartupMetadata(
                    DesignConversionModelServerEndpointRegistry.ModelActivationEndpointPath,
                    mappedRoutes,
                    promotionHostSetup: null)),
            endpoints
                .MapDesignConversionModelServerHumanApprovalChallenge(registry)
                .WithMetadata(CreateStartupMetadata(
                    DesignConversionModelServerEndpointRegistry.HumanApprovalChallengeEndpointPath,
                    mappedRoutes,
                    promotionHostSetup: null)),
            endpoints
                .MapDesignConversionModelServerHumanApprovalApprove(registry)
                .WithMetadata(CreateStartupMetadata(
                    DesignConversionModelServerEndpointRegistry.HumanApprovalApproveEndpointPath,
                    mappedRoutes,
                    promotionHostSetup: null))
        };

        if (artifactUploadHandler is not null)
        {
            builders.Add(
                endpoints
                    .MapDesignConversionModelServerArtifactUpload(artifactUploadHandler)
                    .WithMetadata(CreateStartupMetadata(
                        DesignConversionModelServerArtifactUploadHttpHandler.EndpointPath,
                        mappedRoutes,
                        promotionHostSetup: null)));
        }

        return new DesignConversionModelServerCompositeEndpointConventionBuilder(builders);
    }

    public static DesignConversionModelServerEndpointRegistry CreateImprovementLoopRegistry(
        DesignConversionLocalServiceComposition composition,
        IDesignConversionImprovementCandidateStrategy candidateStrategy)
    {
        if (composition is null)
            throw new ArgumentNullException(nameof(composition));
        DesignConversionModelServerEndpointRegistry
            .ThrowIfServerOwnedLocalCompositionAuthorityMissing(composition);
        if (candidateStrategy is null)
            throw new ArgumentNullException(nameof(candidateStrategy));

        var registry = new DesignConversionModelServerEndpointRegistry();
        var registered = registry.RegisterDesignConversionImprovementLoopFromComposition(
            composition,
            candidateStrategy);
        if (!registered.IsSuccess)
        {
            throw new InvalidOperationException(
                registered.ErrorMessage ??
                "DesignConversion improvement-loop host startup registration failed.");
        }

        return registry;
    }

    public static DesignConversionModelServerEndpointRegistry CreateNonTrainingLocalRouteRegistry(
        DesignConversionLocalServiceComposition composition,
        IDesignConversionImprovementCandidateStrategy candidateStrategy,
        IDesignConversionFlow38QuarantineStore flow38QuarantineStore,
        string? flow38QuarantineBearerToken)
    {
        ArgumentNullException.ThrowIfNull(flow38QuarantineStore);

        var registry = CreateImprovementLoopRegistry(composition, candidateStrategy);
        ThrowIfRegistrationFailed(
            registry.RegisterDesignConversionFlow38Quarantine(
                new DesignConversionModelServerFlow38QuarantineHandler(
                    flow38QuarantineStore,
                    flow38QuarantineBearerToken)),
            "DesignConversion FLOW-38 quarantine non-training local host startup registration failed.");
        return registry;
    }

    public static DesignConversionModelServerEndpointRegistry CreateImprovementLoopRegistry(
        DesignConversionLocalServiceComposition composition,
        IDesignConversionImprovementCandidateStrategy candidateStrategy,
        IDesignConversionLocalArtifactDb artifactDb,
        Func<DateTimeOffset>? utcNow = null,
        DesignConversionModelServerRouteAdmissionOptions? routeAdmissionOptions = null)
        => CreateImprovementLoopRegistry(
            composition,
            candidateStrategy,
            artifactDb,
            DesignConversionTrainingIntakePreflightCoordinatorOptions.Disabled,
            utcNow,
            routeAdmissionOptions);

    public static DesignConversionModelServerEndpointRegistry CreateImprovementLoopRegistry(
        DesignConversionLocalServiceComposition composition,
        IDesignConversionImprovementCandidateStrategy candidateStrategy,
        IDesignConversionLocalArtifactDb artifactDb,
        DesignConversionTrainingIntakePreflightCoordinatorOptions trainingIntakePreflightOptions,
        Func<DateTimeOffset>? utcNow = null,
        DesignConversionModelServerRouteAdmissionOptions? routeAdmissionOptions = null)
    {
        if (composition is null)
            throw new ArgumentNullException(nameof(composition));
        DesignConversionModelServerEndpointRegistry
            .ThrowIfServerOwnedLocalCompositionAuthorityMissing(composition);
        if (candidateStrategy is null)
            throw new ArgumentNullException(nameof(candidateStrategy));
        if (artifactDb is null)
            throw new ArgumentNullException(nameof(artifactDb));

        var registry = new DesignConversionModelServerEndpointRegistry();
        var registered = registry.RegisterDesignConversionImprovementLoopFromComposition(
            composition,
            candidateStrategy,
            artifactDb,
            trainingIntakePreflightOptions,
            utcNow,
            DesignConversionModelServerRouteAdmission.Create(routeAdmissionOptions, utcNow));
        if (!registered.IsSuccess)
        {
            throw new InvalidOperationException(
                registered.ErrorMessage ??
                "DesignConversion improvement-loop host startup registration with local artifact DB failed.");
        }

        return registry;
    }

    public static DesignConversionModelServerEndpointRegistry CreateLocalRouteRegistry(
        DesignConversionLocalServiceComposition composition,
        IDesignConversionImprovementCandidateStrategy candidateStrategy,
        IDesignConversionLocalArtifactDb? artifactDb,
        Func<DateTimeOffset>? utcNow = null,
        DesignConversionPromotedLocalModel? promotedLocalModel = null,
        string? activationStateRootDirectory = null,
        string? modelLifecycleAuthToken = null,
        DesignConversionModelServerModelLifecycleTokenKeyRingOptions? modelLifecycleTokenKeyRing = null,
        string? humanApprovalStateRootDirectory = null,
        IDesignConversionModelServerArtifactRefResolver? artifactRefResolver = null,
        DesignConversionModelServerRouteAdmissionOptions? routeAdmissionOptions = null,
        IDesignConversionVisualCostLedger? visualCostLedger = null,
        IDesignConversionVisualCostBudgetStore? visualCostBudgetStore = null,
        IDesignConversionFlow38QuarantineStore? flow38QuarantineStore = null,
        string? flow38QuarantineBearerToken = null,
        DesignConversionModelServerRepositoryOwnedTrainingOptions?
            repositoryOwnedTrainingOptions = null,
        ContinuousLearningTp1DpoConfiguration?
            repositoryOwnedDpoTeacherConfiguration = null,
        DesignConversionModelServerMeasurementGateThresholdConfiguration?
            measurementGateThresholdConfiguration = null)
        => CreateLocalRouteRegistry(
            composition,
            candidateStrategy,
            artifactDb,
            DesignConversionTrainingIntakePreflightCoordinatorOptions.Disabled,
            utcNow,
            promotedLocalModel,
            activationStateRootDirectory,
            modelLifecycleAuthToken,
            modelLifecycleTokenKeyRing,
            humanApprovalStateRootDirectory,
            artifactRefResolver,
            routeAdmissionOptions,
            visualCostLedger,
            visualCostBudgetStore,
            flow38QuarantineStore,
            flow38QuarantineBearerToken,
            repositoryOwnedTrainingOptions,
            repositoryOwnedDpoTeacherConfiguration,
            measurementGateThresholdConfiguration);

    public static DesignConversionModelServerEndpointRegistry CreateLocalRouteRegistry(
        DesignConversionLocalServiceComposition composition,
        IDesignConversionImprovementCandidateStrategy candidateStrategy,
        IDesignConversionLocalArtifactDb? artifactDb,
        DesignConversionTrainingIntakePreflightCoordinatorOptions trainingIntakePreflightOptions,
        Func<DateTimeOffset>? utcNow = null,
        DesignConversionPromotedLocalModel? promotedLocalModel = null,
        string? activationStateRootDirectory = null,
        string? modelLifecycleAuthToken = null,
        DesignConversionModelServerModelLifecycleTokenKeyRingOptions? modelLifecycleTokenKeyRing = null,
        string? humanApprovalStateRootDirectory = null,
        IDesignConversionModelServerArtifactRefResolver? artifactRefResolver = null,
        DesignConversionModelServerRouteAdmissionOptions? routeAdmissionOptions = null,
        IDesignConversionVisualCostLedger? visualCostLedger = null,
        IDesignConversionVisualCostBudgetStore? visualCostBudgetStore = null,
        IDesignConversionFlow38QuarantineStore? flow38QuarantineStore = null,
        string? flow38QuarantineBearerToken = null,
        DesignConversionModelServerRepositoryOwnedTrainingOptions?
            repositoryOwnedTrainingOptions = null,
        ContinuousLearningTp1DpoConfiguration?
            repositoryOwnedDpoTeacherConfiguration = null,
        DesignConversionModelServerMeasurementGateThresholdConfiguration?
            measurementGateThresholdConfiguration = null)
        => CreateLocalRouteRegistryCore(
            composition,
            candidateStrategy,
            artifactDb,
            utcNow,
            promotedLocalModel,
            activationStateRootDirectory,
            modelLifecycleAuthToken,
            modelLifecycleTokenKeyRing,
            humanApprovalStateRootDirectory,
            artifactRefResolver,
            routeAdmissionOptions,
            promotionHostSetup: null,
            visualCostLedger: visualCostLedger,
            visualCostBudgetStore: visualCostBudgetStore,
            flow38QuarantineStore: flow38QuarantineStore,
            flow38QuarantineBearerToken: flow38QuarantineBearerToken,
            repositoryOwnedTrainingOptions: repositoryOwnedTrainingOptions,
            repositoryOwnedDpoTeacherConfiguration:
                repositoryOwnedDpoTeacherConfiguration,
            measurementGateThresholdConfiguration: measurementGateThresholdConfiguration,
            trainingIntakePreflightOptions: trainingIntakePreflightOptions);

    internal static DesignConversionModelServerEndpointRegistry
        CreateLocalRouteRegistryWithLocalIndependentPreferenceEvaluator(
            DesignConversionLocalServiceComposition composition,
            IDesignConversionImprovementCandidateStrategy candidateStrategy,
            IDesignConversionLocalArtifactDb artifactDb,
            ContinuousLearningTp1DpoConfiguration
                localIndependentPreferenceEvaluatorConfiguration,
            DesignConversionModelServerMeasurementGateThresholdConfiguration?
                measurementGateThresholdConfiguration = null,
            IDesignConversionModelServerArtifactRefResolver?
                artifactRefResolver = null,
            DesignConversionModelServerRepositoryOwnedTrainingOptions?
                repositoryOwnedTrainingOptions = null,
            ContinuousLearningTp1DpoConfiguration?
                repositoryOwnedDpoTeacherConfiguration = null)
    {
        ArgumentNullException.ThrowIfNull(artifactDb);
        ArgumentNullException.ThrowIfNull(
            localIndependentPreferenceEvaluatorConfiguration);
        return CreateLocalRouteRegistryCore(
            composition,
            candidateStrategy,
            artifactDb,
            measurementGateThresholdConfiguration:
                measurementGateThresholdConfiguration,
            artifactRefResolver: artifactRefResolver,
            repositoryOwnedTrainingOptions: repositoryOwnedTrainingOptions,
            repositoryOwnedDpoTeacherConfiguration:
                repositoryOwnedDpoTeacherConfiguration ??
                localIndependentPreferenceEvaluatorConfiguration,
            trainingIntakePreflightOptions:
                DesignConversionTrainingIntakePreflightCoordinatorOptions
                    .Disabled,
            localIndependentPreferenceEvaluatorConfiguration:
                localIndependentPreferenceEvaluatorConfiguration);
    }

    private static DesignConversionModelServerEndpointRegistry CreateLocalRouteRegistryCore(
        DesignConversionLocalServiceComposition composition,
        IDesignConversionImprovementCandidateStrategy candidateStrategy,
        IDesignConversionLocalArtifactDb? artifactDb,
        Func<DateTimeOffset>? utcNow = null,
        DesignConversionPromotedLocalModel? promotedLocalModel = null,
        string? activationStateRootDirectory = null,
        string? modelLifecycleAuthToken = null,
        DesignConversionModelServerModelLifecycleTokenKeyRingOptions? modelLifecycleTokenKeyRing = null,
        string? humanApprovalStateRootDirectory = null,
        IDesignConversionModelServerArtifactRefResolver? artifactRefResolver = null,
        DesignConversionModelServerRouteAdmissionOptions? routeAdmissionOptions = null,
        PromotionHostArtifactDbSetup? promotionHostSetup = null,
        IDesignConversionVisualCostLedger? visualCostLedger = null,
        IDesignConversionVisualCostBudgetStore? visualCostBudgetStore = null,
        IDesignConversionFlow38QuarantineStore? flow38QuarantineStore = null,
        string? flow38QuarantineBearerToken = null,
        DesignConversionModelServerRepositoryOwnedTrainingOptions?
            repositoryOwnedTrainingOptions = null,
        DesignConversionModelServerMeasurementGateThresholdConfiguration?
            measurementGateThresholdConfiguration = null,
        ContinuousLearningTp1DpoConfiguration?
            repositoryOwnedDpoTeacherConfiguration = null,
        DesignConversionTrainingIntakePreflightCoordinatorOptions?
            trainingIntakePreflightOptions = null,
        ContinuousLearningTp1DpoConfiguration?
            localIndependentPreferenceEvaluatorConfiguration = null)
    {
        if (composition is null)
            throw new ArgumentNullException(nameof(composition));
        DesignConversionModelServerEndpointRegistry
            .ThrowIfServerOwnedLocalCompositionAuthorityMissing(composition);
        if (candidateStrategy is null)
            throw new ArgumentNullException(nameof(candidateStrategy));
        var registry = new DesignConversionModelServerEndpointRegistry();
        var dpoUsefulnessApi = artifactDb is null
            ? null
            : new DesignConversionDpoUsefulnessApi(artifactDb);
        var routeAdmission = DesignConversionModelServerRouteAdmission.Create(
            routeAdmissionOptions,
            utcNow);
        var lifecycleTokenKeyRingSetup = CreateModelLifecycleTokenKeyRingSetup(
            modelLifecycleAuthToken,
            modelLifecycleTokenKeyRing,
            utcNow);
        var activationSetup = CreateModelActivationSetup(
            activationStateRootDirectory,
            lifecycleTokenKeyRingSetup,
            utcNow,
            artifactDb as IDesignConversionLocalPromotionActivationAuthority);
        var humanApprovalSetup = CreateHumanApprovalSetup(humanApprovalStateRootDirectory);
        var effectiveVisualCostBudgetStore =
            visualCostBudgetStore ?? JsonlDesignConversionVisualCostBudgetStore.CreateDefault();
        var fallbackEvaluator = new DesignConversionInternalServiceThinClientJobEvaluator(composition.Service);
        IDesignConversionThinClientJobEvaluator evaluator = fallbackEvaluator;
        DesignConversionActivatedModelRuntimeEvaluator? activatedEvaluator = null;
        if (activationSetup.Registry is not null || routeAdmissionOptions?.Enabled == true)
        {
            activatedEvaluator = new DesignConversionActivatedModelRuntimeEvaluator(
                activationSetup.Registry,
                composition.RenderProvider,
                composition.ComparisonProvider,
                composition.VisualJudge,
                promotionHostSetup?.ActivatedModelCheckpointLoader);
            evaluator = activatedEvaluator;
        }

        var modelServerApi = activatedEvaluator is null
            ? new DesignConversionModelServerApi(
                evaluator,
                utcNow,
                artifactDb,
                dpoUsefulnessApi,
                promotedLocalModel,
                artifactRefResolver,
                activatedModelRuntimeReadback: null,
                humanApprovalStore: humanApprovalSetup.Store,
                visualCostLedger: visualCostLedger,
                visualCostBudgetStore: effectiveVisualCostBudgetStore)
            : DesignConversionModelServerApi.CreateForProductionActivatedRuntime(
                activatedEvaluator,
                utcNow,
                artifactDb,
                dpoUsefulnessApi,
                promotedLocalModel,
                artifactRefResolver,
                humanApprovalSetup.Store,
                visualCostLedger,
                effectiveVisualCostBudgetStore);

        ThrowIfRegistrationFailed(
            registry.RegisterDesignConversionEvaluate(
                new DesignConversionModelServerHttpHandler(modelServerApi, routeAdmission)),
            "DesignConversion evaluate full local host startup registration failed.");
        var improvementLoopRegistration = artifactDb is null
            ? registry.RegisterDesignConversionImprovementLoopFromComposition(
                composition,
                candidateStrategy,
                routeAdmission)
            : registry.RegisterDesignConversionImprovementLoopFromComposition(
                composition,
                candidateStrategy,
                artifactDb,
                trainingIntakePreflightOptions ??
                    DesignConversionTrainingIntakePreflightCoordinatorOptions.Disabled,
                utcNow,
                routeAdmission,
                repositoryOwnedTrainingOptions,
                repositoryOwnedDpoTeacherConfiguration ??
                    localIndependentPreferenceEvaluatorConfiguration);
        ThrowIfRegistrationFailed(
            improvementLoopRegistration,
            artifactDb is null
                ? "DesignConversion improvement-loop non-durable local host startup registration failed."
                : "DesignConversion improvement-loop full local host startup registration failed.");
        ThrowIfRegistrationFailed(
            registry.RegisterDesignConversionVisualCostReadback(
                new DesignConversionModelServerVisualCostHandler(modelServerApi, routeAdmission)),
            "DesignConversion visual-cost readback full local host startup registration failed.");
        ThrowIfRegistrationFailed(
            registry.RegisterDesignConversionStatusReadback(
                new DesignConversionModelServerStatusHandler(modelServerApi, routeAdmission)),
            "DesignConversion status/readback full local host startup registration failed.");
        if (dpoUsefulnessApi is not null)
        {
            var trainingIntakeHandler = promotionHostSetup is null
                ? DesignConversionModelServerTrainingIntakeHandler.CreateForProductionHost(
                    dpoUsefulnessApi,
                    measurementGateThresholdConfiguration,
                    lifecycleTokenKeyRingSetup.TokenKeyRing,
                    repositoryOwnedTrainingOptions:
                        repositoryOwnedTrainingOptions)
                : DesignConversionModelServerTrainingIntakeHandler.CreateForValidatedPromotionHost(
                    dpoUsefulnessApi,
                    promotionHostSetup.ExpectedTenantId,
                    promotionHostSetup.RemotePromotionConfigured,
                    measurementGateThresholdConfiguration,
                    lifecycleTokenKeyRingSetup.TokenKeyRing,
                    repositoryOwnedTrainingOptions:
                        promotionHostSetup.RepositoryOwnedTrainingOptions);
            ThrowIfRegistrationFailed(
                registry.RegisterDesignConversionDpoUsefulness(
                    new DesignConversionModelServerDpoUsefulnessHandler(dpoUsefulnessApi, routeAdmission)),
                "DesignConversion DPO usefulness full local host startup registration failed.");
            ThrowIfRegistrationFailed(
                registry.RegisterDesignConversionTrainingIntake(
                    trainingIntakeHandler),
                "DesignConversion training-intake full local host startup registration failed.");
        }

        ThrowIfRegistrationFailed(
            registry.RegisterDesignConversionFlow38Quarantine(
                new DesignConversionModelServerFlow38QuarantineHandler(
                    flow38QuarantineStore,
                    flow38QuarantineBearerToken)),
            "DesignConversion FLOW-38 quarantine full local host startup registration failed.");
        ThrowIfRegistrationFailed(
            registry.RegisterDesignConversionModelActivation(activationSetup.Handler),
            "DesignConversion model activation full local host startup registration failed.");
        ThrowIfRegistrationFailed(
            registry.RegisterDesignConversionHumanApproval(humanApprovalSetup.CreateHandler(modelServerApi)),
            "DesignConversion human approval full local host startup registration failed.");

        registry.ConfigureLocalIndependentPreferenceEvaluator(
            artifactDb,
            localIndependentPreferenceEvaluatorConfiguration);

        return registry;
    }

    public static DesignConversionModelServerEndpointRegistry CreateLocalRouteRegistry(
        DesignConversionLocalServiceComposition composition,
        IDesignConversionImprovementCandidateStrategy candidateStrategy,
        DesignConversionModelServerPromotionHostOptions promotionHostOptions,
        Func<DateTimeOffset>? utcNow = null,
        DesignConversionPromotedLocalModel? promotedLocalModel = null,
        string? activationStateRootDirectory = null,
        string? modelLifecycleAuthToken = null,
        DesignConversionModelServerModelLifecycleTokenKeyRingOptions? modelLifecycleTokenKeyRing = null,
        string? humanApprovalStateRootDirectory = null,
        IDesignConversionModelServerArtifactRefResolver? artifactRefResolver = null,
        DesignConversionModelServerRouteAdmissionOptions? routeAdmissionOptions = null,
        IDesignConversionFlow38QuarantineStore? flow38QuarantineStore = null,
        string? flow38QuarantineBearerToken = null,
        DesignConversionModelServerMeasurementGateThresholdConfiguration?
            measurementGateThresholdConfiguration = null)
        => CreateLocalRouteRegistry(
            composition,
            candidateStrategy,
            promotionHostOptions,
            DesignConversionTrainingIntakePreflightCoordinatorOptions.Disabled,
            utcNow,
            promotedLocalModel,
            activationStateRootDirectory,
            modelLifecycleAuthToken,
            modelLifecycleTokenKeyRing,
            humanApprovalStateRootDirectory,
            artifactRefResolver,
            routeAdmissionOptions,
            flow38QuarantineStore,
            flow38QuarantineBearerToken,
            measurementGateThresholdConfiguration);

    public static DesignConversionModelServerEndpointRegistry CreateLocalRouteRegistry(
        DesignConversionLocalServiceComposition composition,
        IDesignConversionImprovementCandidateStrategy candidateStrategy,
        DesignConversionModelServerPromotionHostOptions promotionHostOptions,
        DesignConversionTrainingIntakePreflightCoordinatorOptions trainingIntakePreflightOptions,
        Func<DateTimeOffset>? utcNow = null,
        DesignConversionPromotedLocalModel? promotedLocalModel = null,
        string? activationStateRootDirectory = null,
        string? modelLifecycleAuthToken = null,
        DesignConversionModelServerModelLifecycleTokenKeyRingOptions? modelLifecycleTokenKeyRing = null,
        string? humanApprovalStateRootDirectory = null,
        IDesignConversionModelServerArtifactRefResolver? artifactRefResolver = null,
        DesignConversionModelServerRouteAdmissionOptions? routeAdmissionOptions = null,
        IDesignConversionFlow38QuarantineStore? flow38QuarantineStore = null,
        string? flow38QuarantineBearerToken = null,
        DesignConversionModelServerMeasurementGateThresholdConfiguration?
            measurementGateThresholdConfiguration = null)
        => CreatePromotionHostRouteRegistrySetup(
            composition,
            candidateStrategy,
            promotionHostOptions,
            utcNow,
            promotedLocalModel,
            activationStateRootDirectory,
            modelLifecycleAuthToken,
            modelLifecycleTokenKeyRing,
            humanApprovalStateRootDirectory,
            artifactRefResolver,
            routeAdmissionOptions,
            flow38QuarantineStore: flow38QuarantineStore,
            flow38QuarantineBearerToken: flow38QuarantineBearerToken,
            measurementGateThresholdConfiguration: measurementGateThresholdConfiguration,
            trainingIntakePreflightOptions: trainingIntakePreflightOptions)
            .Registry;

    public static IEndpointConventionBuilder ConfigureDesignConversionModelServer(
        this WebApplication app,
        DesignConversionLocalServiceComposition composition,
        IDesignConversionImprovementCandidateStrategy candidateStrategy)
        => app.ConfigureDesignConversionModelServer(
            CreateImprovementLoopRegistry(composition, candidateStrategy));

    public static IEndpointConventionBuilder ConfigureDesignConversionModelServer(
        this IEndpointRouteBuilder endpoints,
        DesignConversionLocalServiceComposition composition,
        IDesignConversionImprovementCandidateStrategy candidateStrategy)
        => endpoints.ConfigureDesignConversionModelServer(
            CreateImprovementLoopRegistry(composition, candidateStrategy));

    public static IEndpointConventionBuilder ConfigureDesignConversionModelServer(
        this WebApplication app,
        DesignConversionLocalServiceComposition composition,
        IDesignConversionImprovementCandidateStrategy candidateStrategy,
        IDesignConversionLocalArtifactDb? artifactDb,
        Func<DateTimeOffset>? utcNow = null,
        DesignConversionPromotedLocalModel? promotedLocalModel = null,
        string? activationStateRootDirectory = null,
        string? modelLifecycleAuthToken = null,
        DesignConversionModelServerModelLifecycleTokenKeyRingOptions? modelLifecycleTokenKeyRing = null,
        string? humanApprovalStateRootDirectory = null,
        IDesignConversionModelServerArtifactRefResolver? artifactRefResolver = null,
        DesignConversionModelServerRouteAdmissionOptions? routeAdmissionOptions = null,
        IDesignConversionFlow38QuarantineStore? flow38QuarantineStore = null,
        string? flow38QuarantineBearerToken = null,
        DesignConversionModelServerMeasurementGateThresholdConfiguration?
            measurementGateThresholdConfiguration = null)
        => ConfigureDesignConversionModelServer(
            app,
            composition,
            candidateStrategy,
            artifactDb,
            DesignConversionTrainingIntakePreflightCoordinatorOptions.Disabled,
            utcNow,
            promotedLocalModel,
            activationStateRootDirectory,
            modelLifecycleAuthToken,
            modelLifecycleTokenKeyRing,
            humanApprovalStateRootDirectory,
            artifactRefResolver,
            routeAdmissionOptions,
            flow38QuarantineStore,
            flow38QuarantineBearerToken,
            measurementGateThresholdConfiguration);

    public static IEndpointConventionBuilder ConfigureDesignConversionModelServer(
        this WebApplication app,
        DesignConversionLocalServiceComposition composition,
        IDesignConversionImprovementCandidateStrategy candidateStrategy,
        IDesignConversionLocalArtifactDb? artifactDb,
        DesignConversionTrainingIntakePreflightCoordinatorOptions trainingIntakePreflightOptions,
        Func<DateTimeOffset>? utcNow = null,
        DesignConversionPromotedLocalModel? promotedLocalModel = null,
        string? activationStateRootDirectory = null,
        string? modelLifecycleAuthToken = null,
        DesignConversionModelServerModelLifecycleTokenKeyRingOptions? modelLifecycleTokenKeyRing = null,
        string? humanApprovalStateRootDirectory = null,
        IDesignConversionModelServerArtifactRefResolver? artifactRefResolver = null,
        DesignConversionModelServerRouteAdmissionOptions? routeAdmissionOptions = null,
        IDesignConversionFlow38QuarantineStore? flow38QuarantineStore = null,
        string? flow38QuarantineBearerToken = null,
        DesignConversionModelServerMeasurementGateThresholdConfiguration?
            measurementGateThresholdConfiguration = null)
        => app.ConfigureDesignConversionModelServer(
            CreateLocalRouteRegistry(
                composition,
                candidateStrategy,
                artifactDb,
                trainingIntakePreflightOptions,
                utcNow,
                promotedLocalModel,
                activationStateRootDirectory,
                modelLifecycleAuthToken,
                modelLifecycleTokenKeyRing,
                humanApprovalStateRootDirectory,
                artifactRefResolver,
                routeAdmissionOptions,
                flow38QuarantineStore: flow38QuarantineStore,
                flow38QuarantineBearerToken: flow38QuarantineBearerToken,
                measurementGateThresholdConfiguration: measurementGateThresholdConfiguration));

    public static IEndpointConventionBuilder ConfigureDesignConversionModelServer(
        this WebApplication app,
        DesignConversionLocalServiceComposition composition,
        IDesignConversionImprovementCandidateStrategy candidateStrategy,
        DesignConversionModelServerPromotionHostOptions promotionHostOptions,
        Func<DateTimeOffset>? utcNow = null,
        DesignConversionPromotedLocalModel? promotedLocalModel = null,
        string? activationStateRootDirectory = null,
        string? modelLifecycleAuthToken = null,
        DesignConversionModelServerModelLifecycleTokenKeyRingOptions? modelLifecycleTokenKeyRing = null,
        string? humanApprovalStateRootDirectory = null,
        IDesignConversionModelServerArtifactRefResolver? artifactRefResolver = null,
        DesignConversionModelServerRouteAdmissionOptions? routeAdmissionOptions = null,
        IDesignConversionFlow38QuarantineStore? flow38QuarantineStore = null,
        string? flow38QuarantineBearerToken = null,
        DesignConversionModelServerMeasurementGateThresholdConfiguration?
            measurementGateThresholdConfiguration = null)
        => ConfigureDesignConversionModelServer(
            app,
            composition,
            candidateStrategy,
            promotionHostOptions,
            DesignConversionTrainingIntakePreflightCoordinatorOptions.Disabled,
            utcNow,
            promotedLocalModel,
            activationStateRootDirectory,
            modelLifecycleAuthToken,
            modelLifecycleTokenKeyRing,
            humanApprovalStateRootDirectory,
            artifactRefResolver,
            routeAdmissionOptions,
            flow38QuarantineStore,
            flow38QuarantineBearerToken,
            measurementGateThresholdConfiguration);

    public static IEndpointConventionBuilder ConfigureDesignConversionModelServer(
        this WebApplication app,
        DesignConversionLocalServiceComposition composition,
        IDesignConversionImprovementCandidateStrategy candidateStrategy,
        DesignConversionModelServerPromotionHostOptions promotionHostOptions,
        DesignConversionTrainingIntakePreflightCoordinatorOptions trainingIntakePreflightOptions,
        Func<DateTimeOffset>? utcNow = null,
        DesignConversionPromotedLocalModel? promotedLocalModel = null,
        string? activationStateRootDirectory = null,
        string? modelLifecycleAuthToken = null,
        DesignConversionModelServerModelLifecycleTokenKeyRingOptions? modelLifecycleTokenKeyRing = null,
        string? humanApprovalStateRootDirectory = null,
        IDesignConversionModelServerArtifactRefResolver? artifactRefResolver = null,
        DesignConversionModelServerRouteAdmissionOptions? routeAdmissionOptions = null,
        IDesignConversionFlow38QuarantineStore? flow38QuarantineStore = null,
        string? flow38QuarantineBearerToken = null,
        DesignConversionModelServerMeasurementGateThresholdConfiguration?
            measurementGateThresholdConfiguration = null)
    {
        if (app is null)
            throw new ArgumentNullException(nameof(app));

        var startup = CreatePromotionHostRouteRegistrySetup(
            composition,
            candidateStrategy,
            promotionHostOptions,
            utcNow,
            promotedLocalModel,
            activationStateRootDirectory,
            modelLifecycleAuthToken,
            modelLifecycleTokenKeyRing,
            humanApprovalStateRootDirectory,
            artifactRefResolver,
            routeAdmissionOptions,
            flow38QuarantineStore: flow38QuarantineStore,
            flow38QuarantineBearerToken: flow38QuarantineBearerToken,
            measurementGateThresholdConfiguration: measurementGateThresholdConfiguration,
            trainingIntakePreflightOptions: trainingIntakePreflightOptions);
        return ConfigureDesignConversionModelServerCore(
            app,
            startup.Registry,
            startup.PromotionHostSetup);
    }

    public static IEndpointConventionBuilder ConfigureDesignConversionModelServer(
        this IEndpointRouteBuilder endpoints,
        DesignConversionLocalServiceComposition composition,
        IDesignConversionImprovementCandidateStrategy candidateStrategy,
        IDesignConversionLocalArtifactDb? artifactDb,
        Func<DateTimeOffset>? utcNow = null,
        DesignConversionPromotedLocalModel? promotedLocalModel = null,
        string? activationStateRootDirectory = null,
        string? modelLifecycleAuthToken = null,
        DesignConversionModelServerModelLifecycleTokenKeyRingOptions? modelLifecycleTokenKeyRing = null,
        string? humanApprovalStateRootDirectory = null,
        IDesignConversionModelServerArtifactRefResolver? artifactRefResolver = null,
        DesignConversionModelServerRouteAdmissionOptions? routeAdmissionOptions = null,
        IDesignConversionFlow38QuarantineStore? flow38QuarantineStore = null,
        string? flow38QuarantineBearerToken = null,
        DesignConversionModelServerMeasurementGateThresholdConfiguration?
            measurementGateThresholdConfiguration = null)
        => ConfigureDesignConversionModelServer(
            endpoints,
            composition,
            candidateStrategy,
            artifactDb,
            DesignConversionTrainingIntakePreflightCoordinatorOptions.Disabled,
            utcNow,
            promotedLocalModel,
            activationStateRootDirectory,
            modelLifecycleAuthToken,
            modelLifecycleTokenKeyRing,
            humanApprovalStateRootDirectory,
            artifactRefResolver,
            routeAdmissionOptions,
            flow38QuarantineStore,
            flow38QuarantineBearerToken,
            measurementGateThresholdConfiguration);

    public static IEndpointConventionBuilder ConfigureDesignConversionModelServer(
        this IEndpointRouteBuilder endpoints,
        DesignConversionLocalServiceComposition composition,
        IDesignConversionImprovementCandidateStrategy candidateStrategy,
        IDesignConversionLocalArtifactDb? artifactDb,
        DesignConversionTrainingIntakePreflightCoordinatorOptions trainingIntakePreflightOptions,
        Func<DateTimeOffset>? utcNow = null,
        DesignConversionPromotedLocalModel? promotedLocalModel = null,
        string? activationStateRootDirectory = null,
        string? modelLifecycleAuthToken = null,
        DesignConversionModelServerModelLifecycleTokenKeyRingOptions? modelLifecycleTokenKeyRing = null,
        string? humanApprovalStateRootDirectory = null,
        IDesignConversionModelServerArtifactRefResolver? artifactRefResolver = null,
        DesignConversionModelServerRouteAdmissionOptions? routeAdmissionOptions = null,
        IDesignConversionFlow38QuarantineStore? flow38QuarantineStore = null,
        string? flow38QuarantineBearerToken = null,
        DesignConversionModelServerMeasurementGateThresholdConfiguration?
            measurementGateThresholdConfiguration = null)
        => endpoints.ConfigureDesignConversionModelServer(
            CreateLocalRouteRegistry(
                composition,
                candidateStrategy,
                artifactDb,
                trainingIntakePreflightOptions,
                utcNow,
                promotedLocalModel,
                activationStateRootDirectory,
                modelLifecycleAuthToken,
                modelLifecycleTokenKeyRing,
                humanApprovalStateRootDirectory,
                artifactRefResolver,
                routeAdmissionOptions,
                flow38QuarantineStore: flow38QuarantineStore,
                flow38QuarantineBearerToken: flow38QuarantineBearerToken,
                measurementGateThresholdConfiguration: measurementGateThresholdConfiguration));

    public static IEndpointConventionBuilder ConfigureDesignConversionModelServer(
        this IEndpointRouteBuilder endpoints,
        DesignConversionLocalServiceComposition composition,
        IDesignConversionImprovementCandidateStrategy candidateStrategy,
        DesignConversionModelServerPromotionHostOptions promotionHostOptions,
        Func<DateTimeOffset>? utcNow = null,
        DesignConversionPromotedLocalModel? promotedLocalModel = null,
        string? activationStateRootDirectory = null,
        string? modelLifecycleAuthToken = null,
        DesignConversionModelServerModelLifecycleTokenKeyRingOptions? modelLifecycleTokenKeyRing = null,
        string? humanApprovalStateRootDirectory = null,
        IDesignConversionModelServerArtifactRefResolver? artifactRefResolver = null,
        DesignConversionModelServerRouteAdmissionOptions? routeAdmissionOptions = null,
        IDesignConversionFlow38QuarantineStore? flow38QuarantineStore = null,
        string? flow38QuarantineBearerToken = null,
        DesignConversionModelServerMeasurementGateThresholdConfiguration?
            measurementGateThresholdConfiguration = null)
        => ConfigureDesignConversionModelServer(
            endpoints,
            composition,
            candidateStrategy,
            promotionHostOptions,
            DesignConversionTrainingIntakePreflightCoordinatorOptions.Disabled,
            utcNow,
            promotedLocalModel,
            activationStateRootDirectory,
            modelLifecycleAuthToken,
            modelLifecycleTokenKeyRing,
            humanApprovalStateRootDirectory,
            artifactRefResolver,
            routeAdmissionOptions,
            flow38QuarantineStore,
            flow38QuarantineBearerToken,
            measurementGateThresholdConfiguration);

    public static IEndpointConventionBuilder ConfigureDesignConversionModelServer(
        this IEndpointRouteBuilder endpoints,
        DesignConversionLocalServiceComposition composition,
        IDesignConversionImprovementCandidateStrategy candidateStrategy,
        DesignConversionModelServerPromotionHostOptions promotionHostOptions,
        DesignConversionTrainingIntakePreflightCoordinatorOptions trainingIntakePreflightOptions,
        Func<DateTimeOffset>? utcNow = null,
        DesignConversionPromotedLocalModel? promotedLocalModel = null,
        string? activationStateRootDirectory = null,
        string? modelLifecycleAuthToken = null,
        DesignConversionModelServerModelLifecycleTokenKeyRingOptions? modelLifecycleTokenKeyRing = null,
        string? humanApprovalStateRootDirectory = null,
        IDesignConversionModelServerArtifactRefResolver? artifactRefResolver = null,
        DesignConversionModelServerRouteAdmissionOptions? routeAdmissionOptions = null,
        IDesignConversionFlow38QuarantineStore? flow38QuarantineStore = null,
        string? flow38QuarantineBearerToken = null,
        DesignConversionModelServerMeasurementGateThresholdConfiguration?
            measurementGateThresholdConfiguration = null)
    {
        var startup = CreatePromotionHostRouteRegistrySetup(
            composition,
            candidateStrategy,
            promotionHostOptions,
            utcNow,
            promotedLocalModel,
            activationStateRootDirectory,
            modelLifecycleAuthToken,
            modelLifecycleTokenKeyRing,
            humanApprovalStateRootDirectory,
            artifactRefResolver,
            routeAdmissionOptions,
            flow38QuarantineStore: flow38QuarantineStore,
            flow38QuarantineBearerToken: flow38QuarantineBearerToken,
            measurementGateThresholdConfiguration: measurementGateThresholdConfiguration,
            trainingIntakePreflightOptions: trainingIntakePreflightOptions);
        return ConfigureDesignConversionModelServerCore(
            endpoints,
            startup.Registry,
            startup.PromotionHostSetup);
    }

    private static DesignConversionModelServerAspNetHostStartupMetadata CreateStartupMetadata(
        string mappedRoute,
        IReadOnlyList<string> mappedRoutes,
        PromotionHostArtifactDbSetup? promotionHostSetup)
    {
        var externalProviderConfigured = promotionHostSetup?.RemotePromotionConfigured == true &&
            string.Equals(
                mappedRoute,
                DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointPath,
                StringComparison.Ordinal);

        return new DesignConversionModelServerAspNetHostStartupMetadata(
            StartupSurface: HostStartupSurface,
            Boundary: HostStartupBoundary,
            MappedRoute: mappedRoute,
            MappedRoutes: mappedRoutes.ToArray(),
            LocalOnly: !externalProviderConfigured,
            RefsOnly: true,
            PublicDeploymentVerified: false,
            CrossRepoCallerVerified: false,
            ExternalProviderConfigured: externalProviderConfigured,
            ProductReady: false);
    }

    private static IReadOnlyList<string> FullLocalRouteSet(
        DesignConversionModelServerArtifactUploadHttpHandler? artifactUploadHandler)
        => artifactUploadHandler is null
            ? FullLocalRouteSet()
            :
            [
                ..FullLocalRouteSet(),
                DesignConversionModelServerArtifactUploadHttpHandler.EndpointPath
            ];

    public static IReadOnlyList<string> FullLocalRouteSet()
        =>
        [
            DesignConversionModelServerEndpointRegistry.EndpointPath,
            DesignConversionModelServerEndpointRegistry.InternalEndpointPath,
            DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath,
            DesignConversionModelServerEndpointRegistry.VisualCostReadbackEndpointPath,
            DesignConversionModelServerEndpointRegistry.StatusReadbackEndpointPath,
            DesignConversionModelServerEndpointRegistry.DpoUsefulnessEndpointPath,
            DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointPath,
            DesignConversionModelServerEndpointRegistry.Flow38QuarantineEndpointPath,
            DesignConversionModelServerEndpointRegistry.ModelActivationEndpointPath,
            DesignConversionModelServerEndpointRegistry.HumanApprovalChallengeEndpointPath,
            DesignConversionModelServerEndpointRegistry.HumanApprovalApproveEndpointPath
        ];

    public static IReadOnlyList<string> NonTrainingLocalRouteSet()
        =>
        [
            DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath,
            DesignConversionModelServerEndpointRegistry.Flow38QuarantineEndpointPath
        ];

    private static IReadOnlyList<string> NonDurableLocalRouteSet(
        DesignConversionModelServerArtifactUploadHttpHandler? artifactUploadHandler)
        => artifactUploadHandler is null
            ? NonDurableLocalRouteSet()
            :
            [
                ..NonDurableLocalRouteSet(),
                DesignConversionModelServerArtifactUploadHttpHandler.EndpointPath
            ];

    public static IReadOnlyList<string> NonDurableLocalRouteSet()
        =>
        [
            DesignConversionModelServerEndpointRegistry.EndpointPath,
            DesignConversionModelServerEndpointRegistry.InternalEndpointPath,
            DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath,
            DesignConversionModelServerEndpointRegistry.VisualCostReadbackEndpointPath,
            DesignConversionModelServerEndpointRegistry.StatusReadbackEndpointPath,
            DesignConversionModelServerEndpointRegistry.Flow38QuarantineEndpointPath,
            DesignConversionModelServerEndpointRegistry.ModelActivationEndpointPath,
            DesignConversionModelServerEndpointRegistry.HumanApprovalChallengeEndpointPath,
            DesignConversionModelServerEndpointRegistry.HumanApprovalApproveEndpointPath
        ];

    private static PromotionHostRouteRegistrySetup CreatePromotionHostRouteRegistrySetup(
        DesignConversionLocalServiceComposition composition,
        IDesignConversionImprovementCandidateStrategy candidateStrategy,
        DesignConversionModelServerPromotionHostOptions promotionHostOptions,
        Func<DateTimeOffset>? utcNow,
        DesignConversionPromotedLocalModel? promotedLocalModel,
        string? activationStateRootDirectory,
        string? modelLifecycleAuthToken,
        DesignConversionModelServerModelLifecycleTokenKeyRingOptions? modelLifecycleTokenKeyRing,
        string? humanApprovalStateRootDirectory,
        IDesignConversionModelServerArtifactRefResolver? artifactRefResolver,
        DesignConversionModelServerRouteAdmissionOptions? routeAdmissionOptions,
        IDesignConversionFlow38QuarantineStore? flow38QuarantineStore,
        string? flow38QuarantineBearerToken,
        DesignConversionModelServerMeasurementGateThresholdConfiguration?
            measurementGateThresholdConfiguration = null,
        DesignConversionTrainingIntakePreflightCoordinatorOptions?
            trainingIntakePreflightOptions = null)
    {
        if (composition is null)
            throw new ArgumentNullException(nameof(composition));
        DesignConversionModelServerEndpointRegistry
            .ThrowIfServerOwnedLocalCompositionAuthorityMissing(composition);
        if (promotionHostOptions is null)
            throw new ArgumentNullException(nameof(promotionHostOptions));

        var legalTeacherAdmissionAuthority =
            DesignConversionRetainedDpoPolicyBaseActivation
                .ResolveLegalTeacherAdmissionAuthority(
                    promotionHostOptions.DpoPolicyBaseProvider);
        var promotionSetup = CreatePromotionHostArtifactDbSetup(
            promotionHostOptions,
            routeAdmissionOptions,
            legalTeacherAdmissionAuthority);
        var registry = CreateLocalRouteRegistryCore(
            composition,
            candidateStrategy,
            promotionSetup.ArtifactDb,
            utcNow,
            promotedLocalModel,
            activationStateRootDirectory,
            modelLifecycleAuthToken,
            modelLifecycleTokenKeyRing,
            humanApprovalStateRootDirectory,
            artifactRefResolver,
            routeAdmissionOptions,
            promotionHostSetup: promotionSetup,
            flow38QuarantineStore: flow38QuarantineStore,
            flow38QuarantineBearerToken: flow38QuarantineBearerToken,
            measurementGateThresholdConfiguration: measurementGateThresholdConfiguration,
            trainingIntakePreflightOptions: trainingIntakePreflightOptions);
        return new PromotionHostRouteRegistrySetup(registry, promotionSetup);
    }

    private static PromotionHostArtifactDbSetup CreatePromotionHostArtifactDbSetup(
        DesignConversionModelServerPromotionHostOptions options,
        DesignConversionModelServerRouteAdmissionOptions? routeAdmissionOptions,
        ServerOwnedLegalTrainingTeacherAdmissionAuthority?
            legalTeacherAdmissionAuthority)
    {
        var artifactRoot = NormalizeAndCreatePromotionArtifactRoot(options.ArtifactRootDirectory);
        var hasHttpClient = options.PromotionHttpClient is not null;
        var hasUploaderOptions = options.PromotionHttpUploaderOptions is not null;
        var hasHttpConfig = hasHttpClient || hasUploaderOptions;
        var hasRepositoryConfig = options.RepositoryPromotionOptions is not null;
        if (!hasHttpConfig && !hasRepositoryConfig)
        {
            return new PromotionHostArtifactDbSetup(
                new JsonlDesignConversionLocalArtifactDb(
                    artifactRoot,
                    legalTeacherAdmissionAuthority,
                    dpoPolicyBaseProvider: options.DpoPolicyBaseProvider,
                    // Startup обязан передать explicit pair-store root в тот же
                    // конструктор продуктовой базы, не обходя resolver тестовой инъекцией.
                    trainingIntakeDpoPairStoreOptions:
                        options.TrainingIntakeDpoPairStoreOptions),
                RemotePromotionConfigured: false,
                ExpectedTenantId: null,
                ActivatedModelCheckpointLoader: null,
                RepositoryOwnedTrainingOptions:
                    options.RepositoryOwnedTrainingOptions);
        }

        if (hasHttpConfig && hasRepositoryConfig)
        {
            throw PromotionHostConfigurationException(
                "DESIGN_CONVERSION_PROMOTION_HOST_REMOTE_MODE_EXCLUSIVE_REQUIRED");
        }

        if (hasHttpClient != hasUploaderOptions)
        {
            throw PromotionHostConfigurationException(
                "DESIGN_CONVERSION_PROMOTION_HOST_REMOTE_CONFIG_ATOMIC_REQUIRED");
        }

        if (hasRepositoryConfig)
        {
            var repositoryConfig = ValidateRepositoryPromotionHostConfiguration(
                options.RepositoryPromotionOptions!,
                routeAdmissionOptions);
            var repositoryUploader = new DesignConversionModelRepositoryPromotionUploader(
                repositoryConfig.Repository,
                repositoryConfig.Locator,
                repositoryConfig.ServerBaseUrl);
            var repositoryUploadAuthority =
                TryIssueServerOwnedConfiguredRemotePromotionUploadAuthority(
                    repositoryUploader,
                    legalTeacherAdmissionAuthority,
                    repositoryConfig.TenantId,
                    routeAdmissionOptions);
            var repositoryCheckpointLoader = new DesignConversionRepositoryActivatedModelCheckpointLoader(
                repositoryConfig.Repository,
                repositoryConfig.Locator,
                Path.Combine(artifactRoot, "repository-promotion-active-checkpoint-cache"));
            return new PromotionHostArtifactDbSetup(
                new JsonlDesignConversionLocalArtifactDb(
                    artifactRoot,
                    legalTeacherAdmissionAuthority,
                    dpoPolicyBaseProvider: options.DpoPolicyBaseProvider,
                    promotionUploader: repositoryUploader,
                    localReceiptOnlyPromotionAuthority: null,
                    serverOwnedConfiguredRemotePromotionAuthority:
                        repositoryUploadAuthority,
                    // Продвижение через repository не меняет storage boundary: DPO пары
                    // всё равно выбирают native WSL root через общий resolver.
                    trainingIntakeDpoPairStoreOptions:
                        options.TrainingIntakeDpoPairStoreOptions),
                RemotePromotionConfigured: true,
                ExpectedTenantId: repositoryConfig.TenantId,
                ActivatedModelCheckpointLoader: repositoryCheckpointLoader,
                RepositoryOwnedTrainingOptions:
                    options.RepositoryOwnedTrainingOptions);
        }

        var remoteConfig = ValidateRemotePromotionHostConfiguration(
            artifactRoot,
            options.PromotionHttpUploaderOptions!,
            routeAdmissionOptions);
        var uploaderOptions = options.PromotionHttpUploaderOptions! with
        {
            TenantId = remoteConfig.TenantId,
            AllowedArtifactRoot = artifactRoot
        };
        var uploader = new DesignConversionModelServerPromotionHttpUploader(
            options.PromotionHttpClient!,
            uploaderOptions);
        var uploadAuthority =
            TryIssueServerOwnedConfiguredRemotePromotionUploadAuthority(
                uploader,
                legalTeacherAdmissionAuthority,
                remoteConfig.TenantId,
                routeAdmissionOptions);
        return new PromotionHostArtifactDbSetup(
            new JsonlDesignConversionLocalArtifactDb(
                artifactRoot,
                legalTeacherAdmissionAuthority,
                dpoPolicyBaseProvider: options.DpoPolicyBaseProvider,
                promotionUploader: uploader,
                localReceiptOnlyPromotionAuthority: null,
                serverOwnedConfiguredRemotePromotionAuthority:
                    uploadAuthority,
                // Удалённый uploader не должен возвращать пары на `/mnt/c`; explicit
                // override остаётся приватной настройкой локального artifact DB.
                trainingIntakeDpoPairStoreOptions:
                    options.TrainingIntakeDpoPairStoreOptions),
            RemotePromotionConfigured: true,
            ExpectedTenantId: remoteConfig.TenantId,
            ActivatedModelCheckpointLoader: null,
            RepositoryOwnedTrainingOptions:
                options.RepositoryOwnedTrainingOptions);
    }

    private static DesignConversionServerOwnedConfiguredRemotePromotionUploadAuthority?
        TryIssueServerOwnedConfiguredRemotePromotionUploadAuthority(
            IDesignConversionPromotionUploader uploader,
            ServerOwnedLegalTrainingTeacherAdmissionAuthority?
                legalTeacherAdmissionAuthority,
            string tenantId,
            DesignConversionModelServerRouteAdmissionOptions? routeAdmissionOptions)
    {
        if (legalTeacherAdmissionAuthority is null ||
            !legalTeacherAdmissionAuthority.IsAvailable ||
            routeAdmissionOptions is not { Enabled: true })
        {
            return null;
        }

        var routeTenant = DesignConversionModelActivationRegistry
            .TryBuildTenantIdentity(routeAdmissionOptions.TenantId);
        if (!routeTenant.IsSuccess ||
            !string.Equals(routeTenant.Value!.TenantId, tenantId, StringComparison.Ordinal))
        {
            return null;
        }

        var routeBindingFingerprintSha256 =
            BuildConfiguredRemotePromotionRouteBindingFingerprintSha256(
                tenantId,
                routeAdmissionOptions.RouteScope ?? string.Empty);
        var issued =
            DesignConversionServerOwnedConfiguredRemotePromotionUploadAuthority
                .Issue(
                    uploader,
                    legalTeacherAdmissionAuthority,
                    routeBindingFingerprintSha256,
                    "DESIGN_CONVERSION_MODEL_SERVER_CONFIGURED_REMOTE_PROMOTION_AUTHORITY_ISSUED");
        if (!issued.IsSuccess)
        {
            throw PromotionHostConfigurationException(
                issued.ErrorCode ??
                "DESIGN_CONVERSION_SERVER_OWNED_CONFIGURED_REMOTE_UPLOAD_AUTHORITY_INVALID");
        }

        return issued.Value;
    }

    private static string BuildConfiguredRemotePromotionRouteBindingFingerprintSha256(
        string tenantId,
        string routeScope)
        => "sha256:" + Convert.ToHexString(
                System.Security.Cryptography.SHA256.HashData(
                    System.Text.Encoding.UTF8.GetBytes(string.Join("|",
                    [
                        "design-conversion-model-server-configured-remote-promotion-route-binding-v1",
                        tenantId,
                        routeScope
                    ]))))
            .ToLowerInvariant();

    private static ValidatedRepositoryPromotionHostConfiguration ValidateRepositoryPromotionHostConfiguration(
        DesignConversionModelServerRepositoryPromotionOptions options,
        DesignConversionModelServerRouteAdmissionOptions? routeAdmissionOptions)
    {
        if (options.Repository is null || options.Locator is null)
        {
            throw PromotionHostConfigurationException(
                "DESIGN_CONVERSION_PROMOTION_HOST_REPOSITORY_CONFIG_ATOMIC_REQUIRED");
        }

        var serverBaseUrl = ValidateRepositoryPromotionServerBaseUrl(options.ServerBaseUrl);
        var tenant = DesignConversionModelActivationRegistry.TryBuildTenantIdentity(options.TenantId);
        if (!tenant.IsSuccess)
        {
            throw PromotionHostConfigurationException(
                "DESIGN_CONVERSION_PROMOTION_HOST_TENANT_INVALID");
        }

        if (routeAdmissionOptions is { Enabled: true })
        {
            var routeTenant = DesignConversionModelActivationRegistry.TryBuildTenantIdentity(routeAdmissionOptions.TenantId);
            if (!routeTenant.IsSuccess)
            {
                throw PromotionHostConfigurationException(
                    "DESIGN_CONVERSION_PROMOTION_HOST_ROUTE_ADMISSION_TENANT_INVALID");
            }

            if (!string.Equals(routeTenant.Value!.TenantId, tenant.Value!.TenantId, StringComparison.Ordinal))
            {
                throw PromotionHostConfigurationException(
                    "DESIGN_CONVERSION_PROMOTION_HOST_ROUTE_ADMISSION_TENANT_MISMATCH");
            }
        }

        return new ValidatedRepositoryPromotionHostConfiguration(
            options.Repository,
            options.Locator,
            serverBaseUrl,
            tenant.Value!.TenantId);
    }

    private static string ValidateRepositoryPromotionServerBaseUrl(string? serverBaseUrl)
    {
        var trimmedServerBaseUrl = serverBaseUrl?.Trim() ?? string.Empty;
        if (trimmedServerBaseUrl.Length == 0 ||
            !HasSafeRawRepositoryPromotionAbsoluteUrlPath(trimmedServerBaseUrl) ||
            !Uri.TryCreate(trimmedServerBaseUrl, UriKind.Absolute, out var uri) ||
            !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) ||
            !HasSafePromotionHost(uri) ||
            !HasSafePromotionUrlPath(uri.AbsolutePath) ||
            !string.IsNullOrEmpty(uri.UserInfo) ||
            !string.IsNullOrEmpty(uri.Query) ||
            !string.IsNullOrEmpty(uri.Fragment))
        {
            throw PromotionHostConfigurationException(
                "DESIGN_CONVERSION_PROMOTION_HOST_REPOSITORY_SERVER_BASE_URL_HTTPS_REQUIRED");
        }

        return uri.AbsoluteUri.TrimEnd('/');
    }

    private static bool HasSafeRawRepositoryPromotionAbsoluteUrlPath(string absoluteUrl)
    {
        if (absoluteUrl.Any(ch => char.IsControl(ch) || char.IsWhiteSpace(ch) || ch == '\\'))
            return false;

        var schemeDelimiterIndex = absoluteUrl.IndexOf("://", StringComparison.Ordinal);
        if (schemeDelimiterIndex <= 0)
            return false;

        var authorityStartIndex = schemeDelimiterIndex + 3;
        var componentStartIndex = absoluteUrl.IndexOfAny(['/', '?', '#'], authorityStartIndex);
        if (componentStartIndex < 0 || absoluteUrl[componentStartIndex] != '/')
            return HasSafePromotionUrlPath("/");

        var pathEndIndex = absoluteUrl.IndexOfAny(['?', '#'], componentStartIndex);
        var rawPath = pathEndIndex < 0
            ? absoluteUrl[componentStartIndex..]
            : absoluteUrl[componentStartIndex..pathEndIndex];
        return HasSafePromotionUrlPath(rawPath);
    }

    private static bool HasSafePromotionHost(Uri uri)
    {
        var host = uri.IdnHost;
        if (string.IsNullOrWhiteSpace(host) || host.Length > 253)
            return false;

        return uri.HostNameType switch
        {
            UriHostNameType.Dns => host
                .Split('.', StringSplitOptions.None)
                .All(label => label.Length is > 0 and <= 63 &&
                              char.IsAsciiLetterOrDigit(label[0]) &&
                              char.IsAsciiLetterOrDigit(label[^1]) &&
                              label.All(ch => char.IsAsciiLetterOrDigit(ch) || ch == '-')),
            UriHostNameType.IPv4 => host.All(ch => char.IsAsciiDigit(ch) || ch == '.'),
            UriHostNameType.IPv6 => host.All(ch => char.IsAsciiHexDigit(ch) || ch is ':' or '.'),
            _ => false
        };
    }

    private static bool HasSafePromotionUrlPath(string rawPath)
    {
        const int maxDecodeLayers = 8;
        var currentLayer = string.IsNullOrEmpty(rawPath) ? "/" : rawPath;

        for (var decodeCount = 0; ; decodeCount++)
        {
            if (!TryValidatePromotionUrlPathLayer(currentLayer, out var hasPercentEscapes))
                return false;
            if (!hasPercentEscapes)
                return true;
            if (decodeCount >= maxDecodeLayers)
                return false;

            string decodedLayer;
            try
            {
                decodedLayer = Uri.UnescapeDataString(currentLayer);
            }
            catch (UriFormatException)
            {
                return false;
            }

            if (string.Equals(decodedLayer, currentLayer, StringComparison.Ordinal))
                return false;

            currentLayer = decodedLayer;
        }
    }

    private static bool TryValidatePromotionUrlPathLayer(string path, out bool hasPercentEscapes)
    {
        hasPercentEscapes = false;
        if (path.Any(ch => char.IsControl(ch) || char.IsWhiteSpace(ch) || ch == '\\') ||
            path.Split('/', StringSplitOptions.None).Any(segment => segment is "." or ".."))
        {
            return false;
        }

        for (var index = 0; index < path.Length; index++)
        {
            if (path[index] != '%')
                continue;

            hasPercentEscapes = true;
            if (index > path.Length - 3)
                return false;

            var high = HexDigitValue(path[index + 1]);
            var low = HexDigitValue(path[index + 2]);
            if (high < 0 || low < 0)
                return false;

            var decodedByte = (high << 4) | low;
            if (decodedByte <= 0x20 ||
                decodedByte == 0x7f ||
                decodedByte is 0x2f or 0x5c)
            {
                return false;
            }

            index += 2;
        }

        return true;
    }

    private static int HexDigitValue(char value)
        => value switch
        {
            >= '0' and <= '9' => value - '0',
            >= 'a' and <= 'f' => value - 'a' + 10,
            >= 'A' and <= 'F' => value - 'A' + 10,
            _ => -1
        };

    private static ValidatedRemotePromotionHostConfiguration ValidateRemotePromotionHostConfiguration(
        string artifactRoot,
        DesignConversionModelServerPromotionHttpUploaderOptions options,
        DesignConversionModelServerRouteAdmissionOptions? routeAdmissionOptions)
    {
        if (options.Endpoint is null ||
            !options.Endpoint.IsAbsoluteUri ||
            !string.Equals(options.Endpoint.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) ||
            !string.IsNullOrEmpty(options.Endpoint.UserInfo) ||
            !string.IsNullOrEmpty(options.Endpoint.Query) ||
            !string.IsNullOrEmpty(options.Endpoint.Fragment))
        {
            throw PromotionHostConfigurationException(
                "DESIGN_CONVERSION_PROMOTION_HOST_ENDPOINT_HTTPS_REQUIRED");
        }

        var tenant = DesignConversionModelActivationRegistry.TryBuildTenantIdentity(options.TenantId);
        if (!tenant.IsSuccess)
        {
            throw PromotionHostConfigurationException(
                "DESIGN_CONVERSION_PROMOTION_HOST_TENANT_INVALID");
        }

        if (string.IsNullOrWhiteSpace(options.BearerToken) ||
            options.BearerToken.Any(char.IsControl) ||
            options.BearerToken.Any(char.IsWhiteSpace))
        {
            throw PromotionHostConfigurationException(
                "DESIGN_CONVERSION_PROMOTION_HOST_AUTH_INVALID");
        }

        if (string.IsNullOrWhiteSpace(options.AllowedArtifactRoot) ||
            !PathEquals(artifactRoot, options.AllowedArtifactRoot))
        {
            throw PromotionHostConfigurationException(
                "DESIGN_CONVERSION_PROMOTION_HOST_ARTIFACT_ROOT_MISMATCH");
        }

        if (options.Timeout <= TimeSpan.Zero ||
            options.Timeout == Timeout.InfiniteTimeSpan ||
            options.Timeout > TimeSpan.FromMinutes(5))
        {
            throw PromotionHostConfigurationException(
                "DESIGN_CONVERSION_PROMOTION_HOST_TIMEOUT_INVALID");
        }

        if (options.MaxResponseBodyBytes is <= 0 or > 1024L * 1024L ||
            options.MaxArtifactCount != DesignConversionModelServerPromotionHttpUploaderOptions.HardMaxArtifactCount ||
            options.MaxArtifactBytes <= 0 ||
            options.MaxArtifactBytes > DesignConversionModelServerPromotionHttpUploaderOptions.HardMaxArtifactBytes ||
            options.MaxTotalArtifactBytes <= 0 ||
            options.MaxTotalArtifactBytes > DesignConversionModelServerPromotionHttpUploaderOptions.HardMaxTotalArtifactBytes ||
            options.MaxTotalArtifactBytes < options.MaxArtifactBytes)
        {
            throw PromotionHostConfigurationException(
                "DESIGN_CONVERSION_PROMOTION_HOST_LIMITS_INVALID");
        }

        if (routeAdmissionOptions is { Enabled: true })
        {
            var routeTenant = DesignConversionModelActivationRegistry.TryBuildTenantIdentity(routeAdmissionOptions.TenantId);
            if (!routeTenant.IsSuccess)
            {
                throw PromotionHostConfigurationException(
                    "DESIGN_CONVERSION_PROMOTION_HOST_ROUTE_ADMISSION_TENANT_INVALID");
            }

            if (!string.Equals(routeTenant.Value!.TenantId, tenant.Value!.TenantId, StringComparison.Ordinal))
            {
                throw PromotionHostConfigurationException(
                    "DESIGN_CONVERSION_PROMOTION_HOST_ROUTE_ADMISSION_TENANT_MISMATCH");
            }
        }

        return new ValidatedRemotePromotionHostConfiguration(tenant.Value!.TenantId);
    }

    private static string NormalizeAndCreatePromotionArtifactRoot(string? artifactRootDirectory)
    {
        if (string.IsNullOrWhiteSpace(artifactRootDirectory))
        {
            throw PromotionHostConfigurationException(
                "DESIGN_CONVERSION_PROMOTION_HOST_ARTIFACT_ROOT_REQUIRED");
        }

        string fullRoot;
        try
        {
            fullRoot = Path.GetFullPath(artifactRootDirectory);
            Directory.CreateDirectory(fullRoot);
        }
        catch (Exception ex) when (ex is ArgumentException or IOException or UnauthorizedAccessException or NotSupportedException)
        {
            throw PromotionHostConfigurationException(
                "DESIGN_CONVERSION_PROMOTION_HOST_ARTIFACT_ROOT_INVALID");
        }

        try
        {
            if ((File.GetAttributes(fullRoot) & FileAttributes.ReparsePoint) != 0)
            {
                throw PromotionHostConfigurationException(
                    "DESIGN_CONVERSION_PROMOTION_HOST_ARTIFACT_ROOT_REPARSE");
            }
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException or IOException or UnauthorizedAccessException or NotSupportedException)
        {
            throw PromotionHostConfigurationException(
                "DESIGN_CONVERSION_PROMOTION_HOST_ARTIFACT_ROOT_INVALID");
        }

        return fullRoot;
    }

    private static bool PathEquals(string left, string right)
    {
        string normalizedLeft;
        string normalizedRight;
        try
        {
            normalizedLeft = EnsureTrailingDirectorySeparator(Path.GetFullPath(left));
            normalizedRight = EnsureTrailingDirectorySeparator(Path.GetFullPath(right));
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return false;
        }

        var comparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
        return string.Equals(normalizedLeft, normalizedRight, comparison);
    }

    private static string EnsureTrailingDirectorySeparator(string value)
        => value.EndsWith(Path.DirectorySeparatorChar) || value.EndsWith(Path.AltDirectorySeparatorChar)
            ? value
            : value + Path.DirectorySeparatorChar;

    private static InvalidOperationException PromotionHostConfigurationException(string errorCode)
        => new(
            "DesignConversion remote promotion host configuration failed closed before route serving: " +
            errorCode);

    private static HumanApprovalSetup CreateHumanApprovalSetup(
        string? humanApprovalStateRootDirectory)
    {
        if (string.IsNullOrWhiteSpace(humanApprovalStateRootDirectory))
        {
            return HumanApprovalSetup.NotReady(
                "DESIGN_CONVERSION_HUMAN_APPROVAL_STATE_ROOT_NOT_CONFIGURED",
                "human_approval_state_root_missing");
        }

        try
        {
            var fullRoot = Path.GetFullPath(humanApprovalStateRootDirectory);
            Directory.CreateDirectory(fullRoot);
            var journalPath = Path.Combine(fullRoot, "human-approval-events.jsonl");
            var store = new JsonlDesignConversionHumanApprovalStore(journalPath);
            return HumanApprovalSetup.Ready(store);
        }
        catch (Exception ex) when (ex is ArgumentException or IOException or UnauthorizedAccessException or NotSupportedException)
        {
            return HumanApprovalSetup.NotReady(
                "DESIGN_CONVERSION_HUMAN_APPROVAL_STATE_ROOT_INVALID",
                ex.GetType().Name);
        }
    }

    private static ModelLifecycleTokenKeyRingSetup CreateModelLifecycleTokenKeyRingSetup(
        string? modelLifecycleAuthToken,
        DesignConversionModelServerModelLifecycleTokenKeyRingOptions? modelLifecycleTokenKeyRing,
        Func<DateTimeOffset>? utcNow)
    {
        if (modelLifecycleTokenKeyRing is null &&
            string.IsNullOrWhiteSpace(modelLifecycleAuthToken))
        {
            return ModelLifecycleTokenKeyRingSetup.NotReady(
                "DESIGN_CONVERSION_MODEL_ACTIVATION_AUTH_TOKEN_NOT_CONFIGURED",
                "activation_auth_token_missing");
        }

        var tokenKeyRing = DesignConversionModelServerModelLifecycleTokenKeyRing.Create(
            modelLifecycleTokenKeyRing ??
            new DesignConversionModelServerModelLifecycleTokenKeyRingOptions(modelLifecycleAuthToken),
            utcNow);
        if (!tokenKeyRing.IsSuccess)
        {
            return ModelLifecycleTokenKeyRingSetup.NotReady(
                tokenKeyRing.ErrorCode ?? "DESIGN_CONVERSION_MODEL_ACTIVATION_AUTH_TOKEN_KEY_RING_INVALID",
                StartupAuthTokenFailureReason(tokenKeyRing.ErrorCode));
        }

        return ModelLifecycleTokenKeyRingSetup.Ready(tokenKeyRing.Value!);
    }

    private static ModelActivationSetup CreateModelActivationSetup(
        string? activationStateRootDirectory,
        ModelLifecycleTokenKeyRingSetup lifecycleTokenKeyRingSetup,
        Func<DateTimeOffset>? utcNow,
        IDesignConversionLocalPromotionActivationAuthority? localPromotionAuthority)
    {
        if (string.IsNullOrWhiteSpace(activationStateRootDirectory))
        {
            return ModelActivationSetup.NotReady(
                "DESIGN_CONVERSION_MODEL_ACTIVATION_STATE_ROOT_NOT_CONFIGURED",
                "activation_state_root_missing");
        }

        if (lifecycleTokenKeyRingSetup.TokenKeyRing is null)
        {
            return ModelActivationSetup.NotReady(
                lifecycleTokenKeyRingSetup.ErrorCode ??
                    "DESIGN_CONVERSION_MODEL_ACTIVATION_AUTH_TOKEN_KEY_RING_INVALID",
                lifecycleTokenKeyRingSetup.Reason ?? "activation_auth_token_key_ring_invalid");
        }

        try
        {
            var fullRoot = Path.GetFullPath(activationStateRootDirectory);
            Directory.CreateDirectory(fullRoot);
            var registry = new DesignConversionModelActivationRegistry(
                fullRoot,
                utcNow,
                allowedArtifactRootDirectory: localPromotionAuthority?.AllowedArtifactRootDirectory);
            var api = new DesignConversionModelServerModelActivationApi(
                registry,
                lifecycleTokenKeyRingSetup.TokenKeyRing,
                localPromotionAuthority);
            return new ModelActivationSetup(
                registry,
                new DesignConversionModelServerModelActivationHandler(api));
        }
        catch (Exception ex)
        {
            return ModelActivationSetup.NotReady(
                "DESIGN_CONVERSION_MODEL_ACTIVATION_STATE_ROOT_INVALID",
                ex.GetType().Name);
        }
    }

    private static string StartupAuthTokenFailureReason(string? errorCode)
        => errorCode switch
        {
            "DESIGN_CONVERSION_MODEL_ACTIVATION_AUTH_TOKEN_CURRENT_REQUIRED" =>
                "activation_auth_token_current_missing",
            "DESIGN_CONVERSION_MODEL_ACTIVATION_AUTH_TOKEN_CURRENT_INVALID" =>
                "activation_auth_token_current_invalid",
            "DESIGN_CONVERSION_MODEL_ACTIVATION_AUTH_TOKEN_PREVIOUS_REQUIRED" =>
                "activation_auth_token_previous_missing",
            "DESIGN_CONVERSION_MODEL_ACTIVATION_AUTH_TOKEN_PREVIOUS_INVALID" =>
                "activation_auth_token_previous_invalid",
            "DESIGN_CONVERSION_MODEL_ACTIVATION_AUTH_TOKEN_PREVIOUS_EXPIRATION_REQUIRED" =>
                "activation_auth_token_previous_expiration_missing",
            "DESIGN_CONVERSION_MODEL_ACTIVATION_AUTH_TOKEN_CURRENT_PREVIOUS_MATCH" =>
                "activation_auth_token_current_previous_match",
            _ => "activation_auth_token_key_ring_invalid"
        };

    private static void ThrowIfRegistrationFailed<T>(
        OperationResult<T> result,
        string fallbackMessage)
    {
        if (result.IsSuccess)
            return;

        throw new InvalidOperationException(result.ErrorMessage ?? fallbackMessage);
    }

    private sealed record HumanApprovalSetup(
        IDesignConversionHumanApprovalStore? Store,
        string? ErrorCode,
        string? Reason)
    {
        public static HumanApprovalSetup Ready(IDesignConversionHumanApprovalStore store)
            => new(store, null, null);

        public static HumanApprovalSetup NotReady(string errorCode, string reason)
            => new(null, errorCode, reason);

        public DesignConversionModelServerHumanApprovalHandler CreateHandler(
            DesignConversionModelServerApi api)
            => Store is null
                ? DesignConversionModelServerHumanApprovalHandler.CreateNotReady(
                    ErrorCode ?? "DESIGN_CONVERSION_HUMAN_APPROVAL_ROUTE_NOT_READY",
                    Reason ?? "human_approval_state_root_invalid")
                : new DesignConversionModelServerHumanApprovalHandler(api);
    }

    private sealed class DesignConversionModelServerCompositeEndpointConventionBuilder(
        IReadOnlyList<IEndpointConventionBuilder> builders) : IEndpointConventionBuilder
    {
        private readonly IReadOnlyList<IEndpointConventionBuilder> _builders =
            builders ?? throw new ArgumentNullException(nameof(builders));

        public void Add(Action<EndpointBuilder> convention)
        {
            foreach (var builder in _builders)
                builder.Add(convention);
        }
    }

    private sealed record ModelActivationSetup(
        DesignConversionModelActivationRegistry? Registry,
        DesignConversionModelServerModelActivationHandler Handler)
    {
        public static ModelActivationSetup NotReady(string errorCode, string reason)
            => new(
                Registry: null,
                DesignConversionModelServerModelActivationHandler.CreateNotReady(errorCode, reason));
    }

    private sealed record ModelLifecycleTokenKeyRingSetup(
        DesignConversionModelServerModelLifecycleTokenKeyRing? TokenKeyRing,
        string? ErrorCode,
        string? Reason)
    {
        public static ModelLifecycleTokenKeyRingSetup Ready(
            DesignConversionModelServerModelLifecycleTokenKeyRing tokenKeyRing)
            => new(tokenKeyRing, null, null);

        public static ModelLifecycleTokenKeyRingSetup NotReady(string errorCode, string reason)
            => new(null, errorCode, reason);
    }

    private sealed record PromotionHostRouteRegistrySetup(
        DesignConversionModelServerEndpointRegistry Registry,
        PromotionHostArtifactDbSetup PromotionHostSetup);

    private sealed record PromotionHostArtifactDbSetup(
        IDesignConversionLocalArtifactDb ArtifactDb,
        bool RemotePromotionConfigured,
        string? ExpectedTenantId,
        IDesignConversionActivatedModelCheckpointLoader? ActivatedModelCheckpointLoader,
        DesignConversionModelServerRepositoryOwnedTrainingOptions?
            RepositoryOwnedTrainingOptions);

    private sealed record ValidatedRemotePromotionHostConfiguration(string TenantId);

    private sealed record ValidatedRepositoryPromotionHostConfiguration(
        IModelRepository Repository,
        IRemoteUnitLocator Locator,
        string ServerBaseUrl,
        string TenantId);
}
