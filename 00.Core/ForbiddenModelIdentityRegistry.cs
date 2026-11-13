// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure;

public static class ForbiddenModelIdentityRegistry
{
    private static readonly string[] ForbiddenIdentityTokens =
    [
        "openai",
        "chatgpt",
        "gpt-",
        "o1",
        "o3",
        "o4",
        "anthropic",
        "claude",
        "sonnet",
        "haiku",
        "opus",
        "google",
        "gemini",
        "xai",
        "grok",
        "copilot"
    ];

    public static bool ContainsForbiddenIdentity(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return false;

        foreach (var token in ForbiddenIdentityTokens)
        {
            if (ContainsForbiddenToken(value, token))
                return true;
        }

        return false;
    }

    private static bool ContainsForbiddenToken(string value, string token)
    {
        var startIndex = 0;
        while (startIndex < value.Length)
        {
            var index = value.IndexOf(token, startIndex, StringComparison.OrdinalIgnoreCase);
            if (index < 0)
                return false;

            if (HasLeftBoundary(value, index)
                && HasRightBoundary(value, index, token))
            {
                return true;
            }

            startIndex = index + 1;
        }

        return false;
    }

    private static bool HasLeftBoundary(string value, int index)
        => index == 0 || !char.IsLetterOrDigit(value[index - 1]);

    private static bool HasRightBoundary(string value, int index, string token)
    {
        if (!char.IsLetterOrDigit(token[^1]))
            return true;

        var nextIndex = index + token.Length;
        return nextIndex == value.Length || !char.IsLetterOrDigit(value[nextIndex]);
    }
}
