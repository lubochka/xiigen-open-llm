// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
using System.Text.Json;
using System.Text.Json.Serialization;
using ContinuousLearning;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using TrainableRouter.Training;

namespace ModelServer;

public static class CodeDocumentationGenerationEndpoint
{
    public const string EndpointPath = "/api/code-documentation/generate";
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    { UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow };

    public static void MapCodeDocumentationGeneration(this WebApplication app)
    {
        app.MapPost(EndpointPath, async (HttpContext context) =>
        {
            CodeDocumentationGenerationInput? input;
            try
            {
                input = await JsonSerializer.DeserializeAsync<CodeDocumentationGenerationInput>(
                    context.Request.Body, Json, context.RequestAborted);
            }
            catch (JsonException) { return Results.Json(new { errorCode = CodeDocumentationStudentGeneration.InvalidErrorCode, stopReason = "ERROR" }, statusCode: 400); }
            var service = context.RequestServices.GetService<CodeDocumentationGenerationService>();
            if (service is null)
                return Results.Json(new { errorCode = context.RequestServices.GetService<CodeDocumentationGenerationConfigurationResult>()?.ErrorCode
                    ?? CodeDocumentationGenerationConfiguration.NotConfigured, stopReason = "ERROR" }, statusCode: 503);
            var result = service.Generate(input!, context.RequestAborted);
            return Results.Json(result, statusCode: result.ErrorCode is null ? 200 :
                result.StopReason == "CANCELLED" ? 499 : result.ErrorCode == CodeDocumentationStudentGeneration.InvalidErrorCode ? 400 : 503);
        });
    }
}
