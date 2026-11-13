// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using System.Text.Json;
using AgentInfrastructure;

/// <summary>
/// Fail-closed structural contract for the content-bearing chosen and rejected
/// sides admitted by the canonical DesignConversion optimizer authority.
/// </summary>
internal static class DesignConversionDpoOptimizerSideContract
{
    internal const string InvalidErrorCode =
        "DESIGN_CONVERSION_DPO_OPTIMIZER_SIDE_CONTRACT_INVALID";
    internal const string InvalidMessage =
        "A DesignConversion optimizer side does not satisfy the required content contract.";

    internal static OperationResult<bool> ValidatePair(
        string? chosen,
        string? rejected)
    {
        if (!TryReadSide(chosen, out var chosenContent) ||
            !TryReadSide(rejected, out var rejectedContent) ||
            chosenContent == rejectedContent)
        {
            return Invalid();
        }

        return OperationResult<bool>.Ok(true);
    }

    private static bool TryReadSide(
        string? side,
        out OptimizerSideContent content)
    {
        content = default;
        if (side is null)
            return false;

        try
        {
            using var document = JsonDocument.Parse(
                side,
                new JsonDocumentOptions
                {
                    AllowTrailingCommas = false,
                    CommentHandling = JsonCommentHandling.Disallow,
                    MaxDepth = 16
                });
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
                return false;

            string? html = null;
            string? css = null;
            var htmlCount = 0;
            var cssCount = 0;
            var propertyCount = 0;
            foreach (var property in root.EnumerateObject())
            {
                propertyCount++;
                if (property.Value.ValueKind != JsonValueKind.String)
                    return false;

                switch (property.Name)
                {
                    case "html":
                        htmlCount++;
                        html = property.Value.GetString();
                        break;
                    case "css":
                        cssCount++;
                        css = property.Value.GetString();
                        break;
                    default:
                        return false;
                }
            }

            if (propertyCount != 2 || htmlCount != 1 || cssCount != 1 ||
                html is null || css is null)
            {
                return false;
            }

            html = NormalizeNewlines(html);
            css = NormalizeNewlines(css);
            if (string.IsNullOrWhiteSpace(html) &&
                string.IsNullOrWhiteSpace(css) ||
                IsOnlyHashSurrogates(html, css))
            {
                return false;
            }

            content = new OptimizerSideContent(html, css);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static bool IsOnlyHashSurrogates(string html, string css)
    {
        var hasActualContent = false;
        foreach (var value in new[] { html, css })
        {
            if (string.IsNullOrWhiteSpace(value))
                continue;
            hasActualContent = true;
            if (!IsHashSurrogate(value))
                return false;
        }
        return hasActualContent;
    }

    private static bool IsHashSurrogate(string value)
    {
        var candidate = value.Trim();
        if (candidate.Length == 64 && IsHex(candidate.AsSpan()))
            return true;
        const string prefix = "sha256:";
        if (candidate.Length == prefix.Length + 64 &&
            candidate.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) &&
            IsHex(candidate.AsSpan(prefix.Length)))
        {
            return true;
        }

        var legacySuffixStart = candidate.Length - 16;
        return legacySuffixStart > 1 &&
               candidate[legacySuffixStart - 1] == '-' &&
               IsHex(candidate.AsSpan(legacySuffixStart));
    }

    private static bool IsHex(ReadOnlySpan<char> value)
    {
        foreach (var character in value)
        {
            if (character is not (>= '0' and <= '9') and
                not (>= 'a' and <= 'f') and
                not (>= 'A' and <= 'F'))
            {
                return false;
            }
        }
        return value.Length > 0;
    }

    private static string NormalizeNewlines(string value)
        => value.Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace("\r", "\n", StringComparison.Ordinal);

    private static OperationResult<bool> Invalid()
        => OperationResult<bool>.Fail(InvalidMessage, InvalidErrorCode);

    private readonly record struct OptimizerSideContent(
        string Html,
        string Css);
}
