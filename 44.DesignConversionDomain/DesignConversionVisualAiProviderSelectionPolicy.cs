// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

public static class DesignConversionVisualAiProviderSelectionPolicy
{
    public const string PolicyName = "claude_openai_gemini_priority_refs_only_no_cloud_claim";
    public const string VerifiedCloudExecutionPolicyName = "server_owned_receipt_verified_cloud_execution_claim";
    public const string CandidateProviders = "claude,openai,gemini";
    public const string RequestedProviderKey = "visual_ai_requested_provider";
    public const string RequestedProviderNameKey = "visual_ai_requested_provider_name";
    public const string SelectedProviderKey = "visual_ai_selected_provider";
    public const string SelectedProviderNameKey = "visual_ai_selected_provider_name";
    public const string SelectedProviderStatusKey = "visual_ai_selected_provider_status";
    public const string ProviderModeKey = "visual_ai_provider_mode";
    public const string ProviderFallbackStatusKey = "visual_ai_provider_fallback_status";
    public const string ProviderFallbackReasonKey = "visual_ai_provider_fallback_reason";
    public const string UnavailableProviderCountKey = "visual_ai_unavailable_provider_count";
    public const string UnavailableProvidersKey = "visual_ai_unavailable_providers";
    public const string ProviderSelectionPolicyKey = "visual_ai_provider_selection_policy";
    public const string CandidateProvidersKey = "visual_ai_candidate_providers";
    public const string RealCloudProviderClaimedKey = "visual_ai_real_cloud_provider_claimed";

    public static readonly IReadOnlyList<string> PublicMetadataKeys =
    [
        ProviderSelectionPolicyKey,
        CandidateProvidersKey,
        RequestedProviderKey,
        RequestedProviderNameKey,
        SelectedProviderKey,
        SelectedProviderNameKey,
        SelectedProviderStatusKey,
        ProviderModeKey,
        ProviderFallbackStatusKey,
        ProviderFallbackReasonKey,
        UnavailableProviderCountKey,
        UnavailableProvidersKey,
        RealCloudProviderClaimedKey
    ];

    private static readonly IReadOnlyList<string> KnownProviders =
    [
        "claude",
        "openai",
        "gemini"
    ];

    private static readonly IReadOnlyList<char> KnownProviderAliasSeparators =
    [
        '-',
        '_',
        '.',
        ':'
    ];

    public static IReadOnlyDictionary<string, string> DescribeExternalConfig(
        string? providerName,
        bool configComplete,
        string completeProviderMode = "external_configured_not_called",
        string completeProviderStatus = "configured_not_called",
        string incompleteFallbackReason = "provider_config_incomplete")
    {
        var requestedProvider = NormalizeProvider(providerName);
        var safeProviderName = SafeProviderName(providerName);
        var selectedProvider = configComplete ? requestedProvider : "none";
        var selectedProviderName = configComplete ? safeProviderName : "none";
        return Build(
            requestedProvider,
            safeProviderName,
            selectedProvider,
            selectedProviderName,
            configComplete ? completeProviderStatus : "not_selected",
            configComplete ? completeProviderMode : "not_configured",
            configComplete ? "not_needed" : "fallback_unavailable",
            configComplete ? "none" : incompleteFallbackReason);
    }

    public static IReadOnlyDictionary<string, string> DescribeNotConfigured(
        string? requestedProviderName = null,
        string fallbackReason = "external_provider_not_configured")
    {
        var requestedProvider = NormalizeProvider(requestedProviderName);
        var safeProviderName = SafeProviderName(requestedProviderName);
        return Build(
            requestedProvider,
            safeProviderName,
            "none",
            "none",
            "not_selected",
            "not_configured",
            "fallback_unavailable",
            fallbackReason);
    }

    public static IReadOnlyDictionary<string, string> DescribeLocalStub(
        string providerName,
        string fallbackReason)
    {
        return Build(
            "local_stub",
            SafeProviderName(providerName),
            "local_stub",
            SafeProviderName(providerName),
            "local_stub_selected",
            "local_stub",
            "local_stub",
            fallbackReason);
    }

