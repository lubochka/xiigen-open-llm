// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ModelServer.DesignConversion;

using AgentInfrastructure;
using DesignConversionDomain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

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

public sealed record DesignConversionModelServerPromotionHostOptions(
    string ArtifactRootDirectory,
    HttpClient? PromotionHttpClient = null,
    DesignConversionModelServerPromotionHttpUploaderOptions? PromotionHttpUploaderOptions = null)
{
    public override string ToString()
        => $"{nameof(DesignConversionModelServerPromotionHostOptions)} {{ " +
           $"{nameof(ArtifactRootDirectory)} = [REDACTED], " +
           $"{nameof(PromotionHttpClient)}Configured = {(PromotionHttpClient is not null).ToString()}, " +
           $"{nameof(PromotionHttpUploaderOptions)} = {PromotionHttpUploaderOptions?.ToString() ?? string.Empty} }}";
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

        return ConfigureDesignConversionModelServerCore(app, registry);
    }

    public static IEndpointConventionBuilder ConfigureDesignConversionModelServer(
        this IEndpointRouteBuilder endpoints,
        DesignConversionModelServerEndpointRegistry registry)
        => ConfigureDesignConversionModelServerCore(endpoints, registry);

    private static IEndpointConventionBuilder ConfigureDesignConversionModelServerCore(
        IEndpointRouteBuilder endpoints,
        DesignConversionModelServerEndpointRegistry registry,
        PromotionHostArtifactDbSetup? promotionHostSetup = null)
    {
        if (endpoints is null)
            throw new ArgumentNullException(nameof(endpoints));
        if (registry is null)
            throw new ArgumentNullException(nameof(registry));

        var mappedRoutes = FullLocalRouteSet();
        var builders = new IEndpointConventionBuilder[]
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

        return new DesignConversionModelServerCompositeEndpointConventionBuilder(builders);
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
        this IEndpointRouteBuilder endpoints,
        DesignConversionModelServerEndpointRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        ArgumentNullException.ThrowIfNull(registry);

        var mappedRoutes = NonDurableLocalRouteSet();
        var builders = new IEndpointConventionBuilder[]
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

        return new DesignConversionModelServerCompositeEndpointConventionBuilder(builders);
    }

    public static DesignConversionModelServerEndpointRegistry CreateImprovementLoopRegistry(
        DesignConversionLocalServiceComposition composition,
        IDesignConversionImprovementCandidateStrategy candidateStrategy)
    {
        if (composition is null)
            throw new ArgumentNullException(nameof(composition));
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

    public static DesignConversionModelServerEndpointRegistry CreateNonTrainingLocalRouteRegistryWithRequestScopedExternalVisualProvider(
        DesignConversionLocalServiceComposition composition,
        IDesignConversionImprovementCandidateStrategy candidateStrategy,
        HttpClient sharedHttpClient,
        IDesignConversionFlow38QuarantineStore flow38QuarantineStore,
        string? flow38QuarantineBearerToken)
    {
        ArgumentNullException.ThrowIfNull(flow38QuarantineStore);

        var registry = CreateImprovementLoopRegistryWithRequestScopedExternalVisualProvider(
            composition,
            candidateStrategy,
            sharedHttpClient);
        ThrowIfRegistrationFailed(
            registry.RegisterDesignConversionFlow38Quarantine(
                new DesignConversionModelServerFlow38QuarantineHandler(
                    flow38QuarantineStore,
                    flow38QuarantineBearerToken)),
            "DesignConversion FLOW-38 quarantine non-training request-scoped visual provider host startup registration failed.");
        return registry;
    }

    public static DesignConversionModelServerEndpointRegistry CreateImprovementLoopRegistry(
        DesignConversionLocalServiceComposition composition,
        IDesignConversionImprovementCandidateStrategy candidateStrategy,
        IDesignConversionLocalArtifactDb artifactDb,
        Func<DateTimeOffset>? utcNow = null,
        DesignConversionModelServerRouteAdmissionOptions? routeAdmissionOptions = null)
    {
        if (composition is null)
            throw new ArgumentNullException(nameof(composition));
        if (candidateStrategy is null)
            throw new ArgumentNullException(nameof(candidateStrategy));
        if (artifactDb is null)
            throw new ArgumentNullException(nameof(artifactDb));

        var registry = new DesignConversionModelServerEndpointRegistry();
        var registered = registry.RegisterDesignConversionImprovementLoopFromComposition(
            composition,
            candidateStrategy,
            artifactDb,
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

    public static DesignConversionModelServerEndpointRegistry CreateImprovementLoopRegistryWithRequestScopedExternalVisualProvider(
        DesignConversionLocalServiceComposition composition,
        IDesignConversionImprovementCandidateStrategy candidateStrategy,
        HttpClient sharedHttpClient,
        Func<DateTimeOffset>? utcNow = null,
        DesignConversionModelServerRouteAdmissionOptions? routeAdmissionOptions = null)
    {
        if (composition is null)
            throw new ArgumentNullException(nameof(composition));
        if (candidateStrategy is null)
            throw new ArgumentNullException(nameof(candidateStrategy));
        if (sharedHttpClient is null)
            throw new ArgumentNullException(nameof(sharedHttpClient));

        var registry = new DesignConversionModelServerEndpointRegistry();
        ThrowIfRegistrationFailed(
            registry.RegisterDesignConversionImprovementLoopFromCompositionWithRequestScopedExternalVisualProvider(
                composition,
                candidateStrategy,
                sharedHttpClient,
                DesignConversionModelServerRouteAdmission.Create(routeAdmissionOptions, utcNow)),
            "DesignConversion improvement-loop host startup registration with request-scoped external visual provider failed.");

        return registry;
    }

    public static DesignConversionModelServerEndpointRegistry CreateImprovementLoopRegistryWithRequestScopedExternalVisualProvider(
        DesignConversionLocalServiceComposition composition,
        IDesignConversionImprovementCandidateStrategy candidateStrategy,
        IDesignConversionLocalArtifactDb artifactDb,
        HttpClient sharedHttpClient,
        Func<DateTimeOffset>? utcNow = null,
        DesignConversionModelServerRouteAdmissionOptions? routeAdmissionOptions = null)
    {
        if (composition is null)
            throw new ArgumentNullException(nameof(composition));
        if (candidateStrategy is null)
            throw new ArgumentNullException(nameof(candidateStrategy));
        if (artifactDb is null)
            throw new ArgumentNullException(nameof(artifactDb));
        if (sharedHttpClient is null)
            throw new ArgumentNullException(nameof(sharedHttpClient));

        var registry = new DesignConversionModelServerEndpointRegistry();
        ThrowIfRegistrationFailed(
            registry.RegisterDesignConversionImprovementLoopFromCompositionWithRequestScopedExternalVisualProvider(
                composition,
                candidateStrategy,
                artifactDb,
                sharedHttpClient,
                utcNow,
                DesignConversionModelServerRouteAdmission.Create(routeAdmissionOptions, utcNow)),
            "DesignConversion improvement-loop host startup registration with local artifact DB and request-scoped external visual provider failed.");

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
        string? flow38QuarantineBearerToken = null)
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
            externalVisualProviderSharedHttpClient: null,
            visualCostLedger: visualCostLedger,
            visualCostBudgetStore: visualCostBudgetStore,
            flow38QuarantineStore: flow38QuarantineStore,
            flow38QuarantineBearerToken: flow38QuarantineBearerToken);

    public static DesignConversionModelServerEndpointRegistry CreateLocalRouteRegistryWithRequestScopedExternalVisualProvider(
        DesignConversionLocalServiceComposition composition,
        IDesignConversionImprovementCandidateStrategy candidateStrategy,
        IDesignConversionLocalArtifactDb? artifactDb,
        HttpClient sharedHttpClient,
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
        string? flow38QuarantineBearerToken = null)
    {
        if (sharedHttpClient is null)
            throw new ArgumentNullException(nameof(sharedHttpClient));

        return CreateLocalRouteRegistryCore(
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
            externalVisualProviderSharedHttpClient: sharedHttpClient,
            visualCostLedger: visualCostLedger,
            visualCostBudgetStore: visualCostBudgetStore,
            flow38QuarantineStore: flow38QuarantineStore,
            flow38QuarantineBearerToken: flow38QuarantineBearerToken);
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
        HttpClient? externalVisualProviderSharedHttpClient = null,
        IDesignConversionVisualCostLedger? visualCostLedger = null,
        IDesignConversionVisualCostBudgetStore? visualCostBudgetStore = null,
        IDesignConversionFlow38QuarantineStore? flow38QuarantineStore = null,
        string? flow38QuarantineBearerToken = null)
    {
        if (composition is null)
            throw new ArgumentNullException(nameof(composition));
        if (candidateStrategy is null)
            throw new ArgumentNullException(nameof(candidateStrategy));
        var registry = new DesignConversionModelServerEndpointRegistry();
        var dpoUsefulnessApi = artifactDb is null
            ? null
            : new DesignConversionDpoUsefulnessApi(artifactDb);
        var routeAdmission = DesignConversionModelServerRouteAdmission.Create(
            routeAdmissionOptions,
            utcNow);
        var activationSetup = CreateModelActivationSetup(
            activationStateRootDirectory,
            modelLifecycleAuthToken,
            modelLifecycleTokenKeyRing,
            utcNow,
            artifactDb as IDesignConversionLocalPromotionActivationAuthority);
        var humanApprovalSetup = CreateHumanApprovalSetup(humanApprovalStateRootDirectory);
        var effectiveVisualCostBudgetStore =
            visualCostBudgetStore ?? JsonlDesignConversionVisualCostBudgetStore.CreateDefault();
        var fallbackEvaluator = new DesignConversionInternalServiceThinClientJobEvaluator(composition.Service);
        IDesignConversionThinClientJobEvaluator evaluator = fallbackEvaluator;
        IDesignConversionActivatedModelRuntimeReadback? activatedRuntimeReadback = null;
        if (activationSetup.Registry is not null)
        {
            var activatedEvaluator = new DesignConversionActivatedModelRuntimeEvaluator(
                fallbackEvaluator,
                activationSetup.Registry,
                composition.RenderProvider,
                composition.ComparisonProvider,
                composition.VisualJudge);
            evaluator = activatedEvaluator;
            activatedRuntimeReadback = activatedEvaluator;
        }

        var modelServerApi = new DesignConversionModelServerApi(
            evaluator,
            utcNow,
            artifactDb,
            dpoUsefulnessApi,
            promotedLocalModel,
            artifactRefResolver,
            activatedModelRuntimeReadback: activatedRuntimeReadback,
            humanApprovalStore: humanApprovalSetup.Store,
            visualCostLedger: visualCostLedger,
            visualCostBudgetStore: effectiveVisualCostBudgetStore);

        ThrowIfRegistrationFailed(
            registry.RegisterDesignConversionEvaluate(
                new DesignConversionModelServerHttpHandler(modelServerApi, routeAdmission)),
            "DesignConversion evaluate full local host startup registration failed.");
        var improvementLoopRegistration = externalVisualProviderSharedHttpClient is null
            ? artifactDb is null
                ? registry.RegisterDesignConversionImprovementLoopFromComposition(
                    composition,
                    candidateStrategy,
                    routeAdmission)
                : registry.RegisterDesignConversionImprovementLoopFromComposition(
                    composition,
                    candidateStrategy,
                    artifactDb,
                    utcNow,
                    routeAdmission)
            : artifactDb is null
                ? registry.RegisterDesignConversionImprovementLoopFromCompositionWithRequestScopedExternalVisualProvider(
                    composition,
                    candidateStrategy,
                    externalVisualProviderSharedHttpClient,
                    routeAdmission)
                : registry.RegisterDesignConversionImprovementLoopFromCompositionWithRequestScopedExternalVisualProvider(
                    composition,
                    candidateStrategy,
                    artifactDb,
                    externalVisualProviderSharedHttpClient,
                    utcNow,
                    routeAdmission);
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
                ? new DesignConversionModelServerTrainingIntakeHandler(dpoUsefulnessApi)
                : DesignConversionModelServerTrainingIntakeHandler.CreateForValidatedPromotionHost(
                    dpoUsefulnessApi,
                    promotionHostSetup.ExpectedTenantId,
                    promotionHostSetup.RemotePromotionConfigured);
            ThrowIfRegistrationFailed(
                registry.RegisterDesignConversionDpoUsefulness(
                    new DesignConversionModelServerDpoUsefulnessHandler(dpoUsefulnessApi)),
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
        string? flow38QuarantineBearerToken = null)
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
            flow38QuarantineBearerToken: flow38QuarantineBearerToken)
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

    public static IEndpointConventionBuilder ConfigureDesignConversionModelServerWithRequestScopedExternalVisualProvider(
        this WebApplication app,
        DesignConversionLocalServiceComposition composition,
        IDesignConversionImprovementCandidateStrategy candidateStrategy,
        HttpClient sharedHttpClient)
        => app.ConfigureDesignConversionModelServer(
            CreateImprovementLoopRegistryWithRequestScopedExternalVisualProvider(
                composition,
                candidateStrategy,
                sharedHttpClient));

    public static IEndpointConventionBuilder ConfigureDesignConversionModelServerWithRequestScopedExternalVisualProvider(
        this IEndpointRouteBuilder endpoints,
        DesignConversionLocalServiceComposition composition,
        IDesignConversionImprovementCandidateStrategy candidateStrategy,
        HttpClient sharedHttpClient)
        => endpoints.ConfigureDesignConversionModelServer(
            CreateImprovementLoopRegistryWithRequestScopedExternalVisualProvider(
                composition,
                candidateStrategy,
                sharedHttpClient));

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
        string? flow38QuarantineBearerToken = null)
        => app.ConfigureDesignConversionModelServer(
            CreateLocalRouteRegistry(
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
                flow38QuarantineStore: flow38QuarantineStore,
                flow38QuarantineBearerToken: flow38QuarantineBearerToken));

    public static IEndpointConventionBuilder ConfigureDesignConversionModelServerWithRequestScopedExternalVisualProvider(
        this WebApplication app,
        DesignConversionLocalServiceComposition composition,
        IDesignConversionImprovementCandidateStrategy candidateStrategy,
        IDesignConversionLocalArtifactDb? artifactDb,
        HttpClient sharedHttpClient,
        Func<DateTimeOffset>? utcNow = null,
        DesignConversionPromotedLocalModel? promotedLocalModel = null,
        string? activationStateRootDirectory = null,
        string? modelLifecycleAuthToken = null,
        DesignConversionModelServerModelLifecycleTokenKeyRingOptions? modelLifecycleTokenKeyRing = null,
        string? humanApprovalStateRootDirectory = null,
        IDesignConversionModelServerArtifactRefResolver? artifactRefResolver = null,
        DesignConversionModelServerRouteAdmissionOptions? routeAdmissionOptions = null,
        IDesignConversionFlow38QuarantineStore? flow38QuarantineStore = null,
        string? flow38QuarantineBearerToken = null)
        => app.ConfigureDesignConversionModelServer(
            CreateLocalRouteRegistryWithRequestScopedExternalVisualProvider(
                composition,
                candidateStrategy,
                artifactDb,
                sharedHttpClient,
                utcNow,
                promotedLocalModel,
                activationStateRootDirectory,
                modelLifecycleAuthToken,
                modelLifecycleTokenKeyRing,
                humanApprovalStateRootDirectory,
                artifactRefResolver,
                routeAdmissionOptions,
                flow38QuarantineStore: flow38QuarantineStore,
                flow38QuarantineBearerToken: flow38QuarantineBearerToken));

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
        string? flow38QuarantineBearerToken = null)
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
            flow38QuarantineBearerToken: flow38QuarantineBearerToken);
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
        string? flow38QuarantineBearerToken = null)
        => endpoints.ConfigureDesignConversionModelServer(
            CreateLocalRouteRegistry(
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
                flow38QuarantineStore: flow38QuarantineStore,
                flow38QuarantineBearerToken: flow38QuarantineBearerToken));

    public static IEndpointConventionBuilder ConfigureDesignConversionModelServerWithRequestScopedExternalVisualProvider(
        this IEndpointRouteBuilder endpoints,
        DesignConversionLocalServiceComposition composition,
        IDesignConversionImprovementCandidateStrategy candidateStrategy,
        IDesignConversionLocalArtifactDb? artifactDb,
        HttpClient sharedHttpClient,
        Func<DateTimeOffset>? utcNow = null,
        DesignConversionPromotedLocalModel? promotedLocalModel = null,
        string? activationStateRootDirectory = null,
        string? modelLifecycleAuthToken = null,
        DesignConversionModelServerModelLifecycleTokenKeyRingOptions? modelLifecycleTokenKeyRing = null,
        string? humanApprovalStateRootDirectory = null,
        IDesignConversionModelServerArtifactRefResolver? artifactRefResolver = null,
        DesignConversionModelServerRouteAdmissionOptions? routeAdmissionOptions = null,
        IDesignConversionFlow38QuarantineStore? flow38QuarantineStore = null,
        string? flow38QuarantineBearerToken = null)
        => endpoints.ConfigureDesignConversionModelServer(
            CreateLocalRouteRegistryWithRequestScopedExternalVisualProvider(
                composition,
                candidateStrategy,
                artifactDb,
                sharedHttpClient,
                utcNow,
                promotedLocalModel,
                activationStateRootDirectory,
                modelLifecycleAuthToken,
                modelLifecycleTokenKeyRing,
                humanApprovalStateRootDirectory,
                artifactRefResolver,
                routeAdmissionOptions,
                flow38QuarantineStore: flow38QuarantineStore,
                flow38QuarantineBearerToken: flow38QuarantineBearerToken));

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
        string? flow38QuarantineBearerToken = null)
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
            flow38QuarantineBearerToken: flow38QuarantineBearerToken);
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
        string? flow38QuarantineBearerToken)
    {
        if (promotionHostOptions is null)
            throw new ArgumentNullException(nameof(promotionHostOptions));

        var promotionSetup = CreatePromotionHostArtifactDbSetup(
            promotionHostOptions,
            routeAdmissionOptions);
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
            flow38QuarantineBearerToken: flow38QuarantineBearerToken);
        return new PromotionHostRouteRegistrySetup(registry, promotionSetup);
    }

    private static PromotionHostArtifactDbSetup CreatePromotionHostArtifactDbSetup(
        DesignConversionModelServerPromotionHostOptions options,
        DesignConversionModelServerRouteAdmissionOptions? routeAdmissionOptions)
    {
        var artifactRoot = NormalizeAndCreatePromotionArtifactRoot(options.ArtifactRootDirectory);
        var hasHttpClient = options.PromotionHttpClient is not null;
        var hasUploaderOptions = options.PromotionHttpUploaderOptions is not null;
        if (!hasHttpClient && !hasUploaderOptions)
        {
            return new PromotionHostArtifactDbSetup(
                new JsonlDesignConversionLocalArtifactDb(artifactRoot),
                RemotePromotionConfigured: false,
                ExpectedTenantId: null);
        }

        if (hasHttpClient != hasUploaderOptions)
        {
            throw PromotionHostConfigurationException(
                "DESIGN_CONVERSION_PROMOTION_HOST_REMOTE_CONFIG_ATOMIC_REQUIRED");
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
        return new PromotionHostArtifactDbSetup(
            new JsonlDesignConversionLocalArtifactDb(artifactRoot, uploader),
            RemotePromotionConfigured: true,
            ExpectedTenantId: remoteConfig.TenantId);
    }

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

    private static ModelActivationSetup CreateModelActivationSetup(
        string? activationStateRootDirectory,
        string? modelLifecycleAuthToken,
        DesignConversionModelServerModelLifecycleTokenKeyRingOptions? modelLifecycleTokenKeyRing,
        Func<DateTimeOffset>? utcNow,
        IDesignConversionLocalPromotionActivationAuthority? localPromotionAuthority)
    {
        if (string.IsNullOrWhiteSpace(activationStateRootDirectory))
        {
            return ModelActivationSetup.NotReady(
                "DESIGN_CONVERSION_MODEL_ACTIVATION_STATE_ROOT_NOT_CONFIGURED",
                "activation_state_root_missing");
        }

        if (modelLifecycleTokenKeyRing is null &&
            string.IsNullOrWhiteSpace(modelLifecycleAuthToken))
        {
            return ModelActivationSetup.NotReady(
                "DESIGN_CONVERSION_MODEL_ACTIVATION_AUTH_TOKEN_NOT_CONFIGURED",
                "activation_auth_token_missing");
        }

        var tokenKeyRing = DesignConversionModelServerModelLifecycleTokenKeyRing.Create(
            modelLifecycleTokenKeyRing ??
            new DesignConversionModelServerModelLifecycleTokenKeyRingOptions(modelLifecycleAuthToken),
            utcNow);
        if (!tokenKeyRing.IsSuccess)
        {
            return ModelActivationSetup.NotReady(
                tokenKeyRing.ErrorCode ?? "DESIGN_CONVERSION_MODEL_ACTIVATION_AUTH_TOKEN_KEY_RING_INVALID",
                StartupAuthTokenFailureReason(tokenKeyRing.ErrorCode));
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
                tokenKeyRing.Value!,
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

    private sealed record PromotionHostRouteRegistrySetup(
        DesignConversionModelServerEndpointRegistry Registry,
        PromotionHostArtifactDbSetup PromotionHostSetup);

    private sealed record PromotionHostArtifactDbSetup(
        IDesignConversionLocalArtifactDb ArtifactDb,
        bool RemotePromotionConfigured,
        string? ExpectedTenantId);

    private sealed record ValidatedRemotePromotionHostConfiguration(string TenantId);
}
