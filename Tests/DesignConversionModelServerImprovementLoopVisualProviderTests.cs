// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using DesignConversionDomain;
using ModelServer.DesignConversion;
using Xunit;

public sealed class DesignConversionModelServerImprovementLoopVisualProviderTests
{
    [Fact]
    public void ExternalVisualProviderConfigIntake_MissingSettingsReturnsKeyNamesOnly()
    {
        var secret = "pass135-" + Guid.NewGuid().ToString("N");
        var input = DesignConversionExternalVisualProviderConfigIntake.FromSettings(
            new Dictionary<string, string>
            {
                [DesignConversionExternalVisualProviderConfigIntake.ApiKeySettingName] = secret
            });

        var descriptor = DesignConversionExternalVisualProviderConfigIntake.Describe(input);
        var options = DesignConversionExternalVisualProviderConfigIntake.TryCreateOptions(input);

        Assert.False(descriptor.IsComplete);
        Assert.False(descriptor.FactoryReady);
        Assert.False(descriptor.RealExternalProviderExecutableWhenNetworkAuthorized);
        Assert.False(descriptor.RealExternalProviderExecuted);
        Assert.False(descriptor.NetworkCallExecuted);
        Assert.Contains(
            DesignConversionExternalVisualProviderConfigIntake.ServerOwnedImageArtifactRootSettingName,
            DesignConversionExternalVisualProviderConfigIntake.RequiredSettingNames);
        Assert.Contains(DesignConversionExternalVisualProviderConfigIntake.EndpointSettingName, descriptor.MissingSettingNames);
        Assert.Contains(DesignConversionExternalVisualProviderConfigIntake.ProviderSettingName, descriptor.MissingSettingNames);
        Assert.Contains(DesignConversionExternalVisualProviderConfigIntake.ModelSettingName, descriptor.MissingSettingNames);
        Assert.Contains(DesignConversionExternalVisualProviderConfigIntake.ServerOwnedImageArtifactRootSettingName, descriptor.MissingSettingNames);
        Assert.DoesNotContain(DesignConversionExternalVisualProviderConfigIntake.ApiKeySettingName, descriptor.MissingSettingNames);
        Assert.Equal(bool.TrueString, descriptor.BoundaryMetadata["external_visual_provider_api_key_configured"]);
        Assert.Equal(bool.FalseString, descriptor.BoundaryMetadata["external_visual_provider_api_key_value_included"]);
        Assert.Equal(bool.FalseString, descriptor.BoundaryMetadata["secret_value_included"]);
        Assert.Equal(DesignConversionVisualAiProviderSelectionPolicy.PolicyName, descriptor.BoundaryMetadata[DesignConversionVisualAiProviderSelectionPolicy.ProviderSelectionPolicyKey]);
        Assert.Equal(DesignConversionVisualAiProviderSelectionPolicy.CandidateProviders, descriptor.BoundaryMetadata[DesignConversionVisualAiProviderSelectionPolicy.CandidateProvidersKey]);
        Assert.Equal("auto", descriptor.BoundaryMetadata[DesignConversionVisualAiProviderSelectionPolicy.RequestedProviderKey]);
        Assert.Equal("none", descriptor.BoundaryMetadata[DesignConversionVisualAiProviderSelectionPolicy.SelectedProviderKey]);
        Assert.Equal("not_configured", descriptor.BoundaryMetadata[DesignConversionVisualAiProviderSelectionPolicy.ProviderModeKey]);
        Assert.Equal("fallback_unavailable", descriptor.BoundaryMetadata[DesignConversionVisualAiProviderSelectionPolicy.ProviderFallbackStatusKey]);
        Assert.Equal("provider_config_incomplete", descriptor.BoundaryMetadata[DesignConversionVisualAiProviderSelectionPolicy.ProviderFallbackReasonKey]);
        Assert.Equal("3", descriptor.BoundaryMetadata[DesignConversionVisualAiProviderSelectionPolicy.UnavailableProviderCountKey]);
        Assert.Equal("claude,openai,gemini", descriptor.BoundaryMetadata[DesignConversionVisualAiProviderSelectionPolicy.UnavailableProvidersKey]);
        Assert.Equal(bool.FalseString, descriptor.BoundaryMetadata[DesignConversionVisualAiProviderSelectionPolicy.RealCloudProviderClaimedKey]);
        Assert.False(options.IsSuccess);

        var serializedDescriptor = JsonSerializer.Serialize(descriptor);
        Assert.DoesNotContain(secret, serializedDescriptor, StringComparison.Ordinal);
        Assert.DoesNotContain(secret, options.ErrorMessage ?? string.Empty, StringComparison.Ordinal);
    }