    public static IReadOnlyDictionary<string, string> MergeInto(
        IReadOnlyDictionary<string, string> metadata,
        IReadOnlyDictionary<string, string> policyMetadata)
    {
        var merged = new Dictionary<string, string>(metadata, StringComparer.Ordinal);
        CopyPublicMetadata(merged, policyMetadata);
        return merged;
    }

    public static void CopyPublicMetadata(
        IDictionary<string, string> target,
        IReadOnlyDictionary<string, string>? source)
    {
        if (source is null)
            return;

        foreach (var key in PublicMetadataKeys)
        {
            if (source.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
                target[key] = SafeScalar(value, key);
        }
    }

    private static IReadOnlyDictionary<string, string> Build(
        string requestedProvider,
        string requestedProviderName,
        string selectedProvider,
        string selectedProviderName,
        string selectedProviderStatus,
        string providerMode,
        string fallbackStatus,
        string fallbackReason)
    {
        var unavailable = KnownProviders
            .Where(provider => !string.Equals(provider, selectedProvider, StringComparison.Ordinal))
            .ToArray();

        return new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [ProviderSelectionPolicyKey] = PolicyName,
            [CandidateProvidersKey] = CandidateProviders,
            [RequestedProviderKey] = requestedProvider,
            [RequestedProviderNameKey] = requestedProviderName,
            [SelectedProviderKey] = selectedProvider,
            [SelectedProviderNameKey] = selectedProviderName,
            [SelectedProviderStatusKey] = selectedProviderStatus,
            [ProviderModeKey] = providerMode,
            [ProviderFallbackStatusKey] = fallbackStatus,
            [ProviderFallbackReasonKey] = fallbackReason,
            [UnavailableProviderCountKey] = unavailable.Length.ToString(System.Globalization.CultureInfo.InvariantCulture),
            [UnavailableProvidersKey] = unavailable.Length == 0 ? "none" : string.Join(",", unavailable),
            [RealCloudProviderClaimedKey] = bool.FalseString
        };
    }

    private static string NormalizeProvider(string? providerName)
    {
        if (string.IsNullOrWhiteSpace(providerName))
        {
            return "auto";
        }

        var normalized = providerName.Trim().ToLowerInvariant();
        if (normalized is "none" or "missing" or "unconfigured")
        {
            return "auto";
        }

        if (IsLocalStubProviderName(normalized))
        {
            return "local_stub";
        }

        if (MatchesKnownProviderAlias(normalized, "claude") ||
            MatchesKnownProviderAlias(normalized, "anthropic"))
        {
            return "claude";
        }

        if (MatchesKnownProviderAlias(normalized, "openai") ||
            MatchesKnownProviderAlias(normalized, "gpt"))
        {
            return "openai";
        }

        if (MatchesKnownProviderAlias(normalized, "gemini") ||
            MatchesKnownProviderAlias(normalized, "google"))
        {
            return "gemini";
        }

        return "custom";
    }

    private static bool MatchesKnownProviderAlias(string normalized, string alias)
    {
        if (string.Equals(normalized, alias, StringComparison.Ordinal))
            return true;

        return normalized.Length > alias.Length + 1 &&
               normalized.StartsWith(alias, StringComparison.Ordinal) &&
               KnownProviderAliasSeparators.Contains(normalized[alias.Length]);
    }

    private static bool IsLocalStubProviderName(string normalized)
    {
        if (normalized.Contains("local", StringComparison.Ordinal) ||
            normalized.Contains("fake", StringComparison.Ordinal) ||
            normalized.Contains("test", StringComparison.Ordinal))
        {
            return true;
        }

        return false;
    }

    private static string SafeProviderName(string? providerName)
        => string.IsNullOrWhiteSpace(providerName)
            ? "auto"
            : SafeScalar(providerName, "visual_ai_provider_name");

    private static string SafeScalar(string? value, string scope)
    {
        var redactions = new SortedSet<string>(StringComparer.Ordinal);
        return DesignConversionDpoLeakageGuard.SanitizeScalar(value ?? string.Empty, scope, redactions);
    }
}
