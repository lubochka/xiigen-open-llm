// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using DesignConversionDomain;

public sealed class DesignConversionVisualAiComparisonProviderTests
{
    [Fact]
    public async Task RunAsync_NoConfiguredProviderReturnsBlockedMetadataWithoutRawPayload()
    {
        var comparisonProvider = new DesignConversionExternalVisualAiComparisonProvider();
        var probe = new DesignConversionLocalRenderQaProbe(new RecordingRenderProvider(), comparisonProvider);
        var request = CreateProbeRequest("pass121-no-key-blocked");

        var result = await probe.RunAsync(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.False(result.Value!.Passed);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["external_visual_ai_configured"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["external_visual_ai_executed"]);
        Assert.Equal(bool.TrueString, result.Value.BoundaryMetadata["external_visual_ai_blocked"]);
        Assert.Equal("not_configured", result.Value.BoundaryMetadata["blocked_reason"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["network_call_executed"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["fake_client"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["real_external_provider_executed"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["visual_ai_executed"]);
        Assert.Equal(bool.TrueString, result.Value.BoundaryMetadata["visual_ai_not_executed"]);
        Assert.Equal("0", result.Value.BoundaryMetadata["estimated_cost_usd"]);
        Assert.Equal(DesignConversionVisualAiProviderSelectionPolicy.PolicyName, result.Value.BoundaryMetadata[DesignConversionVisualAiProviderSelectionPolicy.ProviderSelectionPolicyKey]);
        Assert.Equal("auto", result.Value.BoundaryMetadata[DesignConversionVisualAiProviderSelectionPolicy.RequestedProviderKey]);
        Assert.Equal("none", result.Value.BoundaryMetadata[DesignConversionVisualAiProviderSelectionPolicy.SelectedProviderKey]);
        Assert.Equal("not_configured", result.Value.BoundaryMetadata[DesignConversionVisualAiProviderSelectionPolicy.ProviderModeKey]);
        Assert.Equal("fallback_unavailable", result.Value.BoundaryMetadata[DesignConversionVisualAiProviderSelectionPolicy.ProviderFallbackStatusKey]);
        Assert.Equal(
            DesignConversionVisualQaBoundaryPolicy.ExternalProviderNotConfiguredMarker,
            result.Value.BoundaryMetadata[DesignConversionVisualAiProviderSelectionPolicy.ProviderFallbackReasonKey]);
        Assert.Equal("3", result.Value.BoundaryMetadata[DesignConversionVisualAiProviderSelectionPolicy.UnavailableProviderCountKey]);
        Assert.Equal("claude,openai,gemini", result.Value.BoundaryMetadata[DesignConversionVisualAiProviderSelectionPolicy.UnavailableProvidersKey]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata[DesignConversionVisualAiProviderSelectionPolicy.RealCloudProviderClaimedKey]);
        Assert.All(result.Value.Viewports, viewport =>
        {
            Assert.Equal(bool.FalseString, viewport.Comparison.Metadata["external_visual_ai_configured"]);
            Assert.Equal(bool.TrueString, viewport.Comparison.Metadata["external_visual_ai_blocked"]);
            Assert.Equal("not_configured", viewport.Comparison.Metadata["blocked_reason"]);
            Assert.Equal("none", viewport.Comparison.Metadata[DesignConversionVisualAiProviderSelectionPolicy.SelectedProviderKey]);
            Assert.Equal("not_configured", viewport.Comparison.Metadata[DesignConversionVisualAiProviderSelectionPolicy.ProviderModeKey]);
            Assert.Equal(bool.FalseString, viewport.Comparison.Metadata["raw_html_included"]);
            Assert.Equal(bool.FalseString, viewport.Comparison.Metadata["raw_css_included"]);
            Assert.Equal(bool.FalseString, viewport.Comparison.Metadata["provider_payload_included"]);
        });

        var serialized = JsonSerializer.Serialize(result.Value);
        Assert.DoesNotContain(request.Html, serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(request.Css, serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("PRIVATE_PASS121_SECRET", serialized, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RunAsync_MissingConfiguredProviderFailsClosedWithoutDeterministicComparisonFallback()
    {
        var comparisonProvider = new DesignConversionExternalVisualAiComparisonProvider(new NoConfiguredVisualJudge());
        var probe = new DesignConversionLocalRenderQaProbe(new RecordingRenderProvider(), comparisonProvider);
        var request = CreateProbeRequest("pass121-missing-provider-no-fallback");

        var directComparison = await comparisonProvider.CompareAsync(CreateComparisonRequest());
        var result = await probe.RunAsync(request);

        Assert.True(directComparison.IsSuccess, directComparison.ErrorMessage);
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.False(result.Value!.Passed);
        Assert.Equal(DesignConversionExternalVisualAiComparisonProvider.ProviderKind, directComparison.Value!.Metadata["provider_kind"]);
        Assert.Equal(bool.TrueString, directComparison.Value.Metadata["external_visual_ai_blocked"]);
        Assert.Equal(bool.FalseString, directComparison.Value.Metadata["external_visual_ai_configured"]);
        Assert.Equal(bool.FalseString, directComparison.Value.Metadata["visual_ai_provider_contract_executed"]);
        Assert.Equal(bool.FalseString, directComparison.Value.Metadata["network_call_executed"]);
        Assert.Contains(DesignConversionExternalVisualAiComparisonProvider.NoKeyBlockedMarker, directComparison.Value.EvidenceRefs);
        Assert.Contains("blocked_reason:not_configured", directComparison.Value.EvidenceRefs);
        Assert.DoesNotContain(DesignConversionVisualQaBoundaryPolicy.ExternalProviderCallFailedMarker, directComparison.Value.EvidenceRefs);
        Assert.DoesNotContain(
            DeterministicDesignConversionComparisonProvider.ProviderKind,
            JsonSerializer.Serialize(directComparison.Value),
            StringComparison.Ordinal);

        Assert.Equal(bool.TrueString, result.Value.BoundaryMetadata["external_visual_ai_blocked"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["external_visual_ai_configured"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["visual_ai_provider_contract_executed"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["network_call_executed"]);
        Assert.Equal("not_configured", result.Value.BoundaryMetadata["blocked_reason"]);
        Assert.DoesNotContain(
            DeterministicDesignConversionComparisonProvider.ProviderKind,
            JsonSerializer.Serialize(result.Value),
            StringComparison.Ordinal);
    }

    [Fact]
    public async Task CompareAsync_TerminalHttpBlockedPreservesExecutedProviderTruth()
    {
        var comparisonProvider = new DesignConversionExternalVisualAiComparisonProvider(new TerminalHttpBlockedVisualJudge());

        var result = await comparisonProvider.CompareAsync(CreateComparisonRequest());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var comparison = result.Value!;
        Assert.False(comparison.Verdict.Passed);
        Assert.Equal(0.0, comparison.Verdict.Score);
        Assert.Equal(bool.TrueString, comparison.Metadata["external_visual_ai_configured"]);
        Assert.Equal(bool.TrueString, comparison.Metadata["external_visual_ai_executed"]);
        Assert.Equal(bool.TrueString, comparison.Metadata["real_external_provider_executed"]);
        Assert.Equal(bool.TrueString, comparison.Metadata["visual_ai_provider_contract_executed"]);
        Assert.Equal(bool.TrueString, comparison.Metadata["external_visual_ai_blocked"]);
        Assert.Equal(bool.TrueString, comparison.Metadata["network_call_executed"]);
        Assert.Equal(
            DesignConversionExternalVisualAiComparisonProvider.TerminalHttpNonSuccessReason,
            comparison.Metadata["blocked_reason"]);
        Assert.Equal(bool.FalseString, comparison.Metadata["provider_response_identity_verified"]);
        Assert.Equal(bool.FalseString, comparison.Metadata["provider_response_schema_verified"]);
        Assert.Equal(
            bool.FalseString,
            comparison.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseIdentityVerifiedKey]);
        Assert.Equal(
            bool.FalseString,
            comparison.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseSchemaVerifiedKey]);
        Assert.Equal(
            "external_executed",
            comparison.Metadata[DesignConversionVisualAiProviderSelectionPolicy.ProviderModeKey]);
        Assert.Equal(
            "terminal_http_status",
            comparison.Metadata[DesignConversionVisualAiProviderSelectionPolicy.SelectedProviderStatusKey]);
        Assert.Equal(
            "not_needed",
            comparison.Metadata[DesignConversionVisualAiProviderSelectionPolicy.ProviderFallbackStatusKey]);
        Assert.Equal("0.0084", comparison.Metadata["estimated_cost_usd"]);
        Assert.Equal(1.0, comparison.Metrics["external_visual_ai_executed"]);
        Assert.Equal(1.0, comparison.Metrics["visual_ai_provider_contract_executed"]);
        Assert.Equal(0.0084, comparison.Metrics["estimated_cost_usd"], precision: 4);
        Assert.Contains(DesignConversionExternalVisualAiComparisonProvider.ProviderContractExecutedMarker, comparison.EvidenceRefs);
        Assert.Contains("network_call_executed:true", comparison.EvidenceRefs);
        Assert.Contains("real_external_provider_executed:true", comparison.EvidenceRefs);
        Assert.Contains(DesignConversionVisualQaBoundaryPolicy.ExternalProviderCallFailedMarker, comparison.EvidenceRefs);
        Assert.Contains("blocked_reason:terminal_http_non_success", comparison.EvidenceRefs);
        Assert.Contains("estimated_cost_usd:0.0084", comparison.EvidenceRefs);
        Assert.Contains(ExternalHttpDesignConversionVisualJudge.TerminalHttpStatusMarker, comparison.EvidenceRefs);
        Assert.DoesNotContain(DesignConversionExternalVisualAiComparisonProvider.NoKeyBlockedMarker, comparison.EvidenceRefs);
        Assert.DoesNotContain("blocked_reason:not_configured", comparison.EvidenceRefs);
        Assert.DoesNotContain(DesignConversionVisualQaBoundaryPolicy.ExternalProviderNotConfiguredMarker, comparison.EvidenceRefs);
    }

    public static IEnumerable<object[]> ConfiguredBlockedStatusCases()
    {
        yield return [ExternalHttpDesignConversionVisualJudge.StatusHttpBlocked, DesignConversionExternalVisualAiComparisonProvider.TerminalHttpNonSuccessReason, "external_executed", "terminal_http_status", true];
        yield return [ExternalHttpDesignConversionVisualJudge.StatusTransportBlocked, DesignConversionExternalVisualAiComparisonProvider.TransportFailureReason, "external_attempted", DesignConversionExternalVisualAiComparisonProvider.TransportFailureReason, false];
        yield return [ExternalHttpDesignConversionVisualJudge.StatusResponseReadBlocked, DesignConversionExternalVisualAiComparisonProvider.ResponseReadFailureReason, "external_response_received", DesignConversionExternalVisualAiComparisonProvider.ResponseReadFailureReason, true];
        yield return [ExternalHttpDesignConversionVisualJudge.StatusResponsePolicyBlocked, DesignConversionExternalVisualAiComparisonProvider.ResponsePolicyBlockReason, "external_response_received", "response_policy_blocked", true];
        yield return [ExternalHttpDesignConversionVisualJudge.StatusResponseValidationBlocked, DesignConversionExternalVisualAiComparisonProvider.ResponseValidationFailureReason, "external_response_received", DesignConversionExternalVisualAiComparisonProvider.ResponseValidationFailureReason, true];
    }

    [Theory]
    [MemberData(nameof(ConfiguredBlockedStatusCases))]
    public async Task CompareAsync_ConfiguredBlockedStatusesPreserveAttemptBoundaryInsteadOfNotConfigured(
        string status,
        string expectedBlockedReason,
        string expectedProviderMode,
        string expectedProviderStatus,
        bool networkExecutionVerified)
    {
        var comparisonProvider = new DesignConversionExternalVisualAiComparisonProvider(
            new BlockedVisualJudge(status, networkExecutionVerified));

        var result = await comparisonProvider.CompareAsync(CreateComparisonRequest());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var comparison = result.Value!;
        Assert.False(comparison.Verdict.Passed);
        Assert.Equal(bool.TrueString, comparison.Metadata["external_visual_ai_configured"]);
        Assert.Equal(bool.TrueString, comparison.Metadata["external_visual_ai_blocked"]);
        Assert.Equal(expectedBlockedReason, comparison.Metadata["blocked_reason"]);
        Assert.Equal("openai", comparison.Metadata[DesignConversionVisualAiProviderSelectionPolicy.SelectedProviderKey]);
        Assert.Equal(expectedProviderMode, comparison.Metadata[DesignConversionVisualAiProviderSelectionPolicy.ProviderModeKey]);
        Assert.Equal(expectedProviderStatus, comparison.Metadata[DesignConversionVisualAiProviderSelectionPolicy.SelectedProviderStatusKey]);
        Assert.Equal(networkExecutionVerified.ToString(), comparison.Metadata["network_call_executed"]);
        Assert.Equal(bool.TrueString, comparison.Metadata[ExternalHttpDesignConversionVisualJudge.NetworkCallAttemptedKey]);
        Assert.Equal(
            networkExecutionVerified.ToString(),
            comparison.Metadata[ExternalHttpDesignConversionVisualJudge.NetworkCallExecutionVerifiedKey]);
        Assert.Equal(networkExecutionVerified.ToString(), comparison.Metadata["visual_ai_provider_contract_executed"]);
        Assert.Equal(networkExecutionVerified.ToString(), comparison.Metadata["real_external_provider_executed"]);
        Assert.Equal(networkExecutionVerified.ToString(), comparison.Metadata["external_visual_ai_executed"]);
        Assert.Equal("0.0084", comparison.Metadata["estimated_cost_usd"]);
        Assert.Equal(
            DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostSourceFixedPerRequest,
            comparison.Metadata["cost_source"]);
        Assert.Equal("2", comparison.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountKey]);
        Assert.Equal(
            ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountSourceValue,
            comparison.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountSourceKey]);
        Assert.Equal("0.0042", comparison.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedCostPerAttemptUsdKey]);
        Assert.Equal("0.0084", comparison.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedTotalCostUsdKey]);
        Assert.Equal(
            ExternalHttpDesignConversionVisualJudge.ServerOwnedCostAccountingFixedPerAttempt,
            comparison.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedCostAccountingKey]);
        Assert.Contains(DesignConversionVisualQaBoundaryPolicy.ExternalProviderCallFailedMarker, comparison.EvidenceRefs);
        Assert.Contains($"blocked_reason:{expectedBlockedReason}", comparison.EvidenceRefs);
        Assert.DoesNotContain(DesignConversionExternalVisualAiComparisonProvider.NoKeyBlockedMarker, comparison.EvidenceRefs);
        Assert.DoesNotContain("blocked_reason:not_configured", comparison.EvidenceRefs);
        Assert.DoesNotContain(DesignConversionVisualQaBoundaryPolicy.ExternalProviderNotConfiguredMarker, comparison.EvidenceRefs);
        Assert.Equal(networkExecutionVerified ? 1.0 : 0.0, comparison.Metrics["real_external_provider_executed"]);
    }

    [Fact]
    public async Task CompareAsync_KnownBlockedStatusWithoutGroundedAttemptRemainsNotConfigured()
    {
        var comparisonProvider = new DesignConversionExternalVisualAiComparisonProvider(
            new BlockedVisualJudge(
                ExternalHttpDesignConversionVisualJudge.StatusTransportBlocked,
                networkExecutionVerified: false,
                groundedAttempt: false));

        var result = await comparisonProvider.CompareAsync(CreateComparisonRequest());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var comparison = result.Value!;
        Assert.Equal("not_configured", comparison.Metadata["blocked_reason"]);
        Assert.Equal(bool.FalseString, comparison.Metadata["external_visual_ai_configured"]);
        Assert.Equal("none", comparison.Metadata[DesignConversionVisualAiProviderSelectionPolicy.SelectedProviderKey]);
        Assert.Equal(bool.FalseString, comparison.Metadata["visual_ai_provider_contract_executed"]);
        Assert.Equal(bool.FalseString, comparison.Metadata["real_external_provider_executed"]);
        Assert.Equal(bool.FalseString, comparison.Metadata["network_call_executed"]);
        Assert.DoesNotContain(ExternalHttpDesignConversionVisualJudge.NetworkCallAttemptedKey, comparison.Metadata.Keys);
        Assert.Contains(DesignConversionExternalVisualAiComparisonProvider.NoKeyBlockedMarker, comparison.EvidenceRefs);
        Assert.Contains("blocked_reason:not_configured", comparison.EvidenceRefs);
    }

    [Fact]
    public async Task RunAsync_ConfiguredFakeClientMapsProviderScoreDefectsAndCostWithoutNetwork()
    {
        var fakeJudge = new FakeConfiguredVisualAiJudge();
        var comparisonProvider = new DesignConversionExternalVisualAiComparisonProvider(fakeJudge);
        var probe = new DesignConversionLocalRenderQaProbe(new RecordingRenderProvider(), comparisonProvider);
        var request = CreateProbeRequest("pass121-fake-client");

        var directComparison = await comparisonProvider.CompareAsync(CreateComparisonRequest());
        var result = await probe.RunAsync(request);

        Assert.True(directComparison.IsSuccess, directComparison.ErrorMessage);
        Assert.Equal("fake-vision-provider", directComparison.Value!.Metadata["provider_name"]);
        Assert.Equal("fake-vision-model-v1", directComparison.Value.Metadata["provider_model"]);
        Assert.Equal(bool.TrueString, directComparison.Value.Metadata["external_visual_ai_configured"]);
        Assert.Equal(bool.FalseString, directComparison.Value.Metadata["external_visual_ai_executed"]);
        Assert.Equal(bool.TrueString, directComparison.Value.Metadata["visual_ai_provider_contract_executed"]);
        Assert.Equal(bool.FalseString, directComparison.Value.Metadata["network_call_executed"]);
        Assert.Equal(bool.TrueString, directComparison.Value.Metadata["fake_client"]);
        Assert.Equal(bool.FalseString, directComparison.Value.Metadata["real_external_provider_executed"]);
        Assert.Equal("local_stub", directComparison.Value.Metadata[DesignConversionVisualAiProviderSelectionPolicy.SelectedProviderKey]);
        Assert.Equal("local_stub", directComparison.Value.Metadata[DesignConversionVisualAiProviderSelectionPolicy.ProviderModeKey]);
        Assert.Equal("local_stub", directComparison.Value.Metadata[DesignConversionVisualAiProviderSelectionPolicy.ProviderFallbackStatusKey]);
        Assert.Equal(bool.FalseString, directComparison.Value.Metadata[DesignConversionVisualAiProviderSelectionPolicy.RealCloudProviderClaimedKey]);
        Assert.Equal(0.0042, directComparison.Value.Metrics["estimated_cost_usd"], precision: 4);
        Assert.Equal(0.88, directComparison.Value.Verdict.Score);
        Assert.Single(directComparison.Value.Verdict.Defects);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.False(result.Value!.Passed);
        Assert.Equal("0.88", result.Value.BoundaryMetadata["average_score"]);
        Assert.Equal("1", result.Value.BoundaryMetadata["total_defect_count"]);
        Assert.Equal("fake-vision-provider", result.Value.BoundaryMetadata["provider_name"]);
        Assert.Equal("fake-vision-model-v1", result.Value.BoundaryMetadata["provider_model"]);
        Assert.Equal("0.0042", result.Value.BoundaryMetadata["estimated_cost_usd"]);
        Assert.Equal(bool.TrueString, result.Value.BoundaryMetadata["external_visual_ai_configured"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["external_visual_ai_executed"]);
        Assert.Equal(bool.TrueString, result.Value.BoundaryMetadata["visual_ai_provider_contract_executed"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["network_call_executed"]);
        Assert.Equal(bool.TrueString, result.Value.BoundaryMetadata["fake_client"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["real_external_provider_executed"]);
        Assert.Equal("local_stub", result.Value.BoundaryMetadata[DesignConversionVisualAiProviderSelectionPolicy.SelectedProviderKey]);
        Assert.Equal("local_stub", result.Value.BoundaryMetadata[DesignConversionVisualAiProviderSelectionPolicy.ProviderModeKey]);
        Assert.Equal("local_stub", result.Value.BoundaryMetadata[DesignConversionVisualAiProviderSelectionPolicy.ProviderFallbackStatusKey]);
        Assert.Equal("3", result.Value.BoundaryMetadata[DesignConversionVisualAiProviderSelectionPolicy.UnavailableProviderCountKey]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata[DesignConversionVisualAiProviderSelectionPolicy.RealCloudProviderClaimedKey]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["visual_ai_executed"]);
        Assert.Equal(bool.TrueString, result.Value.BoundaryMetadata["visual_ai_not_executed"]);
        Assert.Equal("spacing", Assert.Single(result.Value.Viewports).DefectCategories.Single());
        Assert.Equal(bool.TrueString, result.Value.Viewports[0].Comparison.Metadata["fake_client"]);
        Assert.Equal("fake-vision-provider", result.Value.Viewports[0].Comparison.Metadata["provider_name"]);
        Assert.Equal("0.0042", result.Value.Viewports[0].Comparison.Metadata["estimated_cost_usd"]);
        Assert.Equal(2, fakeJudge.Requests.Count);

        var serializedResult = JsonSerializer.Serialize(result.Value);
        var serializedJudgeRequests = JsonSerializer.Serialize(fakeJudge.Requests);
        Assert.DoesNotContain(request.Html, serializedResult, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(request.Css, serializedResult, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(request.Html, serializedJudgeRequests, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(request.Css, serializedJudgeRequests, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("PRIVATE_PASS121_SECRET", serializedResult, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("PRIVATE_PASS121_SECRET", serializedJudgeRequests, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("none")]
    [InlineData(" missing ")]
    [InlineData("UNCONFIGURED")]
    public void ProviderSelectionPolicy_NotConfiguredNamesNormalizeToAuto(string? providerName)
    {
        var metadata = DesignConversionVisualAiProviderSelectionPolicy.DescribeNotConfigured(providerName);

        Assert.Equal("auto", metadata[DesignConversionVisualAiProviderSelectionPolicy.RequestedProviderKey]);
        Assert.Equal("none", metadata[DesignConversionVisualAiProviderSelectionPolicy.SelectedProviderKey]);
        Assert.Equal("not_configured", metadata[DesignConversionVisualAiProviderSelectionPolicy.ProviderModeKey]);
        Assert.Equal(bool.FalseString, metadata[DesignConversionVisualAiProviderSelectionPolicy.RealCloudProviderClaimedKey]);
    }

    [Theory]
    [InlineData("claude", "claude")]
    [InlineData("anthropic", "claude")]
    [InlineData("openai", "openai")]
    [InlineData("gpt", "openai")]
    [InlineData("gemini", "gemini")]
    [InlineData("google", "gemini")]
    [InlineData("openai-vision", "openai")]
    [InlineData("gpt-4o", "openai")]
    [InlineData("anthropic-claude", "claude")]
    [InlineData("claude-3-5", "claude")]
    [InlineData("google-gemini", "gemini")]
    [InlineData("gemini-1.5", "gemini")]
    [InlineData("openai_vision", "openai")]
    [InlineData("claude.3.5", "claude")]
    [InlineData("google:gemini", "gemini")]
    public void ProviderSelectionPolicy_KnownAliasesRequireExactOrVendorFirstBoundary(
        string providerName,
        string expectedProvider)
    {
        var metadata = DesignConversionVisualAiProviderSelectionPolicy.DescribeExternalConfig(
            providerName,
            configComplete: true);

        Assert.Equal(expectedProvider, metadata[DesignConversionVisualAiProviderSelectionPolicy.RequestedProviderKey]);
        Assert.Equal(expectedProvider, metadata[DesignConversionVisualAiProviderSelectionPolicy.SelectedProviderKey]);
        Assert.Equal(providerName, metadata[DesignConversionVisualAiProviderSelectionPolicy.RequestedProviderNameKey]);
        Assert.Equal("external_configured_not_called", metadata[DesignConversionVisualAiProviderSelectionPolicy.ProviderModeKey]);
        Assert.Equal(bool.FalseString, metadata[DesignConversionVisualAiProviderSelectionPolicy.RealCloudProviderClaimedKey]);
    }

    [Theory]
    [InlineData("not-openai-proxy")]
    [InlineData("myanthropicproxy")]
    [InlineData("googleish")]
    [InlineData("claudeish")]
    [InlineData("provider-gemini")]
    [InlineData("gpt4o")]
    [InlineData("openaiish")]
    [InlineData("x-google")]
    public void ProviderSelectionPolicy_EmbeddedSpoofLikeNamesRemainCustom(string providerName)
    {
        var metadata = DesignConversionVisualAiProviderSelectionPolicy.DescribeExternalConfig(
            providerName,
            configComplete: true);

        Assert.Equal("custom", metadata[DesignConversionVisualAiProviderSelectionPolicy.RequestedProviderKey]);
        Assert.Equal("custom", metadata[DesignConversionVisualAiProviderSelectionPolicy.SelectedProviderKey]);
        Assert.Equal(providerName, metadata[DesignConversionVisualAiProviderSelectionPolicy.RequestedProviderNameKey]);
        Assert.Equal("external_configured_not_called", metadata[DesignConversionVisualAiProviderSelectionPolicy.ProviderModeKey]);
        Assert.Equal(bool.FalseString, metadata[DesignConversionVisualAiProviderSelectionPolicy.RealCloudProviderClaimedKey]);
    }

    [Theory]
    [InlineData("local")]
    [InlineData("local-openai")]
    [InlineData("openai-local")]
    [InlineData("fake-vision-provider")]
    [InlineData("anthropic-fake")]
    [InlineData("test-gemini")]
    [InlineData("visual-test-provider")]
    public void ProviderSelectionPolicy_LocalFakeTestMarkersFailClosedToLocalStub(string providerName)
    {
        var metadata = DesignConversionVisualAiProviderSelectionPolicy.DescribeExternalConfig(
            providerName,
            configComplete: true);

        Assert.Equal("local_stub", metadata[DesignConversionVisualAiProviderSelectionPolicy.RequestedProviderKey]);
        Assert.Equal("local_stub", metadata[DesignConversionVisualAiProviderSelectionPolicy.SelectedProviderKey]);
        Assert.Equal(providerName, metadata[DesignConversionVisualAiProviderSelectionPolicy.RequestedProviderNameKey]);
        Assert.Equal("external_configured_not_called", metadata[DesignConversionVisualAiProviderSelectionPolicy.ProviderModeKey]);
        Assert.Equal(bool.FalseString, metadata[DesignConversionVisualAiProviderSelectionPolicy.RealCloudProviderClaimedKey]);
    }

    private static DesignConversionLocalRenderQaProbeRequest CreateProbeRequest(string probeId)
        => new(
            ProbeId: probeId,
            RequestId: $"{probeId}-request",
            Html: "<main data-secret=\"PRIVATE_PASS121_SECRET\"><h1>Private design</h1></main>",
            Css: ".screen{background:url('PRIVATE_PASS121_SECRET');color:#123456;}",
            Viewports:
            [
                new RenderQaResolution("desktop", 1440, 900, 1.0)
            ],
            ExpectedDesignExports:
            [
                new DesignRenderArtifact(
                    ArtifactId: $"{probeId}-expected",
                    Kind: "design-export",
                    Width: 1440,
                    Height: 900,
                    Uri: $"artifact://pass121/{probeId}/expected.png",
                    Sha256: $"sha256:{Sha256Text($"{probeId}:expected")}",
                    Metadata: new Dictionary<string, string>())
            ],
            Metadata: new Dictionary<string, string>
            {
                ["pass"] = "pass121",
                ["private_note"] = "PRIVATE_PASS121_SECRET"
            });

    private static DesignConversionQaComparisonRequest CreateComparisonRequest()
        => new(
            ComparisonRequestId: "pass121-direct-comparison",
            RequestId: "pass121-direct-request",
            Expected: new RenderQaImageRecord(
                ImageId: "expected-shot",
                Label: "expected",
                Resolution: new RenderQaResolution("desktop", 1440, 900, 1.0),
                Uri: "artifact://pass121/expected.png",
                Sha256: "sha256:expected",
                Metadata: new Dictionary<string, string>()),
            Actual: new RenderQaImageRecord(
                ImageId: "actual-render",
                Label: "actual",
                Resolution: new RenderQaResolution("desktop", 1440, 900, 1.0),
                Uri: "artifact://pass121/actual.png",
                Sha256: "sha256:actual",
                Metadata: new Dictionary<string, string>()),
            Metadata: new Dictionary<string, string>
            {
                ["html"] = "<main>PRIVATE_PASS121_SECRET</main>",
                ["css"] = ".private{color:red}",
                ["safe_note"] = "pass121-direct"
            });

    private static string Sha256Text(string value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    private sealed class RecordingRenderProvider : IDesignConversionRenderProvider
    {
        public Task<OperationResult<DesignConversionRenderResult>> RenderAsync(
            DesignConversionRenderRequest request,
            CancellationToken cancellationToken = default)
        {
            var sha = Sha256Text(string.Join(
                "|",
                request.RenderRequestId,
                request.Resolution.ResolutionId,
                request.Html,
                request.Css));
            var image = new RenderQaImageRecord(
                ImageId: $"{request.RenderRequestId}:actual",
                Label: "actual-render",
                Resolution: request.Resolution,
                Uri: $"artifact://pass121/render/{sha}.png",
                Sha256: $"sha256:{sha}",
                Metadata: new Dictionary<string, string>(request.Metadata, StringComparer.Ordinal)
                {
                    ["provider_kind"] = "pass121-recording-render-provider",
                    ["browser_or_playwright_used"] = bool.FalseString,
                    ["external_ai_used"] = bool.FalseString
                });

            return Task.FromResult(OperationResult<DesignConversionRenderResult>.Ok(new DesignConversionRenderResult(
                RenderResultId: $"{request.RenderRequestId}:result",
                RenderRequestId: request.RenderRequestId,
                Image: image,
                ElapsedMs: 1,
                Warnings: ["pass121-recording-render-provider"])));
        }
    }

    private sealed class TerminalHttpBlockedVisualJudge : IDesignConversionVisualJudge
    {
        public Task<OperationResult<DesignConversionVisualJudgeResult>> JudgeAsync(
            DesignConversionVisualJudgeRequest request,
            CancellationToken cancellationToken = default)
        {
            var judgeResultId = $"{request.JudgeRequestId}:server-owned-visual-result:http-blocked";
            var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["configured"] = bool.TrueString,
                ["network_call_executed"] = bool.TrueString,
                ["external_ai_visual_judge_executed"] = bool.TrueString,
                ["provider_response_identity_verified"] = bool.FalseString,
                ["provider_response_schema_verified"] = bool.FalseString,
                ["cost_source"] = DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostSourceFixedPerRequest,
                [ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountKey] = "2",
                [ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountSourceKey] =
                    ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountSourceValue,
                [ExternalHttpDesignConversionVisualJudge.ServerCallReceiptKey] = bool.TrueString,
                [ExternalHttpDesignConversionVisualJudge.ServerCallReceiptIdKey] =
                    $"{request.JudgeRequestId}:server-visual-provider-call-receipt",
                [ExternalHttpDesignConversionVisualJudge.ServerCallReceiptExternalCallExecutedKey] = bool.TrueString,
                [ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseIdentityVerifiedKey] = bool.FalseString,
                [ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseSchemaVerifiedKey] = bool.FalseString
            };
            var evidenceRefs = new[]
            {
                ExternalHttpDesignConversionVisualJudge.StatusHttpBlocked,
                ExternalHttpDesignConversionVisualJudge.TerminalHttpStatusMarker,
                "terminal_http_status_code:500",
                "external_ai_visual_judge_executed:true"
            };
            var verdict = new VisualVerdict(
                VerdictId: $"{request.JudgeRequestId}:http-blocked-verdict",
                Score: 0.0,
                Defects:
                [
                    new VisualDefect(
                        DefectId: $"{request.JudgeRequestId}:http-blocked-defect",
                        Category: "external_visual_judge_http_non_success",
                        Severity: "blocker",
                        Description: "Synthetic terminal HTTP failure for downstream truth testing.",
                        X: 0,
                        Y: 0,
                        Width: 1,
                        Height: 1,
                        EvidenceImageIds: [request.ExpectedScreenshot.ImageId, request.RenderedImage.ImageId])
                ],
                EvidenceRefs: evidenceRefs,
                Passed: false);

            return Task.FromResult(OperationResult<DesignConversionVisualJudgeResult>.Ok(
                new DesignConversionVisualJudgeResult(
                    JudgeResultId: judgeResultId,
                    JudgeRequestId: request.JudgeRequestId,
                    Status: ExternalHttpDesignConversionVisualJudge.StatusHttpBlocked,
                    IsBlocked: true,
                    Verdict: verdict,
                    Provider: new DesignConversionVisualJudgeProviderMetadata(
                        ProviderName: "openai",
                        EndpointHost: "visual-provider.example.test",
                        ModelId: "vision-model",
                        ExternalCallExecuted: true,
                        Metadata: new Dictionary<string, string>(metadata, StringComparer.Ordinal)),
                    EstimatedCostUsd: 0.0084m,
                    TokenMetadata: new Dictionary<string, double>(),
                    EvidenceRefs: evidenceRefs,
                    Metadata: new Dictionary<string, string>(metadata, StringComparer.Ordinal))));
        }
    }

    private sealed class BlockedVisualJudge(
        string status,
        bool networkExecutionVerified,
        bool groundedAttempt = true) : IDesignConversionVisualJudge
    {
        public Task<OperationResult<DesignConversionVisualJudgeResult>> JudgeAsync(
            DesignConversionVisualJudgeRequest request,
            CancellationToken cancellationToken = default)
        {
            var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["configured"] = groundedAttempt.ToString()
            };
            if (groundedAttempt)
            {
                metadata["network_call_executed"] = networkExecutionVerified.ToString();
                metadata[ExternalHttpDesignConversionVisualJudge.NetworkCallAttemptedKey] = bool.TrueString;
                metadata[ExternalHttpDesignConversionVisualJudge.NetworkCallExecutionVerifiedKey] =
                    networkExecutionVerified.ToString();
                metadata[ExternalHttpDesignConversionVisualJudge.ProviderResponseReceivedKey] =
                    networkExecutionVerified.ToString();
                metadata["cost_source"] =
                    DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostSourceFixedPerRequest;
                metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountKey] = "2";
                metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountSourceKey] =
                    ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountSourceValue;
                metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedCostPerAttemptUsdKey] = "0.0042";
                metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedTotalCostUsdKey] = "0.0084";
                metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedCostAccountingKey] =
                    ExternalHttpDesignConversionVisualJudge.ServerOwnedCostAccountingFixedPerAttempt;
            }

            var verdict = new VisualVerdict(
                VerdictId: $"{request.JudgeRequestId}:blocked-verdict",
                Score: 0.0,
                Defects: [],
                EvidenceRefs: [status],
                Passed: false);

            return Task.FromResult(OperationResult<DesignConversionVisualJudgeResult>.Ok(
                new DesignConversionVisualJudgeResult(
                    JudgeResultId: $"{request.JudgeRequestId}:blocked-result",
                    JudgeRequestId: request.JudgeRequestId,
                    Status: status,
                    IsBlocked: true,
                    Verdict: verdict,
                    Provider: new DesignConversionVisualJudgeProviderMetadata(
                        ProviderName: "openai",
                        EndpointHost: "visual-provider.example.test",
                        ModelId: "vision-model",
                        ExternalCallExecuted: groundedAttempt,
                        Metadata: new Dictionary<string, string>(metadata, StringComparer.Ordinal)),
                    EstimatedCostUsd: 0.0084m,
                    TokenMetadata: new Dictionary<string, double>(),
                    EvidenceRefs: [status],
                    Metadata: new Dictionary<string, string>(metadata, StringComparer.Ordinal))));
        }
    }

    private sealed class FakeConfiguredVisualAiJudge : IDesignConversionVisualJudge
    {
        public List<DesignConversionVisualJudgeRequest> Requests { get; } = [];

        public Task<OperationResult<DesignConversionVisualJudgeResult>> JudgeAsync(
            DesignConversionVisualJudgeRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            var defect = new VisualDefect(
                DefectId: $"{request.JudgeRequestId}:fake-spacing",
                Category: "spacing",
                Severity: "minor",
                Description: "Fake client mapped a spacing defect for contract testing only.",
                X: 18,
                Y: 24,
                Width: 64,
                Height: 20,
                EvidenceImageIds: [request.ExpectedScreenshot.ImageId, request.RenderedImage.ImageId]);
            var verdict = new VisualVerdict(
                VerdictId: $"{request.JudgeRequestId}:fake-verdict",
                Score: 0.88,
                Defects: [defect],
                EvidenceRefs:
                [
                    "fake-visual-ai-client:mapping-only",
                    DesignConversionExternalVisualAiComparisonProvider.FakeClientMarker,
                    DesignConversionExternalVisualAiComparisonProvider.NetworkCallNotExecutedMarker,
                    DesignConversionExternalVisualAiComparisonProvider.RealExternalProviderNotExecutedMarker
                ],
                Passed: false);

            return Task.FromResult(OperationResult<DesignConversionVisualJudgeResult>.Ok(new DesignConversionVisualJudgeResult(
                JudgeResultId: $"{request.JudgeRequestId}:fake-result",
                JudgeRequestId: request.JudgeRequestId,
                Status: "TEST_VISUAL_AI_CLIENT_MAPPING_RECEIVED",
                IsBlocked: false,
                Verdict: verdict,
                Provider: new DesignConversionVisualJudgeProviderMetadata(
                    ProviderName: "fake-vision-provider",
                    EndpointHost: "local-fake-client",
                    ModelId: "fake-vision-model-v1",
                    ExternalCallExecuted: false,
                    Metadata: new Dictionary<string, string>
                    {
                        ["configured"] = bool.TrueString,
                        ["fake_client"] = bool.TrueString,
                        ["network_call_executed"] = bool.FalseString,
                        [DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker] = bool.TrueString
                    }),
                EstimatedCostUsd: 0.0042m,
                TokenMetadata: new Dictionary<string, double>
                {
                    ["input"] = 120,
                    ["output"] = 24,
                    ["total"] = 144
                },
                EvidenceRefs:
                [
                    "fake-visual-ai-client:mapping-only",
                    DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
                    DesignConversionVisualQaBoundaryPolicy.VisualAiExecutedFalseMarker
                ],
                Metadata: new Dictionary<string, string>
                {
                    ["status"] = "TEST_VISUAL_AI_CLIENT_MAPPING_RECEIVED",
                    ["fake_client"] = bool.TrueString,
                    ["network_call_executed"] = bool.FalseString,
                    ["external_ai_visual_judge_executed"] = bool.FalseString,
                    ["visual_ai_executed"] = bool.FalseString
                })));
        }
    }
}