    [Fact]
    public void ExternalVisualProviderConfigIntake_OpenAiSettingsExposeSelectionPolicyWithoutCloudClaim()
    {
        var secret = "pass-selection-" + Guid.NewGuid().ToString("N");
        var artifactRoot = CreateServerOwnedImageArtifactRoot();
        var input = DesignConversionExternalVisualProviderConfigIntake.FromSettings(
            new Dictionary<string, string>
            {
                [DesignConversionExternalVisualProviderConfigIntake.EndpointSettingName] = "https://visual-judge.example/v1/judge",
                [DesignConversionExternalVisualProviderConfigIntake.ProviderSettingName] = "openai",
                [DesignConversionExternalVisualProviderConfigIntake.ModelSettingName] = "gpt-vision-safe-test",
                [DesignConversionExternalVisualProviderConfigIntake.ApiKeySettingName] = secret,
                [DesignConversionExternalVisualProviderConfigIntake.ServerOwnedImageArtifactRootSettingName] =
                    artifactRoot
            });

        var descriptor = DesignConversionExternalVisualProviderConfigIntake.Describe(input);
        var options = DesignConversionExternalVisualProviderConfigIntake.TryCreateOptions(input);

        Assert.True(descriptor.IsComplete);
        Assert.Equal("openai", descriptor.BoundaryMetadata[DesignConversionVisualAiProviderSelectionPolicy.RequestedProviderKey]);
        Assert.Equal("openai", descriptor.BoundaryMetadata[DesignConversionVisualAiProviderSelectionPolicy.SelectedProviderKey]);
        Assert.Equal("configured_not_called", descriptor.BoundaryMetadata[DesignConversionVisualAiProviderSelectionPolicy.SelectedProviderStatusKey]);
        Assert.Equal("external_configured_not_called", descriptor.BoundaryMetadata[DesignConversionVisualAiProviderSelectionPolicy.ProviderModeKey]);
        Assert.Equal("not_needed", descriptor.BoundaryMetadata[DesignConversionVisualAiProviderSelectionPolicy.ProviderFallbackStatusKey]);
        Assert.Equal("none", descriptor.BoundaryMetadata[DesignConversionVisualAiProviderSelectionPolicy.ProviderFallbackReasonKey]);
        Assert.Equal("2", descriptor.BoundaryMetadata[DesignConversionVisualAiProviderSelectionPolicy.UnavailableProviderCountKey]);
        Assert.Equal("claude,gemini", descriptor.BoundaryMetadata[DesignConversionVisualAiProviderSelectionPolicy.UnavailableProvidersKey]);
        Assert.Equal(bool.FalseString, descriptor.BoundaryMetadata[DesignConversionVisualAiProviderSelectionPolicy.RealCloudProviderClaimedKey]);
        Assert.True(options.IsSuccess, options.ErrorMessage);
        Assert.Equal("openai", options.Value!.Metadata![DesignConversionVisualAiProviderSelectionPolicy.SelectedProviderKey]);
        Assert.Equal("external_configured_not_called", options.Value.Metadata[DesignConversionVisualAiProviderSelectionPolicy.ProviderModeKey]);
        Assert.Equal(bool.FalseString, options.Value.Metadata[DesignConversionVisualAiProviderSelectionPolicy.RealCloudProviderClaimedKey]);

        var serializedDescriptor = JsonSerializer.Serialize(descriptor);
        Assert.DoesNotContain(secret, serializedDescriptor, StringComparison.Ordinal);
        Assert.DoesNotContain(artifactRoot, serializedDescriptor, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ExternalVisualProviderConfigIntake_MissingImageRootIsIncompleteAndFactoryFailsBeforeNetwork()
    {
        var secret = "missing-root-secret-" + Guid.NewGuid().ToString("N");
        var input = DesignConversionExternalVisualProviderConfigIntake.FromSettings(
            new Dictionary<string, string>
            {
                [DesignConversionExternalVisualProviderConfigIntake.EndpointSettingName] = "https://visual-judge.example/v1/judge",
                [DesignConversionExternalVisualProviderConfigIntake.ProviderSettingName] = "safe-vision-provider",
                [DesignConversionExternalVisualProviderConfigIntake.ModelSettingName] = "safe-vision-model",
                [DesignConversionExternalVisualProviderConfigIntake.ApiKeySettingName] = secret
            });
        var handler = new CountingNoNetworkHandler();

        var descriptor = DesignConversionExternalVisualProviderConfigIntake.Describe(input);
        var options = DesignConversionExternalVisualProviderConfigIntake.TryCreateOptions(input);
        var comparisonProvider = DesignConversionExternalVisualProviderConfigIntake.TryCreateComparisonProvider(input, handler);
        var visualJudge = DesignConversionExternalVisualProviderConfigIntake.TryCreateVisualJudge(input, handler);

        Assert.False(descriptor.IsComplete);
        Assert.False(descriptor.FactoryReady);
        Assert.False(descriptor.RealExternalProviderExecutableWhenNetworkAuthorized);
        Assert.Contains(
            DesignConversionExternalVisualProviderConfigIntake.ServerOwnedImageArtifactRootSettingName,
            descriptor.MissingSettingNames);
        Assert.Equal(bool.FalseString, descriptor.BoundaryMetadata["external_visual_provider_config_complete"]);
        Assert.Equal(bool.FalseString, descriptor.BoundaryMetadata["external_visual_provider_factory_ready"]);
        Assert.Equal(bool.FalseString, descriptor.BoundaryMetadata["real_external_provider_executable_when_network_authorized"]);
        Assert.False(options.IsSuccess);
        Assert.False(comparisonProvider.IsSuccess);
        Assert.False(visualJudge.IsSuccess);
        Assert.Equal(0, handler.CallCount);

        var serialized = JsonSerializer.Serialize(new
        {
            descriptor,
            options.ErrorCode,
            options.ErrorMessage,
            ComparisonProviderErrorCode = comparisonProvider.ErrorCode,
            VisualJudgeErrorCode = visualJudge.ErrorCode
        });
        Assert.DoesNotContain(secret, serialized, StringComparison.Ordinal);
        Assert.DoesNotContain("/v1/judge", serialized, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(@"\\server\share\visual-artifacts")]
    [InlineData("//server/share/visual-artifacts")]
    public void ExternalVisualProviderConfigIntake_NetworkImageRootIsInvalidAndFactoryFailsBeforeNetwork(string networkRoot)
    {
        var secret = "network-root-secret-" + Guid.NewGuid().ToString("N");
        var input = DesignConversionExternalVisualProviderConfigIntake.FromSettings(
            new Dictionary<string, string>
            {
                [DesignConversionExternalVisualProviderConfigIntake.EndpointSettingName] = "https://visual-judge.example/v1/judge",
                [DesignConversionExternalVisualProviderConfigIntake.ProviderSettingName] = "safe-vision-provider",
                [DesignConversionExternalVisualProviderConfigIntake.ModelSettingName] = "safe-vision-model",
                [DesignConversionExternalVisualProviderConfigIntake.ApiKeySettingName] = secret,
                [DesignConversionExternalVisualProviderConfigIntake.ServerOwnedImageArtifactRootSettingName] = networkRoot
            });
        var handler = new CountingNoNetworkHandler();

        var descriptor = DesignConversionExternalVisualProviderConfigIntake.Describe(input);
        var options = DesignConversionExternalVisualProviderConfigIntake.TryCreateOptions(input);
        var visualJudge = DesignConversionExternalVisualProviderConfigIntake.TryCreateVisualJudge(input, handler);

        Assert.False(descriptor.IsComplete);
        Assert.False(descriptor.FactoryReady);
        Assert.False(descriptor.RealExternalProviderExecutableWhenNetworkAuthorized);
        Assert.Contains(
            DesignConversionExternalVisualProviderConfigIntake.ServerOwnedImageArtifactRootSettingName,
            descriptor.InvalidSettingNames);
        Assert.False(options.IsSuccess);
        Assert.False(visualJudge.IsSuccess);
        Assert.Equal(0, handler.CallCount);

        var serialized = JsonSerializer.Serialize(new
        {
            descriptor,
            OptionsErrorCode = options.ErrorCode,
            OptionsErrorMessage = options.ErrorMessage,
            VisualJudgeErrorCode = visualJudge.ErrorCode,
            VisualJudgeErrorMessage = visualJudge.ErrorMessage
        });
        Assert.DoesNotContain(secret, serialized, StringComparison.Ordinal);
        Assert.DoesNotContain(networkRoot, serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("/v1/judge", serialized, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ExternalVisualProviderConfigIntake_RequiresHttpsAndRedactsInvalidEndpoints()
    {
        var secret = "pass-endpoint-safety-" + Guid.NewGuid().ToString("N");
        var publicHttps = new DesignConversionExternalVisualProviderConfigInput(
            Endpoint: "https://visual-judge.example/v1/judge",
            ProviderName: "openai",
            ModelId: "gpt-vision-public-https",
            ApiKey: secret,
            ServerOwnedImageArtifactRoot: CreateServerOwnedImageArtifactRoot());

        var publicHttpsDescriptor = DesignConversionExternalVisualProviderConfigIntake.Describe(publicHttps);
        var publicHttpsOptions = DesignConversionExternalVisualProviderConfigIntake.TryCreateOptions(publicHttps);

        Assert.True(publicHttpsDescriptor.IsComplete);
        Assert.True(publicHttpsOptions.IsSuccess, publicHttpsOptions.ErrorMessage);
        Assert.Equal("visual-judge.example", publicHttpsDescriptor.EndpointHost);
        var publicHttpsEndpoint = Assert.IsType<Uri>(publicHttpsOptions.Value!.Endpoint);
        Assert.Equal(Uri.UriSchemeHttps, publicHttpsEndpoint.Scheme);
        Assert.Empty(publicHttpsDescriptor.InvalidSettingNames);

        var invalidCases = new[]
        {
            new
            {
                Endpoint = "http://127.0.0.1:43187/v1/judge",
                Forbidden = new[] { "http://127.0.0.1:43187/v1/judge", "127.0.0.1", "43187" }
            },
            new
            {
                Endpoint = "http://visual-judge.example/v1/judge",
                Forbidden = new[] { "http://visual-judge.example/v1/judge", "visual-judge.example", "/v1/judge" }
            },
            new
            {
                Endpoint = "file:///C:/private/visual.png",
                Forbidden = new[] { "file:///C:/private/visual.png", "C:/private", "visual.png" }
            },
            new
            {
                Endpoint = "ftp://visual-judge.example/v1/judge",
                Forbidden = new[] { "ftp://visual-judge.example/v1/judge", "visual-judge.example", "/v1/judge" }
            },
            new
            {
                Endpoint = "https://user:password@visual-judge.example/v1/judge",
                Forbidden = new[] { "https://user:password@visual-judge.example/v1/judge", "user:password", "password", "visual-judge.example" }
            },
            new
            {
                Endpoint = "https://localhost/v1/judge",
                Forbidden = new[] { "https://localhost/v1/judge", "localhost" }
            },
            new
            {
                Endpoint = "https://localhost./v1/judge",
                Forbidden = new[] { "https://localhost./v1/judge", "localhost." }
            },
            new
            {
                Endpoint = "https://api.localhost/v1/judge",
                Forbidden = new[] { "https://api.localhost/v1/judge", "api.localhost", "localhost" }
            },
            new
            {
                Endpoint = "https://visual.local/v1/judge",
                Forbidden = new[] { "https://visual.local/v1/judge", "visual.local" }
            },
            new
            {
                Endpoint = "https://127.0.0.1/v1/judge",
                Forbidden = new[] { "https://127.0.0.1/v1/judge", "127.0.0.1" }
            },
            new
            {
                Endpoint = "https://10.0.0.1/v1/judge",
                Forbidden = new[] { "https://10.0.0.1/v1/judge", "10.0.0.1" }
            },
            new
            {
                Endpoint = "https://169.254.169.254/latest/meta-data",
                Forbidden = new[] { "https://169.254.169.254/latest/meta-data", "169.254.169.254", "meta-data" }
            },
            new
            {
                Endpoint = "https://192.168.1.1/v1/judge",
                Forbidden = new[] { "https://192.168.1.1/v1/judge", "192.168.1.1" }
            },
            new
            {
                Endpoint = "https://[::1]/v1/judge",
                Forbidden = new[] { "https://[::1]/v1/judge", "[::1]", "::1" }
            },
            new
            {
                Endpoint = "https://[fc00::1]/v1/judge",
                Forbidden = new[] { "https://[fc00::1]/v1/judge", "[fc00::1]", "fc00::1" }
            },
            new
            {
                Endpoint = "https://[fe80::1]/v1/judge",
                Forbidden = new[] { "https://[fe80::1]/v1/judge", "[fe80::1]", "fe80::1" }
            }
        };

        foreach (var testCase in invalidCases)
        {
            var input = new DesignConversionExternalVisualProviderConfigInput(
                Endpoint: testCase.Endpoint,
                ProviderName: "openai",
                ModelId: "gpt-vision-invalid-endpoint",
                ApiKey: secret,
                ServerOwnedImageArtifactRoot: CreateServerOwnedImageArtifactRoot());

            var descriptor = DesignConversionExternalVisualProviderConfigIntake.Describe(input);
            var options = DesignConversionExternalVisualProviderConfigIntake.TryCreateOptions(input);

            Assert.False(descriptor.IsComplete);
            Assert.False(descriptor.FactoryReady);
            Assert.False(descriptor.RealExternalProviderExecutableWhenNetworkAuthorized);
            Assert.False(options.IsSuccess);
            Assert.Contains(
                DesignConversionExternalVisualProviderConfigIntake.EndpointSettingName,
                descriptor.InvalidSettingNames);
            Assert.Equal("invalid", descriptor.EndpointHost);
            Assert.Equal("invalid", descriptor.BoundaryMetadata["endpoint_host"]);
            Assert.Equal(bool.FalseString, descriptor.BoundaryMetadata["external_visual_provider_config_complete"]);
            Assert.Equal(bool.FalseString, descriptor.BoundaryMetadata["real_external_provider_executable_when_network_authorized"]);

            var serialized = JsonSerializer.Serialize(new
            {
                descriptor,
                options.ErrorCode,
                options.ErrorMessage
            });
            Assert.DoesNotContain(secret, serialized, StringComparison.Ordinal);
            Assert.DoesNotContain(testCase.Endpoint, serialized, StringComparison.OrdinalIgnoreCase);
            foreach (var fragment in testCase.Forbidden)
                Assert.DoesNotContain(fragment, serialized, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void ExternalVisualProviderConfigIntake_InvalidEndpointReturnsInvalidKeyOnly()
    {
        var secret = "pass135-" + Guid.NewGuid().ToString("N");
        var input = new DesignConversionExternalVisualProviderConfigInput(
            Endpoint: "not-a-valid-absolute-endpoint",
            ProviderName: "safe-provider",
            ModelId: "safe-model",
            ApiKey: secret,
            ServerOwnedImageArtifactRoot: CreateServerOwnedImageArtifactRoot());

        var descriptor = DesignConversionExternalVisualProviderConfigIntake.Describe(input);

        Assert.False(descriptor.IsComplete);
        Assert.Empty(descriptor.MissingSettingNames);
        Assert.Contains(DesignConversionExternalVisualProviderConfigIntake.EndpointSettingName, descriptor.InvalidSettingNames);
        Assert.Equal("invalid", descriptor.EndpointHost);
        Assert.Equal(bool.FalseString, descriptor.BoundaryMetadata["external_visual_provider_config_complete"]);
        Assert.Equal(bool.FalseString, descriptor.BoundaryMetadata["real_external_provider_executable_when_network_authorized"]);

        var serializedDescriptor = JsonSerializer.Serialize(descriptor);
        Assert.DoesNotContain(secret, serializedDescriptor, StringComparison.Ordinal);
    }

    [Fact]
    public void ExternalVisualProviderConfigIntake_CompleteSettingsBuildsFactoryDescriptorWithoutNetwork()
    {
        var secret = "pass135-" + Guid.NewGuid().ToString("N");
        var input = DesignConversionExternalVisualProviderConfigIntake.FromSettings(
            new Dictionary<string, string>
            {
                [DesignConversionExternalVisualProviderConfigIntake.EndpointSettingName] = "https://visual-judge.example/v1/judge",
                [DesignConversionExternalVisualProviderConfigIntake.ProviderSettingName] = "safe-vision-provider",
                [DesignConversionExternalVisualProviderConfigIntake.ModelSettingName] = "safe-vision-model",
                [DesignConversionExternalVisualProviderConfigIntake.ApiKeySettingName] = secret,
                [DesignConversionExternalVisualProviderConfigIntake.ApiKeyHeaderSettingName] = "X-Test-Key",
                [DesignConversionExternalVisualProviderConfigIntake.ServerOwnedImageArtifactRootSettingName] =
                    CreateServerOwnedImageArtifactRoot()
            });

        var descriptor = DesignConversionExternalVisualProviderConfigIntake.Describe(input);
        var options = DesignConversionExternalVisualProviderConfigIntake.TryCreateOptions(input);

        Assert.True(descriptor.IsComplete);
        Assert.True(descriptor.FactoryReady);
        Assert.True(descriptor.RealExternalProviderExecutableWhenNetworkAuthorized);
        Assert.False(descriptor.RealExternalProviderExecuted);
        Assert.False(descriptor.NetworkCallExecuted);
        Assert.Empty(descriptor.MissingSettingNames);
        Assert.Empty(descriptor.InvalidSettingNames);
        Assert.Equal("visual-judge.example", descriptor.EndpointHost);
        Assert.Equal("safe-vision-provider", descriptor.ProviderName);
        Assert.Equal("safe-vision-model", descriptor.ModelId);
        Assert.Equal("X-Test-Key", descriptor.ApiKeyHeaderName);
        Assert.Equal(bool.TrueString, descriptor.BoundaryMetadata["external_visual_provider_config_complete"]);
        Assert.Equal(bool.TrueString, descriptor.BoundaryMetadata["external_visual_provider_factory_ready"]);
        Assert.Equal(bool.TrueString, descriptor.BoundaryMetadata["real_external_provider_executable_when_network_authorized"]);
        Assert.Equal(bool.FalseString, descriptor.BoundaryMetadata["real_external_provider_executed"]);
        Assert.Equal(bool.FalseString, descriptor.BoundaryMetadata["network_call_executed"]);
        Assert.Equal(bool.FalseString, descriptor.BoundaryMetadata["external_visual_provider_api_key_value_included"]);
        Assert.Equal(bool.FalseString, descriptor.BoundaryMetadata["secret_value_included"]);
        Assert.Equal(bool.FalseString, descriptor.BoundaryMetadata["server_owned_pricing_configured"]);
        Assert.Equal(bool.FalseString, descriptor.BoundaryMetadata["server_owned_pricing_valid"]);
        Assert.Equal("unconfigured", descriptor.BoundaryMetadata["server_owned_pricing_status"]);
        Assert.Equal(
            DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostSourceUnknownUnpriced,
            descriptor.BoundaryMetadata["server_owned_pricing_cost_source"]);
        Assert.True(options.IsSuccess, options.ErrorMessage);
        Assert.Equal(secret, options.Value!.ApiKey);
        Assert.Equal("safe-vision-provider", options.Value.ProviderName);
        Assert.Equal("safe-vision-model", options.Value.ModelId);
        Assert.Null(options.Value.ServerOwnedCostPerRequestUsd);
        var configuredFactory = DesignConversionExternalVisualProviderConfigIntake.TryCreateComparisonProvider(
            input,
            new ThrowingNoNetworkHandler());
        Assert.True(configuredFactory.IsSuccess, configuredFactory.ErrorMessage);

        var judge = new ExternalHttpDesignConversionVisualJudge(
            options.Value,
            new HttpClient(new ThrowingNoNetworkHandler()));
        var comparisonProvider = new DesignConversionExternalVisualAiComparisonProvider(judge);

        Assert.NotNull(comparisonProvider);
        var serializedDescriptor = JsonSerializer.Serialize(descriptor);
        Assert.DoesNotContain(secret, serializedDescriptor, StringComparison.Ordinal);
        Assert.DoesNotContain("/v1/judge", serializedDescriptor, StringComparison.Ordinal);
    }

    [Fact]
    public void ExternalVisualProviderConfigIntake_ServerOwnedPriceSettingReachesOptionsWithoutDescriptorLeak()
    {
        var secret = "pass135-price-" + Guid.NewGuid().ToString("N");
        const string rawPrice = "0.0042";
        var input = DesignConversionExternalVisualProviderConfigIntake.FromSettings(
            new Dictionary<string, string>
            {
                [DesignConversionExternalVisualProviderConfigIntake.EndpointSettingName] = "https://visual-judge.example/v1/judge",
                [DesignConversionExternalVisualProviderConfigIntake.ProviderSettingName] = "safe-vision-provider",
                [DesignConversionExternalVisualProviderConfigIntake.ModelSettingName] = "safe-vision-model",
                [DesignConversionExternalVisualProviderConfigIntake.ApiKeySettingName] = secret,
                [DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostPerRequestUsdSettingName] = rawPrice,
                [DesignConversionExternalVisualProviderConfigIntake.ServerOwnedImageArtifactRootSettingName] =
                    CreateServerOwnedImageArtifactRoot()
            });

        var descriptor = DesignConversionExternalVisualProviderConfigIntake.Describe(input);
        var options = DesignConversionExternalVisualProviderConfigIntake.TryCreateOptions(input);

        Assert.True(descriptor.IsComplete);
        Assert.Empty(descriptor.InvalidSettingNames);
        Assert.Equal(bool.TrueString, descriptor.BoundaryMetadata["server_owned_pricing_configured"]);
        Assert.Equal(bool.TrueString, descriptor.BoundaryMetadata["server_owned_pricing_valid"]);
        Assert.Equal("configured", descriptor.BoundaryMetadata["server_owned_pricing_status"]);
        Assert.Equal(
            DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostSourceFixedPerRequest,
            descriptor.BoundaryMetadata["server_owned_pricing_cost_source"]);
        Assert.Equal(bool.FalseString, descriptor.BoundaryMetadata["server_owned_pricing_raw_value_included"]);
        Assert.Contains("server_owned_pricing:configured", descriptor.EvidenceRefs);
        Assert.True(options.IsSuccess, options.ErrorMessage);
        Assert.Equal(0.0042m, options.Value!.ServerOwnedCostPerRequestUsd);
        Assert.Equal(bool.TrueString, options.Value.Metadata!["server_owned_pricing_configured"]);
        Assert.Equal(
            DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostSourceFixedPerRequest,
            options.Value.Metadata["server_owned_pricing_cost_source"]);

        var serializedDescriptor = JsonSerializer.Serialize(descriptor);
        Assert.DoesNotContain(secret, serializedDescriptor, StringComparison.Ordinal);
        Assert.DoesNotContain(rawPrice, serializedDescriptor, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("-0.01")]
    [InlineData("not-a-decimal-private-price")]
    [InlineData("10.0001")]
    public void ExternalVisualProviderConfigIntake_InvalidServerOwnedPricesFailBySettingNameWithoutLeak(string rawPrice)
    {
        var secret = "pass135-invalid-price-" + Guid.NewGuid().ToString("N");
        var input = DesignConversionExternalVisualProviderConfigIntake.FromSettings(
            new Dictionary<string, string>
            {
                [DesignConversionExternalVisualProviderConfigIntake.EndpointSettingName] = "https://visual-judge.example/v1/judge",
                [DesignConversionExternalVisualProviderConfigIntake.ProviderSettingName] = "safe-vision-provider",
                [DesignConversionExternalVisualProviderConfigIntake.ModelSettingName] = "safe-vision-model",
                [DesignConversionExternalVisualProviderConfigIntake.ApiKeySettingName] = secret,
                [DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostPerRequestUsdSettingName] = rawPrice,
                [DesignConversionExternalVisualProviderConfigIntake.ServerOwnedImageArtifactRootSettingName] =
                    CreateServerOwnedImageArtifactRoot()
            });

        var descriptor = DesignConversionExternalVisualProviderConfigIntake.Describe(input);
        var options = DesignConversionExternalVisualProviderConfigIntake.TryCreateOptions(input);

        Assert.False(descriptor.IsComplete);
        Assert.Contains(
            DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostPerRequestUsdSettingName,
            descriptor.InvalidSettingNames);
        Assert.Equal(bool.TrueString, descriptor.BoundaryMetadata["server_owned_pricing_configured"]);
        Assert.Equal(bool.FalseString, descriptor.BoundaryMetadata["server_owned_pricing_valid"]);
        Assert.Equal("invalid", descriptor.BoundaryMetadata["server_owned_pricing_status"]);
        Assert.Equal(bool.FalseString, descriptor.BoundaryMetadata["server_owned_pricing_raw_value_included"]);
        Assert.Contains("server_owned_pricing:invalid", descriptor.EvidenceRefs);
        Assert.Contains(
            $"INVALID_SETTING:{DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostPerRequestUsdSettingName}",
            descriptor.Warnings);
        Assert.False(options.IsSuccess);

        var serialized = JsonSerializer.Serialize(new
        {
            descriptor,
            options.ErrorCode,
            options.ErrorMessage
        });
        Assert.Contains(DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostPerRequestUsdSettingName, serialized, StringComparison.Ordinal);
        Assert.DoesNotContain(secret, serialized, StringComparison.Ordinal);
        Assert.DoesNotContain(rawPrice, serialized, StringComparison.Ordinal);
        Assert.DoesNotContain("/v1/judge", serialized, StringComparison.Ordinal);
    }

    [Fact]
    public void ExternalVisualProviderConfigIntake_SecureProductionVisualJudgeConstructionDoesNotRunNetworkAtStartup()
    {
        var secret = "secure-production-construction-" + Guid.NewGuid().ToString("N");
        var input = DesignConversionExternalVisualProviderConfigIntake.FromSettings(
            new Dictionary<string, string>
            {
                [DesignConversionExternalVisualProviderConfigIntake.EndpointSettingName] = "https://visual-judge.example/v1/judge",
                [DesignConversionExternalVisualProviderConfigIntake.ProviderSettingName] = "safe-vision-provider",
                [DesignConversionExternalVisualProviderConfigIntake.ModelSettingName] = "safe-vision-model",
                [DesignConversionExternalVisualProviderConfigIntake.ApiKeySettingName] = secret,
                [DesignConversionExternalVisualProviderConfigIntake.ApiKeyHeaderSettingName] = "X-Test-Key",
                [DesignConversionExternalVisualProviderConfigIntake.ServerOwnedImageArtifactRootSettingName] =
                    CreateServerOwnedImageArtifactRoot()
            });

        var judge = DesignConversionExternalVisualProviderConfigIntake.TryCreateSecureProductionVisualJudge(input);

        Assert.True(judge.IsSuccess, judge.ErrorMessage);
        Assert.NotNull(judge.Value);
        var comparisonProvider = new DesignConversionExternalVisualAiComparisonProvider(judge.Value!);
        Assert.NotNull(comparisonProvider);
        var serialized = JsonSerializer.Serialize(new
        {
            judge.ErrorCode,
            judge.ErrorMessage
        });
        Assert.DoesNotContain(secret, serialized, StringComparison.Ordinal);
        Assert.DoesNotContain("/v1/judge", serialized, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TryRunImprovementLoopAsync_VisualProviderInjectedRoute_ReturnsSafeProviderBoundaryMetadata()
    {
        const string privateMarker = "PRIVATE_PASS135_VISUAL_PROVIDER_SECRET";
        var fakeProvider = new FakeNoNetworkVisualProvider();
        var comparisonProvider = new DesignConversionExternalVisualAiComparisonProvider(fakeProvider);
        var composition = DesignConversionLocalServiceComposition.CreateBrowserBackedLocal(
            new RecordingBrowserAdapter(),
            comparisonProvider,
            fakeProvider);
        var registry = new DesignConversionModelServerEndpointRegistry();
        var registered = registry.RegisterDesignConversionImprovementLoopFromComposition(
            composition,
            new UnexpectedCandidateStrategy());
        Assert.True(registered.IsSuccess, registered.ErrorMessage);

        var adapter = new DesignConversionModelServerLocalCallerAdapter(registry);
        var package = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
            LoopId: "pass135-visual-provider-route-loop",
            InitialPackage: CreatePackage("pass135-visual-provider-route-request", privateMarker),
            MaxAttempts: 2,
            Metadata: new Dictionary<string, string>
            {
                ["pass"] = "codex-pass135",
                ["caller_private_marker"] = privateMarker
            });

        var envelope = await adapter.TryRunImprovementLoopAsync(package);

        Assert.Equal("success", envelope.EnvelopeKind);
        Assert.Equal(200, envelope.StatusCode);
        Assert.True(envelope.RegistryInvokeSucceeded);
        Assert.True(envelope.RuntimeInvoked);
        Assert.True(envelope.ParsedResponseJsonSucceeded);
        Assert.Equal(DesignConversionThinClientStatus.Completed, envelope.ParsedContractStatus);
        Assert.Equal("pass135-visual-provider-route-request", envelope.ParsedRequestId);
        Assert.Null(envelope.ParsedErrorCode);
        Assert.False(envelope.RawHtmlIncluded);
        Assert.False(envelope.RawCssIncluded);
        Assert.False(envelope.RawDesignTreeIncluded);
        Assert.False(envelope.ResponseContainsRawPrivateData);
        Assert.False(envelope.RawPrivateResponseLeakageDetected);
        Assert.DoesNotContain(privateMarker, envelope.ResponseBodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(package.InitialPackage!.Html!, envelope.ResponseBodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(package.InitialPackage.Css!, envelope.ResponseBodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(package.InitialPackage.DesignTree!.Payload!, envelope.ResponseBodyJson, StringComparison.Ordinal);

        Assert.Equal(bool.TrueString, envelope.BoundaryMetadata["refs_only_public_response"]);
        Assert.Equal(bool.TrueString, envelope.BoundaryMetadata["visual_provider_output_observed"]);
        Assert.Equal(bool.TrueString, envelope.BoundaryMetadata["external_visual_ai_configured"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["external_visual_ai_executed"]);
        Assert.Equal(bool.TrueString, envelope.BoundaryMetadata["visual_ai_provider_contract_executed"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["external_ai_visual_judge_executed"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["real_external_provider_executed"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["network_call_executed"]);
        Assert.Equal(bool.TrueString, envelope.BoundaryMetadata["fake_client"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["external_visual_ai_blocked"]);
        Assert.Equal("none", envelope.BoundaryMetadata["blocked_reason"]);
        Assert.Equal(DesignConversionExternalVisualAiComparisonProvider.ProviderKind, envelope.BoundaryMetadata["provider_kind"]);
        Assert.Equal(FakeNoNetworkVisualProvider.ProviderName, envelope.BoundaryMetadata["provider_name"]);
        Assert.Equal(FakeNoNetworkVisualProvider.ModelId, envelope.BoundaryMetadata["provider_model"]);
        Assert.Equal("local-fake-provider", envelope.BoundaryMetadata["provider_endpoint_host"]);
        Assert.Equal("refs_or_hashes_only", envelope.BoundaryMetadata["image_payload_mode"]);
        Assert.Equal("0", envelope.BoundaryMetadata["visual_provider_estimated_cost_usd"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["provider_payload_included"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["product_ready"]);

        using var document = JsonDocument.Parse(envelope.ResponseBodyJson);
        var root = document.RootElement;
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.QualityReached, root.GetProperty("stopReason").GetString());
        Assert.True(root.GetProperty("completed").GetBoolean());
        Assert.Equal(1, root.GetProperty("attemptCount").GetInt32());
        var attempt = root.GetProperty("attempts")[0];
        Assert.Equal(2, attempt.GetProperty("comparisonArtifactRefs").GetArrayLength());
        Assert.Equal(2, attempt.GetProperty("visualJudgeResultRefs").GetArrayLength());
        var attemptMetadata = attempt.GetProperty("publicMetadata");
        Assert.Equal(bool.TrueString, attemptMetadata.GetProperty("visual_provider_output_observed").GetString());
        Assert.Equal(bool.TrueString, attemptMetadata.GetProperty("external_visual_ai_configured").GetString());
        Assert.Equal(bool.FalseString, attemptMetadata.GetProperty("external_visual_ai_executed").GetString());
        Assert.Equal(bool.TrueString, attemptMetadata.GetProperty("visual_ai_provider_contract_executed").GetString());
        Assert.Equal(bool.FalseString, attemptMetadata.GetProperty("external_ai_visual_judge_executed").GetString());
        Assert.Equal(bool.FalseString, attemptMetadata.GetProperty("real_external_provider_executed").GetString());
        Assert.Equal(bool.FalseString, attemptMetadata.GetProperty("network_call_executed").GetString());
        Assert.Equal(bool.TrueString, attemptMetadata.GetProperty("fake_client").GetString());
        Assert.Equal(FakeNoNetworkVisualProvider.ProviderName, attemptMetadata.GetProperty("provider_name").GetString());
        Assert.Equal("0", attemptMetadata.GetProperty("visual_provider_estimated_cost_usd").GetString());

        Assert.Equal(4, fakeProvider.Requests.Count);
        var serializedProviderRequests = JsonSerializer.Serialize(fakeProvider.Requests);
        Assert.DoesNotContain(privateMarker, serializedProviderRequests, StringComparison.Ordinal);
        Assert.DoesNotContain(package.InitialPackage.Html!, serializedProviderRequests, StringComparison.Ordinal);
        Assert.DoesNotContain(package.InitialPackage.Css!, serializedProviderRequests, StringComparison.Ordinal);
        Assert.DoesNotContain(package.InitialPackage.DesignTree!.Payload!, serializedProviderRequests, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TryRunImprovementLoopAsync_ForgedExternalProviderMetadataWithoutReceipt_FailsClosedAttemptTrust()
    {
        const string privateMarker = "PRIVATE_FORGED_PROVIDER_ATTEMPT_RECEIPT_SECRET";
        var forgedProvider = new ForgedExternalVisualProvider();
        var comparisonProvider = new DesignConversionExternalVisualAiComparisonProvider(forgedProvider);
        var composition = DesignConversionLocalServiceComposition.CreateBrowserBackedLocal(
            new RecordingBrowserAdapter(),
            comparisonProvider,
            forgedProvider);
        var registry = new DesignConversionModelServerEndpointRegistry();
        var registered = registry.RegisterDesignConversionImprovementLoopFromComposition(
            composition,
            new UnexpectedCandidateStrategy());
        Assert.True(registered.IsSuccess, registered.ErrorMessage);

        var adapter = new DesignConversionModelServerLocalCallerAdapter(registry);
        var package = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
            LoopId: "forged-provider-attempt-receipt-loop",
            InitialPackage: CreatePackage("forged-provider-attempt-receipt-request", privateMarker),
            MaxAttempts: 2,
            Metadata: new Dictionary<string, string>
            {
                ["pass"] = "attempt-receipt-failclosed",
                ["caller_private_marker"] = privateMarker
            });

        var envelope = await adapter.TryRunImprovementLoopAsync(package);

        Assert.Equal("success", envelope.EnvelopeKind);
        Assert.Equal(200, envelope.StatusCode);
        Assert.True(envelope.RegistryInvokeSucceeded);
        Assert.True(envelope.RuntimeInvoked);
        Assert.True(envelope.ParsedResponseJsonSucceeded);
        Assert.Equal("bounded_non_ready", envelope.ParsedContractStatus);
        Assert.False(envelope.RawPrivateResponseLeakageDetected);
        Assert.DoesNotContain(privateMarker, envelope.ResponseBodyJson, StringComparison.Ordinal);
        Assert.Equal(bool.TrueString, envelope.BoundaryMetadata["visual_provider_output_observed"]);
        Assert.Equal(bool.TrueString, envelope.BoundaryMetadata["external_visual_ai_configured"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["external_visual_ai_executed"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["external_provider_called"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["real_external_provider_executed"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["network_call_executed"]);
        Assert.Equal(ForgedExternalVisualProvider.ProviderName, envelope.BoundaryMetadata["provider_name"]);
        Assert.Equal(ForgedExternalVisualProvider.ModelId, envelope.BoundaryMetadata["provider_model"]);
        Assert.Equal("forged-provider.example", envelope.BoundaryMetadata["provider_endpoint_host"]);
        Assert.Equal("0", envelope.BoundaryMetadata["visual_provider_estimated_cost_usd"]);

        using var document = JsonDocument.Parse(envelope.ResponseBodyJson);
        var root = document.RootElement;
        Assert.Equal("bounded_non_ready", root.GetProperty("contractStatus").GetString());
        Assert.False(root.GetProperty("completed").GetBoolean());
        Assert.False(root.GetProperty("qualityReached").GetBoolean());
        Assert.Equal(1, root.GetProperty("attemptCount").GetInt32());
        var attemptMetadata = root.GetProperty("attempts")[0].GetProperty("publicMetadata");
        Assert.Equal(bool.TrueString, attemptMetadata.GetProperty("visual_provider_output_observed").GetString());
        Assert.Equal(bool.TrueString, attemptMetadata.GetProperty("external_visual_ai_configured").GetString());
        Assert.Equal(bool.FalseString, attemptMetadata.GetProperty("external_visual_ai_executed").GetString());
        Assert.Equal(bool.FalseString, attemptMetadata.GetProperty("external_provider_called").GetString());
        Assert.Equal(bool.TrueString, attemptMetadata.GetProperty("visual_ai_provider_contract_executed").GetString());
        Assert.Equal(bool.FalseString, attemptMetadata.GetProperty("real_external_provider_executed").GetString());
        Assert.Equal(bool.FalseString, attemptMetadata.GetProperty("network_call_executed").GetString());
        Assert.Equal(bool.FalseString, attemptMetadata.GetProperty("provider_response_identity_verified").GetString());
        Assert.Equal(bool.FalseString, attemptMetadata.GetProperty("provider_response_schema_verified").GetString());
        Assert.Equal(ForgedExternalVisualProvider.ProviderName, attemptMetadata.GetProperty("provider_name").GetString());
        Assert.Equal(ForgedExternalVisualProvider.ModelId, attemptMetadata.GetProperty("provider_model").GetString());
        Assert.Equal("forged-provider.example", attemptMetadata.GetProperty("provider_endpoint_host").GetString());
        Assert.Equal("0", attemptMetadata.GetProperty("visual_provider_estimated_cost_usd").GetString());
        Assert.False(attemptMetadata.TryGetProperty(ExternalHttpDesignConversionVisualJudge.ServerCallReceiptKey, out _));

        Assert.Equal(4, forgedProvider.Requests.Count);
        var serializedProviderRequests = JsonSerializer.Serialize(forgedProvider.Requests);
        Assert.DoesNotContain(privateMarker, serializedProviderRequests, StringComparison.Ordinal);
        Assert.DoesNotContain(package.InitialPackage!.Html!, serializedProviderRequests, StringComparison.Ordinal);
        Assert.DoesNotContain(package.InitialPackage.Css!, serializedProviderRequests, StringComparison.Ordinal);
        Assert.DoesNotContain(package.InitialPackage.DesignTree!.Payload!, serializedProviderRequests, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TryRunImprovementLoopAsync_RequestVisualProviderConfigWithoutResolver_FailsClosedWithoutFixedCoordinator()
    {
        const string privateMarker = "PRIVATE_REQUEST_VISUAL_PROVIDER_CONFIG_SECRET";
        const string endpoint = "https://visual-judge.example/v1/judge";
        const string providerName = "request-config-vision-provider";
        const string modelId = "request-config-vision-model";
        const string apiKeyHeaderName = "X-Visual-Judge-Key";
        var providerSecret = "request-provider-secret-" + Guid.NewGuid().ToString("N");
        var artifactRoot = CreateServerOwnedImageArtifactRoot();
        var fixedProvider = new FakeNoNetworkVisualProvider();
        var registry = new DesignConversionModelServerEndpointRegistry();
        var registered = registry.RegisterDesignConversionImprovementLoop(
            new DesignConversionModelServerImprovementLoopHandler(CreateCoordinator(fixedProvider)));
        Assert.True(registered.IsSuccess, registered.ErrorMessage);

        var package = await CreatePackageWithServerOwnedPngArtifactsAsync(
            "request-visual-provider-config-route-request",
            privateMarker,
            artifactRoot);
        var routeRequest = new DesignConversionModelServerImprovementLoopRequest(
            LoopId: "request-visual-provider-config-loop",
            InitialPackage: package,
            MaxAttempts: 2,
            Metadata: new Dictionary<string, string>
            {
                ["caller_private_marker"] = privateMarker
            },
            ExternalVisualProviderConfig: new DesignConversionExternalVisualProviderConfigInput(
                Endpoint: endpoint,
                ProviderName: providerName,
                ModelId: modelId,
                ApiKey: providerSecret,
                ApiKeyHeaderName: apiKeyHeaderName,
                ApiKeyScheme: "Bearer",
                ServerOwnedImageArtifactRoot: artifactRoot));
        var invoked = await registry.InvokeAsync(new DesignConversionModelServerHttpRequest(
            Method: DesignConversionModelServerEndpointRegistry.EndpointMethod,
            Path: DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath,
            BodyJson: JsonSerializer.Serialize(routeRequest, DesignConversionModelServerHttpHandler.JsonOptions),
            ContentType: "application/json",
            Headers: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["x-design-conversion-test-route"] = "request-config-intake"
            }));

        Assert.True(invoked.IsSuccess, invoked.ErrorMessage);
        var response = invoked.Value!;
        Assert.Equal(503, response.StatusCode);
        Assert.Empty(fixedProvider.Requests);
        AssertNoRequestPayloadLeak(
            response.BodyJson,
            package,
            privateMarker,
            endpoint,
            "visual-judge.example",
            "/v1/judge",
            providerSecret,
            artifactRoot,
            providerName,
            modelId,
            apiKeyHeaderName);

        using var document = JsonDocument.Parse(response.BodyJson);
        var root = document.RootElement;
        Assert.Equal(
            DesignConversionModelServerImprovementLoopHandler.ExternalVisualProviderCoordinatorResolverNotConfiguredErrorCode,
            root.GetProperty("errorCode").GetString());
        Assert.Equal(503, root.GetProperty("statusCode").GetInt32());
        var boundaryMetadata = root.GetProperty("boundaryMetadata");
        Assert.Equal(bool.FalseString, boundaryMetadata.GetProperty("runtime_invoked").GetString());
        Assert.Equal(bool.FalseString, boundaryMetadata.GetProperty("improvement_loop_coordinator_configured").GetString());
        Assert.Equal(bool.FalseString, boundaryMetadata.GetProperty("external_provider_called").GetString());
        Assert.Equal(bool.FalseString, boundaryMetadata.GetProperty("external_ai_visual_judge_executed").GetString());
        Assert.Equal(bool.FalseString, boundaryMetadata.GetProperty("network_call_executed").GetString());
        Assert.Equal(bool.FalseString, boundaryMetadata.GetProperty("provider_payload_included").GetString());
        Assert.Equal(bool.FalseString, boundaryMetadata.GetProperty("raw_html_included").GetString());
        Assert.Equal(bool.FalseString, boundaryMetadata.GetProperty("raw_css_included").GetString());
        Assert.Equal(bool.FalseString, boundaryMetadata.GetProperty("raw_design_tree_included").GetString());
        Assert.Equal(bool.FalseString, boundaryMetadata.GetProperty("response_contains_raw_private_data").GetString());
        Assert.False(boundaryMetadata.TryGetProperty("external_visual_provider_endpoint_host", out _));
        Assert.False(boundaryMetadata.TryGetProperty("external_visual_provider_name", out _));
        Assert.False(boundaryMetadata.TryGetProperty("external_visual_provider_model", out _));
        Assert.False(boundaryMetadata.TryGetProperty("external_visual_provider_api_key_header_name", out _));
    }

    [Fact]
    public async Task TryRunImprovementLoopAsync_RequestVisualProviderConfigResolverSuccess_UsesOnlyResolvedCoordinator()
    {
        const string privateMarker = "PRIVATE_REQUEST_RESOLVED_VISUAL_PROVIDER_SECRET";
        const string endpoint = "https://resolved-visual-judge.example/v1/resolved-judge";
        const string providerName = "resolved-request-config-provider";
        const string modelId = "resolved-request-config-model";
        const string apiKeyHeaderName = "X-Resolved-Visual-Key";
        var providerSecret = "resolved-provider-secret-" + Guid.NewGuid().ToString("N");
        var artifactRoot = CreateServerOwnedImageArtifactRoot();
        var fixedProvider = new FakeNoNetworkVisualProvider();
        var resolvedProvider = new FakeNoNetworkVisualProvider();
        var resolvedCoordinator = CreateCoordinator(resolvedProvider);
        var resolverInvocationCount = 0;
        var handler = new DesignConversionModelServerImprovementLoopHandler(
            CreateCoordinator(fixedProvider),
            externalVisualProviderCoordinatorResolver: config =>
            {
                resolverInvocationCount++;
                Assert.Equal(endpoint, config.Endpoint);
                Assert.Equal(providerName, config.ProviderName);
                Assert.Equal(modelId, config.ModelId);
                return OperationResult<DesignConversionImprovementLoopCoordinator>.Ok(resolvedCoordinator);
            });
        var registry = new DesignConversionModelServerEndpointRegistry();
        var registered = registry.RegisterDesignConversionImprovementLoop(handler);
        Assert.True(registered.IsSuccess, registered.ErrorMessage);

        var package = await CreatePackageWithServerOwnedPngArtifactsAsync(
            "request-resolved-visual-provider-route-request",
            privateMarker,
            artifactRoot);
        var routeRequest = new DesignConversionModelServerImprovementLoopRequest(
            LoopId: "request-resolved-visual-provider-loop",
            InitialPackage: package,
            MaxAttempts: 2,
            Metadata: new Dictionary<string, string>
            {
                ["caller_private_marker"] = privateMarker
            },
            ExternalVisualProviderConfig: new DesignConversionExternalVisualProviderConfigInput(
                Endpoint: endpoint,
                ProviderName: providerName,
                ModelId: modelId,
                ApiKey: providerSecret,
                ApiKeyHeaderName: apiKeyHeaderName,
                ApiKeyScheme: "Bearer",
                ServerOwnedImageArtifactRoot: artifactRoot));
        var invoked = await registry.InvokeAsync(new DesignConversionModelServerHttpRequest(
            Method: DesignConversionModelServerEndpointRegistry.EndpointMethod,
            Path: DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath,
            BodyJson: JsonSerializer.Serialize(routeRequest, DesignConversionModelServerHttpHandler.JsonOptions),
            ContentType: "application/json",
            Headers: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["x-design-conversion-test-route"] = "request-config-resolver-success"
            }));

        Assert.True(invoked.IsSuccess, invoked.ErrorMessage);
        var response = invoked.Value!;
        Assert.Equal(200, response.StatusCode);
        Assert.Equal(1, resolverInvocationCount);
        Assert.Empty(fixedProvider.Requests);
        Assert.Equal(4, resolvedProvider.Requests.Count);
        AssertNoRequestPayloadLeak(
            response.BodyJson,
            package,
            privateMarker,
            endpoint,
            "/v1/resolved-judge",
            providerSecret,
            artifactRoot);

        using var document = JsonDocument.Parse(response.BodyJson);
        var root = document.RootElement;
        Assert.Equal(DesignConversionThinClientStatus.Completed, root.GetProperty("contractStatus").GetString());
        var boundaryMetadata = root.GetProperty("boundaryMetadata");
        Assert.Equal(bool.TrueString, boundaryMetadata.GetProperty("runtime_invoked").GetString());
        Assert.Equal(bool.TrueString, boundaryMetadata.GetProperty("external_visual_provider_request_config_present").GetString());
        Assert.Equal(bool.TrueString, boundaryMetadata.GetProperty("external_visual_provider_config_complete").GetString());
        Assert.Equal(bool.TrueString, boundaryMetadata.GetProperty("fake_client").GetString());
        Assert.Equal(bool.FalseString, boundaryMetadata.GetProperty("external_provider_called").GetString());
        Assert.Equal(bool.FalseString, boundaryMetadata.GetProperty("external_ai_visual_judge_executed").GetString());
        Assert.Equal(bool.FalseString, boundaryMetadata.GetProperty("real_external_provider_executed").GetString());
        Assert.Equal(bool.FalseString, boundaryMetadata.GetProperty("network_call_executed").GetString());
        Assert.Equal(bool.FalseString, boundaryMetadata.GetProperty("provider_payload_included").GetString());
        Assert.Equal(bool.FalseString, boundaryMetadata.GetProperty("response_contains_raw_private_data").GetString());
    }

    [Fact]
    public async Task TryRunImprovementLoopAsync_NoRequestVisualProviderConfig_UsesFixedCoordinatorWithoutResolver()
    {
        const string privateMarker = "PRIVATE_NO_REQUEST_VISUAL_PROVIDER_CONFIG_SECRET";
        var fixedProvider = new FakeNoNetworkVisualProvider();
        var resolverInvocationCount = 0;
        var handler = new DesignConversionModelServerImprovementLoopHandler(
            CreateCoordinator(fixedProvider),
            externalVisualProviderCoordinatorResolver: _ =>
            {
                resolverInvocationCount++;
                return OperationResult<DesignConversionImprovementLoopCoordinator>.Fail(
                    "The request-scoped resolver must not run without request config.",
                    "UNEXPECTED_REQUEST_SCOPED_RESOLVER_INVOCATION");
            });
        var registry = new DesignConversionModelServerEndpointRegistry();
        var registered = registry.RegisterDesignConversionImprovementLoop(handler);
        Assert.True(registered.IsSuccess, registered.ErrorMessage);

        var package = CreatePackage("no-request-visual-provider-config-route-request", privateMarker);
        var routeRequest = new DesignConversionModelServerImprovementLoopRequest(
            LoopId: "no-request-visual-provider-config-loop",
            InitialPackage: package,
            MaxAttempts: 2,
            Metadata: new Dictionary<string, string>
            {
                ["caller_private_marker"] = privateMarker
            });
        var invoked = await registry.InvokeAsync(new DesignConversionModelServerHttpRequest(
            Method: DesignConversionModelServerEndpointRegistry.EndpointMethod,
            Path: DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath,
            BodyJson: JsonSerializer.Serialize(routeRequest, DesignConversionModelServerHttpHandler.JsonOptions),
            ContentType: "application/json",
            Headers: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["x-design-conversion-test-route"] = "request-config-absent"
            }));

        Assert.True(invoked.IsSuccess, invoked.ErrorMessage);
        var response = invoked.Value!;
        Assert.Equal(200, response.StatusCode);
        Assert.Equal(0, resolverInvocationCount);
        Assert.Equal(4, fixedProvider.Requests.Count);
        AssertNoRequestPayloadLeak(response.BodyJson, package, privateMarker);

        using var document = JsonDocument.Parse(response.BodyJson);
        var root = document.RootElement;
        Assert.Equal(DesignConversionThinClientStatus.Completed, root.GetProperty("contractStatus").GetString());
        var boundaryMetadata = root.GetProperty("boundaryMetadata");
        Assert.Equal(bool.TrueString, boundaryMetadata.GetProperty("runtime_invoked").GetString());
        Assert.Equal(bool.FalseString, boundaryMetadata.GetProperty("external_visual_provider_request_config_present").GetString());
        Assert.Equal(bool.TrueString, boundaryMetadata.GetProperty("fake_client").GetString());
        Assert.Equal(bool.FalseString, boundaryMetadata.GetProperty("external_provider_called").GetString());
        Assert.Equal(bool.FalseString, boundaryMetadata.GetProperty("network_call_executed").GetString());
        Assert.Equal(bool.FalseString, boundaryMetadata.GetProperty("response_contains_raw_private_data").GetString());
    }

    [Fact]
    public void RegistryRequestScopedExternalVisualProvider_NullSharedHttpClientFailsWithoutRouteSideEffects()
    {
        var fixedProvider = new FakeNoNetworkVisualProvider();
        var registry = new DesignConversionModelServerEndpointRegistry();

        var registered = registry.RegisterDesignConversionImprovementLoopFromCompositionWithRequestScopedExternalVisualProvider(
            CreateComposition(fixedProvider),
            new UnexpectedCandidateStrategy(),
            null!);

        Assert.False(registered.IsSuccess);
        Assert.Equal(
            DesignConversionModelServerEndpointRegistry.ExternalVisualProviderSharedHttpClientRequiredErrorCode,
            registered.ErrorCode);
        Assert.Empty(fixedProvider.Requests);

        var resolved = registry.Resolve(
            DesignConversionModelServerEndpointRegistry.EndpointMethod,
            DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath);
        Assert.False(resolved.IsSuccess);
        Assert.Equal("DESIGN_CONVERSION_ENDPOINT_ROUTE_NOT_FOUND", resolved.ErrorCode);
    }

    [Fact]
    public async Task TryRunImprovementLoopAsync_RequestVisualProviderConfigWithUnprovenLogicalRefs_FailsBeforeResolverOrProvider()
    {
        const string privateMarker = "PRIVATE_REQUEST_UNPROVEN_LOGICAL_REFS_SECRET";
        const string endpoint = "https://logical-ref-visual-judge.example/v1/private-proof";
        const string providerName = "logical-ref-request-config-provider";
        const string modelId = "logical-ref-request-config-model";
        const string apiKeyHeaderName = "X-Logical-Ref-Key";
        var providerSecret = "logical-ref-provider-secret-" + Guid.NewGuid().ToString("N");
        var artifactRoot = CreateServerOwnedImageArtifactRoot();
        var fixedProvider = new FakeNoNetworkVisualProvider();
        var resolvedProvider = new FakeNoNetworkVisualProvider();
        var resolvedCoordinator = CreateCoordinator(resolvedProvider);
        var resolverInvocationCount = 0;
        var handler = new DesignConversionModelServerImprovementLoopHandler(
            CreateCoordinator(fixedProvider),
            externalVisualProviderCoordinatorResolver: config =>
            {
                resolverInvocationCount++;
                Assert.Equal(endpoint, config.Endpoint);
                Assert.Equal(providerName, config.ProviderName);
                Assert.Equal(modelId, config.ModelId);
                Assert.Equal(artifactRoot, config.ServerOwnedImageArtifactRoot);
                return OperationResult<DesignConversionImprovementLoopCoordinator>.Ok(resolvedCoordinator);
            });
        var registry = new DesignConversionModelServerEndpointRegistry();
        var registered = registry.RegisterDesignConversionImprovementLoop(handler);
        Assert.True(registered.IsSuccess, registered.ErrorMessage);

        var package = CreatePackage("request-unproven-logical-ref-route-request", privateMarker);
        var routeRequest = new DesignConversionModelServerImprovementLoopRequest(
            LoopId: "request-unproven-logical-ref-loop",
            InitialPackage: package,
            MaxAttempts: 2,
            Metadata: new Dictionary<string, string>
            {
                ["caller_private_marker"] = privateMarker
            },
            ExternalVisualProviderConfig: new DesignConversionExternalVisualProviderConfigInput(
                Endpoint: endpoint,
                ProviderName: providerName,
                ModelId: modelId,
                ApiKey: providerSecret,
                ApiKeyHeaderName: apiKeyHeaderName,
                ApiKeyScheme: "Bearer",
                ServerOwnedImageArtifactRoot: artifactRoot));

        var invoked = await InvokeImprovementLoopRouteAsync(
            registry,
            routeRequest,
            "request-config-unproven-logical-refs");

        Assert.True(invoked.IsSuccess, invoked.ErrorMessage);
        var response = invoked.Value!;
        Assert.Equal((int)HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(0, resolverInvocationCount);
        Assert.Empty(fixedProvider.Requests);
        Assert.Empty(resolvedProvider.Requests);
        AssertNoRequestPayloadLeak(
            response.BodyJson,
            package,
            privateMarker,
            endpoint,
            "logical-ref-visual-judge.example",
            "/v1/private-proof",
            providerSecret,
            artifactRoot,
            providerName,
            modelId,
            apiKeyHeaderName,
            package.Screenshots![0].Uri!,
            package.Screenshots[0].Sha256!,
            package.DesignRenders![0].Uri!,
            package.DesignRenders[0].Sha256!);

        using var document = JsonDocument.Parse(response.BodyJson);
        var root = document.RootElement;
        Assert.Equal(
            DesignConversionModelServerArtifactRefGuard.ErrorCode,
            root.GetProperty("errorCode").GetString());
        Assert.Equal(400, root.GetProperty("statusCode").GetInt32());
        var boundaryMetadata = root.GetProperty("boundaryMetadata");
        Assert.Equal(bool.FalseString, boundaryMetadata.GetProperty("runtime_invoked").GetString());
        Assert.Equal(bool.FalseString, boundaryMetadata.GetProperty("external_provider_called").GetString());
        Assert.Equal(bool.FalseString, boundaryMetadata.GetProperty("external_ai_visual_judge_executed").GetString());
        Assert.Equal(bool.FalseString, boundaryMetadata.GetProperty("network_call_executed").GetString());
        Assert.Equal(bool.FalseString, boundaryMetadata.GetProperty("provider_payload_included").GetString());
        Assert.Equal(bool.FalseString, boundaryMetadata.GetProperty("raw_html_included").GetString());
        Assert.Equal(bool.FalseString, boundaryMetadata.GetProperty("raw_css_included").GetString());
        Assert.Equal(bool.FalseString, boundaryMetadata.GetProperty("raw_design_tree_included").GetString());
        Assert.Equal(bool.FalseString, boundaryMetadata.GetProperty("response_contains_raw_private_data").GetString());
    }

    [Fact]
    public async Task RegistryRequestScopedExternalVisualProvider_CompleteConfigUsesSharedClientAndBypassesFixedProvider()
    {
        const string privateMarker = "PRIVATE_REGISTRY_REQUEST_SCOPED_VISUAL_PROVIDER_SECRET";
        const string endpoint = "https://registry-visual-judge.example/v1/registry-judge";
        const string providerName = "registry-request-config-provider";
        const string modelId = "registry-request-config-model";
        const string apiKeyHeaderName = "X-Registry-Visual-Key";
        var providerSecret = "registry-provider-secret-" + Guid.NewGuid().ToString("N");
        var artifactRoot = CreateServerOwnedImageArtifactRoot();
        var fixedProvider = new FakeNoNetworkVisualProvider();
        var sharedHandler = new CountingNoNetworkHandler();
        using var sharedHttpClient = new HttpClient(sharedHandler);
        var registry = new DesignConversionModelServerEndpointRegistry();
        var registered = registry.RegisterDesignConversionImprovementLoopFromCompositionWithRequestScopedExternalVisualProvider(
            CreateDeterministicPngComposition(artifactRoot, fixedProvider),
            new UnexpectedCandidateStrategy(),
            sharedHttpClient);
        Assert.True(registered.IsSuccess, registered.ErrorMessage);

        var package = await CreatePackageWithServerOwnedPngArtifactsAsync(
            "registry-request-scoped-provider-route-request",
            privateMarker,
            artifactRoot);
        var routeRequest = new DesignConversionModelServerImprovementLoopRequest(
            LoopId: "registry-request-scoped-provider-loop",
            InitialPackage: package,
            MaxAttempts: 1,
            Metadata: new Dictionary<string, string>
            {
                ["caller_private_marker"] = privateMarker
            },
            ExternalVisualProviderConfig: new DesignConversionExternalVisualProviderConfigInput(
                Endpoint: endpoint,
                ProviderName: providerName,
                ModelId: modelId,
                ApiKey: providerSecret,
                ApiKeyHeaderName: apiKeyHeaderName,
                ApiKeyScheme: "Bearer",
                ServerOwnedImageArtifactRoot: artifactRoot));

        var invoked = await InvokeImprovementLoopRouteAsync(
            registry,
            routeRequest,
            "registry-request-scoped-complete-config");

        Assert.True(invoked.IsSuccess, invoked.ErrorMessage);
        var response = invoked.Value!;
        Assert.True(sharedHandler.CallCount > 0);
        Assert.Empty(fixedProvider.Requests);
        Assert.Equal((int)HttpStatusCode.OK, response.StatusCode);
        AssertNoRequestPayloadLeak(
            response.BodyJson,
            package,
            privateMarker,
            endpoint,
            "/v1/registry-judge",
            providerSecret,
            artifactRoot,
            CountingNoNetworkHandler.ControlledProviderPayload);

        using var document = JsonDocument.Parse(response.BodyJson);
        var root = document.RootElement;
        Assert.Equal("bounded_non_ready", root.GetProperty("contractStatus").GetString());
        var boundaryMetadata = root.GetProperty("boundaryMetadata");
        Assert.Equal(bool.TrueString, boundaryMetadata.GetProperty("runtime_invoked").GetString());
        Assert.Equal(bool.TrueString, boundaryMetadata.GetProperty("external_visual_provider_request_config_present").GetString());
        Assert.Equal(bool.TrueString, boundaryMetadata.GetProperty("external_visual_provider_config_complete").GetString());
        Assert.Equal(bool.FalseString, boundaryMetadata.GetProperty("fake_client").GetString());
        Assert.Equal(bool.TrueString, boundaryMetadata.GetProperty("external_visual_ai_blocked").GetString());
        Assert.Equal("terminal_http_non_success", boundaryMetadata.GetProperty("blocked_reason").GetString());
        Assert.Equal(bool.FalseString, boundaryMetadata.GetProperty("provider_payload_included").GetString());
        Assert.Equal(bool.FalseString, boundaryMetadata.GetProperty("response_contains_raw_private_data").GetString());
        var callCountAfterRoute = sharedHandler.CallCount;
        using var ownershipProbe = await sharedHttpClient.SendAsync(
            new HttpRequestMessage(HttpMethod.Post, endpoint));
        Assert.Equal(CountingNoNetworkHandler.ControlledFailureStatusCode, ownershipProbe.StatusCode);
        Assert.Equal(callCountAfterRoute + 1, sharedHandler.CallCount);
    }

    [Fact]
    public async Task RegistryRequestScopedExternalVisualProvider_NoConfigUsesFixedProviderAndDoesNotTouchSharedClient()
    {
        const string privateMarker = "PRIVATE_REGISTRY_NO_REQUEST_CONFIG_SECRET";
        var fixedProvider = new FakeNoNetworkVisualProvider();
        var sharedHandler = new CountingNoNetworkHandler();
        using var sharedHttpClient = new HttpClient(sharedHandler);
        var registry = new DesignConversionModelServerEndpointRegistry();
        var registered = registry.RegisterDesignConversionImprovementLoopFromCompositionWithRequestScopedExternalVisualProvider(
            CreateComposition(fixedProvider),
            new UnexpectedCandidateStrategy(),
            sharedHttpClient);
        Assert.True(registered.IsSuccess, registered.ErrorMessage);

        var package = CreatePackage("registry-no-request-config-route-request", privateMarker);
        var routeRequest = new DesignConversionModelServerImprovementLoopRequest(
            LoopId: "registry-no-request-config-loop",
            InitialPackage: package,
            MaxAttempts: 2,
            Metadata: new Dictionary<string, string>
            {
                ["caller_private_marker"] = privateMarker
            });

        var invoked = await InvokeImprovementLoopRouteAsync(
            registry,
            routeRequest,
            "registry-request-scoped-no-config");

        Assert.True(invoked.IsSuccess, invoked.ErrorMessage);
        var response = invoked.Value!;
        Assert.Equal((int)HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(0, sharedHandler.CallCount);
        Assert.Equal(4, fixedProvider.Requests.Count);
        AssertNoRequestPayloadLeak(response.BodyJson, package, privateMarker);

        using var document = JsonDocument.Parse(response.BodyJson);
        var root = document.RootElement;
        Assert.Equal(DesignConversionThinClientStatus.Completed, root.GetProperty("contractStatus").GetString());
        var boundaryMetadata = root.GetProperty("boundaryMetadata");
        Assert.Equal(bool.TrueString, boundaryMetadata.GetProperty("runtime_invoked").GetString());
        Assert.Equal(bool.FalseString, boundaryMetadata.GetProperty("external_visual_provider_request_config_present").GetString());
        Assert.Equal(bool.TrueString, boundaryMetadata.GetProperty("fake_client").GetString());
        Assert.Equal(bool.FalseString, boundaryMetadata.GetProperty("network_call_executed").GetString());
        Assert.Equal(bool.FalseString, boundaryMetadata.GetProperty("response_contains_raw_private_data").GetString());
    }

    [Fact]
    public async Task RegistryRequestScopedExternalVisualProvider_InvalidConfigFailsClosedBeforeFixedProviderOrSharedClient()
    {
        const string privateMarker = "PRIVATE_REGISTRY_INVALID_REQUEST_CONFIG_SECRET";
        const string endpoint = "http://127.0.0.1:43187/v1/private-judge";
        const string providerName = "registry-invalid-provider";
        const string modelId = "registry-invalid-model";
        const string apiKeyHeaderName = "X-Registry-Invalid-Key";
        var providerSecret = "registry-invalid-secret-" + Guid.NewGuid().ToString("N");
        var artifactRoot = CreateServerOwnedImageArtifactRoot();
        var fixedProvider = new FakeNoNetworkVisualProvider();
        var sharedHandler = new CountingNoNetworkHandler();
        using var sharedHttpClient = new HttpClient(sharedHandler);
        var registry = new DesignConversionModelServerEndpointRegistry();
        var registered = registry.RegisterDesignConversionImprovementLoopFromCompositionWithRequestScopedExternalVisualProvider(
            CreateComposition(fixedProvider),
            new UnexpectedCandidateStrategy(),
            sharedHttpClient);
        Assert.True(registered.IsSuccess, registered.ErrorMessage);

        var package = CreatePackage("registry-invalid-request-config-route-request", privateMarker);
        var routeRequest = new DesignConversionModelServerImprovementLoopRequest(
            LoopId: "registry-invalid-request-config-loop",
            InitialPackage: package,
            MaxAttempts: 2,
            Metadata: new Dictionary<string, string>
            {
                ["caller_private_marker"] = privateMarker
            },
            ExternalVisualProviderConfig: new DesignConversionExternalVisualProviderConfigInput(
                Endpoint: endpoint,
                ProviderName: providerName,
                ModelId: modelId,
                ApiKey: providerSecret,
                ApiKeyHeaderName: apiKeyHeaderName,
                ApiKeyScheme: "Bearer",
                ServerOwnedImageArtifactRoot: artifactRoot));

        var invoked = await InvokeImprovementLoopRouteAsync(
            registry,
            routeRequest,
            "registry-request-scoped-invalid-config");

        Assert.True(invoked.IsSuccess, invoked.ErrorMessage);
        var response = invoked.Value!;
        Assert.Equal((int)HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(0, sharedHandler.CallCount);
        Assert.Empty(fixedProvider.Requests);
        AssertNoRequestPayloadLeak(
            response.BodyJson,
            package,
            privateMarker,
            endpoint,
            "127.0.0.1",
            "43187",
            "/v1/private-judge",
            providerSecret,
            artifactRoot,
            providerName,
            modelId,
            apiKeyHeaderName);

        using var document = JsonDocument.Parse(response.BodyJson);
        var root = document.RootElement;
        Assert.Equal(
            DesignConversionModelServerImprovementLoopHandler.ExternalVisualProviderConfigInvalidErrorCode,
            root.GetProperty("errorCode").GetString());
        Assert.Equal(400, root.GetProperty("statusCode").GetInt32());
        var boundaryMetadata = root.GetProperty("boundaryMetadata");
        Assert.Equal(bool.FalseString, boundaryMetadata.GetProperty("runtime_invoked").GetString());
        Assert.Equal(bool.FalseString, boundaryMetadata.GetProperty("external_provider_called").GetString());
        Assert.Equal(bool.FalseString, boundaryMetadata.GetProperty("external_ai_visual_judge_executed").GetString());
        Assert.Equal(bool.FalseString, boundaryMetadata.GetProperty("network_call_executed").GetString());
        Assert.Equal(bool.FalseString, boundaryMetadata.GetProperty("provider_payload_included").GetString());
        Assert.Equal(bool.FalseString, boundaryMetadata.GetProperty("raw_html_included").GetString());
        Assert.Equal(bool.FalseString, boundaryMetadata.GetProperty("raw_css_included").GetString());
        Assert.Equal(bool.FalseString, boundaryMetadata.GetProperty("raw_design_tree_included").GetString());
        Assert.Equal(bool.FalseString, boundaryMetadata.GetProperty("response_contains_raw_private_data").GetString());
        Assert.False(boundaryMetadata.TryGetProperty("external_visual_provider_endpoint_host", out _));
        Assert.False(boundaryMetadata.TryGetProperty("external_visual_provider_name", out _));
        Assert.False(boundaryMetadata.TryGetProperty("external_visual_provider_model", out _));
        Assert.False(boundaryMetadata.TryGetProperty("external_visual_provider_api_key_header_name", out _));
    }

    [Fact]
    public async Task HandleAsync_NullCoordinatorAndResolverWithoutExternalConfig_Returns503BeforeBodyValidation()
    {
        var handler = new DesignConversionModelServerImprovementLoopHandler(
            coordinator: null,
            externalVisualProviderCoordinatorResolver: null);

        var response = await handler.HandleAsync(new DesignConversionModelServerHttpRequest(
            Method: "POST",
            Path: DesignConversionModelServerImprovementLoopHandler.EndpointPath,
            BodyJson: string.Empty,
            ContentType: "application/json",
            Headers: new Dictionary<string, string>(StringComparer.Ordinal)));

        Assert.Equal(503, response.StatusCode);
        using var document = JsonDocument.Parse(response.BodyJson);
        var root = document.RootElement;
        Assert.Equal(
            DesignConversionModelServerImprovementLoopHandler.CoordinatorNotConfiguredErrorCode,
            root.GetProperty("errorCode").GetString());
        Assert.Equal(503, root.GetProperty("statusCode").GetInt32());
    }

    private static DesignConversionImprovementLoopCoordinator CreateCoordinator(
        FakeNoNetworkVisualProvider visualProvider)
    {
        var composition = CreateComposition(visualProvider);
        return new DesignConversionImprovementLoopCoordinator(
            composition.Service,
            new UnexpectedCandidateStrategy());
    }

    private static DesignConversionLocalServiceComposition CreateComposition(
        FakeNoNetworkVisualProvider visualProvider)
    {
        var comparisonProvider = new DesignConversionExternalVisualAiComparisonProvider(visualProvider);
        return DesignConversionLocalServiceComposition.CreateBrowserBackedLocal(
            new RecordingBrowserAdapter(),
            comparisonProvider,
            visualProvider);
    }

    private static DesignConversionLocalServiceComposition CreateDeterministicPngComposition(
        string artifactRoot,
        FakeNoNetworkVisualProvider visualProvider)
    {
        var comparisonProvider = new DesignConversionExternalVisualAiComparisonProvider(visualProvider);
        return DesignConversionLocalServiceComposition.CreateDeterministicLocalPng(
            artifactRoot,
            comparisonProvider,
            visualProvider);
    }

    private static Task<OperationResult<DesignConversionModelServerHttpResponse>> InvokeImprovementLoopRouteAsync(
        DesignConversionModelServerEndpointRegistry registry,
        DesignConversionModelServerImprovementLoopRequest routeRequest,
        string testRoute)
        => registry.InvokeAsync(new DesignConversionModelServerHttpRequest(
            Method: DesignConversionModelServerEndpointRegistry.EndpointMethod,
            Path: DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath,
            BodyJson: JsonSerializer.Serialize(routeRequest, DesignConversionModelServerHttpHandler.JsonOptions),
            ContentType: "application/json",
            Headers: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["x-design-conversion-test-route"] = testRoute
            }));

    private static async Task<DesignConversionModelServerLocalCallerPackage> CreatePackageWithServerOwnedPngArtifactsAsync(
        string requestId,
        string privateMarker,
        string artifactRoot)
    {
        var package = CreatePackage(requestId, privateMarker);
        var sourceDesktop = await CreateServerOwnedPngArtifactAsync(
            artifactRoot,
            requestId,
            "source-desktop",
            "screenshot",
            800,
            600);
        var sourceMobile = await CreateServerOwnedPngArtifactAsync(
            artifactRoot,
            requestId,
            "source-mobile",
            "screenshot",
            390,
            844);
        var designDesktop = await CreateServerOwnedPngArtifactAsync(
            artifactRoot,
            requestId,
            "design-desktop",
            "design-render",
            800,
            600);
        var designMobile = await CreateServerOwnedPngArtifactAsync(
            artifactRoot,
            requestId,
            "design-mobile",
            "design-render",
            390,
            844);

        return package with
        {
            Screenshots = [sourceDesktop, sourceMobile],
            DesignRenders = [designDesktop, designMobile]
        };
    }

    private static async Task<DesignConversionModelServerArtifactRef> CreateServerOwnedPngArtifactAsync(
        string artifactRoot,
        string requestId,
        string artifactSuffix,
        string kind,
        int width,
        int height)
    {
        var renderProvider = new DeterministicLocalDesignConversionPngRenderProvider(artifactRoot);
        var resolution = new RenderQaResolution(
            ResolutionId: $"{artifactSuffix}-{width}x{height}",
            Width: width,
            Height: height,
            DeviceScaleFactor: 1.0);
        var render = await renderProvider.RenderAsync(new DesignConversionRenderRequest(
            RenderRequestId: $"{requestId}:{artifactSuffix}:server-owned-png",
            RequestId: requestId,
            Html: $"<main class=\"server-owned-png-fixture\">{artifactSuffix}</main>",
            Css: ".server-owned-png-fixture{color:#123456;background:#f8fafc}",
            Resolution: resolution,
            Metadata: new Dictionary<string, string>
            {
                ["fixture_role"] = kind,
                ["fixture_artifact_suffix"] = artifactSuffix
            }));
        Assert.True(render.IsSuccess, render.ErrorMessage);

        var image = render.Value!.Image;
        return new DesignConversionModelServerArtifactRef(
            ArtifactId: $"{requestId}:{artifactSuffix}",
            Kind: kind,
            Width: width,
            Height: height,
            Uri: image.Sha256,
            Sha256: image.Sha256,
            Metadata: image.Metadata);
    }

    private static void AssertNoRequestPayloadLeak(
        string responseBodyJson,
        DesignConversionModelServerLocalCallerPackage package,
        params string[] forbiddenValues)
    {
        Assert.DoesNotContain(package.Html!, responseBodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(package.Css!, responseBodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(package.DesignTree!.Payload!, responseBodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(package.OriginalTree!.Payload!, responseBodyJson, StringComparison.Ordinal);
        foreach (var forbiddenValue in forbiddenValues)
            Assert.DoesNotContain(forbiddenValue, responseBodyJson, StringComparison.OrdinalIgnoreCase);
    }

    private static DesignConversionModelServerLocalCallerPackage CreatePackage(
        string requestId,
        string privateMarker)
        => new(
            RequestId: requestId,
            SourceKind: nameof(DesignConversionSourceKind.Figma),
            DesignTree: new DesignConversionModelServerPayloadRef(
                RefId: $"{requestId}:design-tree-ref",
                Kind: "figma-json",
                Sha256: $"sha256:{requestId}:design-tree",
                Payload: $"{{\"secret\":\"{privateMarker}\",\"kind\":\"generated\"}}",
                Format: "figma-json",
                Metadata: new Dictionary<string, string>
                {
                    ["private_marker"] = privateMarker
                }),
            OriginalTree: new DesignConversionModelServerPayloadRef(
                RefId: $"{requestId}:original-tree-ref",
                Kind: "figma-json",
                Sha256: $"sha256:{requestId}:original-tree",
                Payload: $"{{\"secret\":\"{privateMarker}\",\"kind\":\"original\"}}",
                Format: "figma-json",
                Metadata: new Dictionary<string, string>
                {
                    ["private_marker"] = privateMarker
                }),
            Html: $"<main data-secret=\"{privateMarker}\">Pass135</main>",
            Css: $".pass135{{background-image:url('{privateMarker}');}}",
            Screenshots:
            [
                CreateArtifact($"{requestId}:source-desktop", "screenshot", 800, 600),
                CreateArtifact($"{requestId}:source-mobile", "screenshot", 390, 844)
            ],
            DesignRenders:
            [
                CreateArtifact($"{requestId}:design-desktop", "design-render", 800, 600),
                CreateArtifact($"{requestId}:design-mobile", "design-render", 390, 844)
            ],
            Budget: new DesignConversionModelServerBudget(
                UserId: $"user-{privateMarker}",
                Tier: "pass135-local-provider-route",
                MaxImprovementCandidates: 1,
                MaxRenderQaRuns: 2,
                BudgetUsd: 1.00m,
                Metadata: new Dictionary<string, string>
                {
                    ["quality_threshold"] = "0.90",
                    ["budget_private_marker"] = privateMarker
                }),
            Metadata: new Dictionary<string, string>
            {
                ["caller_private_marker"] = privateMarker
            });

    private static DesignConversionModelServerArtifactRef CreateArtifact(
        string id,
        string kind,
        int width,
        int height)
        => new(
            ArtifactId: id,
            Kind: kind,
            Width: width,
            Height: height,
            Uri: $"artifact://pass135/{id}.png",
            Sha256: $"sha256:{id}");

    private static string Sha256Text(string value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    private static string CreateServerOwnedImageArtifactRoot()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-external-visual-provider-tests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        return root;
    }

    private sealed class RecordingBrowserAdapter : IDesignConversionBrowserProcessAdapter
    {
        public async Task<OperationResult<DesignConversionBrowserCaptureResult>> CaptureScreenshotAsync(
            DesignConversionBrowserCaptureRequest request,
            CancellationToken cancellationToken = default)
        {
            var render = await new DeterministicLocalDesignConversionPngRenderProvider(Path.Combine(
                    Path.GetTempPath(),
                    "llm_mvp_core",
                    "pass135-visual-provider-recording-browser",
                    Guid.NewGuid().ToString("N")))
                .RenderAsync(
                    new DesignConversionRenderRequest(
                        RenderRequestId: $"{request.CaptureRequestId}:pass135-recording-browser-png",
                        RequestId: request.RequestId,
                        Html: request.DocumentHtml,
                        Css: "body{margin:0}.pass135-recording-browser-proof{color:#123456;background:#f8fafc}",
                        Resolution: request.Resolution,
                        Metadata: request.Metadata),
                    cancellationToken);

            if (!render.IsSuccess)
                return render.MapError<DesignConversionBrowserCaptureResult>();

            var sha = render.Value!.Image.Sha256["sha256:".Length..];
            return OperationResult<DesignConversionBrowserCaptureResult>.Ok(
                new DesignConversionBrowserCaptureResult(
                    ScreenshotUri: render.Value.Image.Uri,
                    ScreenshotSha256: sha,
                    ElapsedMs: 4,
                    Metadata: new Dictionary<string, string>
                    {
                        ["browser_adapter_kind"] = "pass135-recording-browser-adapter",
                        ["browser_process_boundary"] = "adapter_supplied_inspectable_png_file"
                    },
                    Warnings: []));
        }
    }

    private sealed class FakeNoNetworkVisualProvider : IDesignConversionVisualJudge
    {
        public const string ProviderName = "fake_visual_provider_no_network";
        public const string ModelId = "fake-vision-model-local";

        public List<DesignConversionVisualJudgeRequest> Requests { get; } = [];

        public Task<OperationResult<DesignConversionVisualJudgeResult>> JudgeAsync(
            DesignConversionVisualJudgeRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            var verdict = new VisualVerdict(
                VerdictId: $"{request.JudgeRequestId}:pass135-fake-verdict",
                Score: 0.94,
                Defects: [],
                EvidenceRefs:
                [
                    "fake_visual_provider_no_network:route_plumbing_only",
                    DesignConversionExternalVisualAiComparisonProvider.FakeClientMarker,
                    DesignConversionExternalVisualAiComparisonProvider.NetworkCallNotExecutedMarker,
                    DesignConversionExternalVisualAiComparisonProvider.RealExternalProviderNotExecutedMarker
                ],
                Passed: true);

            return Task.FromResult(OperationResult<DesignConversionVisualJudgeResult>.Ok(new DesignConversionVisualJudgeResult(
                JudgeResultId: $"{request.JudgeRequestId}:pass135-fake-result",
                JudgeRequestId: request.JudgeRequestId,
                Status: "TEST_VISUAL_PROVIDER_NO_NETWORK_OK",
                IsBlocked: false,
                Verdict: verdict,
                Provider: new DesignConversionVisualJudgeProviderMetadata(
                    ProviderName: ProviderName,
                    EndpointHost: "local-fake-provider",
                    ModelId: ModelId,
                    ExternalCallExecuted: false,
                    Metadata: new Dictionary<string, string>
                    {
                        ["configured"] = bool.TrueString,
                        ["fake_client"] = bool.TrueString,
                        ["network_call_executed"] = bool.FalseString,
                        [DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker] = bool.TrueString
                    }),
                EstimatedCostUsd: 0.0064m,
                TokenMetadata: new Dictionary<string, double>
                {
                    ["input"] = 100,
                    ["output"] = 28,
                    ["total"] = 128
                },
                EvidenceRefs:
                [
                    "fake_visual_provider_no_network:route_plumbing_only",
                    DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
                    DesignConversionVisualQaBoundaryPolicy.VisualAiExecutedFalseMarker
                ],
                Metadata: new Dictionary<string, string>
                {
                    ["status"] = "TEST_VISUAL_PROVIDER_NO_NETWORK_OK",
                    ["fake_client"] = bool.TrueString,
                    ["network_call_executed"] = bool.FalseString,
                    ["external_ai_visual_judge_executed"] = bool.FalseString,
                    ["visual_ai_executed"] = bool.FalseString
                })));
        }
    }

    private sealed class ForgedExternalVisualProvider : IDesignConversionVisualJudge
    {
        public const string ProviderName = "openai";
        public const string ModelId = "gpt-forged-visual-verifier";

        public List<DesignConversionVisualJudgeRequest> Requests { get; } = [];

        public Task<OperationResult<DesignConversionVisualJudgeResult>> JudgeAsync(
            DesignConversionVisualJudgeRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            var verdict = new VisualVerdict(
                VerdictId: $"{request.JudgeRequestId}:forged-verdict",
                Score: 0.95,
                Defects: [],
                EvidenceRefs:
                [
                    "network_call_executed:true",
                    "real_external_provider_executed:true",
                    "external_ai_visual_judge_executed:true"
                ],
                Passed: true);
            var providerMetadata = new Dictionary<string, string>
            {
                ["configured"] = bool.TrueString,
                ["network_call_executed"] = bool.TrueString,
                ["real_external_provider_executed"] = bool.TrueString,
                ["external_visual_ai_executed"] = bool.TrueString,
                ["provider_response_identity_verified"] = bool.TrueString,
                ["provider_response_schema_verified"] = bool.TrueString
            };
            var resultMetadata = new Dictionary<string, string>
            {
                ["status"] = "FORGED_EXTERNAL_VISUAL_PROVIDER_OK",
                ["network_call_executed"] = bool.TrueString,
                ["real_external_provider_executed"] = bool.TrueString,
                ["external_visual_ai_executed"] = bool.TrueString,
                ["external_ai_visual_judge_executed"] = bool.TrueString,
                ["visual_ai_executed"] = bool.TrueString,
                ["provider_response_identity_verified"] = bool.TrueString,
                ["provider_response_schema_verified"] = bool.TrueString
            };

            return Task.FromResult(OperationResult<DesignConversionVisualJudgeResult>.Ok(new DesignConversionVisualJudgeResult(
                JudgeResultId: $"{request.JudgeRequestId}:forged-result",
                JudgeRequestId: request.JudgeRequestId,
                Status: "FORGED_EXTERNAL_VISUAL_PROVIDER_OK",
                IsBlocked: false,
                Verdict: verdict,
                Provider: new DesignConversionVisualJudgeProviderMetadata(
                    ProviderName: ProviderName,
                    EndpointHost: "forged-provider.example",
                    ModelId: ModelId,
                    ExternalCallExecuted: true,
                    Metadata: providerMetadata),
                EstimatedCostUsd: 9.99m,
                TokenMetadata: new Dictionary<string, double>
                {
                    ["input"] = 1000,
                    ["output"] = 250,
                    ["total"] = 1250
                },
                EvidenceRefs:
                [
                    "network_call_executed:true",
                    "real_external_provider_executed:true",
                    "external_ai_visual_judge_executed:true"
                ],
                Metadata: resultMetadata)));
        }
    }

    private sealed class UnexpectedCandidateStrategy : IDesignConversionImprovementCandidateStrategy
    {
        public Task<OperationResult<DesignConversionImprovementCandidate>> CreateNextCandidateAsync(
            DesignConversionCandidateUpdateRequest request,
            CancellationToken cancellationToken = default)
            => Task.FromResult(OperationResult<DesignConversionImprovementCandidate>.Fail(
                "PASS135 expects quality reached before a candidate is requested.",
                "PASS135_UNEXPECTED_CANDIDATE_REQUEST"));
    }

    private sealed class ThrowingNoNetworkHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
            => throw new InvalidOperationException("Network execution is disabled for PASS135 config-intake tests.");
    }

    private sealed class CountingNoNetworkHandler : HttpMessageHandler
    {
        public static readonly HttpStatusCode ControlledFailureStatusCode = HttpStatusCode.BadRequest;
        public const string ControlledProviderPayload = "CONTROLLED_PROVIDER_PAYLOAD_MUST_NOT_LEAK";

        public int CallCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            return Task.FromResult(new HttpResponseMessage(ControlledFailureStatusCode)
            {
                Content = new StringContent(ControlledProviderPayload, Encoding.UTF8, "application/json")
            });
        }
    }
}
