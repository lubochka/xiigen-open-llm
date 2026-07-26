// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ModelServer.DesignConversion;

using System.Text.Json;

public sealed record DesignConversionModelServerDpoUsefulnessResponseParseResult(
    bool ParseSucceeded,
    DesignConversionModelServerDpoUsefulnessResponse? Response,
    string? ErrorCode,
    string? ErrorMessage,
    string? ParseError);

public static class DesignConversionModelServerDpoUsefulnessResponseParser
{
    public const string RequiredErrorCode = "ADAPTER_DPO_USEFULNESS_TYPED_RESPONSE_REQUIRED";
    public const string ParseFailedErrorCode = "ADAPTER_DPO_USEFULNESS_TYPED_RESPONSE_PARSE_FAILED";

    public static DesignConversionModelServerDpoUsefulnessResponseParseResult Parse(string? bodyJson)
    {
        if (string.IsNullOrWhiteSpace(bodyJson))
        {
            return Failed(
                RequiredErrorCode,
                "DesignConversion local caller adapter could not parse a typed DPO usefulness response.",
                "empty_body_json");
        }

        try
        {
            var typed = JsonSerializer.Deserialize<DesignConversionModelServerDpoUsefulnessResponse>(
                bodyJson,
                DesignConversionModelServerHttpHandler.JsonOptions);
            return typed is null
                ? Failed(
                    RequiredErrorCode,
                    "DesignConversion local caller adapter could not parse a typed DPO usefulness response.",
                    "null_response")
                : new DesignConversionModelServerDpoUsefulnessResponseParseResult(
                    ParseSucceeded: true,
                    Response: typed,
                    ErrorCode: null,
                    ErrorMessage: null,
                    ParseError: null);
        }
        catch (JsonException ex)
        {
            return Failed(
                ParseFailedErrorCode,
                "DesignConversion local caller adapter could not parse the DPO usefulness response as a typed DTO.",
                ex.GetType().Name);
        }
    }

    private static DesignConversionModelServerDpoUsefulnessResponseParseResult Failed(
        string errorCode,
        string errorMessage,
        string parseError)
        => new(
            ParseSucceeded: false,
            Response: null,
            ErrorCode: errorCode,
            ErrorMessage: errorMessage,
            ParseError: parseError);
}
