// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

namespace LlmFromScratch.Tests;

public sealed class DesignConversionExternalJudgeAuthorityBoundaryArchitectureTests
{
    private static readonly string[] LiveAuthoritySurfaceFiles =
    [
        "25.ModelServer/DesignConversion/DesignConversionModelServerCallerContract.cs",
        "25.ModelServer/DesignConversion/DesignConversionModelServerImprovementLoopHandler.cs",
        "25.ModelServer/DesignConversion/DesignConversionModelServerEndpointRegistry.cs",
        "25.ModelServer/DesignConversion/DesignConversionModelServerAspNetEndpointMapper.cs",
        "25.ModelServer/DesignConversion/DesignConversionModelServerAspNetHostStartup.cs",
        "25.ModelServer/Program.cs",
        "tools/DesignConversionLiveLocalServer/Program.cs",
        "44.DesignConversionDomain/DesignConversionLocalServiceComposition.cs"
    ];

    [Fact]
    public void LiveAuthoritySurface_HasNoExternalJudgeRequestOrResolverCarrier()
    {
        var source = ReadLiveAuthoritySurface();

        Assert.DoesNotContain("new(\"externalVisualProviderConfig\"", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("DesignConversionExternalVisualProviderConfigInput?", source, StringComparison.Ordinal);
        Assert.DoesNotContain("ExternalVisualProviderConfig =", source, StringComparison.Ordinal);
        Assert.DoesNotContain("DesignConversionExternalVisualProviderCoordinatorResolver", source, StringComparison.Ordinal);
        Assert.DoesNotContain("CreateRequestScopedExternalVisualProviderCoordinatorResolver", source, StringComparison.Ordinal);
        Assert.DoesNotContain("WithRequestScopedExternalVisualProvider", source, StringComparison.Ordinal);
        Assert.DoesNotContain("TryCreateVisualJudge", source, StringComparison.Ordinal);
        Assert.DoesNotContain("new ExternalHttpDesignConversionVisualJudge", source, StringComparison.Ordinal);
        Assert.DoesNotContain("DesignConversionExternalVisualProviderConfigIntake.", source, StringComparison.Ordinal);
        Assert.DoesNotContain("DesignConversionExternalVisualJudgeNetwork", source, StringComparison.Ordinal);
        Assert.DoesNotContain("--external-visual-provider-from-env", source, StringComparison.Ordinal);
        Assert.DoesNotContain("ForLiveHostTesting", string.Join(
            "\n",
            LiveAuthoritySurfaceFiles
                .Where(path =>
                    !path.StartsWith("44.DesignConversionDomain/", StringComparison.Ordinal) &&
                    !path.EndsWith(
                        "DesignConversionModelServerEndpointRegistry.cs",
                        StringComparison.Ordinal))
                .Select(ReadLiveFile)), StringComparison.Ordinal);
        Assert.Equal(
            1,
            CountOccurrences(
                ReadLiveFile(
                    "25.ModelServer/DesignConversion/DesignConversionModelServerEndpointRegistry.cs"),
                "RegisterDesignConversionImprovementLoopForLiveHostTesting("));
    }

    public static IEnumerable<object[]> LiveAuthoritySurfaceCases()
        => LiveAuthoritySurfaceFiles.Select(relative => new object[] { relative });

    [Theory]
    [MemberData(nameof(LiveAuthoritySurfaceCases))]
    public void EachLiveAuthoritySurfaceFile_HasNoExternalJudgeFactoryOrResolver(string relativePath)
    {
        var source = ReadLiveFile(relativePath);

        Assert.DoesNotContain("DesignConversionExternalVisualProviderCoordinatorResolver", source, StringComparison.Ordinal);
        Assert.DoesNotContain("CreateRequestScopedExternalVisualProviderCoordinatorResolver", source, StringComparison.Ordinal);
        Assert.DoesNotContain("WithRequestScopedExternalVisualProvider", source, StringComparison.Ordinal);
        Assert.DoesNotContain("TryCreateVisualJudge", source, StringComparison.Ordinal);
        Assert.DoesNotContain("new ExternalHttpDesignConversionVisualJudge", source, StringComparison.Ordinal);
        Assert.DoesNotContain("DesignConversionExternalVisualJudgeNetwork", source, StringComparison.Ordinal);
    }

    [Fact]
    public void RemovedExternalFieldGuard_PrecedesDeserializationAndCoordinatorExecution()
    {
        var source = ReadLiveFile(
            "25.ModelServer/DesignConversion/DesignConversionModelServerImprovementLoopHandler.cs");
        var guard = source.IndexOf(
            "if (ContainsRemovedExternalVisualProviderConfig(bodyJson))",
            StringComparison.Ordinal);
        var deserialize = source.IndexOf(
            "JsonSerializer.Deserialize<DesignConversionModelServerImprovementLoopRequest>",
            StringComparison.Ordinal);
        var coordinatorRun = source.IndexOf("_coordinator!.RunAsync", StringComparison.Ordinal);

        Assert.True(guard >= 0, "Removed external-field guard is missing.");
        Assert.True(deserialize > guard, "Removed external-field guard must run before request deserialization.");
        Assert.True(coordinatorRun > deserialize, "Coordinator execution must remain after guarded deserialization.");
        Assert.Contains(
            "string.Concat(\"external\", \"visual\", \"provider\", \"config\")",
            source,
            StringComparison.Ordinal);
    }

    [Fact]
    public void LocalIndependentPreferenceEvaluatorOptIn_RemainsOnProductionHostPath()
    {
        var root = FindRepositoryRoot();
        var program = File.ReadAllText(Path.Combine(root, "25.ModelServer", "Program.cs"));
        var startup = File.ReadAllText(Path.Combine(
            root,
            "25.ModelServer",
            "DesignConversion",
            "DesignConversionModelServerAspNetHostStartup.cs"));

        Assert.Contains("LocalIndependentPreferenceEvaluatorRequested", program, StringComparison.Ordinal);
        Assert.Contains("CreateLocalRouteRegistryWithLocalIndependentPreferenceEvaluator", program, StringComparison.Ordinal);
        Assert.Contains("ConfigureLocalIndependentPreferenceEvaluator", startup, StringComparison.Ordinal);
    }

    [Fact]
    public void ServerOwnedLocalAuthority_IsPrivateIdentityAndGenericFactoriesCannotMintIt()
    {
        var source = ReadLiveFile(
            "44.DesignConversionDomain/DesignConversionLocalServiceComposition.cs");

        Assert.Contains(
            "private sealed class ServerOwnedLocalLiveHostAuthority",
            source,
            StringComparison.Ordinal);
        Assert.Contains(
            "private ServerOwnedLocalLiveHostAuthority()",
            source,
            StringComparison.Ordinal);
        Assert.Contains(
            "ReferenceEquals(\n            _serverOwnedLocalLiveHostAuthority,",
            source,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "public interface IServerOwnedLocalLiveHostAuthority",
            source,
            StringComparison.Ordinal);

        Assert.DoesNotContain(
            "WithServerOwnedLocalLiveHostAuthority",
            ReadMethodBody(source, "CreateBrowserBackedLocal("),
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "WithServerOwnedLocalLiveHostAuthority",
            ReadMethodBody(source, "CreateBrowserBackedLocalDefault("),
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "WithServerOwnedLocalLiveHostAuthority",
            ReadMethodBody(source, "CreateBrowserBackedMachineLocalDefault("),
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "WithServerOwnedLocalLiveHostAuthority",
            ReadMethodBody(source, "CreateDeterministicLocal("),
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "WithServerOwnedLocalLiveHostAuthority",
            ReadMethodBody(source, "CreateDeterministicLocalPng("),
            StringComparison.Ordinal);

        Assert.Contains(
            "CreateServerOwnedBrowserBackedLocalDefault",
            source,
            StringComparison.Ordinal);
        Assert.Contains(
            "CreateServerOwnedBrowserBackedMachineLocalWithDefaultJudge",
            source,
            StringComparison.Ordinal);
        Assert.Contains(
            "CreateServerOwnedBrowserBackedMachineLocalWithoutConfiguredJudge",
            source,
            StringComparison.Ordinal);
        Assert.Contains(
            "CreateServerOwnedDeterministicLocalPng",
            source,
            StringComparison.Ordinal);
        Assert.True(
            CountOccurrences(source, ".WithServerOwnedLocalLiveHostAuthority();") >= 7,
            "All four safe factories and three bounded test issuers must mint the private identity.");
        Assert.Contains(
            "new ServerOwnedLocalPngComparisonProvider(artifactRoot)",
            source,
            StringComparison.Ordinal);
        Assert.Contains(
            "new DefaultLocalDesignConversionVisualJudge()",
            source,
            StringComparison.Ordinal);
        Assert.Contains("new NoConfiguredVisualJudge()", source, StringComparison.Ordinal);
    }

    [Fact]
    public void LiveRouteCompositionGates_ArePositiveAndPrecedeConstructionOrRegistration()
    {
        var registry = ReadLiveFile(
            "25.ModelServer/DesignConversion/DesignConversionModelServerEndpointRegistry.cs");
        var host = ReadLiveFile(
            "25.ModelServer/DesignConversion/DesignConversionModelServerAspNetHostStartup.cs");
        var mapper = ReadLiveFile(
            "25.ModelServer/DesignConversion/DesignConversionModelServerAspNetEndpointMapper.cs");

        Assert.Contains(
            "DESIGN_CONVERSION_SERVER_OWNED_LOCAL_COMPOSITION_AUTHORITY_REQUIRED",
            registry,
            StringComparison.Ordinal);
        Assert.True(
            CountOccurrences(
                registry,
                "if (!composition.HasServerOwnedLocalLiveHostAuthority)") >= 3,
            "Both public registry overloads and the core resolver must enforce the positive authority.");
        Assert.True(
            registry.IndexOf(
                "if (!composition.HasServerOwnedLocalLiveHostAuthority)",
                StringComparison.Ordinal) <
            registry.IndexOf(
                "new DesignConversionImprovementLoopCoordinator(",
                StringComparison.Ordinal));
        Assert.True(
            CountOccurrences(
                host,
                ".ThrowIfServerOwnedLocalCompositionAuthorityMissing(composition);") >= 4,
            "Host entry/core paths must reject before constructing registries or evaluators.");
        var localCore = ReadMethodBody(
            host,
            "private static DesignConversionModelServerEndpointRegistry CreateLocalRouteRegistryCore(");
        Assert.True(
            localCore.IndexOf(
                ".ThrowIfServerOwnedLocalCompositionAuthorityMissing(composition);",
                StringComparison.Ordinal) <
            localCore.IndexOf(
                "new DesignConversionModelServerEndpointRegistry()",
                StringComparison.Ordinal));
        var mapperMethod = ReadMethodBody(
            mapper,
            "MapDesignConversionModelServerImprovementLoopFromComposition(");
        Assert.True(
            mapperMethod.IndexOf(
                ".ThrowIfServerOwnedLocalCompositionAuthorityMissing(composition);",
                StringComparison.Ordinal) <
            mapperMethod.IndexOf(
                "CreateImprovementLoopRegistry(",
                StringComparison.Ordinal));
        Assert.DoesNotContain(
            "BoundaryMetadata[\"server_owned_local_live_host_authority\"]",
            registry + host + mapper,
            StringComparison.Ordinal);
    }

    [Fact]
    public void DirectHandlerRegistryAndHostMapping_RequirePrivateRegistryAuthorityBeforeEffects()
    {
        var registry = ReadLiveFile(
            "25.ModelServer/DesignConversion/DesignConversionModelServerEndpointRegistry.cs");
        var host = ReadLiveFile(
            "25.ModelServer/DesignConversion/DesignConversionModelServerAspNetHostStartup.cs");
        var mapper = ReadLiveFile(
            "25.ModelServer/DesignConversion/DesignConversionModelServerAspNetEndpointMapper.cs");

        Assert.Contains(
            "private sealed class ServerOwnedLocalImprovementLoopRegistrationAuthority",
            registry,
            StringComparison.Ordinal);
        Assert.Contains(
            "private ServerOwnedLocalImprovementLoopRegistrationAuthority()",
            registry,
            StringComparison.Ordinal);
        Assert.Contains(
            "ReferenceEquals(\n            _serverOwnedLocalImprovementLoopRegistrationAuthority,",
            registry,
            StringComparison.Ordinal);

        var publicDirectRegistration = ReadMethodBody(
            registry,
            "RegisterDesignConversionImprovementLoop(");
        Assert.Contains(
            "CreateServerOwnedLocalCompositionAuthorityRequiredResult()",
            publicDirectRegistration,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "RegisterAuthorizedDesignConversionImprovementLoop",
            publicDirectRegistration,
            StringComparison.Ordinal);

        var boundedTestIssuer = ReadMethodBody(
            registry,
            "RegisterDesignConversionImprovementLoopForLiveHostTesting(");
        Assert.Contains("ThrowUnlessLiveHostTestAssembly", boundedTestIssuer, StringComparison.Ordinal);
        Assert.Contains(
            "RegisterAuthorizedDesignConversionImprovementLoop",
            boundedTestIssuer,
            StringComparison.Ordinal);

        var directMapper = ReadMethodBody(
            mapper,
            "MapDesignConversionModelServerImprovementLoop(");
        Assert.True(
            directMapper.IndexOf(
                ".ThrowIfServerOwnedLocalImprovementLoopRegistrationAuthorityMissing(registry);",
                StringComparison.Ordinal) <
            directMapper.IndexOf("return endpoints.MapPost(", StringComparison.Ordinal));

        var hostCore = ReadMethodBody(
            host,
            "private static IEndpointConventionBuilder ConfigureDesignConversionModelServerCore(");
        Assert.True(
            hostCore.IndexOf(
                ".ThrowIfServerOwnedLocalImprovementLoopRegistrationAuthorityMissing(registry);",
                StringComparison.Ordinal) <
            hostCore.IndexOf("var mappedRoutes =", StringComparison.Ordinal));
        var lifetimeRegistration = ReadMethodBody(host, "private static void RegisterRegistryLifetime(");
        Assert.True(
            lifetimeRegistration.IndexOf(
                ".ThrowIfServerOwnedLocalImprovementLoopRegistrationAuthorityMissing(registry);",
                StringComparison.Ordinal) <
            lifetimeRegistration.IndexOf(
                "app.Lifetime.ApplicationStopping.Register(registry.Dispose);",
                StringComparison.Ordinal));
        Assert.True(
            CountOccurrences(
                host,
                ".ThrowIfServerOwnedLocalImprovementLoopRegistrationAuthorityMissing(registry);") >= 4,
            "All direct host shapes that map improvement-loop routes must gate before effects.");
    }

    [Fact]
    public void ObsoleteExternalProviderTraceCarriers_AreAbsentFromLiveHandlerAndTool()
    {
        var handler = ReadLiveFile(
            "25.ModelServer/DesignConversion/DesignConversionModelServerImprovementLoopHandler.cs");
        var tool = ReadLiveFile("tools/DesignConversionLiveLocalServer/Program.cs");
        var liveSource = handler + "\n" + tool;
        var obsoleteCarriers = new[]
        {
            "external_visual_provider_request_config_supported",
            "external_visual_provider_request_config_present",
            "external_visual_provider_request_scoped_resolver_configured",
            "external_visual_provider_shared_http_client_injected",
            "external_visual_provider_http_client_owned_by_caller",
            "external_visual_provider_http_client_disposed_by_registry",
            "external_visual_provider_config_intake",
            "external_visual_provider_config_complete",
            "external_visual_provider_factory_ready",
            "Pass143RemovedExternalVisualJudgeTrace",
            "ExternalVisualProviderRequestScopedResolverConfigured",
            "ExternalVisualProviderSharedHttpClientInjected"
        };

        foreach (var obsoleteCarrier in obsoleteCarriers)
            Assert.DoesNotContain(obsoleteCarrier, liveSource, StringComparison.Ordinal);

        Assert.Contains(
            "external_visual_provider_request_config_removed",
            handler,
            StringComparison.Ordinal);
        Assert.Contains(
            "ExternalHttpDesignConversionVisualJudge.ServerCallReceiptKey",
            handler,
            StringComparison.Ordinal);
        Assert.Contains("externalJudgeCarrierPresent", tool, StringComparison.Ordinal);
        Assert.Contains("externalVisualJudgeRuntimeRemoved", tool, StringComparison.Ordinal);
        Assert.Contains("serverOwnedLocalEvaluationOnly", tool, StringComparison.Ordinal);
    }

    [Fact]
    public void ExecutableAndLiveTool_UseOnlyApprovedLocalFactoriesAndNoExternalCarrier()
    {
        var program = ReadLiveFile("25.ModelServer/Program.cs");
        var tool = ReadLiveFile("tools/DesignConversionLiveLocalServer/Program.cs");
        var compositionFactory = ReadMethodBody(
            program,
            "internal static DesignConversionLocalServiceComposition CreateLocalLoopbackServiceCompositionWithLocalPngArtifactRoot(");

        Assert.DoesNotContain(
            "DesignConversionExternalVisualAiComparisonProvider",
            program,
            StringComparison.Ordinal);
        Assert.Contains(
            "CreateServerOwnedBrowserBackedMachineLocalWithDefaultJudge",
            compositionFactory,
            StringComparison.Ordinal);
        Assert.Contains(
            "localPngArtifactRoot",
            compositionFactory,
            StringComparison.Ordinal);
        Assert.Contains(
            "var resolvedLocalPngArtifactRoot = ResolveLocalLoopbackPngArtifactRoot(",
            compositionFactory,
            StringComparison.Ordinal);
        Assert.Contains(
            "CreateRequiredLocalLoopbackBrowserAdapter(resolvedLocalPngArtifactRoot)",
            compositionFactory,
            StringComparison.Ordinal);
        Assert.Contains(
            "resolvedLocalPngArtifactRoot);",
            compositionFactory,
            StringComparison.Ordinal);
        Assert.Contains("serverOwnedLocalCompositionAuthority", program, StringComparison.Ordinal);
        Assert.Contains("serverOwnedLocalComparisonProvider", program, StringComparison.Ordinal);
        Assert.Contains("externalJudgeCarrierPresent", program, StringComparison.Ordinal);
        Assert.Contains(
            "var localPngArtifactRoot = CreateLocalLoopbackArtifactUploadRoot(",
            program,
            StringComparison.Ordinal);
        Assert.Contains(
            "localPngArtifactRoot: localPngArtifactRoot",
            program,
            StringComparison.Ordinal);
        Assert.Contains(
            "new DesignConversionServerOwnedLocalArtifactRefResolver(",
            program,
            StringComparison.Ordinal);

        Assert.DoesNotContain(
            "DesignConversionLocalServiceComposition.CreateBrowserBackedLocalDefault(",
            tool,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "DesignConversionLocalServiceComposition.CreateBrowserBackedMachineLocalDefault(",
            tool,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "DesignConversionLocalServiceComposition.CreateDeterministicLocalPng(",
            tool,
            StringComparison.Ordinal);
        Assert.Contains("CreateServerOwnedBrowserBackedLocalDefault", tool, StringComparison.Ordinal);
        Assert.Contains(
            "CreateServerOwnedBrowserBackedMachineLocalWithDefaultJudge",
            tool,
            StringComparison.Ordinal);
        Assert.Contains("CreateServerOwnedDeterministicLocalPng", tool, StringComparison.Ordinal);
    }

    private static string ReadLiveAuthoritySurface()
    {
        var root = FindRepositoryRoot();
        return string.Join(
            "\n",
            LiveAuthoritySurfaceFiles.Select(relative =>
                File.ReadAllText(Path.Combine(root, relative))));
    }

    private static string ReadLiveFile(string relativePath)
        => File.ReadAllText(Path.Combine(FindRepositoryRoot(), relativePath));

    private static string ReadMethodBody(string source, string signature)
    {
        var signatureIndex = source.IndexOf(signature, StringComparison.Ordinal);
        Assert.True(signatureIndex >= 0, $"Method signature '{signature}' was not found.");
        var openingBrace = source.IndexOf('{', signatureIndex);
        Assert.True(openingBrace >= 0, $"Method body for '{signature}' was not found.");
        var depth = 0;
        for (var index = openingBrace; index < source.Length; index++)
        {
            if (source[index] == '{')
                depth++;
            else if (source[index] == '}')
                depth--;

            if (depth == 0)
                return source[signatureIndex..(index + 1)];
        }

        throw new InvalidOperationException($"Method body for '{signature}' is unbalanced.");
    }

    private static int CountOccurrences(string source, string value)
    {
        var count = 0;
        for (var index = 0; ;)
        {
            index = source.IndexOf(value, index, StringComparison.Ordinal);
            if (index < 0)
                return count;
            count++;
            index += value.Length;
        }
    }

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "LlmFromScratch.sln")))
                return current.FullName;
            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Repository root was not found.");
    }
}
