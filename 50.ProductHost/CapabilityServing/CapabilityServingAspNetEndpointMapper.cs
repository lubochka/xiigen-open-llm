// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProductHost.CapabilityServing;

using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using AgentInfrastructure;
using OrchestratorModel.Logical;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

public static class CapabilityServingAspNetEndpointMapper
{
    public const string ExecutionPath = "/api/v1/capabilities/execute";
    public const string GenerationPath = "/api/v1/capabilities/generate";
    public const string ReadinessPath = "/api/v1/capabilities/readiness";
    public const long MaximumRequestBodyBytes = 64 * 1024;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = false,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
    };

    public static WebApplication ConfigureCapabilityServing(
        this WebApplication app,
        CapabilityServingCompositionRoot compositionRoot)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(compositionRoot);

        app.MapGet(ReadinessPath, () => Results.Json(
            new CapabilityServingReadinessResponse(
                CapabilityServingReady: true,
                ProductReady: false,
                PerformsTraining: false,
                DpoReady: false,
                TeacherReady: false,
                compositionRoot.SnapshotId,
                compositionRoot.BundleId,
                compositionRoot.SnapshotRowIndex,
                compositionRoot.ScenarioCount,
                compositionRoot.AssistantInvocationCount,
                Environment.ProcessId,
                compositionRoot.RestartInstanceId,
                compositionRoot.EntrypointTraceId,
                CapabilityServingCompositionRoot.ServingBoundary),
            JsonOptions));

        app.MapPost(ExecutionPath, async (HttpContext context) =>
        {
            context.Response.Headers.CacheControl = "no-store";
            if (context.Request.ContentLength is > MaximumRequestBodyBytes)
                return Error("CAPABILITY_SERVING_BODY_TOO_LARGE", StatusCodes.Status413PayloadTooLarge);

            if (!IsJsonContentType(context.Request.ContentType))
            {
                return Error(
                    "CAPABILITY_SERVING_CONTENT_TYPE_REQUIRED",
                    StatusCodes.Status415UnsupportedMediaType);
            }

            CapabilityServingHttpRequest request;
            try
            {
                var body = await ReadBoundedBodyAsync(
                    context.Request.Body,
                    context.RequestAborted);
                if (body is null)
                    return Error("CAPABILITY_SERVING_BODY_TOO_LARGE", StatusCodes.Status413PayloadTooLarge);
                if (!TryParseExactRequest(body, out request))
                    return Error("CAPABILITY_SERVING_JSON_INVALID", StatusCodes.Status400BadRequest);
            }
            catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
            {
                return Error("CANCELLED", StatusCodes.Status408RequestTimeout);
            }
            catch (JsonException)
            {
                return Error("CAPABILITY_SERVING_JSON_INVALID", StatusCodes.Status400BadRequest);
            }
            catch (BadHttpRequestException exception)
                when (exception.StatusCode == StatusCodes.Status413PayloadTooLarge)
            {
                return Error("CAPABILITY_SERVING_BODY_TOO_LARGE", StatusCodes.Status413PayloadTooLarge);
            }

            OperationResult<LogicalAssistantRunResult> result =
                await compositionRoot.ExecuteAsync(
                    new CapabilityServingRequest(request.UserRequest, request.ScenarioId),
                    context.RequestAborted);
            if (!result.IsSuccess || result.Value is null)
                return Error(result.ErrorCode ?? "CAPABILITY_SERVING_EXECUTION_FAILED", MapStatus(result.ErrorCode));

            var value = result.Value;
            return Results.Json(new CapabilityServingHttpResponse(
                value.ScenarioId,
                value.SelectedFlowId,
                value.OverallVerdict,
                value.FinalHumanResult,
                value.Metrics,
                value.EvidenceIds,
                CapabilityServingReady: true,
                ProductReady: false,
                PerformsTraining: false,
                DpoReady: false,
                TeacherReady: false,
                CapabilityServingCompositionRoot.ServingBoundary), JsonOptions);
        });

        app.MapPost(GenerationPath, async (HttpContext context) =>
        {
            context.Response.Headers.CacheControl = "no-store";
            if (context.Request.ContentLength is > MaximumRequestBodyBytes)
                return Error("CAPABILITY_GENERATION_BODY_TOO_LARGE", StatusCodes.Status413PayloadTooLarge);

            if (!IsJsonContentType(context.Request.ContentType))
            {
                return Error(
                    "CAPABILITY_GENERATION_CONTENT_TYPE_REQUIRED",
                    StatusCodes.Status415UnsupportedMediaType);
            }

            CapabilityGenerationHttpRequest request;
            try
            {
                var body = await ReadBoundedBodyAsync(
                    context.Request.Body,
                    context.RequestAborted);
                if (body is null)
                    return Error("CAPABILITY_GENERATION_BODY_TOO_LARGE", StatusCodes.Status413PayloadTooLarge);
                if (!TryParseExactGenerationRequest(body, out request))
                    return Error("CAPABILITY_GENERATION_JSON_INVALID", StatusCodes.Status400BadRequest);
            }
            catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
            {
                return Error("CANCELLED", StatusCodes.Status408RequestTimeout);
            }
            catch (JsonException)
            {
                return Error("CAPABILITY_GENERATION_JSON_INVALID", StatusCodes.Status400BadRequest);
            }
            catch (BadHttpRequestException exception)
                when (exception.StatusCode == StatusCodes.Status413PayloadTooLarge)
            {
                return Error("CAPABILITY_GENERATION_BODY_TOO_LARGE", StatusCodes.Status413PayloadTooLarge);
            }

            var result = await compositionRoot.GenerateAsync(
                new CapabilityGenerationRequest(
                    request.UserRequest,
                    request.ScenarioId,
                    request.MaxNewTokens,
                    request.Seed),
                context.RequestAborted);
            if (!result.IsSuccess || result.Value is null)
            {
                return Error(
                    result.ErrorCode ?? "CAPABILITY_GENERATION_FAILED",
                    MapGenerationStatus(result.ErrorCode));
            }

            return Results.Json(result.Value, JsonOptions);
        });

        return app;
    }

    private static IResult Error(string code, int statusCode)
        => Results.Json(
            new CapabilityServingErrorResponse(code),
            JsonOptions,
            statusCode: statusCode);

    private static int MapStatus(string? code)
        => code switch
        {
            "CANCELLED" => StatusCodes.Status408RequestTimeout,
            "CAPABILITY_SERVING_REQUEST_REQUIRED" or
            "CAPABILITY_SERVING_USER_REQUEST_INVALID" or
            "CAPABILITY_SERVING_SCENARIO_INVALID" => StatusCodes.Status400BadRequest,
            "CAPABILITY_SERVING_SCENARIO_NOT_AUTHORIZED" => StatusCodes.Status404NotFound,
            "CAPABILITY_SERVING_ROUTE_BOUNDARY_VIOLATION" or
            "CAPABILITY_SERVING_FLAG_BOUNDARY_VIOLATION" or
            "CAPABILITY_SERVING_EXECUTABLE_BOUNDARY_VIOLATION" or
            "CAPABILITY_SERVING_EXECUTION_FAILED" => StatusCodes.Status500InternalServerError,
            _ => StatusCodes.Status422UnprocessableEntity
        };

    private static int MapGenerationStatus(string? code)
        => code switch
        {
            "CANCELLED" => StatusCodes.Status408RequestTimeout,
            "CAPABILITY_GENERATION_REQUEST_REQUIRED" or
            "CAPABILITY_GENERATION_USER_REQUEST_INVALID" or
            "CAPABILITY_GENERATION_SCENARIO_INVALID" or
            "CAPABILITY_GENERATION_MAX_NEW_TOKENS_INVALID" =>
                StatusCodes.Status400BadRequest,
            "CAPABILITY_GENERATION_SCENARIO_NOT_AUTHORIZED" =>
                StatusCodes.Status404NotFound,
            "CAPABILITY_GENERATION_FAILED" or
            "CAPABILITY_GENERATION_RUNTIME_INVALID" =>
                StatusCodes.Status500InternalServerError,
            _ => StatusCodes.Status422UnprocessableEntity
        };

    private static bool IsJsonContentType(string? value)
        => MediaTypeHeaderValue.TryParse(value, out var parsed) &&
           string.Equals(parsed.MediaType, "application/json", StringComparison.OrdinalIgnoreCase) &&
           parsed.Parameters.All(parameter =>
               !string.IsNullOrWhiteSpace(parameter.Name) &&
               !string.IsNullOrWhiteSpace(parameter.Value));

    private static async Task<byte[]?> ReadBoundedBodyAsync(
        Stream body,
        CancellationToken cancellationToken)
    {
        var buffer = new byte[checked((int)MaximumRequestBodyBytes + 1)];
        var total = 0;
        while (total < buffer.Length)
        {
            var read = await body.ReadAsync(buffer.AsMemory(total), cancellationToken);
            if (read == 0)
                return buffer[..total];
            total += read;
        }

        return null;
    }

    private static bool TryParseExactRequest(
        ReadOnlyMemory<byte> body,
        out CapabilityServingHttpRequest request)
    {
        request = default!;
        using var document = JsonDocument.Parse(
            body,
            new JsonDocumentOptions
            {
                AllowTrailingCommas = false,
                CommentHandling = JsonCommentHandling.Disallow,
                MaxDepth = 16
            });
        if (document.RootElement.ValueKind != JsonValueKind.Object)
            return false;

        string? userRequest = null;
        string? scenarioId = null;
        var sawUserRequest = false;
        var sawScenarioId = false;
        var propertyCount = 0;
        foreach (var property in document.RootElement.EnumerateObject())
        {
            propertyCount++;
            if (string.Equals(
                    property.Name,
                    "userRequest",
                    StringComparison.Ordinal))
            {
                if (sawUserRequest ||
                    property.Value.ValueKind != JsonValueKind.String)
                {
                    return false;
                }
                sawUserRequest = true;
                userRequest = property.Value.GetString();
                continue;
            }

            if (string.Equals(
                    property.Name,
                    "scenarioId",
                    StringComparison.Ordinal))
            {
                if (sawScenarioId ||
                    property.Value.ValueKind != JsonValueKind.String)
                {
                    return false;
                }
                sawScenarioId = true;
                scenarioId = property.Value.GetString();
                continue;
            }

            return false;
        }

        if (propertyCount != 2 ||
            !sawUserRequest ||
            !sawScenarioId ||
            userRequest is null ||
            scenarioId is null)
        {
            return false;
        }

        request = new CapabilityServingHttpRequest(userRequest, scenarioId);
        return true;
    }

    private static bool TryParseExactGenerationRequest(
        ReadOnlyMemory<byte> body,
        out CapabilityGenerationHttpRequest request)
    {
        request = default!;
        using var document = JsonDocument.Parse(
            body,
            new JsonDocumentOptions
            {
                AllowTrailingCommas = false,
                CommentHandling = JsonCommentHandling.Disallow,
                MaxDepth = 16
            });
        if (document.RootElement.ValueKind != JsonValueKind.Object)
            return false;

        string? userRequest = null;
        string? scenarioId = null;
        var maxNewTokens = 0;
        var seed = 0;
        var sawUserRequest = false;
        var sawScenarioId = false;
        var sawMaxNewTokens = false;
        var sawSeed = false;
        var propertyCount = 0;
        foreach (var property in document.RootElement.EnumerateObject())
        {
            propertyCount++;
            switch (property.Name)
            {
                case "userRequest":
                    if (sawUserRequest || property.Value.ValueKind != JsonValueKind.String)
                        return false;
                    sawUserRequest = true;
                    userRequest = property.Value.GetString();
                    break;
                case "scenarioId":
                    if (sawScenarioId || property.Value.ValueKind != JsonValueKind.String)
                        return false;
                    sawScenarioId = true;
                    scenarioId = property.Value.GetString();
                    break;
                case "maxNewTokens":
                    if (sawMaxNewTokens || !TryReadExactInt32(property.Value, out maxNewTokens))
                        return false;
                    sawMaxNewTokens = true;
                    break;
                case "seed":
                    if (sawSeed || !TryReadExactInt32(property.Value, out seed))
                        return false;
                    sawSeed = true;
                    break;
                default:
                    return false;
            }
        }

        if (propertyCount != 4 ||
            !sawUserRequest ||
            !sawScenarioId ||
            !sawMaxNewTokens ||
            !sawSeed ||
            userRequest is null ||
            scenarioId is null)
        {
            return false;
        }

        request = new CapabilityGenerationHttpRequest(
            userRequest,
            scenarioId,
            maxNewTokens,
            seed);
        return true;
    }

    private static bool TryReadExactInt32(JsonElement value, out int result)
    {
        result = 0;
        if (value.ValueKind != JsonValueKind.Number)
            return false;

        var raw = value.GetRawText();
        if (raw.Length == 0 ||
            raw.Any(character => character is '.' or 'e' or 'E' or '+'))
        {
            return false;
        }

        return int.TryParse(
            raw,
            NumberStyles.AllowLeadingSign,
            CultureInfo.InvariantCulture,
            out result);
    }
}

public sealed record CapabilityServingHttpRequest(
    string UserRequest,
    string ScenarioId);

public sealed record CapabilityGenerationHttpRequest(
    string UserRequest,
    string ScenarioId,
    int MaxNewTokens,
    int Seed);

public sealed record CapabilityServingHttpResponse(
    string ScenarioId,
    string SelectedFlowId,
    string OverallVerdict,
    string FinalHumanResult,
    IReadOnlyDictionary<string, string> Metrics,
    string[] EvidenceIds,
    bool CapabilityServingReady,
    bool ProductReady,
    bool PerformsTraining,
    bool DpoReady,
    bool TeacherReady,
    string Boundary);

public sealed record CapabilityServingReadinessResponse(
    bool CapabilityServingReady,
    bool ProductReady,
    bool PerformsTraining,
    bool DpoReady,
    bool TeacherReady,
    string SnapshotId,
    string BundleId,
    int SnapshotRowIndex,
    int ScenarioCount,
    long AssistantInvocationCount,
    int ProcessId,
    string RestartInstanceId,
    string EntrypointTraceId,
    string Boundary);

public sealed record CapabilityServingErrorResponse(string ErrorCode);
